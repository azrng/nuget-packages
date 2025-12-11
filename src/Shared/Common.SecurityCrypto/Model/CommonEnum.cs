namespace Common.SecurityCrypto.Model
{
    /// <summary>
    /// 哈希算法枚举
    /// </summary>
    public enum HashingProviderType
    {
        HMACMD5,
        HMACSHA1,
        HMACSHA256,
        HMACSHA512,
        MD5,
        SHA1,
        SHA256,
        SHA512, // 0x0000000B
        SM3, // 0x0000000C
    }

    /// <summary>
    /// 非对称加密枚举
    /// </summary>
    public enum AsymmetricProviderType
    {
        RSA,
        RSA2,
        SM2,
    }

    /// <summary>
    /// 对称加密
    /// </summary>
    public enum SymmetricProviderType
    {
        AES,

        //AES128,
        //AES192,
        //AES256,
        DES,

        //TripleDES128,
        //TripleDES192,
        SM4,

        //SM4JAVA,
        SM4JS
    }

    /// <summary>
    /// 返回格式
    /// </summary>
    public enum OutType
    {
        /// <summary>
        /// base64
        /// </summary>
        Base64,

        /// <summary>
        /// Hex格式
        /// </summary>
        Hex,
    }

    /// <summary>
    /// rsa key格式
    /// </summary>
    public enum RSAKeyType
    {
        Xml,
        Json,
        Base64,
    }

    public enum RSAKeySizeType
    {
        L1024 = 1024, // 0x00000400
        L2048 = 2048, // 0x00000800
        L3072 = 3072, // 0x00000C00
        L4096 = 4096, // 0x00001000
    }

    public enum RSAType
    {
        RSA,
        RSA2,
    }

    public enum AESKeySizeType
    {
        L128 = 128, // 0x00000080
        L192 = 192, // 0x000000C0
        L256 = 256, // 0x00000100
    }
}