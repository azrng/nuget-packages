using System;

namespace Azrng.Cache.MemoryCache
{
    /// <summary>异常扩展</summary>
    internal static class ExceptionExtension
    {
        /// <summary>获取异常详细信息包含堆栈行数</summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        internal static string GetExceptionAndStack(this Exception ex)
        {
            return $"message：{ex.Message} stackTrace：{ex.StackTrace} innerException：{ex.InnerException}";
        }
    }
}