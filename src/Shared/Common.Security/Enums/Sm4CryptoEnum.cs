using System.ComponentModel;

namespace Common.Security.Enums
{
    /// <summary>
    /// 加密类型
    /// </summary>
    public enum Sm4CryptoEnum
    {
        /// <summary>
        /// ECB(电码本模式)
        /// </summary>
        [Description("ECB模式")] ECB = 0,

        /// <summary>
        /// CBC(密码分组链接模式)
        /// </summary>
        [Description("CBC模式")] CBC = 1
    }
}