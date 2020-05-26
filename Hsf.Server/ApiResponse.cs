using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsf.Server
{
    public class ApiResponse
    {
        private int 心跳;

        public ApiResponse(int 心跳)
        {
            this.心跳 = 心跳;
        }
        public Dictionary<string, object> data { get; set; }
    }
}
