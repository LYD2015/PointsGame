using PointsGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PointsGame.Stores
{
    /// <summary>
    /// 积分信息
    /// </summary>
    public interface IScoreInfoStore
    {
        /// <summary>
        /// 新增积分信息
        /// </summary>
        /// <param name="scoreInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task CreateScoreInfo(ScoreInfo scoreInfo, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 批量新增积分信息
        /// </summary>
        /// <param name="scoreInfoList"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task CreateScoreInfoList(List<ScoreInfo> scoreInfoList, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 获取积分信息表
        /// </summary>
        IQueryable<ScoreInfo> GetScoreInfos();

        /// <summary>
        /// 获取积分明细表
        /// </summary>
        IQueryable<ScoreDetailed> GetScoreInfoDetaileds();

        /// <summary>
        /// 获取赛季信息表
        /// </summary>
        IQueryable<Period> GetScorePeriods();
    }
}
