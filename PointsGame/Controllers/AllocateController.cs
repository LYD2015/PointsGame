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
    /// K币派发控制器
    /// </summary>
    [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    [Route("api/allocate")]
    public class AllocateController : Controller
    {        
        private readonly AllocateManager _allocateManager;
        private readonly ILogger Logger = LoggerManager.GetLogger(nameof(AllocateController));
        private readonly IDistributedCache _cache;
        public AllocateController(AllocateManager allocateManager, IDistributedCache distributedCache)
        {
            _allocateManager = allocateManager ?? throw new ArgumentNullException(nameof(allocateManager));
            _cache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        }

        /// <summary>
        /// 查询K币派发列表【V1.1】
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="searchAllocateRequest"></param>
        /// <returns></returns>
        [HttpPost("Search")]
        [CheckPermission]
        public async Task<PagingResponseMessage<SearchAllocateResponse>> SearchAllocate(Models.UserInfo user, [FromBody] SearchAllocateRequest searchAllocateRequest)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})查询K币派发列表，请求参数为：\r\n" + (searchAllocateRequest != null ? JsonHelper.ToJson(searchAllocateRequest) : ""));
            var response = new PagingResponseMessage<SearchAllocateResponse>();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ArgumentNullError;
                response.Message = "模型验证失败:" + ModelState.GetAllErrors();
                Logger.Warn("查询K币派发列表模型验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }
            try
            {
                response = await _allocateManager.SearchAllocateAsync(user, searchAllocateRequest, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})查询K币派发列表，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

        /// <summary>
        /// 派发K币-提交
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="allocateSubmitRequest"></param>
        /// <returns></returns>
        [HttpPost("Submit")]
        [CheckPermission]
        public async Task<ResponseMessage> AllocateSubmit(Models.UserInfo user, [FromBody] AllocateSubmitRequest allocateSubmitRequest)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})新增派发，请求参数为：\r\n" + (allocateSubmitRequest != null ? JsonHelper.ToJson(allocateSubmitRequest) : ""));
            var response = new ResponseMessage();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ArgumentNullError;
                response.Message = "模型验证失败" + ModelState.GetAllErrors();
                Logger.Warn("新增派发模型验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }
            var prefixs = new string[] { "AllocateController" };
            var key = $"{user.Id}{allocateSubmitRequest.Theme}{allocateSubmitRequest.Score}";
            try
            {
                // 防止重复提交
                await _cache.LockSubmit(prefixs, key, "AllocateSubmit", HttpContext.RequestAborted);
                response = await _allocateManager.AllocateSubmitAsync(user, allocateSubmitRequest, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})新增派发，报错：{e.Message}\r\n{e.StackTrace}");
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
