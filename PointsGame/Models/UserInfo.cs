using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Models
{
    /// <summary>
    /// 用户信息实体
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        [Key]
        [MaxLength(127)]
        public string Id { get; set; }
        /// <summary>
        /// 认证中心的UerId
        /// </summary>
        [MaxLength(127)]
        public string UserId { get; set; }

        /// <summary>
        /// 登陆名（工号）
        /// </summary>
        [MaxLength(50)]
        public string LoginName { get; set; }

        /// <summary>
        /// 用户名（TrueName：姓名）
        /// </summary>
        [MaxLength(50)]
        public string UserName { get; set; }

        /// <summary>
        /// 组织名
        /// </summary>
        [MaxLength(50)]
        public string OrganizationName { get; set; }

        /// <summary>
        /// 分组名
        /// </summary>
        [MaxLength(50)]
        public string GroupName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [MaxLength(127)]
        public string CreateUser { get; set; }

        /// <summary>
        /// 是否是管理员
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { get; set; }

        /// <summary>
        /// 电子邮件
        /// </summary>
        [MaxLength(50)]
        public string Email { get; set; }

        /// <summary>
        /// 禅道帐号
        /// </summary>
        [MaxLength(50)]
        public string ZenTao { get; set; }
        
    }
}
