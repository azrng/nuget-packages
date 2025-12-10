namespace Azrng.Core.Helpers
{
    public class ChineseTextHelper
    {
        /// <summary>
        /// 去除字符串开头和结尾的非汉字内容
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>清理后的字符串</returns>
        public static string RemoveNonChineseFromStartAndEnd(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // 如果整个字符串都没有汉字，直接返回空字符串
            if (!ContainsChinese(input))
                return string.Empty;

            // 去除开头的非汉字
            var startIndex = 0;
            while (startIndex < input.Length && !IsChineseCharacter(input[startIndex]))
            {
                startIndex++;
            }

            // 如果已经到字符串末尾，说明没有汉字
            if (startIndex >= input.Length)
                return string.Empty;

            // 去除结尾的非汉字
            var endIndex = input.Length - 1;
            while (endIndex >= startIndex && !IsChineseCharacter(input[endIndex]))
            {
                endIndex--;
            }

            // 提取汉字部分
            return input.Substring(startIndex, endIndex - startIndex + 1);
        }

        /// <summary>
        /// 判断字符是否为汉字
        /// </summary>
        public static bool IsChineseCharacter(char c)
        {
            // 汉字Unicode范围：0x4E00-0x9FFF
            return c >= 0x4E00 && c <= 0x9FFF;
        }

        /// <summary>
        /// 检查字符串是否包含汉字
        /// </summary>
        public static bool ContainsChinese(string text)
        {
            if (text is null)
                return false;
            foreach (var c in text)
            {
                if (IsChineseCharacter(c))
                    return true;
            }

            return false;
        }
    }
}