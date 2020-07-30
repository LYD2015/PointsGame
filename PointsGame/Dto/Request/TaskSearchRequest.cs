using PointsGame.Dto.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    /// <summary>
    /// 任务搜索请求
    /// </summary>
    public class TaskSearchRequest : PagingRequest
    {
        /// <summary>
        /// 赛季ID
        /// </summary>
        [Required(ErrorMessage = "赛季ID不能为空")]
        public string PeriodId { get; set; }
        // 191209-任务名称、发布人、发布时间、任务状态搜索
        /// <summary>
        /// 任务名称
        /// </summary>
        public string TaskName { get; set; }
        /// <summary>
        /// 发布人姓名
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建时间-开始（CreateTime greater than or equal to CreateTimeStart）
        /// </summary>
        public DateTime? CreateTimeStart { get; set; }
        /// <summary>
        /// 创建时间-结束（CreateTime less than CreateTimeStart）
        /// </summary>
        public DateTime? CreateTimeEnd { get; set; }
        /// <summary>
        /// 任务状态(0-未开抢,1-未认领,2-进行中,3-已完结)
        /// </summary>
        public int? TaskState { get; set; }
    }
}
