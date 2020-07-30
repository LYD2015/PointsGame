using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    /// <summary>
    /// 审核任务列表请求体
    /// </summary>
    public class TaskExamineListRequest
    {
        /// <summary>
        /// 赛季Id
        /// </summary>
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
        /// 发布开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 发布结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 审核状态（1-待审核,2-通过,3-驳回）
        /// </summary>
        public int? ExamineState { get; set; }

        /// <summary>
        /// 分页大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 分页索引
        /// </summary>
        public int PageIndex { get; set; }
    }
}
