using System;

namespace Azrng.Core.Exceptions
{
    /// <summary>
    /// 禁止访问（已认证但无权限），对应 HTTP 403
    /// </summary>
    public class ForbiddenException : BaseException
    {
        public ForbiddenException() : base("403", "权限不通过") { }

        public ForbiddenException(string message) : base("403", message) { }

        public ForbiddenException(string message, Exception innerException) : base("403", message, innerException) { }

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

        private static void Throw(string paramName) => throw new ForbiddenException(paramName);

        #endregion
    }
}