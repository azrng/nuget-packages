namespace CommonCollect.Desen.Handles
{
    /// <summary>
    /// 遮盖前面n个和后面m个
    /// </summary>
    public class CoverFrontNBackMHandle : IDesenHandle
    {
        /// <summary>
        /// 执行返回结果
        /// </summary>
        /// <param name="input"></param>
        /// <param name="n">遮盖前多少个</param>
        /// <param name="m">遮盖后面多少个</param>
        /// <param name="keyword"></param>
        /// <param name="replaceChar">替换字符</param>
        /// <returns></returns>
        public string GetDesenResult(string input, int n, int m, string keyword, char replaceChar = '*')
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var length = input.Length;

            if (n < 1 || m < 1)
            {
                return input;
            }

            // 如果长度大于或者前后有交集的那种 直接全部覆盖
            if (n > length || m > length || n > length - m)
                return new string(replaceChar, length);

            var noHandlerStr = input.Substring(n, length - m - n);

            return new string(replaceChar, n) + noHandlerStr + new string(replaceChar, m);
        }
    }
}