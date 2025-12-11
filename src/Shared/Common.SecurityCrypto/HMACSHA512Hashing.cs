using Common.SecurityCrypto.InternationalCrypto;
using Common.SecurityCrypto.Model;
using System.Text;

namespace Common.SecurityCrypto
{
    public class HMACSHA512Hashing : AbsHashingProvider
    {
        public HMACSHA512Hashing()
          : this(encoding: Encoding.UTF8)
        {
        }

        public HMACSHA512Hashing(OutType outType = OutType.Hex, Encoding encoding = null)
          : base(outType, encoding)
        {
        }

        public override string Signature(string value, string key = "") => HMACSHA512HashingProvider.Signature(value, key, OutType, Encoding);
    }
}