using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PointsGame.Stores;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PointsGame.Filters
{
    /// <summary>
    /// 权限检查
    /// </summary>
    public class CheckPermissionAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// 权限检查
        /// </summary>
        public CheckPermissionAttribute() : base(typeof(CheckPermissionImpl))
        {

        }
        private class CheckPermissionImpl : IAsyncActionFilter
        {
            private IUserStore _userStore { get; }
            public CheckPermissionImpl(IUserStore userStore)
            {
                _userStore = userStore;

            }
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                if (context?.HttpContext?.User == null)
                {
                    context.Result = new ContentResult()
                    {
                        Content = "用户未登录",
                        StatusCode = 403
                    };
                    return;
                }
                //直接从令牌获取用户信息
                var identity = context.HttpContext.User.Identity as ClaimsIdentity;
                var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = _userStore.GetUserInfos().Where(a => a.UserId.Equals(userId)).FirstOrDefault();

                if (user == null)
                {
                    context.Result = new ContentResult()
                    {
                        Content = "当前用户无效",
                        StatusCode = 403,
                    };
                    return;
                }
                try
                {
                    if (context.ActionArguments.ContainsKey("User"))
                    {
                        context.ActionArguments["User"] = user;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"错误：\r\n{ex.Message}，跟踪信息：{ex.StackTrace}");
                }
                await next();
            }
        }
    }
}
