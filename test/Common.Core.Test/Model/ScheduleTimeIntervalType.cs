using System.ComponentModel;

namespace Common.Core.Test.Model
{
    /// <summary>
    /// 排班时间段
    /// </summary>
    public enum ScheduleTimeIntervalType
    {
        /// <summary>
        /// 全天
        /// </summary>
        [Description("全天")]
        [EnglishDescription("All Day")]
        AllDay,

        /// <summary>
        /// 早上
        /// </summary>
        [Description("早上")]
        [EnglishDescription("Morning")]
        Morning,

        /// <summary>
        /// 下午
        /// </summary>
        [Description("下午")]
        [EnglishDescription("Afternoon")]
        Afternoon,
    }
}