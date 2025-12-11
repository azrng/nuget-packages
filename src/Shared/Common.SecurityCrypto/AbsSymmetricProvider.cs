using Common.SecurityCrypto.Model;
using System.Text;

namespace Common.SecurityCrypto
{
    public abstract class AbsSymmetricProvider : ISymmetricProvider
    {
        public OutType OutType { get; set; }

        public Encoding Encoding { get; set; }

        public AbsSymmetricProvider(OutType outType = OutType.Hex, Encoding encoding = null)
        {
            OutType = outType;
            if (encoding == null)
                encoding = Encoding.UTF8;
            Encoding = encoding;
        }

        public abstract SymmetricKey CreateKey();

        public abstract string Encrypt(string value, string key, string iv = null);

        public abstract string Decrypt(string value, string key, string iv = null);
    }
}