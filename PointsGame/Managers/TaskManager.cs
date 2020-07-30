using ApiCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PointsGame.Dto.Request;
using PointsGame.Dto.Response;
using PointsGame.Helper;
using PointsGame.Models;
using PointsGame.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYH.Core.Log;

namespace PointsGame.Managers
{
    /// <summary>
    /// 任务
    /// </summary>
    public class TaskManager
    {
        private readonly ITaskStore _iTaskStore;
        private readonly IUserStore _iUserStore;
        private readonly ILogger Logger = LoggerManager.GetLogger(nameof(TaskManager));
        private readonly HelperDynamic _dynamicHelper;
        private readonly RestClient _restClient;
        private readonly IConfigurationRoot _config;
        private readonly ITransaction<PointsGameDbContext> _transaction;//事务
        private readonly HellperPush _hellperEmail;
        private HelperSendClientMessage _sendClientMessageManager;
        public TaskManager(ITaskStore taskStore, IUserStore userStore, HelperDynamic dynamicHelper, RestClient restClient, IConfigurationRoot configuration, ITransaction<PointsGameDbContext> transaction, HellperPush hellperEmail,
             HelperSendClientMessage sendClientMessageManager)
        {
            _iTaskStore = taskStore ?? throw new ArgumentNullException(nameof(taskStore));
            _iUserStore = userStore ?? throw new ArgumentNullException(nameof(userStore));
            _dynamicHelper = dynamicHelper ?? throw new ArgumentNullException(nameof(dynamicHelper));
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            _hellperEmail = hellperEmail ?? throw new ArgumentNullException(nameof(hellperEmail));
            _sendClientMessageManager = sendClientMessageManager;
        }

        /// <summary>
        /// 审核任务
        /// </summary>
        /// <param name="user"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ResponseMessage> ExaminetaskAsync(UserInfo user, ExamineTaskRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = new ResponseMessage();
            if (!user.IsAdmin)
            {
                response.Code = ResponseCodeDefines.NotAllow;
                response.Message = "不是管理员，没有权限";
                return response;
            }
            var taskInfo = await _iTaskStore.GetTaskInfos().Where(a => !a.IsDelete && a.Id == request.TaskId).FirstOrDefaultAsync(cancellationToken);
            var periodInfo = await _iTaskStore.GetScorePeriods().Where(w => !w.IsDelete && w.Id == taskInfo.PeriodId).FirstOrDefaultAsync();
            if (periodInfo == null || periodInfo.State != 1)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "只有进行中的赛季才能操作";
                return response;
            }
            //修改审核状态 时间 审核人 备注
            taskInfo.ExamineState = request.IsOk == true ? 2 : 3;
            taskInfo.ExamineMemo = request.ExamineMemo;
            taskInfo.ExamineTime = DateTime.Now;
            taskInfo.ExamineUser = user.Id;
            var taskPushUserInfo = await _iUserStore.GetUserInfos().Where(w => !w.IsDelete && w.Id == taskInfo.CreateUser).FirstOrDefaultAsync();

            if (taskInfo.LootTime > DateTime.Now)
            {
                #region << 调度中心添加计划任务：刷新任务状态从未开抢(0)到未认领(1) >>
                var task = await _iTaskStore.GetTaskInfos().Where(a => a.IsDelete == false && a.Id == request.TaskId).FirstOrDefaultAsync(cancellationToken);
                var args = new Dictionary<string, string>();
                args.Add(nameof(task.Id), task.Id);
                var schedulerRequest = new ApiCore.Dto.ScheduleSubmitRequest
                {
                    JobGroup = "KcoinTaskGrab",
                    JobName = task.Id,
                    CronStr = "",
                    StarRunTime = task.LootTime,
                    EndRunTime = null,
                    Callback = $"{_config["PointGameUrl"]}/api/task/state/refresh/callback",
                    Args = args,
                };
                string schedulerUrl = $"{_config["ScheduleServerUrl"]}/api/scheduler/start";
                var re = await _restClient.Post<ResponseMessage>(schedulerUrl, schedulerRequest);
                if (re.Code != ResponseCodeDefines.SuccessCode)
                {
                    response.Code = re.Code;
                    response.Message = "调度中心添加计划任务失败：" + re.Message;
                    return response;
                }
                #endregion
            }
            else
            {
                taskInfo.TaskState = 1;
            }
            var dynamicContent = string.Empty;
            //事务保存
            using (var trans = await _transaction.BeginTransaction())
            {
                try
                {
                    await _iTaskStore.UpdateTaskInfoAsync(taskInfo);
                    // 添加动态
                    dynamicContent = await _dynamicHelper.AddDynamicContent(
                                     DynamicType.TaskPublish,
                                     taskInfo.PeriodId,
                                     taskInfo.Id,
                                     "",
                                     "",
                                     taskPushUserInfo?.UserName,
                                     taskPushUserInfo?.GroupName,
                                     taskInfo.CreateUser,
                                     taskInfo.TaskName,
                                     taskInfo.Score,
                                     taskInfo.UserNumber,
                                     taskInfo.LootTime);
                    trans.Commit();
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    throw new Exception("保存事务失败", e);
                }
            }

            #region 发送新任务提醒
            if (request.IsOk)
            {
                //通过发所有人
                var usersIds = await _iUserStore.GetUserInfos().Where(w => !w.IsDelete && !string.IsNullOrWhiteSpace(w.UserId)).Select(s => s.UserId).ToListAsync();
                if (usersIds.Any())
                {
                    _hellperEmail.SendEmpPush($"《{periodInfo.Caption}》中有新任务发布啦！[{taskInfo.TaskState == 0}]",
                                             $"尊敬的勇士您好：{dynamicContent}{taskInfo.LootTime.ToString("yyyy-MM-dd HH:mm:ss")}开抢，赶紧去看看吧。",
                                             usersIds);
                }
            }
            else
            {   //驳回发给发起人
                var usersIds = await _iUserStore.GetUserInfos().Where(w => !w.IsDelete && taskInfo.CreateUser==w.Id&& !string.IsNullOrWhiteSpace(w.UserId)).Select(s => s.UserId).ToListAsync();
                if (usersIds.Any())
                {
                    _hellperEmail.SendEmpPush($"您发布的任务《{(taskInfo.TaskName.Length>10? taskInfo.TaskName.Substring(0, 10)+"...":taskInfo.TaskName.Substring(0,10))}》被驳回",
                                             $"尊敬的勇士您好：您发布的任务《{taskInfo.TaskName}》被驳回，因为:{taskInfo.ExamineMemo},赶紧去看看吧。",
                                             usersIds);
                }
            }
            #endregion

            return response;
        }

        /// <summary>
        /// 任务调度回调,修改任务状态
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ResponseMessage> TaskStateRefreshCallback(ApiCore.Dto.ScheduleExecuteRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = new ResponseMessage();
            var args = request.Args;
            var taskId = args["id"]?.ToString() ?? "";
            if (!string.IsNullOrEmpty(taskId))
            {
                var taskInfo = await _iTaskStore.GetTaskInfos().Where(w => w.IsDelete == false && w.Id == taskId && w.TaskState == 0).FirstOrDefaultAsync(cancellationToken);
                if (taskInfo != null)
                {
                    taskInfo.TaskState = 1;
                    await _iTaskStore.UpdateTaskInfoAsync(taskInfo, cancellationToken);
                }
            }
            return response;
        }

        /// <summary>
        /// 审核任务列表
        /// </summary>
        /// <param name="user"></param>
        /// <param name="condition"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<PagingResponseMessage<TasksExamineResponse>> TasksExaminelistAsync(UserInfo user, TaskExamineListRequest condition, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = new PagingResponseMessage<TasksExamineResponse>();
            response.Extension = new List<TasksExamineResponse>();
            var taskInfoQuery = _iTaskStore.GetTaskExamineList(condition.PeriodId);

            if (!string.IsNullOrWhiteSpace(condition.TaskName)) //任务名称
            {
                taskInfoQuery = taskInfoQuery.Where(task => task.TaskName.Contains(condition.TaskName));
            }
            if (condition.ExamineState != null) //审核状态
            {
                taskInfoQuery = taskInfoQuery.Where(task => task.ExamineState == condition.ExamineState);
            }
            if (!string.IsNullOrWhiteSpace(condition.CreateUser)) //发布人
            {
                taskInfoQuery = taskInfoQuery.Where(task => task.CreateUser.Contains(condition.CreateUser));
            }
            if (condition.StartTime.HasValue)//发布开始时间
            {
                taskInfoQuery = taskInfoQuery.Where(task => task.CreateTime >= condition.StartTime);
            }
            if (condition.EndTime.HasValue)//发布结束时间
            {
                taskInfoQuery = taskInfoQuery.Where(task => task.CreateTime <= condition.EndTime);
            }


            response.PageIndex = condition.PageIndex;
            response.PageSize = condition.PageSize;
            response.TotalCount = await taskInfoQuery.CountAsync();
            var taskInfoList = await taskInfoQuery.Skip(condition.PageIndex * condition.PageSize).Take(condition.PageSize).ToListAsync();

            // 19-12-10 查询任务接取人
            var taskIds = taskInfoList.Select(a => a.Id);
            var periodId = condition.PeriodId;
            var taskUsers = await (from tu in _iTaskStore.GetTaskUsers()
                                   where taskIds.Contains(tu.TaskId)
                                   join u in _iUserStore.GetUserInfos() on tu.UserId equals u.Id
                                   select new TasksExamineResponse.TaskUserDetails4
                                   {
                                       TaskId = tu.TaskId,
                                       UserId = tu.UserId,
                                       UserName = u.UserName,
                                       OrganizationName = u.OrganizationName,
                                       GroupName = u.GroupName,
                                   })
                                   //.OrderBy(a => a.TaskId)
                                   //// 20200107-修复-王森 一个格子里面显示两行每行3个总共6个人，多的不显示
                                   //.Take(6)
                                   .ToListAsync(cancellationToken);
            //任务人员关系表
            var taskUserIdQuery = _iTaskStore.GetTaskUsers().Where(w => taskIds.Contains(w.TaskId)).Select(s => s.UserId);
            //参与人员积分(任务对应赛季的积分)
            var scoreInfoList = await _iTaskStore.GetScoreInfos().Where(w => taskUserIdQuery.Contains(w.UserId) && w.PeriodId == periodId).ToListAsync();
            //称号信息
            var titleList = await _iTaskStore.GetScoreTitles().Where(w => !w.IsDelete && w.PeriodId == periodId).ToListAsync();
            titleList.ForEach(fo => fo.Icon = _config["FileUrl"] + fo.Icon);
            taskUsers.ForEach(task =>
            {
                task.ScoreTitle = (titleList.FirstOrDefault(fi => fi.StartScore <= scoreInfoList.First(f => f.UserId == task.UserId).Score && fi.EndScore >= scoreInfoList.First(f => f.UserId == task.UserId).Score)?.Title) ?? "未知称号";
                task.Icon = titleList.FirstOrDefault(fi => fi.StartScore <= scoreInfoList.First(f => f.UserId == task.UserId).Score && fi.EndScore >= scoreInfoList.First(f => f.UserId == task.UserId).Score)?.Icon;
            });

            taskInfoList.ForEach(fo => fo.TaskUsers = taskUsers.Where(a => a.TaskId == fo.Id));
            response.Extension = taskInfoList;
            return response;
        }

        /// <summary>
        /// 获取K币任务列表
        /// </summary>
        /// <param name="user"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<PagingResponseMessage<TaskItemResponse>> GetKcoinTaskListAsync(UserInfo user, TaskSearchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var periodId = request.PeriodId;
            var response = new PagingResponseMessage<TaskItemResponse>();
            var querySql = @"SELECT 
                                uuid() uuid,
                                t.Id,
                                t.PeriodId, 
                                t.TaskName,
                                t.TaskTntro,
                                t.UserNumber,
                                t.CreateTime, 
                                CONCAT(u.UserName, '-', u.GroupName) CreateUser,
                                t.Score, 
                                t.LootTime, 
                                t.TaskState
                             FROM jf_taskinfo as t
                             LEFT JOIN jf_userinfo as u on t.CreateUser = u.Id";
            var whereSql = $" WHERE t.IsDelete = FALSE && t.ExamineState = 2 && t.PeriodId = '{periodId}'";
            var orderSql = @" ORDER BY FIELD(t.`TaskState`, 1, 0, 2, 3), 
                            (CASE t.TaskState WHEN 0 THEN t.LootTime WHEN 1 THEN t.LootTime END), 
                            (CASE t.TaskState WHEN 2 THEN t.CreateTime WHEN 3 THEN t.CreateTime END
                        ) DESC ";

            // 筛选 191209-任务名称、发布人、发布时间(End>CreateTime>=Start)、任务状态搜索
            if (!string.IsNullOrWhiteSpace(request.TaskName))
            {
                whereSql += $@" && t.TaskName LIKE '%{request.TaskName.Trim()}%'";
            }
            if (!string.IsNullOrWhiteSpace(request.CreateUser))
            {
                whereSql += $@" && CONCAT(u.UserName, '-', u.GroupName) LIKE '%{request.CreateUser.Trim()}%'";
            }
            if (request.CreateTimeStart != null)
            {
                whereSql += $@" && t.CreateTime >= '{request.CreateTimeStart?.ToString("yyyy-MM-dd HH:mm:ss")}'";
            }
            if (request.CreateTimeEnd != null)
            {
                whereSql += $@" && t.CreateTime < '{request.CreateTimeEnd?.ToString("yyyy-MM-dd HH:mm:ss")}'";
            }
            if (request.TaskState != null)
            {
                whereSql += $@" && t.TaskState = {request.TaskState}";
            }
            response.PageIndex = request.PageIndex;
            response.PageSize = request.PageSize;
            response.TotalCount = await _iTaskStore.TaskItems.FromSql(querySql + whereSql + orderSql).CountAsync(cancellationToken);
            var limitSql = $@" LIMIT {request.PageIndex * request.PageSize}, {request.PageSize}";
            var reList = await _iTaskStore.TaskItems.FromSql(querySql + whereSql + orderSql + limitSql).ToListAsync();
            
            // 数据组装
            // 19-12-10 任务接取人列表
            var taskIds = reList.Select(a => a.Id);
            var taskUsers = await (from tu in _iTaskStore.GetTaskUsers()
                                   where taskIds.Contains(tu.TaskId)
                                   join u in _iUserStore.GetUserInfos() on tu.UserId equals u.Id
                                   select new TaskItemResponse.TaskUserDetails5
                                   {
                                       TaskId = tu.TaskId,
                                       UserId = tu.UserId,
                                       UserName = u.UserName,
                                       OrganizationName = u.OrganizationName,
                                       GroupName = u.GroupName,
                                   })
                                   //.OrderBy(a => a.TaskId)
                                   //// 20200107-修复-王森 一个格子里面显示两行每行3个总共6个人，多的不显示
                                   //.Take(6)
                                   .ToListAsync(cancellationToken);
            //任务人员关系表
            var taskUserIdQuery = _iTaskStore.GetTaskUsers().Where(w => taskIds.Contains(w.TaskId)).Select(s => s.UserId);
            //参与人员积分(任务对应赛季的积分)
            var scoreInfoList = await _iTaskStore.GetScoreInfos().Where(w => taskUserIdQuery.Contains(w.UserId) && w.PeriodId == periodId).ToListAsync();
            //称号信息
            var titleList = await _iTaskStore.GetScoreTitles().Where(w => !w.IsDelete && w.PeriodId == periodId).ToListAsync();
            titleList.ForEach(fo => fo.Icon = _config["FileUrl"] + fo.Icon);
            taskUsers.ForEach(task =>
            {
                task.ScoreTitle = (titleList.FirstOrDefault(fi => fi.StartScore <= scoreInfoList.First(f => f.UserId == task.UserId).Score && fi.EndScore >= scoreInfoList.First(f => f.UserId == task.UserId).Score)?.Title) ?? "未知称号";
                task.Icon = titleList.FirstOrDefault(fi => fi.StartScore <= scoreInfoList.First(f => f.UserId == task.UserId).Score && fi.EndScore >= scoreInfoList.First(f => f.UserId == task.UserId).Score)?.Icon;
            });

            reList.ForEach(fo => fo.TaskUsers = taskUsers.Where(a => a.TaskId == fo.Id));

            response.Extension = reList;
            return response;
        }

        /// <summary>
        /// 抢任务
        /// </summary>
        /// <param name="user"></param>
        /// <param name="taskId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ResponseMessage> KcoinTaskGrab(Models.UserInfo user, string taskId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = new ResponseMessage();
            var taskInfo = await _iTaskStore.GetTaskInfos().Where(a => !a.IsDelete && a.Id == taskId).FirstOrDefaultAsync(cancellationToken);
            var nowTime = DateTime.Now;
            if (taskInfo == null)
            {
                response.Code = ResponseCodeDefines.NotFound;
                response.Message = "任务未找到";
                return response;
            }
            var periodInfo = await _iTaskStore.GetScorePeriods().Where(w => !w.IsDelete && w.Id == taskInfo.PeriodId).FirstOrDefaultAsync();
            if (periodInfo == null || periodInfo.State != 1)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "只有进行中的赛季才能操作";
                return response;
            }
            if (taskInfo.ExamineState != 2)
            {
                response.Code = ResponseCodeDefines.NotAllow;
                response.Message = "任务未审核通过";
                return response;
            }

            if (taskInfo.TaskState == 0 || nowTime < taskInfo.LootTime)
            {
                response.Code = ResponseCodeDefines.NotAllow;
                response.Message = "未到开抢时间";
                return response;
            }
            if (taskInfo.TaskState == 2 || taskInfo.TaskState == 3)
            {
                response.Code = ResponseCodeDefines.NotAllow;
                response.Message = "任务已被领取";
                return response;
            }
            var taskUser = new TaskUser
            {
                UserId = user.Id,
                TaskId = taskId,
                GetTime = nowTime,
            };
            //事务保存
            using (var trans = await _transaction.BeginTransaction())
            {
                try
                {
                    var oldTaskUser = await _iTaskStore.GetTaskUsers().Where(w => w.TaskId == taskInfo.Id).ToListAsync();
                    if (oldTaskUser.Any(fi => fi.UserId == user.Id))
                    {
                        response.Code = ResponseCodeDefines.NotAllow;
                        response.Message = "您已抢到该任务！";
                        trans.Rollback();
                        return response;
                    }
                    if (taskInfo.UserNumber < oldTaskUser.Count + 1)
                    {
                        response.Code = ResponseCodeDefines.NotAllow;
                        response.Message = "当前任务所需人数已满";
                        trans.Rollback();
                        return response;
                    }
                    await _iTaskStore.AddTaskUserAsync(taskUser, cancellationToken);
                    var taskUserQuery = _iTaskStore.GetTaskUsers().Where(a => a.TaskId == taskId);
                    var taskUserCount = await taskUserQuery.CountAsync(cancellationToken);
                    if (taskUserCount >= taskInfo.UserNumber)
                    {
                        taskInfo.TaskState = 2;
                        await _iTaskStore.UpdateTaskInfoAsync(taskInfo, cancellationToken);
                    }
                    // 抢任务成功添加动态
                    await _dynamicHelper.AddDynamicContent(
                        DynamicType.TaskGet,
                        taskInfo.PeriodId,
                        taskInfo.Id,
                        "",
                        "",
                        user.UserName,
                        user.GroupName,
                        user.Id,
                        taskInfo.TaskName,
                        taskInfo.Score,
                        null,
                        null);
                    trans.Commit();
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    throw new Exception("保存事务失败", e);
                }
            }
            return response;
        }

        /// <summary>
        /// 获取任务详情
        /// </summary>
        /// <param name="user"></param>
        /// <param name="taskId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<TaskDetailsResponse>> GetTaskDetailsAsync(Models.UserInfo user, string taskId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = new ResponseMessage<TaskDetailsResponse>();
            var taskInfo = await _iTaskStore.GetTaskInfos().Where(a => !a.IsDelete && a.Id == taskId).FirstOrDefaultAsync(cancellationToken);
            if (taskInfo == null)
            {
                response.Code = ResponseCodeDefines.NotFound;
                response.Message = "未找到相关任务";
                return response;
            }
            var newitem = new TaskDetailsResponse
            {
                Id = taskInfo.Id,
                PeriodId = taskInfo.PeriodId,
                TaskName = taskInfo.TaskName,
                TaskTntro = taskInfo.TaskTntro,
                UserNumber = taskInfo.UserNumber,
                Score = taskInfo.Score,
                CreateUser = (await _iUserStore.GetUserInfos().Where(w => w.Id == taskInfo.CreateUser && !w.IsDelete).FirstOrDefaultAsync())?.UserName,
                LootTime = taskInfo.LootTime.ToString("yyyy-MM-dd HH:mm:ss"),
                GetTime = (await _iTaskStore.GetTaskUsers().Where(w => w.UserId == user.Id && w.TaskId == taskId).FirstOrDefaultAsync())?.GetTime.ToString("yyyy-MM-dd HH:mm:ss"),
                TaskState = taskInfo.TaskState,
                ExamineState = taskInfo.ExamineState,
                ExamineMemo = taskInfo.ExamineMemo,
                Labels = taskInfo.Labels,
                CreateTime = taskInfo.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")
            };
            response.Extension = newitem;
            return response;
        }

        /// <summary>
        /// 任务参与人列表
        /// </summary>
        /// <param name="user"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<List<TaskUserDetails>>> TaskUserDetailsAsync(UserInfo user, string taskId)
        {
            var response = new ResponseMessage<List<TaskUserDetails>>();
            var taskInfo = await _iTaskStore.GetTaskInfos().Where(w => w.Id == taskId).FirstOrDefaultAsync();
            if (taskInfo == null)
            {
                response.Code = ResponseCodeDefines.NotFound;
                response.Message = "未找到相关任务";
                return response;
            }
            //任务人员关系表
            var taskUserIdQuery = _iTaskStore.GetTaskUsers().Where(w => w.TaskId == taskId).Select(s => s.UserId);
            //参与人员信息
            var userInfoList = await _iUserStore.GetUserInfos().Where(w => taskUserIdQuery.Contains(w.Id)).ToListAsync();
            //参与人员积分(任务对应赛季的积分)
            var scoreInfoList = await _iTaskStore.GetScoreInfos().Where(w => taskUserIdQuery.Contains(w.UserId) && w.PeriodId == taskInfo.PeriodId).ToListAsync();
            //称号信息
            var titleList = await _iTaskStore.GetScoreTitles().Where(w => !w.IsDelete && w.PeriodId == taskInfo.PeriodId).ToListAsync();
            titleList.ForEach(fo => fo.Icon = _config["FileUrl"] + fo.Icon);

            response.Extension = userInfoList.Select(s => new TaskUserDetails
            {
                UserId = s.Id,
                UserName = s.UserName,
                OrganizationName = s.OrganizationName,
                GroupName = s.GroupName,
                ScoreTitle = (titleList.FirstOrDefault(fi => fi.StartScore <= scoreInfoList.First(f => f.UserId == s.Id).Score && fi.EndScore >= scoreInfoList.First(f => f.UserId == s.Id).Score)?.Title) ?? "未知称号",
                Icon = titleList.FirstOrDefault(fi => fi.StartScore <= scoreInfoList.First(f => f.UserId == s.Id).Score && fi.EndScore >= scoreInfoList.First(f => f.UserId == s.Id).Score)?.Icon
            }).ToList();

            return response;
        }

        /// <summary>
        /// 获取我的任务列表
        /// </summary>
        /// <param name="user"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<PagingResponseMessage<MyTaskResponse>> MyTaskAsync(UserInfo user, MyTaskRequest request)
        {
            var response = new PagingResponseMessage<MyTaskResponse>();

            var myTaskQuery = _iTaskStore.GetMyTasksAsync(user.Id, request.PeriodId);

            if (!string.IsNullOrWhiteSpace(request.TaskName))//任务名称
            {
                myTaskQuery = myTaskQuery.Where(task => task.TaskName.Contains(request.TaskName));
            }
            if (request.TaskState != null) //任务状态
            {
                myTaskQuery = myTaskQuery.Where(task => task.TaskState == request.TaskState);
            }
            if (!string.IsNullOrWhiteSpace(request.CreateUser))//发布人
            {
                myTaskQuery = myTaskQuery.Where(task => task.CreateUser.Contains(request.CreateUser));
            }
            if (request.StartTime.HasValue)//发布开始时间
            {
                myTaskQuery = myTaskQuery.Where(task => task.CreateTime >= request.StartTime);
            }
            if (request.EndTime.HasValue)//发布开始时间
            {
                myTaskQuery = myTaskQuery.Where(task => task.CreateTime <= request.EndTime);
            }
            response.TotalCount = await myTaskQuery.CountAsync();
            var myTaskList = await myTaskQuery.Skip(request.PageIndex * request.PageSize).Take(request.PageSize).ToListAsync();
            response.PageIndex = request.PageIndex;
            response.PageSize = request.PageSize;

            // 查询任务接取人
            var taskIds = myTaskList.Select(a => a.Id);
            var periodId = request.PeriodId;
            var taskUsers = await (from tu in _iTaskStore.GetTaskUsers()
                                   where taskIds.Contains(tu.TaskId)
                                   join u in _iUserStore.GetUserInfos() on tu.UserId equals u.Id
                                   select new MyTaskResponse.TaskUserDetails3
                                   {
                                       TaskId = tu.TaskId,
                                       UserId = tu.UserId,
                                       UserName = u.UserName,
                                       OrganizationName = u.OrganizationName,
                                       GroupName = u.GroupName,
                                   })
                                   //.OrderBy(a => a.TaskId)
                                   //// 20200107-修复-王森 一个格子里面显示两行每行3个总共6个人，多的不显示
                                   //.Take(6)
                                   .ToListAsync();
            //任务人员关系表
            var taskUserIdQuery = _iTaskStore.GetTaskUsers().Where(w => taskIds.Contains(w.TaskId)).Select(s => s.UserId);
            //参与人员积分(任务对应赛季的积分)
            var scoreInfoList = await _iTaskStore.GetScoreInfos().Where(w => taskUserIdQuery.Contains(w.UserId) && w.PeriodId == periodId).ToListAsync();
            //称号信息
            var titleList = await _iTaskStore.GetScoreTitles().Where(w => !w.IsDelete && w.PeriodId == periodId).ToListAsync();
            titleList.ForEach(fo => fo.Icon = _config["FileUrl"] + fo.Icon);
            taskUsers.ForEach(task =>
            {
                task.ScoreTitle = (titleList.FirstOrDefault(fi => fi.StartScore <= scoreInfoList.First(f => f.UserId == task.UserId).Score && fi.EndScore >= scoreInfoList.First(f => f.UserId == task.UserId).Score)?.Title) ?? "未知称号";
                task.Icon = titleList.FirstOrDefault(fi => fi.StartScore <= scoreInfoList.First(f => f.UserId == task.UserId).Score && fi.EndScore >= scoreInfoList.First(f => f.UserId == task.UserId).Score)?.Icon;
            });

            myTaskList.ForEach(fo => fo.TaskUsers = taskUsers.Where(a => fo.Id == a.TaskId));
            response.Extension = myTaskList;

            return response;
        }

        /// <summary>
        /// 获取我发布的任务列表
        /// </summary>
        /// <param name="user"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<PagingResponseMessage<MyPushTaskResponse>> MyPushTaskAsync(UserInfo user, MyTaskRequest request)
        {

            var response = new PagingResponseMessage<MyPushTaskResponse>();

            var myTaskQuery = _iTaskStore.GetMyPushTasksAsync(user.Id, request.PeriodId);

            if (!string.IsNullOrWhiteSpace(request.TaskName))//任务名称
            {
                myTaskQuery = myTaskQuery.Where(task => task.TaskName.Contains(request.TaskName));
            }
            if (request.TaskState != null) //任务状态
            {
                myTaskQuery = myTaskQuery.Where(task => task.TaskState == request.TaskState);
            }
            if (!string.IsNullOrWhiteSpace(request.CreateUser))//发布人
            {
                myTaskQuery = myTaskQuery.Where(task => task.CreateUser.Contains(request.CreateUser));
            }
            if (request.StartTime.HasValue)//发布开始时间
            {
                myTaskQuery = myTaskQuery.Where(task => task.CreateTime >= request.StartTime);
            }
            if (request.EndTime.HasValue)//发布开始时间
            {
                myTaskQuery = myTaskQuery.Where(task => task.CreateTime <= request.EndTime);
            }
            response.TotalCount = await myTaskQuery.CountAsync();
            var myTaskList = await myTaskQuery.Skip(request.PageIndex * request.PageSize).Take(request.PageSize).ToListAsync();
            response.PageIndex = request.PageIndex;
            response.PageSize = request.PageSize;

            // 19-12-10 查询任务接取人
            var taskIds = myTaskList.Select(a => a.Id);
            var periodId = request.PeriodId;
            var taskUsers = await (from tu in _iTaskStore.GetTaskUsers()
                                   where taskIds.Contains(tu.TaskId)
                                   join u in _iUserStore.GetUserInfos() on tu.UserId equals u.Id
                                   select new MyPushTaskResponse.TaskUserDetails2
                                   {
                                       TaskId = tu.TaskId,
                                       UserId = tu.UserId,
                                       UserName = u.UserName,
                                       OrganizationName = u.OrganizationName,
                                       GroupName = u.GroupName,
                                   })
                                   //.OrderBy(a => a.TaskId)
                                   //// 20200107-修复-王森 一个格子里面显示两行每行3个总共6个人，多的不显示
                                   //.Take(6)
                                   .ToListAsync();
            //任务人员关系表
            var taskUserIdQuery = _iTaskStore.GetTaskUsers().Where(w => taskIds.Contains(w.TaskId)).Select(s => s.UserId);
            //参与人员积分(任务对应赛季的积分)
            var scoreInfoList = await _iTaskStore.GetScoreInfos().Where(w => taskUserIdQuery.Contains(w.UserId) && w.PeriodId == periodId).ToListAsync();
            //称号信息
            var titleList = await _iTaskStore.GetScoreTitles().Where(w => !w.IsDelete && w.PeriodId == periodId).ToListAsync();
            titleList.ForEach(fo => fo.Icon = _config["FileUrl"] + fo.Icon);
            taskUsers.ForEach(task =>
            {
                task.ScoreTitle = (titleList.FirstOrDefault(fi => fi.StartScore <= scoreInfoList.First(f => f.UserId == task.UserId).Score && fi.EndScore >= scoreInfoList.First(f => f.UserId == task.UserId).Score)?.Title) ?? "未知称号";
                task.Icon = titleList.FirstOrDefault(fi => fi.StartScore <= scoreInfoList.First(f => f.UserId == task.UserId).Score && fi.EndScore >= scoreInfoList.First(f => f.UserId == task.UserId).Score)?.Icon;
            });

            myTaskList.ForEach(fo => fo.TaskUsers = taskUsers.Where(a => a.TaskId == fo.Id));

            response.Extension = myTaskList;

            return response;
        }

        /// <summary>
        /// 新增/修改我的任务
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseMessage> SaveTaskInfo(Models.UserInfo user, TaskSaveRequest request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessage();
            if (request.Score <= 0)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "分数必须大于零";
                return response;
            }
            if (request.UserNumber == 0)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "所需人数必须大于零";
                return response;
            }
            var periodInfo = await _iTaskStore.GetScorePeriods().Where(w => !w.IsDelete && w.Id == request.PeriodId).FirstOrDefaultAsync();
            if (periodInfo == null || periodInfo.State != 1)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "只有进行中的赛季才能操作";
                return response;
            }
            var labelList = new List<string>();
            if (!string.IsNullOrWhiteSpace(request.Labels))
            {
                //标签处理,现在只是去重做存储
                labelList = request.Labels.Replace("，", ",").Split(",").Where(w => !string.IsNullOrWhiteSpace(w)).Select(s => s.ToLower()).Distinct().ToList();//请求的标签
                if (labelList.Where(w => w.Length > 12).Count() > 0)
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "单个标签必须小于6个字";
                    return response;
                }
            }
            if (string.IsNullOrEmpty(request.Id))
            {
                var saveInfo = new TaskInfo()
                {
                    Id = Guid.NewGuid().ToString(),
                    CreateUser = user.Id,
                    CreateTime = DateTime.Now,
                    ExamineMemo = string.Empty,
                    ExamineState = 1,
                    ExamineTime = null,
                    TaskState = 0,
                    IsDelete = false,
                    ExamineUser = string.Empty,
                    Labels = labelList.Count > 0 ? string.Join(",", labelList) : "",
                    LootTime = request.LootTime.GetValueOrDefault(DateTime.Now),
                    PeriodId = request.PeriodId,
                    Score = request.Score ?? 0,
                    TaskName = request.TaskName,
                    TaskTntro = request.TaskTntro,
                    UserNumber = request.UserNumber ?? 0
                };
                await _iTaskStore.AddTaskInfoAsync(saveInfo, cancellationToken);
                #region 发送任务审核提醒
                var adminUserIds = await _iUserStore.GetUserInfos().Where(w => w.IsAdmin && !w.IsDelete&& !string.IsNullOrWhiteSpace(w.UserId)).Select(s=>s.UserId).ToListAsync();
                if (adminUserIds.Any())
                {
                    _hellperEmail.SendEmpPush($"{periodInfo.Caption}中有新任务需要您审核",
                                             $"尊敬的系统管理员您好：有一条任务需要您审核：《{request.TaskName}》。",
                                             adminUserIds);
                }
                #endregion
            }
            else
            {
                var oldTaskInfo = await _iTaskStore.GetTaskInfos().FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

                if (oldTaskInfo == null || user.Id != oldTaskInfo.CreateUser)
                {
                    response.Code = ResponseCodeDefines.NotFound;
                    response.Message = "原任务信息已不存在";
                    return response;
                }
                if (oldTaskInfo.TaskState >= 2)
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "进行中和已完结的任务不能修改";
                    return response;
                }
                if (oldTaskInfo.ExamineState == 3)
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "审核驳回的任务不能修改";
                    return response;
                }
                var oldTaskUserCount = await _iTaskStore.GetTaskUsers().Where(w => w.TaskId == oldTaskInfo.Id).CountAsync();
                if (request.UserNumber < oldTaskUserCount)
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = $"需要人数不能小于当前已抢的人数:{oldTaskUserCount}";
                    return response;
                }
                else if (request.UserNumber == oldTaskUserCount)
                {
                    oldTaskInfo.TaskState = 2;//如果修改的人数等于了已领取的人数,就开始任务
                }
                //简介,人数,印象标签(如果是已抢中 则人数只能该大不能改小)
                oldTaskInfo.TaskTntro = request.TaskTntro;
                oldTaskInfo.UserNumber = request.UserNumber ?? 1;
                oldTaskInfo.Labels = labelList.Count > 0 ? string.Join(",", labelList) : "";

                await _iTaskStore.UpdateTaskInfoAsync(oldTaskInfo, cancellationToken);
            }
            return response;
        }

        /// <summary>
        /// 完结任务
        /// </summary>
        /// <param name="user"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public async Task<ResponseMessage> OverTaskAsync(UserInfo user, string taskId)
        {
            var response = new ResponseMessage();
            var taskInfo = await _iTaskStore.GetTaskInfos().Where(w => !w.IsDelete && w.Id == taskId).FirstOrDefaultAsync();
            if (taskInfo == null)
            {
                response.Code = ResponseCodeDefines.NotFound;
                response.Message = "未找到相关任务";
                return response;
            }
            var periodInfo = await _iTaskStore.GetScorePeriods().Where(w => !w.IsDelete && w.Id == taskInfo.PeriodId).FirstOrDefaultAsync();
            if (periodInfo == null || periodInfo.State != 1)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "只有进行中的赛季才能操作";
                return response;
            }
            if (taskInfo.CreateUser != user.Id)
            {
                response.Code = ResponseCodeDefines.NotAllow;
                response.Message = "不能操作其他人创建的任务";
                return response;
            }

            //印象标签
            var userLabelList = new List<UserLabel>();
            var labelList = new List<string>();
            if (!string.IsNullOrWhiteSpace(taskInfo.Labels))
            {
                //标签处理,现在只是去重做存储
                labelList = taskInfo.Labels.Replace("，", ",").Split(",").Where(w => !string.IsNullOrWhiteSpace(w)).Distinct().ToList();//请求的标签
                if (labelList.Where(w => w.Length > 12).Count() > 0)
                {
                    response.Code = ResponseCodeDefines.NotAllow;
                    response.Message = "标签必须小于6个字";
                    return response;
                }
            }
            //分配积分
            var taskUserList = await _iTaskStore.GetTaskUsers().Where(w => w.TaskId == taskId).Select(s => s.UserId).ToListAsync();
            var taskUserInfoList = await _iUserStore.GetUserInfos().Where(w => !w.IsDelete && taskUserList.Contains(w.Id)).ToListAsync();
            //平均每人分多少分
            int userScore = 0;
            //事务保存
            using (var trans = await _transaction.BeginTransaction())
            {

                try
                {
                    //查询拿到事务里面防止并发
                    var scoreInfoList = await _iTaskStore.GetScoreInfos().Where(w => !w.IsDelete && w.PeriodId == taskInfo.PeriodId && taskUserList.Contains(w.UserId)).ToListAsync();
                    if (!scoreInfoList.Any())
                    {
                        response.Code = ResponseCodeDefines.NotFound;
                        response.Message = "未找到相关完成人";
                        return response;
                    }
                    userScore = taskInfo.Score / scoreInfoList.Count;
                    var scoreDetailedList = new List<ScoreDetailed>();
                    foreach (var scoreInfo in scoreInfoList)
                    {
                        scoreInfo.Score = scoreInfo.Score + userScore;
                        scoreInfo.ConsumableScore = scoreInfo.ConsumableScore + userScore;
                        //积分明细
                        scoreDetailedList.Add(new ScoreDetailed
                        {
                            Id = Guid.NewGuid().ToString(),
                            PeriodId = scoreInfo.PeriodId,
                            DealType = 3,
                            UserId = taskInfo.CreateUser,
                            TaskId = taskInfo.Id,
                            DealUserId = user.Id,
                            Theme = taskInfo.TaskName,
                            Memo = taskInfo.TaskTntro,
                            Score = userScore,
                            ScoreId = scoreInfo.Id,
                            CreateTime = DateTime.Now,
                            CreateUser = user.Id,
                            IsDelete = false,
                            Labels = taskInfo.Labels
                        });
                        //印象标签
                        foreach (var lable in labelList)
                        {
                            userLabelList.Add(new UserLabel
                            {
                                Id = Guid.NewGuid().ToString(),
                                UserId = scoreInfo.UserId,
                                Label = lable,
                            });
                        }
                        //添加收入动态
                        await _dynamicHelper.AddDynamicContent(
                            DynamicType.TaskIncome,
                            taskInfo.PeriodId,
                            scoreInfo.Id,
                            null,
                            null,
                            taskUserInfoList.FirstOrDefault(fi => fi.Id == scoreInfo.UserId)?.UserName,
                            taskUserInfoList.FirstOrDefault(fi => fi.Id == scoreInfo.UserId)?.GroupName,
                            scoreInfo.UserId,
                            taskInfo.TaskName,
                            userScore,
                            null,
                            null
                            );
                    }
                    //更新任务
                    taskInfo.TaskState = 3;
                    await _iTaskStore.UpdateTaskInfoAsync(taskInfo);
                    //更新积分信息
                    await _iTaskStore.UpdateScoreInfoList(scoreInfoList);
                    //添加积分明细
                    if (scoreDetailedList != null && scoreDetailedList.Count != 0)
                        await _iTaskStore.CreateScoreDetailedList(scoreDetailedList);
                    //添加用户标签
                    if (userLabelList != null && userLabelList.Count != 0)
                        await _iTaskStore.CreateUserLabelList(userLabelList);
                    trans.Commit();
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    throw new Exception("保存事务失败", e);
                }

                #region 任务完结发送提醒
                var usersIds = taskUserInfoList.Where(w => !string.IsNullOrWhiteSpace(w.UserId)).Select(s=>s.UserId).ToList();
                if (usersIds.Any())
                {
                    _hellperEmail.SendEmpPush($"您在{periodInfo.Caption}中有任务已完结，快去看看吧！",                                             
                                             $"尊敬的勇士您好：您在《{periodInfo.Caption}》中完成了任务《{taskInfo.TaskName}》获得了{userScore}K币，赶紧去看看吧。",
                                             usersIds);
                }
                #endregion
            }

            // 触发排行榜,个人信息,动态变化
            await _sendClientMessageManager.SendInfos(new List<Dto.Common.SendClientType>() {
                Dto.Common.SendClientType.Rank,
                Dto.Common.SendClientType.Dynamic,
                Dto.Common.SendClientType.User
            });
            return response;
        }
    }
}
