using ApiCore;
using ApiCore.Dto;
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
    /// 等级称号控制器
    /// </summary>
    [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    [Route("api/title")]
    public class RankTitleController : Controller
    {
        private readonly RankTitleManager _titleManager;
        private readonly ILogger Logger = LoggerManager.GetLogger(nameof(RankTitleController));
        public RankTitleController(RankTitleManager titleManager)
        {
            _titleManager = titleManager ?? throw new ArgumentNullException(nameof(titleManager));
        }

        /// <summary>
        /// 查询称号列表
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="periodId"></param>
        /// <returns></returns>
        [HttpGet("{periodId}")]
        [CheckPermission]
        public async Task<ResponseMessage<List<SearchTitleResponse>>> SearchTitle(Models.UserInfo user, [FromRoute] string periodId)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})查询称号，请求参数为：\r\n" + (periodId != null ? JsonHelper.ToJson(periodId) : ""));
            var response = new ResponseMessage<List<SearchTitleResponse>>();
            if (string.IsNullOrWhiteSpace(periodId))
            {
                response.Code = ResponseCodeDefines.ArgumentNullError;
                response.Message = "periodId不能为空";
                Logger.Warn("查询称号模型验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }
            try
            {
                response = await _titleManager.SearchTitleAsync(user, periodId, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})查询称号，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

        /// <summary>
        /// 添加称号
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="titleRequest"></param>
        /// <returns></returns>
        [HttpPost("add")]
        [CheckPermission]
        public async Task<ResponseMessage> AddTitle(Models.UserInfo user, [FromBody] TitleAddRequest titleRequest)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})添加称号，请求参数为：\r\n" + (titleRequest != null ? JsonHelper.ToJson(titleRequest) : ""));
            var response = new ResponseMessage();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = ModelState.GetAllErrors();
                Logger.Warn("添加称号模型验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }
            try
            {
                response = await _titleManager.AddTitleAsync(user, titleRequest, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})添加称号，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

        /// <summary>
        /// 修改称号
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="titleUpdateRequest"></param>
        /// <returns></returns>
        [HttpPut("update")]
        [CheckPermission]
        public async Task<ResponseMessage> UpdateTitle(Models.UserInfo user, [FromBody] TitleUpdateRequest titleUpdateRequest)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})修改称号，请求参数为：\r\n" + (titleUpdateRequest != null ? JsonHelper.ToJson(titleUpdateRequest) : ""));
            var response = new ResponseMessage();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = ModelState.GetAllErrors();
                Logger.Warn("修改称号模型验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }
            try
            {
                response = await _titleManager.UpdateTitleAsync(user, titleUpdateRequest, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})修改称号，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

        /// <summary>
        /// 删除称号
        /// </summary>
        /// <param name="user">不传</param>
        /// <param name="titleId"></param>
        /// <returns></returns>
        [HttpDelete("{titleId}")]
        [CheckPermission]
        public async Task<ResponseMessage> DeleteTitle(Models.UserInfo user, [FromRoute] string titleId)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})删除称号，请求参数为：\r\n" + (titleId != null ? JsonHelper.ToJson(titleId) : ""));
            var response = new ResponseMessage();
            if (string.IsNullOrWhiteSpace(titleId))
            {
                response.Code = ResponseCodeDefines.ArgumentNullError;
                response.Message = "titleId不能为空";
                Logger.Warn("删除称号模型验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }
            try
            {
                response = await _titleManager.DeleteTitleAsync(user, titleId, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})删除称号，报错：{e.Message}\r\n{e.StackTrace}");
            }
            return response;
        }

    }
}
