using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Models
{
    public class PeriodGift
    {
        /// <summary>
        /// Id
        /// </summary>
        [Key]
        public string Id { get; set; }
        /// <summary>
        /// 赛季ID
        /// </summary>
        [Required(ErrorMessage = "PeriodId必填")]
        public string PeriodId { get; set; }
        /// <summary>
        /// 本赛季抽奖一次需要多少积分
        /// </summary>
        [Required(ErrorMessage = "Score必填")]
        public int Score { get; set; }
        /// <summary>
        /// 备注说明
        /// </summary>
        public string Memo { get; set; }
        /// <summary>
        /// 启用禁用(true-启用，false-禁用)
        /// </summary>
        public bool Enabled { get; set; }
    }
}
