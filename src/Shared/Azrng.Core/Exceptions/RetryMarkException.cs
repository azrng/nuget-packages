using System;

namespace Azrng.Core.Exceptions
{
    /// <summary>
    /// 重试标记异常
    /// </summary>
    public class RetryMarkException : BaseException
    {
        public RetryMarkException() : base("400", "需要重试") { }

        public RetryMarkException(string message) : base("400", message) { }

        public RetryMarkException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>表示重试标记的异常</summary>
        /// <summary>重试标记的异常</summary>
        /// <param name="inner">内部异常</param>
        public RetryMarkException(Exception inner) : base("需要重试", inner) { }

        #region 私有方法

        /// <summary>
        /// 如果为null就抛出异常
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="paramName"></param>
        public static void ThrowIfNull(object? argument, string paramName)
        {
            if (argument != null)
                return;
            Throw(paramName);
        }

        private static void Throw(string paramName) => throw new ParameterException(paramName);

        #endregion
    }
}