using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    /// <summary>
    /// 用户保存请求体
    /// </summary>
    public class UserSaveRequest
    {
        /// <summary>
        /// 唯一ID(新增不传，修改传)
        /// </summary>
        [MaxLength(127)]
        public string Id { get; set; }

        /// <summary>
        /// 登陆名（工号、对应认证中心UserName）
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
        /// 是否是管理员
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// 禅道帐号
        /// </summary>
        [MaxLength(50)]
        public string ZenTao { get; set; }
    }
}
