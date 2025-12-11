using Common.SecurityCrypto.Model;
using System.Text;

namespace Common.SecurityCrypto
{
    public interface ISymmetricProvider
    {
        OutType OutType { get; set; }

        Encoding Encoding { get; set; }

        SymmetricKey CreateKey();

        string Encrypt(string value, string key, string iv = null);

        string Decrypt(string value, string key, string iv = null);
    }
}