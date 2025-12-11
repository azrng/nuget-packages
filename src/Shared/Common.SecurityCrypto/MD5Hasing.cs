using Common.SecurityCrypto.Hash;
using Common.SecurityCrypto.InternationalCrypto;
using Common.SecurityCrypto.Model;
using System.Text;

namespace Common.SecurityCrypto
{
    public class MD5Hasing : AbsHashingProvider
    {
        public MD5Hasing()
          : this(encoding: Encoding.UTF8)
        {
        }

        public MD5Hasing(OutType outType = OutType.Hex, Encoding encoding = null)
          : base(outType, encoding)
        {
        }

        public override string Signature(string value, string key = "")
        {
            switch (OutType)
            {
                case OutType.Base64:
                    return MD5HashingProvider.Signature(value, MD5BitType.L64, Encoding);

                case OutType.Hex:
                    return MD5HashingProvider.Signature(value, encoding: Encoding);

                default:
                    return MD5HashingProvider.Signature(value, MD5BitType.L64, Encoding);
            }
        }
    }
}