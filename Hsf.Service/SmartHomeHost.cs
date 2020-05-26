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
using Hsf.Entity;
using Hsf.Redis.Service;

namespace Hsf.Service
{
    /// <summary>
    /// 智能家居主机
    /// </summary>
    public class SmartHomeHost
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("SmartHomeHost");


        #region 111更改是否自身播放音乐状态反转
        /// <summary>
        /// 更改是否自身播放音乐状态反转
        /// </summary>
        /// <param name="msg">user:zys_Server type:other msg:
        /// zys_DCD9165057AD;1;111;all;admin,admin,shouquanma,zys_Server,DCD9165057AD,192.168.88.101$/r$</param>
        /// ssxy_C40BCB80050A;111;all;Zip;H4sIAAAAAAAAADN0AAILi+KUNAsgMDKuKAPRAHas4VgWAAAA$/r$
        public static string Host111(string msg)
        {
            try
            {
                string account_mac = msg.Split(';')[0];
                if (account_mac.Contains("_"))
                {
                    string account = account_mac.Split('_')[0];
                    string mac = account_mac.Split('_')[1];

                    //如果发送到音响失败，则不修改数据库
                    using (HsfDBContext hsfDBContext = new HsfDBContext())
                    {
                        //获取主机和设备，删除标记为0，未删除的
                        var accountEntity = hsfDBContext.host_account.Where(t => t.Account == account && t.DeleteMark == 0).FirstOrDefault();
                        if (accountEntity != null)
                        {
                            return $"H4sIAAAAAAAAADN0AAILi+KUNAsgMDKuKAPRAHas4VgWAAAA";//user:{account_mac} type:other msg:{account_mac};111;all;Zip;  $/r$
                        }
                        else
                        {
                            return $"H4sIAAAAAAAAAEstKsovAgBxvN1dBQAAAA==";//user:{account_mac} type:other msg:{account_mac};111;all;Zip;  $/r$
                        }
                    }
                }
                else
                {
                    return $"Command Fail";
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region 房间操作

        #region 835获取家庭房间列表
        /// <summary>
        /// 1.请求主机房间列表
        /// user:DAJCHSF_Server type:other msg:DAJCHSF_2047DABEF936;8;835;admin$/r$
        /// 2.主机返回房间列表
        /// user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;835;admin;Zip;H4sIAAAAAAAAAIuuViotTi3KTFGyUlLSUSouSSxJBTJLikpTgdzkjMy8xLzEXJDQs46Jz2e1PF23DSiemZyfB9GQmZuYngrTXZBfDGYaQNgFiUWJSlbVSimpZSX5JYk5QBlDS5AliWmpxaklJZl56TCrasEaSioLUqHa40EGGego+aWWB6Um5xcBeSCFtTrY3Yvm1qfrFj3ta8Xh0KL8/FxDJNcaGhgbmpoYG1iam5iiOJzajupd/nTdEtIcBcRmBjR1VNe8p61rSHaXkampBW0Da13nyxmbSHOUsYmBkTl5jooFAHQFerEIAwAA$/r$
        /// </summary>
        /// <param name="msg">user:DAJCHSF_Server type:other msg:DAJCHSF_2047DABEF936;8;835;admin$/r$</param>
        public static string Host835(string msg)
        {
            try
            {
                if (msg.Split(';').Length >= 3)
                {
                    string appUser = msg.Split(';')[0];
                    if (appUser.Contains("_"))
                    {
                        string account = appUser.Split('_')[0];//DAJCHSF,一个家庭可能有多个用户，mac不同，只取账户
                        IQueryable<host_room> roomList = null;
                        //获取当前房间的设备列表，先找缓存
                        using (RedisHashService service = new RedisHashService())
                        {
                            string msgResult = service.GetValueFromHash("Room", account);
                            if (string.IsNullOrEmpty(msgResult))
                            {
                                using (HsfDBContext hsfDBContext = new HsfDBContext())
                                {
                                    roomList = hsfDBContext.host_room.Where(t => t.Account == account && t.DeleteMark == 0);
                                    msgResult = $"{appUser};835;admin;Zip;{EncryptionHelp.Encryption(JsonConvert.SerializeObject(roomList), true)}$/r$\r\n";//拼接
                                                                                                                                                            //缓存当前账户房间列表返回字符串
                                    service.SetEntryInHash("Room", account, msgResult);
                                }
                            }
                            log.Info($"835 OK,返回房间列表成功！返回信息：{msgResult}");
                            return msgResult;
                        }
                    }
                    else
                    {
                        log.Error($"835 Fail,命令不符合规范！");
                        return null;
                    }
                }
                else
                {
                    log.Error($"835 Fail,命令不符合规范！");
                    return null;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region 836新增房间
        /// <summary>
        /// 新增房间
        /// </summary>
        /// <param name="msg">user:123_Server type:other msg:
        /// 123_DCD9165057AD;8;836;H4sIAAAAAAAAAC2Muw7CIBSG3+XMDBxavLAZmR18gYbgiTIADdAQ0/Tdhdbt+68rPKg+ycb0AlXSQgwm15AzsB8XTDCeQEGtwMDZGBp38uZNvQYpRo/NmWPeNQohceQ4ohguhz+bZECt2y7Kd+5/vEW5mELH35Ip/efDpO/6iifJ5fmmYfsBAlhUH6EAAAA=$/r$
        /// {"NewRecord":true,"_id":0,"chinaname":"ww","icon":"","imageid":"room1","posid":"1225140141238","pospara":{},"postype":"0","state":"","userid":"123_DCD9165057AD"}</param>
        public static void Host836(string msg)
        {
            try
            {
                if (msg.Split(';').Length >= 3)
                {
                    string appUser = msg.Split(';')[0];
                    if (appUser.Contains("_"))
                    {
                        string account = appUser.Split('_')[0];
                        string mac = appUser.Split('_')[1];

                        string zipStr = msg.Split(';')[3].Replace("$/r$", "");
                        string base64j = EncryptionHelp.Decrypt(zipStr, true);
                        var room = JsonConvert.DeserializeObject<host_room>(base64j);//list多件开关
                        room.Account = account;
                        room.Mac = mac;
                        using (HsfDBContext hsfDBContext = new HsfDBContext())
                        {
                            var roomEntity = hsfDBContext.host_room.Where(t => t.posid == room.posid && t.DeleteMark == 0).FirstOrDefault();
                            if (roomEntity != null)
                            {
                                roomEntity.DeleteMark = 1;
                                roomEntity.ModifyTime = DateTime.Now;
                                AddRoom(hsfDBContext, room);
                                log.Debug($"836 Ok,房间信息修改成功！");
                            }
                            else
                            {
                                //当前房间id需要保存,网关房间id为0，不可以
                                AddRoom(hsfDBContext, room);
                                log.Info($"836 OK,添加房间成功！");
                            }
                            //清除房间缓存信息，等待查询之后再次缓存
                            using (RedisHashService service = new RedisHashService())
                            {
                                service.RemoveEntryFromHash("Room", account);//解决默认posid都为0的问题
                            }
                        }
                    }
                    else
                    {
                        log.Error($"836 Fail,添加房间失败，命令不符合规范！");
                    }
                }
                else
                {
                    log.Error($"836 Fail,添加房间失败，命令不符合规范！");
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// 新增房间数据库操作
        /// </summary>
        /// <param name="hsfDBContext"></param>
        /// <param name="_SoundHost"></param>
        public static void AddRoom(HsfDBContext hsfDBContext, host_room room)
        {
            room.id = Guid.NewGuid().ToString();
            room.CreateTime = DateTime.Now;
            room.DeleteMark = 0;
            hsfDBContext.host_room.Add(room);
            hsfDBContext.SaveChanges();
        }
        #endregion

        #endregion

        #region 设备操作

        #region 815获得当前房间的设备列表的命令
        /// <summary>
        /// 获得当前房间的设备列表的命令8;815;+ posid
        /// </summary>
        /// <param name="msg">user:123_Server type:other msg:
        /// 123_DCD9165057AD;8;815;1225155025360$/r$</param>
        public static string Host815(string msg)
        {
            try
            {
                if (msg.Split(';').Length >= 3)
                {
                    string appUser = msg.Split(';')[0];
                    string posid = msg.Split(';')[3].Replace("$/r$", "");//房间id  ,默认id为0，
                    List<host_device> deviceList = null;
                    //获取当前房间的设备列表，先找缓存
                    using (RedisHashService service = new RedisHashService())
                    {
                        string deviceListJson = service.GetValueFromHash("RoomDevices", appUser + "|" + posid);
                        if (!string.IsNullOrEmpty(deviceListJson))
                        {
                            deviceList = JsonConvert.DeserializeObject<List<host_device>>(deviceListJson);
                        }
                        //如果缓存中没有，再查数据库
                        else
                        {
                            using (HsfDBContext hsfDBContext = new HsfDBContext())
                            {
                                deviceList = hsfDBContext.host_device.Where(t => t.devposition == posid && t.deletemark == 0).ToList();
                                //缓存当前房间的设备列表,不包括状态,不管空与否都缓存，防止第二次还查数据库
                                service.SetEntryInHash("RoomDevices", appUser + "|" + posid, JsonConvert.SerializeObject(deviceList));//解决默认posid都为0的问题
                            }
                        }

                        //真正更新设备状态
                        string zipStr = "";
                        foreach (var item in deviceList)
                        {
                            //读取缓存状态
                            string status = service.GetValueFromHash("DeviceStatus", item.cachekey);
                            if (string.IsNullOrEmpty(status))
                            {
                                //离线
                                item.powvalue = "离线";
                                item.devstate = "false";
                            }
                            else
                            {
                                item.powvalue = "在线";
                                item.devstate = status;
                            }
                        }
                        zipStr = EncryptionHelp.Encryption(JsonConvert.SerializeObject(deviceList), true);
                        string msgResult = $"{appUser};815;{posid};Zip;{zipStr}$/r$\r\n";//拼接
                        log.Info($"815 OK,返回房间设备列表成功！返回信息：{msgResult}");
                        return msgResult;
                    }
                }
                else
                {
                    log.Error($"815 Fail,命令不符合规范！");
                    return null;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region 8211添加设备
        /// <summary>
        /// 添加设备 8;8211;All;+Base64(zip(设备对象jhson串))
        /// </summary>
        /// <param name="msg">user:123_Server type:other msg:
        /// 123_DCD9165057AD;8;8211;ALL;H4sIAAAAAAAAAH2QPU7EMBCF7zJ1CjuRs2wuQEdBuQitBmectZTEke2wQqutEAeg5Bx0SHscfo6Bf6JINMiNv/eeZ8Zzd4IbOt6SNLaFxtuZCtjrcGUFyIMeccSBoIGvt4+f58v35fXz5Z1B8Hp0TsUgXAVs6RF7tEPATPKA40j9ylpSCvOyFFyIWvCKbdniTcFgTKlSxlNjrRgTG6WyPaD815/QIjSncwZjfWxTic12sY3TXpsx1siKpU6altI0VZacR0/rsIn4XyxX9E9TjO5090C0v44PC9ADdvmHIcGZCFLfuTRM2ORkjmE/89JhdmRTFM73v4aKUN2AAQAA$/r$</param>
        public static string Host8211(string msg)
        {
            try
            {
                if (msg.Split(';').Length >= 4)
                {
                    string appUser = msg.Split(';')[0];
                    string zipStr = msg.Split(';')[4].Replace("$/r$", "");
                    string base64j = EncryptionHelp.Decrypt(zipStr, true);
                    var deviceLists = JsonConvert.DeserializeObject<List<host_device>>(base64j);//list多件开关,ALL数组
                    string posid = "";
                    string cachekey = "";
                    using (HsfDBContext hsfDBContext = new HsfDBContext())
                    {
                        foreach (var item in deviceLists)
                        {
                            posid = item.devposition;
                            cachekey = item.devmac + "_" + item.devport;//存在mac相同，端口不相同的多键设备
                            if (string.IsNullOrEmpty(item.userid))
                            {
                                item.userid = appUser;
                            }
                            var deviceEntity = hsfDBContext.host_device.Where(t => t.cachekey == cachekey && t.deletemark == 0).FirstOrDefault();
                            if (deviceEntity != null)
                            {
                                deviceEntity.deletemark = 1;
                                deviceEntity.modifiytime = DateTime.Now;
                                AddDevice(hsfDBContext, item);
                                log.Debug($"8211 OK,重新添加设备成功！");
                            }
                            else
                            {
                                //当前房间id需要保存,网关房间id为0，不可以
                                AddDevice(hsfDBContext, item);
                            }
                        }
                        //2.主机返回app添加成功
                        string msgResult = $"{appUser};8211;ALL;Zip;H4sIAAAAAAAAAHNMScnPBgD0Si5gBQAAAA==$/r$";//拼接
                                                                                                              //清除房间设备列表缓存
                        using (RedisHashService service = new RedisHashService())
                        {
                            service.RemoveEntryFromHash("RoomDevices", appUser + "|" + posid);//解决默认posid都为0的问题
                        }

                        log.Info($"8211 OK,添加设备成功！返回信息：{msgResult}");
                        return msgResult;
                    }
                }
                else
                {
                    log.Error($"8211 Fail,添加设备失败，命令不符合规范！");
                    return null;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 添加新的设备数据库
        /// </summary>
        /// <param name="hsfDBContext"></param>
        /// <param name="_SoundHost"></param>
        public static void AddDevice(HsfDBContext hsfDBContext, host_device item)
        {
            item.id = Guid.NewGuid().ToString();
            item.createtime = DateTime.Now;
            item.deletemark = 0;
            item.cachekey = item.devmac + "_" + item.devport;
            hsfDBContext.host_device.Add(item);
            hsfDBContext.SaveChanges();
        }
        #endregion

        #region 8135关闭设备
        /// <summary>
        /// 8135关闭设备 8;8135;设备id
        /// </summary>
        /// <param name="msg">user:DAJCHSF_Server type:other msg:DAJCHSF_2047DABEF936;8;8135;1041657481380;2;8$/r$
        /// {"code":1002,"id":"010000124b00198c4341","ep":1,"serial":1,"control":{"on":true},"result":0,"zigbee":"00ff2c2c2c6a6f0057ff"}</param>
        public static string Host8135(string msg)
        {
            Zigbee1002 zigbee1002 = new Zigbee1002()
            {
                code = 1002,
                id = "010000124b00198c4341",
                ep = 1,
                serial = 1,
                control = new Control { on = true },
                result = 0,
                zigbee = "00ff2c2c2c6a6f005979"
            };
            YunHostClient.SendMsg(JsonConvert.SerializeObject(zigbee1002));
            return "123_DCD9165057AD;8135;808181248576;Zip;H4sIAAAAAAAAAEvOyS9Ozc92sDCwMLQwNDKxMDU3AwCjJ+18FAAAAA==$/r$";
        }
        #endregion

        #region 8145关闭设备
        /// <summary>
        /// 8145关闭设备 8;8135;设备id
        /// </summary>
        /// <param name="msg">user:0203_Server type:other msg:0203_ASDFDSSE123;8;8145;808181248576;3,0$/r$</param>
        public static string Host8145(string msg)
        {
            Zigbee1002 zigbee1002 = new Zigbee1002()
            {
                code = 1002,
                id = "010000124b00198c4341",
                ep = 1,
                serial = 1,
                control = new Control{ on = false },
                result = 0,
                zigbee = "00ff2c2c2c6a6f005979"
            };
            YunHostClient.SendMsg(JsonConvert.SerializeObject(zigbee1002));
            return "123_DCD9165057AD;8145;808181248576;Zip;H4sIAAAAAAAAAEvOyS9Ozc92sDCwMLQwNDKxMDU3AwCjJ+18FAAAAA==$/r$";
        }
        #endregion

        #endregion

    }
}
