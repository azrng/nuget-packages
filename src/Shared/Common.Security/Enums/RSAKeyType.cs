namespace Common.Security.Enums
{
    /// <summary>
    /// rsa key格式
    /// </summary>
    public enum RSAKeyType
    {
        Xml,

        /// <summary>
        /// PEM格式编码的字符串
        /// </summary>
        PEM,
    }

    /// <summary>
    /// rsa key格式
    /// </summary>
    public enum RsaKeyFormat
    {
        PKCS8,

        PKCS1,
    }
}