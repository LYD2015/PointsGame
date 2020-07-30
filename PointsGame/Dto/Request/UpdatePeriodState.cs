using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Dto.Request
{
    public class UpdatePeriodState
    {
        /// <summary>
        /// 赛季ID
        /// </summary>
        public string PeriodId { get; set; }
        /// <summary>
        /// 赛季状态。1为开始。2为结束
        /// </summary>
        public int State { get; set; }
    }
}
