using ApiCore;
using PointsGame.Dto.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    /// <summary>
    /// 用户搜索请求
    /// </summary>
    public class UserSearchRequest
    {
        /// <summary>
        /// 分页类型（分页[默认]-0、所有-1）
        /// </summary>
        public PageType? Type { get; set; } = PageType.Page;

        /// <summary>
        /// 分页索引（默认：0）
        /// </summary>
        public int PageIndex { get; set; } = 0;

        /// <summary>
        /// 分页尺寸（默认：20）
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// 过滤（姓名、登录名(工号)、部门名，模糊查询）
        /// </summary>
        public string Filter { get; set; }
    }
}
