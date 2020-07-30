using ApiCore;
using ApiCore.Dto;
using AspNet.Security.OAuth.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using PointsGame.Dto.Common;
using PointsGame.Dto.Request;
using PointsGame.Dto.Response;
using PointsGame.Filters;
using PointsGame.Managers;
using PointsGame.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XYH.Core.Log;

namespace PointsGame.Controllers
{
    /// <summary>
    /// 奖品管理控制器
    /// </summary>
    [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    [Route("api/gift")]
    public class GiftController: Controller
    {
        private readonly ILogger Logger = LoggerManager.GetLogger(nameof(GiftController));
        private readonly IDistributedCache _cache;
        private readonly GiftManager _giftManager;
        public GiftController(GiftManager giftManager, IDistributedCache cache)
        {
            _giftManager = giftManager;
            _cache = cache;
        }

        /// <summary>
        /// 赛季奖品管理-查询赛季奖品的说明
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="periodId">赛季ID</param>        
        /// <returns></returns>
        [HttpGet("memo/{periodId}")]
        [CheckPermission]
        public async Task<ResponseMessage<PeriodGift>> GetPeriodIdMemo(Models.UserInfo user, [FromRoute] string periodId)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})赛季奖品管理-查询赛季奖品的说明，请求参数为：periodId{periodId}");
            var response = new ResponseMessage<PeriodGift>();
            if (string.IsNullOrWhiteSpace(periodId))
            {
                response.Code = ResponseCodeDefines.ArgumentNullError;
                response.Message = "periodId不能为空";              
                return response;
            }
            try
            {
                response = await _giftManager.GetPeriodIdMemoAsync(user, periodId, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})赛季奖品管理-查询赛季奖品的说明，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

        /// <summary>
        /// 赛季奖品管理-更新赛季奖品的说明(没有新增接口，每一个赛季只有一个说明)
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("memo/update")]
        [CheckPermission]
        public async Task<ResponseMessage> MemoUpdate(Models.UserInfo user, [FromBody] PeriodGiftMemoRequest request)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})赛季奖品管理-更新赛季奖品的说明，请求参数为：\r\n" + (request != null ? JsonHelper.ToJson(request) : ""));
            var response = new ResponseMessage();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "模型验证失败" + ModelState.GetAllErrors();
                Logger.Warn("赛季奖品管理-更新赛季奖品的说明，模型验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }
            try
            {
                response = await _giftManager.MemoUpdateAsync(user, request, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})赛季奖品管理-更新赛季奖品的说明，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

        /// <summary>
        /// 赛季奖品管理-列表
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="periodId">赛季ID</param>        
        /// <returns></returns>
        [HttpGet("{periodId}")]
        [CheckPermission]
        public async Task<ResponseMessage<List<GiftInfo>>> GetGift(Models.UserInfo user, [FromRoute] string periodId)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})赛季奖品管理-列表，请求参数为：periodId{periodId}");
            var response = new ResponseMessage<List<GiftInfo>>();
            if (string.IsNullOrWhiteSpace(periodId))
            {
                response.Code = ResponseCodeDefines.ArgumentNullError;
                response.Message = "periodId不能为空";
                return response;
            }
            try
            {
                response = await _giftManager.GetGiftAsync(user, periodId, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})赛季奖品管理-列表，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

        /// <summary>
        /// 赛季奖品管理-新增/修改赛季奖品
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("add/update")]
        [CheckPermission]
        public async Task<ResponseMessage> GiftAddOrUpdate(Models.UserInfo user, [FromBody] GiftAddRequest request)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})赛季奖品管理-新增/修改赛季奖品，请求参数为：\r\n" + (request != null ? JsonHelper.ToJson(request) : ""));
            var response = new ResponseMessage();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "模型验证失败:"+ ModelState.GetAllErrors();
                Logger.Warn("赛季奖品管理-新增/修改赛季奖品，模型验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }
            try
            {
                response = await _giftManager.GiftAddOrUpdateAsync(user, request, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})赛季奖品管理-新增/修改赛季奖品，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

        /// <summary>
        /// 赛季奖品管理-删除赛季奖品
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="id">奖品ID</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [CheckPermission]
        public async Task<ResponseMessage> GiftDelete(Models.UserInfo user, [FromRoute] string id)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})赛季奖品管理-删除赛季奖品，请求参数为：\r\n" + (id != null ? JsonHelper.ToJson(id) : ""));
            var response = new ResponseMessage();
            if (string.IsNullOrWhiteSpace(id))
            {
                response.Code = ResponseCodeDefines.ArgumentNullError;
                response.Message = "Id不能为空";
                return response;
            }
            try
            {
                response = await _giftManager.GiftDeleteAsync(user, id, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})赛季奖品管理-删除赛季奖品，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }


        /// <summary>
        /// 抽奖/兑奖-奖品列表
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="periodId">赛季ID</param>        
        /// <returns></returns>
        [HttpGet("consumable/{periodId}")]
        [CheckPermission]
        public async Task<ResponseMessage<List<GiftInfo>>> GetConsumableGift(Models.UserInfo user, [FromRoute] string periodId)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})抽奖/兑奖-奖品列表，请求参数为：periodId{periodId}");
            var response = new ResponseMessage<List<GiftInfo>>();
            if (string.IsNullOrWhiteSpace(periodId))
            {
                response.Code = ResponseCodeDefines.ArgumentNullError;
                response.Message = "periodId不能为空";
                return response;
            }
            try
            {
                response = await _giftManager.GetConsumableGiftAsync(user, periodId, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})抽奖/兑奖-奖品列表，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

        /// <summary>
        /// 抽奖/兑奖-提交
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("consumable")]
        [CheckPermission]
        public async Task<ResponseMessage<GiftInfo>> ConsumableScore(Models.UserInfo user, [FromBody] ConsumableScoreRequest request)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})抽奖，请求参数为：\r\n" + JsonHelper.ToJson(request));
            var response = new ResponseMessage<GiftInfo>();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "模型验证失败" + ModelState.GetAllErrors();
                Logger.Warn("抽奖模型验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }
            var prefixs = new string[] { "ConsumableScore" };
            var key = "ConsumableScore";
            try
            {
                // 防止同时中奖,兑奖,数量不够。所以这里是一个一个来
                await _cache.LockSubmit(prefixs, key, "ConsumableScore", HttpContext.RequestAborted);               
                response = await _giftManager.ConsumableScoreAsync(user, request, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})抽奖，报错：{e.Message}\r\n{e.StackTrace}");
            }
            finally
            {
                // 成功失败都要移除
                await _cache.UnlockSubmit(prefixs, key);
            }
            return response;
        }

        /// <summary>
        /// 查询抽奖记录列表(抽奖界面/管理端的抽奖记录列表)
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("consumable/record")]
        [CheckPermission]
        public async Task<PagingResponseMessage<SearchDynamicResponse>> SearchConsumableRecord(Models.UserInfo user, [FromBody] ConsumableRecordRequest request)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})查询抽奖记录列表，请求参数为：\r\n" + (request != null ? JsonHelper.ToJson(request) : ""));
            var response = new PagingResponseMessage<SearchDynamicResponse>();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ArgumentNullError;
                response.Message = "模型验证失败" + ModelState.GetAllErrors();
                Logger.Warn("查询抽奖记录列表，模型验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }
            try
            {
                response = await _giftManager.SearchConsumableRecordAsync(request, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})查询抽奖记录列表，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

        /// <summary>
        /// 彩票开奖(由定时任务调用)
        /// </summary>
        [HttpGet("lottery/run")]
        [AllowAnonymous]
        public async Task<ResponseMessage> LotteryRun()
        {
            Logger.Trace($"彩票开始开奖");
            var response = new ResponseMessage();                      
            var prefixs = new string[] { "lottery" };
            var key = "lottery";
            try
            {
                // 防止同时重复提交
                await _cache.LockSubmit(prefixs, key, "lottery", HttpContext.RequestAborted);
                response = await _giftManager.LotteryRunAsync(HttpContext.RequestAborted);
                if (response.Code != ResponseCodeDefines.SuccessCode)
                {
                    Logger.Warn($"彩票开始开奖，失败：{JsonHelper.ToJson(response)}");
                }
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"彩票开始开奖，报错：{e.Message}\r\n{e.StackTrace}");
            }
            finally
            {
                // 成功失败都要移除
                await _cache.UnlockSubmit(prefixs, key);
            }
            return response;
        }


        /// <summary>
        /// 彩票投注
        /// </summary>
        [HttpPost("lottery/bet")]
        [CheckPermission]
        public async Task<ResponseMessage> LotteryBet(Models.UserInfo user,[FromBody] LotteryBetRequest request)
        {
            Logger.Trace($"彩票开始开奖");
            var response = new ResponseMessage();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "模型验证失败" + ModelState.GetAllErrors();
                Logger.Warn("彩票投注型验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }
            var prefixs = new string[] { "lottery" };
            var key = "lottery";
            try
            {
                // 防止同时重复提交
                await _cache.LockSubmit(prefixs, key, "lottery", HttpContext.RequestAborted);
                response = await _giftManager.LotteryBetAsync(user, request, HttpContext.RequestAborted);
                if (response.Code != ResponseCodeDefines.SuccessCode)
                {
                    Logger.Warn($"彩票投注，失败：{JsonHelper.ToJson(response)}");
                }
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"彩票投注，报错：{e.Message}\r\n{e.StackTrace}");
            }
            finally
            {
                // 成功失败都要移除
                await _cache.UnlockSubmit(prefixs, key);
            }
            return response;
        }

        /// <summary>
        /// 彩票投注(测试用)
        /// </summary>
        [HttpGet("lottery/bettest/{periodId}")]
        [AllowAnonymous]
        public async Task<ResponseMessage> LotteryBetTest([FromRoute] string periodId)
        {
           
            var response = new ResponseMessage();
         
            try
            {

                response = await _giftManager.LotteryBetTestAsync(periodId, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"彩票投注，报错：{e.Message}\r\n{e.StackTrace}");
            }           
            return response;
        }

        /// <summary>
        /// 查询我的投注列表
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("lottery/mybet")]
        [CheckPermission]
        public async Task<PagingResponseMessage<LotteryMyBetResponse>> LotteryMyBet(Models.UserInfo user, [FromBody] LotteryMyBetRequest request)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})查询我的投注列表，请求参数为：\r\n" + (request != null ? JsonHelper.ToJson(request) : ""));
            var response = new PagingResponseMessage<LotteryMyBetResponse>();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ArgumentNullError;
                response.Message = "模型验证失败" + ModelState.GetAllErrors();
                Logger.Warn("查询我的投注列表，模型验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }
            try
            {
                response = await _giftManager.LotteryMyBetAsync(user, request, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})查询我的投注列表，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

        /// <summary>
        /// 查询开奖列表
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("lottery/runlist")]
        [AllowAnonymous]
        public async Task<PagingResponseMessage<LotteryRunListResponse>> LotteryRunList([FromBody] PagingRequest request)
        {
            Logger.Trace($"查询开奖列表，请求参数为：\r\n" + (request != null ? JsonHelper.ToJson(request) : ""));
            var response = new PagingResponseMessage<LotteryRunListResponse>();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ArgumentNullError;
                response.Message = "模型验证失败" + ModelState.GetAllErrors();
                Logger.Warn("查询开奖列表，模型验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }
            try
            {
                response = await _giftManager.LotteryRunListAsync(request, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"查询开奖列表，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }
    }
}
