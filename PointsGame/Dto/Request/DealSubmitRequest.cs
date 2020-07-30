using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    /// <summary>
    /// 发起交易请求体
    /// </summary>
    public class DealSubmitRequest
    {
        /// <summary>
        /// 赛季ID
        /// </summary>
        [MaxLength(127)]
        [Required]
        public string PeriodId { get; set; }
        /// <summary>
        /// 交易对象用户ID
        /// </summary>
        [MaxLength(127)]
        [Required]
        public string DealUserId { get; set; }
        /// <summary>
        /// 交易的K币数(积分数)
        /// </summary>
        public int Score { get; set; }
        /// <summary>
        /// 交易标题
        /// </summary>
        [MaxLength(25)]
        [Required]
        public string Theme { get; set; }
        /// <summary>
        /// 交易说明
        /// </summary>
        [MaxLength(100)]
        [Required]
        public string Memo { get; set; }
    }
}
