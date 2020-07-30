using PointsGame.Dto.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    /// <summary>
    /// 动态查询请求体
    /// </summary>
    public class SearchDynamicRequest: PagingRequest
    {
        /// <summary>
        /// 赛季ID
        /// </summary>
        [MaxLength(127)]
        [Required]
        public string PeriodId { get; set; }        
    }
}
