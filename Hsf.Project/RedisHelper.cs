using Hsf.DAL;
using Hsf.Redis.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsf.Project
{
    public class RedisHelper
    {
        public static string GetHostId(string deviceId)
        {
            string hostid = "";
            using (RedisHashService service = new RedisHashService())
            {
                //hostid = service.Get(body.deviceId);//获取缓存中与音响绑定的主机
                hostid = service.GetValueFromHash("Sound_Host", deviceId);
                //缓存中不存在再查数据库
                if (!string.IsNullOrEmpty(hostid))
                {
                    return hostid;
                }
                else
                {
                    //根据设备id获取主机ID
                    using (HsfDBContext hsfDBContext = new HsfDBContext())
                    {
                        //根据音响devmac找对应的主机userid，向主机发送消息
                        var soundhostEntity = hsfDBContext.sound_host.Where(t => t.devmac == deviceId && t.deletemark == 0).FirstOrDefault();
                        if (soundhostEntity != null)
                        {
                            if (!string.IsNullOrEmpty(soundhostEntity.userid))
                            {
                                hostid = soundhostEntity.userid;
                                //service.Set<string>(body.deviceId, hostid);//缓存主机与音响的绑定关系
                                service.SetEntryInHash("Sound_Host", deviceId, hostid);//缓存主机与音响的绑定关系,重复绑定覆盖
                                return hostid;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }
    }
}
