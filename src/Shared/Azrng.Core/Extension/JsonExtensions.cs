namespace Azrng.Core.Extension
{
    /// <summary>
    /// json扩展
    /// </summary>
    public static class JsonExtensions
    {
        /// <summary>
        /// 验证json字符串是否是JArray
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public static bool IsJArrayString(this string jsonStr)
        {
            return !string.IsNullOrWhiteSpace(jsonStr) && jsonStr.Replace(" ", "").StartsWith("[");
        }
    }
}
