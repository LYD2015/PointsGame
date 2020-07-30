using ApiCore;
using ApiCore.Dto;
using AspNet.Security.OAuth.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using PointsGame.Dto.Request;
using PointsGame.Dto.Response;
using PointsGame.Filters;
using PointsGame.Managers;
using System;
using System.Threading.Tasks;
using XYH.Core.Log;

namespace PointsGame.Controllers
{
    /// <summary>
    /// 自由交易控制器
    /// </summary>
    [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    [Route("api/deal")]
    public class DealController : Controller
    {
        private readonly DealManager _dealManager;
        private readonly ILogger Logger = LoggerManager.GetLogger(nameof(DealController));
        private readonly IDistributedCache _cache;
        public DealController(DealManager dealManager, IDistributedCache distributedCache)
        {
            _dealManager = dealManager ?? throw new ArgumentNullException(nameof(dealManager));
            _cache = distributedCache;
        }

        /// <summary>
        /// 查询交易列表【V1.1】
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="searchDealRequest"></param>
        /// <returns></returns>
        [HttpPost("Search")]
        [CheckPermission]
        public async Task<PagingResponseMessage<SearchDealResponse>> SearchDeal(Models.UserInfo user, [FromBody] SearchDealRequest searchDealRequest)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})查询交易列表，请求参数为：\r\n" + (searchDealRequest != null ? JsonHelper.ToJson(searchDealRequest) : ""));
            var response = new PagingResponseMessage<SearchDealResponse>();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ArgumentNullError;
                response.Message = "模型验证失败" + ModelState.GetAllErrors();
                Logger.Warn("查询交易列表模型验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }
            try
            {
                response = await _dealManager.SearchDealAsync(user, searchDealRequest, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})查询交易列表，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

        /// <summary>
        /// 自由交易-发起交易
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="dealSubmitRequest"></param>
        /// <returns></returns>
        [HttpPost("Submit")]
        [CheckPermission]
        public async Task<ResponseMessage> DealSubmit(Models.UserInfo user, [FromBody] DealSubmitRequest dealSubmitRequest)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})查询交易列表，请求参数为：\r\n" + (dealSubmitRequest != null ? JsonHelper.ToJson(dealSubmitRequest) : ""));
            var response = new ResponseMessage();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ArgumentNullError;
                response.Message = "模型验证失败" + ModelState.GetAllErrors();
                Logger.Warn("发起交易模型验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }

            var prefixs = new string[] { "DealController" };
            var key = $"{user.Id}{dealSubmitRequest.Theme}{dealSubmitRequest.Score}";
            try
            {
                // 防止重复提交
                await _cache.LockSubmit(prefixs, key, "DealSubmit", HttpContext.RequestAborted);
                response = await _dealManager.DealSubmitAsync(user, dealSubmitRequest, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})发起交易，报错：{e.Message}\r\n{e.StackTrace}");
            }
            finally
            {
                // 成功失败都要移除
                await _cache.UnlockSubmit(prefixs, key);
            }
            return response;
        }
    }
}
