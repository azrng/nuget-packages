using System;

namespace CommonCollect.Desen.Handles
{
    /// <summary>
    /// 关键词前遮盖：遮盖关键词的前面  从x到y的位置遮盖
    /// </summary>
    public class KeywordFrontCoverHandle : IDesenHandle
    {
        /// <summary>
        /// 执行结果
        /// </summary>
        /// <param name="input"></param>
        /// <param name="x">关键字前开始位置</param>
        /// <param name="y">关键字前结束位置</param>
        /// <param name="keyword">执行结果</param>
        /// <param name="replaceChar">替换字符</param>
        /// <returns></returns>
        public string GetDesenResult(string input, int x, int y, string keyword, char replaceChar = '*')
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(keyword))
                return input;

            var length = input.Length;
            var keywordIndex = input.IndexOf(keyword, StringComparison.Ordinal);
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

                if (x > keywordIndex)
                {
                    return input;
                }

                if (y > keywordIndex)
                {
                    y = keywordIndex;
                }

                // 前面不处理字符串
                var frontNoHandleStr = input.Substring(0, x - 1);

                var noHandlerStr = input.Substring(y, length - y);

                return string.Concat(frontNoHandleStr, new string(replaceChar, y - x + 1), noHandlerStr);
            }
            else
            {
                if (x < 1)
                    return input;
                if (y < 1)
                    y = 1;

                if (x < y || y > keywordIndex)
                {
                    return input;
                }

                // 转换为正向位置索引
                x = (x > keywordIndex) ? 0 : keywordIndex - x;

                // 前面不处理字符串
                var frontNoHandleStr = input.Substring(0, x);

                var noHandlerStr = input.Substring(keywordIndex - y + 1, length - keywordIndex + y - 1);

                // 转换为正向位置索引
                y = keywordIndex - y;

                return string.Concat(frontNoHandleStr, new string(replaceChar, y - x + 1), noHandlerStr);
            }
        }
    }
}