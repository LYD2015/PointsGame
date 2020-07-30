using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    public class GiftAddRequest
    {
        /// <summary>
        /// 关键值(新增不传,修改传)
        /// </summary>
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
        [StringLength(20, ErrorMessage = "奖品名称不能超过20个字")]
        public string Name { get; set; }
        /// <summary>
        /// 奖品数量(-1:表示不限数量)
        /// </summary>
        [Required(ErrorMessage = "Number必填")]
        public int Number { get; set; }
        /// <summary>
        /// 奖品图片地址
        /// </summary>
        [Required(ErrorMessage = "ImageUrl必填")]
        public string ImageUrl { get; set; }
        /// <summary>
        /// 中奖几率(0-100)
        /// </summary>
        [Required(ErrorMessage = "Odds必填")]
        [Range(0,1000,ErrorMessage ="中奖几率只能是0-1000")]
        public int Odds { get; set; }
        /// <summary>
        /// 是否可以兑换
        /// </summary>
        [Required(ErrorMessage = "IsGet必填")]
        public bool IsGet { get; set; } = false;
        /// <summary>
        /// 兑换所需积分
        /// </summary>
        [Required(ErrorMessage = "GetScore必填")]
        public int GetScore { get; set; }     
        /// <summary>
        /// 排序
        /// </summary>
        public int Order { get; set; }
    }
}
