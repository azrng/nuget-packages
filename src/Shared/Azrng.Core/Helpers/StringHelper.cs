using System;
using System.Collections.Generic;
using System.Linq;
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
            if (string.IsNullOrEmpty(originStr))
                return originStr ?? string.Empty;

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

        /// <summary>
        /// 普通批量替换
        /// </summary>
        /// <param name="input"></param>
        /// <param name="replacements"></param>
        /// <returns></returns>
        public static string ReplaceWithDictionary(string input, Dictionary<string, string> replacements)
        {
            if (string.IsNullOrEmpty(input) || replacements == null || replacements.Count == 0)
                return input;

            var sb = new StringBuilder(input);

            foreach (var pair in replacements)
            {
                if (!string.IsNullOrEmpty(pair.Key))
                {
                    sb.Replace(pair.Key, pair.Value);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 按照替换字典key长度排序进行替换
        /// </summary>
        /// <param name="input"></param>
        /// <param name="replacements"></param>
        /// <returns></returns>
        public static string ReplaceWithOrder(string input, Dictionary<string, string> replacements)
        {
            if (string.IsNullOrEmpty(input) || replacements == null || replacements.Count == 0)
                return input;

            // 按键长度降序排序，避免短字符串替换影响长字符串
            var orderedReplacements = replacements
                                      .Where(x => !string.IsNullOrEmpty(x.Key))
                                      .OrderByDescending(x => x.Key.Length)
                                      .ToList();

            var sb = new StringBuilder(input);

            foreach (var pair in orderedReplacements)
            {
                sb.Replace(pair.Key, pair.Value);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 通过正则字典实现批量替换
        /// </summary>
        /// <param name="input"></param>
        /// <param name="replacements">key：正则表达式  value:要替换的值</param>
        /// <returns></returns>
        public static string ReplaceWithRegex(string input, Dictionary<string, string> replacements)
        {
            if (string.IsNullOrEmpty(input) || replacements == null || replacements.Count == 0)
                return input;

            // 转义特殊字符并按长度排序
            var patterns = replacements
                           .Where(x => !string.IsNullOrEmpty(x.Key))
                           .Select(x => new { Pattern = Regex.Escape(x.Key), Replacement = x.Value })
                           .OrderByDescending(x => x.Pattern.Length)
                           .ToList();

            if (patterns.Count == 0)
                return input;

            // 构建正则表达式模式
            var pattern = string.Join("|", patterns.Select(x => x.Pattern));
            var regex = new Regex(pattern);

            return regex.Replace(input, match =>
            {
                var key = match.Value;
                var kvp = replacements.FirstOrDefault(x => x.Key == key);
                return kvp.Value ?? string.Empty;
            });
        }
    }
}