using System;
using System.Net;

namespace Azrng.Core.Exceptions
{
    /// <summary>
    /// 基础自定义错误信息
    /// </summary>
    public class BaseException : Exception
    {
        public BaseException() { }

        public BaseException(string message) : base(message) { }

        public BaseException(string code, string message)
            : base(message)
        {
            ErrorCode = code;
        }

        public BaseException(string message, Exception innerException)
            : base(message, innerException) { }

        public virtual HttpStatusCode HttpCode { get; set; } = HttpStatusCode.InternalServerError;

        /// <summary>
        /// 异常编码
        /// </summary>
        public string ErrorCode { get; }
    }
}