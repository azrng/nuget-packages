using Common.SecurityCrypto.Core;
using Common.SecurityCrypto.Model;
using System.Security.Cryptography;
using System.Text;

namespace Common.SecurityCrypto.InternationalCrypto
{
    public sealed class HMACMD5HashingProvider : HMACHashingBase
    {
        private HMACMD5HashingProvider()
        {
        }

        public static string Signature(string value, string key, OutType outType = OutType.Hex, Encoding encoding = null) => Encrypt<HMACMD5>(value, key, encoding, outType);

        public static bool Verify(
          string comparison,
          string value,
          string key,
          OutType outType = OutType.Hex,
          Encoding encoding = null)
        {
            return comparison == Signature(value, key, outType, encoding);
        }
    }
}