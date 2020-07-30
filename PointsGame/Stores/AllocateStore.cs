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
    /// 派发
    /// </summary>
    public class AllocateStore : IAllocateStore
    {
        protected PointsGameDbContext Context { get; }
        public AllocateStore(PointsGameDbContext context)
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
        /// 获取积分明细表
        /// </summary>
        /// <returns></returns>
        public IQueryable<ScoreDetailed> GetScoreDetaileds()
        {
            return Context.ScoreDetaileds.AsNoTracking();
        }
        

        /// <summary>
        /// 查询交易列表
        /// </summary>
        /// <returns></returns>
        public IQueryable<SearchAllocateResponse> GetSearchAllocate(string periodId)
        {
            var query = from s in Context.ScoreDetaileds.AsNoTracking()
                            //派发用户ID
                        join u1 in Context.UserInfos.AsNoTracking() on s.CreateUser equals u1.Id into u1T
                        from u1r in u1T.DefaultIfEmpty()
                            //派发对象用户ID
                        join u2 in Context.UserInfos.AsNoTracking() on s.DealUserId equals u2.Id into u2T
                        from u2r in u2T.DefaultIfEmpty()

                        where s.IsDelete == false && s.DealType == 4 && s.PeriodId == periodId
                        orderby s.CreateTime descending
                        select new SearchAllocateResponse
                        {
                            Id = s.Id,
                            PeriodId = s.PeriodId,
                            Theme = s.Theme,
                            UserId = s.UserId,
                            UserName = u1r.UserName + "-" + u1r.GroupName,
                            DealUserId = u2r.Id,
                            DealUserName = u2r.UserName + "-" + u2r.GroupName,
                            Score = s.Score,
                            Memo = s.Memo,
                            Labels = s.Labels,
                            CreateTime = s.CreateTime
                        };
            return query;
        }

        /// <summary>
        /// 更新积分信息
        /// </summary>
        /// <param name="scoreInfoList"></param>
        /// <returns></returns>
        public async Task UpdateScoreInfo(ScoreInfo scoreInfo, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (scoreInfo == null)
            {
                throw new ArgumentNullException(nameof(scoreInfo));
            }

            Context.Update(scoreInfo);

            await Context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        ///添加积分明细
        /// </summary>
        /// <param name="scoreDetailedList"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task CreateScoreDetailed(ScoreDetailed scoreDetailed, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (scoreDetailed == null)
            {
                throw new ArgumentNullException(nameof(scoreDetailed));
            }

            Context.Add(scoreDetailed);

            await Context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 批量添加用户印象标签
        /// </summary>
        /// <param name="userLabelList"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task CreateUserLabelList(List<UserLabel> userLabelList, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (userLabelList == null || userLabelList.Count == 0)
            {
                throw new ArgumentNullException(nameof(userLabelList));
            }

            Context.AddRange(userLabelList);

            await Context.SaveChangesAsync(cancellationToken);
        }
    }
}
