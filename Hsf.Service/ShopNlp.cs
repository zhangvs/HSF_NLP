using Hsf.Model;
using Hsf.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hsf.Service
{
    public class ShopNlp : INlp
    {
        public NlpAnswers SendMsg(SoundBodyRequest body)
        {
            NlpAnswers semanticsSlot = new NlpAnswers()
            {
                Code = "ShopNlp",
                Name = "购物",
                Answers = "购买窗帘",
            };
            return semanticsSlot;
        }
        public string CallbackMsg(AIAnswers _Semantics)
        {
            return $"窗帘购买成功！";
        }
    }
}
