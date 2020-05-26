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
using Hsf.Model;

namespace Hsf.Service
{
    public class SmartHomeNlpServer
    {
        #region 声明
        private static log4net.ILog log = log4net.LogManager.GetLogger("SmartHomeNlpServer");
        private static AppServer server = new AppServer();
        //智能家居主机服务器连接集合
        public static List<US> ls = new List<US>();
        #endregion

        public static List<US> GetUS()
        {
            return ls;
        }

        #region 启动
        public static void Start()
        {
            //server.Setup(

            server.SessionClosed += new SessionHandler<AppSession, CloseReason>(server_SessionClosed);
            server.NewRequestReceived += new RequestHandler<AppSession, StringRequestInfo>(server_NewRequestReceive);
            server.NewSessionConnected += new SessionHandler<AppSession>(server_NewSessionConnected);
            ServerConfig config = new ServerConfig();

            config.Port = 9002;
            config.MaxRequestLength = 8192;
            config.ClearIdleSession = true;//是否清除空闲会话，默认为false。
            config.ClearIdleSessionInterval = 120;//清除空闲会话的时间间隔，默认为120，单位为秒。
            config.MaxConnectionNumber = 10000;//最大允许的客户端连接数目，默认为100。
            //config.SendBufferSize = 10240;
            //config.IdleSessionTimeOut = 50000;
            //config.KeepAliveTime = 120;
            server.Setup(config);
            if (server.Start())
                log.Info($"智能家居Socket服务器开启{DateTime.Now.ToString()}");

            Console.WriteLine($"智能家居Socket服务器开启{DateTime.Now.ToString()}");
        }
        #endregion        

        #region 处理请求

        public static void server_NewRequestReceive(AppSession session, StringRequestInfo requestInfo)
        {
            try
            {
                //log.Info($"智能家居服务器收到新的消息<<<<<<：{session.RemoteEndPoint.ToString()} @ {requestInfo.Key} @ {requestInfo.Body}");
                if (requestInfo != null && requestInfo.Key != null && requestInfo.Key.ToLower() == "Heartbeat".ToLower())
                {
                    session.Send("NetHeart");
                    return;
                }

                if (!string.IsNullOrEmpty(requestInfo.Key) && !string.IsNullOrEmpty(requestInfo.Body))
                {
                    log.Info($"智能家居服务器收到新的消息<<<<<<： {requestInfo.Body}");
                }
                

                if (requestInfo.Key == "USERS")
                {
                    string users = "";
                    foreach (var item in ls)
                    {
                        users += item.user + ",";
                    }
                    session.Send($"{users}");
                }
                if (requestInfo.Key == "KICK" && !string.IsNullOrEmpty(requestInfo.Body))
                {
                    var us = ls.Where(t => t.user == requestInfo.Body).FirstOrDefault();
                    if (us != null)
                    {
                            log.Debug($"{us.user} 被踢下线");
                        ls.Remove(us);
                    }
                    session.Send("KICK OK");
                }
                if (requestInfo.Body.Split(' ').Length>=3 && requestInfo.Body.Contains("user:") && requestInfo.Body.Contains("type:") && requestInfo.Body.Contains("msg:"))
                {
                    string relayUser = requestInfo.Body.Split(' ')[0].Split(':')[1];//user:
                    string requestType = requestInfo.Body.Split(' ')[1].Split(':')[1];//type:
                    string requestMac = requestInfo.Body.Split(' ')[2].Split(':')[1];//msg:
                    if (requestType == "isonline")
                    {
                        ///校验当前服务器是否有同名的用户  
                        //登录用户和mac地址相同的：有同名的用户
                        var us = ls.Where(t => t.user == relayUser && t.mac.ToLower() == requestMac.ToLower()).FirstOrDefault();
                        if (us != null)
                        {
                                log.Debug(us.user + "@" + us.mac);
                            session.Send("online");//在线
                        }
                        else
                        {
                            session.Send("offline");//不存在
                        }
                    }

                    if (requestType == "home")
                    {
                        //校验当前服务器是否有同名的用户
                        //user:test_Server type:home msg:001519009611
                        //user: test_C40BCB80050A type:home msg:C40BCB80050A
                        var usList = ls.Where(t => t.user == relayUser); 
                        if (usList.Count()>0)
                        {
                            foreach (var us in usList)
                            {
                                if (us.mac.ToLower() == requestMac.ToLower())
                                {
                                    if (!us.sesson.Equals(session))
                                    {
                                        us.sesson.Close();
                                        us.sesson = session;//替换成新的session
                                    }
                                    session.Send("regist sucess");
                                    log.Debug($"{us.user}@ { relayUser} {us.mac.ToLower()}@ {requestMac.ToLower()}重复连接 \r\n");
                                }
                                else
                                {
                                    session.Send("repeat link");
                                    session.Close();
                                    log.Debug($"{us.user}@ { relayUser} {us.mac.ToLower()}@ {requestMac.ToLower()}异地登陆被强制下线 \r\n");
                                }
                            }
                        }
                        else
                        {
                            //缓存当前socket连接
                            ls.Add(new US() { sesson = session, user = relayUser, mac = requestMac });
                            session.Send("regist sucess");
                                log.Info($"{relayUser} regist sucess 登陆智能家居Socket服务器 在线人数{ ls.Count}");
                                Console.WriteLine($"{relayUser} 登陆智能家居Socket服务器 在线人数{ ls.Count}");
                        }                        
                    }
                    //转发消息
                    if (requestType == "other")
                    {
                        string msg = requestInfo.Body.Split(':')[3]; // requestInfo.Body.Split(' ')[2].Substring(4, requestInfo.Body.Split(' ')[2].Length - 4)
                        
                        //主机发送过来方向
                        if ((msg.Contains("513;") || msg.Contains("517;") || msg.Contains("515;")) && msg.Contains("Zip"))
                        {
                            //异步接收到主机返回结果，再传到前台响应音响：sessionId_deviceId;513;5omT5byA56qX5biY;Zip;H4sIAAAAAAAAAAEfAOD/6K6+5aSH5peg5rOV6K+G5YirQOaJk+W8gOeql+W4mCDHP38fAAAA$/r$
                            SmartHomeNlp.SmartHomeNlpReceiveMsg(msg);
                        }
                        else if (msg.Contains("8212;") && !msg.Contains("Zip"))
                        {
                            //注册音响:user:test_Server type:other msg:test_C40BCB80050A;8;8212;ALL;H4sIAAAAAAAAAG2QvU7DMBDH38VzBjvfytZ2YoCBFaHIci6ppcSObIcKVZVY2NkYeQJAqCuvE6G+BZcPhYLY/Lv76e7vu9mTK9hdg9CmIJkzHXgkl/ikHhFbqbjiDZCMnF6OX28fBIs1t7YcDJIiFnDHa24axInElisF9cJSwCgznyU+Y0kYR1FA5147NoKJGi4QuUiDMmApjcNyqrfccJLtiW5BodB/PvRP7x7O8pj3xxa1tkPa/vF4en79zzlMI7VxqDmwbt6hrXRSq5+ggZ/ELJq6BiqhCzgPax13sHxyJPYb/QXdfTuoq4v8ElRV6A7LsuHVdBgUGB0W1ZUdY+HlW73Ds3bzgs6CGdUhb74J6XqzTimN6Iocbr8BfQIiVMEBAAA=$/r$
                            //8212的第二条带Zip的不是真实的注册指令
                            string binding= SmartHomeNlp.RegSoundBinding(msg);
                            session.Send(binding);
                            log.Info(binding);
                        }
                        else if (msg.Contains("8215;"))
                        {
                            //是否自身播放音乐状态反转：user:wali_Server type:other msg:wali_C40BCB80050A;8;8215;88888888;1$/r$
                            string reversal = SmartHomeNlp.SoundReversal(msg);
                            session.Send(reversal);
                            log.Info(reversal);
                        }
                        else if (msg.Contains("8216;"))
                        {
                            //音箱自动升级的指令：user:wali_Server type:other msg:wali_C40BCB80050A;8;8216;88888888;1$/r$
                            string upgrade= SmartHomeNlp.SoundUpgrade(msg);
                            session.Send(upgrade);
                            log.Info(upgrade);
                        }
                        else if (msg.Contains("2011;"))
                        {
                            //音箱提醒：user:wali_Server type:other msg:wali_C40BCB80050A;3;2011;slfjdsfj;64(中文);1$/r$
                            string remind = SmartHomeNlp.SoundRemind(msg);
                            session.Send(remind);
                            log.Info(remind);
                        }else if (msg.Contains("3;301;"))
                        {
                            log.Info($"301请求： {msg}");
                            string remind = SmartHomeNlp.SoundMsg(msg);
                           // session.Send(remind);
                            log.Info(remind);
                        }



                        #region lot部分，old主机是需要转发两次，new主机替换old主机，我就是主机无需转发，直接从数据库拿结果，直接返回app
                        //if (msg.Contains("1;111"))
                        //{
                        //    //DAJCHSF_2047DABEF936 type:home msg:2047DABEF936  手机app的也已经存到list缓存之中了
                        //    //1.请求主机登录信息
                        //    //user:DAJCHSF_Server type:other msg:DAJCHSF_2047DABEF936;1;111;all;admin,admin,shouquanma,DAJCHSF_Server,2047DABEF936,192.168.1.101$/r$
                        //    //2.主机返回登录结果  错误返回值error,  成功返回值：一串密钥字符串1@@@@88sdf888823xv8888
                        //    //user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;111;all;Zip;H4sIAAAAAAAAADN0AAILi+KUNAsgMDKuKAPRAHas4VgWAAAA$/r$//

                        //    //启动多线程
                        //    //Task.Run(() =>
                        //    //{
                        //    //发送收到的语音到NLP管理器
                        //    //账号登录：user:wali_Server type:other msg:wali_C40BCB80050A;1;111;all;用户名称,用户密码,授权码, 家的名称,登陆手机mac,登陆ip地址
                        //    //relayUser = msg.Split(';')[0];
                        //    //string msg111 = SmartHomeHost.Host111(msg);  user:{appUser} type:other msg:
                        //    relayUser = msg.Split(';')[0];//直接返回请求的app
                        //    msg = $"{relayUser};111;all;Zip;H4sIAAAAAAAAADN0AAILi+KUNAsgMDKuKAPRAHas4VgWAAAA$/r$\r\n";//后面的发给前面的，与请求的对调一下
                        //    log.Info(msg);
                        //    //});
                        //}

                        //else if (msg.Contains("8;835"))
                        //{
                        //    //1.请求主机房间列表
                        //    //user:DAJCHSF_Server type:other msg:DAJCHSF_2047DABEF936;8;835;admin$/r$
                        //    //2.主机返回房间列表
                        //    //user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;835;admin;Zip;H4sIAAAAAAAAAIuuViotTi3KTFGyUlLSUSouSSxJBTJLikpTgdzkjMy8xLzEXJDQs46Jz2e1PF23DSiemZyfB9GQmZuYngrTXZBfDGYaQNgFiUWJSlbVSimpZSX5JYk5QBlDS5AliWmpxaklJZl56TCrasEaSioLUqHa40EGGego+aWWB6Um5xcBeSCFtTrY3Yvm1qfrFj3ta8Xh0KL8/FxDJNcaGhgbmpoYG1iam5iiOJzajupd/nTdEtIcBcRmBjR1VNe8p61rSHaXkampBW0Da13nyxmbSHOUsYmBkTl5jooFAHQFerEIAwAA$/r$

                        //    msg = SmartHomeHost.Host835(msg);
                        //    //获取房间列表,8;835;+loginname 
                        //    //relayUser = msg.Split(';')[0];
                        //    //msg = $"{appUser};835;admin;Zip;H4sIAAAAAAAEAI2OsQ6CMBRF/+XNHWhJMbIZmR1cjSENPJFEKGmLxBA2B1dXP8LRP1J/QwqS4EDidt67N7lnU8MKqzVGUsXgG1UigTBt0SEQ7dNc5CJD8OF1ub5v5+f9AQTSSObty1ImErRtexRSd+j0XAglwK8hxqORRhy+iRY71GhMmiftxw5C0/XNqcChY4TBISVQalT9SEP+1K2qakJUSZnRkS1ljFPOHcZd79d8UmusRJkbBstgTj3u8NkigGb7AQZ+Ko1TAQAA$/r$\r\n";
                        //    //msg = $"{relayUser};835;admin;Zip;H4sIAAAAAAAEAIuuVkrOyMxLzEvMTVWyUnrWMfH5rJan67Yp6ShlJufnAYVArNzE9NTMFAinIL8YzDSAsEsqC1KhvOKSxBIQu6SoNBXILS1OLYJoqtVBtaW8vByH+UX5+bmGSJYYGhmZGpqaGhiZGpvhthDZMkMj43gXZxdLQzNTA1NzRxel2lgAbVUR+eQAAAA=$/r$\r\n";
                        //    log.Info(msg);
                        //}
                        //else if (msg.Contains("8351;") && !msg.Contains("Zip"))
                        //{
                        //    //获取房间列表，多余
                        //    //string appUser = msg.Split(';')[0];
                        //    //msg = $"{appUser};8351;admin;Zip;H4sIAAAAAAAEAI2OsQ6CMBRF/+XNHWhJMbIZmR1cjSENPJFEKGmLxBA2B1dXP8LRP1J/QwqS4EDidt67N7lnU8MKqzVGUsXgG1UigTBt0SEQ7dNc5CJD8OF1ub5v5+f9AQTSSObty1ImErRtexRSd+j0XAglwK8hxqORRhy+iRY71GhMmiftxw5C0/XNqcChY4TBISVQalT9SEP+1K2qakJUSZnRkS1ljFPOHcZd79d8UmusRJkbBstgTj3u8NkigGb7AQZ+Ko1TAQAA$/r$\r\n";
                        //    //log.Info(msg);
                        //}
                        //else if (msg.Contains("836;") && !msg.Contains("Zip"))
                        //{
                        //    //新增房间 8;836;+Base64(zip(Position对象jhson串))，无返回
                        //    SmartHomeHost.Host836(msg);
                        //}
                        //else if (msg.Contains("837;") && !msg.Contains("Zip"))
                        //{
                        //    //删除房间8;837;+posid
                        //    log.Info($"删除房间" + msg);
                        //}
                        //else if (msg.Contains("8;815;"))
                        //{
                        //    //获得当前房间的设备列表的命令8;815;+ posid
                        //    //1.请求主机房间列表
                        //    //user:DAJCHSF_Server type:other msg:DAJCHSF_2047DABEF936;8;815;103154315460$/r$
                        //    //2.主机返回结果
                        //    //user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;815;103154315460;Zip;H4sIAAAAAAAAAN2Vz0vDMBTH/5eehyRp0607Kbt70Jsi4y1Lu0CbljRzjDEQxL9BBc8iXr2IP/CvqfPPMGm7edphYwVZQ1O+3/deXpPPIeczZ5xzJYZO13FaTpZOLiEec6OKh6fF25fx2EhIkJBY8/v+7ef6s/i4Km5ekI3FkOfhsnrITTGoZKXYCKTksdEIhSFhdvjghwjRwKdVjmC8rMfIxW2XEup2KKpDmfVJ+wCZgQ8xruwEmF0RI/Ng4g3M7DEauDio4hkocLqzeSVSpe0qdSjNhRaprPtRz75+3S7XoO0mtTIH8Ofg1XZKSVZSTzOb3oNMxGbb0yOheqkclg2EjEySSCDiy9OJo7z8F9Ry+tYz32M+OeEsVUbZpvPW9jRwYzR8vHMaZN9pkMZotMnOabj7TsNtjoa7cxreBjRCiPN/h2Px+L4Ox+L5tni9a+TiCGjgBXjTiwOFHRwGjK7H0WkQx2kCSvfPRDTgvDdWGoTcgsTFL5qU1AfEBwAA$/r$

                        //    msg = SmartHomeHost.Host815(msg);
                        //    //msg = "123_DCD9165057AD;815;0108155252403;Zip;H4sIAAAAAAAEAN2VzUrDQBDH3yXnIvuRTZOelN496E2RMt1u0oVkEzappZSCID6DCp5FvHoRP/BpYn0Md5O2nnpoaUCahYT/f2Z2Mvs77PnUGeVCy4HTcZyWk6XjS4hHwqjy4Wn+9mU8PpQKFCTW/L5/+7n+LD+uypsXZGMx5Hm4rB4IUww6WSk+BKVEbDRCYUi4XR54IUIs8FidI7mo6jGiuE0ZYdRnaBHKrE/aB8gsfIhxbSfA7Y4YmQcTt2/eLmcBxUEdz0CD05nOapHqwu6yCKW5LGSq6nofM9OPuIjW0byAwk5ZaHMCfw5ezVNJspLFJLPpXchkbOaeHEndTdWg6iBVZJJkApFYHk8c5dXPoJbTs575HovxieCpNso2nbW2x4Ebw+HhneMge4+DNIajTXaOg+49DtocDrpzHO4mOEKI83/HY/74vo7H/Pm2fL1r5PIIWOAGeNPLA4U+DgPO1vPwm+RxmoAuemcy6gvRHekCpNoCxcUvMbh28skHAAA=$/r$";
                        //}
                        //else if (msg.Contains("8211;") && !msg.Contains("Zip"))
                        //{

                        //    //1.告诉主机添加设备
                        //    //user:DAJCHSF_Server type:other msg:DAJCHSF_2047DABEF936;8;8211;ALL;H4sIAAAAAAAAAFVRTUsDMRD9LznvIdkvu71696A3Rco0nd0GdjdLkrVIKQji3Vv1LwjiwYuI/pz68S862SwVE0Ly5r3MvEku1uwEV6cotVmwqTM9Rmym6MgjJpeqhRYaZFP29fj+c/v5/bTdvT1wRlwN1pZeyCYEF3gFNZiGYEByCW2LNWHOyzKWfuaQl5xnRZ4FjZI4JOCCJ+IoKbIiLQQfuY6ITKQBNSCDjoaI0zlt5USUhRwzdWCATdfkSlvvdnf3+rt9jtIkiUTk/ekOWx//uNndvxzim3BZG/fXRqetckp7tbeVpX7loyuDldQLX0HESQhZBw4PbQ9I/IfxAbrrzkvPGjBudq6qOeJxbxyolnjVQBXeg5SC+87qyg7e6C86vaIX7sdKvUUzSNnmcg8kEEBHwgEAAA==$/r$
                        //    //2.主机返回app添加成功
                        //    //user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;8211;ALL;Zip;H4sIAAAAAAAAAHNMScnPBgD0Si5gBQAAAA==$/r$

                        //    //新增安防设备8;8211;All;+Base64(zip(设备对象jhson串))
                        //    msg = SmartHomeHost.Host8211(msg);
                        //}
                        //else if (msg.Contains("8211;") && !msg.Contains("Zip"))
                        //{

                        //    //1.告诉主机删除设备
                        //    //user: 123_Server type:other msg:123_DCD9165057AD; 8; 822; 01120818544930$/ r$
                        //    //2.主机返回删除成功delok@105 124612 6590
                        //    //user:JQQ_2C5731D4367C type:other msg:JQQ_2C5731D4367C;822;1051246126590;Zip;H4sIAAAAAAAAAEtJzcnPdjA0MDU0MjEzNDIztTQAALsdu8YTAAAA$/r$
                        //    msg = SmartHomeHost.Host822(msg);
                        //}
                        //else if (msg.Contains("8135;") && !msg.Contains("Zip"))
                        //{
                        //    //1.收到打开指令
                        //    //user:123_Server type:other msg:123_DCD9165057AD;8;8135;1041657481380;2;8$/r$
                        //    //2.返回结果openok@1041657481380
                        //    //user:123_DCD9165057AD type:other msg:123_DCD9165057AD;8135;1041657481380;Zip;H4sIAAAAAAAAAMsvSM3Lz3YwNDAxNDM1N7EwNLYwAAD0EGgCFAAAAA==$/r$
                        //    msg = SmartHomeHost.Host8135(msg);
                        //}
                        //else if (msg.Contains("8145;") && !msg.Contains("Zip"))
                        //{
                        //    //1.收到关闭指令
                        //    //user:123_Server type:other msg:123_DCD9165057AD;8;8145;808181248576;3,0$/r$
                        //    //2.返回结果closeok@808181248576
                        //    //user:123_DCD9165057AD type:other msg:123_DCD9165057AD;8145;808181248576;Zip;H4sIAAAAAAAAAEvOyS9Ozc92sDCwMLQwNDKxMDU3AwCjJ+18FAAAAA==$/r$
                        //    msg = SmartHomeHost.Host8145(msg);
                        //}
                        #endregion

                        //转发
                        relay(relayUser, msg);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        /// <summary>
        /// 转发
        /// </summary>
        /// <param name="user"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static void relay(string user, string msg)
        {
            bool sok = false;
            //群发广播
            if (user.Contains("%"))
            {
                user = user.Replace("%", "");
                var userServer= ls.Where(t => t.user.Contains(user));
                foreach (var item in userServer)
                {
                    try
                    {
                            log.Debug("群发广播转发数据 " + item.user + "数据 " + msg);
                        item.sesson.Send(msg);
                        sok = true;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            else
            {
                var us = ls.Where(t => t.user== user).FirstOrDefault();
                if (us!=null)
                {
                    try
                    {
                        //转发到主机
                        log.Debug("转发数据 " + us.user + "数据>>>>>>： " + msg);
                        us.sesson.Send(msg);
                        sok = true;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else
                {
                    //_server不存在，则用后面的mag的user，转发到手机app，返回主机得到的结果
                    string appUser = msg.Split(';')[0];
                    var us2 = ls.Where(t => t.user == appUser).FirstOrDefault();
                    if (us2 != null)
                    {
                        try
                        {
                            log.Debug("转发app数据 " + appUser + "数据 " + msg);
                            us2.sesson.Send(msg);
                            sok = true;
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
            }

            if (!sok)
            {
                var us = ls.Where(t => t.user == msg.Split(';')[0]).FirstOrDefault();
                if (us!=null)
                {
                    us.sesson.Send("server noactive");
                    log.Debug("转发数据失败 " + user + "数据 " + msg+ "server noactive");
                }
            }
        }
        

        #region 向智能主机发送音响语音内容
        /// <summary>
        /// 向智能主机发送音响语音内容
        /// </summary>
        /// <param name="user">指定的主机</param>
        /// <param name="sendstr">发送的指令</param> 
        /// <returns></returns>
        public static void SendMsg(string user,string sendstr)
        {
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(sendstr))
            {
                var us = ls.Where(t => t.user == user).FirstOrDefault();
                if (us != null)
                {
                    try
                    {
                        us.sesson.Send(sendstr);
                        log.Debug($"给智能家居主机 {user} 发送问题>>>>>>： {sendstr.Replace("\r\n", "")}");
                    }
                    catch { };
                }
                else
                {
                    log.Error($"智能家居主机{user} ： server noactive");
                }
            }
            else
            {
                log.Error($"SendMsg参数为空 ");
            }
        }
        #endregion
        

        #region 新的连接
        /// <summary>
        /// 新的连接
        /// </summary>
        /// <param name="session"></param>
        public static void server_NewSessionConnected(AppSession session)
        {
            session.Send("Even " + DateTime.Now.ToString());
            //session.Send("{\"code\" :1007,\"serial\": 11111}");
            log.Info($"{session.RemoteEndPoint.ToString()} 登录到服务器");
        }
        #endregion

        #region 关闭
        public static void server_SessionClosed(AppSession session, SuperSocket.SocketBase.CloseReason value)
        {            
            try
            {                
                var us = ls.Where(t => t.sesson == session).FirstOrDefault();
                if (us!=null)
                {
                    log.Debug($"{us.user} 离开智能家居Socket服务器");
                    Console.WriteLine($"{us.user} 离开智能家居Socket服务器");
                    ls.Remove(us);
                }
            }
            catch (Exception e)
            {
                log.Error($"{e.Message} 离开智能家居Socket服务器");
            }
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
    public string mac;
    public AppSession sesson;
}