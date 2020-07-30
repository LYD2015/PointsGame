using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Response
{
    /// <summary>
    /// 查询称号返回体
    /// </summary>
    public class SearchTitleResponse
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 赛季ID
        /// </summary>
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
        public string Title { get; set; }

        /// <summary>
        /// 小图标
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 图片地址
        /// </summary>
        public string Card { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 修改人ID
        /// </summary>
        public string UpdateUser { get; set; }

        /// <summary>
        /// 字体颜色(个人等级那里使用)
        /// </summary>
        public string FontColor { get; set; }

    }
}
