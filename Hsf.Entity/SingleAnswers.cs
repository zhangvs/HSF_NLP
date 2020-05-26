using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsf.Model
{
    /// <summary>
    /// AI处理结果
    /// </summary>
    public class SingleAnswers
    {
        /// <summary>
        /// 请求SessionId
        /// </summary>
        public string SessionId { get; set; }
        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceId { get; set; }
        /// <summary>
        /// 发送内容
        /// </summary>
        public string Questions { get; set; }
        /// <summary>
        /// 发送到的NLP服务器
        /// </summary>
        public string NlpCode { get; set; }
        /// <summary>
        /// 发送到的NLP服务器
        /// </summary>
        public string NlpName { get; set; }
        /// <summary>
        /// 接收到的语义
        /// </summary>
        public string NlpAnswers { get; set; }
        ///// <summary>
        ///// 是否默认执行 true 直接执行，false 等待确认再执行
        ///// </summary>
        //public bool  IsCallBack { get; set; }
        ///// <summary>
        ///// 回调执行的具体命令
        ///// </summary>
        //public string CallBackAction { get; set; }
    }
}
