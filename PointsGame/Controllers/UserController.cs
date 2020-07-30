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
using System.Linq;
using System.Threading.Tasks;
using XYH.Core.Log;

namespace PointsGame.Controllers
{
    /// <summary>
    /// 交易控制器（交易列表/K币派发/交易）
    /// </summary>
    [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    [Route("api/user")]
    public class UserController : Controller
    {
        private ILogger Logger = LoggerManager.GetLogger(nameof(UserController));
        private UserManager UserManager { get; }
        public UserController(UserManager userManager)
        {
            UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        /// <summary>
        /// 搜索用户(可用于用户管理、交易或派发时选择用户)
        /// </summary>
        /// <param name="user"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("search")]
        [CheckPermission]
        public async Task<PagingResponseMessage<UserResponse>> Search(Models.UserInfo user, [FromBody] UserSearchRequest request)
        {
            var response = new PagingResponseMessage<UserResponse>();

            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})搜索用户，请求参数为：\r\n" + (request != null ? JsonHelper.ToJson(request) : ""));
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = ModelState.GetAllErrors();
                Logger.Warn("搜索用户模型验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }

            try
            {
                response = await UserManager.Search(user, request, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                Logger.Error($"搜索用户,报错：{e.Message}\r\n{e.StackTrace}");
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                return response;
            }
            return response;
        }

        /// <summary>
        /// 新增/修改用户
        /// </summary>
        /// <param name="user"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("save")]
        [CheckPermission]
        public async Task<ResponseMessage> Save(Models.UserInfo user, [FromBody] UserSaveRequest request)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})保存用户，请求参数为：\r\n" + (request != null ? JsonHelper.ToJson(request) : ""));
            var response = new ResponseMessage();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = ModelState.GetAllErrors();
                Logger.Warn("保存用户模型验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }

            try
            {
                response = await UserManager.Save(user, request, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                Logger.Error($"保存用户,报错：{e.Message}\r\n{e.StackTrace}");
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                return response;
            }
            return response;
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="user"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("delete/{id}")]
        [CheckPermission]
        public async Task<ResponseMessage> Delete(Models.UserInfo user, string id)
        {
            var response = new ResponseMessage();

            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})删除用户，请求参数为：\r\n{(id != null ? JsonHelper.ToJson(id) : "")}");
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = ModelState.GetAllErrors();
                Logger.Warn($"用户{user?.UserName ?? ""}({user?.Id ?? ""})删除用户，模型验证失败：\r\n{response.Message ?? ""}");
                return response;
            }

            try
            {
                response = await UserManager.Delete(user, id, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})删除用户,报错：{e.Message}\r\n{e.StackTrace}");
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                return response;
            }

            return response;
        }

        /// <summary>
        /// ZenTaoEgg
        /// </summary>
        /// <param name="zentaoLoginName"></param>
        /// <returns></returns>
        [HttpGet("zentao/{zentaoLoginName}")]
        [AllowAnonymous]
        public async Task<ResponseMessage> ZenTaoEgg([FromRoute] string zentaoLoginName)
        {
            Logger.Trace($"禅道神秘彩蛋，请求参数为：loginName:{ zentaoLoginName}");
            var response = new ResponseMessage();                         
            try
            {              
                await UserManager.ZenTaoEggAsync(zentaoLoginName);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"禅道神秘彩蛋，报错：{e.Message}\r\n{e.StackTrace}");
            }           
            return response;
        }

    }
}
