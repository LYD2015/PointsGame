using PointsGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PointsGame.Stores
{
    /// <summary>
    /// 等级称号
    /// </summary>
    public interface IRankTitleStore
    {
        /// <summary>
        /// 获取赛季信息表
        /// </summary>
        /// <returns></returns>
        IQueryable<Period> GetScorePeriods();

        /// <summary>
        /// 获取称号信息表
        /// </summary>
        /// <returns></returns>
        IQueryable<RankTitle> GetScoreTitles();

        /// <summary>
        /// 获取用户信息表(本系统)
        /// </summary>
        /// <returns></returns>
        IQueryable<UserInfo> GetUserInfos();

        /// <summary>
        /// 新增称号
        /// </summary>
        /// <param name="scoreTitle"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<RankTitle> AddTitleAsync(RankTitle scoreTitle, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 批量新增称号
        /// </summary>
        /// <param name="scoreTitle"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<RankTitle>> AddTitleListAsync(List<RankTitle> scoreTitleList, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 批量修改称号
        /// </summary>
        /// <param name="scoreTitleList"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<RankTitle>> UpdateTitleListAsync(List<RankTitle> scoreTitleList, CancellationToken cancellationToken = default(CancellationToken));
    }
}
