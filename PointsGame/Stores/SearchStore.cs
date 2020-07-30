using System.Linq;
using Microsoft.EntityFrameworkCore;
using PointsGame.Dto.Response;
using PointsGame.Models;

namespace PointsGame.Stores
{
    /// <summary>
    /// 查询
    /// </summary>
    public class SearchStore : ISearchStore
    {
        protected PointsGameDbContext Context { get; }
        public SearchStore(PointsGameDbContext context)
        {
            Context = context;
        }

        /// <summary>
        /// 获取称号信息
        /// </summary>
        /// <returns></returns>
        public IQueryable<RankTitle> GetScoreTitles()
        {
            return Context.ScoreTitles.AsNoTracking();
        }

        /// <summary>
        /// 获取用户印象标签表
        /// </summary>
        /// <returns></returns>
        public IQueryable<UserLabel> GetUserLabels()
        {
            return Context.UserLabels.AsNoTracking();
        }

        /// <summary>
        /// 获取动态信息表
        /// </summary>
        /// <returns></returns>
        public IQueryable<DynamicInfo> GetDynamicInfos()
        {
            return Context.DynamicInfos.AsNoTracking();
        }

        /// <summary>
        /// 查询排行榜
        /// </summary>
        /// <returns></returns>
        public IQueryable<SearchTopResponse> GetSearchTop(string periodId)
        {
            var query = from u in Context.UserInfos.AsNoTracking()
                            // 左连接子查询优化
                        join s in Context.ScoreInfos.AsNoTracking() on new { UserId = u.Id, PeriodId = periodId, IsDelete = false } equals new { s.UserId, s.PeriodId, s.IsDelete } into sT
                        from sr in sT.DefaultIfEmpty()
                            // 解析的时候会先判断是否为空
                        where u.IsDelete == false
                        orderby sr.Score descending, u.UserName, u.LoginName
                        select new SearchTopResponse
                        {
                            UserId = u.Id,
                            UserName = u.UserName,
                            OrganizationName = u.OrganizationName,
                            GroupName = u.GroupName,
                            Score = sr.Score,
                            ConsumableScore = sr.ConsumableScore,
                        };
            return query;
        }

    }
}
