using Common.SecurityCrypto.InternationalCrypto;
using Common.SecurityCrypto.Model;
using System.Text;

namespace Common.SecurityCrypto
{
    public class HMACSHA256Hashing : AbsHashingProvider
    {
        public HMACSHA256Hashing()
          : this(encoding: Encoding.UTF8)
        {
        }

        public HMACSHA256Hashing(OutType outType = OutType.Hex, Encoding encoding = null)
          : base(outType, encoding)
        {
        }

        public override string Signature(string value, string key) => HMACSHA256HashingProvider.Signature(value, key, OutType, Encoding);
    }
}