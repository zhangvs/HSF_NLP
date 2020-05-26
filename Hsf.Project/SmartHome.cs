using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using System.IO;
using SuperSocket.SocketBase.Config;
using Newtonsoft.Json;
using Hsf.Entity;
using Hsf.Framework;
using System.Threading.Tasks;

namespace Hsf.Project
{
    public class SmartHome
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("SmartHome");
        /// <summary>
        /// 向智能主机发送音响语音内容，异步返回答案为null
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static NlpAnswers SendMsg(SoundBodyRequest body)
        {
            log.Info($"音响{body.sessionId}向智能家居smarthome发送问题:{body.questions}");
            string user = "wali_Server";
            string sendstr = "";
            if (!string.IsNullOrEmpty(body.deviceId))
            {
                //读取配置文件中音响对应的主机
                user = ConfigAppSettings.GetValue(body.deviceId);//wali_Server
                log.Info($"音响{body.deviceId}对应的主机为：{user}");
            }
            else
            {
                log.Info($"音响{body.deviceId}对应的主机不存在，请配置。");
            }

            try
            {
                ///这里要判断，数据类型了
                if (body.sourceId == "mengdou")
                {
                    String temp = body.req.ToString();
                    if (temp.IndexOf("service\":\"musicX") > -1)
                    {
                        sendstr = body.sessionId + "_" + body.deviceId + ";5;517;" + EncryptionHelp.Encryption("music@" + body.req, false) + "$/r$" + "\r\n";
                    }
                    else
                    if (temp.IndexOf("service\":\"news") > -1)
                    {
                        sendstr = body.sessionId + "_" + body.deviceId + ";5;517;" + EncryptionHelp.Encryption("news@" + body.req, false) + "$/r$" + "\r\n";
                    }
                    else
                    if (temp.IndexOf("service\":\"story") > -1)
                    {
                        sendstr = body.sessionId + "_" + body.deviceId + ";5;517;" + EncryptionHelp.Encryption("story@" + body.req, false) + "$/r$" + "\r\n";
                    }
                    else
                    if (temp.IndexOf("service\":\"joke") > -1)
                    {
                        sendstr = body.sessionId + "_" + body.deviceId + ";5;517;" + EncryptionHelp.Encryption("joke@" + body.req, false) + "$/r$" + "\r\n";
                    }
                    else
                        sendstr = body.sessionId + "_" + body.deviceId + ";5;513;" + EncryptionHelp.Encryption(body.questions, false) + "$/r$" + "\r\n";
                }
                else
                {
                    //sessionid_deviceId;5;513;base64不zip（打开窗帘）$/r$
                    sendstr = body.sessionId + "_" + body.deviceId + ";5;513;" + EncryptionHelp.Encryption(body.questions, false) + "$/r$" + "\r\n";
                }
                SmartHomeServer.SendMsg(user,sendstr);
                log.Info($"智能家居给{user}发送数据：{sendstr}完毕");
            }
            catch
            {};
            return null;
        }


        /// <summary>
        /// 异步返回智能家居数据
        /// 12345678_Ying;513;5omT5byA56qX5biY;Zip;H4sIAAAAAAAAAAEfAOD/6K6+5aSH5peg5rOV6K+G5YirQOaJk+W8gOeql+W4mCDHP38fAAAA$/r$
        /// </summary>
        /// <param name="msg"></param>
        public static void AsyncReceiveMsg(string msg)
        {
            log.Info($"AsyncSmartHomeReceiveMsg异步返回：{msg}");
            try
            {
                if (msg.IndexOf(";") > 0)
                {
                    //根据返回字符串得到sessionId
                    string sessionId = msg.Split(';')[0].Split('_')[0];
                    string deviceId = msg.Split(';')[0].Split('_')[1];
                    log.Info("AsyncSmartHomeReceiveMsg异步返回sessionId：" + sessionId);

                    //添加当前智能家居异步返回语义
                    string msg64j = EncryptionHelp.Decrypt(msg.Split(';')[4].Replace("$/r$", ""), true);//解密"设备无法识别@打开窗帘"
                    string Answers;
                    if (msg64j.IndexOf("@") > 0)
                    {
                        Answers = "";//2020播放响应效果音可持续交流
                    }
                    else
                    {
                        Answers = msg64j; //2011播放url内容，播放完自动唤醒
                    }
                    NlpAnswers _NlpAnswers = new NlpAnswers()
                    {
                        Code = "SmartHomeNlp",
                        Name = "家居",
                        Answers = Answers,
                    };

                    //————————————————向中枢控制器指定的sessionId发送返回的语义结果——————————————————————————
                    NlpControler.AsyncReturnMsg(sessionId, _NlpAnswers);

                    log.Info($"AsyncSmartHomeReceiveMsg异步完毕,向中枢控制器指定的{sessionId}发送返回的语义结果:{_NlpAnswers.Answers}");
                }
            }
            catch (Exception ex)
            {
                log.Info($"异步发生异常:" + ex.Message);
            }

        }

        /// <summary>
        /// 向智能主机执行回调方法
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static string CallbackMsg(AIAnswers _Semantics)
        {
            log.Info("智能家居CallbackMsg回调:::::::" + _Semantics.Questions);
            Console.WriteLine("智能家居CallbackMsg回调:::::::" + _Semantics.Questions);
            log.Info($"智能家居smarthome{_Semantics.SessionId}回调完毕:::::::");
            return "sendok";

        }
    }
}
