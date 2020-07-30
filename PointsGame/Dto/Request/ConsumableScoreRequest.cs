using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    public class ConsumableScoreRequest
    {
        /// <summary>
        /// 赛季ID
        /// </summary>
        [Required(ErrorMessage = "PeriodId不能为空")]
        public string PeriodId { get; set; }
        /// <summary>
        /// 消费方式(1-抽奖,2-兑奖)
        /// </summary>
        public int ConsumableType { get; set; } = 1;
        /// <summary>
        /// 奖品ID,兑奖的时候传
        /// </summary>
        public string GiftId { get; set; }
    }
}
