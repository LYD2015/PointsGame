using Microsoft.EntityFrameworkCore;
using PointsGame.Dto.Response;
using PointsGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame
{
    /// <summary>
    /// 积分系统数据上下文
    /// </summary>
    public class PointsGameDbContext : DbContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public PointsGameDbContext(DbContextOptions options) : base(options) { }
        /// <summary>
        /// 任务列表
        /// </summary>
        public DbSet<TaskItemResponse> TaskItems { get; set; }
        /// <summary>
        /// 积分明细表
        /// </summary>
        public DbSet<ScoreDetailed> ScoreDetaileds { get; set; }
        /// <summary>
        /// 积分信息表
        /// </summary>
        public DbSet<ScoreInfo> ScoreInfos { get; set; }
        /// <summary>
        /// 赛季表
        /// </summary>
        public DbSet<Period> ScorePeriods { get; set; }
        /// <summary>
        /// 称号表
        /// </summary>
        public DbSet<RankTitle> ScoreTitles { get; set; }
        /// <summary>
        /// 任务表
        /// </summary>
        public DbSet<TaskInfo> TaskInfos { get; set; }
        /// <summary>
        /// 抢到任务的人员表
        /// </summary>
        public DbSet<TaskUser> TaskUsers { get; set; }
        /// <summary>
        /// 用户表
        /// </summary>
        public DbSet<UserInfo> UserInfos { get; set; }
        /// <summary>
        /// 用户对应的印象标签表
        /// </summary>
        public DbSet<UserLabel> UserLabels { get; set; }
        /// <summary>
        /// 登录日志表
        /// </summary>
        public DbSet<UserSignLog> UserSignLogs { get; set; }
        /// <summary>
        /// 动态表
        /// </summary>
        public DbSet<DynamicInfo> DynamicInfos { get; set; }
        /// <summary>
        /// 形容词库表
        /// </summary>
        public DbSet<DynamicAdjective> DynamicAdjectives { get; set; }
        /// <summary>
        /// 赛季奖品说明表
        /// </summary>
        public DbSet<PeriodGift> PeriodGifts { get; set; }
        /// <summary>
        /// 赛季奖品信息表
        /// </summary>
        public DbSet<GiftInfo> GiftInfos { get; set; }
        /// <summary>
        /// 彩票开奖结果
        /// </summary>
        public DbSet<LotteryResult> LotteryResults { get; set; }
        /// <summary>
        /// 用户投注情况
        /// </summary>
        public DbSet<LotteryUser> LotteryUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ScoreDetailed>(a =>
            {
                a.ToTable("jf_scoredetailed");
            });

            modelBuilder.Entity<ScoreInfo>(a =>
            {
                a.ToTable("jf_scoreinfo");
            });

            modelBuilder.Entity<Period>(a =>
            {
                a.ToTable("jf_period");
            });

            modelBuilder.Entity<RankTitle>(a =>
            {
                a.ToTable("jf_ranktitle");
            });

            modelBuilder.Entity<TaskInfo>(a =>
            {
                a.ToTable("jf_taskinfo");
            });

            modelBuilder.Entity<TaskUser>(a =>
            {
                a.ToTable("jf_taskuser").HasKey(k => new { k.TaskId, k.UserId });
            });

            modelBuilder.Entity<UserInfo>(a =>
            {
                a.ToTable("jf_userinfo");
            });

            modelBuilder.Entity<UserLabel>(a =>
            {
                a.ToTable("jf_userlabel");
            });

            modelBuilder.Entity<UserSignLog>(a =>
            {
                a.ToTable("jf_usersign_log");
            });
            modelBuilder.Entity<DynamicInfo>(a =>
            {
                a.ToTable("jf_dynamicinfo");
            });
            modelBuilder.Entity<DynamicAdjective>(a =>
            {
                a.ToTable("jf_dynamicadjective");
            });
            modelBuilder.Entity<PeriodGift>(a =>
            {
                a.ToTable("jf_periodgift");
            });
            modelBuilder.Entity<GiftInfo>(a =>
            {
                a.ToTable("jf_giftinfo");
            });
            modelBuilder.Entity<LotteryResult>(a =>
            {
                a.ToTable("jf_lotteryresult");
            });
            modelBuilder.Entity<LotteryUser>(a =>
            {
                a.ToTable("jf_lotteryuser");
            });
        }
    }
}
