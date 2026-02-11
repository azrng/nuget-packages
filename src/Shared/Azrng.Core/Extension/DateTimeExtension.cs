using System;
using System.Linq;

namespace Azrng.Core.Extension
{
    /// <summary>
    /// 时间扩展
    /// </summary>
    public static class DateTimeExtension
    {
        #region 格式化时间

        /// <summary>
        /// 自定义时间格式，默认返回返回格式：2019-01-21 20:57:51
        /// </summary>
        /// <param name="time"></param>
        /// <remarks>
        /// 为啥不将这个方法和和重载的方法合并，因为表达式中不能包含默认参数，又不想在指定时间格式
        /// </remarks>
        /// <returns></returns>
        public static string ToStandardString(this DateTime time)
        {
            return time.ToFormatString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 自定义时间格式，默认返回返回格式：2019-01-21 20:57:51
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string ToStandardString(this DateTime? time)
        {
            return !time.HasValue ? string.Empty : time.Value.ToStandardString();
        }

        /// <summary>
        /// 转详细时间字符串
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string ToDetailedTimeString(this DateTime time)
        {
            return time.ToFormatString("yyyy-MM-dd HH:mm:ss.fffffff");
        }

        /// <summary>
        /// 转详细时间字符串
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string ToDetailedTimeString(this DateTime? time)
        {
            return time.HasValue ? time.Value.ToDetailedTimeString() : string.Empty;
        }

        /// <summary>
        /// 转 yyyy-MM-dd
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string ToDateString(this DateTime? time)
        {
            return !time.HasValue ? string.Empty : time.Value.ToDateString();
        }

        /// <summary>
        /// 转 yyyy-MM-dd
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string ToDateString(this DateTime time)
        {
            return time.ToFormatString("yyyy-MM-dd");
        }

        /// <summary>
        /// 转  ISO 8601 标准时间字符串
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToIsoDateTimeString(this DateTime? dateTime)
        {
            return dateTime.HasValue
                ? dateTime.Value.ToIsoDateTimeString()
                : "";
        }

        /// <summary>
        /// 转  ISO 8601 标准时间字符串
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToIsoDateTimeString(this DateTime dateTime)
        {
            // O 相当于 yyyy-MM-ddTHH:mm:ss.fffffffK
            return dateTime.ToFormatString("O");
        }

        /// <summary>
        /// 自定义时间格式
        /// </summary>
        /// <param name="time"></param>
        /// <param name="format">要转换的格式</param>
        /// <returns></returns>
        public static string ToFormatString(this DateTime time, string? format)
        {
            return time.ToString(format ?? "yyyy-MM-dd HH:mm:ss");
        }

        #endregion

        /// <summary>
        /// 获取无时区的当前时间
        /// </summary>
        /// <returns>无时区时间</returns>
        public static DateTime ToNowDateTime(this DateTime _)
        {
            return DateTime.SpecifyKind(DateTime.UtcNow.AddHours(8), DateTimeKind.Unspecified);
        }

        /// <summary>
        /// 获取无时区时间
        /// </summary>
        /// <param name="dateTime">时间</param>
        /// <returns>无时区时间</returns>
        public static DateTime ToUnspecifiedDateTime(this DateTime dateTime)
        {
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
        }

        #region 时间戳

        /// <summary>
        /// 获取指定时间时间戳，默认毫秒级 改为使用ToTimestamp
        /// </summary>
        /// <param name="dateTime">时间</param>
        /// <param name="isSecond">是否是秒</param>
        /// <returns></returns>
        [Obsolete]
        public static long GetTimestamp(this DateTime dateTime, bool isSecond = false)
        {
            var dateTimeOffset = new DateTimeOffset(dateTime);
            return isSecond
                ? dateTimeOffset.ToUnixTimeSeconds()
                : dateTimeOffset.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// 获取指定时间时间戳，默认毫秒级
        /// </summary>
        /// <param name="dateTime">时间</param>
        /// <param name="isSecond">是否是秒</param>
        /// <returns></returns>
        public static long ToTimestamp(this DateTime dateTime, bool isSecond = false)
        {
            var dateTimeOffset = new DateTimeOffset(dateTime);
            return isSecond
                ? dateTimeOffset.ToUnixTimeSeconds()
                : dateTimeOffset.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// 时间戳转本地时间，默认是毫秒级
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <param name="isSecond">是否是秒</param>
        /// <returns></returns>
        public static DateTime ToDateTime(this long timestamp, bool isSecond = false)
        {
            return isSecond
                ? DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime
                : DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;
        }

        #endregion

        #region 时间段

        /// <summary>
        /// TimeSpan转DateTime
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns>从DateTime.MinValue开始加上TimeSpan的DateTime</returns>
        public static DateTime ToDateTime(this TimeSpan timeSpan)
        {
            return DateTime.MinValue.Add(timeSpan);
        }

        /// <summary>
        /// DateTime转TimeSpan
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static TimeSpan ToTimeSpan(this DateTime dateTime)
        {
            return dateTime - DateTime.MinValue;
        }

        #endregion

        /// <summary>
        /// 当前时间是否周末
        /// </summary>
        /// <param name="dateTime">时间点</param>
        /// <returns></returns>
        public static bool IsWeekend(this DateTime dateTime)
        {
            var weeks = new[]
                        {
                            (int)DayOfWeek.Saturday,
                            (int)DayOfWeek.Sunday
                        };
            return weeks.Contains((int)dateTime.DayOfWeek);
        }

        /// <summary>
        /// 获取日期是当月的第几周
        /// </summary>
        /// <param name="day">日期</param>
        /// <param name="mode">周次计算模式</param>
        /// <returns>周次（从1开始）</returns>
        /// <exception cref="ArgumentException">weekStart参数无效时抛出</exception>
        public static int GetWeekOfMonth(this DateTime day, int mode = 2)
        {
            // 一种是从月首日开始逐周计数，另一种是按完整周划分
            if (mode == 1)
            {
                // 模式1：从月首日开始逐周计数
                // 将每个月的第一天算作第1周的开始，然后每7天为一周
                return (day.Day - 1) / 7 + 1;
            }
            else
            {
                // 模式2：按完整周划分（以周日为一周的开始）
                // 规则：第1周从第一个周日开始，但月份第一天到第一个周日之间的日期也算第1周
                var firstOfMonth = new DateTime(day.Year, day.Month, 1);
                var firstDayOfWeek = (int)firstOfMonth.DayOfWeek; // 0=周日, 1=周一, ..., 6=周六

                // 以周日（0）为一周的开始
                var targetDayOfWeek = 0;

                // 计算月份第一天到第一个周开始日（周日）之间的天数
                var daysBeforeFirstWeek = (targetDayOfWeek - firstDayOfWeek + 7) % 7;

                // 计算周次
                // 从月份第一天开始算，每7天为一周
                // day.Day - 1: 从0开始计数（第1天是0）
                // + daysBeforeFirstWeek: 调整偏移量
                // / 7: 计算是第几个完整周
                // + 1: 周次从1开始
                var weekNumber = (day.Day - 1 + daysBeforeFirstWeek) / 7 + 1;
                return weekNumber;
            }
        }

        /// <summary>
        /// 获取日期对应的季度
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static int GetQuarter(this DateTime dateTime)
        {
            if (dateTime.Month % 3 > 0)
                return dateTime.Month / 3 + 1;
            return dateTime.Month / 3;
        }

        /// <summary>
        /// 当前时间是否为工作日
        /// </summary>
        /// <param name="dateTime">时间点</param>
        /// <returns></returns>
        public static bool IsWeekday(this DateTime dateTime)
        {
            return !dateTime.IsWeekend();
        }

        /// <summary>
        /// 获取当前月份有多少天
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static int GetCurrentMonthDayNumber(this DateTime dateTime)
        {
            return DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
        }

        /// <summary>
        /// 计算两个日期天数(未满24小时算1天  超过24小时才算一天)
        /// </summary>
        /// <param name="dateStart">开始时间</param>
        /// <param name="dateEnd">结束时间</param>
        /// <returns></returns>
        public static int DateDiff(this DateTime? dateStart, DateTime? dateEnd)
        {
            if (!dateStart.HasValue || !dateEnd.HasValue)
            {
                return 0;
            }

            return dateStart.DateDiff(dateEnd.Value);
        }

        /// <summary>
        /// 计算两个日期天数(未满24小时算1天  超过24小时才算一天)
        /// </summary>
        /// <param name="dateStart">开始时间</param>
        /// <param name="dateEnd">结束时间</param>
        /// <returns></returns>
        public static int DateDiff(this DateTime? dateStart, DateTime dateEnd)
        {
            if (!dateStart.HasValue)
            {
                return 0;
            }

            return dateStart.DateDiff(dateEnd);
        }

        /// <summary>
        /// 计算两个日期天数(未满24小时算1天  超过24小时才算一天)
        /// </summary>
        /// <param name="dateStart">开始时间</param>
        /// <param name="dateEnd">结束时间</param>
        /// <returns></returns>
        public static int DateDiff(this DateTime dateStart, DateTime? dateEnd)
        {
            if (!dateEnd.HasValue)
            {
                return 0;
            }

            return dateStart.DateDiff(dateEnd.Value);
        }

        /// <summary>
        /// 计算两个日期天数(未满24小时算1天  超过24小时才算一天)
        /// </summary>
        /// <param name="dateStart">开始时间</param>
        /// <param name="dateEnd">结束时间</param>
        /// <returns></returns>
        public static int DateDiff(this DateTime dateStart, DateTime dateEnd)
        {
            // 计算时间差
            var difference = dateEnd - dateStart;

            // 返回天数差（绝对值）
            return (int)Math.Ceiling(Math.Abs(difference.TotalDays));
        }
    }
}