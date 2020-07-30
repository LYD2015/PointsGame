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

namespace PointsGame.Managers
{
    /// <summary>
    /// 称号
    /// </summary>
    public class RankTitleManager
    {
        private readonly IRankTitleStore _iTitleStore;
        private readonly IConfigurationRoot _config;
        private HelperSendClientMessage _sendClientMessageHelper;
        public RankTitleManager(IRankTitleStore titleStore, IConfigurationRoot configuration, HelperSendClientMessage sendClientMessageManager)
        {
            _iTitleStore = titleStore;
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _sendClientMessageHelper = sendClientMessageManager;
        }
        /// <summary>
        /// 查询称号列表
        /// </summary>
        /// <param name="user"></param>
        /// <param name="periodId"></param>
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<List<SearchTitleResponse>>> SearchTitleAsync(UserInfo user, string periodId, CancellationToken requestAborted)
        {
            var response = new ResponseMessage<List<SearchTitleResponse>>();
            if (!user.IsAdmin)
            {
                response.Code = ResponseCodeDefines.NotAllow;
                response.Message = "权限不足";
                return response;
            }

            response.Extension = await _iTitleStore.GetScoreTitles().Where(w => w.PeriodId == periodId && !w.IsDelete).Select(s => new SearchTitleResponse
            {
                Id = s.Id,
                PeriodId = s.PeriodId,
                Title = s.Title,
                StartScore = s.StartScore,
                EndScore = s.EndScore,
                Icon = _config["FileUrl"]+s.Icon,
                Card = _config["FileUrl"]+s.Card,
                CreateTime = s.CreateTime,
                CreateUser = s.CreateUser,
                UpdateTime = s.UpdateTime,
                UpdateUser = s.UpdateUser,
                FontColor = s.FontColor,
            }).OrderBy(o => o.StartScore).ToListAsync();

            return response;

        }

        /// <summary>
        /// 添加称号
        /// </summary>
        /// <param name="user"></param>
        /// <param name="titleRequest"></param>
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<ResponseMessage> AddTitleAsync(UserInfo user, TitleAddRequest titleRequest, CancellationToken requestAborted)
        {
            var response = new ResponseMessage();
            if (!user.IsAdmin)
            {
                response.Code = ResponseCodeDefines.NotAllow;
                response.Message = "权限不足";
                return response;
            }
            if (titleRequest.StartScore >= titleRequest.EndScore)
            {
                response.Code = ResponseCodeDefines.NotFound;
                response.Message = "开始值不能大于或等于结束值";
                return response;
            }

            var periodInfo = await _iTitleStore.GetScorePeriods().Where(w => !w.IsDelete && w.Id == titleRequest.PeriodId).FirstOrDefaultAsync();
            if (periodInfo == null || periodInfo.State != 1)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "只有进行中的赛季才能操作";
                return response;
            }
            var titles = await _iTitleStore.GetScoreTitles().Where(w => w.PeriodId == titleRequest.PeriodId && !w.IsDelete).ToListAsync();
            if (titles != null && titles.Count != 0)
            {
                if (titleRequest.EndScore <= titles.Max(w => w.EndScore) && (titleRequest.EndScore + 1) != titles.Min(w => w.StartScore))
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "开始值和结束值不能重复，且必须保持连续";
                    return response;
                }
                if (titleRequest.StartScore >= titles.Min(w => w.StartScore) && (titleRequest.StartScore - 1) != titles.Max(w => w.EndScore))
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "开始值和结束值不能重复，且必须保持连续";
                    return response;
                }
            }
            //需要保存的数据
            var title = new RankTitle
            {
                Id = Guid.NewGuid().ToString(),
                PeriodId = titleRequest.PeriodId,
                Title = titleRequest.Title,
                StartScore = titleRequest.StartScore,
                EndScore = titleRequest.EndScore,
                Icon = titleRequest.Icon?.Replace(_config["FileUrl"], ""),
                Card = titleRequest.Card?.Replace(_config["FileUrl"], ""),
                CreateTime = DateTime.Now,
                CreateUser = user.Id,
                UpdateTime = DateTime.Now,
                UpdateUser = user.Id,
                IsDelete = false,
                FontColor = titleRequest.FontColor,
            };
            //保存
            await _iTitleStore.AddTitleAsync(title, requestAborted);

            // 触发赛季和排行榜
            await _sendClientMessageHelper.SendInfos(new List<Dto.Common.SendClientType>() { Dto.Common.SendClientType.Season, Dto.Common.SendClientType.Rank });
            return response;
        }

        /// <summary>
        /// 修改称号
        /// </summary>
        /// <param name="user"></param>
        /// <param name="titleUpdateRequest"></param>
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<ResponseMessage> UpdateTitleAsync(UserInfo user, TitleUpdateRequest titleUpdateRequest, CancellationToken requestAborted)
        {
            var response = new ResponseMessage();
            if (!user.IsAdmin)
            {
                response.Code = ResponseCodeDefines.NotAllow;
                response.Message = "权限不足";
                return response;
            }
            //验证开始值和结束值的连续性
            var temp = titleUpdateRequest.titleUpdateListRequests.OrderBy(o => o.StartScore).ToList();
            int? endScore = null;
            foreach (var title in temp)
            {
                if (endScore != null)
                {
                    if (endScore + 1 != title.StartScore)
                    {
                        response.Code = ResponseCodeDefines.ModelStateInvalid;
                        response.Message = "开始值和结束值必须保持连续";
                        return response;
                    }
                }

                if (title.StartScore >= title.EndScore)
                {
                    response.Code = ResponseCodeDefines.ModelStateInvalid;
                    response.Message = "开始值和结束值必须保持连续";
                    return response;
                }

                endScore = title.EndScore;
            }

            var periodInfo = await _iTitleStore.GetScorePeriods().Where(w => !w.IsDelete && w.Id == titleUpdateRequest.PeriodId).FirstOrDefaultAsync();
            if (periodInfo == null || periodInfo.State != 1)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "只有进行中的赛季才能操作";
                return response;
            }
            var titles = await _iTitleStore.GetScoreTitles().Where(w => w.PeriodId == titleUpdateRequest.PeriodId && !w.IsDelete).ToListAsync();
            if (titles == null || titles.Count == 0 || titles.Count != titleUpdateRequest.titleUpdateListRequests.Count)
            {
                response.Code = ResponseCodeDefines.NotFound;
                response.Message = "未找到称号数据";
                return response;
            }
            foreach (var title in titles)
            {
                if (titleUpdateRequest.titleUpdateListRequests.FirstOrDefault(fi => fi.Id == title.Id) == null)
                {
                    response.Code = ResponseCodeDefines.NotFound;
                    response.Message = "需要保存的称号数据与赛季ID不对应";
                    return response;
                }
            }
            //更新值
            var titleListRequests = titleUpdateRequest.titleUpdateListRequests;
            titles.ForEach(f =>
            {
                f.Title = titleListRequests.FirstOrDefault(fi => fi.Id == f.Id).Title;
                f.StartScore = titleListRequests.FirstOrDefault(fi => fi.Id == f.Id).StartScore;
                f.EndScore = titleListRequests.FirstOrDefault(fi => fi.Id == f.Id).EndScore;
                f.Icon = titleListRequests.FirstOrDefault(fi => fi.Id == f.Id)?.Icon?.Replace(_config["FileUrl"], "");
                f.Card = titleListRequests.FirstOrDefault(fi => fi.Id == f.Id)?.Card?.Replace(_config["FileUrl"], "");
                f.UpdateTime = DateTime.Now;
                f.UpdateUser = user.Id;
                f.FontColor= titleListRequests.FirstOrDefault(fi => fi.Id == f.Id).FontColor;
            });
            //保存更新
            await _iTitleStore.UpdateTitleListAsync(titles, requestAborted);
            // 触发赛季和排行榜
            await _sendClientMessageHelper.SendInfos(new List<Dto.Common.SendClientType>() { Dto.Common.SendClientType.Season, Dto.Common.SendClientType.Rank });

            return response;
        }

        /// <summary>
        /// 删除称号
        /// </summary>
        /// <param name="user"></param>
        /// <param name="titleId"></param>
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<ResponseMessage> DeleteTitleAsync(UserInfo user, string titleId, CancellationToken requestAborted)
        {
            var response = new ResponseMessage();
            if (!user.IsAdmin)
            {
                response.Code = ResponseCodeDefines.NotAllow;
                response.Message = "权限不足";
                return response;
            }
            var titleInfo = await _iTitleStore.GetScoreTitles().Where(w => !w.IsDelete && w.Id == titleId).FirstOrDefaultAsync();
            if (titleInfo == null)
            {
                response.Code = ResponseCodeDefines.NotFound;
                response.Message = "未找到称号数据";
                return response;
            }
            var periodInfo = await _iTitleStore.GetScorePeriods().Where(w => !w.IsDelete && w.Id == titleInfo.PeriodId).FirstOrDefaultAsync();
            if (periodInfo == null || periodInfo.State != 1)
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "只有进行中的赛季才能操作";
                return response;
            }
            //只能从最小或者最大称号开始删除
            var minScore = await _iTitleStore.GetScoreTitles().Where(w => !w.IsDelete).MinAsync(m => m.StartScore);
            var maxScore = await _iTitleStore.GetScoreTitles().Where(w => !w.IsDelete).MaxAsync(m => m.EndScore);
            if (!(titleInfo.StartScore == minScore || titleInfo.EndScore == maxScore))
            {
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = "只能从最大或者最小称号开始删除";
                return response;
            }

            titleInfo.IsDelete = true;
            titleInfo.UpdateTime = DateTime.Now;
            titleInfo.UpdateUser = user.Id;
            //保存
            await _iTitleStore.UpdateTitleListAsync(new List<RankTitle> { titleInfo }, requestAborted);

            // 触发赛季和排行榜
            await _sendClientMessageHelper.SendInfos(new List<Dto.Common.SendClientType>() { Dto.Common.SendClientType.Season, Dto.Common.SendClientType.Rank });
            return response;
        }
    }
}
