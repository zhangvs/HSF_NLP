using Hsf.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsf.Interface
{
    public interface INlp
    {
        //发送语义
        NlpAnswers SendMsg(SoundBodyRequest msg);

        //执行语义回调
        string CallbackMsg(SingleAnswers _SingleAnswers);
    }
}
