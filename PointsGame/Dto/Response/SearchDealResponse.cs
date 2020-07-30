using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Response
{
    /// <summary>
    /// 交易列表查询返回体
    /// </summary>
    public class SearchDealResponse
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
        /// 交易标题
        /// </summary>
        public string Theme { get; set; }

        /// <summary>
        /// 交易说明
        /// </summary>
        public string Memo { get; set; }

        /// <summary>
        /// K币数(积分数)
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 交易用户ID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 交易用户姓名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 交易对象用户ID
        /// </summary>
        public string DealUserId { get; set; }

        /// <summary>
        /// 交易对象用户姓名
        /// </summary>
        public string DealUserName { get; set; }

        /// <summary>
        /// 交易编号 (配对的)
        /// </summary>
        public string DealNumber { get; set; }

        /// <summary>
        /// 交易类型 1-支出,2-收入
        /// </summary>
        public int DealType { get; set; }

        /// <summary>
        /// 交易时间
        /// </summary>
        public DateTime CreateTime { get; set; }

    }
}
