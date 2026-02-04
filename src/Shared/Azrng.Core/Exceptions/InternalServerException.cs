using System;

namespace Azrng.Core.Exceptions
{
    /// <summary>
    /// 系统服务异常
    /// </summary>
    public class InternalServerException : BaseException
    {
        public InternalServerException() : base("500", "系统服务异常") { }

        public InternalServerException(string message) : base("500", message) { }

        public InternalServerException(string message, Exception innerException) : base(message, innerException) { }

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

        private static void Throw(string paramName) => throw new InternalServerException(paramName);

        #endregion 私有方法
    }
}