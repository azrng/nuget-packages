using Common.SecurityCrypto.InternationalCrypto;
using Common.SecurityCrypto.Model;
using System.Text;

namespace Common.SecurityCrypto
{
    public class SHA256Hashing : AbsHashingProvider
    {
        public SHA256Hashing()
          : this(encoding: Encoding.UTF8)
        {
        }

        public SHA256Hashing(OutType outType = OutType.Hex, Encoding encoding = null)
          : base(outType, encoding)
        {
        }

        public override string Signature(string value, string key = "") => SHA256HashingProvider.Signature(value, OutType, Encoding);
    }
}