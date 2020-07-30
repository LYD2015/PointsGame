using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PointsGame.Models
{
    /// <summary>
    /// 彩票开奖结果
    /// </summary>
    public class LotteryResult
    {
        /// <summary>
        /// Key
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 期数：(目前设定每天4期)000001、000002、000003、000004
        /// </summary>
        public string NumberPeriods { get; set; }
        /// <summary>
        /// 开奖号码：逗号分隔
        /// </summary>
        [Required]
        public string Number { get; set; }      
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 中奖情况
        /// </summary>
        public string WinResult { get; set; }
    }
}
