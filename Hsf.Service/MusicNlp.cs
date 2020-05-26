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
    public class MusicNlp : INlp
    {
        
        public NlpAnswers SendMsg(SoundBodyRequest body)
        {
            NlpAnswers semanticsSlot = new NlpAnswers(){
                Code = "MusicNlp",
                Name = "音乐",
                Answers = "播放《卷珠帘》",
            };
            return semanticsSlot;
        }
        public string CallbackMsg(AIAnswers semantics)
        {
            return $"已为您播放《卷珠帘》";
        }
    }
}
