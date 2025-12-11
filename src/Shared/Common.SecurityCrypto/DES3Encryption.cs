using Common.SecurityCrypto.InternationalCrypto;
using Common.SecurityCrypto.Model;
using System.Text;

namespace Common.SecurityCrypto
{
    public class DES3Encryption : AbsSymmetricProvider
    {
        public DES3Encryption()
          : this(encoding: Encoding.UTF8)
        {
        }

        public DES3Encryption(OutType outType = OutType.Hex, Encoding encoding = null)
          : base(outType, encoding)
        {
        }

        public override SymmetricKey CreateKey()
        {
            var key = DESEncryptionProvider.CreateKey();
            return new SymmetricKey()
            {
                Key = key.Key,
                IV = key.IV
            };
        }

        public override string Encrypt(string value, string key, string iv = null) => ThreeDESEncryptionProvider.Encrypt(value, key, outType: OutType, encoding: Encoding);

        public override string Decrypt(string value, string key, string iv = null) => ThreeDESEncryptionProvider.Decrypt(value, key, outType: OutType, encoding: Encoding);
    }
}