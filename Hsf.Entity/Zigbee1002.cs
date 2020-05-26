using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsf.Entity
{
    public class Zigbee1002
    {
        /// <summary>
        /// 
        /// </summary>
        public int code { get; set; }
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
        public int serial { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Control control { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int result { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string zigbee { get; set; }
    }

    public class Control
    {
        /// <summary>
        /// 
        /// </summary>
        public bool on { get; set; }
    }
}
