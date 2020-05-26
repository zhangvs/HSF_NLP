using Hsf.Entity;
using Hsf.Framework;
using Hsf.Server;
using Hsf.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Hsf.Service
{
    public class SmartHomeRequest : INlp
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("SmartHomeNlp");
        //定义,最好定义成静态的, 因为我们只需要一个就好  
        static SocketManager smanager = null;
        static UserInfoModel userInfo = null;

        //定义事件与委托  
        public delegate void ReceiveData(object message);
        public delegate void ServerClosed();
        public static event ReceiveData OnReceiveData;
        public static event ServerClosed OnServerClosed;

        /// <summary>  
        /// 心跳定时器  
        /// </summary>  
        static System.Timers.Timer heartTimer = null;
        /// <summary>  
        /// 心跳包  
        /// </summary>  
        static ApiResponse heartRes = null;

        /// <summary>  
        /// 判断是否已连接  
        /// </summary>  
        public static bool Connected
        {
            get { return smanager != null && smanager.Connected; }
        }

        /// <summary>  
        /// 已登录的用户信息  
        /// </summary>  
        private static UserInfoModel UserInfo
        {
            get { return userInfo; }
        }


        #region 基本方法  
        /// <summary>  
        /// 连接到服务器  
        /// </summary>  
        /// <returns></returns>  
        public static SocketError Connect()
        {
            if (Connected) return SocketError.Success;
            //我这里是读取配置
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string ip = config.AppSettings.Settings["server"].Value;
            int port = Convert.ToInt32(config.AppSettings.Settings["port"].Value);
            if (string.IsNullOrWhiteSpace(ip) || port <= 1000) return SocketError.Fault;

            //创建连接对象, 连接到服务器  
            smanager = new SocketManager(ip, port);
            SocketError error = smanager.Connect();
            if (error == SocketError.Success)
            {
                //连接成功后,就注册事件. 最好在成功后再注册.  
                smanager.ServerDataHandler += OnReceivedServerData;
                smanager.ServerStopEvent += OnServerStopEvent;
            }
            return error;
        }

        /// <summary>  
        /// 断开连接  
        /// </summary>  
        public static void Disconnect()
        {
            try
            {
                smanager.Disconnect();
            }
            catch (Exception) { }
        }


        /// <summary>  
        /// 发送请求  
        /// </summary>  
        /// <param name="request"></param>  
        /// <returns></returns>  
        private static bool Send(ApiResponse request)
        {
            return Send(JsonConvert.SerializeObject(request));
        }

        /// <summary>  
        /// 发送消息  
        /// </summary>  
        /// <param name="message">消息实体</param>  
        /// <returns>True.已发送; False.未发送</returns>  
        public static bool Send(string message)
        {
            if (!Connected) return false;

            byte[] buff = Encoding.UTF8.GetBytes(message);
            //加密,根据自己的需要可以考虑把消息加密  
            //buff = AESEncrypt.Encrypt(buff, m_aesKey);  
            smanager.Send(buff);
            return true;
        }
        /// <summary>  
        /// 同步发送消息  
        /// </summary>  
        /// <param name="message">消息实体</param>  
        /// <returns>True.已发送; False.未发送</returns>  
        public static string SendSync(string message)
        {
            if (!Connected) Connect();
            byte[] buff = Encoding.UTF8.GetBytes(message);
            return smanager.SendSync(buff);
        }

        /// <summary>  
        /// 发送字节流  
        /// </summary>  
        /// <param name="buff"></param>  
        /// <returns></returns>  
        static bool Send(byte[] buff)
        {
            if (!Connected) return false;
            smanager.Send(buff);
            return true;
        }



        /// <summary>  
        /// 接收消息  
        /// </summary>  
        /// <param name="buff"></param>  
        private static void OnReceivedServerData(byte[] buff)
        {
            //To do something  
            //你要处理的代码,可以实现把buff转化成你具体的对象, 再传给前台  
            //if (OnReceiveData != null)
            //{
            //    OnReceiveData(buff);
            //    string str = System.Text.Encoding.Default.GetString(buff);
            //    Console.WriteLine($"接收到数据怎么传到前台是个问题：{str}");
            //    SmartHomeNlp.ReceiveMsg(str);
            //}
            if (buff!=null)
            {
                string str = Encoding.UTF8.GetString(buff);
                Console.WriteLine($"接收到数据怎么传到前台是个问题：{str}");
                //SmartHomeNlp.ReceiveMsg(str);
            }
        }


        /// <summary>  
        /// 服务器已断开  
        /// </summary>  
        private static void OnServerStopEvent()
        {
            if (OnServerClosed != null)
                OnServerClosed();
        }

        #endregion

        #region 心跳包  
        //心跳包也是很重要的,看自己的需要了, 我只定义出来, 你自己找个地方去调用吧  
        /// <summary>  
        /// 开启心跳  
        /// </summary>  
        private static void StartHeartbeat()
        {
            if (heartTimer == null)
            {
                heartTimer = new System.Timers.Timer();
                heartTimer.Elapsed += TimeElapsed;
            }
            heartTimer.AutoReset = true;     //循环执行  
            heartTimer.Interval = 30 * 1000; //每30秒执行一次  
            heartTimer.Enabled = true;
            heartTimer.Start();

            ////初始化心跳包  
            //heartRes = new ApiResponse((int)ApiCode.心跳);
            //heartRes.data = new Dictionary<string, object>();
            //heartRes.data.Add("beat", Function.Base64Encode(userInfo.nickname + userInfo.userid + DateTime.Now.ToString("HH:mm:ss")));
        }

        /// <summary>  
        /// 定时执行  
        /// </summary>  
        /// <param name="source"></param>  
        /// <param name="e"></param>  
        static void TimeElapsed(object source, ElapsedEventArgs e)
        {
            //Request.Send(heartRes);
        }

        #endregion



        /// <summary>
        /// 向智能主机发送音响语音内容，异步返回答案为null
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public NlpAnswers SendMsg(SoundBodyRequest body)
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

                Send(sendstr);

                log.Info($"智能家居给{user}发送数据：{sendstr}完毕");
            }
            catch
            { };
            return null;
        }

        public string CallbackMsg(AIAnswers _Semantics)
        {
            try
            {
                //string sendMsg = "connect user:wali_Server type:other msg:wali_C40BCB80050A;8;815;0$/r$\r\n";
                //string returnMsg = "";

                ////通过 clientSocket 发送指令
                //clientSocket.Send(Encoding.UTF8.GetBytes(msg));
                //int receiveL2 = clientSocket.Receive(result);
                //returnMsg = Encoding.UTF8.GetString(result, 0, receiveL2);
                //if (string.IsNullOrEmpty(returnMsg))
                //{
                //    returnMsg = $"{msg}无回调结果";
                //}
                //return returnMsg;

                return "smartHome回调";

                //string returnMsg = SmartRequest.SendSync(msg);
                //return $"{msg} 回调 {returnMsg}";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ex.Message;
            }
        }


        public static string ReceiveMsg(string msg)
        {
            Console.WriteLine($"拿到返回数据，怎么传到语义槽是个问题{msg}");
            return msg;
        }
    }
}
