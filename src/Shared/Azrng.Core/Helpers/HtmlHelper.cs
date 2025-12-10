using System.Linq;
using System.Text.RegularExpressions;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// html帮助类
    /// </summary>
    public static class HtmlHelper
    {
        /// <summary>
        /// 将HTML标签转换成TEXT文本
        /// </summary>
        /// <param name="strHtml"></param>
        /// <returns></returns>
        public static string HtmlToText(string strHtml)
        {
            string[] aryReg =
            [
                @"<script[^>]*?>.*?</script>",
                @"<style[^>]*?>.*?</style>",
                @"<(\/\s*)?!?((\w+:)?\w+)(\w+(\s*=?\s*(([""'])(\\[""'tbnr]|[^\7])*?\7|\w+)|.{0})|\s)*?(\/\s*)?>",
                @"([\r\n])[\s]+", "&(quot|#34);", @"&(amp|#38);", @"&(lt|#60);", @"&(gt|#62);", @"&(nbsp|#160);",
                @"&(iexcl|#161);", @"&(cent|#162);", @"&(pound|#163);", @"&(copy|#169);", @"&#(\d+);", @"-->",
                @"<!--.*\n"
            ];

            var strOutput = aryReg.Select(t => new Regex(t, RegexOptions.IgnoreCase))
                                  .Aggregate(strHtml, (current, regex) => regex.Replace(current, string.Empty));

            return strOutput.Replace("<", "").Replace(">", "").Replace("\r\n", "").Trim();
        }
    }
}