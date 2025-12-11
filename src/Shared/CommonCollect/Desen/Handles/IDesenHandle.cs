namespace CommonCollect.Desen.Handles
{
    /// <summary>
    /// 脱敏处理程序
    /// </summary>
    public interface IDesenHandle
    {
        /// <summary>
        /// 获取脱敏结果
        /// </summary>
        /// <param name="input"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="keyword">关键字</param>
        /// <param name="replaceChar">替换字符</param>
        /// <returns></returns>
        string GetDesenResult(string input, int x, int y, string keyword = "", char replaceChar = '*');
    }
}