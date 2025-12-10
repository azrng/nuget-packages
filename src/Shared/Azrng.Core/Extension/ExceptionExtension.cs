using System;

namespace Azrng.Core.Extension
{
    /// <summary>
    /// 异常扩展
    /// </summary>
    public static class ExceptionExtension
    {
        /// <summary>
        /// 获取异常详细信息包含堆栈行数
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string GetExceptionAndStack(this Exception ex)
        {
            return $"message：{ex.Message} stackTrace：{ex.StackTrace} innerException：{ex.InnerException}";
        }
    }
}