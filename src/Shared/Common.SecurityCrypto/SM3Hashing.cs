using Common.SecurityCrypto.Model;
using Common.SecurityCrypto.SMCrypto;
using System.Text;

namespace Common.SecurityCrypto
{
    /// <summary>
    /// sm2加密
    /// </summary>
    public class SM3Hashing : AbsHashingProvider
    {
        public SM3Hashing()
          : this(encoding: Encoding.UTF8)
        {
        }

        public SM3Hashing(OutType outType = OutType.Hex, Encoding encoding = null)
          : base(outType, encoding)
        {
        }

        public override string Signature(string value, string key = "") => SM3.Hash(value);
    }
}