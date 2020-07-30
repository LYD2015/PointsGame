using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Response
{
    /// <summary>
    /// 动态查询返回体
    /// </summary>
    public class SearchDynamicResponse
    {
        /// <summary>
        /// 关键值ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 赛季ID
        /// </summary>
        public string PeriodId { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 用户全称(如：刘亚东-后端组)
        /// </summary>
        public string UserFullName { get; set; }        
        /// <summary>
        /// 对应数据ID
        /// </summary>
        public string DataId { get; set; }
        /// <summary>
        /// 动态类型(交易支出 1,交易收入2,任务收入3,派发收入4,任务发布5,任务抢到6,任务完成7)
        /// </summary>
        public int DynamicType { get; set; }
        /// <summary>
        /// 动态内容
        /// </summary>
        public string DynamicContent { get; set; }
        /// <summary>
        /// 动态形容词
        /// </summary>
        public string Adjective { get; set; }
        /// <summary>
        /// 积分
        /// </summary>
        public int Score { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
