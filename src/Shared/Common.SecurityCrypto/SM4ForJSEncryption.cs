using Common.SecurityCrypto.Model;
using System.Text;

namespace Common.SecurityCrypto
{
    public class SM4ForJSEncryption : SM4Encryption
    {
        public SM4ForJSEncryption()
        {
        }

        public SM4ForJSEncryption(OutType outType = OutType.Hex, Encoding encoding = null)
          : base(encoding: Encoding.UTF8)
        {
        }

        public override string Encrypt(string value, string key, string iv = null)
        {
            sm4.secretKey = key;
            sm4.hexString = false;
            if (string.IsNullOrWhiteSpace(iv) || iv == null || !(iv != ""))
                return sm4.EncryptECB4JS(value);
            sm4.iv = iv;
            return sm4.EncryptCBC4JS(value);
        }

        public override string Decrypt(string value, string key, string iv = null)
        {
            sm4.secretKey = key;
            sm4.hexString = false;
            if (string.IsNullOrWhiteSpace(iv) || iv == null || iv == "")
                return sm4.DecryptECB4JS(value);
            sm4.iv = iv;
            return sm4.DecryptCBC4JS(value);
        }
    }
}