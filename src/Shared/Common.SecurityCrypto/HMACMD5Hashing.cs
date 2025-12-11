using Common.SecurityCrypto.InternationalCrypto;
using Common.SecurityCrypto.Model;
using System.Text;

namespace Common.SecurityCrypto
{
    public class HMACMD5Hashing : AbsHashingProvider
    {
        public HMACMD5Hashing()
          : this(encoding: Encoding.UTF8)
        {
        }

        public HMACMD5Hashing(OutType outType = OutType.Hex, Encoding encoding = null)
          : base(outType, encoding)
        {
        }

        public override string Signature(string value, string key = "") => HMACMD5HashingProvider.Signature(value, key, OutType, Encoding);
    }
}