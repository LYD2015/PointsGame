using Newtonsoft.Json;
using PointsGame.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PointsGame.Dto.Response
{
    /// <summary>
    /// K币任务项
    /// </summary>
    public class TaskItemResponse
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        [JsonIgnore]
        public string Uuid { get; set; }

        /// <summary>
        /// 任务ID
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
        /// 积分数（总K币）
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 发布人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 发布(创建)时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 开抢时间
        /// </summary>
        public DateTime LootTime { get; set; }

        /// <summary>
        /// 任务状态(0-未开抢,1-未认领,2-进行中,3-已完结)
        /// </summary>
        public int TaskState { get; set; }
        /// <summary>
        /// 任务状态文案
        /// </summary>
        [NotMapped]
        public string TaskStateString { get => HelperConvert.TaskStateToString(this.TaskState); }
        /// <summary>
        /// 任务用户
        /// </summary>
        [NotMapped]
        public IEnumerable<TaskUserDetails5> TaskUsers { get; set; }
        /// <summary>
        /// 任务用户详情
        /// </summary>
        public class TaskUserDetails5
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
