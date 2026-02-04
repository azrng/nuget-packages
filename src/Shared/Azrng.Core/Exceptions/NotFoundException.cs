namespace Azrng.Core.Exceptions
{
    /// <summary>
    /// 对象未找到
    /// </summary>
    public class NotFoundException : BaseException
    {
        public NotFoundException() : base("未找到对象") { }

        public NotFoundException(string errorMessage)
            : base("404", errorMessage) { }

        public NotFoundException(string code, string message)
            : base(code, message) { }

        public NotFoundException(string message, System.Exception innerException)
            : base(message, innerException) { }

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

        /// <summary>
        ///Throw
        /// </summary>
        /// <param name="paramName"></param>
        private static void Throw(string paramName) => throw new NotFoundException(paramName);

        #endregion 私有方法
    }
}