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
            return time.ToFormatString("yyyy-MM-dd HH:mm:ss.fff");
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
        /// 自定义时间格式
        /// </summary>
        /// <param name="time"></param>
        /// <param name="format">要转换的格式</param>
        /// <returns></returns>
        public static string ToFormatString(this DateTime time, string format)
        {
            return time.ToString(format);
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
        /// <returns></returns>
        public static DateTime ToDateTime(this TimeSpan timeSpan)
        {
            return new DateTime(timeSpan.Ticks);
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
            var weeks = new int[]
                        {
                            (int)DayOfWeek.Saturday,
                            (int)DayOfWeek.Sunday
                        };
            return weeks.Contains((int)dateTime.DayOfWeek);
        }

        /// <summary>
        /// 获取日期是当月的第几周
        /// </summary>
        /// <param name="day"></param>
        /// <param name="weekStart">1表示 周一至周日 为一周 2表示 周日至周六 为一周</param>
        /// <returns></returns>
        public static int GetWeekOfMonth(this DateTime day, int weekStart = 1)
        {
            //WeekStart
            //1表示 周一至周日 为一周

            //2表示 周日至周六 为一周

            var firstOfMonth = Convert.ToDateTime(day.Date.Year + "-" + day.Date.Month + "-" + 1);

            var i = (int)firstOfMonth.Date.DayOfWeek;
            if (i == 0)
            {
                i = 7;
            }

            return weekStart switch
            {
                1 => (day.Date.Day + i - 2) / 7 + 1,
                2 => (day.Date.Day + i - 1) / 7,
                _ => throw new ArgumentException("无效的值")
            };
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
    }
}