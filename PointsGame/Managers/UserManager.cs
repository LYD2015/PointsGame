using ApiCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    /// 用户
    /// </summary>
    public class UserManager
    {
        private IUserStore _userStore { get; }
        private IScoreInfoStore _scoreInfoStore { get; }
        private HelperAuthentication helperAuthentication { get; }
        private readonly ITransaction<PointsGameDbContext> _transaction;//事务       
        private readonly AllocateManager _allocateManager;
        private readonly HellperPush _hellperEmail;
        private ILogger Logger = LoggerManager.GetLogger(nameof(UserManager));
        private HelperSendClientMessage _sendClientMessageManager;
        public UserManager(IUserStore userStore, IScoreInfoStore scoreInfoStore, HelperAuthentication tokenHelper, ITransaction<PointsGameDbContext> transaction, AllocateManager allocateManager, HellperPush hellperEmail,
            HelperSendClientMessage sendClientMessageManager)
        {
            _userStore = userStore ?? throw new ArgumentNullException(nameof(userStore));
            _scoreInfoStore = scoreInfoStore ?? throw new ArgumentNullException(nameof(scoreInfoStore));
            helperAuthentication = tokenHelper ?? throw new ArgumentNullException(nameof(tokenHelper));
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            _allocateManager = allocateManager;
            _hellperEmail = hellperEmail;
            _sendClientMessageManager = sendClientMessageManager;
        }

        /// <summary>
        /// 搜索用户
        /// </summary>
        /// <param name="user"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<PagingResponseMessage<UserResponse>> Search(UserInfo user, UserSearchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = new PagingResponseMessage<UserResponse>();

            var query = _userStore.GetUserInfos().Where(a => a.IsDelete == false);

            // 过滤
            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                query = query.Where(u => u.LoginName.Contains(request.Filter) || u.UserName.Contains(request.Filter) || u.GroupName.Contains(request.Filter));
            }
            // 排序
            query = query.OrderBy(a => a.UserName);
            // 分页
            response.PageIndex = request.PageIndex;
            response.PageSize = request.PageSize;
            response.TotalCount = await query.LongCountAsync(cancellationToken);
            if (request.Type == Dto.Common.PageType.All)
            {
                response.Extension = await query.Select(a => new UserResponse
                {
                    Id = a.Id,
                    LoginName = a.LoginName,
                    UserName = a.UserName,
                    OrganizationName = a.OrganizationName,
                    GroupName = a.GroupName,
                    IsAdmin = a.IsAdmin,
                    ZenTao = a.ZenTao
                }).ToListAsync(cancellationToken);
            }
            else
            {
                response.Extension = await query.Skip(request.PageIndex * request.PageSize).Take(request.PageSize).Select(a => new UserResponse
                {
                    Id = a.Id,
                    LoginName = a.LoginName,
                    UserName = a.UserName,
                    OrganizationName = a.OrganizationName,
                    GroupName = a.GroupName,
                    IsAdmin = a.IsAdmin,
                    ZenTao = a.ZenTao
                }).ToListAsync(cancellationToken);
            }

            return response;
        }

        /// <summary>
        /// 新增/修改用户
        /// </summary>
        /// <param name="user"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ResponseMessage> Save(UserInfo user, UserSaveRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = new ResponseMessage();
            if (!user.IsAdmin)
            {
                response.Code = ResponseCodeDefines.NotAllow;
                response.Message = "权限不足";
                return response;
            }
            if (string.IsNullOrWhiteSpace(request.Id))
            {
                if (request.LoginName == null)
                {
                    response.Code = ResponseCodeDefines.ArgumentNullError;
                    response.Message = "登陆名不能为空";
                    return response;
                }
                if (await _userStore.GetUserInfos().AnyAsync(a => a.LoginName.Equals(request.LoginName), cancellationToken))
                {
                    response.Code = ResponseCodeDefines.ObjectAlreadyExists;
                    response.Message = "新增用户的登陆名已经存在";
                    return response;
                }
                // 新增
                var addUser = new UserInfo
                {
                    Id = Guid.NewGuid().ToString(),
                    LoginName = request.LoginName,
                    UserName = request.UserName,
                    OrganizationName = request.OrganizationName,
                    GroupName = request.GroupName,
                    IsAdmin = request.IsAdmin,
                    CreateUser = user.Id,
                    CreateTime = DateTime.Now,
                    IsDelete = false,
                    ZenTao = request.ZenTao,
                };
                ScoreInfo scoreInfo = null;
                var scorePeriod = await _scoreInfoStore.GetScorePeriods().Where(w => !w.IsDelete && w.State == 1).FirstOrDefaultAsync();
                if (scorePeriod != null)
                {
                    var scoreInfoOld = await _scoreInfoStore.GetScoreInfos().Where(w => w.PeriodId == scorePeriod.Id && !w.IsDelete && w.UserId == addUser.Id).FirstOrDefaultAsync();
                    if (scoreInfoOld == null)
                    {
                        var nowTime = DateTime.Now;
                        scoreInfo = new ScoreInfo
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserId = addUser.Id,
                            CreateUser = addUser.Id,
                            CreateTime = nowTime,
                            IsDelete = false,
                            Score = 0,
                            ConsumableScore = 0,
                            PeriodId = scorePeriod.Id,
                            UpdateTime = nowTime,
                            UpdateUser = addUser.Id
                        };
                    }
                }

                using (var trans = await _transaction.BeginTransaction())
                {
                    try
                    {
                        if (scoreInfo != null)
                        {
                            await _scoreInfoStore.CreateScoreInfo(scoreInfo);
                        }
                        await _userStore.Create(new List<UserInfo> { addUser }, cancellationToken);
                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        throw;
                    }
                }
                // 触发排行榜变化
                await _sendClientMessageManager.SendInfo(Dto.Common.SendClientType.Rank);
            }
            else
            {
                // 修改
                var entity = await _userStore.GetUserInfos().Where(a => a.Id.Equals(request.Id)).FirstOrDefaultAsync(cancellationToken);

                if (!string.IsNullOrWhiteSpace(request.LoginName))
                {
                    if (entity.UserId != null && request.LoginName != entity.LoginName)
                    {
                        response.Code = ResponseCodeDefines.NotAllow;
                        response.Message = "已经登陆的用户不允许修改登陆名";
                        return response;
                    }
                    if (await _userStore.GetUserInfos().AnyAsync(a => a.LoginName.Equals(request.LoginName) && a.Id != request.Id, cancellationToken))
                    {
                        response.Code = ResponseCodeDefines.ObjectAlreadyExists;
                        response.Message = "用户的登陆名与其他用户名重复";
                        return response;
                    }
                }
                
                if (entity.IsAdmin == true && request.IsAdmin == false)
                {
                    var adminList = await _userStore.GetUserInfos().Where(w => !w.IsDelete && w.IsAdmin).ToListAsync();
                    if (adminList.Count == 1 && adminList[0].Id == entity.Id)
                    {
                        response.Code = ResponseCodeDefines.ObjectAlreadyExists;
                        response.Message = "您已经是最后一个管理员了，不能取消自己的管理员权限。";
                        return response;
                    } 
                }
                entity.LoginName = string.IsNullOrWhiteSpace(request.LoginName) ? entity.LoginName : request.LoginName;
                entity.UserName = request.UserName ?? entity.UserName;
                entity.OrganizationName = request.OrganizationName ?? entity.OrganizationName;
                entity.GroupName = request.GroupName ?? entity.GroupName;
                entity.IsAdmin = request.IsAdmin;
                entity.ZenTao = request.ZenTao;
                await _userStore.Update(new List<UserInfo> { entity }, cancellationToken);
            }
            return response;
        }


        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="user"></param>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ResponseMessage> Delete(UserInfo user, string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = new ResponseMessage();
            if (!user.IsAdmin)
            {
                response.Code = ResponseCodeDefines.NotAllow;
                response.Message = "权限不足";
                return response;
            }
            await _userStore.Delete(new List<string> { id }, cancellationToken);

            // 触发排行榜变化
            await _sendClientMessageManager.SendInfo(Dto.Common.SendClientType.Rank);

            return response;
        }

        /// <summary>
        /// 登陆（获取Token）-自动绑定认证中心UserId
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseMessage<IActionResult>> SignIn(SignRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = new ResponseMessage<IActionResult>();
            var localUser = await _userStore.GetUserInfos().FirstOrDefaultAsync(a => a.LoginName.Equals(request.LoginName), cancellationToken);
            if (localUser == null)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "用户不存在,请联系管理员添加用户";
                return response;
            }
            var token = await helperAuthentication.GetUserTokenObject(request);
            if (!token.HasValues)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "用户名或密码错误";
                return response;               
            }
            if (token["error"] != null)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = token["error"].ToString();
                return response;               
            }
            var userId = HelperAuthentication.GetUserId(token["access_token"].ToString());
            localUser.UserId = userId;
            //登录日志
            var signLog = new UserSignLog
            {
                Id = Guid.NewGuid().ToString(),
                LoginName = localUser.LoginName,
                Platform = 0, // TODO header里面传
                SigninTime = DateTime.Now,
                LocalUserId = localUser.Id,
                UserId = userId,
                UserName = localUser.UserName,
            };

            localUser.Email =await helperAuthentication.GetUserEmail(userId, token["access_token"].ToString());

            //事务保存数据
            using (var trans = await _transaction.BeginTransaction())
            {
                try
                {
                    await _userStore.Create(new List<UserSignLog> { signLog }, cancellationToken);
                    await _userStore.Update(new List<UserInfo> { localUser }, cancellationToken);
                    trans.Commit();
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    throw e;
                }
            }
            // [王森-191008-添加是否为管理员字段]
            token.Add("isAdmin", localUser.IsAdmin);
            response.Extension = new JsonResult(token);         
            //try
            //{                
            //   await EasterEgg(localUser.Id);
            //}
            //catch (Exception e)
            //{

            //    Logger.Error($"彩蛋触发失败:{e.Message}\r\n{e.StackTrace}");
            //}

            return response;
        }

        #region Egg
        /// <summary>
        /// 2019-12-16取消彩蛋
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task EasterEgg(string userId)
        {
            //Egg1
            if (DateTime.Now.Hour <= 8 && DateTime.Now.Hour >= 6)
            {
                var periodNow = await _scoreInfoStore.GetScorePeriods().FirstOrDefaultAsync(w => !w.IsDelete && w.State == 1);
                if (periodNow == null)
                {
                    return;
                }
                //今天是否加过
                var toDayAdd = await _scoreInfoStore.GetScoreInfoDetaileds().AnyAsync(a => a.PeriodId == periodNow.Id && !a.IsDelete && a.DealUserId == userId && DateTime.Now.Date == a.CreateTime.Date && a.Memo == "Egg1");
                if (!toDayAdd)
                {
                    var egg1UserNumber = await _scoreInfoStore.GetScoreInfoDetaileds().Where(w => w.PeriodId == periodNow.Id && !w.IsDelete && w.Memo == "Egg1").Select(s => s.DealUserId).Distinct().CountAsync();
                    var userNumber = await _userStore.GetUserInfos().Where(w => !w.IsDelete).CountAsync();
                    var half = (egg1UserNumber > (userNumber / 2)) && ((userNumber / 2) > 10);//是否大于一半
                    var allocateSubmitRequest = new AllocateSubmitRequest
                    {
                        AllocateUserId = userId,
                        PeriodId = periodNow.Id,
                        Theme = !half ? "神秘彩蛋一" : "早间签到",
                        Memo = "Egg1",
                        Score = 10,
                        Labels = _scoreInfoStore.GetScoreInfoDetaileds().Where(w => w.PeriodId == periodNow.Id && !w.IsDelete && w.Memo == "Egg1" && w.DealUserId == userId).Count() > 30 ? "不屈不挠" : ""
                    };
                    await _allocateManager.AllocateSubmitAsync(new UserInfo { Id = "K先生" }, allocateSubmitRequest, default(CancellationToken), true);
                    //大于一半人的时候向所有人公布Egg1
                    if ((userNumber / 2) + 1 == egg1UserNumber)
                    {
                        //var allUserEmail = await _userStore.GetUserInfos().Where(w => !w.IsDelete && !string.IsNullOrEmpty(w.Email)).Select(s => s.Email).ToListAsync();
                        //_hellperEmail.SendEmail("积分系统紧急通知【神秘彩蛋一】",
                        //                        $"尊敬的勇士您好：",
                        //                        $"目前已有超过一半的人触发了《神秘彩蛋一》，为了公平，K先生现在向所有人公布《神秘彩蛋一》：为了鼓励大家早上早到，在每天的早上6点到9点之间重新登录积分系统可获得10K币，保持登录不算哦。--K先生",
                        //                        allUserEmail);
                    }
                }
            }
            //Egg2
            if (DateTime.Now.Hour <= 23 && DateTime.Now.Hour >= 18)
            {               
                var periodNow = await _scoreInfoStore.GetScorePeriods().FirstOrDefaultAsync(w => !w.IsDelete && w.State == 1);
                if (periodNow == null)
                {
                    return;
                }
                //今天是否加过
                var toDayAdd = await _scoreInfoStore.GetScoreInfoDetaileds().AnyAsync(a => a.PeriodId == periodNow.Id && !a.IsDelete && a.DealUserId == userId && DateTime.Now.Date == a.CreateTime.Date && a.Memo == "Egg2");
                if (!toDayAdd)
                {
                    var egg2UserNumber = await _scoreInfoStore.GetScoreInfoDetaileds().Where(w => w.PeriodId == periodNow.Id && !w.IsDelete && w.Memo == "Egg2").Select(s => s.DealUserId).Distinct().CountAsync();
                    var userNumber = await _userStore.GetUserInfos().Where(w => !w.IsDelete).CountAsync();
                    var half = (egg2UserNumber > (userNumber / 2)) && ((userNumber / 2) > 10);//是否大于一半
                    var allocateSubmitRequest = new AllocateSubmitRequest
                    {
                        AllocateUserId = userId,
                        PeriodId = periodNow.Id,
                        Theme = !half ? "神秘彩蛋二" : "晚间签到",
                        Memo = "Egg2",
                        Score = 5,
                        Labels = _scoreInfoStore.GetScoreInfoDetaileds().Where(w => w.PeriodId == periodNow.Id && !w.IsDelete && w.Memo == "Egg2" && w.DealUserId == userId).Count() > 20 ? "持之以恒" : ""
                    };
                    await _allocateManager.AllocateSubmitAsync(new UserInfo { Id = "K先生" }, allocateSubmitRequest, default(CancellationToken), true);
                    //大于一半人的时候向所有人公布Egg2
                    if ((userNumber / 2) + 1 == egg2UserNumber)
                    {
                        //var allUserEmail = await _userStore.GetUserInfos().Where(w => !w.IsDelete && !string.IsNullOrEmpty(w.Email)).Select(s => s.Email).ToListAsync();
                        //_hellperEmail.SendEmail("积分系统紧急通知【神秘彩蛋二】",
                        //                        $"尊敬的勇士您好：",
                        //                        $"目前已有超过一半的人触发了《神秘彩蛋二》，为了公平，K先生现在向所有人公布《神秘彩蛋二》：为了感谢大家辛苦的工作，在每天晚上17点到次日零点之间重新登录积分系统可获得5K币，保持登录不算哦。--K先生",
                        //                        allUserEmail);
                    }
                }
            }
        }

        /// <summary>
        /// 禅道签到彩蛋
        /// </summary>
        /// <param name="zentaoLoginName"></param>
        /// <returns></returns>
        public async Task ZenTaoEggAsync(string zentaoLoginName)
        {
            //Egg3
            if (DateTime.Now.Hour <= 10 && DateTime.Now.Hour >= 6)
            {               
                var periodNow = await _scoreInfoStore.GetScorePeriods().FirstOrDefaultAsync(w => !w.IsDelete && w.State == 1);
                if (periodNow == null)
                {
                    return;
                }
                //根据禅道帐号找到对应用户
                var userInfo = await _userStore.GetUserInfos().FirstOrDefaultAsync(w => !w.IsDelete && w.ZenTao == zentaoLoginName);
                if (userInfo==null)
                { 
                    return; 
                }
                //今天是否加过
                var toDayAdd = await _scoreInfoStore.GetScoreInfoDetaileds().AnyAsync(a => a.PeriodId == periodNow.Id && !a.IsDelete && a.DealUserId == userInfo.Id && DateTime.Now.Date == a.CreateTime.Date && a.Memo == "Egg3");
                if (!toDayAdd)
                {                   
                    var allocateSubmitRequest = new AllocateSubmitRequest
                    {
                        AllocateUserId = userInfo.Id,
                        PeriodId = periodNow.Id,
                        Theme = "禅道早间签到",
                        Memo = "Egg3",
                        Score = DateTime.Now.Hour <= 8 ? 6 : 3,
                        Labels = ""
                    };
                    await _allocateManager.AllocateSubmitAsync(new UserInfo { Id = "K先生" }, allocateSubmitRequest, default(CancellationToken), true);                   
                }               
            }
            //Egg4
            if (DateTime.Now.Hour <= 23 && DateTime.Now.Hour >= 17)
            {               
                var periodNow = await _scoreInfoStore.GetScorePeriods().FirstOrDefaultAsync(w => !w.IsDelete && w.State == 1);
                if (periodNow == null)
                {
                    return;
                }
                //根据禅道帐号找到对应用户
                var userInfo = await _userStore.GetUserInfos().FirstOrDefaultAsync(w => !w.IsDelete && w.ZenTao == zentaoLoginName);
                if (userInfo == null)
                {
                    return;
                }
                //今天是否加过
                var toDayAdd = await _scoreInfoStore.GetScoreInfoDetaileds().AnyAsync(a => a.PeriodId == periodNow.Id && !a.IsDelete && a.DealUserId == userInfo.Id && DateTime.Now.Date == a.CreateTime.Date && a.Memo == "Egg4");
                if (!toDayAdd)
                {                    
                    var allocateSubmitRequest = new AllocateSubmitRequest
                    {
                        AllocateUserId = userInfo.Id,
                        PeriodId = periodNow.Id,
                        Theme = "禅道晚间签到",
                        Memo = "Egg4",
                        Score = DateTime.Now.Hour < 20 ? 3 : 6,
                        Labels = ""
                    };
                    await _allocateManager.AllocateSubmitAsync(new UserInfo { Id = "K先生" }, allocateSubmitRequest, default(CancellationToken), true);                   
                }               
            }
        }

        #endregion
    }
}
