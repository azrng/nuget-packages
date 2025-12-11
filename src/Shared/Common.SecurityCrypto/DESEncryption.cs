using Common.SecurityCrypto.InternationalCrypto;
using Common.SecurityCrypto.Model;
using System.Text;

namespace Common.SecurityCrypto
{
    public class DESEncryption : AbsSymmetricProvider
    {
        public DESEncryption()
          : this(encoding: Encoding.UTF8)
        {
        }

        public DESEncryption(OutType outType = OutType.Hex, Encoding encoding = null)
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

        public override string Encrypt(string value, string key, string iv = null) => DESEncryptionProvider.Encrypt(value, key, iv, outType: OutType, encoding: Encoding);

        public override string Decrypt(string value, string key, string iv = null) => DESEncryptionProvider.Decrypt(value, key, iv, outType: OutType, encoding: Encoding);
    }
}