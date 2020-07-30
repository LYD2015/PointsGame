using ApiCore;
using AspNet.Security.OAuth.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PointsGame.Dto.Request;
using PointsGame.Dto.Response;
using PointsGame.Filters;
using PointsGame.Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XYH.Core.Log;

namespace PointsGame.Controllers
{
    /// <summary>
    /// 查询控制器（排行榜/动态查询/个人信息）
    /// </summary>
    [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    [Route("api/Search")]
    public class SearchController : Controller
    {
        private readonly SearchManager _searchManager;
        private readonly ILogger Logger = LoggerManager.GetLogger(nameof(SearchController));
        public SearchController(SearchManager searchManager)
        {
            _searchManager = searchManager ?? throw new ArgumentNullException(nameof(searchManager));
        }

        /// <summary>
        /// 查询排行榜
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="searchTopRequest"></param>
        /// <returns></returns>
        [HttpPost("top")]
        [CheckPermission]
        public async Task<PagingResponseMessage<SearchTopResponse>> SearchTop(Models.UserInfo user, [FromBody] SearchTopRequest searchTopRequest)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})查询排行榜，请求参数为：\r\n" + (searchTopRequest != null ? JsonHelper.ToJson(searchTopRequest) : ""));
            var response = new PagingResponseMessage<SearchTopResponse>();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ArgumentNullError;
                response.Message = "模型验证失败";
                Logger.Warn("查询排行榜模型验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }
            try
            {
                response = await _searchManager.SearchTopAsync(searchTopRequest, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})查询排行榜，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

        /// <summary>
        /// 获取个人等级信息
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="periodId">赛季ID</param>
        /// <returns></returns>
        [HttpGet("user/title/{periodId}")]
        [CheckPermission]
        public async Task<ResponseMessage<SearchTopResponse>> SearchUserTitle(Models.UserInfo user, [FromRoute] string periodId)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})获取个人等级信息开始，请求参数为：\r\nperiodId:{periodId}");
            var response = new ResponseMessage<SearchTopResponse>();
            try
            {
                response = await _searchManager.SearchUserTitleAsync(user.Id, periodId, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})获取个人等级信息，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

        /// <summary>
        /// 获取指定用户等级信息
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="periodId">赛季ID</param>
        /// <param name="userId">指定用户UserId</param>
        /// <returns></returns>
        [HttpGet("user/title/{periodId}/{userId}")]
        [CheckPermission]
        public async Task<ResponseMessage<SearchTopResponse>> SearchUserTitle(Models.UserInfo user, [FromRoute] string periodId, [FromRoute] string userId)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})获取指定用户等级信息开始，请求参数为：\r\nperiodId:{periodId},userId:{userId}");
            var response = new ResponseMessage<SearchTopResponse>();
            try
            {
                response = await _searchManager.SearchUserTitleAsync(userId, periodId, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})获取指定用户等级信息，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

        /// <summary>
        /// 获取动态列表
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="searchDynamicRequest"></param>
        /// <returns></returns>
        [HttpPost("dynamic")]
        [CheckPermission]
        public async Task<PagingResponseMessage<SearchDynamicResponse>> SearchDynamic(Models.UserInfo user, [FromBody] SearchDynamicRequest searchDynamicRequest)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})动态查询，请求参数为：\r\n" + (searchDynamicRequest != null ? JsonHelper.ToJson(searchDynamicRequest) : ""));
            var response = new PagingResponseMessage<SearchDynamicResponse>();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ArgumentNullError;
                response.Message = "模型验证失败";
                Logger.Warn("动态查询模型验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }
            try
            {
                response = await _searchManager.SearchDynamicAsync(searchDynamicRequest?.PeriodId,
                                                                    null,
                                                                    searchDynamicRequest?.PageIndex??0,
                                                                    searchDynamicRequest.PageSize,
                                                                    HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})动态查询，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

        /// <summary>
        /// 获取指定用户动态列表
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="searchUserDynamicRequest"></param>
        /// <returns></returns>
        [HttpPost("dynamic/user")]
        [CheckPermission]
        public async Task<PagingResponseMessage<SearchDynamicResponse>> SearchUserDynamic(Models.UserInfo user, [FromBody] SearchUserDynamicRequest searchUserDynamicRequest)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})用户动态查询，请求参数为：\r\n" + (searchUserDynamicRequest != null ? JsonHelper.ToJson(searchUserDynamicRequest) : ""));
            var response = new PagingResponseMessage<SearchDynamicResponse>();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ArgumentNullError;
                response.Message = "模型验证失败";
                Logger.Warn("用户动态查询模型验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }
            try
            {
                response = await _searchManager.SearchDynamicAsync(searchUserDynamicRequest.PeriodId,
                                                                    searchUserDynamicRequest.UserId,
                                                                    searchUserDynamicRequest.PageIndex,
                                                                    searchUserDynamicRequest.PageSize,
                                                                    HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})用户动态查询，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

        /// <summary>
        /// 获取常用印象标签(新建任务和派发K币界面的常用标签,后端最多反20个,具体界面显示多少个前端决定)
        /// </summary>
        /// <param name="user">不传</param>
        /// <returns></returns>
        [HttpGet("label")]
        [CheckPermission]
        public async Task<ResponseMessage<List<string>>> SearchLabel(Models.UserInfo user)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})获取常用印象标签开始。");
            var response = new ResponseMessage<List<string>>();

            try
            {
                response = await _searchManager.SearchLabel(user);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})获取常用印象标签，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }
    }
}
