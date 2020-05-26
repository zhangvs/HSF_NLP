using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsf.Model
{
    public class SoundHost
    {
        /// <summary>
        /// 
        /// </summary>
        public string NewRecord { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int _id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string chinaname { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string classfid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string devalarm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string devchannel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string deviceid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string devip { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string devmac { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Devpara devpara { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string devport { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string devposition { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string devregcode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string devstate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string devstate1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string devstate2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string devtype { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string imageid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int lgsort { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string powvalue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string userid { get; set; }
    }

    public class Devpara
    {
        /// <summary>
        /// 关闭,211,1,192.168.88.102
        /// </summary>
        public string close { get; set; }
        /// <summary>
        /// 开启,211,1,192.168.88.102
        /// </summary>
        public string open { get; set; }
    }
}
