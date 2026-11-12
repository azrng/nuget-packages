using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// html帮助类
    /// </summary>
    public static class HtmlHelper
    {
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(2);

        /// <summary>
        /// 将HTML标签转换成TEXT文本
        /// </summary>
        /// <param name="strHtml"></param>
        /// <returns></returns>
        public static string HtmlToText(string strHtml)
        {
            // script/style 加 Singleline 让 "." 匹配换行，否则多行块只剥标签、JS/CSS 正文残留在文本里
            string[] aryReg =
            [
                @"<script[^>]*?>.*?</script>",
                @"<style[^>]*?>.*?</style>",
                @"<(\/\s*)?!?((\w+:)?\w+)(\w+(\s*=?\s*(([""'])(\\[""'tbnr]|[^\7])*?\7|\w+)|.{0})|\s)*?(\/\s*)?>",
                @"([\r\n])[\s]+"
            ];

            var strOutput = aryReg.Select(t => new Regex(t, RegexOptions.IgnoreCase | RegexOptions.Singleline, RegexTimeout))
                                  .Aggregate(strHtml, (current, regex) => regex.Replace(current, string.Empty));

            strOutput = new Regex("<!--.*?-->", RegexOptions.IgnoreCase | RegexOptions.Singleline, RegexTimeout)
                .Replace(strOutput, string.Empty);

            // 先清理标签残留再还原实体，避免 &lt; 还原出的 "<" 被标签清理误删
            strOutput = strOutput.Replace("<", "").Replace(">", "").Replace("\r\n", "");

            // 命名/数字实体还原为对应字符（原实现整体替换为空串，R&amp;D 会变成 RD）
            strOutput = System.Net.WebUtility.HtmlDecode(strOutput);

            return strOutput.Trim();
        }
    }
}