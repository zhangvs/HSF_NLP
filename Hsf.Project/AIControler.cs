using Hsf.Model;
using Hsf.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hsf.Framework;

namespace Hsf.Project
{
    public class AIControler
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("AIControler");
        /// <summary>
        ///调用第三方语义排序API，对于语义进行排序
        /// </summary>
        /// <param name="_SemanticsDictionary">传递语义槽</param>
        /// <returns>排序结果</returns>
        public static void GetAIAnswers(SemanticsSlot _SemanticsSlot,string type)
        {
            try
            {
                string sessionId = _SemanticsSlot.SessionId;
                string deviceId = _SemanticsSlot.DeviceId;
                string questions = _SemanticsSlot.Questions;
                string sourceid = _SemanticsSlot.SourceId;
                log.Info($"{questions} 执行方式：{type} 返回次数：{_SemanticsSlot.Answertimes} 语义槽数量：{_SemanticsSlot.NlpAnswers.Count} ");
                //过滤掉所有空
                var Answers = _SemanticsSlot.NlpAnswers.OrderBy(t => t.Level);//.Where(t => !string.IsNullOrEmpty(t.Answers))
                if (Answers.Count()>0)
                {
                    foreach (var item in Answers)
                    {
                        string code = item.Code;
                        string answers = item.Answers;
                        //优先返回基础nlp
                        if (code == "BaseNlp")
                        {
                            NlpControler.BackAnswers(sessionId, deviceId, questions, "2011", answers, BaiduSDK.Tts(answers));// 2011播放url内容，播放完自动唤醒
                            break;
                        }
                        //返回智能家居nlp,不带的设备无法识别@
                        else if (code == "SmartHomeNlp" && !answers.Contains("2014@"))
                        {
                            if (answers.Split('@').Length == 2)
                            {
                                string action = answers.Split('@')[0];
                                string req = answers.Split('@')[1];
                                NlpControler.BackAnswers(sessionId, deviceId, questions, action, answers, "");//根据@中的action返回
                                break;
                            }
                            else
                            {
                                NlpControler.BackAnswers(sessionId, deviceId, questions, "2020", answers, "");//2020播放响应效果音可持续交流
                                break;
                            }
                        }
                        else
                        {
                            //设备无法识别的时候，优先返回音响自带的结果（不为空的情况下）
                            var item2 = _SemanticsSlot.NlpAnswers.Where(t => t.Code == "SoundNlp").FirstOrDefault();
                            if (sourceid!="mengdou")
                            {
                                ///执行百度自己请求
                                NlpControler.BackAnswers(sessionId, deviceId, questions, "2010", "无法识别您说的意思", "");
                                break;
                            }
                            else 
                            if (item2 != null)
                            {
                                if (answers.Contains("service\":\"musicX") ||
                                    answers.Contains("service\":\"news") ||
                                    answers.Contains("service\":\"story") ||
                                    answers.Contains("service\":\"joke"))
                                {
                                    NlpControler.BackAnswers(sessionId, deviceId, questions, "2025", item2.Answers, "");//2025主机不返回，超时，播放错误音，其实主机已经在播放音乐
                                    break;
                                }
                                else
                                {
                                    NlpControler.BackAnswers(sessionId, deviceId, questions, "2011", item2.Answers, BaiduSDK.Tts(item2.Answers));// 2011播放url内容，播放完自动唤醒
                                    break;
                                }
                            }
                            else
                            {
                                NlpControler.BackAnswers(sessionId, deviceId, questions, "2014", "无法识别您说的意思", "");
                                //NlpControler.BackAnswers(sessionId, deviceId, questions, "2014", "无法识别您说的意思", BaiduSDK.mp3Fail);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (sourceid != "mengdou")
                        ///执行百度自己请求
                        NlpControler.BackAnswers(sessionId, deviceId, questions, "2010", "无法识别您说的意思", "");
                    else
                        NlpControler.BackAnswers(sessionId, deviceId, questions, "2014", "无法识别您说的意思", "");
                    //NlpControler.BackAnswers(sessionId, deviceId, questions, "2014", "无法识别您说的意思", BaiduSDK.mp3Fail);
                }
            }
            catch (Exception ex)
            {
                log.Info($"{_SemanticsSlot.Questions} AI处理异常: {ex.Message}");
            }
        }

        
        /// <summary>
        /// 获取领域:音乐，聊天(天气)，家庭设备控制，视频
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static string Getdomain(SoundBodyRequest body)
        {
            return "smarthome";
        }

        /// <summary>
        /// 不用请求任何Nlu服务器，直接丢给AIControl函数Setanswer处理，返回抛给音箱
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static void Setanswer(SoundBodyRequest body)
        {
            string answers = "不用请求任何Nlu服务器，直接丢给AIControl函数Setanswer处理，返回抛给音箱";
            NlpControler.BackAnswers(body.sessionId, body.deviceId, body.questions, "2011", answers, BaiduSDK.Tts(answers));
        }
        
    }
}
