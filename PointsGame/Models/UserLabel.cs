using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Models
{
    /// <summary>
    /// 用户印象标签实体
    /// </summary>
    public class UserLabel
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        [Key]
        [MaxLength(127)]
        public string Id { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [MaxLength(127)]
        public string UserId { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        [MaxLength(10)]
        public string Label { get; set; }
    }
}
