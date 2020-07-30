using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Response
{
    /// <summary>
    /// 用户信息响应体
    /// </summary>
    public class UserResponse
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string Id { get; set; }         

        /// <summary>
        /// 登陆名（工号）
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// 用户名（TrueName：姓名）
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 组织名
        /// </summary>
        public string OrganizationName { get; set; }

        /// <summary>
        /// 分组名
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 是否是管理员
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// 禅道帐号
        /// </summary>
        public string ZenTao { get; set; }
    }
}
