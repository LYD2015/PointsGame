using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Models
{
    /// <summary>
    /// 任务信息实体
    /// </summary>
    public class TaskInfo
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        [Key]
        [MaxLength(127)]
        public string Id { get; set; }

        /// <summary>
        /// 赛季ID
        /// </summary>
        [MaxLength(127)]
        public string PeriodId { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        [MaxLength(50)]
        public string TaskName { get; set; }

        /// <summary>
        /// 任务描述
        /// </summary>
        [MaxLength(5000)]
        public string TaskTntro { get; set; }

        /// <summary>
        /// 用户需要人数
        /// </summary>
        public int UserNumber { get; set; }

        /// <summary>
        /// 分数
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 开抢时间
        /// </summary>
        public DateTime LootTime { get; set; }

        /// <summary>
        /// 任务状态（0-未开抢,1-未领完,2-进行中,3-已完结）
        /// </summary>
        public int TaskState { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? ExamineTime { get; set; }

        /// <summary>
        /// 审核人ID
        /// </summary>
        [MaxLength(127)]
        public string ExamineUser { get; set; }

        /// <summary>
        /// 审核状态（1-待审核,2-同意,3-不同意）
        /// </summary>
        public int ExamineState { get; set; }

        /// <summary>
        /// 审核备注
        /// </summary>
        [MaxLength(100)]
        public string ExamineMemo { get; set; }

        /// <summary>
        /// 印象标签 逗号分隔
        /// </summary>
        [MaxLength(50)]
        public string Labels { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        [MaxLength(127)]
        public string CreateUser { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { get; set; }
    }
}
