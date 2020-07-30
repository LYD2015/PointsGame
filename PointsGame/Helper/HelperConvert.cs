namespace PointsGame.Helper
{
    /// <summary>
    /// 状态装换
    /// </summary>
    public static class HelperConvert
    {
        /// <summary>
        /// 转换任务状态到中文字符
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string TaskStateToString(int state)
        {
            switch (state)
            {
                case 0: return "未开抢";
                case 1: return "未领完";
                case 2: return "进行中";
                case 3: return "已完结";
                default:
                    return "未知状态";
            }
        }
        /// <summary>
        /// 转换任务审核状态到中文字符
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string TaskExamineStateToString(int state)
        {
            switch (state)
            {
                case 1: return "待审核";
                case 2: return "通过";
                case 3: return "驳回";
                default:
                    return "未知状态";
            }
        }
        /// <summary>
        /// 转换赛季状态到中文字符
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string PeriodStateToString(int state)
        {
            switch (state)
            {
                case 0: return "未开始";
                case 1: return "进行中";
                case 2: return "已结束";
                default:
                    return "未知状态";
            }
        }
    }
}
