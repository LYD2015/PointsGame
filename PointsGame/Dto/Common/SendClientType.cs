using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Common {

    /// <summary>
    /// 消息类型
    /// </summary>
    public enum SendClientType {
        /// <summary>
        /// 赛季变化
        /// </summary>
        Season=1,
        /// <summary>
        /// 用户信息变化
        /// </summary>
        User = 2,
        /// <summary>
        /// 排行榜变化
        /// </summary>
        Rank = 3,
        /// <summary>
        /// 右侧动态变化
        /// </summary>
        Dynamic = 4
    }
}
