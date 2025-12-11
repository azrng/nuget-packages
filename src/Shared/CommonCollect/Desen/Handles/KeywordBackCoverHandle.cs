using System;

namespace CommonCollect.Desen.Handles
{
    /// <summary>
    /// 关键词后遮盖：遮盖关键词的后面 从x到y的位置遮盖
    /// </summary>
    public class KeywordBackCoverHandle : IDesenHandle
    {
        /// <summary>
        /// 执行结果
        /// </summary>
        /// <param name="input"></param>
        /// <param name="x">关键字后开始位置</param>
        /// <param name="y">关键字后结束位置</param>
        /// <param name="keyword">关键词</param>
        /// <param name="replaceChar">替换字符</param>
        /// <returns></returns>
        public string GetDesenResult(string input, int x, int y, string keyword, char replaceChar = '*')
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(keyword))
                return input;

            var length = input.Length;
            var keywordIndex = input.IndexOf(keyword, StringComparison.CurrentCulture);
            if (keywordIndex == -1)
                return input;

            if (x < 0 && y < 0)
            {
                // 从左侧开始

                x = Math.Abs(x);
                y = Math.Abs(y);

                if (x > y)
                {
                    return input;
                }

                if (x > length - keywordIndex - keyword.Length)
                {
                    return input;
                }

                if (y > length - keywordIndex - keyword.Length)
                {
                    y = keywordIndex - keyword.Length;
                }

                var prefix = input.Substring(0, length - y);
                var suffix = input.Substring(length - x + 1, x - 1);
                return string.Concat(prefix, new string(replaceChar, y - x + 1), suffix);
            }
            else
            {
                if (y < 1)
                    return input;
                if (x < 1)
                    x = 1;

                if (x > y || x > (length - keywordIndex))
                {
                    return input;
                }

                if (x > (input.Length - keywordIndex - keyword.Length))
                    return input;

                if ((keywordIndex + keyword.Length + y) > input.Length)
                    y = input.Length - keywordIndex - keyword.Length;

                var prefix = input.Substring(0, keywordIndex + x + keyword.Length - 1);
                var suffix = input.Substring(keywordIndex + keyword.Length + y, length - keywordIndex - y - keyword.Length);
                return string.Concat(prefix, new string(replaceChar, y - x + 1), suffix);
            }
        }
    }
}