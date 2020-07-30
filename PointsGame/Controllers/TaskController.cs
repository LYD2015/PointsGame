using ApiCore;
using ApiCore.Dto;
using AspNet.Security.OAuth.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using PointsGame.Dto.Request;
using PointsGame.Dto.Response;
using PointsGame.Filters;
using PointsGame.Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XYH.Core.Log;

namespace PointsGame.Controllers
{
    /// <summary>
    /// 任务控制器
    /// </summary>
    [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    [Route("api/task")]
    public class TaskController: Controller
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskManager"></param>
        /// <param name="distributedCache"></param>
        public TaskController(TaskManager taskManager, IDistributedCache distributedCache)
        {
            _taskManager = taskManager ?? throw new ArgumentNullException(nameof(taskManager));
            _cache = distributedCache;
        }

        private readonly TaskManager _taskManager;
        private readonly ILogger Logger = LoggerManager.GetLogger(nameof(TaskController));
        private readonly IDistributedCache _cache;

        /// <summary>
        /// 任务详情(K币任务,我的任务,我的发布,任务审核这四个类别中查看任务详情都是这个接口)
        /// </summary>
        /// <param name="user"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        [HttpGet("details/{taskId}")]
        [CheckPermission]
        public async Task<ResponseMessage<TaskDetailsResponse>> TaskDetails(Models.UserInfo user,[FromRoute] string taskId)
        {
            var response = new ResponseMessage<TaskDetailsResponse>();

            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})K币任务详情，请求参数为：\r\n{JsonHelper.ToJson(taskId)}");
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = ModelState.GetAllErrors();
                Logger.Warn($"用户{user?.UserName ?? ""}({user?.Id ?? ""})K币任务详情，模型验证失败：\r\n{response.Message ?? ""}");
                return response;
            }
            try
            {
                response = await _taskManager.GetTaskDetailsAsync(user, taskId);
            }
            catch (Exception e)
            {
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})K币任务详情，报错：{e.Message}\r\n{e.StackTrace}");
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                return response;
            }

            return response;
        }

        /// <summary>
        /// 任务参与人列表
        /// </summary>
        /// <param name="user"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        [HttpGet("user/{taskId}")]
        [CheckPermission]
        public async Task<ResponseMessage<List<TaskUserDetails>>> TaskUserDetails(Models.UserInfo user, [FromRoute] string taskId)
        {
            var response = new ResponseMessage<List<TaskUserDetails>> ();

            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})K币任务详情，请求参数为：\r\n{JsonHelper.ToJson(taskId)}");
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = ModelState.GetAllErrors();
                Logger.Warn($"用户{user?.UserName ?? ""}({user?.Id ?? ""})K币任务详情，模型验证失败：\r\n{response.Message ?? ""}");
                return response;
            }
            try
            {
                response = await _taskManager.TaskUserDetailsAsync(user, taskId);
            }
            catch (Exception e)
            {
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})K币任务详情，报错：{e.Message}\r\n{e.StackTrace}");
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                return response;
            }

            return response;
        }

        #region 我的任务/我的发布

        /// <summary>
        /// 获取我的任务列表【V1.1】
        /// </summary>
        /// <param name="user"></param>
        /// <param name="myTaskRequest"></param>
        /// <returns></returns>
        [HttpPost("my")]
        [CheckPermission]
        public async Task<PagingResponseMessage<MyTaskResponse>> MyTask(Models.UserInfo user,[FromBody] MyTaskRequest myTaskRequest)
        {
            var response = new PagingResponseMessage<MyTaskResponse>();

            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})获取我的任务列表，请求参数为：\r\n{JsonHelper.ToJson(myTaskRequest)}");
            try
            {
                response = await _taskManager.MyTaskAsync(user, myTaskRequest);
            }
            catch (Exception e)
            {
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})获取我的任务列表，报错：{e.Message}\r\n{e.StackTrace}");
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                return response;
            }

            return response;
        }

        /// <summary>
        /// 获取我发布的任务列表【V1.1】
        /// </summary>
        /// <param name="user"></param>
        /// <param name="myPushTaskRequest"></param>
        /// <returns></returns>
        [HttpPost("my/push")]
        [CheckPermission]
        public async Task<PagingResponseMessage<MyPushTaskResponse>> MyPushTask(Models.UserInfo user, [FromBody] MyTaskRequest myPushTaskRequest)
        {
            var response = new PagingResponseMessage<MyPushTaskResponse>();

            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})获取我的发布的任务列表。");
            try
            {
                response = await _taskManager.MyPushTaskAsync(user, myPushTaskRequest);
            }
            catch (Exception e)
            {
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})获取我的发布的任务列表，报错：{e.Message}\r\n{e.StackTrace}");
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                return response;
            }

            return response;
        }

        /// <summary>
        /// 新增/修改我的任务(如果传入ID则会直接更新，否则新增。前端注意：修改只能修改简介、人数、印象标签，进行中和已完结的任务不能修改。如果修改时已经有部分人抢到了任务，那么人数的修改不能小于现在已抢到的人数)
        /// </summary>
        /// <returns></returns>
        [HttpPost("save")]
        [CheckPermission]
        public async Task<ResponseMessage> SaveTaskInfo(Models.UserInfo user,[FromBody] TaskSaveRequest request)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})新增/修改我的任务:{JsonHelper.ToJson(request)}");
            var response = new ResponseMessage();

            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = ModelState.GetAllErrors();
                Logger.Warn($"用户{user?.UserName ?? ""}({user?.Id ?? ""})新增/修改我的任务，模型验证失败：\r\n{response.Message ?? ""}");
                return response;
            }
            try
            {
                response = await _taskManager.SaveTaskInfo(user, request, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})新增/修改任务，报错：{e.Message}\r\n{e.StackTrace}");
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                return response;
            }
            
            return response;
        }

        /// <summary>
        /// 完结任务
        /// </summary>
        /// <param name="user"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        [HttpPut("over/{taskId}")]
        [CheckPermission]
        public async Task<ResponseMessage> OverTask(Models.UserInfo user, [FromRoute] string taskId)
        {
            var response = new ResponseMessage();

            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})完结任务:taskId{taskId}");
            try
            {
                response = await _taskManager.OverTaskAsync(user, taskId);
            }
            catch (Exception e)
            {
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})完结任务，报错：{e.Message}\r\n{e.StackTrace}");
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                return response;
            }

            return response;
        }

        #endregion

        #region 审核任务

        /// <summary>
        /// 审核任务
        /// </summary>
        /// <param name="user"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        [HttpPost("examine")]
        [CheckPermission]
        public async Task<ResponseMessage> ExamineTask(Models.UserInfo user,[FromBody]ExamineTaskRequest condition)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})管理员审核任务，请求体为：\r\n{JsonHelper.ToJson(condition)}");
            var response = new ResponseMessage();

            var prefixs = new string[] { "TaskController" };
            var key = $"{user.Id}{condition.TaskId}{condition.IsOk}";
            try
            {
                // 防止重复提交
                await _cache.LockSubmit(prefixs, key, "ExamineTask", HttpContext.RequestAborted);
                response = await _taskManager.ExaminetaskAsync(user, condition);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})管理员审核任务，失败：{e.Message}错误堆栈信息：\r\n" + (JsonHelper.ToJson(e.StackTrace)));
            }
            finally
            {
                // 成功失败都要移除
                await _cache.UnlockSubmit(prefixs, key);
            }
            return response;
        }

        /// <summary>
        /// 更新任务状态(任务调度回调,前端不调用)（任务状态：0->1）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("state/refresh/callback")]
        [AllowAnonymous]
        public async Task<ResponseMessage> TaskStateRefreshCallback([FromBody] ScheduleExecuteRequest request)
        {
            var response = new ResponseMessage();

            Logger.Trace($"更新任务状态,任务调度回调，请求参数为：\r\n{JsonHelper.ToJson(request)}");
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = ModelState.GetAllErrors();
                Logger.Warn($"更新任务状态,任务调度回调，模型验证失败：\r\n{response.Message ?? ""}");
                return response;
            }
            try
            {
                return await _taskManager.TaskStateRefreshCallback(request, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                Logger.Error($"更新任务状态,任务调度回调，报错：{e.Message}\r\n{e.StackTrace}");
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                return response;
            }
        }

        /// <summary>
        /// 审核任务列表【V1.1】
        /// </summary>
        /// <param name="user"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        [HttpPost("examine/list")]
        [CheckPermission]
        public async Task<PagingResponseMessage<TasksExamineResponse>> TasksExaminelist(Models.UserInfo user,[FromBody]TaskExamineListRequest condition)
        {
            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})获取审核任务列表，请求体为：\r\n{JsonHelper.ToJson(condition)}");
            var response = new PagingResponseMessage<TasksExamineResponse>();
            try
            {
                response = await _taskManager.TasksExaminelistAsync(user, condition);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})获取审核任务列表，失败：{e.Message}错误堆栈信息：\r\n" + (JsonHelper.ToJson(e.StackTrace)));
            }
            return response;
        }

        #endregion

        #region K币任务

        /// <summary>
        /// K币任务列表
        /// </summary>
        /// <param name="user"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("kcoin/list")]
        [CheckPermission]
        public async Task<PagingResponseMessage<TaskItemResponse>> KcoinTaskList(Models.UserInfo user,[FromBody] TaskSearchRequest request)
        {
            var response = new PagingResponseMessage<TaskItemResponse>();

            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})K币任务列表，请求参数为：\r\n{JsonHelper.ToJson(request)}");
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = ModelState.GetAllErrors();
                Logger.Warn($"用户{user?.UserName ?? ""}({user?.Id ?? ""})K币任务列表，模型验证失败：\r\n{response.Message ?? ""}");
                return response;
            }
            try
            {
                return await _taskManager.GetKcoinTaskListAsync(user, request, HttpContext.RequestAborted);
            }
            catch (Exception e)
            {
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})K币任务列表，报错：{e.Message}\r\n{e.StackTrace}");
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                return response;
            }
        }
               
        /// <summary>
        /// 抢K币任务
        /// </summary>
        /// <param name="user"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        [HttpPut("kcoin/grab/{taskId}")]
        [CheckPermission]
        public async Task<ResponseMessage> KcoinTaskGrab(Models.UserInfo user,[FromRoute] string taskId)
        {
            var response = new ResponseMessage();

            Logger.Trace($"用户{user?.UserName ?? ""}({user?.Id ?? ""})抢K币任务，请求参数为：\r\n{JsonHelper.ToJson(taskId)}");
            if (!ModelState.IsValid)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = ModelState.GetAllErrors();
                Logger.Warn($"用户{user?.UserName ?? ""}({user?.Id ?? ""})抢K币任务，模型验证失败：\r\n{response.Message ?? ""}");
                return response;
            }
            var prefixs = new string[] { "TaskController" };
            var key = $"{user.Id}{taskId}";
            try
            {
                // 防止重复提交
                await _cache.LockSubmit(prefixs, key, "KcoinTaskGrab", HttpContext.RequestAborted);
                response = await _taskManager.KcoinTaskGrab(user, taskId);
            }
            catch (Exception e)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                response.Message = e.Message;
                Logger.Error($"用户{user?.UserName ?? ""}({user?.Id ?? ""})抢K币任务，报错：{e.Message}\r\n{e.StackTrace}");
            }
            finally
            {
                // 成功失败都要移除
                await _cache.UnlockSubmit(prefixs, key);
            }
            return response;
        }

        #endregion
    }
}
