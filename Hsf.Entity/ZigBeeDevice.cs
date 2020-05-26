using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsf.Entity
{
    public class ZigBeeDevice
    {
        /// <summary>
        /// 
        /// </summary>
        public long timestamp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<DeviceItem> device { get; set; }
    }

    public class DeviceItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int ep { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int pid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int did { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ol { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string dn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int dtype { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string fac { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int ztype { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string dsp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string swid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public St st { get; set; }
    }

    public class St
    {
        /// <summary>
        /// 
        /// </summary>
        public string fac { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string on { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string eplist { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string dsp { get; set; }
    }


}
