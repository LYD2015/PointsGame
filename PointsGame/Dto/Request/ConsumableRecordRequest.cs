using PointsGame.Dto.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    public class ConsumableRecordRequest: PagingRequest
    {
        /// <summary>
        /// 赛季ID
        /// </summary>
        [Required(ErrorMessage = "PeriodId不能为空")]
        public string PeriodId { get; set; }
        /// <summary>
        /// 搜索的名字(为空查询全部)
        /// </summary>
        public string UserNmae { get; set; }
    }
}
