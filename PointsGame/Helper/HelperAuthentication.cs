using ApiCore;
using AuthenticationSDK.Managers;
using Microsoft.Extensions.Configuration;
using PointsGame.Dto.Request;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using XYH.Core.Log;

namespace PointsGame.Helper
{
    /// <summary>
    /// Token助手
    /// </summary>
    public class HelperAuthentication
    {
        private IConfigurationRoot Configuration { get; }
        protected AuthenticationManager AuthenticationManager { get; }
        public HelperAuthentication(IConfigurationRoot configuration, AuthenticationManager authenticationManager)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            AuthenticationManager = authenticationManager;
        }

        /// <summary>
        /// 获取用户Token
        /// </summary>
        /// <returns></returns>
        public async Task<Newtonsoft.Json.Linq.JObject> GetUserTokenObject(SignRequest request)
        {
            string tokenUrl = Configuration["AuthUrl"];
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(tokenUrl)
            };
            var parameters = new Dictionary<string, string>
                            {
                                { "username", request.LoginName },
                                { "password", request.Password },
                                { "grant_type", "password" },//密码模式
                                { "client_id", Configuration["ClientID"] },
                                { "client_secret", Configuration["ClientSecret"] }//密钥
                            };
            client.DefaultRequestHeaders.Add("ReqHeader", "{\"source\":3}");
            //Logger.Info("C端小程序用户登录" + JsonHelper.ToJson(client));
            //获取token
            HttpResponseMessage tokenResponse = await client.PostAsync("/connect/token", new System.Net.Http.FormUrlEncodedContent(parameters));
            //Logger.Info($"小程序虚拟账号登录请求相应：{JsonHelper.ToJson(tokenResponse)}");
            if (tokenResponse?.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var jObj = JsonHelper.ToObject<Newtonsoft.Json.Linq.JObject>(await tokenResponse.Content.ReadAsStringAsync());
                return jObj;
            }
            return new Newtonsoft.Json.Linq.JObject();
        }

        /// <summary>
        /// 获取用户ID
        /// </summary>
        /// <param name="entoken"></param>
        /// <returns></returns>
        public static string GetUserId(string entoken)
        {
            try
            {
                var token = new JwtSecurityToken(entoken);
                if (token?.Payload != null)
                {
                    var sub = token.Payload["sub"];
                    return sub.ToString();
                }
            }
            catch { }
            return string.Empty;
        }

        /// <summary>
        /// 获取用户Email
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<string> GetUserEmail(string userId,string token)
        {
            return (await AuthenticationManager.GetUserInfo(userId, token))?.Extension?.Email;
        }

    }
}
