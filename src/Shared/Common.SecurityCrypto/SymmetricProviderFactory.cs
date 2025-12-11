using Common.SecurityCrypto.Extensions;
using Common.SecurityCrypto.Model;

namespace Common.SecurityCrypto
{
    public sealed class SymmetricProviderFactory
    {
        public static ISymmetricProvider Create(string providerTypestr = "SM4") => Create(providerTypestr.ToEnum<SymmetricProviderType>());

        public static ISymmetricProvider Create(SymmetricProviderType providerType = SymmetricProviderType.SM4)
        {
            switch (providerType)
            {
                //case SymmetricProviderType.AES128:
                //    return (ISymmetricProvider)new AESEncryptionL128();
                //case SymmetricProviderType.AES192:
                //    return (ISymmetricProvider)new AESEncryptionL192();
                //case SymmetricProviderType.AES256:
                //    return (ISymmetricProvider)new AESEncryptionL256();
                case SymmetricProviderType.DES:
                    return new DESEncryption();
                //case SymmetricProviderType.TripleDES128:
                //    return (ISymmetricProvider)new TripleDESEncryptionL128();
                //case SymmetricProviderType.TripleDES192:
                //    return (ISymmetricProvider)new TripleDESEncryptionL192();
                case SymmetricProviderType.SM4:
                    return new SM4Encryption();
                //case SymmetricProviderType.SM4JAVA:
                //    return (ISymmetricProvider)new SM4ForJavaEncryption();
                case SymmetricProviderType.SM4JS:
                    return new SM4ForJSEncryption();

                default:
                    return new DESEncryption();
            }
        }
    }
}