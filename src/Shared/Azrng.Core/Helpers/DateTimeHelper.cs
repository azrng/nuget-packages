using Azrng.Core.Enums;
using Azrng.Core.Extension;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Azrng.Core.Helpers
{
    public class DateTimeHelper
    {
        /// <summary>
        /// 获取当前网络时间
        /// </summary>
        /// <returns></returns>
        public static async Task<DateTime?> GetNetworkTime()
        {
            try
            {
                const string serviceUrl = "https://cn.apihz.cn/api/time/getapi.php?id=88888888&key=88888888&type=1";
                var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = delegate { return true; } };

                var client = new HttpClient(handler);

                var response = await client.GetStringAsync(serviceUrl);

                var time = response.Substring(19, 10).ToInt64().ToDateTime(true);

                return time;
            }
            catch
            {
                return null;
            }
        }

        #region 获取指定时间

        /// <summary>
        /// 获取开始时间
        /// </summary>
        /// <param name="now"></param>
        /// <param name="timeType"></param>
        /// <returns></returns>
        public static DateTime GetTargetTimeStart(DateTime now, TimeType timeType = TimeType.Today)
        {
            switch (timeType)
            {
                case TimeType.Week:
                    return now.AddDays(0 - (int)now.DayOfWeek + 1).Date;

                case TimeType.CurrentMonth:
                    return now.AddDays(-now.Day + 1).Date;

                case TimeType.Season:
                    {
                        var dateTime = now.AddMonths(-((now.Month - 1) % 3));
                        return dateTime.AddDays(-dateTime.Day + 1).Date;
                    }
                case TimeType.Year:
                    return now.AddDays(-now.DayOfYear + 1).Date;
                case TimeType.Yesterday:
                    return now.Date.AddDays(-1);
                case TimeType.Today:
                    return now.Date;
                case TimeType.Tomorrow:
                    return now.Date.AddDays(1);
                case TimeType.NextMonth:
                    return now.AddDays(-now.Day + 1).AddMonths(1).Date;
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeType), timeType, null);
            }
        }

        /// <summary>
        /// 获取结束时间
        /// </summary>
        /// <param name="now"></param>
        /// <param name="timeType"></param>
        /// <returns></returns>
        public static DateTime GetTargetTimeEnd(DateTime now, TimeType timeType = TimeType.Today)
        {
            switch (timeType)
            {
                case TimeType.Week:
                    return now.Date.AddDays((double)(7 - now.DayOfWeek)).AddDays(1).AddSeconds(-1);

                case TimeType.CurrentMonth:
                    return now.Date.AddMonths(1).AddDays(-now.AddMonths(1).Day + 1).AddSeconds(-1);

                case TimeType.Season:
                    {
                        var dateTime2 = now.AddMonths(3 - (now.Month - 1) % 3 - 1);
                        return dateTime2.Date.AddMonths(1).AddDays(-dateTime2.AddMonths(1).Day + 1).AddSeconds(-1);
                    }
                case TimeType.Year:
                    {
                        var dateTime = now.AddYears(1);
                        return dateTime.Date.AddDays(-dateTime.DayOfYear).AddDays(1).AddSeconds(-1);
                    }
                case TimeType.Yesterday:
                    return now.Date.AddSeconds(-1);
                case TimeType.Today:
                    return now.Date.AddDays(1).AddSeconds(-1);
                case TimeType.Tomorrow:
                    return now.Date.AddDays(2).AddSeconds(-1);
                case TimeType.NextMonth:
                    return now.Date.AddDays(-now.Day + 1).AddMonths(2).AddSeconds(-1);
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeType), timeType, null);
            }
        }

        #endregion

        /// <summary>
        /// 获取时间差字符串(最大单位为月)
        /// </summary>
        /// <param name="startDateTime"></param>
        /// <param name="endDataTime"></param>
        /// <returns></returns>
        public static string GetDateIntervalStr(DateTime startDateTime, DateTime endDataTime)
        {
            string dateDiff = null;
            try
            {
                var ts = endDataTime - startDateTime;
                if (ts.Days >= 1)
                {
                    dateDiff = startDateTime.Month + "月" + startDateTime.Day + "日";
                }
                else
                {
                    if (ts.Hours > 1)
                    {
                        dateDiff = ts.Hours + "小时前";
                    }
                    else
                    {
                        dateDiff = ts.Minutes + "分钟前";
                    }
                }
            }
            catch { }

            return dateDiff;
        }

        /// <summary>
        /// 获取日期间隔
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public static (int days, int hours, int minutes, int seconds) GetDateInterval(DateTime startTime, DateTime endTime)
        {
            var ts1 = new TimeSpan(startTime.Ticks);
            var ts2 = new TimeSpan(endTime.Ticks);

            var ts = ts1.Subtract(ts2).Duration();
            return (ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
        }

        /// <summary>
        /// 计算相差的天数（超出的时间算一天）
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static int CalculateDaysDifference(DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null || endDate == null)
                return 0;

            // 计算时间差
            var difference = endDate.Value - startDate.Value;

            // 返回天数差（绝对值）
            return (int)Math.Ceiling(Math.Abs(difference.TotalDays));
        }

        /// <summary>
        /// 查询指定的时间距离现在的时间差
        /// </summary>
        /// <param name="getTime">指定的时间</param>
        /// <returns></returns>
        public string GetTimeDifferenceText(DateTime getTime)
        {
            var time = Convert.ToDateTime(getTime);
            var timeDifference = DateTime.Now - time;
            if (timeDifference.Days >= 365)
            {
                //超过一年=》那一年哪个月
                return time.Year + "年" + time.Month.ToString().PadLeft(2, '0') + "月";
            }

            if (timeDifference.Days >= 30)
            {
                //大于30小于一年=》多少月前
                return Math.Floor(Convert.ToDouble(timeDifference.Days / 30)) + "月前";
            }

            if (timeDifference.Days >= 1)
            {
                //一月之内=》是多少天前
                return timeDifference.Days + "天前";
            }

            if (timeDifference.Hours >= 1)
            {
                //一天之内=》多少小时前
                return timeDifference.Hours + "小时前";
            }

            if (timeDifference.Minutes >= 1)
            {
                //一小时之内=》多少分钟前
                return timeDifference.Minutes + "分钟前";
            }

            // 一分钟之内=》刚刚
            return "刚刚";
        }
    }
}