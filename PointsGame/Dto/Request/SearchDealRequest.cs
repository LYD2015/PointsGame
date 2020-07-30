﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    /// <summary>
    /// 交易列表查询请求体
    /// </summary>
    public class SearchDealRequest
    {
        /// <summary>
        /// 赛季ID
        /// </summary>
        [MaxLength(127)]
        [Required]
        public string PeriodId { get; set; }


        /// <summary>
        /// 交易标题
        /// </summary>
        public string Theme { get; set; }

        /// <summary>
        /// 交易用户姓名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 交易对象用户姓名
        /// </summary>
        public string DealUserName { get; set; }

        /// <summary>
        /// 交易类型 1-支出,2-收入
        /// </summary>
        public int? DealType { get; set; }

        /// <summary>
        /// 交易开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 交易结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 页码(从零开始)
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 每页条数
        /// </summary>
        public int PageSize { get; set; }
    }
}