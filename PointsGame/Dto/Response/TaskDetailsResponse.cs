using PointsGame.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Response
{
    /// <summary>
    /// 任务详情返回体
    /// </summary>
    public class TaskDetailsResponse
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 赛季Id
        /// </summary>
        public string PeriodId { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// 任务简介
        /// </summary>
        public string TaskTntro { get; set; }

        /// <summary>
        /// 需求人数
        /// </summary>
        public int UserNumber { get; set; }

        /// <summary>
        /// 积分数
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 开抢时间
        /// </summary>
        public string LootTime { get; set; }

        /// <summary>
        /// 领取时间
        /// </summary>
        public string GetTime { get; set; }

        /// <summary>
        /// 任务状态（0-未开抢,1-未领完,2-进行中,3-已完结）
        /// </summary>
        public int TaskState { get; set; }

        /// <summary>
        /// 任务状态文案
        /// </summary>
        public string TaskStateString { get => HelperConvert.TaskStateToString(TaskState); }

        /// <summary>
        /// 审核状态(1-待审核,2-通过,3-驳回)
        /// </summary>
        public int ExamineState { get; set; }

        /// <summary>
        /// 审核状态文案
        /// </summary>
        public string ExamineStateString { get => HelperConvert.TaskExamineStateToString(ExamineState); }

        /// <summary>
        /// 审核备注
        /// </summary>
        public string ExamineMemo { get; set; }

        /// <summary>
        /// 印象标签
        /// </summary>
        public string Labels { get; set; }

        /// <summary>
        /// 发布人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }
    }
}
