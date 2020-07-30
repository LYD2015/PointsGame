using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Validation;
using AuthenticationSDK;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using PointsGame.Extensions;
using PointsGame.Helper;
using PointsGame.Managers;
using Swashbuckle.AspNetCore.Swagger;
using XYH.Core.Log;

namespace PointsGame
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // 注入配置文件
            services.AddSingleton(Configuration as IConfigurationRoot);

            #region 日志配置
            LogLevels logLevel = LogLevels.Info;
            int maxDays = 7;
            var logConfig = Configuration.GetSection("Log");
            string maxFileSize = "10MB";
            if (logConfig != null)
            {

                Enum.TryParse(logConfig["Level"] ?? "", out logLevel);
                int.TryParse(logConfig["SaveDays"], out maxDays);
                maxFileSize = logConfig["MaxFileSize"];
                if (string.IsNullOrEmpty(maxFileSize))
                {
                    maxFileSize = "10MB";
                }
            }
            string logFolder = Path.Combine(AppContext.BaseDirectory, "Logs");
            LoggerManager.InitLogger(new LogConfig()
            {
                LogBaseDir = logFolder,
                MaxFileSize = maxFileSize,
                LogLevels = logLevel,
                IsAsync = true,
                LogFileTemplate = LogFileTemplates.PerDayDirAndLogger,
                LogContentTemplate = LogLayoutTemplates.SimpleLayout,
            });
            LoggerManager.SetLoggerAboveLevels(logLevel);
            LoggerManager.StartClear(maxDays, logFolder, LoggerManager.GetLogger("clear"));

            #endregion

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            // 注入SignalR
            services.AddSignalR();
            services.AddDbContext<PointsGameDbContext>(it =>
            {
                it.UseMySql(Configuration["Data:DefaultConnection:ConnectionString"]);
            });           
            services.AddScoped<ApiCore.ITransaction<PointsGameDbContext>, ApiCore.Stores.Transaction<PointsGameDbContext>>();
            #region 认证中心接口访问权限配置
            string authUrl = Configuration["AuthUrl"];
            string clientId = Configuration["ClientID"];
            string clientSecret = Configuration["ClientSecret"];
            string appName = Configuration["AppName"];

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie("Cookies")
            .AddOpenIdConnect("oidc", options =>
            {
                options.SignInScheme = "Cookies";

                options.Authority = authUrl;
                //options.Audiences.Add("*");
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.RequireHttpsMetadata = false;

                options.ResponseType = "code";
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.Scope.Remove("profile");
                options.Scope.Add("openid");
                options.Scope.Add("AuthenticationService");

                options.Scope.Add(appName);

                options.Scope.Add("offline_access");
                // 认证错误时捕获错误信息
                options.Events = new Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        return Task.CompletedTask;
                    }
                };
            })
            .AddJwtBearer(OAuthValidationDefaults.AuthenticationScheme, options =>
            {
                options.Authority = authUrl;
                options.RequireHttpsMetadata = false;
                options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                {
                    OnAuthenticationFailed = (context) =>
                    {
                        var msg = context.Exception;

                        return Task.CompletedTask;
                    }
                };
                //注册应用的AppName值
                options.Audience = appName;
            });
            #endregion

            services.AddUserDefined();
            // 注入认证中心
            services.AddAuthenticationDefined(options =>
            {
                options.AuthenticationUrl = authUrl;//必填
            });
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "积分系统接口文档",
                    TermsOfService = "None",
                });
                c.IgnoreObsoleteActions();
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var xmlPath = Path.Combine(basePath, "PointsGame.xml");
                c.IncludeXmlComments(xmlPath);
                // 添加httpHeader参数
                c.OperationFilter<HttpHeaderOperation>(); 
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "积分系统 API V1");
            });
            // 允许跨域访问
            app.UseCors(options => {
                options.AllowAnyHeader();
                options.AllowAnyMethod();
                options.AllowAnyOrigin();
                options.AllowCredentials();
            });

            // 注入消息信息触发
            app.UseSignalR(routes => {
                // 这个路径必须和前端的调用路径一致
                routes.MapHub<HelperSendClientMessage>("/pointgamemessage");
            });

            app.UseMvc();
            app.UseAuthentication();

            // 加入异常处理 wangsl
            app.UseMiddleware(typeof(ApiCore.Utils.ExceptionHandlerMiddleWare));
        }
    }
}
