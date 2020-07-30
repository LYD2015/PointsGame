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
    /// K币派发
    /// </summary>
    public class AllocateManager
    {
        private readonly IAllocateStore _iAllocateStore;
        private readonly HelperDynamic _dynamicHelper;
        private readonly ITransaction<PointsGameDbContext> _transaction;//事务
        private readonly HellperPush _hellperEmail;
        private HelperSendClientMessage _sendClientMessageManager;
        public AllocateManager(IAllocateStore allocateStore, ITransaction<PointsGameDbContext> transaction, HelperDynamic dynamicHelper, HellperPush hellperEmail,
            HelperSendClientMessage sendClientMessageManager)
        {
            _iAllocateStore = allocateStore ?? throw new ArgumentNullException(nameof(allocateStore));
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            _dynamicHelper = dynamicHelper ?? throw new ArgumentNullException(nameof(dynamicHelper));
            _hellperEmail = hellperEmail ?? throw new ArgumentNullException(nameof(hellperEmail));
            _sendClientMessageManager = sendClientMessageManager;
        }

        /// <summary>
        /// 查询派发列表
        /// </summary>
        /// <param name="user"></param>
        /// <param name="searchAllocateRequest"></param>
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<PagingResponseMessage<SearchAllocateResponse>> SearchAllocateAsync(UserInfo user, SearchAllocateRequest searchAllocateRequest, CancellationToken requestAborted)
        {
            var response = new PagingResponseMessage<SearchAllocateResponse>();
            var query = _iAllocateStore.GetSearchAllocate(searchAllocateRequest.PeriodId);

            if (!string.IsNullOrWhiteSpace(searchAllocateRequest.Theme)) //派发原因
            {
                query = query.Where(item => item.Theme.Contains(searchAllocateRequest.Theme));
            }
            if (!string.IsNullOrWhiteSpace(searchAllocateRequest.UserName))//派发人
            {
                query = query.Where(item => item.UserName.Contains(searchAllocateRequest.UserName));
            }
            if (!string.IsNullOrWhiteSpace(searchAllocateRequest.DealUserName)) //幸运儿
            {
                query = query.Where(item => item.DealUserName.Contains(searchAllocateRequest.DealUserName));
            }
            if (searchAllocateRequest.StartTime.HasValue) //派发开始时间
            {
                query = query.Where(item => item.CreateTime >= searchAllocateRequest.StartTime);
            }
            if (searchAllocateRequest.EndTime.HasValue) //派发结束时间
            {
                query = query.Where(item => item.CreateTime <= searchAllocateRequest.EndTime);
            }
            response.TotalCount = await query.CountAsync();
            query = query.Skip((searchAllocateRequest.PageIndex) * searchAllocateRequest.PageSize).Take(searchAllocateRequest.PageSize);
            response.PageIndex = searchAllocateRequest.PageIndex;
            response.PageSize = searchAllocateRequest.PageSize;

            response.Extension = await query.ToListAsync();
            response.Extension.ForEach(fo =>
            {
                if (fo.UserId == "K先生")
                {
                    fo.UserName = "K先生";
                }
            });
            return response;
        }

        /// <summary>
        /// 派发K币-提交
        /// </summary>
        /// <param name="user"></param>
        /// <param name="allocateSubmitRequest"></param>
        /// <param name="requestAborted"></param>
        /// <param name="isEgg">是否彩蛋，默认不是,主要控制邮件发送</param>
        /// <returns></returns>
        public async Task<ResponseMessage> AllocateSubmitAsync(UserInfo user, AllocateSubmitRequest allocateSubmitRequest, CancellationToken requestAborted, bool isEgg = false)
        {
            var response = new ResponseMessage();

            var periodInfo = await _iAllocateStore.GetScorePeriods().Where(w => !w.IsDelete && w.Id == allocateSubmitRequest.PeriodId).FirstOrDefaultAsync();
            if (periodInfo == null || periodInfo.State != 1)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "只有进行中的赛季才能操作";
                return response;
            }
            var scoreInfo = await _iAllocateStore.GetScoreInfos().Where(w => !w.IsDelete && w.PeriodId == allocateSubmitRequest.PeriodId && w.UserId == allocateSubmitRequest.AllocateUserId).FirstOrDefaultAsync();
            if (scoreInfo == null)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "没有找到派发对象的积分信息";
                return response;
            }

            var allocateUserInfo = await _iAllocateStore.GetUserInfos().Where(w => w.Id == allocateSubmitRequest.AllocateUserId).FirstOrDefaultAsync();
            if (allocateUserInfo == null)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "派发对象用户不存在";
                return response;
            }

            #region 派发对象的收入信息
            var scoreDetailed = new ScoreDetailed
            {
                Id = Guid.NewGuid().ToString(),
                PeriodId = allocateSubmitRequest.PeriodId,
                DealType = 4,
                UserId = user.Id,//派发人的ID
                DealUserId = allocateSubmitRequest.AllocateUserId,
                Theme = allocateSubmitRequest.Theme,
                Memo = allocateSubmitRequest.Memo,
                Score = allocateSubmitRequest.Score ?? 1,
                ScoreId = scoreInfo.Id,
                CreateTime = DateTime.Now,
                CreateUser = user.Id,
                IsDelete = false,
                Labels = allocateSubmitRequest.Labels
            };
            scoreInfo.Score = scoreInfo.Score + allocateSubmitRequest.Score ?? 0;
            scoreInfo.ConsumableScore = scoreInfo.ConsumableScore + allocateSubmitRequest.Score ?? 0;
            #endregion

            #region 派发对象的印象标签
            var userLabelList = new List<UserLabel>();
            if (!string.IsNullOrWhiteSpace(allocateSubmitRequest.Labels))
            {
                //标签处理,现在只是去重做存储
                var labelList = allocateSubmitRequest.Labels.Replace("，", ",").Split(",").Where(w => !string.IsNullOrWhiteSpace(w)).Select(s => s.ToLower()).Distinct().ToList();//请求的标签
                if (labelList.Where(w => w.Length > 12).Count() > 0)
                {
                    response.Code = ResponseCodeDefines.NotAllow;
                    response.Message = "单个标签必须小于6个字";
                    return response;
                }
                foreach (var lable in labelList)
                {
                    userLabelList.Add(new UserLabel
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = allocateUserInfo.Id,
                        Label = lable,
                    });
                }
            }
            #endregion
            //事务保存
            using (var trans = await _transaction.BeginTransaction())
            {
                try
                {
                    await _iAllocateStore.CreateScoreDetailed(scoreDetailed, requestAborted);
                    await _iAllocateStore.UpdateScoreInfo(scoreInfo, requestAborted);
                    if (userLabelList != null && userLabelList.Count != 0)
                        await _iAllocateStore.CreateUserLabelList(userLabelList, requestAborted);

                    //添加派发动态
                    await _dynamicHelper.AddDynamicContent(
                       DynamicType.DistributeIncome,
                       allocateSubmitRequest.PeriodId,
                       scoreDetailed.Id,
                       null,
                       null,
                       allocateUserInfo.UserName,
                       allocateUserInfo.GroupName,
                       allocateUserInfo.Id,
                       allocateSubmitRequest.Theme,
                       allocateSubmitRequest.Score ?? 1,
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
            #region 派发积分发送提醒  

            if (isEgg)
            {
                //一个彩蛋一个用户只会提醒一次
                var isSend = await _iAllocateStore.GetScoreDetaileds().AnyAsync(w => !w.IsDelete && w.Memo == allocateSubmitRequest.Memo && w.DealUserId == allocateSubmitRequest.AllocateUserId);
                if (isSend)
                {
                    return response;
                }
            }
            _hellperEmail.SendEmpPush("您收到了新的积分，快去看看吧！",
                                     $"尊敬的勇士您好：您在《{periodInfo.Caption}》因为《{allocateSubmitRequest.Theme}》获得了{allocateSubmitRequest.Score ?? 1}K币，赶紧去看看吧。",
                                     new List<string> { allocateUserInfo.UserId });

            #endregion

            // 触发排行榜,个人信息,动态变化
            await _sendClientMessageManager.SendInfos(new List<Dto.Common.SendClientType>()
            {
                Dto.Common.SendClientType.Rank,
                Dto.Common.SendClientType.Dynamic,
                Dto.Common.SendClientType.User
            });
            return response;

        }
    }
}
