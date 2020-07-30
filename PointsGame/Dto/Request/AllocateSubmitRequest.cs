using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    /// <summary>
    /// 新增派发请求体
    /// </summary>
    public class AllocateSubmitRequest
    {
        /// <summary>
        /// 赛季ID
        /// </summary>
        [MaxLength(127)]
        [Required]
        public string PeriodId { get; set; }
        /// <summary>
        /// 派发对象用户ID
        /// </summary>
        [MaxLength(127)]
        [Required]
        public string AllocateUserId { get; set; }
        /// <summary>
        /// 派发的K币数(积分数)(默认为1)
        /// </summary>
        [Required(ErrorMessage = "K币数不能为空")]
        public int? Score { get; set; }
        /// <summary>
        /// 派发标题
        /// </summary>
        [MaxLength(25)]
        [Required(ErrorMessage = "标题不能为空")]
        public string Theme { get; set; }
        /// <summary>
        /// 派发说明
        /// </summary>
        [MaxLength(100)]
        [Required(ErrorMessage = "说明不能为空")]
        public string Memo { get; set; }
        /// <summary>
        /// 映像标签(英文逗号分隔,单个标签不能超过6个汉字)
        /// </summary>
        [MaxLength(100)]        
        public string Labels { get; set; }
    }
}
