using System.Text;
using System.Text.RegularExpressions;

namespace Azrng.Core.Extension
{
    /// <summary>
    /// 正则扩展
    /// </summary>
    public static class RegexExtensions
    {
        /// <summary>
        /// 普通字符串处理为正则表达式可识别字符串
        /// </summary>
        /// <param name="value">待转义字符串</param>
        /// <returns></returns>
        public static string ToRegex(this string value)
        {
            return Regex.Replace(value, @"\W", match => @"\" + match);
        }

        /// <summary>
        /// 验证输入字符串是否与模式字符串匹配，匹配返回true
        /// </summary>
        /// <param name="input">输入的字符串</param>
        /// <param name="pattern">模式字符串</param>
        /// <param name="options">筛选条件</param>
        public static bool IsMatch(this string input, string pattern, RegexOptions options)
        {
            return Regex.IsMatch(input, pattern, options);
        }

        /// <summary>
        /// 验证身份证是否合法
        /// </summary>
        /// <param name="idCard">要验证的身份证</param>
        public static bool IsIdCard(this string? idCard)
        {
            //如果为空，认为验证合格
            if (string.IsNullOrEmpty(idCard))
            {
                return true;
            }

            //模式字符串
            var pattern = new StringBuilder();
            pattern.Append(@"^(11|12|13|14|15|21|22|23|31|32|33|34|35|36|37|41|42|43|44|45|46|");
            pattern.Append(@"50|51|52|53|54|61|62|63|64|65|71|81|82|91)");
            pattern.Append(@"(\d{13}|\d{15}[\dx])$");

            //验证
            return idCard.Trim().IsMatch(pattern.ToString());
        }

        /// <summary>
        /// 验证手机格式是否合法
        /// </summary>
        /// <param name="cellPhone">要验证的手机号码</param>
        /// <returns></returns>
        public static bool IsPhone(this string? cellPhone)
        {
            if (string.IsNullOrEmpty(cellPhone))
            {
                return true;
            }

            cellPhone = cellPhone.Trim();
            const string pattern = @"^[1][3,4,5,7,8,9]\d{9}$";
            return cellPhone.IsMatch(pattern);
        }

        /// <summary>
        /// 验证字符串是否匹配正则表达式描述的规则
        /// </summary>
        /// <param name="inputStr">待验证的字符串</param>
        /// <param name="patternStr">正则表达式字符串</param>
        /// <returns>是否匹配</returns>
        public static bool IsMatch(this string inputStr, string patternStr)
        {
            return inputStr.IsMatch(patternStr, false, false);
        }

        /// <summary>
        /// 验证字符串是否匹配正则表达式描述的规则
        /// </summary>
        /// <param name="inputStr">待验证的字符串</param>
        /// <param name="patternStr">正则表达式字符串</param>
        /// <param name="ifIgnoreCase">匹配时是否不区分大小写</param>
        /// <param name="ifValidateWhiteSpace">是否验证空白字符串</param>
        /// <returns>是否匹配</returns>
        public static bool IsMatch(this string inputStr, string patternStr, bool ifIgnoreCase,
            bool ifValidateWhiteSpace)
        {
            if (!ifValidateWhiteSpace && string.IsNullOrWhiteSpace(inputStr))
                return false; //如果不要求验证空白字符串而此时传入的待验证字符串为空白字符串，则不匹配
            var regex = ifIgnoreCase
                ? new Regex(patternStr, RegexOptions.IgnoreCase)
                : //指定不区分大小写的匹配
                new Regex(patternStr);
            return regex.IsMatch(inputStr);
        }

        #region 字符串

        /// <summary>
        /// 有效盘符
        /// </summary>
        public static readonly string AvailDisk = new Regex(@"[a-zA-Z]").ToString();

        /// <summary>
        /// 有效分隔符
        /// </summary>
        public static readonly string AvailSplit = new Regex(@"(?<!\\| |\.)\\(?!\\| )").ToString();

        /// <summary>
        /// 无效命名字符集
        /// </summary>
        // ""转义为"，并非正则的转义
        public static readonly string NoAvailChar = new Regex(@"\\/:*?""<>|\f\n\r\t\v").ToString();

        /// <summary>
        /// 无效扩展名字符集
        /// </summary>
        public static readonly string NoAvailExt = NoAvailChar + @" \.";

        /// <summary>
        /// 文件路径（扩展名可有可无），示例：
        /// <para>C:\folder\1txt</para>
        /// </summary>
        // 解析：[盘符:] + 1或多段[\合法文件夹或文件名]
        public static readonly string FilePath = $@"^{AvailDisk}:({AvailSplit}[^{NoAvailChar}]+)+(?<! |\.)$";

        /// <summary>
        /// 文件路径（带扩展名），示例：
        /// <para>C:\1.txt</para>
        /// </summary>
        // 解析：[盘符:] + 1或多段[\合法文件夹或文件名] + [合法后缀]
        public static readonly string FilePath2 = $@"^{AvailDisk}:({AvailSplit}[^{NoAvailChar}]+)+(?<=\.[^ \.]+)$";

        /// <summary>
        /// 文件扩展名（不含.），示例：
        /// <para>txt</para>
        /// <para>xls</para>
        /// </summary>
        public static readonly string ExtName = $@"^([^{NoAvailExt}])+$";


        /// <summary>
        /// 文件夹路径（磁盘根路径有效），示例：
        /// <para>C:\</para>
        /// <para>C:\folder\</para>
        /// </summary>
        // 解析：[盘符:] + 0或多段[\合法文件夹名]
        public static readonly string FolderPath = $@"^{AvailDisk}:({AvailSplit}[^{NoAvailChar}]*)*(?<! |\.)$";

        /// <summary>
        /// 文件夹路径（磁盘根路径无效），示例：
        /// <para>C:\folder</para>
        /// </summary>
        // 解析：[盘符:] + 1或多段[\合法文件夹名]
        public static readonly string FolderPath2 = $@"^{AvailDisk}:(?!\\$)({AvailSplit}[^{NoAvailChar}]*)+(?<! |\.)$";

        #endregion 字符串
    }
}