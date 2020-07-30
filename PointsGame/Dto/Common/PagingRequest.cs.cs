using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Common
{
    /// <summary>
    /// 分页请求
    /// </summary>
    public class PagingRequest
    {
        /// <summary>
        /// 分页下标（从0开始，默认为0）
        /// </summary>
        public int PageIndex { get; set; } = 0;
        /// <summary>
        /// 分页大小（默认为20）
        /// </summary>
        public int PageSize { get; set; } = 20;
    }
}
