using Common.SecurityCrypto.InternationalCrypto;
using Common.SecurityCrypto.Model;
using System.Text;

namespace Common.SecurityCrypto
{
    public class RSA2Encryption : RSAEncryption
    {
        public RSA2Encryption()
          : this(encoding: Encoding.UTF8)
        {
        }

        public RSA2Encryption(OutType outType = OutType.Hex, RSAKeyType keyType = RSAKeyType.Xml, Encoding encoding = null)
          : base(outType, keyType, encoding)
        {
        }

        public override string SignData(string source, string privateKey) => RSAEncryptionProvider.SignData(source, privateKey, Encoding, OutType, RSAType.RSA2, KeyType);

        public override bool VerifyData(string source, string signData, string publicKey) => RSAEncryptionProvider.VerifyData(source, signData, publicKey, Encoding, OutType, RSAType.RSA2, KeyType);
    }
}