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
    public class SearchManager
    {
        private readonly ISearchStore _iSearchStore;
        private readonly Helper.HelperDynamic _dynamicHelper;
        private readonly IConfigurationRoot _config;

        public SearchManager(ISearchStore  searchStore,Helper.HelperDynamic dynamicHelper, IConfigurationRoot configuration)
        {
            _iSearchStore = searchStore;
            _dynamicHelper = dynamicHelper;
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// 查询排行榜
        /// </summary>
        /// <param name="user"></param>
        /// <param name="searchTopRequest"></param>
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<PagingResponseMessage<SearchTopResponse>> SearchTopAsync(SearchTopRequest searchTopRequest, CancellationToken requestAborted)
        {
            var response = new PagingResponseMessage<SearchTopResponse>();
            //赛季下的所有人员积分信息
            var query = _iSearchStore.GetSearchTop(searchTopRequest.PeriodId);
            response.TotalCount = await query.CountAsync();
            query = query.Skip((searchTopRequest.PageIndex) * searchTopRequest.PageSize).Take(searchTopRequest.PageSize);
            response.PageIndex = searchTopRequest.PageIndex;
            response.PageSize = searchTopRequest.PageSize;
            var topList =await query.ToListAsync();

            //称号信息
            var titleList = await _iSearchStore.GetScoreTitles().Where(w => !w.IsDelete && w.PeriodId == searchTopRequest.PeriodId).ToListAsync();
            //所有人员的印象标签及印象标签次数
            var labelList = await _iSearchStore.GetUserLabels().GroupBy(g =>new { g.UserId,g.Label }).Select(s=> new
            { 
                UserId = s.Key.UserId, 
                Label = s.Key.Label,
                LabelCount=s.Count()
            }).ToListAsync();

            //循环组合对应人员的名次、称号名称、称号图标、剩余升级积分、印象标签列表
            int number= 1;//排名
            topList.ForEach(f =>
            {
                f.TopNumber = number++;
                f.Score = f.Score ?? 0;
                f.ConsumableScore = f.ConsumableScore ?? 0;
                f.ScoreTitle = (titleList.FirstOrDefault(fi => fi.StartScore <= f.Score && fi.EndScore >= f.Score)?.Title) ?? "未知称号";
                f.Icon = _config["FileUrl"] + titleList.FirstOrDefault(fi => fi.StartScore <= f.Score && fi.EndScore >= f.Score)?.Icon;
                f.Card = _config["FileUrl"] + titleList.FirstOrDefault(fi => fi.StartScore <= f.Score && fi.EndScore >= f.Score)?.Card;
                f.NextScore = titleList.FirstOrDefault(fi => fi.StartScore <= f.Score && fi.EndScore >= f.Score)?.EndScore == null ?
                              0 :
                              titleList.FirstOrDefault(fi => fi.StartScore <= f.Score && fi.EndScore >= f.Score).EndScore - f.Score + 1;
                f.LabelList = labelList.Where(w => w.UserId == f.UserId).Select(s => new LabelList { Label = s.Label, LabelCount = s.LabelCount }).OrderByDescending(o => o.LabelCount).ToList();
                f.FontColor= titleList.FirstOrDefault(fi => fi.StartScore <= f.Score && fi.EndScore >= f.Score)?.FontColor;
            });

            response.Extension = topList;
            return response;
        }

        /// <summary>
        /// 获取用户等级信息
        /// </summary>
        /// <param name="userId"></param>        
        /// <param name="periodId"></param>
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<SearchTopResponse>> SearchUserTitleAsync(string userId, string periodId,CancellationToken requestAborted)
        {
            var response = new ResponseMessage<SearchTopResponse>();
            var request = new SearchTopRequest 
            {
                PeriodId = periodId,
                PageIndex = 0,
                PageSize = 999999999
            };

            //直接通过排名里面取得自己的数据,目前使用人员很少，无所谓。
            var top = await SearchTopAsync(request, requestAborted);
            var userTitleInfo = top.Extension.Where(w => w.UserId == userId).FirstOrDefault();

            response.Extension = userTitleInfo;
            return response;
        }

        /// <summary>
        /// 获取动态列表
        /// </summary>
        /// <param name="periodId">赛季ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="pageIndex">分页下标(0开始)</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="requestAborted"></param>
        /// <returns></returns>
        public async Task<PagingResponseMessage<SearchDynamicResponse>> SearchDynamicAsync(string periodId, string userId, int pageIndex, int pageSize, CancellationToken requestAborted)
        {
            var response = new PagingResponseMessage<SearchDynamicResponse>();
            //每个人购买彩票的动态和中奖的信息不显示出来
            var query = _iSearchStore.GetDynamicInfos().Where(w => w.PeriodId == periodId && w.DynamicType != (int)DynamicType.WinLottery && w.DynamicType != (int)DynamicType.BuyLottery);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                query = query.Where(w => w.UserId == userId);
            }
            query = query.OrderByDescending(o => o.CreateTime);
            response.TotalCount = await query.CountAsync();
            response.PageIndex = pageIndex;
            response.PageSize = pageSize;
            response.Extension = await query.Skip(pageIndex * pageSize).Take(pageSize).Select(s => new SearchDynamicResponse
            {
                Id=s.Id,
                DataId=s.DataId,
                PeriodId=s.PeriodId,
                UserId = s.UserId,
                UserFullName=s.UserFullName,
                Score = s.Score,
                DynamicType =s.DynamicType,
                Adjective=s.Adjective,
                DynamicContent=s.DynamicContent,               
                CreateTime=s.CreateTime
            }).ToListAsync();

            return response;
            
        }
        
        /// <summary>
        /// 获取常用印象标签
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<List<string>>> SearchLabel(UserInfo user)
        {
            var response = new ResponseMessage<List<string>>();
            response.Extension = await _iSearchStore.GetUserLabels()
                                                    .GroupBy(g => g.Label)
                                                    .Select(s => new { Label = s.Key, Count = s.Count() })
                                                    .OrderByDescending(o => o.Count)                                                    
                                                    .Select(s => s.Label)
                                                    .Take(20)
                                                    .ToListAsync();
            return response;
        }    
    }
}
