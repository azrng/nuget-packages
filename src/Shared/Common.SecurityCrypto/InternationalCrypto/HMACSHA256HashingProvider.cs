using Common.SecurityCrypto.Core;
using Common.SecurityCrypto.Model;
using System.Security.Cryptography;
using System.Text;

namespace Common.SecurityCrypto.InternationalCrypto
{
    public sealed class HMACSHA256HashingProvider : HMACHashingBase
    {
        private HMACSHA256HashingProvider()
        {
        }

        public static string Signature(string value, string key, OutType outType = OutType.Hex, Encoding encoding = null) => Encrypt<HMACSHA256>(value, key, encoding, outType);

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