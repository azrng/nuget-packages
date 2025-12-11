using Common.SecurityCrypto.Interfrace;
using Common.SecurityCrypto.Model;
using System.Text;

namespace Common.SecurityCrypto
{
    public abstract class AbsHashingProvider : IHashingProvider
    {
        public OutType OutType { get; set; }

        public Encoding Encoding { get; set; }

        public AbsHashingProvider(OutType outType = OutType.Hex, Encoding encoding = null)
        {
            OutType = outType;
            if (encoding == null)
                encoding = Encoding.UTF8;
            Encoding = encoding;
        }

        public abstract string Signature(string value, string key = "");

        public virtual bool Verify(string comparison, string value, string key) => comparison == Signature(value, key);
    }
}