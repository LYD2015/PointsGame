using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Response
{
    /// <summary>
    /// 排行榜返回体
    /// </summary>
    public class SearchTopResponse
    {
        /// <summary>
        /// 当前排名
        /// </summary>
        public int TopNumber { get; set; }
        /// <summary>
        /// 本系统的用户ID
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 用户姓名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        public string OrganizationName { get; set; }
        /// <summary>
        /// 组别
        /// </summary>
        public string GroupName { get; set; }
        /// <summary>
        /// 称号
        /// </summary>
        public string ScoreTitle { get; set; }
        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// 图片地址
        /// </summary>
        public string Card { get; set; }
        /// <summary>
        /// 当前等级积分
        /// </summary>
        public int? Score { get; set; }
        /// <summary>
        /// 可消费积分
        /// </summary>
        public int? ConsumableScore { get; set; }
        /// <summary>
        /// 距下一等级记还差多少分
        /// </summary>
        public int? NextScore { get; set; }
        /// <summary>
        /// 印象标签(前端根据显示效果可以选择取前几个显示)
        /// </summary>
        public List<LabelList> LabelList { get; set; }
        /// <summary>
        /// 字体颜色(个人等级那里使用)
        /// </summary>
        public string FontColor { get; set; }
    }
    /// <summary>
    /// 印象标签以及对应标签的次数
    /// </summary>
    public class LabelList
    {
        /// <summary>
        /// 印象标签
        /// </summary>
        public string Label { get; set; }
        /// <summary>
        /// 印象标签次数
        /// </summary>
        public int LabelCount { get; set; }
    }
}
