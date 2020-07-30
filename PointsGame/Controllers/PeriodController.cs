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
using PointsGame.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XYH.Core.Log;

namespace PointsGame.Controllers
{
    /// <summary>
    /// 赛季控制器
    /// </summary>
    [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    [Route("api/period")]
    public class PeriodController : Controller
    {
        private readonly PeriodManager _scorePeriodManager;
        private readonly ILogger Logger = LoggerManager.GetLogger(nameof(PeriodController));
        private readonly IDistributedCache _cache;
        public PeriodController(PeriodManager scorePeriodManager, IDistributedCache distributedCache)
        {
            _scorePeriodManager = scorePeriodManager ?? throw new ArgumentNullException(nameof(scorePeriodManager));
            _cache = distributedCache;
        }
        /// <summary>
        /// 查询赛季列表(赛季管理里面用)
        /// </summary>
        /// <param name="user">不传</param>
        /// <returns></returns>
        [HttpGet("Search")]
        [CheckPermission]
        public async Task<ResponseMessage<List<PeriodReponse>>> SearchAllPeriod(Models.UserInfo user)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})查询赛季列表");
            var response = new ResponseMessage<List<PeriodReponse>>();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ArgumentNullError;
                response.Message = "模型验证失败" + ModelState.GetAllErrors();
                Logger.Warn("查询赛季列表验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }
            if (user.IsAdmin == false)
            {
                response.Code = ResponseCodeDefines.NotAllow;
                response.Message = "没有权限";
                Logger.Trace("没有权限：\r\n{0}", response.Message ?? "");
            }
            try
            {
                response = await _scorePeriodManager.SearchPeriodlist();
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})查询赛季列表，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

        /// <summary>
        /// 查询当前和历史赛季列表(最顶上的使用)
        /// </summary>
        /// <param name="user">不传</param>
        /// <returns></returns>
        [HttpGet("Searchnowandhistory")]
        [CheckPermission]
        public async Task<ResponseMessage<List<PeriodReponse>>> SearchnowPeriod(Models.UserInfo user)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})查询赛季列表");
            var response = new ResponseMessage<List<PeriodReponse>>();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ArgumentNullError;
                response.Message = "模型验证失败" + ModelState.GetAllErrors();
                Logger.Warn("查询赛季列表验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }
            try
            {
                response = await _scorePeriodManager.SearchNowAndhistoryPeriodlist();
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})查询赛季列表，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

        /// <summary>
        /// 添加赛季信息
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="createPeriodRequest"></param>
        /// <returns></returns>
        [HttpPost("create")]
        [CheckPermission]
        public async Task<ResponseMessage> CratePeriod(Models.UserInfo user, [FromBody] CreatePeriodRequest createPeriodRequest)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})添加赛季，请求参数为：\r\n" + (createPeriodRequest != null ? JsonHelper.ToJson(createPeriodRequest) : ""));
            var response = new ResponseMessage();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ArgumentNullError;
                response.Message = "模型验证失败" + ModelState.GetAllErrors();
                Logger.Warn("添加赛季验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }
            if (user.IsAdmin == false)
            {
                response.Code = ResponseCodeDefines.NotAllow;
                response.Message = "没有权限";
                Logger.Trace("没有权限：\r\n{0}", response.Message ?? "");
            }

            var prefixs = new string[] { "ScorePeriodController" };
            var key = $"{user.Id}{createPeriodRequest.Caption}{createPeriodRequest.EndDate.Date}";
            try
            {
                // 防止重复提交
                await _cache.LockSubmit(prefixs, key, "CratePeriod", HttpContext.RequestAborted);
                response = await _scorePeriodManager.CreatePeriod(user, createPeriodRequest);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})添加赛季，报错：{e.Message}\r\n{e.StackTrace}");
            }
            finally
            {
                // 成功失败都要移除
                await _cache.UnlockSubmit(prefixs, key);
            }
            return response;
        }

        /// <summary>
        /// 修改赛季信息
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="updatePeriodRequest"></param>
        /// <returns></returns>
        [HttpPost("update")]
        [CheckPermission]
        public async Task<ResponseMessage> Update(Models.UserInfo user, [FromBody] UpdatePeriodRequest updatePeriodRequest)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})更改赛季信息，请求参数为：\r\n" + (updatePeriodRequest != null ? JsonHelper.ToJson(updatePeriodRequest) : ""));
            var response = new ResponseMessage();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ArgumentNullError;
                response.Message = "模型验证失败" + ModelState.GetAllErrors();
                Logger.Warn("添加赛季验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }
            if (user.IsAdmin == false)
            {
                response.Code = ResponseCodeDefines.NotAllow;
                response.Message = "没有权限";
                Logger.Trace("没有权限：\r\n{0}", response.Message ?? "");
                return response;
            }
            try
            {
                response = await _scorePeriodManager.UapdatePeriod(user, updatePeriodRequest);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})更改赛季，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

        /// <summary>
        /// 开始/结束赛季
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="updatePeriodState"></param>
        /// <returns></returns>
        [HttpPost("update/state")]
        [CheckPermission]
        public async Task<ResponseMessage> Update(Models.UserInfo user, [FromBody] UpdatePeriodState updatePeriodState)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})更改赛季信息，请求参数为：\r\n" + (updatePeriodState != null ? JsonHelper.ToJson(updatePeriodState) : ""));
            var response = new ResponseMessage();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ArgumentNullError;
                response.Message = "模型验证失败" + ModelState.GetAllErrors();
                Logger.Warn("添加赛季验证失败：\r\n{0}", response.Message ?? "");
            }
            if (user.IsAdmin == false)
            {
                response.Code = ResponseCodeDefines.NotAllow;
                response.Message = "没有权限";
                Logger.Trace("没有权限：\r\n{0}", response.Message ?? "");
            }
            try
            {
                response = await _scorePeriodManager.UapdatePeriodState(updatePeriodState);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})更改赛季状态，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

    }
}
