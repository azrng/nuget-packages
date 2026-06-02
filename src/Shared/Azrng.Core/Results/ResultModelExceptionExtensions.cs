using System;

namespace Azrng.Core.Results
{
    /// <summary>
    /// 异常到结果模型的扩展方法。
    /// </summary>
    public static class ResultModelExceptionExtensions
    {
        /// <summary>
        /// 将异常转换为失败结果。
        /// </summary>
        /// <param name="exception">异常。</param>
        /// <param name="message">失败消息；为空时使用异常消息。</param>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <returns>失败结果。</returns>
        public static IResultModel<T> ToFailureResult<T>(this Exception exception, string? message = null)
        {
            return ResultModelFactory.FromException<T>(exception, message);
        }
    }
}
