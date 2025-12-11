using Common.SecurityCrypto.Internals;
using Common.SecurityCrypto.Model;
using Common.SecurityCrypto.SMCrypto;
using System.Text;

namespace Common.SecurityCrypto
{
    public class SM4Encryption : AbsSymmetricProvider
    {
        protected SM4 sm4 = null;

        public SM4Encryption()
          : this(encoding: Encoding.UTF8)
        {
        }

        public SM4Encryption(OutType outType = OutType.Hex, Encoding encoding = null)
          : base(outType, encoding)
        {
            sm4 = new SM4(outType);
        }

        public override SymmetricKey CreateKey()
        {
            var str1 = RandomStringGenerator.Generate(16);
            var str2 = RandomStringGenerator.Generate(16);
            return new SymmetricKey() { Key = str1, IV = str2 };
        }

        public override string Encrypt(string value, string key, string iv = null)
        {
            sm4.secretKey = key;
            sm4.hexString = false;
            if (string.IsNullOrWhiteSpace(iv) || iv == null || iv?.Length == 0)
                return sm4.EncryptECB(value);
            sm4.iv = iv;
            return sm4.EncryptCBC(value);
        }

        public override string Decrypt(string value, string key, string iv = null)
        {
            sm4.secretKey = key;
            sm4.hexString = false;
            if (string.IsNullOrWhiteSpace(iv) || iv == null || iv?.Length == 0)
                return sm4.DecryptECB(value);
            sm4.iv = iv;
            return sm4.DecryptCBC(value);
        }
    }
}