using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Azrng.Core.Helpers
{
    public class StringHelper
    {
        /// <summary>
        /// 压缩文本
        /// </summary>
        /// <param name="originStr"></param>
        /// <returns></returns>
        public static string CompressText(string originStr)
        {
            // 移除换行符和回车符
            var compressed = originStr.Replace("\r", "").Replace("\n", "");

            // 移除多余的空格
            compressed = Regex.Replace(compressed, @"\s+", " ");

            return compressed.Trim(); // 去除首尾空格
        }

        /// <summary>
        /// 将文本转换为Unicode编码字符串
        /// </summary>
        /// <param name="text">要转换的文本</param>
        /// <returns>Unicode编码字符串</returns>
        public static string TextToUnicode(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var unicode = new StringBuilder();
            foreach (var c in text)
            {
                // 将每个字符转换为Unicode编码形式（如\u0041）
                unicode.Append("\\u").Append(((int)c).ToString("x4"));
            }

            return unicode.ToString();
        }

        /// <summary>
        /// 将Unicode编码字符串还原为文本
        /// </summary>
        /// <param name="unicode">Unicode编码字符串</param>
        /// <returns>还原后的文本</returns>
        public static string UnicodeToText(string unicode)
        {
            if (string.IsNullOrEmpty(unicode))
            {
                return string.Empty;
            }

            return new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(unicode,
                x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)));
        }
    }
}