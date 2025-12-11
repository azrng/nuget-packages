using Common.SecurityCrypto.InternationalCrypto;
using Common.SecurityCrypto.Model;
using System.Text;

namespace Common.SecurityCrypto
{
    public class HMACSHA1Hashing : AbsHashingProvider
    {
        public HMACSHA1Hashing()
          : this(encoding: Encoding.UTF8)
        {
        }

        public HMACSHA1Hashing(OutType outType = OutType.Hex, Encoding encoding = null)
          : base(outType, encoding)
        {
        }

        public override string Signature(string value, string key = "") => HMACSHA1HashingProvider.Signature(value, key, OutType, Encoding);
    }
}