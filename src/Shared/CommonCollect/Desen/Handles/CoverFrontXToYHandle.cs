using System;

namespace CommonCollect.Desen.Handles
{
    /// <summary>
    /// 遮盖从x至y个字符
    /// </summary>
    public class CoverFrontXToYHandle : IDesenHandle
    {
        /// <summary>
        /// 执行返回结果
        /// </summary>
        /// <param name="input"></param>
        /// <param name="x">开始位置</param>
        /// <param name="y">结束位置</param>
        /// <param name="keyword"></param>
        /// <param name="replaceChar">替换字符</param>
        /// <returns></returns>
        public string GetDesenResult(string input, int x, int y, string keyword, char replaceChar = '*')
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var length = input.Length;

            if (x < 0 && y < 0)
            {
                // 从右侧开始
                if (x < y)
                {
                    return input;
                }

                x = Math.Abs(x);
                y = Math.Abs(y);
                if (x > length)
                {
                    return input;
                }

                if (y > length)
                {
                    y = length;
                }

                return input.Substring(0, length - y) + new string(replaceChar, y - x + 1) +
                       input.Substring(length - x + 1, x - 1);
            }
            else
            {
                if (x < 1 || y < 1 || x > y || x > length)
                {
                    return input;
                }

                if (y > length)
                {
                    y = input.Length;
                }

                var prefix = input.Substring(0, x - 1);
                var suffix = input.Substring(y, length - y);

                return string.Concat(prefix, new string(replaceChar, y - x + 1), suffix);
            }
        }
    }
}