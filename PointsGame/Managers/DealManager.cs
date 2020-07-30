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
using System.Threading;
using System.Threading.Tasks;

namespace PointsGame.Managers
{
    /// <summary>
    /// 自由交易
    /// </summary>
    public class DealManager
    {
        private readonly IDealStore _iDealStore;
        private readonly HelperDynamic _dynamicHelper;        
        private readonly ITransaction<PointsGameDbContext> _transaction;// 事务
        private readonly HellperPush _hellperEmail;
        private HelperSendClientMessage _sendClientMessageManager;
        public DealManager(IDealStore dealStore, ITransaction<PointsGameDbContext> transaction, HelperDynamic dynamicHelper, HellperPush hellperEmail,
           HelperSendClientMessage sendClientMessageManager)
        {
            _iDealStore = dealStore ?? throw new ArgumentNullException(nameof(dealStore));
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            _dynamicHelper = dynamicHelper ?? throw new ArgumentNullException(nameof(dynamicHelper));
            _hellperEmail = hellperEmail ?? throw new ArgumentNullException(nameof(hellperEmail));
            _sendClientMessageManager = sendClientMessageManager;
        }
        /// <summary>
        /// 查询交易列表
        /// </summary>
        /// <param name="user"></param>
        /// <param name="searchDealRequest"></param>
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<PagingResponseMessage<SearchDealResponse>> SearchDealAsync(UserInfo user, SearchDealRequest searchDealRequest, CancellationToken requestAborted)
        {
            var response = new PagingResponseMessage<SearchDealResponse>();
            var query = _iDealStore.GetSearchDeal(searchDealRequest.PeriodId);

            if (!string.IsNullOrWhiteSpace(searchDealRequest.Theme)) //交易标题
            {
                query = query.Where(deal => deal.Theme.Contains(searchDealRequest.Theme));
            }
            if (!string.IsNullOrWhiteSpace(searchDealRequest.UserName))//交易用户姓名
            {
                query = query.Where(deal => deal.UserName.Contains(searchDealRequest.UserName));
            }
            if (!string.IsNullOrWhiteSpace(searchDealRequest.DealUserName)) //交易对象用户姓名
            {
                query = query.Where(deal => deal.DealUserName.Contains(searchDealRequest.DealUserName));
            }
            if (searchDealRequest.DealType != null) //交易类型
            {
                query = query.Where(deal => deal.DealType == searchDealRequest.DealType);
            }
            if (searchDealRequest.StartTime.HasValue)//交易开始时间
            {
                query = query.Where(deal => deal.CreateTime >= searchDealRequest.StartTime);
            }
            if (searchDealRequest.EndTime.HasValue)//交易结束时间
            {
                query = query.Where(deal => deal.CreateTime <= searchDealRequest.EndTime);
            }
            response.TotalCount = await query.CountAsync(requestAborted);
            query = query.Skip((searchDealRequest.PageIndex) * searchDealRequest.PageSize).Take(searchDealRequest.PageSize);
            response.PageIndex = searchDealRequest.PageIndex;
            response.PageSize = searchDealRequest.PageSize;

            response.Extension = await query.ToListAsync(requestAborted);
            return response;
        }

        /// <summary>
        /// 自由交易-发起交易
        /// </summary>
        /// <param name="user"></param>
        /// <param name="dealSubmitRequest"></param>
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<ResponseMessage> DealSubmitAsync(UserInfo user, DealSubmitRequest dealSubmitRequest, CancellationToken requestAborted)
        {
            var response = new ResponseMessage();
            var periodInfo = await _iDealStore.GetScorePeriods().Where(w => !w.IsDelete && w.Id == dealSubmitRequest.PeriodId).FirstOrDefaultAsync();
            if (periodInfo == null || periodInfo.State != 1)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "只有进行中的赛季才能操作";
                return response;
            }
            var submitUserScoreInfo =await _iDealStore.GetScoreInfos().Where(w => !w.IsDelete && w.PeriodId == dealSubmitRequest.PeriodId && w.UserId == user.Id).FirstOrDefaultAsync();
            var dealUserScoreInfo = await _iDealStore.GetScoreInfos().Where(w => !w.IsDelete && w.PeriodId == dealSubmitRequest.PeriodId && w.UserId == dealSubmitRequest.DealUserId).FirstOrDefaultAsync();
            if (submitUserScoreInfo == null || dealUserScoreInfo==null)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "没有找到对应的积分信息";
                return response;
            }
            if (submitUserScoreInfo.ConsumableScore < dealSubmitRequest.Score)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "交易K币数不能大于自己的可消费K币数";
                return response;
            }
            var userInfodGet = await _iDealStore.GetUserInfos().Where(w => w.Id == dealSubmitRequest.DealUserId).FirstOrDefaultAsync();
            if (userInfodGet==null)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "交易对象用户不存在";
                return response;
            }
            if (userInfodGet.Id == user.Id)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "兄die，还是不要自己跟自己玩了吧，把积分交易给别人吧。";
                return response;
            }

            var nowTime = DateTime.Now;
            var dealNumber = Guid.NewGuid().ToString();//支出收入两条记录同一个编号
            dealSubmitRequest.Score = Math.Abs(dealSubmitRequest.Score);//防止反向交易
            #region 当前用户的支出信息
            var scoreDetailedOut = new ScoreDetailed
            {
                Id = Guid.NewGuid().ToString(),
                PeriodId= dealSubmitRequest.PeriodId,               
                DealNumber= dealNumber,
                DealType=1,
                UserId = user.Id,
                DealUserId = dealSubmitRequest.DealUserId,               
                Theme = dealSubmitRequest.Theme,
                Memo = dealSubmitRequest.Memo,
                Score= dealSubmitRequest.Score,
                ScoreId= submitUserScoreInfo.Id,
                CreateTime = nowTime,
                CreateUser = user.Id,
                IsDelete = false,
            };
            //submitUserScoreInfo.Score = submitUserScoreInfo.Score - dealSubmitRequest.Score;//等级积分减少
            submitUserScoreInfo.ConsumableScore = submitUserScoreInfo.ConsumableScore - dealSubmitRequest.Score;//消费积分减少
            #endregion

            #region 交易对象的收入信息
            var scoreDetailedGet = new ScoreDetailed
            {
                Id = Guid.NewGuid().ToString(),
                PeriodId = dealSubmitRequest.PeriodId,
                DealNumber = dealNumber,
                DealType = 2,
                UserId = dealSubmitRequest.DealUserId,
                DealUserId = user.Id,
                Theme = dealSubmitRequest.Theme,
                Memo = dealSubmitRequest.Memo,
                Score = dealSubmitRequest.Score,
                ScoreId = dealUserScoreInfo.Id,
                CreateTime = nowTime,
                CreateUser = user.Id,
                IsDelete = false,
            };
            //dealUserScoreInfo.Score = dealUserScoreInfo.Score + dealSubmitRequest.Score;//等级积分增加
            dealUserScoreInfo.ConsumableScore= dealUserScoreInfo.ConsumableScore+ dealSubmitRequest.Score;//消费积分增加
            #endregion

            var dynamicContent = string.Empty;

            //事务保存
            using (var trans = await _transaction.BeginTransaction())
            {
                try
                {
                    await _iDealStore.CreateScoreDetailedList(new List<ScoreDetailed> { scoreDetailedOut, scoreDetailedGet }, requestAborted);
                    await _iDealStore.UpdateScoreInfoList(new List<ScoreInfo> { submitUserScoreInfo, dealUserScoreInfo }, requestAborted);
                    //添加支出动态
                    await _dynamicHelper.AddDynamicContent(
                        DynamicType.DealExpenditure, 
                        dealSubmitRequest.PeriodId,
                        scoreDetailedOut.Id,
                        user.UserName,
                        user.GroupName, 
                        userInfodGet.UserName, 
                        userInfodGet.GroupName,
                        user.Id, 
                        dealSubmitRequest.Theme, 
                        dealSubmitRequest.Score,
                        null,
                        null
                      );
                    //添加收入动态
                    dynamicContent= await _dynamicHelper.AddDynamicContent(
                                   DynamicType.DealIncome,
                                   dealSubmitRequest.PeriodId,
                                   scoreDetailedOut.Id,
                                   user.UserName,
                                   user.GroupName,
                                   userInfodGet.UserName,
                                   userInfodGet.GroupName,
                                   userInfodGet.Id,
                                   dealSubmitRequest.Theme,
                                   dealSubmitRequest.Score,
                                   null,
                                   null
                                 );
                    trans.Commit();
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    throw new Exception("保存事务失败", e);
                }
            }
            // 触发个人信息,动态变化
            await _sendClientMessageManager.SendInfos(new List<Dto.Common.SendClientType>() 
            {                
                Dto.Common.SendClientType.Dynamic,
                Dto.Common.SendClientType.User
            });
            #region K币交易发送提醒            

            _hellperEmail.SendEmpPush($"您在《{periodInfo.Caption}》收到了K币，快去看看吧！",
                                     $"尊敬的勇士您好：{dynamicContent}赶紧去看看吧。",
                                     new List<string> { userInfodGet.UserId });

            #endregion

            return response;
        }
    }
}
