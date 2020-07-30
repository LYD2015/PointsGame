using PointsGame.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Response
{
    /// <summary>
    /// 任务审核返回体
    /// </summary>
    public class TasksExamineResponse
    {
        /// <summary>
        /// 任务主键Id
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
        /// 需要人数
        /// </summary>
        public int UserNumber { get; set; }

        /// <summary>
        /// 总K币
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 发布人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 开抢时间
        /// </summary>
        public DateTime LootTime { get; set; }

        /// <summary>
        /// 审核备注
        /// </summary>
        public string ExamineMemo { get; set; }

        /// <summary>
        /// 审核状态（1-待审核,2-通过,3-驳回）
        /// </summary>
        public int ExamineState { get; set; }
        /// <summary>
        /// 审核状态文案
        /// </summary>
        public string ExamineStateString { get => HelperConvert.TaskExamineStateToString(ExamineState); }
        /// <summary>
        /// 审核人
        /// </summary>
        public string ExamineUser { get; set; }

        /// <summary>
        /// 任务用户
        /// </summary>
        public IEnumerable<TaskUserDetails4> TaskUsers { get; set; }

        /// <summary>
        /// 任务用户详情
        /// </summary>
        public class TaskUserDetails4
        {
            /// <summary>
            /// 任务ID
            /// </summary>
            public string TaskId { get; set; }
            /// <summary>
            /// 本系统的用户ID
            /// </summary>
            public string UserId { get; set; }
            /// <summary>
            /// 用户姓名
            /// </summary>
            public string UserName { get; set; }
            /// <summary>
            /// 部门
            /// </summary>
            public string OrganizationName { get; set; }
            /// <summary>
            /// 组别
            /// </summary>
            public string GroupName { get; set; }
            /// <summary>
            /// 称号
            /// </summary>
            public string ScoreTitle { get; set; }
            /// <summary>
            /// 图标
            /// </summary>
            public string Icon { get; set; }
        }
    }
}
