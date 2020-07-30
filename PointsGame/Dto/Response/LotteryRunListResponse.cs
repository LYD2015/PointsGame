using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Response
{
    /// <summary>
    /// 查询开奖列表返回体
    /// </summary>
    public class LotteryRunListResponse
    {
        /// <summary>
        /// 是否开奖(false-未开奖,true-已开奖)
        /// </summary>
        public bool IsRunlottery { get; set; }
        /// <summary>
        /// 期数
        /// </summary>
        public string NumberPeriods { get; set; }
        /// <summary>
        /// 开奖号码
        /// </summary>
        public string Number { get; set; }      
        /// <summary>
        /// 创建时间(开奖时间)
        /// </summary>
        public string CreateTime { get; set; }
        /// <summary>
        /// 开奖结果统计(是一个Json字符串，可以把里面的 中出总注数:all.count 和 中出总分数:all.outScore 显示到界面上)
        /// </summary>
        public string WinResult { get; set; }
        
    }
}
