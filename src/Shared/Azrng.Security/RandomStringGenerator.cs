using System;
using System.Security.Cryptography;

namespace Azrng.Security
{
    /// <summary>
    /// 安全随机字符串生成器。吸收自 Common.SecurityCrypto 并重写：
    /// 原实现使用 RNGCryptoServiceProvider（已过时）+ 可预测的 new Random(seed)，
    /// 此处改为基于 RandomNumberGenerator 的密码学安全随机源。
    /// </summary>
    public static class RandomStringGenerator
    {
        private const string Dictionary =
            "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

        public static string Generate(int length = 8)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            var chars = new char[length];
            Span<byte> buffer = length <= 128 ? stackalloc byte[length] : new byte[length];
            RandomNumberGenerator.Fill(buffer);

            for (var i = 0; i < length; i++)
                chars[i] = Dictionary[buffer[i] % Dictionary.Length];

            return new string(chars);
        }
    }
}
