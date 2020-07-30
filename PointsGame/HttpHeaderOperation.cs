using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace PointsGame
{
    /// <summary>
    /// HTTP请求头配置
    /// </summary>
    public class HttpHeaderOperation : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
            {
                operation.Parameters = new List<IParameter>();
            }
            var actionAttrs = context.ApiDescription.ActionAttributes();
            var isAuthorized = actionAttrs.Any(a => a.GetType() == typeof(AuthorizeAttribute));
            if (isAuthorized == false) //提供action都没有权限特性标记，检查控制器有没有
            {
                var controllerAttrs = context.ApiDescription.ControllerAttributes();

                isAuthorized = controllerAttrs.Any(a => a.GetType() == typeof(AuthorizeAttribute));
            }
            var isAllowAnonymous = actionAttrs.Any(a => a.GetType() == typeof(AllowAnonymousAttribute));
            if (isAuthorized && isAllowAnonymous == false)
            {
                operation.Parameters.Add(new NonBodyParameter()
                {
                    Name = "Authorization",  //添加Authorization头部参数
                    In = "header",
                    Type = "string",
                    Required = false,
                    Default = "Bearer ",
                    Description = "授权验证"
                });

                operation.Parameters.Add(new NonBodyParameter()
                {
                    Name = "ReqHeader",  //添加ReqHeader头部参数
                    In = "header",
                    Type = "string",
                    Required = false,
                    Default = "{Source:\"3\"}",
                    Description = "请求来源 （1-IOS,2-Android,3-web）"
                });
            }
        }
    }
}
