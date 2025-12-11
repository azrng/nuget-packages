using Common.SecurityCrypto.Extensions;
using Common.SecurityCrypto.Interfrace;
using Common.SecurityCrypto.Model;

namespace Common.SecurityCrypto
{
    /// <summary>
    /// 哈希提供程序工厂
    /// </summary>
    public sealed class HashingProviderFactory
    {
        public static IHashingProvider Create(string providerTypestr = "SHA256") => Create(providerTypestr.ToEnum<HashingProviderType>());

        public static IHashingProvider Create(HashingProviderType providerType = HashingProviderType.SHA256)
        {
            switch (providerType)
            {
                case HashingProviderType.HMACMD5:
                    return new HMACMD5Hashing();

                case HashingProviderType.HMACSHA1:
                    return new HMACSHA1Hashing();

                case HashingProviderType.HMACSHA256:
                    return new HMACSHA256Hashing();

                case HashingProviderType.HMACSHA512:
                    return new HMACSHA512Hashing();

                case HashingProviderType.MD5:
                    return new HMACMD5Hashing();

                case HashingProviderType.SHA1:
                    return new HMACSHA1Hashing();

                case HashingProviderType.SHA256:
                    return new SHA256Hashing();

                case HashingProviderType.SHA512:
                    return new SHA512Hashing();

                case HashingProviderType.SM3:
                    return new SM3Hashing();

                default:
                    return new SHA256Hashing();
            }
        }
    }
}