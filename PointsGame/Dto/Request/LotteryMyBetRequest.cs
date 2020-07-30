using PointsGame.Dto.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    public class LotteryMyBetRequest : PagingRequest
    {
        /// <summary>
        /// 赛季ID
        /// </summary>
        [Required(ErrorMessage = "PeriodId不能为空")]
        public string PeriodId { get; set; }
    }
}
