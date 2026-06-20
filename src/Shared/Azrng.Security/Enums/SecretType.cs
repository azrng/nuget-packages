namespace Common.Security.Enums
{
    /// <summary>
    /// 秘钥类型
    /// </summary>
    public enum SecretType
    {
        /// <summary>
        /// 文本
        /// </summary>
        Text,

        /// <summary>
        /// base64
        /// </summary>
        Base64,

        /// <summary>
        /// Hex格式（十六进制）
        /// </summary>
        Hex,
    }
}