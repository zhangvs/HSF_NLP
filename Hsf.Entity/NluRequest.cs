using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsf.Model
{
    public class NluRequest
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public string version { get; set; }
        /// <summary>
        /// 每次会话id,唯一，由客户端生成
        /// </summary>
        public string sessionId { get; set; }
        /// <summary>
        /// 音箱id,唯一，由客户端生成
        /// </summary>
        public string deviceId { get; set; }
        /// <summary>
        /// 201 ,      //每次请求动作id，int类型
        /// </summary>
        public string actionId { get; set; }
        /// <summary>
        /// 登录注册成功时，收到的返回
        /// </summary>
        public string token { get; set; }
        /// <summary>
        /// 音箱请求百度的文本
        /// </summary>
        public string questions { get; set; }
        /// <summary>
        /// 音箱数据源id
        /// </summary>
        public string sourceId { get; set; }
        /// <summary>
        /// 百度返回的处理结果 
        /// </summary>
        public object req { get; set; }
    }
}
