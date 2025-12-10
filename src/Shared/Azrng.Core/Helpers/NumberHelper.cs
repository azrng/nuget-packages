using System.Text;

namespace Azrng.Core.Helpers
{
    public static class NumberHelper
    {
        /// <summary>
        /// 格式化long类型数字
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        /// <remarks>
        /// 如果数字小于一千，则直接转换为字符串
        /// 如果数字大于或等于一千且小于一亿则将其除以一千并转换为带有两位小数的字符串，后面加上“万”。
        /// 如果数字大于或等于一亿，则将其除以一亿并转换为带有两位小数的字符串，后面加上“亿”
        /// 如果数字大于或等于一亿亿，则将其除以一亿亿并转换为带有两位小数的字符串，后面加上“万亿”
        /// </remarks>
        public static string FormatLongNumber(long number)
        {
            switch (number)
            {
                case < 1000:
                    return number.ToString();
                case < 100000000:
                    {
                        var value = number / (double)10000;
                        return $"{value:0.0}万";
                    }
                case < 1_000_000_000_000:
                    {
                        var value = number / (double)100000000;
                        return $"{value:0.0}亿";
                    }
                default:
                    {
                        var value = number / (double)1_000_000_000_000;
                        return $"{value:0.0}万亿";
                    }
            }
        }

        /// <summary>
        /// 将数字字符串转换为中文数字
        /// </summary>
        /// <param name="input">例如 "1234"</param>
        /// <returns>例如 "一二三四"</returns>
        public static string ConvertToChinese(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var result = new StringBuilder();
            foreach (var c in input)
            {
                if (CommonCoreConst._digitToChinese.TryGetValue(c, out var chineseDigit))
                {
                    result.Append(chineseDigit);
                }
                else if (c == ' ') // 处理空格分隔符
                {
                    result.Append(' ');
                }

                // 忽略其他非数字字符（或可抛出异常）
            }

            return result.ToString().Trim();
        }
    }
}