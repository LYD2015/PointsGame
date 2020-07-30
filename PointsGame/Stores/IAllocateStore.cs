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
    public interface IAllocateStore
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
        /// 获取积分明细表
        /// </summary>
        /// <returns></returns>
        IQueryable<ScoreDetailed> GetScoreDetaileds();

        /// <summary>
        /// 查询交易列表
        /// </summary>
        /// <param name="periodId"></param>
        /// <returns></returns>
        IQueryable<SearchAllocateResponse> GetSearchAllocate(string periodId);

        /// <summary>
        /// 更新积分信息
        /// </summary>
        /// <param name="scoreInfo"></param>
        /// <returns></returns>
        Task UpdateScoreInfo(ScoreInfo scoreInfo, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 添加积分明细
        /// </summary>
        /// <param name="scoreDetailed"></param>
        /// <returns></returns>
        Task CreateScoreDetailed(ScoreDetailed scoreDetailed, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 批量添加用户印象标签
        /// </summary>
        /// <param name="userLabelList"></param>
        /// <returns></returns>
        Task CreateUserLabelList(List<UserLabel> userLabelList, CancellationToken cancellationToken = default(CancellationToken));
    }
}
