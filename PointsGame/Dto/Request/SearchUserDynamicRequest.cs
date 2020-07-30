using PointsGame.Dto.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    public class SearchUserDynamicRequest: PagingRequest
    {
        /// <summary>
        /// 赛季ID
        /// </summary>
        [MaxLength(127)]
        [Required]
        public string PeriodId { get; set; }
        /// <summary>
        /// 用户Id
        /// </summary>
        [MaxLength(127)]
        [Required]
        public string UserId { get; set; }
    }
}
