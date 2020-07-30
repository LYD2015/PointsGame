using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Models
{
    public class GiftInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        [Key]
        public string Id { get; set; }
        /// <summary>
        /// 赛季ID
        /// </summary>
        [Required(ErrorMessage = "PeriodId必填")]
        public string PeriodId { get; set; }
        /// <summary>
        /// 奖品名称
        /// </summary>
        [Required(ErrorMessage = "Name必填")]
        [StringLength(20,ErrorMessage ="奖品名称不能超过20个字")]
        public string Name { get; set; }
        /// <summary>
        /// 奖品剩余数量(-1表示不限)
        /// </summary>
        [Required(ErrorMessage = "Number必填")]
        public int Number { get; set; }
        /// <summary>
        /// 奖品图片地址
        /// </summary>
        [Required(ErrorMessage = "ImageUrl必填")]
        public string ImageUrl { get; set; }
        /// <summary>
        /// 中奖几率
        /// </summary>
        [Required(ErrorMessage = "Odds必填")]
        public int Odds { get; set; }
        /// <summary>
        /// 是否可以直接兑换
        /// </summary>
        [Required(ErrorMessage = "IsGet必填")]
        public bool IsGet { get; set; } = false;
        /// <summary>
        /// 兑换所需积分
        /// </summary>
        [Required(ErrorMessage = "GetScore必填")]
        public int GetScore { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Order { get; set; }
    }
}
