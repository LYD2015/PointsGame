using ApiCore;
using Microsoft.Extensions.DependencyInjection;
using PointsGame.Helper;
using PointsGame.Managers;
using PointsGame.Stores;

namespace PointsGame.Extensions
{
    /// <summary>
    /// 服务容器扩展
    /// </summary>
    public static class IServiceCollectionExtension
    {
        /// <summary>
        /// 添加用户定义的依赖注入项
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddUserDefined(this IServiceCollection services)
        {
            services.AddScoped<RestClient>();

            // 用户管理
            services.AddScoped<IUserStore, UserStore>();
            services.AddScoped<UserManager>();

            // 任务
            services.AddScoped<TaskManager>();
            services.AddScoped<ITaskStore, TaskStore>();

            // 称号
            services.AddScoped<IRankTitleStore, RankTitleStore>();
            services.AddScoped<RankTitleManager>();

            // Token助手
            services.AddScoped<Helper.HelperAuthentication>();
            // 动态内容助手
            services.AddScoped<Helper.HelperDynamic>();

            // 查询
            services.AddScoped<ISearchStore, SearchStore>();
            services.AddScoped<SearchManager>();

            // 交易
            services.AddScoped<IDealStore, DealStore>();
            services.AddScoped<DealManager>();

            // 积分信息
            services.AddScoped<IScoreInfoStore, ScoreInfoStore>();

            // K币派发
            services.AddScoped<IAllocateStore, AllocateStore>();
            services.AddScoped<AllocateManager>();

            // 赛季
            services.AddScoped<IPeriodStore, PeriodStore>();
            services.AddScoped<PeriodManager>();

            //邮件
            services.AddScoped<HellperPush>();

            //抽奖
            services.AddScoped<GiftManager>();
            services.AddScoped<IGiftStore, GiftStore>();            
            // 注入SignalR
            services.AddSingleton<HelperSendClientMessage>();

            return services;
        }
    }
}
