using PointsGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Stores
{
    /// <summary>
    /// 赛季
    /// </summary>
    public interface IPeriodStore
    {
        /// <summary>
        /// 获取赛季信息表
        /// </summary>
        /// <returns></returns>
        IQueryable<Period> GetScorePeriods();

        /// <summary>
        /// 添加赛季
        /// </summary>
        /// <returns></returns>
        Task CreatePeriodAsync(Period scorePeriod);

        /// <summary>
        /// 修改赛季
        /// </summary>
        /// <param name="scorePeriod"></param>
        /// <returns></returns>
        Task UpdatePeriodAsync(Period scorePeriod);

        /// <summary>
        /// 获取指定赛季下没有积分信息的用户
        /// </summary>
        /// <param name="periodId"></param>
        /// <returns></returns>
        IQueryable<UserInfo> GetNotScoreInfoUsers(string periodId);

    }
}
