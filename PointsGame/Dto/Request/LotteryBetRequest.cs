using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    /// <summary>
    /// 彩票投注请求体
    /// </summary>
    public class LotteryBetRequest
    {
        /// <summary>
        /// 赛季ID
        /// </summary>
        [Required]
        public string PeriodId { get; set; }

        /// <summary>
        /// 投注列表
        /// </summary>
        [Required]
        public List<BetNumbers> BetNumberList { get; set; }
       
       
    }
    /// <summary>
    /// 投注信息
    /// </summary>
    public class BetNumbers
    {
        /// <summary>
        /// 投注号码列表
        /// </summary>
        [Required]
        public List<int> BetNumber { get; set; }
        /// <summary>
        /// 投注K币
        /// </summary>
        [Required]        
        public int? BetScore { get; set; }
    }
}
