using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Models
{
    /// <summary>
    /// 积分信息实体
    /// </summary>
    public class ScoreInfo
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        [Key]
        [MaxLength(127)]
        public string Id { get; set; }

        /// <summary>
        /// 赛季ID
        /// </summary>
        [MaxLength(127)]
        public string PeriodId { get; set; }

        /// <summary>
        /// 等级积分
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 可消费积分
        /// </summary>
        public int ConsumableScore { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [MaxLength(127)]
        public string UserId { get; set; }      

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        [MaxLength(127)]
        public string CreateUser { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 修改人ID
        /// </summary>
        [MaxLength(127)]
        public string UpdateUser { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { get; set; }       
    }
}
