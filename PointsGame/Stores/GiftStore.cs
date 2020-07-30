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
    /// 任务
    /// </summary>
    public class GiftStore : IGiftStore
    {
        protected PointsGameDbContext Context { get; }
        public GiftStore(PointsGameDbContext context)
        {
            Context = context;
        }

        /// <summary>
        /// 获取赛季奖品说明表
        /// </summary>
        /// <returns></returns>
        public IQueryable<PeriodGift> GetPeriodGifts()
        {
            return Context.PeriodGifts.AsNoTracking();
        }

        /// <summary>
        /// 获取赛季奖品信息表
        /// </summary>
        /// <returns></returns>
        public IQueryable<GiftInfo> GetGiftInfos()
        {
            return Context.GiftInfos.AsNoTracking();
        }
        /// <summary>
        /// 更新季奖品说明表
        /// </summary>
        /// <param name="periodGift"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task UpdatePeriodGift(PeriodGift periodGift, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (periodGift == null)
            {
                throw new ArgumentNullException(nameof(periodGift));
            }
            Context.Update(periodGift);
            await Context.SaveChangesAsync(cancellationToken);            
        }
        /// <summary>
        /// 新增季奖品说明表
        /// </summary>
        /// <param name="periodGift"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task AddPeriodGift(PeriodGift periodGift, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (periodGift == null)
            {
                throw new ArgumentNullException(nameof(periodGift));
            }
            Context.Add(periodGift);
            await Context.SaveChangesAsync(cancellationToken);
        }
        /// <summary>
        /// 新增赛季奖品信息
        /// </summary>
        /// <param name="giftInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task AddGift(GiftInfo giftInfo, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (giftInfo == null)
            {
                throw new ArgumentNullException(nameof(giftInfo));
            }
            Context.Add(giftInfo);
            await Context.SaveChangesAsync(cancellationToken);
        }
        /// <summary>
        /// 修改赛季奖品信息
        /// </summary>
        /// <param name="giftInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task UpdateGift(GiftInfo giftInfo, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (giftInfo == null)
            {
                throw new ArgumentNullException(nameof(giftInfo));
            }
            Context.Update(giftInfo);
            await Context.SaveChangesAsync(cancellationToken);
        }
        /// <summary>
        /// 获取彩票开奖结果表
        /// </summary>
        /// <returns></returns>

        public IQueryable<LotteryResult> GetLotteryResults()
        {
            return Context.LotteryResults.AsNoTracking();
        }
        /// <summary>
        /// 获取用户投注情况表
        /// </summary>
        /// <returns></returns>
        public IQueryable<LotteryUser> GetLotteryUsers()
        {
            return Context.LotteryUsers.AsNoTracking();
        }
        /// <summary>
        /// 修改用户投注信息
        /// </summary>
        /// <param name="lotteryUser"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task UpdateLotteryUser(LotteryUser lotteryUser, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (lotteryUser == null)
            {
                throw new ArgumentNullException(nameof(lotteryUser));
            }
            Context.Update(lotteryUser);
            await Context.SaveChangesAsync(cancellationToken);
        }
        /// <summary>
        /// 批量新增用户投注信息
        /// </summary>
        /// <param name="lotteryUser"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task AddLotteryUserList(List<LotteryUser> lotteryUserList, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (lotteryUserList == null|| lotteryUserList.Count==0)
            {
                throw new ArgumentNullException(nameof(lotteryUserList));
            }
            Context.AddRange(lotteryUserList);
            await Context.SaveChangesAsync(cancellationToken);
        }
        /// <summary>
        /// 新增彩票开奖结果
        /// </summary>
        /// <param name="lotteryResult"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task AddLotteryResult(LotteryResult lotteryResult, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (lotteryResult == null)
            {
                throw new ArgumentNullException(nameof(lotteryResult));
            }
            Context.Add(lotteryResult);
            await Context.SaveChangesAsync(cancellationToken);
        }
    }
}
