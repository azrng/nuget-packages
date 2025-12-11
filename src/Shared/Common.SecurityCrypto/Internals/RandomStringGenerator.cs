using System;
using System.Security.Cryptography;
using System.Text;

namespace Common.SecurityCrypto.Internals
{
    public static class RandomStringGenerator
    {
        private static readonly int StringDictionaryLength = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~".Length;
        private const string STRING_DICTIONARY = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

        public static string Generate(int bits = 8)
        {
            var stringBuilder = new StringBuilder();
            var data = new byte[4];
            using (var cryptoServiceProvider = new RNGCryptoServiceProvider())
                cryptoServiceProvider.GetBytes(data);
            var random = new Random(BitConverter.ToInt32(data, 0));
            for (var index = 0; index < bits; ++index)
                stringBuilder.Append("0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~".Substring(random.Next(0, StringDictionaryLength), 1));
            return stringBuilder.ToString();
        }
    }
}