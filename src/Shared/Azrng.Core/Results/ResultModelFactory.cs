using Azrng.Core.Exceptions;
using System;

namespace Azrng.Core.Results
{
    /// <summary>
    /// 结果模型创建工厂。
    /// </summary>
    public static class ResultModelFactory
    {
        /// <summary>
        /// 创建成功结果。
        /// </summary>
        /// <param name="data">返回数据。</param>
        /// <param name="message">成功消息。</param>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <returns>结果模型。</returns>
        public static IResultModel<T> Success<T>(T data, string? message = null)
        {
            return message is null
                ? ResultModel<T>.Success(data)
                : ResultModel<T>.Success(data, message);
        }

        /// <summary>
        /// 创建失败结果。
        /// </summary>
        /// <param name="message">失败消息。</param>
        /// <param name="errorCode">错误码。</param>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <returns>结果模型。</returns>
        public static IResultModel<T> Failure<T>(string message, string errorCode = "ERROR")
        {
            return ResultModel<T>.Failure(message, errorCode);
        }

        /// <summary>
        /// 根据异常创建失败结果。
        /// </summary>
        /// <param name="exception">异常。</param>
        /// <param name="message">失败消息；为空时使用异常消息。</param>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <returns>结果模型。</returns>
        public static IResultModel<T> FromException<T>(Exception exception, string? message = null)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            var errorCode = exception is BaseException baseException ? baseException.ErrorCode : "ERROR";
            return Failure<T>(message ?? exception.Message, errorCode);
        }
    }
}
