using Hsf.Model;
using Hsf.Framework;
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
using Hsf.DAL;
using Hsf.Redis.Service;

namespace Hsf.Service
{
    /// <summary>
    /// 智能家居Nlp
    /// </summary>
    public class SmartHomeNlp : INlp
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("SmartHomeNlp");

        //智能主机返回的语义
        public static Action<string, NlpAnswers> _NlpControlerReceiveMsg;
        //主动请求音响指令
        public static Func<SoundBodyResult, bool> _NlpControlerRequestMsg;


        /// <summary>
        /// 向智能主机发送音响语音内容，异步返回答案为null
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public NlpAnswers SendMsg(SoundBodyRequest body)
        {
            log.Debug($"SmartHomeNlp接收到问题 ：{body.questions}");
            try
            {
                string hostid = "";
                using (RedisHashService service = new RedisHashService())
                {
                    //hostid = service.Get(body.deviceId);//获取缓存中与音响绑定的主机
                    hostid = service.GetValueFromHash("Sound_Host", body.deviceId);
                    //缓存中不存在再查数据库
                    if (!string.IsNullOrEmpty(hostid))
                    {
                        //hostid = hostid.Replace("\"", "");
                        SendStr(body, hostid);
                    }
                    else
                    {
                        //根据设备id获取主机ID
                        using (HsfDBContext hsfDBContext = new HsfDBContext())
                        {
                            //根据音响devmac找对应的主机userid，向主机发送消息
                            var soundhostEntity = hsfDBContext.sound_host.Where(t => t.devmac == body.deviceId && t.deletemark == 0).FirstOrDefault();
                            if (soundhostEntity != null)
                            {
                                if (!string.IsNullOrEmpty(soundhostEntity.userid))
                                {
                                    hostid = soundhostEntity.userid;
                                    //service.Set<string>(body.deviceId, hostid);//缓存主机与音响的绑定关系
                                    service.SetEntryInHash("Sound_Host", body.deviceId, hostid);//缓存主机与音响的绑定关系,重复绑定覆盖
                                    SendStr(body, hostid);
                                }
                                else
                                {
                                    log.Info($"音响{body.deviceId}，对应的主机为空字符");
                                    return null;
                                }
                            }
                            else
                            {
                                log.Info($"未到找音响{body.deviceId}，对应的主机");
                                return null;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                log.Info($"SmartHomeNlp SendMsg 异常：{ex.Message}");
            };
            return null;
        }
        /// <summary>
        /// 发送消息str
        /// </summary>
        /// <param name="body"></param>
        /// <param name="hostid"></param>
        public static void SendStr(SoundBodyRequest body,string hostid)
        {
            string sendstr = "";
            ///这里要判断，数据类型了
            if (body.talkstate== "train")
            {
                //训练模式不发送开关指令，执行5135指令
            }
            else
            {
                if (body.sourceId == "mengdou")
                {
                    //得到发送给智能家居的命令
                    sendstr = GetMengDouSendStr(body.sessionId, body.deviceId, body.req.ToString(), body.questions);
                }
                else
                {

                    sendstr = body.sessionId + "_" + body.deviceId + ";5;513;" + EncryptionHelp.Encryption(body.questions, false) + "$/r$" + "\r\n";
                }
                //向主机发送数据
                SmartHomeNlpServer.SendMsg(hostid, sendstr);
            }

        }
        /// <summary>
        /// 拼装得到发送给智能家居的命令
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="deviceId"></param>
        /// <param name="req"></param>
        /// <param name="questions"></param>
        /// <returns></returns>
        public static string GetMengDouSendStr(string sessionId, string deviceId, string req, string questions)
        {
            string sendstr = "";
            if (req.Contains("service\":\"musicX"))
            {
                sendstr = sessionId + "_" + deviceId + ";5;517;Zip;" + EncryptionHelp.Encryption("music@" + req, true) + "$/r$" + "\r\n";
            }
            else if (req.Contains("service\":\"news"))
            {
                sendstr = sessionId + "_" + deviceId + ";5;517;Zip;" + EncryptionHelp.Encryption("news@" + req, true) + "$/r$" + "\r\n";
            }
            else if (req.Contains("service\":\"story"))
            {
                sendstr = sessionId + "_" + deviceId + ";5;517;Zip;" + EncryptionHelp.Encryption("story@" + req, true) + "$/r$" + "\r\n";
            }
            else if (req.Contains("service\":\"joke"))
            {
                sendstr = sessionId + "_" + deviceId + ";5;517;Zip;" + EncryptionHelp.Encryption("joke@" + req, true) + "$/r$" + "\r\n";
            }
            else if (req.Contains("service\": \"weather"))
            {
                try
                {
                    dynamic result = JsonConvert.DeserializeObject<dynamic>(req);//反序列化
                    string answer_text = result.nameValuePairs.answer.nameValuePairs["text"].Value;
                    //string airQuality = result.nameValuePairs.data.nameValuePairs.result.values[0].nameValuePairs["airQuality"].Value;//；空气质量：{airQuality}
                    string pm25 = result.nameValuePairs.data.nameValuePairs.result.values[0].nameValuePairs["pm25"].Value;
                    req = $"info@{questions}@{answer_text}；PM2.5：{pm25}";
                    sendstr = sessionId + "_" + deviceId + ";5;515;" + EncryptionHelp.Encryption(req, false) + "$/r$" + "\r\n";
                }
                catch (Exception ex)
                {
                    log.Error("天气json处理错误：" + ex.Message);
                    return sessionId + "_" + deviceId + ";5;515;" + EncryptionHelp.Encryption("info@" + questions + "@天气json处理错误：" + ex.Message, false) + "$/r$" + "\r\n";
                }
            }
            else
            {
                sendstr = sessionId + "_" + deviceId + ";5;513;" + EncryptionHelp.Encryption(questions, false) + "$/r$" + "\r\n";
            }
            return sendstr;
        }
        

        /// <summary>
        /// 异步返回智能家居数据     
        /// user:16080_ac83f318064f type:other msg:16080_ac83f318064f;513;6Zi/5aeo5L+u6YeM6Z2i;Zip;H4sIAAAAAAAAAAEiAN3/6K6+5aSH5peg5rOV6K+G5YirQOmYv+WnqOS/rumHjOmdokgOnEMiAAAA$/r$
        /// </summary>
        /// <param name="msg">收到的响应结果</param>
        public static void SmartHomeNlpReceiveMsg(string msg)
        {
            try
            {
                string sessionId = msg.Split(';')[0].Split('_')[0];//根据返回字符串得到sessionId
                string msg64j = EncryptionHelp.Decrypt(msg.Split(';')[4].Replace("$/r$", ""), true);//解密"设备无法识别@打开窗帘"
                                                                                                    //拼装返回答案
                NlpAnswers _NlpAnswers = new NlpAnswers()
                {
                    Code = "SmartHomeNlp",
                    Name = "家居",
                    Answers = msg64j,
                    Level = 1
                };
                //向中枢控制器指定的sessionId发送返回的语义结果
                _NlpControlerReceiveMsg(sessionId, _NlpAnswers);
            }
            catch (Exception)
            {

                throw;
            }

        }


        /// <summary>
        /// 向智能主机执行回调方法
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public string CallbackMsg(SingleAnswers _SingleAnswers)
        {
            log.Info("智能家居CallbackMsg回调:::::::" + _SingleAnswers.NlpAnswers);
            //Console.WriteLine("智能家居CallbackMsg回调:::::::" + _SingleAnswers.NlpAnswers);
            return "sendok";

        }


        #region 注册音响
        /// <summary>
        /// 注册音响
        /// </summary>
        /// <param name="msg">user:wali_Server type:other 
        /// msg:wali_Server;8;8212;ALL;H4sIAAAAAAAAAG2RsU7DMBCG38VzVOUcN6TZGBlggBGh6ORcUkuJHdlJI1RVYmFn4ykQAyuv04G3wIlLRBHy4u/+/3y/zvd7dkPjLUljS5b3dqCIFcpf44jJrdKosSWWM5RZUiUgCKsL5qUGnasmH8s8lrTDBm3rMZDcotbULKwkzWbgsAbBIRYiSeOT1s1CEqhFOeGGryDNVlm2gpgHpUOLLN/72cZNkY7PH1+vbxEHiPz522A60pPp8+n48v6/6RCeNbZfcnbGqV4ZHaICCJHGsPlJaqmWpqTfcV2PPS3tM8E58gX7x26yXl4V16Tr0gy+rFqsw2q8AeK1LzW1myP5H+jM6Bc7nAYMjuxsHbFRxR3ZHVl2ePgGQ/XuYsMBAAA=$/r$</param>
        public static string RegSoundBinding(string msg)
        {
            string msg64j = EncryptionHelp.Decrypt(msg.Split(';')[4].Replace("$/r$", ""), true);//解密"设备无法识别@打开窗帘"
            msg64j = msg64j.Replace("[", "").Replace("]", "");
            SoundHost _SoundHost = JsonConvert.DeserializeObject<SoundHost>(msg64j);//反序列化
            if (!string.IsNullOrEmpty(_SoundHost.userid))
            {
                using (HsfDBContext hsfDBContext = new HsfDBContext())
                {
                    _SoundHost.userid = GetNewUserId_Server(_SoundHost.userid);
                    //根据音响的设备id：devmac去判断，有的话软删除，没有的话注册新增
                    var soundhostEntity = hsfDBContext.sound_host.Where(t => t.devmac == _SoundHost.devmac && t.deletemark == 0).FirstOrDefault();
                    if (soundhostEntity != null)
                    {
                        soundhostEntity.deletemark = 1;//软删除old记录
                        soundhostEntity.modifiytime = DateTime.Now;

                        BindNewHost(hsfDBContext, _SoundHost);
                        return $"Binding OK Again";
                    }
                    else
                    {
                        BindNewHost(hsfDBContext, _SoundHost);
                        return $"Binding OK";
                    }
                }
            }
            else
            {
                return $"Binding Fail";
            }
        }
        /// <summary>
        /// 绑定新的主机
        /// </summary>
        /// <param name="hsfDBContext"></param>
        /// <param name="_SoundHost"></param>
        public static void BindNewHost(HsfDBContext hsfDBContext,SoundHost _SoundHost)
        {            
            sound_host sound_Host = GetNewSoundHost(_SoundHost);
            hsfDBContext.sound_host.Add(sound_Host);
            hsfDBContext.SaveChanges();
            using (RedisHashService service = new RedisHashService())
            {
                //service.Set<string>(_SoundHost.devmac, _SoundHost.userid);//缓存主机与音响的绑定关系
                service.SetEntryInHash("Sound_Host", _SoundHost.devmac, _SoundHost.userid);//缓存主机与音响的绑定关系,重复绑定覆盖
            }
        }
        /// <summary>
        /// 判断主机名后缀是否是_Server
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public static string GetNewUserId_Server(string userid)
        {
            if (!userid.Contains("_Server"))
            {
                userid = userid.Split('_')[0] + "_Server";
            }
            return userid;
        }
        /// <summary>
        /// 添加音响
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="_SoundHost"></param>
        /// <returns></returns>
        public static sound_host GetNewSoundHost(SoundHost _SoundHost)
        {
            sound_host sound_Host = new sound_host()
            {
                id = Guid.NewGuid().ToString(),
                chinaname = _SoundHost.chinaname,
                classfid = _SoundHost.classfid,
                deviceid = _SoundHost.deviceid,
                devip = _SoundHost.devip,
                devmac = _SoundHost.devmac,//音响设备id，手填
                devport = _SoundHost.devport,
                devposition = _SoundHost.devposition,
                devregcode = _SoundHost.devregcode,
                devtype = _SoundHost.devtype,
                imageid = _SoundHost.imageid,
                lgsort = _SoundHost.lgsort,
                userid = _SoundHost.userid,//wali_Server主机名称
                playstate = 1,//是否自身播放音乐状态反转,0：否，1：是
                deletemark = 0,//删除标记
                createtime = DateTime.Now
            };
            return sound_Host;
        }
        #endregion


        #region 更改是否自身播放音乐状态反转
        /// <summary>
        /// 更改是否自身播放音乐状态反转
        /// </summary>
        /// <param name="msg">user:wali_Server type:other 
        /// msg:wali_C40BCB80050A;8;8215;88888888;1$/r$</param>
        public static string SoundReversal(string msg)
        {
            string userid = msg.Split(';')[0];
            userid = GetNewUserId_Server(userid);//主机_Server
            string devmac = msg.Split(';')[3];

            SoundBodyResult _SoundPassiveRequest = new SoundBodyResult()
            {
                sessionId = Guid.NewGuid().ToString(),
                deviceId = devmac,
                actionId = "8215",
                blwakeup = "0"
            };
            //如果发送到音响失败，则不修改数据库
            if (_NlpControlerRequestMsg(_SoundPassiveRequest))
            {
                using (HsfDBContext hsfDBContext = new HsfDBContext())
                {
                    //获取主机和设备，删除标记为0，未删除的
                    var soundhostEntity = hsfDBContext.sound_host.Where(t => t.userid == userid && t.devmac == devmac && t.deletemark == 0).FirstOrDefault();
                    if (soundhostEntity != null)
                    {
                        //反转状态
                        if (soundhostEntity.playstate == 1 || soundhostEntity.playstate == null)
                        {
                            soundhostEntity.playstate = 0;
                        }
                        else
                        {
                            soundhostEntity.playstate = 1;
                        }
                        soundhostEntity.modifiytime = DateTime.Now;
                        hsfDBContext.SaveChanges();
                        return $"Reversal OK";
                    }
                    else
                    {
                        return $"Reversal Fail Database";
                    }
                }
            }
            else
            {
                return $"Reversal Fail";
            }
        }
        #endregion


        #region 音响升级
        /// <summary>
        /// 音响升级
        /// </summary>
        /// <param name="msg">user:wali_Server type:other 
        /// msg:wali_C40BCB80050A;8;8216;88888888;1$/r$</param>
        public static string SoundUpgrade(string msg)
        {
            string devmac = msg.Split(';')[3];//给指定的音响发送消息
            SoundBodyResult _SoundPassiveRequest = new SoundBodyResult()
            {
                sessionId = Guid.NewGuid().ToString(),
                deviceId = devmac,
                actionId = "8216",
                blwakeup = "0"
            };
            if (_NlpControlerRequestMsg(_SoundPassiveRequest))
            {
                return $"Upgrade OK";
            }
            else
            {
                return $"Upgrade Fail";
            }
        }
        #endregion

        #region 提醒信息
        /// <summary>
        /// 播放url内容，播放完成后自动唤醒
        /// </summary>
        /// <param name="msg">user:wali_Server type:other 
        /// msg:wali_C40BCB80050A;3;2011;ac83f318064f(设备id);64(中文)1$/r$</param>
        public static string SoundRemind(string msg)
        {
            string devmac = msg.Split(';')[3];//给指定的音响发送消息
            string msg64j = EncryptionHelp.Decrypt(msg.Split(';')[4].Replace("$/r$", ""), false);
            //SoundPassiveRequest _SoundPassiveRequest = new SoundPassiveRequest()
            //{
            //    sessionId = Guid.NewGuid().ToString(),
            //    deviceId = devmac,
            //    actionId = "2011",
            //    req= msg64j,
            //    url= BaiduSDK.Tts(msg64j),
            //    blwakeup = "0"
            //};
            SoundBodyResult _SoundBodyRequest = new SoundBodyResult()
            {
                sessionId = Guid.NewGuid().ToString(),
                deviceId = devmac,
                questions="提醒",
                actionId = "2011",//与2011相似
                req = msg64j,
                url = BaiduSDK.Tts(msg64j),
                blwakeup = "0"
            };
            if (_NlpControlerRequestMsg(_SoundBodyRequest))
            {
                return $"Remind OK";
            }
            else
            {
                return $"Remind Fail";
            }
        }
        #endregion

        #region 提醒信息
        /// <summary>
        /// 播放url内容，播放完成后自动唤醒
        /// </summary>
        /// <param name="msg">user:wali_Server type:other 
        /// msg:wali_C40BCB80050A;3;3011;ac83f318064f(设备id);64(中文)1$/r$</param>
        public static string SoundMsg(string msg)
        {
            string devmac = msg.Split(';')[3];//给指定的音响发送消息
            string msg64j = EncryptionHelp.Decrypt(msg.Split(';')[4].Replace("$/r$", ""), false);
            //SoundPassiveRequest _SoundPassiveRequest = new SoundPassiveRequest()
            //{
            //    sessionId = Guid.NewGuid().ToString(),
            //    deviceId = devmac,
            //    actionId = "2011",
            //    req= msg64j,
            //    url= BaiduSDK.Tts(msg64j),
            //    blwakeup = "0"
            //};
            SoundBodyResult _SoundBodyRequest = new SoundBodyResult()
            {
                sessionId = Guid.NewGuid().ToString(),
                deviceId = devmac,
                questions = "提醒",
                actionId = "301",//与2011相似
                req = msg64j,
                url = BaiduSDK.Tts(msg64j),
                blwakeup = "0"
            };
            if (_NlpControlerRequestMsg(_SoundBodyRequest))
            {
                return $"Remind OK";
            }
            else
            {
                return $"Remind Fail";
            }
        }
        #endregion

    }
}
