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
    public interface IDealStore
    {
        /// <summary>
        /// 获取赛季信息表
        /// </summary>
        /// <returns></returns>
        IQueryable<Period> GetScorePeriods();

        /// <summary>
        /// 获取用户信息表
        /// </summary>
        /// <returns></returns>
        IQueryable<UserInfo> GetUserInfos();

        /// <summary>
        /// 获取积分信息表
        /// </summary>
        /// <returns></returns>
        IQueryable<ScoreInfo> GetScoreInfos();

        /// <summary>
        /// 查询交易列表
        /// </summary>
        /// <param name="periodId"></param>
        /// <returns></returns>
        IQueryable<SearchDealResponse> GetSearchDeal(string periodId);

        /// <summary>
        /// 批量更新积分信息
        /// </summary>
        /// <param name="scoreInfoList"></param>
        /// <returns></returns>
        Task UpdateScoreInfoList(List<ScoreInfo> scoreInfoList, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 批量添加积分明细
        /// </summary>
        /// <param name="scoreDetailedList"></param>
        /// <returns></returns>
        Task CreateScoreDetailedList(List<ScoreDetailed> scoreDetailedList, CancellationToken cancellationToken = default(CancellationToken));
    }
}
