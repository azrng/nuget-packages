using Common.SecurityCrypto.Core;
using Common.SecurityCrypto.Extensions;
using Common.SecurityCrypto.Model;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Common.SecurityCrypto.InternationalCrypto
{
    public class SHA512HashingProvider : HMACHashingBase
    {
        private SHA512HashingProvider()
        {
        }

        public static string Signature(string value, OutType outType = OutType.Hex, Encoding encoding = null)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            encoding ??= Encoding.UTF8;

            using var sha = SHA512.Create();
            var buffer = encoding.GetBytes(value);
            var hashResult = sha.ComputeHash(buffer);
            return outType == OutType.Base64 ? Convert.ToBase64String(hashResult) : hashResult.ToHexString();
        }

        public static bool Verify(
          string comparison,
          string value,
          OutType outType = OutType.Hex,
          Encoding encoding = null)
        {
            return comparison == Signature(value, outType, encoding);
        }
    }
}