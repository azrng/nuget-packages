using Common.SecurityCrypto.Extensions;
using Common.SecurityCrypto.Hash;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Common.SecurityCrypto.InternationalCrypto
{
    public static class MD5HashingProvider
    {
        public static string Signature(string value, MD5BitType bitType = MD5BitType.L32, Encoding encoding = null)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            encoding ??= Encoding.UTF8;
            return bitType switch
            {
                MD5BitType.L16 => Encrypt16Func()(value)(encoding),
                MD5BitType.L32 => Encrypt32Func()(value)(encoding),
                MD5BitType.L64 => Encrypt64Func()(value)(encoding),
                _ => throw new ArgumentOutOfRangeException(nameof(bitType), (object)bitType, null),
            };
        }

        private static Func<string, Func<Encoding, byte[]>> PreencryptFunc() => (Func<string, Func<Encoding, byte[]>>)(str => (Func<Encoding, byte[]>)(encoding =>
        {
            using var md5 = MD5.Create();
            return md5.ComputeHash(encoding.GetBytes(str));
        }));

        private static Func<string, Func<Encoding, string>> Encrypt16Func() => (Func<string, Func<Encoding, string>>)(str => (Func<Encoding, string>)(encoding => BitConverter.ToString(PreencryptFunc()(str)(encoding), 4, 8).Replace("-", "")));

        private static Func<string, Func<Encoding, string>> Encrypt32Func() => (Func<string, Func<Encoding, string>>)(str => (Func<Encoding, string>)(encoding => PreencryptFunc()(str)(encoding).ToHexString()));

        private static Func<string, Func<Encoding, string>> Encrypt64Func() => (Func<string, Func<Encoding, string>>)(str => (Func<Encoding, string>)(encoding => Convert.ToBase64String(PreencryptFunc()(str)(encoding))));

        public static bool Verify(
          string comparison,
          string value,
          MD5BitType bitType = MD5BitType.L32,
          Encoding encoding = null)
        {
            return comparison == Signature(value, bitType, encoding);
        }
    }
}