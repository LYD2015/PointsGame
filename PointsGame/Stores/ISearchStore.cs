using PointsGame.Dto.Response;
using PointsGame.Models;
using System.Linq;

namespace PointsGame.Stores
{
    /// <summary>
    /// 查询
    /// </summary>
    public interface ISearchStore
    {
        /// <summary>
        /// 获取称号信息表
        /// </summary>
        /// <returns></returns>
        IQueryable<RankTitle> GetScoreTitles();

        /// <summary>
        /// 获取用户印象标签表
        /// </summary>
        /// <returns></returns>
        IQueryable<UserLabel> GetUserLabels();

        /// <summary>
        /// 获取动态信息表
        /// </summary>
        /// <returns></returns>
        IQueryable<DynamicInfo> GetDynamicInfos();

        /// <summary>
        /// 获取排行榜
        /// </summary>
        /// <returns></returns>
        IQueryable<SearchTopResponse> GetSearchTop(string periodId);

    }
}
