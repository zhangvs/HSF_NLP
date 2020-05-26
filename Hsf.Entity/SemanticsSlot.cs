using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsf.Model
{
    /// <summary>
    /// 语义槽
    /// </summary>
    public class SemanticsSlot
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
        /// 音箱资源id
        /// </summary>
        public string SourceId { get; set; }
        /// <summary>
        /// 发送内容
        /// </summary>
        public string Questions { get; set; }
        /// <summary>
        /// 回答次数
        /// </summary>
        public int Answertimes { get; set; }
        /// <summary>
        /// NLP返回的答案集合
        /// </summary>
        public List<NlpAnswers> NlpAnswers { get; set; }
        /// <summary>
        /// 结束标识位：0:可以继续放入，-1：超时部分完成，1：全部放入完成
        /// </summary>
        public int State { get; set; }
    }

    /// <summary>
    /// NLP返回的答案
    /// </summary>
    public class NlpAnswers
    {
        /// <summary>
        /// 发送到的NLP服务器
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 发送到的NLP服务器
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 接收到的语义
        /// </summary>
        public string Answers { get; set; }
        /// <summary>
        /// 语义处理级别
        /// </summary>
        public int Level { get; set; }
        ///// <summary>
        ///// 回调是否默认执行 true 直接执行，false 等待确认再执行
        ///// </summary>
        //public bool  IsCallBack { get; set; }
        ///// <summary>
        ///// 回调执行的具体命令
        ///// </summary>
        //public string CallBackAction { get; set; }
    }



}
