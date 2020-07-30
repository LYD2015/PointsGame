using Microsoft.EntityFrameworkCore;
using PointsGame.Dto.Response;
using PointsGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PointsGame.Stores
{
    /// <summary>
    /// 任务
    /// </summary>
    public class TaskStore : ITaskStore
    {
        protected PointsGameDbContext Context { get; }
        public TaskStore(PointsGameDbContext context)
        {
            Context = context;
            TaskItems = Context.TaskItems;
        }

        /// <summary>
        /// 获取赛季信息表
        /// </summary>
        /// <returns></returns>
        public IQueryable<Period> GetScorePeriods()
        {
            return Context.ScorePeriods.AsNoTracking();
        }

        /// <summary>
        /// 获取任务信息表
        /// </summary>
        /// <returns></returns>
        public IQueryable<TaskInfo> GetTaskInfos()
        {
            return Context.TaskInfos.AsNoTracking();
        }

        /// <summary>
        /// 获取任务人员关系表
        /// </summary>
        /// <returns></returns>
        public IQueryable<TaskUser> GetTaskUsers()
        {
            return Context.TaskUsers.AsNoTracking();
        }

        /// <summary>
        /// 获取积分信息表
        /// </summary>
        /// <returns></returns>
        public IQueryable<ScoreInfo> GetScoreInfos()
        {
            return Context.ScoreInfos.AsNoTracking();
        }

        /// <summary>
        /// 获取称号信息
        /// </summary>
        /// <returns></returns>
        public IQueryable<RankTitle> GetScoreTitles()
        {
            return Context.ScoreTitles.AsNoTracking();
        }

        /// <summary>
        /// 获取审核任务列表
        /// </summary>
        /// <param name="periodId"></param>
        /// <returns></returns>
        public IQueryable<TasksExamineResponse> GetTaskExamineList(string periodId)
        {
            var query = from q in Context.TaskInfos.AsNoTracking()
                        join u in Context.UserInfos.AsNoTracking() on q.CreateUser equals u.Id into qu
                        from item in qu.DefaultIfEmpty()
                        join u1 in Context.UserInfos.AsNoTracking() on q.ExamineUser equals u1.Id into qu1
                        from item1 in qu1.DefaultIfEmpty()
                        where q.PeriodId == periodId && !q.IsDelete
                        orderby q.ExamineState, q.CreateTime descending
                        select new TasksExamineResponse
                        {
                            Id = q.Id,
                            PeriodId = q.PeriodId,
                            TaskName = q.TaskName,
                            TaskTntro = q.TaskTntro,
                            UserNumber = q.UserNumber,
                            Score = q.Score,
                            CreateUser = item.UserName,
                            CreateTime = q.CreateTime,
                            LootTime = q.LootTime,
                            ExamineMemo = q.ExamineMemo,
                            ExamineState = q.ExamineState,
                            ExamineUser=item1.UserName
                        };
            return query;
        }

        /// <summary>
        /// 新增任务信息
        /// </summary>
        /// <param name="taskInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TaskInfo> AddTaskInfoAsync(TaskInfo taskInfo, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (taskInfo == null)
            {
                throw new ArgumentNullException(nameof(taskInfo));
            }
            Context.Add(taskInfo);
            try
            {
                await Context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return taskInfo;
        }

        /// <summary>
        /// 修改任务信息
        /// </summary>
        /// <param name="taskInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TaskInfo> UpdateTaskInfoAsync(TaskInfo taskInfo, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (taskInfo == null)
            {
                throw new ArgumentNullException(nameof(taskInfo));
            }
            Context.Update(taskInfo);
            try
            {
                await Context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return taskInfo;
        }

        /// <summary>
        /// 获取我的任务列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="periodId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public IQueryable<MyTaskResponse> GetMyTasksAsync(string userId, string periodId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = from t in Context.TaskInfos.AsNoTracking()
                        join tu in Context.TaskUsers.AsNoTracking() on t.Id equals tu.TaskId
                        join u in Context.UserInfos.AsNoTracking() on t.CreateUser equals u.Id into uT
                        from uTr in uT.DefaultIfEmpty()
                        where tu.UserId == userId && !t.IsDelete && t.PeriodId == periodId
                        select new MyTaskResponse
                        {
                            Id = t.Id,
                            PeriodId = t.PeriodId,
                            TaskName = t.TaskName,
                            TaskTntro = t.TaskTntro,
                            UserNumber = t.UserNumber,
                            CreateTime = t.CreateTime,
                            CreateUser = uTr.UserName + "-" + uTr.GroupName,
                            Score = t.Score,
                            TaskState = t.TaskState,
                            GetTime = tu.GetTime
                        };
            return query;
        }

        /// <summary>
        /// 添加任务领取人
        /// </summary>
        /// <param name="taskUser"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TaskUser> AddTaskUserAsync(TaskUser taskUser, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (taskUser == null)
            {
                throw new ArgumentNullException(nameof(taskUser));
            }
            Context.Add(taskUser);
            try
            {
                await Context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return taskUser;
        }

        /// <summary>
        /// 获取我发布的任务列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="periodId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public IQueryable<MyPushTaskResponse> GetMyPushTasksAsync(string userId, string periodId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = from t in Context.TaskInfos.AsNoTracking()
                        join u in Context.UserInfos.AsNoTracking() on t.CreateUser equals u.Id into uT
                        from uTr in uT.DefaultIfEmpty()
                        join u1 in Context.UserInfos.AsNoTracking() on t.ExamineUser equals u1.Id into uT1
                        from uTr1 in uT1.DefaultIfEmpty()
                        where t.CreateUser == userId && t.PeriodId == periodId && !t.IsDelete
                        orderby t.CreateTime descending
                        select new MyPushTaskResponse
                        {
                            Id = t.Id,
                            PeriodId = t.PeriodId,
                            TaskName = t.TaskName,
                            TaskTntro = t.TaskTntro,
                            UserNumber = t.UserNumber,
                            CreateTime = t.CreateTime,
                            CreateUser = uTr.UserName + "-" + uTr.GroupName,
                            Score = t.Score,
                            TaskState = t.TaskState,
                            ExamineState = t.ExamineState,
                            ExamineMemo = t.ExamineMemo,
                            LootTime = t.LootTime,
                            ExamineUser = uTr1.UserName
                        };
            return query;
        }

        /// <summary>
        /// 任务列表查询
        /// </summary>
        public IQueryable<TaskItemResponse> TaskItems { get; set; }

        /// <summary>
        /// 查询K币任务列表
        /// </summary>
        /// <returns></returns>
        public IQueryable<TaskItemResponse> GetKcoinTaskList()
        {
            var query = from t in Context.TaskInfos.AsNoTracking()
                        join u in Context.UserInfos.AsNoTracking() on t.CreateUser equals u.Id into uT
                        from uTr in uT.DefaultIfEmpty()
                        where !t.IsDelete && t.ExamineState == 2
                        select new TaskItemResponse
                        {
                            Id = t.Id,
                            PeriodId = t.PeriodId,
                            TaskName = t.TaskName,
                            TaskTntro = t.TaskTntro,
                            UserNumber = t.UserNumber,
                            CreateTime = t.CreateTime,
                            CreateUser = uTr.UserName + "-" + uTr.GroupName,
                            Score = t.Score,
                            LootTime = t.LootTime,
                            TaskState = t.TaskState,
                        };
            return query;
        }

        /// <summary>
        /// 查询K币任务列表
        /// 排序规则：
        /// 1 状态排序：未认领、未抢、进行中、已完结
        /// 2 状态为未认领、未抢时，按开抢时间升序排列；状态为进行中、已完结时，按发布时间降序排列
        /// </summary>
        /// <returns></returns>
        public IQueryable<TaskItemResponse> GetKcoinTaskListByStateOrder()
        {
            var querySql = @"# 查询任务列表
SELECT uuid() uuid, t.Id, t.PeriodId, t.TaskName, t.TaskTntro, t.UserNumber, t.CreateTime, CONCAT(u.UserName, '-', u.GroupName) CreateUser, t.Score, t.LootTime, t.TaskState
FROM jf_taskinfo as t
LEFT JOIN jf_userinfo as u on t.CreateUser = u.Id
WHERE t.IsDelete = FALSE && t.ExamineState = 2
ORDER BY FIELD(t.`TaskState`, 1, 0, 2, 3), 
    (CASE t.TaskState WHEN 0 THEN t.LootTime WHEN 1 THEN t.LootTime END), 
    (CASE t.TaskState WHEN 2 THEN t.CreateTime WHEN 3 THEN t.CreateTime END
) DESC";
            var query = Context.TaskItems.FromSql<TaskItemResponse>(querySql);
            return query;
        }

        /// <summary>
        /// 批量更新积分信息
        /// </summary>
        /// <param name="scoreInfoList"></param>
        /// <returns></returns>
        public async Task UpdateScoreInfoList(List<ScoreInfo> scoreInfoList, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (scoreInfoList == null || scoreInfoList.Count == 0)
            {
                throw new ArgumentNullException(nameof(scoreInfoList));
            }

            Context.UpdateRange(scoreInfoList);

            await Context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        ///批量添加积分明细
        /// </summary>
        /// <param name="scoreDetailedList"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task CreateScoreDetailedList(List<ScoreDetailed> scoreDetailedList, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (scoreDetailedList == null || scoreDetailedList.Count == 0)
            {
                throw new ArgumentNullException(nameof(scoreDetailedList));
            }

            Context.AddRange(scoreDetailedList);

            await Context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 批量添加用户印象标签
        /// </summary>
        /// <param name="userLabelList"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task CreateUserLabelList(List<UserLabel> userLabelList, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (userLabelList == null || userLabelList.Count == 0)
            {
                throw new ArgumentNullException(nameof(userLabelList));
            }

            Context.AddRange(userLabelList);

            await Context.SaveChangesAsync(cancellationToken);
        }
    }
}
