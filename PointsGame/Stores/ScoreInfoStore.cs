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
    /// 积分信息
    /// </summary>
    public class ScoreInfoStore : IScoreInfoStore
    {
        /// <summary>
        /// 积分
        /// </summary>
        private PointsGameDbContext Context { get; }

        /// <summary>
        /// </summary>
        /// <param name="context"></param>
        public ScoreInfoStore(PointsGameDbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// 获取积分信息表
        /// </summary>
        /// <returns></returns>
        public IQueryable<ScoreInfo> GetScoreInfos()
        {
            return Context.ScoreInfos.AsNoTracking();
        }
        /// <summary>
        /// 获取积分明细表
        /// </summary>
        /// <returns></returns>
        public IQueryable<ScoreDetailed> GetScoreInfoDetaileds()
        {
            return Context.ScoreDetaileds.AsNoTracking();
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
        /// 新增积分信息
        /// </summary>
        /// <param name="scoreInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task CreateScoreInfo(ScoreInfo scoreInfo, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (scoreInfo == null)
            {
                throw new ArgumentNullException(nameof(scoreInfo));
            }

            Context.Add(scoreInfo);

            await Context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 批量新增积分信息
        /// </summary>
        /// <param name="scoreInfoList"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task CreateScoreInfoList(List<ScoreInfo> scoreInfoList, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (scoreInfoList == null || scoreInfoList.Count == 0)
            {
                throw new ArgumentNullException(nameof(scoreInfoList));
            }

            Context.AddRange(scoreInfoList);

            await Context.SaveChangesAsync(cancellationToken);
        }
    }
}
