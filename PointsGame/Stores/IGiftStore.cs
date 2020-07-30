using PointsGame.Dto.Response;
using PointsGame.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PointsGame.Stores
{
    /// <summary>
    /// 任务
    /// </summary>
    public interface IGiftStore
    {
        /// <summary>
        /// 获取赛季奖品说明表
        /// </summary>
        /// <returns></returns>
        IQueryable<PeriodGift> GetPeriodGifts();

        /// <summary>
        /// 获取赛季奖品信息表
        /// </summary>
        /// <returns></returns>
        IQueryable<GiftInfo> GetGiftInfos();

        /// <summary>
        /// 获取彩票开奖结果表
        /// </summary>
        /// <returns></returns>
        IQueryable<LotteryResult> GetLotteryResults();

        /// <summary>
        /// 获取用户投注情况表
        /// </summary>
        /// <returns></returns>
        IQueryable<LotteryUser> GetLotteryUsers();

        /// <summary>
        /// 更新赛季奖品说明表
        /// </summary>
        /// <param name="scoreInfoList"></param>
        /// <returns></returns>
        Task UpdatePeriodGift(PeriodGift periodGift, CancellationToken cancellationToken = default(CancellationToken));
        /// <summary>
        /// 新增赛季奖品说明表
        /// </summary>
        /// <param name="scoreInfoList"></param>
        /// <returns></returns>
        Task AddPeriodGift(PeriodGift periodGift, CancellationToken cancellationToken = default(CancellationToken));
        /// <summary>
        /// 新增赛季奖品信息
        /// </summary>
        /// <param name="scoreInfoList"></param>
        /// <returns></returns>
        Task AddGift(GiftInfo giftInfo, CancellationToken cancellationToken = default(CancellationToken));
        /// <summary>
        /// 修改赛季奖品信息
        /// </summary>
        /// <param name="scoreInfoList"></param>
        /// <returns></returns>
        Task UpdateGift(GiftInfo giftInfo, CancellationToken cancellationToken = default(CancellationToken));
        /// <summary>
        /// 修改用户投注信息
        /// </summary>
        /// <param name="lotteryUser"></param>
        /// <returns></returns>
        Task UpdateLotteryUser(LotteryUser lotteryUser, CancellationToken cancellationToken = default(CancellationToken));
        /// <summary>
        /// 批量新增用户投注信息
        /// </summary>
        /// <param name="lotteryUser"></param>
        /// <returns></returns>
        Task AddLotteryUserList(List<LotteryUser> lotteryUserList, CancellationToken cancellationToken = default(CancellationToken));
        /// <summary>
        /// 新增彩票开奖结果
        /// </summary>
        /// <param name="lotteryResult"></param>
        /// <returns></returns>
        Task AddLotteryResult(LotteryResult lotteryResult, CancellationToken cancellationToken = default(CancellationToken));
    }
}
