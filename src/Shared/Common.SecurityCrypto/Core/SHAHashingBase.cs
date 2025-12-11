using Common.SecurityCrypto.Extensions;
using Common.SecurityCrypto.Model;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Common.SecurityCrypto.Core
{
    public abstract class SHAHashingBase
    {
        protected static string Encrypt<T>(string value, Encoding encoding = null, OutType outType = OutType.Hex) where T : HashAlgorithm, new()
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (encoding == null)
                encoding = Encoding.UTF8;
            using (var hashAlgorithm = (HashAlgorithm)new T())
            {
                var hash = hashAlgorithm.ComputeHash(encoding.GetBytes(value));
                return outType == OutType.Base64 ? Convert.ToBase64String(hash) : hash.ToHexString();
            }
        }
    }
}