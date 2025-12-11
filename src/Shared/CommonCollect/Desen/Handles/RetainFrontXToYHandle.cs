using System;

namespace CommonCollect.Desen.Handles
{
    /// <summary>
    /// 保留自x到y字符 保存从x到y的位置
    /// </summary>
    public class RetainFrontXToYHandle : IDesenHandle
    {
        /// <summary>
        /// 执行结果
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
                    return string.Concat(new string(replaceChar, length));
                }

                if (y > length)
                {
                    y = input.Length;
                }

                // 要保留的字符串
                var retainStr = input.Substring(length - y, y - x + 1);

                return string.Concat(new string(replaceChar, length - y), retainStr, new string(replaceChar, x - 1));
            }
            else
            {
                // 从左侧开始
                if (x < 1 || y < 1 || x > y)
                {
                    return input;
                }

                if (y > length)
                {
                    y = input.Length;
                }

                // 要保留的字符串
                var retainStr = input.Substring(x - 1, y - x + 1);

                return string.Concat(new string(replaceChar, x - 1), retainStr, new string(replaceChar, length - y));
            }
        }
    }
}