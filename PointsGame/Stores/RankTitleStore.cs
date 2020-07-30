using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PointsGame.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace PointsGame.Stores
{
    /// <summary>
    /// 称号
    /// </summary>
    public class RankTitleStore : IRankTitleStore
    {
        protected PointsGameDbContext Context { get; }
        public RankTitleStore(PointsGameDbContext context)
        {
            Context = context;
        }

        /// <summary>
        /// 获取赛季信息表
        /// </summary>
        /// <returns></returns>
        public IQueryable<Period> GetScorePeriods()
        {
            return Context.ScorePeriods.AsNoTracking();
        }

        /// <summary>
        /// 获取赛季信息表
        /// </summary>
        /// <returns></returns>
        public IQueryable<RankTitle> GetScoreTitles()
        {
            return Context.ScoreTitles.AsNoTracking();
        }

        /// <summary>
        /// 获取用户信息表
        /// </summary>
        /// <returns></returns>
        public IQueryable<UserInfo> GetUserInfos()
        {
            return Context.UserInfos.AsNoTracking();
        }

        /// <summary>
        /// 添加称号
        /// </summary>
        /// <param name="scoreTitle"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<RankTitle> AddTitleAsync(RankTitle scoreTitle, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (scoreTitle == null)
            {
                throw new ArgumentNullException(nameof(scoreTitle));
            }
            Context.Add(scoreTitle);
            await Context.SaveChangesAsync(cancellationToken);
            return scoreTitle;
        }
        /// <summary>
        /// 批量添加称号
        /// </summary>
        /// <param name="scoreTitleList"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<RankTitle>> AddTitleListAsync(List<RankTitle> scoreTitleList, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (scoreTitleList == null)
            {
                throw new ArgumentNullException(nameof(scoreTitleList));
            }
            Context.AddRange(scoreTitleList);
            await Context.SaveChangesAsync(cancellationToken);
            return scoreTitleList;
        }


        

        /// <summary>
        /// 批量修改称号
        /// </summary>
        /// <param name="scoreTitleList"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<RankTitle>> UpdateTitleListAsync(List<RankTitle> scoreTitleList, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (scoreTitleList == null || scoreTitleList.Count == 0)
            {
                throw new ArgumentNullException(nameof(scoreTitleList));
            }
            Context.UpdateRange(scoreTitleList);
            try
            {
                await Context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return scoreTitleList;
        }
    }
}
