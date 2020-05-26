using Hsf.DAL;
using Hsf.Entity;
using Hsf.Framework;
using Hsf.Interface;
using Hsf.Redis.Service;
using Hsf.Service;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hsf.Project
{
    class Program
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("Main");
        static void Main(string[] args)
        {
            //启动nlp中枢控制器
            HsfWebSocket.Start();

            //启动智能家居socket服务器
            SmartHomeNlpServer.Start();

            //设置默认线程数量，最小数量
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            int port = Convert.ToInt32(config.AppSettings.Settings["MinThreads"].Value);
            ThreadPool.SetMinThreads(port, port);

            ContainerFactory.CreateContainer();

            NlpControler.RegEvent();

            Console.ReadKey();
        }

    }

    public class RoomDev
    {
        public string roomname { get; set; }
        public string devname { get; set; }
    }
}

//YunHostClient.ConnectServer();
//YunHostClient.Heartbeat();
//string dd = "";
//Thread.Sleep(100);
//for (int i = 0; i < 1000; i++)
//{
//    Thread.Sleep(1);
//    dd = "{\"code\":104,\"control\":2,\"id\":\"010000124b00198c4341\",\"ol\":true,\"ep\":" + i + ",\"pid\":260,\"did\":0,\"st\":{\"on\":true}}";
//    YunHostClient.SendMsg(dd);
//    log.Info(dd);
//}
//long _timestamp1 = DataHelper.GetTimeSpan(DateTime.Now);
//long _timestamp2 = DataHelper.GetTimeSpan(DateTime.Now);

//long cha12 = _timestamp2 - _timestamp1;//一分钟60秒

//long l1 = 1552376640;
//long l2 = 1552376700;
//long cha = l2 - l1;//一分钟60秒

// SqlQuery
//try
//{
//    using (HsfDBContext entity = new HsfDBContext())
//    {
//        string strSQL = "select case when room.chinaname is null then '默认房间' else room.chinaname end roomname ,dev.chinaname devname from host_device dev left join host_room room on room.id = dev.devposition where dev.deletemark = 0";
//        var info = entity.Database.SqlQuery<RoomDev>(strSQL);
//        foreach (var item in info)
//        {
//            Console.WriteLine("房间名:" + item.roomname + " " + "设备名:" + item.devname);
//        }
//    }
//}
//catch (Exception ex)
//{
//    Console.WriteLine(ex.Message);
//}


//BaiduSDK.Nlp("打开所有灯光");

//string str = "1,2,3,4,5,6,7";
//string[] strArray = str.Split(','); //字符串转数组
//str = string.Empty;
//str = string.Join(",", strArray);//数组转成字符串

//using (RedisHashService service = new RedisHashService())
//{
//    //service.Set<string>(_SoundHost.devmac, _SoundHost.userid);//缓存主机与音响的绑定关系
//    service.SetEntryInHash("Sound_Host", "10d07aaa335e", "walifire_Server");//缓存主机与音响的绑定关系,重复绑定覆盖
//    string hostid = service.GetValueFromHash("Sound_Host", "10d07aaa335e");
//}
//using (RedisStringService service = new RedisStringService())
//{
//    string hostid = service.Get("10d07aaa335e");//获取缓存中与音响绑定的主机
//}

//string device = "{\"timestamp\":2573194097,\"code\":101,\"device\":[{\"id\":\"010000124b0014c592c2\",\"ep\":3,\"pid\":260,\"did\":0,\"ol\":false,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt3\",\"swid\":\"Lafei_RT_3Switch-v1.0\",\"st\":{\"dsp\":\"SZShunCom RmtCt3\",\"fac\":\"ShunCom\",\"on\":false}},{\"id\":\"010000124b00198c36f1\",\"ep\":2,\"pid\":260,\"did\":0,\"ol\":false,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt2\",\"swid\":\"Lafei_RT_2Switch-v1.9\",\"st\":{\"fac\":\"ShunCom\",\"on\":false,\"dsp\":\"SZShunCom RmtCt2\"}},{\"id\":\"010000124b00198c36f1\",\"ep\":1,\"pid\":260,\"did\":0,\"ol\":false,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt2\",\"swid\":\"Lafei_RT_2Switch-v1.9\",\"st\":{\"fac\":\"ShunCom\",\"swid\":\"Lafei_RT_2Switch-v1.9\",\"on\":false,\"dsp\":\"SZShunCom RmtCt2\"}},{\"id\":\"010000124b0014c592c2\",\"ep\":2,\"pid\":260,\"did\":0,\"ol\":false,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt3\",\"swid\":\"Lafei_RT_3Switch-v1.0\",\"st\":{\"dsp\":\"SZShunCom RmtCt3\",\"fac\":\"ShunCom\",\"on\":false}},{\"id\":\"010000124b0014c592c2\",\"ep\":4,\"pid\":260,\"did\":0,\"ol\":false,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt3\",\"swid\":\"Lafei_RT_3Switch-v1.0\",\"st\":{\"dsp\":\"SZShunCom RmtCt3\",\"fac\":\"ShunCom\",\"on\":false}},{\"id\":\"010000124b0014c592c2\",\"ep\":1,\"pid\":260,\"did\":0,\"ol\":false,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt3\",\"swid\":\"Lafei_RT_3Switch-v1.0\",\"st\":{\"fac\":\"ShunCom\",\"dsp\":\"SZShunCom RmtCt3\",\"swid\":\"Lafei_RT_3Switch-v1.0\",\"on\":false}},{\"id\":\"010000124b00170fabec\",\"ep\":1,\"pid\":260,\"did\":1026,\"ol\":true,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"Shuncom\",\"ztype\":65535,\"dsp\":\"HORN-GAS--A1.7-R\",\"swid\":\"HORN-GAS--V1.7-B\",\"st\":{\"fac\":\"Shuncom\",\"lqi\":254,\"dsp\":\"HORN-GAS--A1.7-R\"}},{\"id\":\"010000124b0016043ebc\",\"ep\":1,\"pid\":260,\"did\":1026,\"ol\":true,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"HORN-ZONE       \",\"ztype\":13,\"dsp\":\"HORN-PIR--A3.9-E\",\"swid\":\"HORN-PIR--V3.9-B\",\"st\":{\"dsp\":\"HORN-PIR--A3.9-E\",\"zid\":3,\"ztype\":13,\"zstate\":1,\"zsta\":4,\"Supervision\":250,\"batpt\":200,\"lqi\":212,\"fac\":\"HORN-ZONE       \"}},{\"id\":\"010000124b0016041441\",\"ep\":1,\"pid\":260,\"did\":1026,\"ol\":true,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"HORN-ZONE       \",\"ztype\":13,\"dsp\":\"HORN-PIR--A3.9-E\",\"swid\":\"HORN-PIR--V3.9-B\",\"st\":{\"fac\":\"HORN-ZONE       \",\"zid\":1,\"ztype\":13,\"zstate\":1,\"zsta\":0,\"Supervision\":250,\"batpt\":200,\"lqi\":221,\"dsp\":\"HORN-PIR--A3.9-E\"}},{\"id\":\"010000124b000fa4ae5b\",\"ep\":1,\"pid\":260,\"did\":1026,\"ol\":true,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"HORN-ZONE       \",\"ztype\":40,\"dsp\":\"HORN-SMOG-A3.9-E\",\"swid\":\"HORN-SMOG-V3.9-B\",\"st\":{\"fac\":\"HORN-ZONE       \",\"zid\":0,\"ztype\":40,\"zstate\":1,\"zsta\":0,\"Supervision\":250,\"lqi\":215,\"batpt\":200,\"dsp\":\"HORN-SMOG-A3.9-E\"}},{\"id\":\"010000124b00198c46a2\",\"ep\":13,\"pid\":49246,\"did\":57694,\"ol\":false,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"Shuncom\",\"ztype\":65535,\"dsp\":\"SZ Light\",\"swid\":\"1.0.2\",\"st\":{}},{\"id\":\"010000124b00198c46a2\",\"ep\":12,\"pid\":49246,\"did\":512,\"ol\":false,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"Shuncom\",\"ztype\":65535,\"dsp\":\"SZ Light\",\"swid\":\"1.0.2\",\"st\":{}},{\"id\":\"010000124b00198c46a2\",\"ep\":11,\"pid\":49246,\"did\":512,\"ol\":false,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"Shuncom\",\"ztype\":65535,\"dsp\":\"SZ Light\",\"swid\":\"1.0.2\",\"st\":{\"fac\":\"Shuncom\",\"dsp\":\"SZ Light\"}},{\"id\":\"010000124b0014c06e3d\",\"ep\":3,\"pid\":260,\"did\":0,\"ol\":true,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt3\",\"swid\":\"ShuncomDevice-v1.0\",\"st\":{\"fac\":\"ShunCom\",\"on\":false,\"dsp\":\"SZShunCom RmtCt3\"}},{\"id\":\"010000124b0014c5942e\",\"ep\":4,\"pid\":260,\"did\":0,\"ol\":true,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt3\",\"swid\":\"ShuncomDevice-v1.0\",\"st\":{\"fac\":\"ShunCom\",\"on\":false,\"dsp\":\"SZShunCom RmtCt3\"}},{\"id\":\"010000124b00198c4356\",\"ep\":2,\"pid\":260,\"did\":0,\"ol\":true,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt2\",\"swid\":\"ShuncomDevice-v1.0\",\"st\":{\"fac\":\"ShunCom\",\"on\":false,\"dsp\":\"SZShunCom RmtCt2\"}},{\"id\":\"010000124b00198c4356\",\"ep\":1,\"pid\":260,\"did\":0,\"ol\":true,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt2\",\"swid\":\"Lafei_RT_2Switch-v1.9\",\"st\":{\"fac\":\"ShunCom\",\"on\":false,\"dsp\":\"SZShunCom RmtCt2\"}},{\"id\":\"010000124b00198c4341\",\"ep\":1,\"pid\":260,\"did\":0,\"ol\":true,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt1\",\"swid\":\"Lafei_RT_1Switch-v1.9\",\"st\":{\"dsp\":\"SZShunCom RmtCt1\",\"on\":false,\"fac\":\"ShunCom\"}},{\"id\":\"010000124b0014c06e3d\",\"ep\":1,\"pid\":260,\"did\":0,\"ol\":true,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt3\",\"swid\":\"Lafei_RT_3Switch-v1.0\",\"st\":{\"dsp\":\"SZShunCom RmtCt3\",\"on\":false,\"fac\":\"ShunCom\"}},{\"id\":\"010000124b0014c06e3d\",\"ep\":2,\"pid\":260,\"did\":0,\"ol\":true,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt3\",\"swid\":\"Lafei_RT_3Switch-v1.0\",\"st\":{\"fac\":\"ShunCom\",\"on\":false,\"dsp\":\"SZShunCom RmtCt3\"}},{\"id\":\"010000124b0014c5942e\",\"ep\":3,\"pid\":260,\"did\":0,\"ol\":true,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt3\",\"swid\":\"Lafei_RT_3Switch-v1.0\",\"st\":{\"dsp\":\"SZShunCom RmtCt3\",\"fac\":\"ShunCom\",\"on\":false}},{\"id\":\"010000124b0014c5942e\",\"ep\":2,\"pid\":260,\"did\":0,\"ol\":true,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt3\",\"swid\":\"Lafei_RT_3Switch-v1.0\",\"st\":{\"dsp\":\"SZShunCom RmtCt3\",\"fac\":\"ShunCom\",\"on\":false}},{\"id\":\"010000124b0014c5942e\",\"ep\":1,\"pid\":260,\"did\":0,\"ol\":true,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt3\",\"swid\":\"Lafei_RT_3Switch-v1.0\",\"st\":{\"dsp\":\"SZShunCom RmtCt3\",\"fac\":\"ShunCom\",\"on\":false}},{\"id\":\"010000124b000f81f7f4\",\"ep\":8,\"pid\":260,\"did\":514,\"ol\":false,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"Shuncom\",\"ztype\":65535,\"dsp\":\"SZwincover000001\",\"swid\":\"UiotMoto3_1.2.06\",\"st\":{\"fac\":\"Shuncom\",\"pt\":255,\"dsp\":\"SZwincover000001\"}},{\"id\":\"010000124b00172ef9b5\",\"ep\":2,\"pid\":260,\"did\":0,\"ol\":false,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt3\",\"swid\":\"Lafei_RT_3Switch-v1.5\",\"st\":{}},{\"id\":\"010000124b00172ef9b5\",\"ep\":3,\"pid\":260,\"did\":0,\"ol\":false,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt3\",\"swid\":\"Lafei_RT_3Switch-v1.5\",\"st\":{}},{\"id\":\"010000124b00172ef9b5\",\"ep\":1,\"pid\":260,\"did\":0,\"ol\":false,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt3\",\"swid\":\"Lafei_RT_3Switch-v1.5\",\"st\":{\"dsp\":\"SZShunCom RmtCt3\",\"fac\":\"ShunCom\"}},{\"id\":\"010000124b00181a1ea3\",\"ep\":1,\"pid\":260,\"did\":1026,\"ol\":true,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"Shuncom\",\"ztype\":277,\"dsp\":\"Hoen_KEY_BP     \",\"swid\":\"Hoen_KEY_BP_1.33\",\"st\":{\"fac\":\"Shuncom\",\"Supervision\":240,\"zid\":0,\"ztype\":277,\"lqi\":214,\"dsp\":\"Hoen_KEY_BP     \"}},{\"id\":\"010000124b0016025049\",\"ep\":1,\"pid\":260,\"did\":1026,\"ol\":true,\"dn\":\"ShuncomDevice\",\"dtype\":107,\"fac\":\"HORN-ZONE       \",\"ztype\":42,\"dsp\":\"HORN-WATE-A3.9-E\",\"swid\":\"HORN-WATE-V3.9-B\",\"st\":{\"fac\":\"HORN-ZONE       \",\"zid\":4,\"ztype\":42,\"zstate\":1,\"zsta\":4,\"Supervision\":250,\"lqi\":193,\"batpt\":200,\"dsp\":\"HORN-WATE-A3.9-E\"}},{\"id\":\"010000124b00172f668f\",\"ep\":2,\"pid\":260,\"did\":0,\"ol\":false,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt3\",\"swid\":\"Lafei_RT_3Switch-v1.5\",\"st\":{}},{\"id\":\"010000124b00172f668f\",\"ep\":3,\"pid\":260,\"did\":0,\"ol\":false,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt3\",\"swid\":\"Lafei_RT_3Switch-v1.5\",\"st\":{}},{\"id\":\"010000124b00172f668f\",\"ep\":1,\"pid\":260,\"did\":0,\"ol\":false,\"dn\":\"ShuncomDevice\",\"dtype\":0,\"fac\":\"ShunCom\",\"ztype\":65535,\"dsp\":\"SZShunCom RmtCt3\",\"swid\":\"Lafei_RT_3Switch-v1.5\",\"st\":{\"fac\":\"ShunCom\",\"dsp\":\"SZShunCom RmtCt3\"}}]}";

//ZigBeeDevice host_device = JsonConvert.DeserializeObject<ZigBeeDevice>(device);//反序列化
//foreach (var item in host_device.device)
//{
//    var ol = item.ol;
//    if (ol == "true")
//    {
//        var st = item.st;
//        string on = st.on;
//    }
//}
//using (RedisStringService service = new RedisStringService())
//{
//    string hostid = service.Get("44C874F7CC43").Replace("\"","");//获取缓存中与音响绑定的主机
//}
//string jsonStr = "{\"data\": {\"ssoToken\": \"70abd3d8a6654ff189c482fc4842468c\",\"account\":\"admin\",\"userType\":\"platformAdmin\",\"realName\": \"超级管理员\",\"sex\": 0,\"sexName\":\"男\",\"email\":\"alina_dong@163.com\",\"mobile\":\"15120757948\",\"createdDt\": \"2013-08-16 00:00:00\",\"updatedDt\": \"2014-12-10 00:00:00\" },\"isSuccess\": true}";
//var loginInfo = JsonConvert.DeserializeObject<dynamic>(jsonStr);
//var user = loginInfo.data;
//string ssoToken = user.ssoToken;
//string account = user.account;

//string msg= "{\"code\":104,\"control\":2,\"id\":\"010000124b00198c4341\",\"ol\":true,\"ep\":1,\"pid\":260,\"did\":0,\"st\":{\"on\":true}}";
//dynamic result = JsonConvert.DeserializeObject<dynamic>(msg);//反序列化
//string dd= "[{\"NewRecord\":true,\"_id\":0,\"chinaname\":\"智能开关0\",\"classfid\":\"8\",\"devalarm\":\"\",\"devchannel\":\"00ff2c2c2c6a6f005965\",\"deviceid\":\"01040925117040\",\"devip\":\"0\",\"devmac\":\"010000124b0014c039a3\",\"devpara\":{\"close\":\"关闭,433,1,1\",\"open\":\"开启,433,1,1\"},\"devport\":\"1\",\"devposition\":\"1225155025360\",\"devregcode\":\"123\",\"devstate\":\"\",\"devstate1\":\"\",\"devstate2\":\"\",\"devtype\":\"Panel_Zigbee\",\"imageid\":\"dev105\",\"lgsort\":0,\"powvalue\":\"\",\"userid\":\"\"},{\"NewRecord\":true,\"_id\":0,\"chinaname\":\"智能开关1\",\"classfid\":\"8\",\"devalarm\":\"\",\"devchannel\":\"00ff2c2c2c6a6f005965\",\"deviceid\":\"01040925117051\",\"devip\":\"0\",\"devmac\":\"010000124b0014c039a3\",\"devpara\":{\"close\":\"关闭,433,1,2\",\"open\":\"开启,433,1,2\"},\"devport\":\"2\",\"devposition\":\"1225155025360\",\"devregcode\":\"123\",\"devstate\":\"\",\"devstate1\":\"\",\"devstate2\":\"\",\"devtype\":\"Panel_Zigbee\",\"imageid\":\"dev105\",\"lgsort\":0,\"powvalue\":\"\",\"userid\":\"\"},{\"NewRecord\":true,\"_id\":0,\"chinaname\":\"智能开关2\",\"classfid\":\"8\",\"devalarm\":\"\",\"devchannel\":\"00ff2c2c2c6a6f005965\",\"deviceid\":\"01040925117052\",\"devip\":\"0\",\"devmac\":\"010000124b0014c039a3\",\"devpara\":{\"close\":\"关闭,433,1,3\",\"open\":\"开启,433,1,3\"},\"devport\":\"3\",\"devposition\":\"103154315460\",\"devregcode\":\"123\",\"devstate\":\"\",\"devstate1\":\"\",\"devstate2\":\"\",\"devtype\":\"Panel_Zigbee\",\"imageid\":\"dev105\",\"lgsort\":0,\"powvalue\":\"\",\"userid\":\"\"}]";
//var deviceLists = JsonConvert.DeserializeObject<List<host_device>>(dd);




////读取（音响-主机）的配置文件--缓存
//ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
//fileMap.ExeConfigFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "CfgFiles\\SoundHost.Config");//找配置文件的路径
//Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

//var mySection = configuration.GetSection("mySection") as MySection;
//string dd2 = mySection.KeyValues["ddd"].ToString();//异常
//foreach (MySection.MyKeyValueSetting add in mySection.KeyValues)
//{
//    //Console.WriteLine(string.Format("{0}-{1}", add.Key, add.Value));
//}

//string dd = ConfigAppSettings.GetValue("ddd");//不报错


//#region  讯飞


///// <summary>
///// HttpWebRequest实现post请求
///// </summary>
//public static string PostWebQuest()
//{
//    string URL = "http://api.xfyun.cn/v1/service/v1/tts";
//    string AUE = "raw";
//    string APPID = "5bf3c333";
//    string API_KEY = "72be330c4d33e57dc4a48cdab532237c";

//    var bodyResult = new
//    {
//        text = "打开窗帘"
//    };
//    string postData = JsonConvert.SerializeObject(bodyResult);

//    var request = HttpWebRequest.Create(URL) as HttpWebRequest;
//    request.Timeout = 30 * 1000;//设置30s的超时
//                                //request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36";
//    request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
//    request.Method = "POST";


//    string curTime = GetTimeSpan(DateTime.Now).ToString();
//    string param = "{\"aue\":\"" + AUE + "\",\"auf\":\"audio/L16;rate=16000\",\"voice_name\":\"xiaoyan\",\"engine_type\":\"intp65\"}";
//    byte[] plaindata = Encoding.UTF8.GetBytes(param);
//    string paramBase64 = Convert.ToBase64String(plaindata);
//    string checkSum = Hash_MD5_32(API_KEY + curTime + paramBase64);

//    request.Headers.Set("X-CurTime", curTime);//自定义
//    request.Headers.Set("X-Param", paramBase64);//自定义
//    request.Headers.Set("X-Appid", APPID);//自定义
//    request.Headers.Set("X-CheckSum", checkSum);//自定义
//                                                //request.Headers.Set("'X-Real-Ip", "127.0.0.1");//自定义

//    byte[] data = Encoding.UTF8.GetBytes(postData);
//    request.ContentLength = data.Length;
//    Stream postStream = request.GetRequestStream();
//    postStream.Write(data, 0, data.Length);
//    postStream.Close();

//    string result = "";
//    using (var res = request.GetResponse() as HttpWebResponse)
//    {
//        if (res.StatusCode == HttpStatusCode.OK)
//        {
//            StreamReader reader = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
//            result = reader.ReadToEnd();
//        }
//    }
//    //Console.WriteLine("resultresultresult" + result);
//    return result;
//}


////DateTime转换为时间戳
//public static long GetTimeSpan(DateTime time)
//{
//    DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
//    return (long)(time - startTime).TotalSeconds;
//}//timeSpan转换为DateTime
//public DateTime TimeSpanToDateTime(long span)
//{
//    DateTime time = DateTime.MinValue;
//    DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
//    time = startTime.AddSeconds(span);
//    return time;
//}

///// <summary>
///// 计算32位MD5码
///// </summary>
///// <param name="word">字符串</param>
///// <param name="toUpper">返回哈希值格式 true：英文大写，false：英文小写</param>
///// <returns></returns>
//public static string Hash_MD5_32(string word, bool toUpper = true)
//{
//    try
//    {
//        System.Security.Cryptography.MD5CryptoServiceProvider MD5CSP
//         = new System.Security.Cryptography.MD5CryptoServiceProvider();
//        byte[] bytValue = System.Text.Encoding.UTF8.GetBytes(word);
//        byte[] bytHash = MD5CSP.ComputeHash(bytValue);
//        MD5CSP.Clear();
//        //根据计算得到的Hash码翻译为MD5码
//        string sHash = "", sTemp = "";
//        for (int counter = 0; counter < bytHash.Count(); counter++)
//        {
//            long i = bytHash[counter] / 16;
//            if (i > 9)
//            {
//                sTemp = ((char)(i - 10 + 0x41)).ToString();
//            }
//            else
//            {
//                sTemp = ((char)(i + 0x30)).ToString();
//            }
//            i = bytHash[counter] % 16;
//            if (i > 9)
//            {
//                sTemp += ((char)(i - 10 + 0x41)).ToString();
//            }
//            else
//            {
//                sTemp += ((char)(i + 0x30)).ToString();
//            }
//            sHash += sTemp;
//        }
//        //根据大小写规则决定返回的字符串
//        return toUpper ? sHash : sHash.ToLower();
//    }
//    catch (Exception ex)
//    {
//        throw new Exception(ex.Message);
//    }
//}
//#endregion