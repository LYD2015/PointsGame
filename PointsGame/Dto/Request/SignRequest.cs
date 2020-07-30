using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    /// <summary>
    /// 登陆请求 后面需要加密传输
    /// </summary>
    public class SignRequest
    {
        /// <summary>
        /// 登陆名
        /// </summary>
        [MaxLength(50)]
        public string LoginName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [MaxLength(50)]
        public string Password { get; set; }
    }
}
