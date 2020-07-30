using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Models
{
    /// <summary>
    /// 用户登陆日志实体
    /// </summary>
    public class UserSignLog
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        [Key]
        [MaxLength(127)]
        public string Id { get; set; }

        /// <summary>
        /// 本地用户ID
        /// </summary>
        [MaxLength(127)]
        public string LocalUserId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [MaxLength(127)]
        public string UserId { get; set; }

        /// <summary>
        /// 登陆名（对应工号和认证中心的UserName）
        /// </summary>
        [MaxLength(50)]
        public string LoginName { get; set; }

        /// <summary>
        /// 用户名（TrueName：姓名）
        /// </summary>
        [MaxLength(50)]
        public string UserName { get; set; }

        /// <summary>
        /// 登陆时间
        /// </summary>
        public DateTime? SigninTime { get; set; }

        /// <summary>
        /// 登陆平台(web-0, android-2, ios-3)
        /// </summary>
        public int? Platform { get; set; }
    }
}
