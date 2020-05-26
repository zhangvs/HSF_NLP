using Hsf.Model;
using Hsf.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Hsf.Service
{
    /// <summary>
    /// 音响自身的答案
    /// </summary>
    public class SoundNlp : INlp
    {
        public NlpAnswers SendMsg(SoundBodyRequest body)
        {
            NlpAnswers semanticsSlot = new NlpAnswers()
            {
                Code = "SoundNlp",
                Name = "音响",
                Level = 2,
                Answers =body.req.ToString()
            };
            if (!string.IsNullOrEmpty(body.req.ToString()) && !body.req.ToString().Contains("name\":\"Speak"))
            {
                if (body.req.ToString().Contains("askingType\":\"WEATHER"))
                {
                    dynamic result = JsonConvert.DeserializeObject<dynamic>(body.req.ToString());//反序列化
                    //临沂今天阴转多云，4℃～11℃，和昨天差不多，当前空气质量指数170，中度污染，外出记得带上口罩。  description
                    semanticsSlot.Answers = result.directive.payload["description"].Value;
                }
                else
                {
                    semanticsSlot.Answers = body.req.ToString();
                }
            }
            else
            {
                semanticsSlot.Answers = "";
            }
            return semanticsSlot;
        }
        public string CallbackMsg(SingleAnswers _SingleAnswers)
        {
            return $"音响NLP回调内容";
        }
    }
}
