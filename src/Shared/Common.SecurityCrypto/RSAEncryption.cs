using Common.SecurityCrypto.InternationalCrypto;
using Common.SecurityCrypto.Model;
using System.Text;

namespace Common.SecurityCrypto
{
    public class RSAEncryption : AbsAsymmetricProvider
    {
        public RSAEncryption()
  : this(encoding: Encoding.UTF8)
        {
        }

        public RSAEncryption(OutType outType = OutType.Hex, RSAKeyType keyType = RSAKeyType.Xml, Encoding encoding = null)
          : base(outType, keyType, encoding)
        {
        }

        public override AsymmetricKey CreateKey(RSAKeySizeType size = RSAKeySizeType.L2048)
        {
            var key = RSAEncryptionProvider.CreateKey(size, KeyType);
            return new AsymmetricKey()
            {
                PrivateKey = key.PrivateKey,
                PublickKey = key.PublickKey
            };
        }

        public override string Encrypt(string value, string publicKey) => RSAEncryptionProvider.Encrypt(value, publicKey, Encoding, OutType, KeyType);

        public override string Decrypt(string value, string privateKey) => RSAEncryptionProvider.Decrypt(value, privateKey, Encoding, OutType, KeyType);

        public override string SignData(string source, string privateKey) => RSAEncryptionProvider.SignData(source, privateKey, Encoding, OutType, keyType: KeyType);

        public override bool VerifyData(string source, string signData, string publicKey) => RSAEncryptionProvider.VerifyData(source, signData, publicKey, Encoding, OutType, keyType: KeyType);
    }
}