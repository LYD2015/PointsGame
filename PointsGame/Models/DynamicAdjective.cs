using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Models
{
    /// <summary>
    /// 动态形容词库实体
    /// </summary>
    public class DynamicAdjective
    {
        /// <summary>
        /// 关键值
        /// </summary>
        [Key]
        [MaxLength(127)]
        public string Id { get; set; }

        /// <summary>
        /// 动态类型(交易支出 1,交易收入2,任务收入3,派发收入4,任务发布5,任务抢到6,任务完成7)
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 动态词
        /// </summary>
        [MaxLength(10)]
        public string Adjective { get; set; }
    }
}
