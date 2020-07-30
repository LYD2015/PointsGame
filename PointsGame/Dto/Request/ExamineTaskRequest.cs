using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    /// <summary>
    /// 任务审核请求体
    /// </summary>
    public class ExamineTaskRequest
    {
        /// <summary>
        /// 任务Id
        /// </summary>
        public string TaskId { get; set; }

        /// <summary>
        /// 是否同意(true同意,false不同意)
        /// </summary>
        public bool IsOk { get; set; }

        /// <summary>
        /// 审核备注
        /// </summary>
        public string ExamineMemo { get; set; }
    }
}
