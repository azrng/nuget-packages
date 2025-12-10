using System;
using System.Globalization;

namespace Azrng.Core.Extension
{
    /// <summary>
    /// Double扩展
    /// </summary>
    public static class DoubleExtension
    {
        /// <summary>
        /// 自定义格式，默认返回返回格式：1.01
        /// </summary>
        /// <param name="dec">decimal值</param>
        /// <param name="number">保留的小数位数</param>
        /// <returns></returns>
        public static string ToStandardString(this double? dec, int number = 2)
        {
            if (!dec.HasValue)
                return "0";

            return dec.Value.ToStandardString(number);
        }

        /// <summary>
        /// 自定义格式，默认返回返回格式：1.01
        /// </summary>
        /// <param name="dec">decimal值</param>
        /// <param name="number">保留的小数位数</param>
        /// <returns></returns>
        public static string ToStandardString(this double dec, int number = 2)
        {
            // Math.Round：四舍六入五考虑，五后非零就进一，五后皆零看奇偶，五前为偶应舍去，五前为奇要进一
            number = number < 0 ? 2 : number;
            return Math.Round(dec, number).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 自定义格式，不保留小数点后的0
        /// </summary>
        /// <param name="dec">decimal值</param>
        /// <param name="number">保留的小数位数</param>
        /// <returns></returns>
        public static string ToNoZeroString(this double? dec, int number = 2)
        {
            if (!dec.HasValue)
                return "0";

            return dec.Value.ToNoZeroString(number);
        }

        /// <summary>
        /// 自定义格式，不保留小数点后的0
        /// </summary>
        /// <param name="dec">decimal值</param>
        /// <param name="number">保留的小数位数</param>
        /// <returns></returns>
        public static string ToNoZeroString(this double dec, int number = 2)
        {
            return dec.ToString("0.".PadRight(number + 2, '#'));
        }

        /// <summary>
        /// 取绝对值
        /// </summary>
        /// <param name="decimal"></param>
        /// <returns></returns>
        public static double ToAbs(this double @decimal)
        {
            return Math.Abs(@decimal);
        }

        /// <summary>
        /// 保留几位小数（保留结尾0）
        /// </summary>
        public static string ToFixedString(this double? dec, int number = 2)
        {
            return !dec.HasValue ? "0" : dec.Value.ToFixedString(number);
        }

        /// <summary>
        /// 保留几位小数（保留结尾0）
        /// </summary>
        public static string ToFixedString(this double dec, int number = 2)
        {
            number = number < 0 ? 2 : number;
            return dec.ToString($"F{number}", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 转百分比字符串（保留指定小数位数）
        /// </summary>
        public static string ToPercentString(this double? dec, int number = 2)
        {
            return !dec.HasValue ? "0%" : dec.Value.ToPercentString(number);
        }

        /// <summary>
        /// 转百分比字符串（保留指定小数位数）
        /// </summary>
        public static string ToPercentString(this double dec, int number = 2)
        {
            number = number < 0 ? 2 : number;
            return (Math.Round(dec * 100, number)).ToString($"F{number}", CultureInfo.InvariantCulture) + "%";
        }

        /// <summary>
        /// 强制截取到指定小数位数（不四舍五入）
        /// </summary>
        public static string ToTruncateString(this double? dec, int number = 2)
        {
            return !dec.HasValue ? "0" : dec.Value.ToTruncateString(number);
        }

        /// <summary>
        /// 强制截取到指定小数位数（不四舍五入）
        /// </summary>
        public static string ToTruncateString(this double dec, int number = 2)
        {
            number = number < 0 ? 2 : number;
            var factor = Math.Pow(10, number);
            var truncated = Math.Truncate(dec * factor) / factor;
            return truncated.ToString($"F{number}", CultureInfo.InvariantCulture);
        }
    }
}