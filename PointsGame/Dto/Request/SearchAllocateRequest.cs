using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    /// <summary>
    /// 交易列表查询请求体
    /// </summary>
    public class SearchAllocateRequest
    {
        /// <summary>
        /// 赛季ID
        /// </summary>
        [MaxLength(127)]
        [Required]
        public string PeriodId { get; set; }

   

        /// <summary>
        /// 派发原因
        /// </summary>
        public string Theme { get; set; }

        /// <summary>
        /// 派发用户姓名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 派发对象用户姓名
        /// </summary>
        public string DealUserName { get; set; }
       
        /// <summary>
        /// 派发开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }


        /// <summary>
        /// 派发结束时间
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
