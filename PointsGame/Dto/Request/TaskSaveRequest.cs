using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request {
    /// <summary>
    /// 保存任务信息请求对象
    /// </summary>
    public class TaskSaveRequest {
        /// <summary>
        /// ID 修改任务时需要才传
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 赛季ID
        /// </summary>
        [Required(ErrorMessage = "赛季ID不能为空")]
        public string PeriodId { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        [Required(ErrorMessage = "任务名称不能为空")]
        public string TaskName { get; set; }

        /// <summary>
        /// 任务描述
        /// </summary>
        [Required(ErrorMessage = "任务描述不能为空")]
        public string TaskTntro { get; set; }

        /// <summary>
        /// 需要人数
        /// </summary>
        [Required(ErrorMessage = "需要人数不能为空")]
        [Range(1, 99, ErrorMessage = "用户需要人数只能是1-99")]
        public int? UserNumber { get; set; }

        /// <summary>
        /// 总K币数
        /// </summary>
        [Required(ErrorMessage = "总K币数不能为空")]
        public int? Score { get; set; }

        /// <summary>
        /// 开抢时间
        /// </summary>
        [Required(ErrorMessage = "开抢时间不能为空")]
        public DateTime? LootTime { get; set; }

        /// <summary>
        /// 印象标签 逗号分隔
        /// </summary>               
        public string Labels { get; set; }

    }
}
