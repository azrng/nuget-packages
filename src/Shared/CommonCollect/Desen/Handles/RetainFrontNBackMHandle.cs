namespace CommonCollect.Desen.Handles
{
    /// <summary>
    /// 保留前面n个以及后面m个字符
    /// </summary>
    public class RetainFrontNBackMHandle : IDesenHandle
    {
        /// <summary>
        /// 执行脱敏
        /// </summary>
        /// <param name="input">输入的文本</param>
        /// <param name="n">前面保留的位数</param>
        /// <param name="m">后面保留的位数</param>
        /// <param name="keyword"></param>
        /// <param name="replaceChar">替换字符</param>
        /// <returns></returns>
        public string GetDesenResult(string input, int n, int m, string keyword, char replaceChar = '*')
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var length = input.Length;

            if (n < 1 || m < 1 || length < n || length < m || n + m > length)
            {
                return input;
            }

            var prefix = input[..n];
            var suffix = input[(length - m)..];
            var residue = length - n - m;
            return prefix.PadRight(residue + n, replaceChar) + suffix;
        }
    }
}