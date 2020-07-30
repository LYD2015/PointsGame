using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Models
{
    /// <summary>
    /// 动态信息表实体
    /// </summary>
    public class DynamicInfo
    {
        /// <summary>
        /// 关键值ID
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
        /// 用户ID
        /// </summary>
        [MaxLength(127)]
        public string UserId { get; set; }
        /// <summary>
        /// 用户全名
        /// </summary>
        [MaxLength(50)]
        public string UserFullName { get; set; }
        /// <summary>
        /// 对应数据ID
        /// </summary>
        [MaxLength(127)]
        public string DataId { get; set; }
        /// <summary>
        /// 动态类型(交易支出 1,交易收入2,任务收入3,派发收入4,任务发布5,任务抢到6,任务完成7)
        /// </summary>
        public int DynamicType { get; set; }
        /// <summary>
        /// 动态内容
        /// </summary>
        [MaxLength(100)]
        public string DynamicContent { get; set; }
        /// <summary>
        /// 动态形容词
        /// </summary>
        [MaxLength(20)]
        public string Adjective { get; set; }
        /// <summary>
        /// 积分
        /// </summary>
        public int Score { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
