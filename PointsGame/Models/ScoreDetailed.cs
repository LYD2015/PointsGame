using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Models
{
    /// <summary>
    /// 积分明细实体
    /// </summary>
    public class ScoreDetailed
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
        public string PeriodId { get;set; }

        /// <summary>
        /// 积分信息ID
        /// </summary>
        [MaxLength(127)]
        public string ScoreId { get; set; }

        /// <summary>
        /// 交易用户ID
        /// </summary>
        [MaxLength(127)]
        public string UserId { get; set; }
               
        /// <summary>
        /// 交易对象用户ID
        /// </summary>
        [MaxLength(50)]
        public string DealUserId { get; set; }
        
        /// <summary>
        /// K币数(积分数)
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 交易编号 就一个GUID,交易类型1和2的是一样的编号(配对的)
        /// </summary>
        [MaxLength(127)]
        public string DealNumber { get; set; }

        /// <summary>
        /// 交易类型 1-交易支出,2-交易收入,3-任务收入,4-派发收入
        /// </summary>
        public int DealType { get; set; }

        /// <summary>
        /// 主题(派发和自由交易时填写,任务产生时填任务名称)
        /// </summary>
        [MaxLength(50)]
        public string Theme { get; set; }

        /// <summary>
        /// 说明(派发和自由交易的时候必填)
        /// </summary>
        [MaxLength(255)]
        public string Memo { get; set; }

        /// <summary>
        /// 任务ID
        /// </summary>
        [MaxLength(127)]
        public string TaskId { get; set; }

        /// <summary>
        /// 印象标签 逗号分隔 派发积分时才有
        /// 每个标签最长4个汉字
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
