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
    public class BaseNlp : INlp
    {
        public NlpAnswers SendMsg(SoundBodyRequest body)
        {
            NlpAnswers semanticsSlot = new NlpAnswers()
            {
                Code = "BaseNlp",
                Name = "基础",
                Level=0,
            };
            if (body.questions.Contains("几点"))
            {
                semanticsSlot.Answers = "现在时间是" + DateTime.Now.Hour + "点" + DateTime.Now.Minute + "分";
            }
            //else if (body.questions.Contains("天气") && !body.req.ToString().Contains("天气"))
            //{
            //    semanticsSlot.Answers = "今天气温2℃~11℃，天气晴，空气质量状况良";
            //}
            else
            {
                semanticsSlot.Answers = "";
            }
            return semanticsSlot;
        }
        public string CallbackMsg(SingleAnswers _SingleAnswers)
        {
            return $"基础NLP回调内容";
        }
    }
}
