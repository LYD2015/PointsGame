using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PointsGame.Models;

namespace PointsGame.Stores
{
    /// <summary>
    /// 赛季
    /// </summary>
    public class PeriodStore : IPeriodStore
    {
        protected PointsGameDbContext Context { get; }
        public PeriodStore(PointsGameDbContext context)
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
        /// 添加赛季
        /// </summary>
        /// <param name="scorePeriod"></param>
        /// <returns></returns>
        public async Task CreatePeriodAsync(Period scorePeriod)
        {
            if (scorePeriod==null)
            {
                throw new ArgumentNullException(nameof(scorePeriod));

            }
            Context.Add(scorePeriod);
            await Context.SaveChangesAsync();
        }

        /// <summary>
        /// 修改赛季信息
        /// </summary>
        /// <param name="scorePeriod"></param>
        /// <returns></returns>
        public async Task UpdatePeriodAsync(Period scorePeriod)
        {
            if (scorePeriod == null)
            {
                throw new ArgumentNullException(nameof(scorePeriod));

            }
            Context.Update(scorePeriod);
            await Context.SaveChangesAsync();
        }

        /// <summary>
        /// 获取指定赛季下没有积分信息的用户
        /// </summary>
        /// <param name="periodId">赛季ID</param>
        /// <returns></returns>
        public IQueryable<UserInfo> GetNotScoreInfoUsers(string periodId)
        {
            var query = from u in Context.UserInfos
                        where !(from s in Context.ScoreInfos 
                                where s.PeriodId == periodId
                                select s.UserId).Contains(u.Id)
                        select u;
            return query;
        }      

    }
}
