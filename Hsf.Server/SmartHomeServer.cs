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
using System.Threading.Tasks;
using Hsf.DAL;
using Hsf.Framework;
using Hsf.Entity;
using Hsf.Model;

namespace Hsf.Server
{
    public static class SmartHomeServer
    {
        #region 声明
        private static log4net.ILog log = log4net.LogManager.GetLogger("SmartHomeServer");
        private static bool debug = true;
        private static AppServer server = new AppServer();
        //智能家居主机服务器连接集合
        private static List<US> ls = new List<US>();
        #endregion

        #region 服务器主动发出数据受理委托及事件  
        //返回语义答案
        public delegate void OnServerDataReceive(string msg);
        public static event OnServerDataReceive _SmartHomeNlpReceiveMsg;
        //主动请求音响
        public delegate void OnServerDataRequest(SoundPassiveRequest _SoundPassiveRequest);
        public static event OnServerDataRequest _SmartHomeNlpRequestMsg;

        /// <summary>  
        /// 使用新进程通知事件回调  
        /// </summary>  
        /// <param name="msg"></param>  
        public static void DoReceiveEvent(string msg)
        {
            if (_SmartHomeNlpReceiveMsg == null) return;

            Task.Run(() =>
            {
                _SmartHomeNlpReceiveMsg(msg);
            });
        }

        /// <summary>  
        /// 向音响发送指令 
        /// </summary>  
        /// <param name="req"></param>  
        public static void DoRequestEvent(SoundPassiveRequest _SoundPassiveRequest)
        {
            if (_SmartHomeNlpRequestMsg == null) return;

            Task.Run(() =>
            {
                _SmartHomeNlpRequestMsg(_SoundPassiveRequest);
            });
        }
        #endregion

        #region 启动
        public static void Start()
        {
            //server.Setup(

            server.SessionClosed += new SessionHandler<AppSession, SuperSocket.SocketBase.CloseReason>(server_SessionClosed);
            server.NewRequestReceived += new RequestHandler<AppSession, SuperSocket.SocketBase.Protocol.StringRequestInfo>(server_NewRequestReceive);
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
        #endregion
        
        #region 关闭
        public static void server_SessionClosed(AppSession session, SuperSocket.SocketBase.CloseReason value)
        {
            log.Info("离开服务器\r\n");
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
        #endregion

        #region 处理请求

        public static void server_NewRequestReceive(AppSession session, SuperSocket.SocketBase.Protocol.StringRequestInfo requestInfo)
        {
            try
            {
                if (!string.IsNullOrEmpty(requestInfo.Key) && !string.IsNullOrEmpty(requestInfo.Body))
                {
                    log.Info($"{DateTime.Now.ToString("yyyyMMdd-HHmmss:fff")}SmartHome服务器接收到新的消息key ：{requestInfo.Key} Body ：{requestInfo.Body}");
                }

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
                if (requestInfo.Body.Contains(";"))
                {
                    string requestType = requestInfo.Body.Split(' ')[1].Split(':')[1];
                    if (requestType == "isonline")
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

                    if (requestType == "home")
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
                    if (requestType == "other")
                    {
                        sendstr = relay(requestInfo, sendstr);

                        var u = requestInfo.Body.Split(' ')[0].Split(':')[1];
                        var sesson = ls.FirstOrDefault(p => p.user == u);
                        //user:803802e9-bf27-4593-8ba0-96fd2e12b29d_200015010100002 type:other msg:803802e9-bf27-4593-8ba0-96fd2e12b29d_200015010100002;513;5pS+5LiA6aaW5q2M;Zip;H4sIAAAAAAAAAAEfAOD/6K6+5aSH5peg5rOV6K+G5YirQOaUvuS4gOmmluatjBG7oFsfAAAA$/r$
                        var msg = requestInfo.Body.Split(' ')[2].Substring(4, requestInfo.Body.Split(' ')[2].Length - 4);
                        if (sesson != null)
                            sesson.sesson.Send(msg);
                        else if (msg.IndexOf("513;") > 0 || msg.IndexOf("517;") > 0 || msg.IndexOf("515;") > 0)
                        {
                            //异步接收到smarthome返回结果
                            DoReceiveEvent(msg);
                        }
                        else if (msg.Contains("8212;"))
                        {
                            //注册音响
                            RegSoundHost(msg);
                        }
                        else if (msg.Contains("8215;"))
                        {
                            //是否自身播放音乐状态反转
                            UpdatePlayState(msg);
                        }
                        else if (msg.Contains("8216;"))
                        {
                            //转发给音箱，音箱自动升级的指令

                        }

                    }
                }
                
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion



        #region 注册音响
        /// <summary>
        /// 注册音响
        /// </summary>
        /// <param name="msg">user:wali_Server type:other 
        /// msg:wali_Server;8;8212;ALL;H4sIAAAAAAAAAG2RsU7DMBCG38VzVOUcN6TZGBlggBGh6ORcUkuJHdlJI1RVYmFn4ykQAyuv04G3wIlLRBHy4u/+/3y/zvd7dkPjLUljS5b3dqCIFcpf44jJrdKosSWWM5RZUiUgCKsL5qUGnasmH8s8lrTDBm3rMZDcotbULKwkzWbgsAbBIRYiSeOT1s1CEqhFOeGGryDNVlm2gpgHpUOLLN/72cZNkY7PH1+vbxEHiPz522A60pPp8+n48v6/6RCeNbZfcnbGqV4ZHaICCJHGsPlJaqmWpqTfcV2PPS3tM8E58gX7x26yXl4V16Tr0gy+rFqsw2q8AeK1LzW1myP5H+jM6Bc7nAYMjuxsHbFRxR3ZHVl2ePgGQ/XuYsMBAAA=$/r$</param>
        public static void RegSoundHost(string msg)
        {
            string msg64j = EncryptionHelp.Decrypt(msg.Split(';')[4].Replace("$/r$", ""), true);//解密"设备无法识别@打开窗帘"
            msg64j = msg64j.Replace("[", "").Replace("]", "");
            SoundHost _SoundHost = JsonConvert.DeserializeObject<SoundHost>(msg64j);//反序列化

            using (HsfDBContext hsfDBContext = new HsfDBContext())
            {
                sound_host sound_Host = new sound_host()
                {
                    id = Guid.NewGuid().ToString(),
                    chinaname = _SoundHost.chinaname,
                    classfid = _SoundHost.classfid,
                    deviceid = _SoundHost.deviceid,
                    devip = _SoundHost.devip,
                    devmac = _SoundHost.devmac,
                    devport = _SoundHost.devport,
                    devposition = _SoundHost.devposition,
                    devregcode = _SoundHost.devregcode,
                    devtype = _SoundHost.devtype,
                    imageid = _SoundHost.imageid,
                    lgsort = _SoundHost.lgsort,
                    userid = _SoundHost.userid,
                    playstate = 1,//是否自身播放音乐状态反转,0：否，1：是
                    createtime = DateTime.Now
                };
                hsfDBContext.sound_host.Add(sound_Host);
                hsfDBContext.SaveChanges();
            }
        }
        #endregion


        #region 更改是否自身播放音乐状态反转
        /// <summary>
        /// 更改是否自身播放音乐状态反转
        /// </summary>
        /// <param name="msg">user:wali_Server type:other 
        /// msg:wali_C40BCB80050A;8;8215;88888888;1$/r$</param>
        public static void UpdatePlayState(string msg)
        {
            if (msg.IndexOf(";") > 0)
            {
                string userid = msg.Split(';')[0];
                string deviceid = msg.Split(';')[3];
                //
                using (HsfDBContext hsfDBContext = new HsfDBContext())
                {
                    var soundhostEntity= hsfDBContext.sound_host.Where(t => t.userid == userid && t.deviceid == deviceid).FirstOrDefault();
                    if(soundhostEntity.playstate==1 || soundhostEntity.playstate == null)
                    {
                        soundhostEntity.playstate = 0;
                    }
                    else
                    {
                        soundhostEntity.playstate = 1;
                    }
                    hsfDBContext.SaveChanges();

                }
                SoundPassiveRequest _SoundPassiveRequest = new SoundPassiveRequest()
                {
                    sessionId = Guid.NewGuid().ToString(),
                    deviceId = deviceid,
                    actionId = "8215",
                    blwakeup = "0"
                };
                DoRequestEvent(_SoundPassiveRequest);
            }
        }
        #endregion


        #region 音响升级
        /// <summary>
        /// 音响升级
        /// </summary>
        /// <param name="msg">user:wali_Server type:other 
        /// msg:wali_C40BCB80050A;8;8216;88888888;1$/r$</param>
        public static void SoundUpgrade(string msg)
        {
            if (msg.IndexOf(";") > 0)
            {
                string userid = msg.Split(';')[0];
                string deviceid = msg.Split(';')[3];
                SoundPassiveRequest _SoundPassiveRequest = new SoundPassiveRequest()
                {
                    sessionId = Guid.NewGuid().ToString(),
                    deviceId = deviceid,
                    actionId = "8216",
                    blwakeup = "0"
                };
                DoRequestEvent(_SoundPassiveRequest);
            }
        }
        #endregion

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

        #region 向智能主机发送音响语音内容
        /// <summary>
        /// 向智能主机发送音响语音内容
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static void SendMsg(string sendstr)
        {
            string user = "wali_Server";
            //string sessionId = sendstr.Split(';')[0].Split('_')[0];
            //string deviceId = sendstr.Split(';')[0].Split('_')[1];

            //if (!string.IsNullOrEmpty(body.deviceId))
            //{
            //    //读取配置文件中音响对应的主机
            //    user = ConfigAppSettings.GetValue(body.deviceId);//wali_Server
            //    log.Info($"音响{body.deviceId}对应的主机为：{user}");
            //}
            //else
            //{
            //    log.Info($"音响{body.deviceId}对应的主机不存在，请配置。");
            //}

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
                            sendstr = "connect user:" + user + " type:other msg:" + "12345678_Ying" + ";5;513;5omT5byA56qX5biY" + "$/r$" + "\r\n";
                            us.sesson.Send(sendstr);
                        }
                        catch { };
                        sok = true;
                    }
                }
            }
            else
            {
                foreach (var us in ls)
                {
                    if (us.user == user)
                    {
                        try
                        {
                            us.sesson.Send(sendstr);
                            log.Info($"给智能家居主机 {user} 发送问题：{sendstr}完毕");
                        }
                        catch { };
                        sok = true;
                        break;
                    }
                }
            }

            if (!sok)
            {
                log.Info($"智能家居主机{user} ： server noactive");
                Console.WriteLine($"智能家居主机{user} ： server noactive");
            }
        }
        #endregion

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

        #region 新的连接
        /// <summary>
        /// 新的连接
        /// </summary>
        /// <param name="session"></param>
        public static void server_NewSessionConnected(AppSession session)
        {
            session.Send("Even " + DateTime.Now.ToString());
            log.Info($"{DateTime.Now.ToString("yyyyMMdd-HHmmss:fff")} {session.RemoteEndPoint.ToString()} 登录到服务器");
        }
        #endregion
    }
}

/// <summary>
/// 智能家居主机服务器连接
/// </summary>
public class US
{
    public string user;
    public AppSession sesson;
    public string type;
    public string mac;
    public DateTime date;
}