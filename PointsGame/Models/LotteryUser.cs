using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PointsGame.Models
{
    /// <summary>
    /// 用户投注情况
    /// </summary>
    public class LotteryUser
    {
        /// <summary>
        /// Key
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 赛季ID
        /// </summary>
        public string PeriodId { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 期数：(目前设定每天4期)000001、000002、000003、000004
        /// </summary>
        public string NumberPeriods { get; set; }
        /// <summary>
        /// 投注号码：逗号分隔
        /// </summary>
        [Required]
        public string Number { get; set; }       
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 投注K币
        /// </summary>
        [Required]
        [MaxLength(11)]
        public int? Score { get; set; }
        /// <summary>
        /// 中奖号码：中了才有，如：1,2,13
        /// </summary>
        public string WinningNumber { get; set; }
        /// <summary>
        /// 中奖K币数
        /// </summary>
        [MaxLength(11)]
        public int? WinningScore { get; set; }
    }
}
