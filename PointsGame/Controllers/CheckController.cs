using Microsoft.AspNetCore.Mvc;
using PointsGame.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Controllers
{
    /// <summary>
    /// 用于容器确认服务存活
    /// </summary>
    [Route("[controller]")]
    public class CheckController : Controller
    {       
        public CheckController(HellperPush hellperPush)
        {
            _hellperPush = hellperPush ?? throw new ArgumentNullException(nameof(hellperPush));
        }

        private readonly HellperPush _hellperPush;

        /// <summary>
        /// 邮件测试
        /// </summary>
        /// <returns></returns>
        [HttpGet("email/{address}")]
        public ActionResult EmailTest([FromRoute] string address)
        {
            //_hellperPush.SendEmail("积分系统紧急通知【神秘彩蛋一】",
            //                                   $"尊敬的勇士您好：",
            //                                   $"目前已有超过一半的人触发了《神秘彩蛋一》，为了公平，K先生现在向所有人公布《神秘彩蛋一》：为了鼓励大家早上早到，在每天的早上6点到9点之间重新登录积分系统可获得10K币，保持登录不算哦。--K先生",
            //                                   new List<string> { address });
            return Content("OK");
        }
        /// <summary>
        /// push测试
        /// </summary>
        /// <returns></returns>
        [HttpGet("push/{userId}")]
        public ActionResult PushTest([FromRoute] string userId)
        {
            //_hellperPush.SendEmpPush("积分系统紧急通知【神秘彩蛋一】",                                               
            //                                   $"尊敬的勇士您好：\r\n目前已有超过一半的人触发了《神秘彩蛋一》，为了公平，K先生现在向所有人公布《神秘彩蛋一》：为了鼓励大家早上早到，在每天的早上6点到9点之间重新登录积分系统可获得10K币，保持登录不算哦。--K先生",
            //                                   new List<string> { userId });
            return Content("OK");
        }

        /// <summary>
        /// 用于容器确认服务存活
        /// </summary>
        /// <returns></returns>
        [HttpHead]
        [HttpGet]
        public ActionResult Index()
        {
            return Content("OK");            
        }

        /// <summary>
        /// 获取服务器当前时间
        /// </summary>
        /// <returns></returns>
        [HttpGet("time")]
        public ActionResult Time()
        {
            return Content(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }


        /// <summary>
        /// 抽奖测试
        /// </summary>
        /// <returns></returns>
        [HttpGet("prize")]
        public ActionResult Prize()
        {
            //奖品以及中奖率
            const string prizeString = "奖品一:1,奖品二:1,奖品三:1,奖品四:2,奖品五:3,奖品六:10,奖品七:22,奖品八:60";
            //将中奖率*100再取整,放在数组中,并从小到大排序
            var prizeArray = prizeString.Split(',').Select(j => new { Prize = j.Split(':')[0], Change = (int)(double.Parse(j.Split(':')[1])) }).OrderBy(j => j.Change).ToArray();
            //将中奖率累加,放到字典里
            var prizeDictionary = new Dictionary<string, int>();
            for (var i = 0; i < 8; i++)
            {
                var allChange = 0;
                for (var j = 0; j <= i; j++)
                {
                    allChange += prizeArray[j].Change;
                }
                prizeDictionary.Add(prizeArray[i].Prize, allChange);
            }
            //产生一个1-10000的随机数
            var rd = new Random();
            var rdChange = rd.Next(1, 101);
            //找第一个大于随机值的奖项
            return Content((prizeDictionary.FirstOrDefault(j => j.Value >= rdChange).Key?? prizeDictionary.LastOrDefault().Key));
        }
    }
}
