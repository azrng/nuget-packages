using Common.SecurityCrypto.Extensions;
using Common.SecurityCrypto.Model;

namespace Common.SecurityCrypto
{
    /// <summary>
    /// 非对称加密服务工厂
    /// </summary>
    public sealed class AsymmetricProviderFactory
    {
        public static IAsymmetricProvider Create(string providerTypestr = "RSA") => Create(providerTypestr.ToEnum<AsymmetricProviderType>());

        public static IAsymmetricProvider Create(AsymmetricProviderType providerType = AsymmetricProviderType.RSA)
        {
            switch (providerType)
            {
                case AsymmetricProviderType.RSA:
                    return new RSAEncryption();

                case AsymmetricProviderType.RSA2:
                    return new RSA2Encryption();

                case AsymmetricProviderType.SM2:
                    return new SM2Encryption();

                default:
                    return new RSAEncryption();
            }
        }
    }
}