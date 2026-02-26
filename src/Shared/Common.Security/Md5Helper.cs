using Common.Security.Enums;
using Common.Security.Extensions;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Common.Security
{
    /// <summary>
    /// md5哈希算法
    /// </summary>
    public static class Md5Helper
    {
        /// <summary>
        /// 获取字符串MD5值
        /// </summary>
        /// <param name="plaintext">字符串</param>
        /// <param name="is16">是否要16位的</param>
        /// <param name="outputType">输出类型(只有32位支持)</param>
        /// <returns></returns>
        public static string GetMd5Hash(string plaintext, bool is16 = false, OutType outputType = OutType.Hex)
        {
            if (plaintext == null)
            {
                throw new ArgumentNullException(nameof(plaintext));
            }

#if NET6_0_OR_GREATER
            // .net6以上还可以使用该方式进行md5哈希 MD5.HashData(Encoding.UTF8.GetBytes(aa))
            var inputBytes = Encoding.UTF8.GetBytes(plaintext);
            var hashResultNew = MD5.HashData(inputBytes);
            return is16
                ? Convert.ToHexString(hashResultNew, 4, 8)
                : hashResultNew.GetString(outputType);
#else
            using var md5 = MD5.Create();
            var buffer = Encoding.UTF8.GetBytes(plaintext);
            var hashResult = md5.ComputeHash(buffer);
            return is16
                ? BitConverter.ToString(hashResult, 4, 8).Replace("-", string.Empty)
                : hashResult.GetString(outputType);
#endif
        }

        /// <summary>
        /// 获取字符串HmacMd5Hash值
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="secret">密钥</param>
        /// <param name="outputType">输出类型</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string GetHmacMd5Hash(string plaintext, string secret, OutType outputType = OutType.Hex)
        {
            if (plaintext is null)
                throw new ArgumentNullException(nameof(plaintext));

            if (secret is null)
                throw new ArgumentNullException(nameof(secret));

            using var sha = new HMACMD5();
            sha.Key = Encoding.UTF8.GetBytes(secret);

            var buffer = Encoding.UTF8.GetBytes(plaintext);
            var hashResult = sha.ComputeHash(buffer);
            return hashResult.GetString(outputType);
        }

        /// <summary>
        /// 获取文件mdm值
        /// </summary>
        /// <param name="path"></param>
        /// <param name="outputType">输出类型</param>
        /// <returns></returns>
        public static string GetFileMd5Hash(string path, OutType outputType = OutType.Hex)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            using var md5 = MD5.Create();
            using var stream = File.OpenRead(path);
            var hash = md5.ComputeHash(stream);
            return hash.GetString(outputType);
        }
    }
}
