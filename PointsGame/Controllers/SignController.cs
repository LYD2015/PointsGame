using ApiCore;
using ApiCore.Dto;
using AspNet.Security.OAuth.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PointsGame.Dto.Request;
using PointsGame.Helper;
using PointsGame.Managers;
using System;
using System.Threading.Tasks;
using XYH.Core.Log;

namespace PointsGame.Controllers
{
    /// <summary>
    /// 登陆接口
    /// </summary>
    [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    [Route("api/sign")]
    public class SignController : Controller
    {
        private ILogger Logger = LoggerManager.GetLogger(nameof(SignController));
        private UserManager UserManager { get; }
        private readonly HellperPush _hellperEmail;
        public SignController(UserManager userManager, HellperPush hellperEmail)
        {
            UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _hellperEmail = hellperEmail ?? throw new ArgumentNullException(nameof(hellperEmail));
        }

        /// <summary>
        /// 登陆（获取Token）-自动绑定认证中心UserId
        /// </summary>
        /// <returns></returns>
        [HttpPost("in")]
        [AllowAnonymous]
        public async Task<ResponseMessage<IActionResult>> SignIn([FromBody] SignRequest request)
        {
            Logger.Trace($"登陆用户({nameof(SignIn)})，请求参数为：\r\n" + (request != null ? JsonHelper.ToJson(request) : ""));
            var response = new ResponseMessage<IActionResult>();
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = ModelState.GetAllErrors();
                Logger.Warn("添加称号模型验证失败：\r\n{0}", response.Message ?? "");
                return response;
            }

            try
            {          
                return await UserManager.SignIn(request, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {               
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = ModelState.GetAllErrors();
                Logger.Error($"登陆失败：{e.Message}\r\n{e.StackTrace}");
                return response;
            }
        }
    }
}
