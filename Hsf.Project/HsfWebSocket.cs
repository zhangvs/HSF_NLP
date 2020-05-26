using Hsf.Model;
using Hsf.Framework;
using Hsf.DAL;
using Newtonsoft.Json;
using SuperSocket.SocketBase.Config;
using SuperWebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hsf.Redis.Service;

namespace Hsf.Project
{
    public class HsfWebSocket
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("HsfWebSocket");

        //认证的音响
        public static List<SoundAuthentication> SoundListIn = new List<SoundAuthentication>();

        //保存回调session
        private static ConcurrentDictionary<string, WebSocketSession> _SessionDic = new ConcurrentDictionary<string, WebSocketSession>();

        #region 启动WebSocketServer
        public static void Start()
        {
            WebSocketServer ws = new WebSocketServer();//实例化SuperWebSocket中的WebSocketServer对象
            //添加事件侦听
            ws.NewSessionConnected += ws_NewSessionConnected;//有新会话握手并连接成功
            ws.SessionClosed += ws_SessionClosed;//有会话被关闭 可能是服务端关闭 也可能是客户端关闭
            ws.NewMessageReceived += ws_NewMessageReceived;//有客户端发送新的消息

            ServerConfig serverConfig = new ServerConfig
            {
                Ip = IPAddress.Any.ToString(),//"127.0.0.1"不可以写死，要使用IPAddress.Any.ToString()
                Port = 7878,//set the listening port
                MaxConnectionNumber = 10000,
                MaxRequestLength = 1024000//最大长度
            };
            if (!ws.Setup(serverConfig))
            {
                log.Info("WebSocket 设置WebSocket服务侦听地址失败");
                return;
            }

            if (!ws.Start())
            {
                log.Info("WebSocket 启动WebSocket服务侦听失败");
                return;
            }

            log.Info($"NLPWebSocket中枢服务器启动成功{DateTime.Now.ToString()}");
            Console.WriteLine($"NLPWebSocket中枢服务器启动成功{DateTime.Now.ToString()}");
        }
        #endregion

        #region 收到消息
        /// <summary>
        /// 接收到消息，发送给Nlp管理器
        /// </summary>
        /// <param name="session">会话</param>
        /// <param name="message">消息内容</param>
        public static void ws_NewMessageReceived(WebSocketSession session, string message)
        {
            message = message.Replace(" ", "");
            message = message.Replace("\t", "");
            message = message.Replace("\r", "");
            message = message.Replace("\n", "");
            //log.Info($"web消息：{session.RemoteEndPoint.ToString()} ： {message}");
            if (message.Equals("ping"))
            {
                session.Send("ping");
            }
            else if (message.Contains("\"actionId\":\"800\""))//心跳
            {
                //log.Info($"心跳： {session.RemoteEndPoint.ToString()}说： {message}");
                message = message.Replace("800", "801");
                session.Send(message);
            }
            else
            {
                //log.Info($"心跳外所有的消息： {session.RemoteEndPoint.ToString()}说： {message}");
                if (message.Contains("\"actionId\":\"111\""))///注册
                {
                    log.Info($"新的注册消息：{session.RemoteEndPoint.ToString()} ： {message}");

                    #region 注册认证
                    try
                    {
                        SoundAuthentication soundEntity = JsonConvert.DeserializeObject<SoundAuthentication>(message);
                        if (soundEntity.req == "123")
                        {
                            //判断是否已经存在该主机的认证信息
                            SoundAuthentication soundAuth = SoundListIn.Where(a => a.deviceId == soundEntity.deviceId).FirstOrDefault();
                            //初次认证
                            if (soundAuth == null)
                            {
                                soundEntity.actionId = "1111";
                                soundEntity.IPAddress = session.RemoteEndPoint.Address.ToString(); //登录ip  
                                soundEntity.req = Guid.NewGuid().ToString();// 如果成功，token会放入req内容处，客户端每次提交请求，都需带上token
                                SoundListIn.Add(soundEntity);//放入音响缓存list

                                //保存当前session连接与主机对应
                                _SessionDic.TryAdd(soundEntity.deviceId, session);

                                //认证返回
                                SoundAuthenticationBack authBack = new SoundAuthenticationBack()
                                {
                                    sessionId = soundEntity.sessionId,
                                    actionId = soundEntity.actionId,//1111
                                    req = soundEntity.req,
                                };

                                session.Send(JsonConvert.SerializeObject(authBack));
                                log.Info($"认证成功：deviceId { soundEntity.deviceId} token {soundEntity.req}");
                            }
                            else
                            {
                                //断线之后会再次认证，返回之前的令牌，替换当前的session
                                //已经认证过的
                                SoundAuthenticationBack authBack = new SoundAuthenticationBack()
                                {
                                    sessionId = soundEntity.sessionId,//当前请求的session
                                    actionId = soundAuth.actionId,//1111
                                    req = soundAuth.req,//之前认证的令牌
                                };
                                //保存当前session连接与主机对应
                                _SessionDic.TryUpdate(soundEntity.deviceId, session, _SessionDic[soundEntity.deviceId]);

                                session.Send(JsonConvert.SerializeObject(authBack));
                                log.Info($"{ soundEntity.deviceId}已认证成功，req：{soundAuth.req}");
                            }
                        }
                        else
                        {
                            SoundAuthenticationBack authBack = new SoundAuthenticationBack()
                            {
                                sessionId = soundEntity.sessionId,
                                actionId = "1112",
                            };
                            session.Send(JsonConvert.SerializeObject(authBack));
                            log.Info(soundEntity.deviceId + "认证失败");
                        }
                    }
                    catch (Exception ex)
                    {
                        SoundAuthenticationBack authBack = new SoundAuthenticationBack()
                        {
                            actionId = "1112",
                        };
                        session.Send(JsonConvert.SerializeObject(authBack));
                        log.Info("认证异常:" + ex.Message);
                    }
                    #endregion
                }
                else if (message.Contains("\"actionId\":\"201\""))///语音请求，音乐返回的语音结果
                {
                    log.Info($"\r\n新的语音请求： {message}");
                    #region 语音请求
                    try
                    {
                        SoundBodyRequest body = JsonConvert.DeserializeObject<SoundBodyRequest>(message);

                        //如果缓存中的音响槽，包括当前请求的  1.设备id  2.服务器分配的token一致  3.ip地址
                        SoundAuthentication sound = SoundListIn.Where(a => a.deviceId == body.deviceId && a.req == body.token).FirstOrDefault();//&& a.IPAddress == session.RemoteEndPoint.Address.ToString()
                                                                                                                                                //缓存中存在,则视为正常访问
                        if (sound != null)
                        {
                            //2.0增加人工智能处理部分代码
                            AIFunction(body);
                            ////1.0启动多线程
                            //Task.Run(() =>
                            //{
                            //    //发送收到的语音到NLP管理器
                            //    NlpControler.ProcessingRequest(body);
                            //});
                        }
                        else
                        {
                            //缓存中不存在则返回认证失败
                            SoundAuthenticationBack authBack = new SoundAuthenticationBack()
                            {
                                sessionId = body.sessionId,
                                actionId = "1112",
                                req = "认证失败"
                            };
                            session.Send(JsonConvert.SerializeObject(authBack));

                            //插入认证失败表
                            using (HsfDBContext hsfDBContext = new HsfDBContext())
                            {
                                sound_fail sound_Fail = new sound_fail()
                                {
                                    sessionId = body.sessionId,
                                    deviceId = body.deviceId,
                                    actionId = body.actionId,
                                    token = body.token,
                                    questions = body.questions,
                                    IPAddress = session.RemoteEndPoint.Address.ToString(),
                                    CreateTime = DateTime.Now
                                };
                                hsfDBContext.sound_fail.Add(sound_Fail);
                                hsfDBContext.SaveChanges();
                            }
                            log.Info(body.deviceId + "认证失败，已写入认证失败记录表");
                        }
                    }
                    catch (Exception ex)
                    {
                        SoundBodyResult bodyResult = new SoundBodyResult()
                        {
                            actionId = "2025",//异常，暂时返回不能识别
                            req = ex.Message,
                            blwakeup = "0"
                        };
                        string jsonResult = JsonConvert.SerializeObject(bodyResult);
                        session.Send(jsonResult);
                    }
                    #endregion
                }
                else if (message.Contains("\"actionId\":\"202\""))///url媒体文件播放完
                {

                }
                else if (message.Contains("\"actionId\":\"205\""))///url媒体文件播放完
                {

                }
                else
                {
                    ///不做任何处理，让音箱执行自己逻辑
                    SoundBodyResult bodyResult = new SoundBodyResult()
                    {
                        actionId = "2010",
                        blwakeup = "0"
                    };
                    string jsonResult = JsonConvert.SerializeObject(bodyResult);
                    session.Send(jsonResult);
                }
            }

        }
        #endregion

        #region 新的连接
        public static void ws_NewSessionConnected(WebSocketSession session)
        {
            log.Info($"{session.RemoteEndPoint.ToString()} 登录到websocket服务器");
            Console.WriteLine($"{session.RemoteEndPoint.ToString()} 登录到websocket服务器");
        }
        #endregion

        #region 连接关闭
        public static void ws_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
        {
            log.Info($"{session.RemoteEndPoint.ToString()} 离开websocket服务器");
        }
        #endregion

        #region 广播
        private void SendToAll(WebSocketSession session, string msg)
        {
            foreach (var sendSession in session.AppServer.GetAllSessions())
            {
                sendSession.Send(msg);
            }
            log.Info(msg);
        }
        #endregion

        /// <summary>
        /// 返回当前请求session的答案
        /// </summary>
        /// <param name="msg"></param>
        public static string ResponseSoundBodyResult(SoundBodyResult bodyResult)
        {
            string returnMsg = "";
            if (_SessionDic.ContainsKey(bodyResult.deviceId))
            {
                WebSocketSession session = _SessionDic[bodyResult.deviceId];
                if (session != null)
                {
                    string jsonResult = JsonConvert.SerializeObject(bodyResult);
                    if (session.TrySend(jsonResult))
                    {
                        returnMsg = $"{bodyResult.questions} 返回音响成功：{jsonResult}";
                    }
                    else
                    {
                        returnMsg = $"{bodyResult.questions} WebSocketSession连接失效，返回结果失败";
                    }
                }
                else
                {
                    returnMsg = $"{bodyResult.questions} 音响对应的WebSocketSession为null";
                }
            }
            else
            {
                returnMsg = $"{bodyResult.questions} 音响不存在WebSocketSession连接";
            }
            log.Info(returnMsg);
            return returnMsg;
        }

        /// <summary>
        /// 主动请求音响
        /// </summary>
        /// <param name="msg"></param>
        public static bool ResponseSoundToRequest(SoundBodyResult bodyRequest)
        {
            if (_SessionDic.ContainsKey(bodyRequest.deviceId))
            {
                WebSocketSession session = _SessionDic[bodyRequest.deviceId];
                if (session != null)
                {
                    string jsonResult = JsonConvert.SerializeObject(bodyRequest);
                    if (session.TrySend(jsonResult))
                    {
                        log.Info($"{bodyRequest.deviceId}请求音响成功：{jsonResult}");
                        return true;
                    }
                    else
                    {
                        log.Error($"{bodyRequest.deviceId}WebSocketSession连接失效，请求失败");
                        return false;
                    }
                }
                else
                {
                    log.Info($"{bodyRequest.deviceId}音响对应的WebSocketSession为null，请求失败");
                    return false;
                }
            }
            else
            {
                log.Info($"{bodyRequest.deviceId}音响不存在WebSocketSession连接，请求失败");
                return false;
            }
        }

        public static void AIFunction(SoundBodyRequest body)
        {
            //百度分词
            BaiduNlp nlpEntity = BaiduSDK.Nlp(body.questions);

            //添加句子表，分词表
            string talkDevice = "Talk_" + body.deviceId;//每个音响创建一个会话
            long newTimestamp = DataHelper.GetTimeSpan(DateTime.Now);//句子时间戳
            string newTitleid = nlpEntity.log_id.ToString();//句子id
            string _Talkid = "";
            string userid = RedisHelper.GetHostId(body.deviceId);//音响对应用户

            body.timestamp = newTimestamp;//	时间标签(Long型)
            body.baidu_Items = nlpEntity.items;//	分词列表(字符串数组)

            string field = AIControler.Getdomain(body);//获取领域
            body.field = field; //领域
            int sort = 1;//会话中句子的顺序
            string preTitleid = "";//上一个句子id

            string questions = "";//问题列表：(字符串数组) 音箱的会话记录
            string answers = "";//回答列表：(字符串数组)  AIcontrol会话记录

            //创建会话缓存
            using (RedisHashService service = new RedisHashService())
            {
                string _timestampStr = service.GetValueFromHash(talkDevice, "timestamp");//获取缓存时间戳
                if (!string.IsNullOrEmpty(_timestampStr))
                {
                    long oldTimestamp = Convert.ToInt64(_timestampStr);
                    long cha = newTimestamp - oldTimestamp;
                    if (cha > 60)//一分钟60秒
                    {
                        _Talkid = body.deviceId + "_" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff");//超过一分钟，创建新的会话缓存
                        service.SetEntryInHash(talkDevice, "talkid", _Talkid);//缓存会话id
                    }
                    //一分钟内继续使用，不更新
                    else
                    {
                        _Talkid = service.GetValueFromHash(talkDevice, "talkid");
                        string sortStr= service.GetValueFromHash(talkDevice, "sort");//先获取当前顺序
                        if (!string.IsNullOrEmpty(sortStr))
                        {
                            sort = Convert.ToInt32(sortStr);
                        }
                        sort++;//一分钟内顺序+1
                    }
                }
                else
                {
                    _Talkid = body.deviceId + "_" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff");//没有新建
                    service.SetEntryInHash(talkDevice, "talkid", _Talkid);//缓存会话id
                }
                preTitleid = service.GetValueFromHash(talkDevice, "titleid");//前个句子id
                //每次都更新句子时间戳
                service.SetEntryInHash(talkDevice, "titleid", newTitleid);//缓存新的句子id
                service.SetEntryInHash(talkDevice, "timestamp", newTimestamp.ToString());//缓存新的时间戳
                service.SetEntryInHash(talkDevice, "field", field); //新的领域
                service.SetEntryInHash(talkDevice, "sort", sort.ToString()); //顺序


                string talkstate = service.GetValueFromHash(talkDevice, "state");//获取缓存会话模式,提前赋值
                body.talkstate = talkstate; //会话模式
                talkstate = "inquiry";
                if (talkstate == "inquiry" || talkstate == "await")//询问，等待
                {
                    Task.Run(() =>
                    {
                        //不用请求任何Nlu服务器，直接丢给AIControl函数Setanswer处理，返回抛给音箱
                        AIControler.Setanswer(body);
                    });
                }
                //else if(_state== "train")//训练模式
                else
                {
                    Task.Run(() =>
                    {
                        //发送收到的语音到NLP管理器
                        NlpControler.ProcessingRequest(body);
                    });
                }

                //启动线程存数据库
                Task.Run(() =>
                {
                    using (HsfDBContext hsfDBContext = new HsfDBContext())
                    {
                        //句子表
                        sound_title sound_Title = new sound_title()
                        {
                            titleid = newTitleid,
                            titletext = body.questions,
                            timestamp = newTimestamp,
                            preid= preTitleid,
                            sort = sort,
                            talkid = _Talkid,
                            sender = body.deviceId,
                            userid = userid,
                            sendtype = body.sourceId,
                            field = field,
                            talkstate = talkstate
                        };
                        hsfDBContext.sound_title.Add(sound_Title);

                        int items_sort = 0;
                        //分词表
                        foreach (var item in nlpEntity.items)
                        {
                            items_sort++;
                            baidu_items baidu_Items = new baidu_items()
                            {
                                wordid = Guid.NewGuid().ToString(),
                                titleid = newTitleid,
                                byte_length = item.byte_length,
                                byte_offset = item.byte_offset,
                                uri = item.uri,
                                pos = item.pos,
                                ne = item.ne,
                                item = item.item,
                                basic_words = string.Join(",", item.basic_words),
                                formal = item.formal,
                                sort = items_sort,
                                timestamp = newTimestamp
                            };
                            hsfDBContext.baidu_items.Add(baidu_Items);
                        }
                        hsfDBContext.SaveChanges();
                    }
                    log.Info($"保存到句子表和分词表");
                });

                


            }

        }
    }
}


//Thread.Sleep(5000);
////测试主动发送
//SoundTo bodyResult = new SoundTo()
//{
//    sessionId = Guid.NewGuid().ToString(),
//    deviceId = "200015010100002",
//    request = "",
//    actionId = "1001",
//    actionValue = "0",
//    url = "http://hao.haolingsheng.com/ring/000/995/fbc33cda344ba43992d3e1b809054280.mp3",
//    msg = "",
//    blwakeup = "0"
//};
//string jsonResult = JsonConvert.SerializeObject(bodyResult);
//session.Send(jsonResult);
//log.Info("主动发送"+ jsonResult);