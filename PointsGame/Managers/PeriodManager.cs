using ApiCore;
using Microsoft.EntityFrameworkCore;
using PointsGame.Dto.Request;
using PointsGame.Dto.Response;
using PointsGame.Helper;
using PointsGame.Models;
using PointsGame.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Managers
{
    /// <summary>
    /// 赛季
    /// </summary>
    public class PeriodManager
    {
        private readonly IPeriodStore _scorePeriodStore;
        private readonly IScoreInfoStore _scoreInfoStore;
        private readonly IRankTitleStore _rankTitleStore;
        private readonly IGiftStore _giftStore;
        private readonly ITransaction<PointsGameDbContext> _transaction;// 事务
        private HelperSendClientMessage _sendClientMessageManager;
        public PeriodManager(IPeriodStore scorePeriodStore, IScoreInfoStore scoreInfo, IScoreInfoStore scoreInfoStore, ITransaction<PointsGameDbContext> transaction, 
            HelperSendClientMessage sendClientMessageManager, IRankTitleStore rankTitleStore, IGiftStore giftStore)
        {
            _scorePeriodStore = scorePeriodStore ?? throw new ArgumentNullException(nameof(scorePeriodStore));
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            _scoreInfoStore = scoreInfoStore ?? throw new ArgumentNullException(nameof(transaction));
            _sendClientMessageManager = sendClientMessageManager;
            _rankTitleStore = rankTitleStore;
            _giftStore = giftStore;
        }
        /// <summary>
        /// 查询所有赛季信息
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseMessage<List<PeriodReponse>>> SearchPeriodlist()
        {
            var response = new ResponseMessage<List<PeriodReponse>>();
            var perlist = await _scorePeriodStore.GetScorePeriods().Where(w => !w.IsDelete).OrderByDescending(o => o.StartDate).Select(s => new PeriodReponse
            {
                Caption = s.Caption,
                EndDate = s.EndDate,
                StartDate = s.StartDate,
                Memo = s.Memo,
                PlayingUrl = s.PlayingUrl,
                SystemUrl = s.SystemUrl,
                State = s.State,
                StateString = HelperConvert.PeriodStateToString(s.State),
                Id = s.Id,
            }).ToListAsync();
            perlist = perlist.OrderBy(o => (o.State == 1 ? -1 : o.State)).ToList();//进行中的赛季排前面
            response.Extension = perlist;
            return response;
        }

        /// <summary>
        /// 查询当前和历史赛季信息
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseMessage<List<PeriodReponse>>> SearchNowAndhistoryPeriodlist()
        {
            var response = new ResponseMessage<List<PeriodReponse>>();
            var perlist = await _scorePeriodStore.GetScorePeriods().Where(w => !w.IsDelete && w.State != 0).OrderBy(o => o.State).ThenByDescending(o=>o.StartDate).Select(s => new PeriodReponse
            {
                Caption = s.Caption,
                EndDate = s.EndDate,
                StartDate = s.StartDate,
                Memo = s.Memo,
                PlayingUrl = s.PlayingUrl,
                SystemUrl = s.SystemUrl,
                State = s.State,
                StateString = HelperConvert.PeriodStateToString(s.State),
                Id = s.Id,
            }).ToListAsync();
            //判断进行中的赛季是否可以抽奖
            perlist.ForEach(fo => 
            {
                if (fo.State == 1)
                {
                    var giftPeriodInfo = _giftStore.GetPeriodGifts().Where(w => w.PeriodId == fo.Id).FirstOrDefaultAsync();
                    if (giftPeriodInfo != null && giftPeriodInfo.Result != null && giftPeriodInfo.Result.Enabled == true)
                    {
                        fo.GiftEnabeld = true;
                    }
                }
            });

            response.Extension = perlist;
            return response;
        }

        /// <summary>
        /// 添加赛季
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseMessage> CreatePeriod(UserInfo userInfo, CreatePeriodRequest createPeriodRequest)
        {
            var response = new ResponseMessage();
            var scorePeriod = new Period();
            scorePeriod.Id = Guid.NewGuid().ToString();
            scorePeriod.IsDelete = false;
            scorePeriod.Memo = createPeriodRequest.Memo;
            scorePeriod.StartDate = createPeriodRequest.StartDate;
            scorePeriod.Caption = createPeriodRequest.Caption;
            scorePeriod.CreateTime = DateTime.Now;
            scorePeriod.CreateUser = userInfo.Id;
            scorePeriod.UpdateTime = DateTime.Now;
            scorePeriod.UpdateUser = userInfo.Id;
            scorePeriod.State = 0;
            scorePeriod.EndDate = createPeriodRequest.EndDate;
            scorePeriod.PlayingUrl = createPeriodRequest.PlayingUrl;
            scorePeriod.SystemUrl = createPeriodRequest.SystemUrl;

            //复制一份上一个赛季的称号信息
            //先查询有没有进行中的赛季
            var periods = await _rankTitleStore.GetScorePeriods().Where(w => !w.IsDelete && w.State == 1).FirstOrDefaultAsync();
            var rankInfoList = new List<RankTitle>();
            if (periods == null)
            {
                //在查询上一个赛季
                periods= await _rankTitleStore.GetScorePeriods().Where(w => !w.IsDelete).OrderByDescending(o=>o.EndDate).FirstOrDefaultAsync();
            }
            if (periods != null)
            {
               rankInfoList = await _rankTitleStore.GetScoreTitles().Where(w => !w.IsDelete && w.PeriodId == periods.Id).ToListAsync();
                rankInfoList.ForEach(fo => 
                {
                    fo.Id = Guid.NewGuid().ToString();
                    fo.PeriodId = scorePeriod.Id;
                    fo.UpdateTime = DateTime.Now;
                    fo.UpdateUser = userInfo.Id;
                    fo.CreateTime = DateTime.Now;
                    fo.CreateUser = userInfo.Id;
                });
            }
            //事务保存
            using (var trans = await _transaction.BeginTransaction())
            {
                try
                {
                    if (rankInfoList.Any())
                        await _rankTitleStore.AddTitleListAsync(rankInfoList);
                    await _scorePeriodStore.CreatePeriodAsync(scorePeriod);
                    trans.Commit();
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    throw new Exception("保存事务失败", e);
                }
            }


           

            // 触发赛季
            await _sendClientMessageManager.SendInfo(Dto.Common.SendClientType.Season);
            return response;
        }

        /// <summary>
        /// 修改赛季
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseMessage> UapdatePeriod(UserInfo userInfo, UpdatePeriodRequest updatePeriodRequest)
        {
            var response = new ResponseMessage();
            var scorePeriod = await _scorePeriodStore.GetScorePeriods().FirstOrDefaultAsync(a => a.Id == updatePeriodRequest.id);
            if (scorePeriod == null)
            {
                response.Code = ResponseCodeDefines.NotFound;
                response.Message = "未找到赛季信息";
                return response;
            }

            scorePeriod.Memo = updatePeriodRequest.Memo;
            scorePeriod.StartDate = updatePeriodRequest.StartDate;
            scorePeriod.Caption = updatePeriodRequest.Caption;
            scorePeriod.UpdateTime = DateTime.Now;
            scorePeriod.UpdateUser = userInfo.Id;
            scorePeriod.EndDate = updatePeriodRequest.EndDate;
            scorePeriod.PlayingUrl = updatePeriodRequest.PlayingUrl;
            scorePeriod.SystemUrl = updatePeriodRequest.SystemUrl;

            await _scorePeriodStore.UpdatePeriodAsync(scorePeriod);
            // 触发赛季
            await _sendClientMessageManager.SendInfo(Dto.Common.SendClientType.Season);
            return response;
        }

        /// <summary>
        /// 开始/结束赛季
        /// </summary>
        /// <param name="updatePeriodState"></param>
        /// <returns></returns>
        public async Task<ResponseMessage> UapdatePeriodState(UpdatePeriodState updatePeriodState)
        {
            var response = new ResponseMessage();
            var scorePeriod = await _scorePeriodStore.GetScorePeriods().FirstOrDefaultAsync(a => a.Id == updatePeriodState.PeriodId);
            if (scorePeriod == null)
            {
                response.Code = ResponseCodeDefines.NotFound;
                response.Message = "未找到赛季信息";
                return response;
            }
            //开始赛季时需要初始化的用户积分信息
            var scoreInfoList = new List<ScoreInfo>();
            switch (updatePeriodState.State)
            {
                case 1:
                    if (scorePeriod.State != 0)
                    {
                        response.Code = ResponseCodeDefines.ModelStateInvalid;
                        response.Message = "只有未开始的赛季才能操作[开始]";
                        return response;
                    }
                    var temp = await _scorePeriodStore.GetScorePeriods().FirstOrDefaultAsync(a => a.State == 1 && a.IsDelete == false);
                    if (temp != null)
                    {
                        response.Code = ResponseCodeDefines.ModelStateInvalid;
                        response.Message = "还有赛季未结束,不能开始新的赛季";
                        return response;
                    }
                    scorePeriod.State = 1;
                    //查询当前赛季需要初始化积分信息的用户
                    var userInfos = _scorePeriodStore.GetNotScoreInfoUsers(scorePeriod.Id);
                    var nowTime = DateTime.Now;
                    scoreInfoList = await userInfos.Select(s => new ScoreInfo
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = s.Id,
                        CreateUser = s.Id,
                        CreateTime = nowTime,
                        IsDelete = false,
                        Score = 0,
                        ConsumableScore = 0,
                        PeriodId = scorePeriod.Id,
                        UpdateTime = nowTime,
                        UpdateUser = s.Id
                    }).ToListAsync();
                    break;
                case 2:
                    if (scorePeriod.State != 1)
                    {
                        response.Code = ResponseCodeDefines.ModelStateInvalid;
                        response.Message = "需要结束的赛季必须是进行中的";
                        return response;
                    }
                    scorePeriod.State = 2;
                    break;
                default:
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "请求修改的状态不符合。";
                    return response;
            }
            using (var trans = await _transaction.BeginTransaction())
            {
                try
                {
                    if (scoreInfoList.Any())
                    {
                        await _scoreInfoStore.CreateScoreInfoList(scoreInfoList);
                    }
                    await _scorePeriodStore.UpdatePeriodAsync(scorePeriod);
                    trans.Commit();
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
            // 触发赛季
            await _sendClientMessageManager.SendInfo(Dto.Common.SendClientType.Season);
            return response;
        }
    }
}
