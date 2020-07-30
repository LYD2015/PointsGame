using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Response
{
    public class LotteryMyBetResponse
    {
        /// <summary>
        /// 是否开奖(false-未开奖,true-已开奖)
        /// </summary>
        public bool IsRunlottery { get; set; }
        /// <summary>
        /// 期数
        /// </summary>
        public string NumberPeriods { get; set; }
        /// <summary>
        /// 投注号码
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 投注K币数
        /// </summary>
        public int? Score { get; set; }
        /// <summary>
        /// 中奖号码(没有中奖该字段为null,如中奖了该字段为“1,2,3”类似的)
        /// </summary>
        public string WinningNumber { get; set; }
        /// <summary>
        /// 中奖K币数(没中奖为null)
        /// </summary>
        public int? WinningScore { get; set; }
        /// <summary>
        /// 创建时间(投注时间)
        /// </summary>
        public string CreateTime { get; set; }
    }
}
