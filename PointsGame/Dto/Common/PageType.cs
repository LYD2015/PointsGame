using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Common
{
    /// <summary>
    /// 分页类型（分页-0、所有-1）
    /// </summary>
    public enum PageType
    {
        /// <summary>
        /// 分页
        /// </summary>
        [Description("分页")]
        Page = 0,

        /// <summary>
        /// 所有
        /// </summary>
        [Description("所有")]
        All = 1
    }
}
