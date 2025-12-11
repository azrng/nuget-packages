using Common.Security.Enums;
using Common.Security.Extensions;
using System;
using System.Text;

namespace Common.Security
{
    /// <summary>
    /// sm3哈希算法
    /// </summary>
    /// <remarks>
    /// 和该网站互相测试：https://lzltool.cn/SM3
    /// </remarks>
    public static class Sm3Helper
    {
        /// <summary>
        /// 获取字符串Sm3哈希值
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="outputType">输出类型</param>
        /// <param name="encoding">编码类型</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string GetSm3Hash(string plaintext, OutType outputType = OutType.Hex,
                                        Encoding encoding = null)
        {
            if (plaintext == null)
            {
                throw new ArgumentNullException(nameof(plaintext));
            }

            encoding ??= Encoding.UTF8;

            var plaintextBytes = encoding.GetBytes(plaintext);

            var digest = new Org.BouncyCastle.Crypto.Digests.SM3Digest();
            var hashBytes = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(plaintextBytes, 0, plaintextBytes.Length);
            digest.DoFinal(hashBytes, 0);

            return hashBytes.GetString(outputType);
        }
    }
}