namespace Common.SecurityCrypto
{
    /// <summary>
    /// 加密类型
    /// </summary>
    public enum EncryptionTypeEnum
    {
        None = 0,
        SM2 = 1,
        SM3 = 2,
        SM4 = 3,
        AES = 4,
        RSA = 5,
        HMACSHA256 = 6
    }
}