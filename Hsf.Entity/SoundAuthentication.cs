using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsf.Model
{
    public class SoundAuthentication
    {
        /// <summary>
        /// 由宜城提供
        /// </summary>
        public string key { get; set; }
        /// <summary>
        /// 设备ID 音箱id,唯一，由客户端生成
        /// </summary>
        public string deviceId { get; set; }
        /// <summary>
        /// 每次会话id,,唯一，由客户端生成
        /// </summary>
        public string sessionId { get; set; }
        /// <summary>
        /// 每次请求动作id 111为注册登录请求
        /// </summary>
        public string actionId { get; set; }
        /// <summary>
        /// unix时间戳
        /// </summary>
        public string timestamp { get; set; }
        /// <summary>
        /// 由以上几项及 secret 按约定的加密方式生成,值由上方拼接的字符串组成key={key}&deviceId={deviceid}&version={version}&time={time}&secret={secret}
        /// 服务器返回消息其他参数,测试值为123
        /// </summary>
        public string req { get; set; }
        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddress { get; set; }

    }

    public class SoundAuthenticationBack
    {
        /// <summary>
        /// 每次会话id,,唯一，由客户端生成
        /// </summary>
        public string sessionId { get; set; }
        /// <summary>
        /// 1111为登录成功，1112为登录失败
        /// </summary>
        public string actionId { get; set; }
        /// <summary>
        /// 如果成功，token会放入req内容处，客户端每次提交请求，都需带上token
        /// </summary>
        public string req { get; set; }
    }

    public class SoundHeart
    {
        /// <summary>
        /// 设备ID 音箱id,唯一，由客户端生成
        /// </summary>
        public string deviceId { get; set; }
        /// <summary>
        /// 1111为登录成功，1112为登录失败
        /// </summary>
        public string actionId { get; set; }

    }
}
