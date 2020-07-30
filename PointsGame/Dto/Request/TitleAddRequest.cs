using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    /// <summary>
    /// 称号新增请求体
    /// </summary>
    public class TitleAddRequest
    {    
        /// <summary>
        /// 赛季ID
        /// </summary>
        [MaxLength(127)]
        [Required]
        public string PeriodId { get; set; }
        /// <summary>
        /// 开始积分
        /// </summary>
        [Required]
        public int StartScore { get; set; }

        /// <summary>
        /// 结束积分
        /// </summary>
        [Required]
        public int EndScore { get; set; }

        /// <summary>
        /// 称号名称
        /// </summary>
        [MaxLength(20)]
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// 图标地址
        /// </summary>
        [MaxLength(500)]
        [Required]
        public string Icon { get; set; }

        /// <summary>
        /// 图片地址
        /// </summary>
        [MaxLength(500)]
        [Required]
        public string Card { get; set; }

        /// <summary>
        /// 字体颜色(个人等级那里使用)
        /// </summary>
        [MaxLength(50)]        
        public string FontColor { get; set; }
    }
}
