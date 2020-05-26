using Hsf.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity;
using Unity.Lifetime;
using System.Configuration;
using System.IO;
using Microsoft.Practices.Unity.Configuration;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using Hsf.Model;
using Hsf.Framework;
using Newtonsoft.Json;
using Hsf.Service;

namespace Hsf.Project
{
    /// <summary>
    /// NLP中枢控制器
    /// </summary>
    public static class NlpControler
    {
        #region 声明变量
        private static log4net.ILog log = log4net.LogManager.GetLogger("NlpControler");
        /// nlp正确总数
        private static int INlps = 3;//types.Count();
        /// 锁
        private static readonly object _lock = new object();
        //语义槽
        private static ConcurrentDictionary<string, SemanticsSlot> _SemanticsDic = new ConcurrentDictionary<string, SemanticsSlot>();//ConcurrentBag
        #endregion

        #region 音响向NLP发起请求
        /// <summary>
        /// 注册异步接收消息事件
        /// </summary>
        public static void RegEvent()
        {
            SmartHomeNlp._NlpControlerReceiveMsg += NlpControlerReceiveMsg;
            SmartHomeNlp._NlpControlerRequestMsg += NlpControlerRequestMsg;
        }

        #region 向nlp多线程发送消息，并启动超时线程
        /// <summary>
        /// 接收到请求过来的消息，再开始发送向所有nlp多线程发送消息
        /// </summary>
        /// <param name="session">接收客户端session</param>
        /// <param name="body.questions">接收消息内容</param>
        public static void ProcessingRequest(SoundBodyRequest body)
        {
            try
            {
                log.Info($"--------------------------开始向各个NLP发送问题： {body.questions} --------------------------");
                if (body.sourceId == null) body.sourceId = "";
                //添加当前请求到语义槽
                SemanticsSlot semanticsSlot = new SemanticsSlot()
                {
                    SessionId = body.sessionId,
                    DeviceId = body.deviceId,
                    Questions = body.questions,
                    SourceId = body.sourceId,
                    NlpAnswers = new List<NlpAnswers>(),
                    Answertimes=0,
                    State = 0
                };
                //创建语义槽
                _SemanticsDic.TryAdd(body.sessionId, semanticsSlot);

                //超时检测标识
                CancellationTokenSource cts = new CancellationTokenSource();
                //多线程集合
                List<Task> taskList = new List<Task>();
                TaskFactory taskFactory = new TaskFactory();

                var type = typeof(INlp);
                var types = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(INlp))))
                        .ToArray();
                INlps = types.Count();//语义槽应该放入个数

                foreach (var v in types)
                {
                    if (v.IsClass)
                    {
                        taskList.Add(taskFactory.StartNew(() =>
                        {
                            try
                            {
                                //开始发送消息，并接收返回的语义
                                NlpAnswers _NlpAnswers = (Activator.CreateInstance(v) as INlp).SendMsg(body);
                                //如果线程没有被取消，放入语义槽
                                if (!cts.IsCancellationRequested)
                                {
                                    //过滤异步
                                    if (_NlpAnswers != null)
                                    {
                                        _SemanticsDic[body.sessionId].Answertimes += 1;
                                        lock (_lock)
                                        {
                                            //过滤""
                                            if (_NlpAnswers.Answers != "")
                                            {
                                                _SemanticsDic[body.sessionId].NlpAnswers.Add(_NlpAnswers);
                                                log.Info($"{body.questions} {_SemanticsDic[body.sessionId].Answertimes} {v.Name}  入槽内容: { _NlpAnswers.Answers.Replace("\r\n","")}");
                                            }
                                            else
                                            {
                                                log.Info($"{body.questions} {_SemanticsDic[body.sessionId].Answertimes} {v.Name} 返回内容为空");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    log.Info($"{body.questions} {v.Name} 超时线程取消未入槽！");
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Info($"开始向{v.Name}发送消息异常:{ex.Message}");
                            }
                        }, cts.Token));
                    }
                }

                //到了1.5秒还没有全部执行的，取消线程并返回
                taskList.Add(Task.Delay(1500).ContinueWith(t =>
                {
                    //如果语义槽为空不再执行
                    if (_SemanticsDic.ContainsKey(body.sessionId))
                    {
                        //如果当前语义槽标识位为State=0，则表示可以继续执行
                        if (_SemanticsDic[body.sessionId].State == 0)
                        {
                            //把当前session状态设置false
                            _SemanticsDic[body.sessionId].State = -1;
                            lock (_lock)
                            {
                                cts.Cancel();
                            }
                            //如果语义槽不为null再继续执行
                            if (_SemanticsDic[body.sessionId] != null)
                            {
                                //返回语义结果
                                AIControler.GetAIAnswers(_SemanticsDic[body.sessionId], "超时");
                            }
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                log.Info($"发送消息异常:{ex.Message}");
            }

        }
        #endregion

        #region 异步接收返回的NlpAnswers
        /// <summary>
        /// 将结果放入当前sessionid的语义槽
        /// </summary>
        /// <param name="sessionId">与请求对应的sessionid</param>
        /// <param name="_SingleAnswers">返回的答案</param>
        public static void NlpControlerReceiveMsg(string sessionId, NlpAnswers _NlpAnswers)
        {
            //如果语义槽为空不再执行
            if (_SemanticsDic.ContainsKey(sessionId))
            {
                //如果当前语义槽标识位为State=0，则表示可以继续执行
                if (_SemanticsDic[sessionId].State == 0)
                {
                    //将结果放入当前sessionid的语义槽
                    _SemanticsDic[sessionId].NlpAnswers.Add(_NlpAnswers);
                    _SemanticsDic[sessionId].Answertimes += 1;
                    log.Info($"{_SemanticsDic[sessionId].Questions} {_SemanticsDic[sessionId].Answertimes} {_NlpAnswers.Code} 入槽内容（异步）： { _NlpAnswers.Answers}");

                    //判断异步放入语义槽之后的数量，数量相等发送AI进行处理
                    if (_SemanticsDic[sessionId].Answertimes == INlps)
                    {
                        //把当前session状态设置1，表示全部放入完毕
                        _SemanticsDic[sessionId].State = 1;
                        //返回AI处理结果
                        AIControler.GetAIAnswers(_SemanticsDic[sessionId],"异步");
                    }
                }
                else
                {
                    log.Info($"异步超时入槽失败session ：{sessionId},返回语义 ： {_NlpAnswers.Answers}");
                }
            }
            else
            {
                log.Info($"异步响应未找到session ：{sessionId},返回语义 ： {_NlpAnswers.Answers}");
            }
        }
        #endregion

        #region 返回音响结果
        public static void BackAnswers(string sessionId, string deviceId, string questions, string actionId, string req, string url)
        {
            //语义槽为空的情况下，只返回
            SoundBodyResult bodyResult = new SoundBodyResult()
            {
                sessionId = sessionId,
                deviceId = deviceId,
                questions = questions,
                actionId = actionId,
                req = req,
                url = url,//只有2014和2011返回的req走百度语音合成
                blwakeup = "0"
            };
            HsfWebSocket.ResponseSoundBodyResult(bodyResult);
            //释放当前session
            SessionDispose(sessionId);
        }
        #endregion

        #region 执行回调结果
        /// <summary>
        /// 执行回调结果
        /// </summary>
        /// <param name="_SingleAnswers"></param>
        /// <returns></returns>
        public static string CallbackMsg(SingleAnswers _SingleAnswers)
        {
            if (_SingleAnswers != null)
            {
                var type = typeof(INlp);
                var types = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(INlp))))
                        .ToArray();
                string returnMsg = "";
                foreach (var v in types)
                {
                    if (v.Name == _SingleAnswers.NlpName)
                    {
                        returnMsg = (Activator.CreateInstance(v) as INlp).CallbackMsg(_SingleAnswers);
                    }
                }
                //执行完回调，释放当前session
                SessionDispose(_SingleAnswers.SessionId);
                return returnMsg;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region 释放Session
        /// <summary>
        /// 释放Session
        /// </summary>
        /// <param name="sessionId">指定的sessionId</param>
        public static void SessionDispose(string sessionId)
        {
            //清空当前session语义槽
            SemanticsSlot dd = null;
            _SemanticsDic.TryRemove(sessionId, out dd);
        }
        #endregion

        #endregion



        #region 家居app向音响发起请求
        /// <summary>
        /// 拼装返回音响的Json串，并返回
        /// </summary>
        public static bool NlpControlerRequestMsg(SoundBodyResult _SoundPassiveRequest)
        {
            log.Info($"智能家居app向音响发起请求： {_SoundPassiveRequest.req}");
            return HsfWebSocket.ResponseSoundToRequest(_SoundPassiveRequest);
        }
        #endregion
    }
}
