using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PointsGame.Models;

namespace PointsGame.Stores
{
    /// <summary>
    /// 用户
    /// </summary>
    public class UserStore : IUserStore
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public UserStore(PointsGameDbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }
        private PointsGameDbContext Context { get; }

        /// <summary>
        /// 用户信息
        /// </summary>
        /// <returns></returns>
        public IQueryable<UserInfo> GetUserInfos()
        {
            return Context.UserInfos.AsNoTracking();
        }

        /// <summary>
        /// </summary>
        /// <param name="users"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Create(List<UserInfo> users, CancellationToken cancellationToken = default(CancellationToken))
        {
            Context.AddRange(users);
            await Context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="users"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Delete(List<UserInfo> users, CancellationToken cancellationToken = default(CancellationToken))
        {
            Context.AttachRange(users);
            Context.RemoveRange(users);
            await Context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 批量删除用户
        /// </summary>
        /// <param name="userIds"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Delete(List<string> userIds, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entities = await GetUserInfos().Where(a => (a.IsDelete != false) && userIds.Contains(a.Id)).ToListAsync(cancellationToken);

            foreach (var entity in entities)
            {
                entity.IsDelete = true;
            }
            Context.AttachRange(entities);
            Context.UpdateRange(entities);
            await Context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// </summary>
        /// <param name="users"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Update(List<UserInfo> users, CancellationToken cancellationToken = default(CancellationToken))
        {
            Context.AttachRange(users);
            Context.UpdateRange(users);
            await Context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// </summary>
        /// <param name="logs"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Create(List<UserSignLog> logs, CancellationToken cancellationToken = default)
        {
            Context.AddRange(logs);
            await Context.SaveChangesAsync(cancellationToken);
        }
    }
}
