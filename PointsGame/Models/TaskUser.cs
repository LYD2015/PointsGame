using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Models
{
    /// <summary>
    /// 任务用户实体(完成任务的人员)
    /// </summary>
    public class TaskUser
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [MaxLength(127)]
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// 任务ID
        /// </summary>
        [MaxLength(127)]
        [Required]
        public string TaskId { get; set; }

        /// <summary>
        /// 领取任务时间
        /// </summary>
        [Required]
        public DateTime GetTime { get; set; }
    }
}
