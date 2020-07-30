using ApiCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PointsGame.Dto.Common;
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
    public class GiftManager
    {
        private readonly IAllocateStore _allocateStore;
        private readonly IGiftStore  _giftStore;
        private readonly IConfigurationRoot _config;
        private readonly ITransaction<PointsGameDbContext> _transaction;//事务
        private readonly HelperDynamic _dynamicHelper;
        private readonly ISearchStore _iSearchStore;

        private HelperSendClientMessage _sendClientMessageManager;
        public GiftManager(IAllocateStore allocateStore, IGiftStore giftStore, IConfigurationRoot configuration, ITransaction<PointsGameDbContext> transaction, HelperDynamic dynamicHelper,
            ISearchStore searchStore, HelperSendClientMessage sendClientMessageManager)
        {
            _allocateStore = allocateStore;
            _giftStore = giftStore;
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            _dynamicHelper = dynamicHelper ?? throw new ArgumentNullException(nameof(dynamicHelper));
            _iSearchStore = searchStore;
            _sendClientMessageManager = sendClientMessageManager;
        }

        /// <summary>
        /// 赛季奖品管理-查询赛季奖品的说明
        /// </summary>
        /// <param name="user"></param>
        /// <param name="periodId"></param>
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<PeriodGift>> GetPeriodIdMemoAsync(UserInfo user, string periodId, CancellationToken requestAborted)
        {
            var response = new ResponseMessage<PeriodGift>();
            response.Extension = await _giftStore.GetPeriodGifts().Where(w => w.PeriodId == periodId).FirstOrDefaultAsync();            
            return response;
        }

        /// <summary>
        /// 赛季奖品管理-更新赛季奖品的说明
        /// </summary>
        /// <param name="user"></param>
        /// <param name="request"></param>
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<ResponseMessage> MemoUpdateAsync(UserInfo user, PeriodGiftMemoRequest request, CancellationToken requestAborted)
        {
            var response = new ResponseMessage();
            var periodGiftInfo = await _giftStore.GetPeriodGifts().Where(w =>w.PeriodId == request.PeriodId).FirstOrDefaultAsync();
            if (periodGiftInfo == null)
            {
                periodGiftInfo = new PeriodGift
                {
                    Id = Guid.NewGuid().ToString(),
                    Enabled = request.Enabled,
                    PeriodId = request.PeriodId,
                    Score = request.Score,
                    Memo = request.Memo
                };
                await _giftStore.AddPeriodGift(periodGiftInfo, requestAborted);
            }
            else
            {
                periodGiftInfo.Enabled = request.Enabled;
                periodGiftInfo.PeriodId = request.PeriodId;
                periodGiftInfo.Score = request.Score;
                periodGiftInfo.Memo = request.Memo;
                await _giftStore.UpdatePeriodGift(periodGiftInfo, requestAborted);
            }
            // 触发赛季信息变化
            await _sendClientMessageManager.SendInfos(new List<Dto.Common.SendClientType>()
            {
                Dto.Common.SendClientType.Season
            });
            return response;
        }

        /// <summary>
        /// 赛季奖品管理-列表
        /// </summary>
        /// <param name="user"></param>
        /// <param name="periodId"></param>
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<List<GiftInfo>>> GetGiftAsync(UserInfo user, string periodId, CancellationToken requestAborted)
        {
            var response = new ResponseMessage<List<GiftInfo>>();
            var giftInfoList = await _giftStore.GetGiftInfos().Where(w => !w.IsDelete && w.PeriodId == periodId).OrderBy(o=>o.Order).ToListAsync();
            giftInfoList.ForEach(fo => 
            {
                fo.ImageUrl = _config["FileUrl"] + fo.ImageUrl;
            });
            response.Extension = giftInfoList;
            return response;
        }

        /// <summary>
        /// 赛季奖品管理-新增/修改赛季奖品
        /// </summary>
        /// <param name="user"></param>
        /// <param name="request"></param>
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<ResponseMessage> GiftAddOrUpdateAsync(UserInfo user, GiftAddRequest request, CancellationToken requestAborted)
        {
            var response = new ResponseMessage();
            if (string.IsNullOrWhiteSpace(request.Id))
            {
                var giftQuery = _giftStore.GetGiftInfos().Where(w => !w.IsDelete && w.PeriodId == request.PeriodId);

                if ((giftQuery.Sum(s => s.Odds) + request.Odds)>1000)
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "中奖几率总和不能大于1000";
                    return response;
                }
                if (giftQuery.Count() >= 8)
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "每个赛季礼品数量最多8个";
                    return response;
                }

                var giftInfo = new GiftInfo
                {
                    Id = Guid.NewGuid().ToString(),
                    PeriodId = request.PeriodId,
                    CreateTime = DateTime.Now,
                    CreateUser = user.Id,
                    IsDelete = false,
                    Name = request.Name,
                    Number = request.Number,
                    Odds = request.Odds,
                    ImageUrl = request.ImageUrl.Replace(_config["FileUrl"], ""),
                    IsGet = request.IsGet,
                    GetScore = request.GetScore,
                    Order = request.Order
                };
                await _giftStore.AddGift(giftInfo, requestAborted);
            }
            else
            {
                var giftInfo = await _giftStore.GetGiftInfos().Where(w => !w.IsDelete && w.PeriodId == request.PeriodId && w.Id == request.Id).FirstOrDefaultAsync();
                if (giftInfo == null)
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "未找到对应的奖品信息";
                    return response;
                }
                giftInfo.PeriodId = request.PeriodId;
                giftInfo.Name = request.Name;
                giftInfo.Number = request.Number;
                giftInfo.Odds = request.Odds;
                giftInfo.ImageUrl = request.ImageUrl.Replace(_config["FileUrl"], "");
                giftInfo.IsGet = request.IsGet;
                giftInfo.GetScore = request.GetScore;
                giftInfo.Order = request.Order;
                await _giftStore.UpdateGift(giftInfo, requestAborted);
            }
            return response;
        }

        /// <summary>
        /// 赛季奖品管理-删除赛季奖品
        /// </summary>
        /// <param name="user"></param>
        /// <param name="id"></param>
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<ResponseMessage> GiftDeleteAsync(UserInfo user, string id, CancellationToken requestAborted)
        {
            var response = new ResponseMessage();
            var gitfInfo = await _giftStore.GetGiftInfos().Where(w => !w.IsDelete && w.Id == id).FirstOrDefaultAsync();
            if (gitfInfo == null)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "未找到对应的奖品信息";
                return response;
            }
            gitfInfo.IsDelete = true;
            await _giftStore.UpdateGift(gitfInfo, requestAborted);
            return response;
        }

        /// <summary>
        /// 抽奖/兑奖-奖品列表
        /// </summary>
        /// <param name="user"></param>
        /// <param name="periodId"></param>
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<List<GiftInfo>>> GetConsumableGiftAsync(UserInfo user, string periodId, CancellationToken requestAborted)
        {
            var response = new ResponseMessage<List<GiftInfo>>();
            var periodGift = await _giftStore.GetPeriodGifts().Where(w => w.PeriodId == periodId).FirstOrDefaultAsync();
            if (periodGift == null || periodGift.Enabled == false)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "当前赛季奖品暂未开放";
                return response;
            }
            var giftInfoList = await _giftStore.GetGiftInfos().Where(w => !w.IsDelete && w.PeriodId == periodId).OrderBy(o=>o.Order).ToListAsync();
            if (!giftInfoList.Any())
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "没有奖品";
                return response;
            }
            giftInfoList.ForEach(fo =>
            {
                fo.ImageUrl = _config["FileUrl"] + fo.ImageUrl;
            });
            response.Extension = giftInfoList;
            return response;
        }

        /// <summary>
        /// 抽奖/兑奖
        /// </summary>
        /// <param name="user"></param>
        /// <param name="request"></param>        
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<GiftInfo>> ConsumableScoreAsync(UserInfo user, ConsumableScoreRequest request, CancellationToken requestAborted)
        {
            var response = new ResponseMessage<GiftInfo>();
            var periodGift =await _giftStore.GetPeriodGifts().Where(w => w.PeriodId == request.PeriodId).FirstOrDefaultAsync();
            if (periodGift == null|| periodGift.Enabled==false)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "当前赛季奖品暂未开放";
                return response;
            }
            //抽奖
            if (request.ConsumableType == 1)
            {
                var giftInfoList = await _giftStore.GetGiftInfos().Where(w => !w.IsDelete && w.PeriodId == request.PeriodId).ToListAsync();
                if (!giftInfoList.Any())
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "没有奖品可供抽取";
                    return response;
                }
                var scoreInfo = await _allocateStore.GetScoreInfos().Where(w => !w.IsDelete && w.PeriodId == request.PeriodId && w.UserId == user.Id).FirstOrDefaultAsync();
                if (scoreInfo == null)
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "未查询到您的积分信息";
                    return response;
                }
                if (scoreInfo.ConsumableScore < periodGift.Score)
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "您的可消费K币不足,赶紧去赚点K币吧";
                    return response;
                }

                //总几率
                var sumOdds = giftInfoList.Sum(s => s.Odds);
                if (sumOdds > 1000)
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "奖品设置异常,请联系管理员。";
                    return response;
                }
                var giftList = giftInfoList.Where(w => w.Number != 0).ToList();//排除抽完的奖品
                if (!giftList.Any())
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "所有奖品已被抽取/兑换完,请等待新奖品上架吧。";
                    return response;
                }

                //将奖品ID和中奖率放在数组中,并从小到大排序               
                var prizeList = giftList.Select(s => new { Prize = s.Id, Change = s.Odds }).OrderBy(o => o.Change).ToList();
                //将中奖率累加,放到字典里
                var prizeDictionary = new Dictionary<string, int>();
                for (var i = 0; i < giftList.Count; i++)
                {
                    var allChange = 0;
                    for (var j = 0; j <= i; j++)
                    {
                        allChange += prizeList[j].Change;
                    }
                    prizeDictionary.Add(prizeList[i].Prize, allChange);
                }
                //产生一个1-1000的随机数
                var rd = new Random();
                var rdChange = rd.Next(1, 1001);
                //找第一个大于随机值的奖项，如果中途有奖品抽完了，找不到到大于随机值的奖项，就直接取几率最大
                var giftResult_Id = prizeDictionary.FirstOrDefault(j => j.Value >= rdChange).Key ?? prizeDictionary.LastOrDefault().Key;
                var giftInfo = giftInfoList.FirstOrDefault(fi => fi.Id == giftResult_Id);               

                if (giftInfo.Number > 0)
                {
                    giftInfo.Number -= 1;
                }
                
                //事务保存
                using (var trans = await _transaction.BeginTransaction())
                {
                    try
                    {
                        //事务里面重新查一次
                        scoreInfo = await _allocateStore.GetScoreInfos().Where(w => !w.IsDelete && w.PeriodId == request.PeriodId && w.UserId == user.Id).FirstOrDefaultAsync();
                        if (scoreInfo == null)
                        {
                            response.Code = ResponseCodeDefines.ModelStateInvalid;
                            response.Message = "未查询到您的积分信息";
                            return response;
                        }
                        scoreInfo.ConsumableScore = scoreInfo.ConsumableScore - periodGift.Score;
                        await _allocateStore.UpdateScoreInfo(scoreInfo, requestAborted);
                        await _giftStore.UpdateGift(giftInfo, requestAborted);

                        //添加抽奖动态
                        await _dynamicHelper.AddDynamicContent(
                           DynamicType.RandomGift,
                           request.PeriodId,
                           giftInfo.Id,
                           null,
                           null,
                           user.UserName,
                           user.GroupName,
                           user.Id,
                           giftInfo.Name,
                           periodGift.Score,
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
                response.Extension = giftInfo;
            }
            //兑奖
            else if (request.ConsumableType == 2)
            {

                var scoreInfo = await _allocateStore.GetScoreInfos().Where(w => !w.IsDelete && w.PeriodId == request.PeriodId && w.UserId == user.Id).FirstOrDefaultAsync();
                if (scoreInfo == null)
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "未查询到您的积分信息";
                    return response;
                }
                var giftInfo = await _giftStore.GetGiftInfos().Where(w => !w.IsDelete && w.PeriodId == request.PeriodId && w.Number != 0 && w.Id == request.GiftId).FirstOrDefaultAsync();
                if (giftInfo == null)
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "兑换奖品不存在";
                    return response;
                }
                if (!giftInfo.IsGet)
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = $"兑换奖品{giftInfo.Name}不运行直接兑换";
                    return response;
                }
                if (scoreInfo.ConsumableScore < giftInfo.GetScore)
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "您的可消费K币不足,赶紧去赚点K币吧";
                    return response;
                }
                if (giftInfo.Number == 0)
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "奖品数量不足不能直接兑换";
                    return response;
                }
                if (giftInfo.Number > 0)
                {
                    giftInfo.Number -= 1;
                }
                
                //事务保存
                using (var trans = await _transaction.BeginTransaction())
                {
                    try
                    {
                        //事务里面重新查一次
                        scoreInfo = await _allocateStore.GetScoreInfos().Where(w => !w.IsDelete && w.PeriodId == request.PeriodId && w.UserId == user.Id).FirstOrDefaultAsync();
                        if (scoreInfo == null)
                        {
                            response.Code = ResponseCodeDefines.ModelStateInvalid;
                            response.Message = "未查询到您的积分信息";
                            trans.Rollback();
                            return response;
                        }
                        scoreInfo.ConsumableScore = scoreInfo.ConsumableScore - giftInfo.GetScore;
                        await _allocateStore.UpdateScoreInfo(scoreInfo, requestAborted);
                        await _giftStore.UpdateGift(giftInfo, requestAborted);
                        //添加直接兑换动态
                        await _dynamicHelper.AddDynamicContent(
                           DynamicType.GetGift,
                           request.PeriodId,
                           giftInfo.Id,
                           null,
                           null,
                           user.UserName,
                           user.GroupName,
                           user.Id,
                           giftInfo.Name,
                           periodGift.Score,
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
                response.Extension = giftInfo;
            }
            else 
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "ConsumableType参数错误";
                return response;
            }

            // 触发个人信息变化
            await _sendClientMessageManager.SendInfos(new List<Dto.Common.SendClientType>()
            {
                Dto.Common.SendClientType.User,
                Dto.Common.SendClientType.Dynamic
            });

            return response;
        }


        /// <summary>
        /// 查询抽奖记录列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<PagingResponseMessage<SearchDynamicResponse>> SearchConsumableRecordAsync(ConsumableRecordRequest request, CancellationToken requestAborted)
        {
            var response = new PagingResponseMessage<SearchDynamicResponse>();
            var query = _iSearchStore.GetDynamicInfos().Where(w => w.PeriodId == request.PeriodId && (w.DynamicType == (int)DynamicType.RandomGift || w.DynamicType == (int)DynamicType.GetGift));
            if (!string.IsNullOrWhiteSpace(request.UserNmae))
            {
                query = query.Where(w => w.UserFullName.Contains(request.UserNmae)||w.DynamicContent.Contains(request.UserNmae));
            }
            query = query.OrderByDescending(o => o.CreateTime);
            response.TotalCount = await query.CountAsync();
            response.PageIndex = request.PageIndex;
            response.PageSize = request.PageSize;
            response.Extension = await query.Skip(request.PageIndex * request.PageSize).Take(request.PageSize).Select(s => new SearchDynamicResponse
            {
                Id = s.Id,
                DataId = s.DataId,
                PeriodId = s.PeriodId,
                UserId = s.UserId,
                UserFullName = s.UserFullName,
                Score = s.Score,
                DynamicType = s.DynamicType,
                Adjective = s.Adjective,
                DynamicContent = s.DynamicContent,
                CreateTime = s.CreateTime
            }).ToListAsync();
            return response;
        }

        /// <summary>
        /// 彩票开始开奖
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseMessage> LotteryRunAsync(CancellationToken requestAborted)
        {
            var response = new ResponseMessage();
            var periodInfo = await _allocateStore.GetScorePeriods().Where(w => w.State == 1 && !w.IsDelete).FirstOrDefaultAsync();
            if (periodInfo == null)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "没有进行中的赛季,开奖失败";                
                return response;
            }

            var nowTime = DateTime.Now;
            var numberList = new List<int>();//开奖号码
            var numberPeriods =await GetNumberPeriods();//本次开奖期数

            #region 生成中奖号码
            //生成6个不相同的号码            
            while (numberList.Count<6)
            {
                var rd = new Random();
                var rdNext = rd.Next(0, 26);
                if (numberList.Any(a => a == rdNext))
                {
                    continue;
                }
                numberList.Add(rdNext);
            }
            numberList = numberList.OrderBy(o => o).ToList();
            #endregion

            #region 计算中奖的人
            var lotteryUser = await _giftStore.GetLotteryUsers().Where(w => w.NumberPeriods == numberPeriods && w.PeriodId == periodInfo.Id).ToListAsync();//当期投注的人
            var number6Count = 0; var number6Score = 0;//中6个的注数和总分数
            var number5Count = 0; var number5Score = 0;//中5个的注数和总分数
            var number4Count = 0; var number4Score = 0;//中4个的注数和总分数
            var number3Count = 0; var number3Score = 0;//中3个的注数和总分数
            var number2Count = 0; var number2Score = 0;//中2个的注数和总分数
            var inputScore = lotteryUser.Sum(s => s.Score);//总投注K币
            foreach (var lu in lotteryUser)
            {
                var winNumber = new List<int>();//中奖的号码
                var buyNumber = lu.Number.Split(',');//购买的号码
                foreach (var bn in buyNumber)
                {
                    if (numberList.Any(a => a == int.Parse(bn)))
                    {
                        winNumber.Add(int.Parse(bn));
                    }
                }                
                winNumber = winNumber.OrderBy(o => o).ToList();

                //中2个号，保本
                if (winNumber.Count == 2)
                {
                    lu.WinningNumber = string.Join(',', winNumber);
                    lu.WinningScore = lu.Score;
                    number2Count++;
                    number2Score += lu.WinningScore ?? 0;
                }
                //中3个号，翻2倍
                if (winNumber.Count == 3)
                {
                    lu.WinningNumber = string.Join(',', winNumber);
                    lu.WinningScore = lu.Score * 2;
                    number3Count++;
                    number3Score += lu.WinningScore ?? 0;
                }
                //中4个号，翻10倍
                if (winNumber.Count == 4)
                {
                    lu.WinningNumber = string.Join(',', winNumber);
                    lu.WinningScore = lu.Score * 10;
                    number4Count++;
                    number4Score += lu.WinningScore ?? 0;
                }
                //中5个号，翻300倍
                if (winNumber.Count == 5)
                {
                    lu.WinningNumber = string.Join(',', winNumber);
                    lu.WinningScore = lu.Score * 300;
                    number5Count++;
                    number5Score += lu.WinningScore ?? 0;
                }
                //中6个号，翻5000倍
                if (winNumber.Count == 6)
                {
                    lu.WinningNumber = string.Join(',', winNumber);
                    lu.WinningScore = lu.Score * 5000;
                    number6Count++;
                    number6Score += lu.WinningScore ?? 0;
                }
            }
            lotteryUser = lotteryUser.Where(w => !string.IsNullOrEmpty(w.WinningNumber)).ToList();//筛选出中奖的人
            #endregion

            var lotteryResult = new LotteryResult
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = nowTime,
                NumberPeriods = numberPeriods,
                Number = string.Join(',', numberList),
                WinResult=JsonHelper.ToJson(new 
                {
                    All = new { Count = lotteryUser.Count, OutScore = lotteryUser.Sum(s => s.WinningScore), InputScore = inputScore },
                    Number6 = new { Count = number6Count, Score = number6Score },
                    Number5 = new { Count = number5Count, Score = number5Score },
                    Number4 = new { Count = number4Count, Score = number4Score },
                    Number3 = new { Count = number3Count, Score = number3Score },
                    Number2 = new { Count = number2Count, Score = number2Score },
                })
            };


            var userIds = lotteryUser.Select(s => s.UserId).ToList();
            var userInfoList = await _allocateStore.GetUserInfos().Where(w => !w.IsDelete && userIds.Contains(w.Id)).ToListAsync();
            //事务保存
            using (var trans = await _transaction.BeginTransaction())
            {
                try
                {
                    //事务里面重新查一次
                    var  scoreInfoList = await _allocateStore.GetScoreInfos().Where(w => !w.IsDelete && w.PeriodId == periodInfo.Id && userIds.Contains(w.UserId)).ToListAsync();
                    foreach (var lu in lotteryUser)
                    {

                        var storeInfo = scoreInfoList.FirstOrDefault(fi => fi.UserId == lu.UserId);
                        if (storeInfo != null)
                        {
                            storeInfo.ConsumableScore = storeInfo.ConsumableScore + lu.WinningScore.Value;
                            await _allocateStore.UpdateScoreInfo(storeInfo, requestAborted);                            
                            await _giftStore.UpdateLotteryUser(lu, requestAborted);
                            //添加中彩票动态
                            await _dynamicHelper.AddDynamicContent(
                               DynamicType.WinLottery,
                               periodInfo.Id,
                               lu.Id,
                               null,
                               null,
                               userInfoList.FirstOrDefault(fi => fi.Id == lu.UserId)?.UserName,
                               userInfoList.FirstOrDefault(fi => fi.Id == lu.UserId)?.GroupName,
                               lu.UserId,
                               $"在K彩第《{lu.NumberPeriods}》期投中号码:{lu.WinningNumber}",
                               lu.WinningScore.Value,
                               null,
                               null
                             );
                        } 
                    }
                    await _giftStore.AddLotteryResult(lotteryResult, requestAborted);
                    //添加开奖动态
                    await _dynamicHelper.AddDynamicContent(
                       DynamicType.LotteryResult,
                       periodInfo.Id,
                       lotteryResult.Id,
                       null,
                       null,
                       "K先生",
                       "K先生",
                       "K先生",
                       $"⭐K彩⭐第《{lotteryResult.NumberPeriods}》期开奖结果:{lotteryResult.Number}。共中出{lotteryUser.Count}注，共中出{lotteryUser.Sum(s => s.WinningScore)}K。" +
                        $"{(number6Count > 0 ? "一等奖" + number6Count + "注,恭喜走上人生巅峰。" : "")}" +
                       $"{(number5Count > 0?"二等奖"+ number5Count + "注,恭喜一夜暴富。" : "")}" +                      
                       $"{(number4Count > 0 ? "三等奖共" + number4Count + $"注。" : "")}" +
                       $"{(number3Count > 0 ? "四等奖共" + number3Count + "注。" : "")}" +
                       $"{(number2Count > 0 ? "五等奖共" + number2Count + "注。" : "")}" +
                       $"单注最高奖{lotteryUser.Max(m=>m.WinningScore)}K。",
                       0,
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
            // 触发信息变化
            await _sendClientMessageManager.SendInfos(new List<Dto.Common.SendClientType>()
            {
                Dto.Common.SendClientType.Dynamic
            });
            return response;
        }

        /// <summary>
        /// 彩票投注
        /// </summary>
        /// <param name="user"></param>
        /// <param name="request"></param>
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<ResponseMessage> LotteryBetAsync(UserInfo user, LotteryBetRequest request, CancellationToken requestAborted)
        {
            var response = new ResponseMessage();
            var periodInfo = await _allocateStore.GetScorePeriods().Where(w => w.State == 1 && !w.IsDelete).FirstOrDefaultAsync();
            if (periodInfo == null)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "没有进行中的赛季,暂时不能投注";
                return response;
            }
            if (periodInfo.Id != request.PeriodId)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "请到主页先选择进行中的赛季。";
                return response;
            }

            if (request.BetNumberList.Any(w => w.BetNumber.Count != 6))
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "投注号码格式不正确";
                return response;
            }
            foreach (var item in request.BetNumberList)
            {
                if (item.BetNumber.Distinct().Count() != item.BetNumber.Count)
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "同一注号码中不能重复选号";
                    return response;
                }
                if (item.BetScore <= 0)
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "投注K币数必须大于零";
                    return response;
                }
                if (item.BetScore >500)
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "投注K币数不能大于500K";
                    return response;
                }
                item.BetNumber = item.BetNumber.OrderBy(o => o).ToList();//排序
            }

            var scoreInfo = await _allocateStore.GetScoreInfos().Where(w => !w.IsDelete && w.PeriodId == request.PeriodId && w.UserId == user.Id).FirstOrDefaultAsync();
            if (scoreInfo == null)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "未查询到您的积分信息";
                return response;
            }
            if (scoreInfo.ConsumableScore < request.BetNumberList.Sum(s=>s.BetScore))
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "您的可投注的K币不足,赶紧去赚点K币吧";
                return response;
            }

            var nowTime = DateTime.Now;
            var numberPeriods = await GetNumberPeriods();//投注期数

            var loteryUserList = request.BetNumberList.Select(s => new LotteryUser 
            {
                Id = Guid.NewGuid().ToString(),
                PeriodId=request.PeriodId,
                UserId=user.Id,
                NumberPeriods=numberPeriods,
                Number = string.Join(',', s.BetNumber),
                Score=s.BetScore,
                CreateTime=nowTime                
            }).ToList();

            //事务保存
            using (var trans = await _transaction.BeginTransaction())
            {
                try
                {
                    //事务里面重新查一次
                    scoreInfo = await _allocateStore.GetScoreInfos().Where(w => !w.IsDelete && w.PeriodId == request.PeriodId && w.UserId==user.Id).FirstOrDefaultAsync();
                    if (scoreInfo == null)
                    {
                        response.Code = ResponseCodeDefines.ModelStateInvalid;
                        response.Message = "未查询到您的积分信息";
                        return response;
                    }
                    scoreInfo.ConsumableScore = scoreInfo.ConsumableScore - request.BetNumberList.Sum(s => s.BetScore).GetValueOrDefault(0);
                    await _allocateStore.UpdateScoreInfo(scoreInfo, requestAborted);
                    await _giftStore.AddLotteryUserList(loteryUserList, requestAborted);
                    //添加够买彩票动态动态
                    await _dynamicHelper.AddDynamicContent(
                       DynamicType.BuyLottery,
                       request.PeriodId,
                       scoreInfo.Id,
                       null,
                       null,
                       user.UserName,
                       user.GroupName,
                       user.Id,
                       numberPeriods,
                       loteryUserList.Sum(s=>s.Score.Value),
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
            // 触发信息变化
            await _sendClientMessageManager.SendInfos(new List<Dto.Common.SendClientType>()
            {
                Dto.Common.SendClientType.User,
                Dto.Common.SendClientType.Dynamic
            });
            return response;
        }

        /// <summary>
        /// 彩票投注（测试）
        /// </summary>
        /// <param name="user"></param>
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<ResponseMessage> LotteryBetTestAsync(string periodId, CancellationToken requestAborted)
        {
            var response = new ResponseMessage();

           
            var numberPeriods = await GetNumberPeriods();//本次开奖期数
            var loteryUserList = new List<LotteryUser>();
            for (int i = 0; i < 100; i++)
            {
                var numberList = new List<int>();//开奖号码
                //生成6个不相同的号码            
                while (numberList.Count < 6)
                {
                    var rd = new Random();
                    var rdNext = rd.Next(0, 26);
                    if (numberList.Any(a => a == rdNext))
                    {
                        continue;
                    }
                    numberList.Add(rdNext);
                }
                numberList = numberList.OrderBy(o => o).ToList();
                loteryUserList.Add(new LotteryUser
                {
                    Id = Guid.NewGuid().ToString(),
                    PeriodId = periodId,
                    UserId = "32561d1b-8a56-4432-9a39-dd25f442ea76",
                    NumberPeriods = numberPeriods,
                    Number = string.Join(',', numberList),
                    Score = 1,
                    CreateTime = DateTime.Now
                });
                numberList = new List<int>();
            }           

            await _giftStore.AddLotteryUserList(loteryUserList, requestAborted);

            return response;
        }

        /// <summary>
        /// 计算下一期期数
        /// </summary>
        /// <param name="nowTime"></param>
        /// <returns></returns>
        private async Task<string> GetNumberPeriods()
        {           
            var numberPeriods = string.Empty;
            var lastResult = await _giftStore.GetLotteryResults().OrderByDescending(o => o.CreateTime).FirstOrDefaultAsync();//最后一次开奖结果
            if (lastResult == null)
            {
                numberPeriods = "000001";//如果一个开奖结果都没有,就从当前日期第一期开始
            }
            else
            {               
                numberPeriods = (int.Parse(lastResult.NumberPeriods) + 1).ToString().PadLeft(6, '0');
            }        
            return numberPeriods;
        }

        /// <summary>
        /// 查询我的投注列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<PagingResponseMessage<LotteryMyBetResponse>> LotteryMyBetAsync(Models.UserInfo user, LotteryMyBetRequest request, CancellationToken requestAborted)
        {
            var response = new PagingResponseMessage<LotteryMyBetResponse>();
            var numberPeriods = await GetNumberPeriods();//下一次开奖期数
            var myLuQuery = _giftStore.GetLotteryUsers().Where(w => w.PeriodId == request.PeriodId && w.UserId == user.Id);

            myLuQuery = myLuQuery.OrderByDescending(o => o.CreateTime);
            response.TotalCount = await myLuQuery.CountAsync();
            response.PageIndex = request.PageIndex;
            response.PageSize = request.PageSize;
            response.Extension = await myLuQuery.Skip(request.PageIndex * request.PageSize).Take(request.PageSize).Select(s => new LotteryMyBetResponse
            {
                IsRunlottery = numberPeriods == s.NumberPeriods ? false : true,//如果等于下一期就是未开奖
                NumberPeriods = s.NumberPeriods,
                Number = s.Number,
                Score = s.Score,
                WinningNumber = s.WinningNumber,
                WinningScore = s.WinningScore,
                CreateTime = s.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToListAsync();

            return response;
        }

        /// <summary>
        /// 查询开奖列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<PagingResponseMessage<LotteryRunListResponse>> LotteryRunListAsync(PagingRequest request, CancellationToken requestAborted)
        {
            var response = new PagingResponseMessage<LotteryRunListResponse> { Extension = new List<LotteryRunListResponse>() };
            var numberPeriods = await GetNumberPeriods();//下一次开奖期数
            var lrQuery = _giftStore.GetLotteryResults();

            lrQuery = lrQuery.OrderByDescending(o => o.CreateTime);
            response.TotalCount = await lrQuery.CountAsync();
            response.PageIndex = request.PageIndex;
            response.PageSize = request.PageSize;
            //第一页在最前面加一条下一期未开奖的记录
            if (request.PageIndex == 0)
            {
                //下一期开奖时间
                var nextTime = DateTime.Now;
                if (DateTime.Now < Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 10:00:00")))
                {
                    nextTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 10:00:00"));
                }
                else if (DateTime.Now < Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 11:30:00")))
                {
                    nextTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 11:30:00"));//TODO 年前追加,开年删除
                }
                else if (DateTime.Now < Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 13:00:00")))
                {
                    nextTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 13:00:00"));
                }
                else if (DateTime.Now < Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 15:30:00")))
                {
                    nextTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 15:30:00"));//TODO 年前追加,开年删除
                }
                else if (DateTime.Now < Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 18:00:00")))
                {
                    nextTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 18:00:00"));
                }
                else if (DateTime.Now < Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 21:00:00")))
                {
                    nextTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 21:00:00"));
                }
                response.Extension.Add(new LotteryRunListResponse
                {
                    IsRunlottery = false,//下一期还没有开奖
                    NumberPeriods = numberPeriods,
                    Number = "XX,XX,XX,XX,XX,XX",
                    CreateTime = nextTime.ToString("yyyy-MM-dd HH:mm:ss")
                }) ;
            }
            var re = await lrQuery.Skip(request.PageIndex * request.PageSize).Take(request.PageSize).Select(s => new LotteryRunListResponse
            {
                IsRunlottery = true,//查出来的都是已开奖的
                NumberPeriods = s.NumberPeriods,
                Number = s.Number,
                CreateTime = s.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                
            }).ToListAsync();

            response.Extension.AddRange(re);
            return response;
        }
        
    }
}
