using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Response
{
    /// <summary>
    /// K币派发列表查询返回体
    /// </summary>
    public class SearchAllocateResponse
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
        /// 派发标题
        /// </summary>
        public string Theme { get; set; }

        /// <summary>
        /// 派发说明
        /// </summary>
        public string Memo { get; set; }

        /// <summary>
        /// K币数(积分数)
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 派发用户ID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 派发用户姓名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 派发对象用户ID
        /// </summary>
        public string DealUserId { get; set; }

        /// <summary>
        /// 派发对象用户姓名
        /// </summary>
        public string DealUserName { get; set; }

        /// <summary>
        /// 印象标签
        /// </summary>
        public string Labels { get; set; }

        /// <summary>
        /// 派发时间
        /// </summary>
        public DateTime CreateTime { get; set; }

    }
}
