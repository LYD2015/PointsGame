using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Response
{
    /// <summary>
    /// 赛季管理请求体
    /// </summary>
    public class PeriodReponse
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
        /// 状态文案
        /// </summary>
        public string StateString { get; set; }
        /// <summary>
        /// 抽奖是否可用
        /// </summary>
        public bool GiftEnabeld { get; set; } = false;
    }
}
