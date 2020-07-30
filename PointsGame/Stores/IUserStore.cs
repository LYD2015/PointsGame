using PointsGame.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PointsGame.Stores
{
    /// <summary>
    /// 用户
    /// </summary>
    public interface IUserStore
    {
        /// <summary>
        /// 用户信息
        /// </summary>
        IQueryable<UserInfo> GetUserInfos();
        /// <summary>
        /// 新增用户登陆日志
        /// </summary>
        /// <param name="logs"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Create(List<UserSignLog> logs, CancellationToken cancellationToken = default(CancellationToken));
        /// <summary>
        /// 新增用户
        /// </summary>
        /// <param name="users"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Create(List<UserInfo> users, CancellationToken cancellationToken = default(CancellationToken));
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="users"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Delete(List<UserInfo> users, CancellationToken cancellationToken = default(CancellationToken));
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="userIds"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Delete(List<string> userIds, CancellationToken cancellationToken = default(CancellationToken));
        /// <summary>
        /// 更新用户
        /// </summary>
        /// <param name="users"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Update(List<UserInfo> users, CancellationToken cancellationToken = default(CancellationToken));       
    }
}
