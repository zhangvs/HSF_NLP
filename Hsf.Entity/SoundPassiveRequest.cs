using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsf.Model
{
    /// <summary>
    /// 主动请求
    /// </summary>
    public class SoundPassiveRequest
    {
        /// <summary>
        /// 每次会话id,唯一，由客户端生成
        /// </summary>
        public string sessionId { get; set; }
        /// <summary>
        /// websocket请求音箱id
        /// </summary>
        public string deviceId { get; set; }
        /// <summary>
        ///音箱需要执行的动作id，例如跑马灯指定特效、播放url
        ///301播放url内容，播放完成后自动唤醒
        ///302播放url内容，播放完成后自动休眠
        ///305指示灯特效，参数在req中
        ///306音量增加一档
        ///307音量减少一档
        ///308音量设定百分比，参数在req中
        ///309代码唤醒音箱
        ///310暂停当前音箱媒体播放
        ///311继续当前音箱媒体播放
        ///312停止当前音箱媒体播放
        /// </summary>
        public string actionId { get; set; }
        /// <summary>
        /// 登录注册成功时，收到的返回
        /// </summary>
        public string token { get; set; }
        /// <summary>
        /// websocket请求百度的文本
        /// </summary>
        public string request { get; set; }
        /// <summary>
        /// 播放媒体地址，如果有值则读取，没有就继续原有流程
        /// </summary>
        public string url { get; set; }
        /// <summary>
        /// 播放完url媒体资源后，是否自动唤醒，字符串格式：0或者1
        /// </summary>
        public string blwakeup { get; set; }
        /// <summary>
        /// //控制参数，例如音量值，跑马灯效果值等等
        /// </summary>
        public string req { get; set; }
    }


    /// <summary>
    /// 主动发送返回
    /// </summary>
    public class SoundToBack
    {
        /// <summary>
        /// 每次会话id,唯一，由客户端生成
        /// </summary>
        public string sessionId { get; set; }
        /// <summary>
        /// 3001代表收到web发起请求
        /// </summary>
        public string actionId { get; set; }
        /// <summary>
        /// 登录注册成功时，收到的返回
        /// </summary>
        public string token { get; set; }
        /// <summary>
        /// websocket请求百度的文本
        /// </summary>
        public string request { get; set; }
        /// <summary>
        /// 其他参数存放地方
        /// </summary>
        public string req { get; set; }
    }
}
