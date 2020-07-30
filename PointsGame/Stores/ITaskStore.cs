using PointsGame.Dto.Response;
using PointsGame.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PointsGame.Stores
{
    /// <summary>
    /// 任务
    /// </summary>
    public interface ITaskStore
    {
        /// <summary>
        /// 获取赛季信息表
        /// </summary>
        /// <returns></returns>
        IQueryable<Period> GetScorePeriods();

        /// <summary>
        /// 获取任务信息表
        /// </summary>
        /// <returns></returns>
        IQueryable<TaskInfo> GetTaskInfos();

        /// <summary>
        /// 获取K币任务列表
        /// </summary>
        /// <returns></returns>
        IQueryable<TaskItemResponse> GetKcoinTaskList();

        /// <summary>
        /// 任务列表查询
        /// </summary>
        IQueryable<TaskItemResponse> TaskItems { get; set; }

        /// <summary>
        /// 获取K币任务列表(排序)
        /// </summary>
        /// <returns></returns>
        IQueryable<TaskItemResponse> GetKcoinTaskListByStateOrder();

        /// <summary>
        /// 获取任务人员关系表
        /// </summary>
        /// <returns></returns>
        IQueryable<TaskUser> GetTaskUsers();

        /// <summary>
        /// 新增任务用户
        /// </summary>
        /// <param name="taskUser"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TaskUser> AddTaskUserAsync(TaskUser taskUser, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 获取积分信息表
        /// </summary>
        /// <returns></returns>
        IQueryable<ScoreInfo> GetScoreInfos();

        /// <summary>
        /// 获取称号信息
        /// </summary>
        /// <returns></returns>
        IQueryable<RankTitle> GetScoreTitles();

        /// <summary>
        /// 新增任务信息
        /// </summary>
        /// <param name="taskInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TaskInfo> AddTaskInfoAsync(TaskInfo taskInfo, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 修改任务信息
        /// </summary>
        /// <param name="taskInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TaskInfo> UpdateTaskInfoAsync(TaskInfo taskInfo, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 获取我的任务列表
        /// </summary>
        /// <returns></returns>
        IQueryable<MyTaskResponse> GetMyTasksAsync(string userId, string periodId, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 获取我发布的任务列表
        /// </summary>
        /// <returns></returns>
        IQueryable<MyPushTaskResponse> GetMyPushTasksAsync(string userId, string periodId, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 批量更新积分信息
        /// </summary>
        /// <param name="scoreInfoList"></param>
        /// <returns></returns>
        Task UpdateScoreInfoList(List<ScoreInfo> scoreInfoList, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 批量添加积分明细
        /// </summary>
        /// <param name="scoreDetailedList"></param>
        /// <returns></returns>
        Task CreateScoreDetailedList(List<ScoreDetailed> scoreDetailedList, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 批量添加用户印象标签
        /// </summary>
        /// <param name="userLabelList"></param>
        /// <returns></returns>
        Task CreateUserLabelList(List<UserLabel> userLabelList, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 获取审核任务列表
        /// </summary>
        /// <param name="periodId"></param>
        /// <returns></returns>
        IQueryable<TasksExamineResponse> GetTaskExamineList(string periodId);
    }
}
