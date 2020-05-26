using Baidu.Aip.Nlp;
using Baidu.Aip.Speech;
using Hsf.Framework;
using Hsf.Redis.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Hsf.Framework
{
    public class BaiduSDK
    {
        public static Tts tts;
        public static Nlp nlp;
        public static string mp3Path = ConfigAppSettings.GetValue("mp3Path");
        public static string mp3Url = ConfigAppSettings.GetValue("mp3Url");
        public static string mp3Fail = ConfigAppSettings.GetValue("mp3Fail");

        /// <summary>
        /// 语音合成1024字节最大
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Tts(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    string url = "";
                    if (text.Contains('{'))
                    {
                        //int uu = text.IndexOf("url\":\"");
                        //if (uu > 0)
                        //{
                        //    text = text.Substring(uu + 6, text.Length - uu - 6);
                        //    int yy = text.IndexOf('\"');
                        //    url = text.Substring(0, yy);
                        //    return url;//还有不是MP3的url
                        //}
                        //else
                        //{
                        //    return null;
                        //}
                        return null;//json字符串的不生成url
                    }
                    else
                    {
                        if (tts == null)
                        {
                            string APP_ID = "14884741";
                            string API_KEY = "UNBsgAYhfLciqDsWRApMmSTn";
                            string SECRET_KEY = "bLpvCoLguv8Xg04c6E8W8LVgwluC4GC4";
                            tts = new Tts(API_KEY, SECRET_KEY);
                            tts.Timeout = 60000;  // 修改超时时间
                        }

                        // 可选参数
                        var option = new Dictionary<string, object>()
                    {
                        {"spd", 6}, // 语速
                        {"vol", 5}, // 音量
                        {"per", 4}  // 发音人，发音人选择, 0为普通女声，1为普通男生，2为普通男生，3为情感合成-度逍遥，4为情感合成-度丫丫，默认为普通女声
                    };
                        var result = tts.Synthesis(text, option);
                        string path = "";
                        if (result.ErrorCode == 0)  // 或 result.Success
                        {
                            string filename = DateTime.Now.ToString("HHmmss-fff") + ".mp3";
                            path = $"{ mp3Path}{ DateTime.Now.Date.ToString("yyyyMMdd")}\\{filename}";//C:\IIS\MP3\20181216\150225-266.mp3
                            if (!Directory.Exists($"{ mp3Path}{ DateTime.Now.Date.ToString("yyyyMMdd")}"))
                            {
                                Directory.CreateDirectory($"{ mp3Path}{ DateTime.Now.Date.ToString("yyyyMMdd")}");
                            }
                            File.WriteAllBytes(path, result.Data);

                            url = $"{mp3Url}{ DateTime.Now.Date.ToString("yyyyMMdd")}/{filename}";//http://47.107.66.121:8044/20181216/150225-266.mp3

                        }
                        return url;
                    }


                }
                else
                {
                    return null;
                }

            }
            catch (Exception)
            {

                throw;
            }

        }
        /// <summary>
        /// 分词
        /// </summary>
        public static BaiduNlp Nlp(string text)
        {
            try
            {
                if (nlp == null)
                {
                    string APP_ID = "14902717";
                    string API_KEY = "1qHCYskEsmQyMYYwGa2b4RI9";
                    string SECRET_KEY = "34uRO8hYapy7OTKGzuEDK3EeLyDsZOMt";
                    nlp = new Nlp(API_KEY, SECRET_KEY);
                    nlp.Timeout = 60000;  // 修改超时时间
                }

                // 调用词法分析，可能会抛出网络等异常，请使用try/catch捕获
                var result = nlp.Lexer(text);
                BaiduNlp nlpEntity = JsonConvert.DeserializeObject<BaiduNlp>(result.ToString());
                return nlpEntity;

            }
            catch (Exception)
            {

                throw;
            }

        }
    }

    public static class AccessToken
    {
        // 调用getAccessToken()获取的 access_token建议根据expires_in 时间 设置缓存Access Token的有效期(秒为单位，一般为1个月)；
        // 返回token示例
        public static String TOKEN = "24.adda70c11b9786206253ddb70affdc46.2592000.1493524354.282335-1234567";

        // 百度云中开通对应服务应用的 API Key 建议开通应用的时候多选服务
        private static String clientId = "sOdODjDDe1LaD9DIbz98KC2C";
        // 百度云中开通对应服务应用的 Secret Key
        private static String clientSecret = "tcvtTTQ2FCuVj0aTrOEOo3e0HRbkOVLV";

        public static String getAccessToken()
        {
            String authHost = "https://aip.baidubce.com/oauth/2.0/token";
            HttpClient client = new HttpClient();
            List<KeyValuePair<String, String>> paraList = new List<KeyValuePair<string, string>>();
            paraList.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            paraList.Add(new KeyValuePair<string, string>("client_id", clientId));
            paraList.Add(new KeyValuePair<string, string>("client_secret", clientSecret));

            HttpResponseMessage response = client.PostAsync(authHost, new FormUrlEncodedContent(paraList)).Result;
            String result = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(result);
            return result;
        }
    }

    public class Utterance
    {
        // unit对话接口
        public static string unit_utterance()
        {
            string token = "";//#####调用鉴权接口获取的token#####
            using (RedisStringService service = new RedisStringService())
            {
                token=service.Get("unit_token");
                if (!string.IsNullOrEmpty(token))
                {
                    token= token.Replace("\"", "");
                }
                else
                {
                    string tokenJson = AccessToken.getAccessToken();
                    var tokenEntity=JsonConvert.DeserializeObject<UnitToken>(tokenJson);
                    token = tokenEntity.access_token;
                    service.Set("unit_token", tokenEntity.access_token,DateTime.Now.AddDays(29));//缓存29天
                }
            }

            string host = "https://aip.baidubce.com/rpc/2.0/unit/service/chat?access_token=" + token;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(host);
            request.Method = "post";
            request.ContentType = "application/json";
            request.KeepAlive = true;
            string str = "{\"log_id\":\"UNITTEST_10000\",\"version\":\"2.0\",\"service_id\":\"S15567\",\"session_id\":\"\",\"request\":{\"query\":\"你好\",\"user_id\":\"88888\"},\"dialog_state\":{\"contexts\":{\"SYS_REMEMBERED_SKILLS\":[\"1057\"]}}}"; // json格式
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string resultJson = reader.ReadToEnd();
            dynamic resultEntity = JsonConvert.DeserializeObject<dynamic>(resultJson);
            string result = resultEntity.result.response_list[0].action_list[0].say.Value;
            Console.WriteLine("对话接口返回:");
            Console.WriteLine(result);
            return result;
        }
    }

    public class BaiduNlpItem
    {
        /// <summary>
        /// 
        /// </summary>
        public List<string> loc_details { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int byte_offset { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string uri { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string pos { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ne { get; set; }
        /// <summary>
        /// 百度
        /// </summary>
        public string item { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> basic_words { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int byte_length { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string formal { get; set; }
    }

    public class BaiduNlp
    {
        /// <summary>
        /// 
        /// </summary>
        public long log_id { get; set; }
        /// <summary>
        /// 百度是一家高科技公司
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<BaiduNlpItem> items { get; set; }
    }


    public class UnitToken
    {
        /// <summary>
        /// 
        /// </summary>
        public string refresh_token { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int expires_in { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string session_key { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string access_token { get; set; }
        /// <summary>
        /// audio_voice_assistant_get brain_enhanced_asr audio_tts_post public brain_all_scope unit_理解与交互V2 wise_adapt lebo_resource_base lightservice_public hetu_basic lightcms_map_poi kaidian_kaidian ApsMisTest_Test权限 vis-classify_flower lpq_开放 cop_helloScope ApsMis_fangdi_permission smartapp_snsapi_base iop_autocar oauth_tp_app smartapp_smart_game_openapi oauth_sessionkey smartapp_swanid_verify smartapp_opensource_openapi
        /// </summary>
        public string scope { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string session_secret { get; set; }
    }
}
