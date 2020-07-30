using Microsoft.EntityFrameworkCore;
using PointsGame.Dto.Response;
using PointsGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PointsGame.Stores
{
    /// <summary>
    /// 自由交易
    /// </summary>
    public class DealStore : IDealStore
    {
        protected PointsGameDbContext Context { get; }
        public DealStore(PointsGameDbContext context)
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
        /// 获取用户信息表
        /// </summary>
        /// <returns></returns>
        public IQueryable<UserInfo> GetUserInfos()
        {
            return Context.UserInfos.AsNoTracking();
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
        /// 查询交易列表
        /// </summary>
        /// <returns></returns>
        public IQueryable<SearchDealResponse> GetSearchDeal(string periodId)
        {
            var query = from s in Context.ScoreDetaileds.AsNoTracking()
                            //交易用户ID
                        join u1 in Context.UserInfos.AsNoTracking() on s.UserId equals u1.Id into u1T
                        from u1r in u1T.DefaultIfEmpty()
                            //交易对象用户ID
                        join u2 in Context.UserInfos.AsNoTracking() on s.DealUserId equals u2.Id into u2T
                        from u2r in u2T.DefaultIfEmpty()

                        where s.IsDelete == false && (s.DealType == 1 || s.DealType == 2) && s.PeriodId == periodId
                        orderby s.CreateTime descending
                        select new SearchDealResponse
                        {
                            Id = s.Id,
                            PeriodId = s.PeriodId,
                            Theme = s.Theme,
                            DealNumber = s.DealNumber,
                            DealType = s.DealType,
                            UserId = u1r.Id,
                            UserName = u1r.UserName + "-" + u1r.GroupName,
                            DealUserId = u2r.Id,
                            DealUserName = u2r.UserName + "-" + u2r.GroupName,
                            Score = s.Score,
                            Memo = s.Memo,
                            CreateTime = s.CreateTime
                        };
            return query;
        }

        /// <summary>
        /// 批量更新积分信息
        /// </summary>
        /// <param name="scoreInfoList"></param>
        /// <returns></returns>
        public async Task UpdateScoreInfoList(List<ScoreInfo> scoreInfoList, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (scoreInfoList == null || scoreInfoList.Count == 0)
            {
                throw new ArgumentNullException(nameof(scoreInfoList));
            }

            Context.UpdateRange(scoreInfoList);

            await Context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        ///批量添加积分明细
        /// </summary>
        /// <param name="scoreDetailedList"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task CreateScoreDetailedList(List<ScoreDetailed> scoreDetailedList, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (scoreDetailedList == null || scoreDetailedList.Count == 0)
            {
                throw new ArgumentNullException(nameof(scoreDetailedList));
            }

            Context.AddRange(scoreDetailedList);

            await Context.SaveChangesAsync(cancellationToken);
        }
    }
}
