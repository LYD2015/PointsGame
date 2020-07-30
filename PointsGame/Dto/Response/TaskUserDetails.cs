using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Response
{
    /// <summary>
    /// 任务用户详情
    /// </summary>
    public class TaskUserDetails
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
