using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    public class PeriodGiftMemoRequest
    {
        /// <summary>
        /// 赛季ID
        /// </summary>
        [Required(ErrorMessage = "PeriodId 必填")]
        public string PeriodId { get; set; }
        /// <summary>
        /// 本赛季抽奖一次需要多少积分
        /// </summary>
        [Required(ErrorMessage = "Score 必填")]
        public int Score { get; set; }
        /// <summary>
        /// 备注说明
        /// </summary>
        public string Memo { get; set; }
        /// <summary>
        /// 启用禁用(1-启用，0-禁用)
        /// </summary>
        public bool Enabled { get; set; } = false;
    }
}
