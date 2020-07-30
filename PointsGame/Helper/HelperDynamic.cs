using Microsoft.EntityFrameworkCore;
using PointsGame.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Helper
{
    /// <summary>
    /// 动态内容助手
    /// </summary>
    public class HelperDynamic
    {
        protected PointsGameDbContext Context { get; }
        public HelperDynamic(PointsGameDbContext context)
        {
            Context = context;
        }

        /// <summary>
        /// 使用随机形容词生成一条动态,并保存到数据库
        /// </summary>
        /// <param name="dynamicType">[必传]动态类型,用户产生相关动态的类型</param>
        /// <param name="periodId">[必传]赛季ID</param>
        /// <param name="dataId">[必传]关键数据ID(自由交易、派发收入、完成任务收入时是积分明细表的ID,发布任务、枪到任务是任务表的ID,抽奖兑奖是奖品ID)</param>
        /// <param name="outUserName">支出用户姓名(自由交易时的支出方,只有自由交易时才传)</param>
        /// <param name="outUserGroupName">支出用户组别名称(自由交易时的支出方,只有自由交易时才传)</param>
        /// <param name="getUserName">[必传]收入用户姓名(自由交易时的收入方)</param>
        /// <param name="getUserGroupName">[必传]收入用户组别名称(自由交易时的收入方)</param>
        /// <param name="userId">动态人的用户ID(支出时传支出人的ID,收入时传收入人的ID,任务/抽奖/兑奖时就是产生动态的那个人的ID)</param>
        /// <param name="theme">[必传]主题(任务相关就传任务名称,派发和自由交易传前端填写的标题)</param>
        /// <param name="Score">[必传]产生的积分(支出和收入时产生的积分)</param>
        /// <param name="userNumber">需要的人数(发布任务时才传)</param>
        /// <param name="robDateTime">抢任务时间(发布任务时才传)</param>
        /// <returns></returns>
        public async Task<string> AddDynamicContent(DynamicType dynamicType, string periodId, string dataId, string outUserName, string outUserGroupName, string getUserName, string getUserGroupName, string userId, string theme, int Score, int? userNumber, DateTime? robDateTime)
        {
            if (string.IsNullOrEmpty(periodId) || string.IsNullOrEmpty(dataId) || string.IsNullOrEmpty(getUserName) || string.IsNullOrEmpty(getUserGroupName) || string.IsNullOrEmpty(theme))
            {
                return string.Empty;
            }
            //关键形容词
            string keyAdjective = "";
            //查询形容词库
            var adjective = await Context.DynamicAdjectives.AsNoTracking().Where(w => w.Type == (int)dynamicType).ToListAsync();
            if (adjective != null && adjective.Count != 0)
            {
                Random random = new Random();
                keyAdjective = adjective[random.Next(0, adjective.Count - 1)].Adjective;
            }
            //动态内容
            string content = "";
            switch (dynamicType)
            {
                case DynamicType.DealExpenditure:
                    content = $"{outUserName}-{outUserGroupName}，因为《{theme}》{keyAdjective}给了{getUserName}-{getUserGroupName}{Math.Abs(Score)}K币。";
                    break;
                case DynamicType.DealIncome:
                    content = $"{getUserName}-{getUserGroupName}，因为《{theme}》{keyAdjective}获得了{outUserName}-{outUserGroupName}给的{Math.Abs(Score)}K币。";
                    break;
                case DynamicType.TaskIncome:
                    content = $"{getUserName}-{getUserGroupName}，{keyAdjective}完成了任务《{theme}》，获得{Math.Abs(Score)}K币。";
                    break;
                case DynamicType.DistributeIncome:
                    content = $"{getUserName}-{getUserGroupName}，因为《{theme}》{keyAdjective}获得了{Math.Abs(Score)}K币。";
                    break;
                case DynamicType.TaskPublish:
                    content = $"{getUserName}-{getUserGroupName}，{keyAdjective}发布了任务《{theme}》，需{userNumber}人,共{Math.Abs(Score)}K币。";
                    break;
                case DynamicType.TaskGet:
                    content = $"{getUserName}-{getUserGroupName}，{keyAdjective}抢到了任务《{theme}》，预计最高可获得{Math.Abs(Score)}K币。";
                    break;
                case DynamicType.RandomGift:
                    content = $"{getUserName}-{getUserGroupName}，花费{Math.Abs(Score)}K币，通过抽奖{keyAdjective}获得《{theme}》";
                    break;
                case DynamicType.GetGift:
                    content = $"{getUserName}-{getUserGroupName}，{keyAdjective}花费{Math.Abs(Score)}K币，直接兑换了《{theme}》";
                    break;
                case DynamicType.BuyLottery:
                    content = $"{getUserName}-{getUserGroupName}，在K彩第《{theme}》期，{keyAdjective}投注{Math.Abs(Score)}K币。";
                    break;
                case DynamicType.WinLottery:
                    content = $"{getUserName}-{getUserGroupName}，{theme}。{keyAdjective}中得{Math.Abs(Score)}K币。";
                    break;
                case DynamicType.LotteryResult:
                    content = $"{theme}";
                    break;
                default:
                    return string.Empty;
            }
            var dynamicInfo = new DynamicInfo
            {
                Id = Guid.NewGuid().ToString(),
                PeriodId = periodId,
                UserId = userId,
                UserFullName = $" { getUserName }-{ getUserGroupName }",
                DataId = dataId,
                DynamicContent = content,
                Score = Score,
                Adjective = keyAdjective,
                CreateTime = DateTime.Now,
                DynamicType = (int)dynamicType
            };
            await SaveDynamicContent(dynamicInfo);        
            return content;
        }

        /// <summary>
        /// 保存动态内容
        /// </summary>
        /// <param name="dynamicInfo"></param>
        /// <returns></returns>
        private async Task<bool> SaveDynamicContent(DynamicInfo dynamicInfo)
        {
            if (dynamicInfo == null)
            {
                throw new ArgumentNullException();
            }

            Context.Add(dynamicInfo);
            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return true;
        }
    }
    /// <summary>
    /// 动态类型(用户产生相关动态的类型)
    /// </summary>
    public enum DynamicType
    {
        /// <summary>
        /// 交易支出
        /// </summary>
        DealExpenditure = 1,
        /// <summary>
        /// 交易收入
        /// </summary>
        DealIncome = 2,
        /// <summary>
        /// 任务完成
        /// </summary>
        TaskIncome = 3,
        /// <summary>
        /// 派发收入
        /// </summary>
        DistributeIncome = 4,
        /// <summary>
        /// 任务发布
        /// </summary>
        TaskPublish = 5,
        /// <summary>
        /// 任务抢到
        /// </summary>
        TaskGet = 6,
        /// <summary>
        /// 抽奖
        /// </summary>
        RandomGift=7,
        /// <summary>
        /// 直接兑换奖
        /// </summary>
        GetGift = 8,
        /// <summary>
        /// 购买彩票
        /// </summary>
        BuyLottery=9,
        /// <summary>
        /// 彩票中奖
        /// </summary>
        WinLottery=10,
        /// <summary>
        /// 彩票开奖结果
        /// </summary>
        LotteryResult=11
    }
}
