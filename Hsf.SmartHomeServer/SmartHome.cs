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

namespace Hsf.SmartHomeServer
{
    public class SmartHome
    {

        private static IMthod innerClassA = null;

        public SmartHome(IMthod classA)
        {
            innerClassA = classA;
        }
        
        private static log4net.ILog log = log4net.LogManager.GetLogger("SmartHome");
        private static bool debug = true;
        private static AppServer server = new AppServer();
        public static void Start()
        {
            //server.Setup(

            server.SessionClosed += new SessionHandler<AppSession, SuperSocket.SocketBase.CloseReason>(server_SessionClosed);
            server.NewRequestReceived += new RequestHandler<AppSession, SuperSocket.SocketBase.Protocol.StringRequestInfo>(server_NewRequestReceived);
            server.NewSessionConnected += new SessionHandler<AppSession>(server_NewSessionConnected);
            ServerConfig config = new ServerConfig();

            config.Port = 9002;
            config.MaxRequestLength = 8192;
            config.ClearIdleSession = false;
            //config.SendBufferSize = 10240;
            //config.IdleSessionTimeOut = 50000;
            //config.KeepAliveTime = 120;
            server.Setup(config);
            //timer1.Enabled = false;
            if (server.Start())
                //this.textBox1.Text += "服务器开启 \r\n";
                log.Info($"SmartHome服务器开启 开启时间：{DateTime.Now.ToString("yyyyMMdd-HHmmss:fff")}");

            Console.WriteLine($"SmartHome服务器开启 开启时间：{DateTime.Now.ToString("yyyyMMdd-HHmmss:fff")}");
        }

        public static void server_SessionClosed(AppSession session, SuperSocket.SocketBase.CloseReason value)
        {
            //File.AppendAllText("D:\\111\\1.log", "~~~" + DateTime.Now.ToString() + "~~~" + session.SessionID);
            //ChangeText c = new ChangeText(Get);
            try
            {
                String name = "";
                foreach (var us in ls)
                {
                    if (us.sesson == session)
                    {
                        if (debug == true)
                            File.AppendAllText("c:\\log\\1.log", DateTime.Now.ToString() + us.user + "离开服务器\r\n");
                        name = us.user;
                        ls.Remove(us);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                File.AppendAllText("c:\\log\\1.log", e.ToString() + "\r\n");
            }
        }

        private void Get(string text)
        {
            //this.textBox1.Text += text;

        }

        public static void server_NewRequestReceived(AppSession session, SuperSocket.SocketBase.Protocol.StringRequestInfo requestInfo)
        {
            try
            {
                //MessageBox.Show(requestInfo.Key);
                //if (debug == true)
                //File.AppendAllText("c:\\log\\6002.log", requestInfo.Body);
                String sendstr = "";
                if (requestInfo != null && requestInfo.Key != null && requestInfo.Key.ToLower() == "Heartbeat".ToLower())
                {
                    //session.Send("alive");
                    return;
                }
                //GetModelype.Get()
                //JsonConvert.DeserializeObject<ServerMessage>(requestInfo.Body);
                //ChangeText c = new ChangeText(Get);
                //this.Invoke(c, "云服务器收到数据转发："+requestInfo.Body+"\r\n");
                //if (requestInfo.Body.Split(' ')[1].Split(':')[1] == "rename")
                //{
                //    int i = 0;
                //    foreach (var us in ls)
                //    {
                //        if (us.user == requestInfo.Body.Split(' ')[0].Split(':')[1])
                //        {
                //            us.user = requestInfo.Body.Split(' ')[2].Split(':')[1];
                //            ls[i] = us;
                //            us.sesson.Send("regist sucess");
                //            return;
                //        }
                //        i++;
                //    }
                //    //ls.Add(new US() { sesson = session, user = requestInfo.Body.Split(' ')[0].Split(':')[1] });
                //    //session.Send("regist sucess");
                //    return;

                //}

                if (requestInfo.Body.Split(' ')[1].Split(':')[1] == "isonline")
                {
                    ///校验当前服务器是否有同名的用户       
                    Boolean reg = true;

                    foreach (var us in ls)
                    {
                        if (debug == true)
                            File.AppendAllText("c:\\log\\6002.log", us.user + "@" + us.mac + "\r\n");
                        if ((us.user == requestInfo.Body.Split(' ')[0].Split(':')[1]) && (ToggleCase(us.mac) != ToggleCase(requestInfo.Body.Split(' ')[2].Split(':')[1])))
                        {
                            reg = false;
                            break;
                        }
                    }
                    if (reg)
                        session.Send("offline");
                    else
                        session.Send("online");
                    return;
                }

                if (requestInfo.Body.Split(' ')[1].Split(':')[1] == "home")
                {
                    ///校验当前服务器是否有同名的用户
                    Boolean reg = true;
                    foreach (var us in ls)
                    {
                        if ((us.user == requestInfo.Body.Split(' ')[0].Split(':')[1]) && (ToggleCase(us.mac) != ToggleCase(requestInfo.Body.Split(' ')[2].Split(':')[1])))
                        {
                            if (debug == true)
                                File.AppendAllText("c:\\log\\6002.log", us.user + "@" + requestInfo.Body.Split(' ')[0].Split(':')[1] + ToggleCase(us.mac) + "@" + ToggleCase(requestInfo.Body.Split(' ')[2].Split(':')[1]) + "异地登陆被强制下线\r\n");
                            us.sesson.Send("repeat link");
                            reg = false;

                            //ls.Remove(us);
                        }
                    }

                    //if (reg)
                    //{
                    ls.Add(new US() { sesson = session, user = requestInfo.Body.Split(' ')[0].Split(':')[1], mac = requestInfo.Body.Split(' ')[2].Split(':')[1] });
                    session.Send("regist sucess");
                    if (debug == true)
                    {
                        File.AppendAllText("c:\\log\\6002.log", "在线人数" + ls.Count + "," + DateTime.Now.ToString() + requestInfo.Body.Split(' ')[0].Split(':')[1] + "登陆服务器\r\n");
                        log.Info("在线人数" + ls.Count + "," + DateTime.Now.ToString() + requestInfo.Body.Split(' ')[0].Split(':')[1] + "登陆服务器\r\n");
                        Console.WriteLine("智能家居app端连接成功");
                    }

                    //}
                    if (!reg)
                    {
                        session.Send("repeat link");
                    }
                    return;
                }
                //转发消息
                if (requestInfo.Body.Split(' ')[1].Split(':')[1] == "other")
                {
                    sendstr = relay(requestInfo, sendstr);

                    var u = requestInfo.Body.Split(' ')[0].Split(':')[1];
                    var sesson = ls.FirstOrDefault(p => p.user == u);
                    var msg = requestInfo.Body.Split(' ')[2].Substring(4, requestInfo.Body.Split(' ')[2].Length - 4);
                    if (sesson != null)
                        sesson.sesson.Send(msg);
                    else
                    if (msg.IndexOf("513") > 0)
                    {
                        log.Info("智能家居nlp接收信息513 ，多线程");
                        Task.Run(() =>
                        {
                            //SmartHomeNlp.SmartHomeCallbackMsg(msg);
                            innerClassA.ReceiveMsg(msg);
                        });

                        log.Info("智能家居nlp接收信息完毕");
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }


        }


        public static string relay(SuperSocket.SocketBase.Protocol.StringRequestInfo requestInfo, String sendstr)
        {
            bool sok = false;
            //session.Send("server noactive");
            String user = requestInfo.Body.Split(' ')[0].Split(':')[1];
            //群发广播
            if (user.IndexOf("%") > 0)
            {
                user = user.Replace("%", "");
                foreach (var us in ls)
                {
                    if (debug == true)
                        File.AppendAllText("c:\\log\\6002.log", "用户对比" + us.user + ":" + requestInfo.Body.Split(' ')[0].Split(':')[1] + "\r\n");
                    if (us.user.IndexOf(user) > -1)
                    {
                        //string[] rev = requestInfo.Body.Split(' ')[2].Split(':');
                        //for (int i=1;i<rev.Count;i++)
                        try
                        {
                            sendstr = requestInfo.Body.Split(' ')[2].Substring(4, requestInfo.Body.Split(' ')[2].Length - 4);
                            if (debug == true)
                                File.AppendAllText("c:\\log\\6002.log", "转发数据" + us.user + "数据" + sendstr + "\r\n");
                            us.sesson.Send(sendstr);
                        }
                        catch
                        {

                        };
                        sok = true;

                    }

                }
            }
            else
            {
                foreach (var us in ls)
                {
                    if (debug == true)
                        File.AppendAllText("c:\\log\\6002.log", "用户对比" + us.user + ":" + requestInfo.Body.Split(' ')[0].Split(':')[1] + "\r\n");
                    if (us.user == requestInfo.Body.Split(' ')[0].Split(':')[1])
                    {
                        //string[] rev = requestInfo.Body.Split(' ')[2].Split(':');
                        //for (int i=1;i<rev.Count;i++)
                        try
                        {
                            sendstr = requestInfo.Body.Split(' ')[2].Substring(4, requestInfo.Body.Split(' ')[2].Length - 4);
                            if (debug == true)
                                File.AppendAllText("c:\\log\\6002.log", "转发数据" + us.user + "数据" + sendstr + "\r\n");
                            us.sesson.Send(sendstr);
                        }
                        catch
                        {

                        };
                        sok = true;
                        break;
                    }

                }
            }

            if (!sok)
            {
                //requestInfo.Body.Split(' ')[2].Split(':')[1].Split(';')[0]
                foreach (var us in ls)
                {
                    if (us.user == requestInfo.Body.Split(' ')[2].Split(':')[1].Split(';')[0])
                    {
                        us.sesson.Send("server noactive");
                        break;
                    }

                }
            }
            return sendstr;
        }

        public string sendvoice(SoundBodyRequest body)
        {
            log.Info("智能家居smarthome:::::::" + body.questions);
            string user = "wali";
            string sendstr = "";
            if (!string.IsNullOrEmpty(body.deviceId))
            {
                user = ConfigAppSettings.GetValue(body.deviceId);//wali_Server
            }
            else
            {
                return "";
            }
            bool sok = false;

            //群发广播
            if (user.IndexOf("%") > 0)
            {
                user = user.Replace("%", "");
                foreach (var us in ls)
                {

                    if (us.user.IndexOf(user) > -1)
                    {

                        try
                        {
                            //sendstr = requestInfo.Body.Split(' ')[2].Substring(4, requestInfo.Body.Split(' ')[2].Length - 4);
                            sendstr = "connect user:" + user + " type:other msg:" + "12345678_Ying" + ";5;513;5omT5byA56qX5biY" + "$/r$" + "\r\n";
                            us.sesson.Send(sendstr);
                        }
                        catch
                        {

                        };
                        sok = true;

                    }

                }
            }
            else
            {
                user = "wali_Server";
                foreach (var us in ls)
                {
                    if (us.user == user)
                    {
                        try
                        {
                            //Task.Run(() =>
                            //{
                            log.Info("多线程发送数据user是：" + user);
                            sendstr = body.sessionId + "_" + body.deviceId + ";5;513;" + EncryptionHelp.Encryption(body.questions, true) + "$/r$" + "\r\n";
                            us.sesson.Send(sendstr);
                            log.Info("给user发送数据完毕" + body.sessionId);
                            //});

                        }
                        catch
                        {

                        };
                        sok = true;
                        break;
                    }
                }
            }

            if (!sok)
            {
                //requestInfo.Body.Split(' ')[2].Split(':')[1].Split(';')[0]
                return "server noactive";
            }

            log.Info("智能家居smarthome完毕:::::::" + body.sessionId);
            return "sendok";

        }

        public static string ToggleCase(string input)
        {
            char[] array = input.ToCharArray();
            char temp;

            for (int i = 0; i < array.Length; i++)
            {
                temp = array[i];

                if (char.IsUpper(temp))
                    array[i] = char.ToLower(temp);
                // else if (char.IsLower(temp))
                //array[i] = char.ToUpper(temp);
            }

            return new string(array);
        }




        private static List<US> ls = new List<US>();


        public static void server_NewSessionConnected(AppSession session)
        {

            //File.AppendAllText("D:\\111\\1.log", "~~~"+DateTime.Now.ToString()+"~~~"+session.SessionID);            
            session.Send("Even " + DateTime.Now.ToString());




        }




    }
}
public class US
{
    public string user;
    public AppSession sesson;
    public string type;
    public string mac;
    public DateTime date;
}