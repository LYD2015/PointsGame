using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Models
{
    /// <summary>
    /// 等级称号实体
    /// </summary>
    public class RankTitle
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
        /// 开始积分
        /// </summary>
        public int StartScore { get; set; }

        /// <summary>
        /// 结束积分
        /// </summary>
        public int EndScore { get; set; }

        /// <summary>
        /// 称号
        /// </summary>
        [MaxLength(20)]
        public string Title { get; set; }

        /// <summary>
        /// 小图标
        /// </summary>
        [MaxLength(500)]
        public string Icon { get; set; }

        /// <summary>
        /// 图片地址
        /// </summary>
        [MaxLength(500)]
        public string Card { get; set; }

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

        /// <summary>
        /// 字体颜色(个人等级那里使用)
        /// </summary>
        [MaxLength(50)]
        public string FontColor { get; set; }
    }
}
