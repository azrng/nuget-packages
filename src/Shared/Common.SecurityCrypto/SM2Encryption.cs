using Common.SecurityCrypto.Model;
using Common.SecurityCrypto.SMCrypto;
using System.Text;

namespace Common.SecurityCrypto
{
    /// <summary>
    /// sm2加密
    /// </summary>
    public class SM2Encryption : AbsAsymmetricProvider
    {
        public SM2Encryption()
          : this(encoding: Encoding.UTF8)
        {
        }

        public SM2Encryption(OutType outType = OutType.Hex, RSAKeyType keyType = RSAKeyType.Xml, Encoding encoding = null)
          : base(outType, keyType, encoding)
        {
        }

        public override AsymmetricKey CreateKey(RSAKeySizeType size = RSAKeySizeType.L2048)
        {
            var prik = "";
            var pubk = "";
            SM2.GenerateKeyPair(out prik, out pubk);
            return new AsymmetricKey()
            {
                PrivateKey = prik,
                PublickKey = pubk
            };
        }

        public override string Encrypt(string value, string publicKey) => SM2.Encrypt(publicKey, value);

        public override string Decrypt(string value, string privateKey) => Encoding.UTF8.GetString(SM2.Decrypt(privateKey, value));

        public override string SignData(string source, string privateKey) => SM2.Sm2Sign(source, privateKey, KeyType == RSAKeyType.Xml);

        public override bool VerifyData(string source, string signData, string publicKey) => SM2.Verify(source, signData, publicKey, KeyType == RSAKeyType.Xml);
    }
}