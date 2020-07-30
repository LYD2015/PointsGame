using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Models
{
    /// <summary>
    /// 赛季信息实体
    /// </summary>
    public class Period
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        [Key]
        [MaxLength(127)]
        public string Id { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// 赛季名称
        /// </summary>
        [MaxLength(50)]
        public string Caption { get; set; }

        /// <summary>
        /// 说明备注
        /// </summary>
        [MaxLength(1000)]
        public string Memo { get; set; }

        /// <summary>
        /// 玩法说明链接
        /// </summary>
        [MaxLength(500)]
        public string PlayingUrl { get; set; }

        /// <summary>
        /// 系统说明链接
        /// </summary>
        [MaxLength(500)]
        public string SystemUrl { get; set; }

        /// <summary>
        /// 状态 0-未开始,1-进行中,2-已结束
        /// </summary>
        public int State { get; set; }

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
        public DateTime? UpdateTime { get; set; }

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
