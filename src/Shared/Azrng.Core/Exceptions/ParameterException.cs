using System;

namespace Azrng.Core.Exceptions
{
    /// <summary>
    /// 参数异常
    /// </summary>
    public class ParameterException : BaseException
    {
        public ParameterException() : base("400", "参数错误") { }

        public ParameterException(string message) : base("400", message) { }

        public ParameterException(string message, Exception innerException) : base(message, innerException) { }

        #region 私有方法

        /// <summary>
        /// 如果为null就抛出异常
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="paramName"></param>
        public static void ThrowIfNull(object argument, string paramName)
        {
            if (argument != null)
                return;
            Throw(paramName);
        }

        private static void Throw(string paramName) => throw new ParameterException(paramName);

        #endregion
    }
}