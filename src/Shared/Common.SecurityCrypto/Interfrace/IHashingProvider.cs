using Common.SecurityCrypto.Model;
using System.Text;

namespace Common.SecurityCrypto.Interfrace
{
    public interface IHashingProvider
    {
        OutType OutType { get; set; }

        Encoding Encoding { get; set; }

        string Signature(string value, string key);

        bool Verify(string comparison, string value, string key);
    }
}