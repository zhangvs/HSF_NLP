using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsf.Entity
{
    public class SoundRedisBody
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public string talkid { get; set; }
        /// <summary>
        /// 每次会话id,唯一，由客户端生成
        /// </summary>
        public string titleid { get; set; }
        /// <summary>
        /// 音箱id,唯一，由客户端生成
        /// </summary>
        public string timestamp { get; set; }
        /// <summary>
        /// 问题列表
        /// </summary>
        public string[] questions { get; set; }
        /// <summary>
        /// 回答列表
        /// </summary>
        public string[] answers { get; set; }
        /// <summary>
        /// 领域
        /// </summary>
        public string field { get; set; }
        /// <summary>
        /// 会话状态
        /// </summary>
        public string state { get; set; }
    }
}
