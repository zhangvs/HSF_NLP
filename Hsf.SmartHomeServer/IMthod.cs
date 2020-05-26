using Hsf.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsf.SmartHomeServer
{
    public interface IMthod
    {
        string SendMsg(SoundBodyRequest body);
        string ReceiveMsg(string msg);
    }
}
