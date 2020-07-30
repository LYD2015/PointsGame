using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    /// <summary>
    /// 新增赛季
    /// </summary>
    public class CreatePeriodRequest
    {
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
        /// 玩法说明
        /// </summary>
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
    }
}
