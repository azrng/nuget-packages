using Common.SecurityCrypto.InternationalCrypto;
using Common.SecurityCrypto.Model;
using System.Text;

namespace Common.SecurityCrypto
{
    public class SHA512Hashing : AbsHashingProvider
    {
        public SHA512Hashing()
          : this(encoding: Encoding.UTF8)
        {
        }

        public SHA512Hashing(OutType outType = OutType.Hex, Encoding encoding = null)
          : base(outType, encoding)
        {
        }

        public override string Signature(string value, string key = "") => SHA512HashingProvider.Signature(value, OutType, Encoding);
    }
}