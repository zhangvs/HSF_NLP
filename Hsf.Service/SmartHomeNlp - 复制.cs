using Hsf.Entity;
using Hsf.Framework;
using Hsf.Interface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hsf.Service
{
    public class SmartHomeNlp2:INlp
    {
        private Socket clientSocket = null;
        private IPAddress ipAddress = null;
        private byte[] result = new byte[1024];
        private string user { get; set; }

        public SmartHomeNlp2()
        {
            this.user = DateTime.Now.ToString("yyyyMMdd-HHmmss:fff");
            Connect();
        }

        //public string SendMsg(string msg)
        //{
        //    Thread.Sleep(600);
        //    return $"{msg}打开窗帘";
        //}
        //public string CallbackMsg(string msg)
        //{
        //    return $"{msg}SmartHome开窗帘！";
        //}

        public void Connect()
        {
            try
            {
                //读取配置
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                string ip = config.AppSettings.Settings["server"].Value;
                int port = Convert.ToInt32(config.AppSettings.Settings["port"].Value);
                ipAddress = IPAddress.Parse(ip);
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//实例化WebSocketServer
                clientSocket.ReceiveTimeout = 1500;

                if (!clientSocket.Connected)
                {
                    //添加事件侦听
                    clientSocket.Connect(new IPEndPoint(ipAddress, port)); //配置服务器IP与端口
                    int connectLength = clientSocket.Receive(result);//通过clientSocket接收数据
                    string connectMsg = Encoding.UTF8.GetString(result, 0, connectLength);//Even 2018/10/17 17:00:53
                    //Console.WriteLine($"连接：{connectMsg}");

                    string registMsg = $"connect user:wali_{user} type:home msg:{user}\r\n";
                    //Console.WriteLine($"注册：{registMsg}");
                    byte[] registBuffer = Encoding.UTF8.GetBytes(registMsg);
                    clientSocket.Send(registBuffer);
                    int registLength = clientSocket.Receive(result);
                    string registBack = Encoding.UTF8.GetString(result, 0, registLength);
                    //Console.WriteLine($"注册返回：{registBack}");//regist sucess
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("连接失败");
                Console.WriteLine(e.ToString());
                return;
            }
        }

        private static log4net.ILog log = log4net.LogManager.GetLogger("SmartHome");
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

                //SmartHomeServer.SendMsg(user, sendstr);

                log.Info($"智能家居给{user}发送数据：{sendstr}完毕");
            }
            catch
            { };
            return null;
        }

        //public string SendMsg(string msg)
        //{
        //    try
        //    {
        //        string sendMsg = $"connect user:wali_Server type:other msg:wali_{user};8;815;0$/r$\r\n";
        //        //Console.WriteLine($"发送：{sendMsg}");
        //        //通过 clientSocket 注册用户{ DateTime.Now.ToString("yyyyMMdd-HHmmss:fff")}
        //        byte[] sendBuffer = Encoding.UTF8.GetBytes(sendMsg);
        //        clientSocket.Send(sendBuffer);
        //        clientSocket.ReceiveTimeout = 1500;

        //        //Console.WriteLine($"【{Thread.CurrentThread.ManagedThreadId.ToString("00")}】发送SmartHome内容：{msg}");
        //        int receiveL = clientSocket.Receive(result);
        //        string returnMsg = Encoding.UTF8.GetString(result, 0, receiveL);
        //        Console.WriteLine($"发送返回：{returnMsg} 返回时间：{DateTime.Now.ToString("yyyyMMdd-HHmmss:fff")}");
        //        return returnMsg;

        //        //string returnMsg = SmartRequest.SendSync(msg);
        //        //return $"{msg} 返回 {returnMsg}";
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"连接服务器失败，请按回车键退出！{ex.Message}");
        //        return null;
        //    }
        //}


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

//if (!NlpSocketManager.Connected)
//{
//    NlpSocketManager.Connect();
//    Console.WriteLine("NlpSocketManager服务器 断线重连！");
//}


//if (!clientSocket.Poll(200, SelectMode.SelectRead))
//{
//    returnMsg = $"{msg}re: socket not read...";
//}
//else
//{
//    // byt = new byte[1];
//    do
//    {
//        int receiveL2 = clientSocket.Receive(result);
//        returnMsg = Encoding.UTF8.GetString(result, 0, receiveL2);
//        if (string.IsNullOrEmpty(returnMsg))
//        {
//            returnMsg = $"{msg}无回调结果";
//        }
//        //} while (s.Poll(500, SelectMode.SelectRead) && s.Available > 0 && s.Connected);
//    } while (clientSocket.Available > 0);
//    //Poll 可检测缓冲区是否还有数据可读。 , 如 socket 处于 blocking 状态 ， Receive 时 防止   blocking , 可先调用 Poll 检测是否可读， 同时 Available 属性 应大于 0 , 再调用  Receive
//    // } while (s.Poll(500, SelectMode.SelectRead));
//}



//connect user:wali_C40BCB80050A type:home msg:C40BCB80050A
//connect user:wali_Server type:other msg:wali_C40BCB80050A;8;815;0$/r$