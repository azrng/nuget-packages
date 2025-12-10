using System;

namespace Azrng.Core.Exceptions
{
    /// <summary>
    /// 业务逻辑异常
    /// </summary>
    public class LogicBusinessException : BaseException
    {
        public LogicBusinessException() : base("400", "业务逻辑异常") { }

        public LogicBusinessException(string message) : base(message) { }

        public LogicBusinessException(string message, Exception innerException) : base(message, innerException) { }

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

        private static void Throw(string paramName) => throw new LogicBusinessException(paramName);

        #endregion
    }
}