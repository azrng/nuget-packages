using Common.SecurityCrypto.Extensions;
using Common.SecurityCrypto.Model;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Common.SecurityCrypto.Core
{
    public abstract class HMACHashingBase
    {
        protected static string Encrypt<T>(string value, string key, Encoding encoding = null, OutType outType = OutType.Hex)
            where T : KeyedHashAlgorithm, new()
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            encoding ??= Encoding.UTF8;

            using var keyedHashAlgorithm = (KeyedHashAlgorithm)new T();
            keyedHashAlgorithm.Key = encoding.GetBytes(key);
            var hash = keyedHashAlgorithm.ComputeHash(encoding.GetBytes(value));
            return outType == OutType.Base64 ? Convert.ToBase64String(hash) : hash.ToHexString();
        }
    }
}