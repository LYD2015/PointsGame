using PointsGame.Dto.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    public class MyTaskRequest : PagingRequest
    {
        /// <summary>
        /// 赛季ID
        /// </summary>
        [MaxLength(127)]
        [Required]
        public string PeriodId { get; set; }


        /// <summary>
        /// 任务名称
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// 发布人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 任务状态（0-未开抢,1-未领完,2-进行中,3-已完结）
        /// </summary>
        public int? TaskState { get; set; }

        /// <summary>
        /// 发布开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 发布结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }


    }
}
