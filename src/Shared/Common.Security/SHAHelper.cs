using Common.Security.Enums;
using Common.Security.Extensions;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Common.Security
{
    /// <summary>
    /// SHA哈希
    /// </summary>
    /// <remarks>
    /// 在线测试网址：https://www.lddgo.net/encrypt/hmac
    /// </remarks>
    public static class ShaHelper
    {
        /// <summary>
        /// 获取字符串sha1值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="outputType">输出类型</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string GetSha1Hash(string str, OutType outputType = OutType.Hex)
        {
            ValidateInput(str);
            return ComputeHash(str, SHA1.Create(), outputType);
        }

        /// <summary>
        /// 获取字符串HMACSHA1值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="secret">密钥</param>
        /// <param name="outputType">输出类型</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string GetHmacSha1Hash(string str, string secret, OutType outputType = OutType.Hex)
        {
            ValidateInput(str, secret);
            using var sha = new HMACSHA1(Encoding.UTF8.GetBytes(secret));
            return ComputeHash(str, sha, outputType);
        }

        /// <summary>
        /// 获取字符串sha256值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="outputType">输出类型</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string GetSha256Hash(string str, OutType outputType = OutType.Hex)
        {
            ValidateInput(str);
            return ComputeHash(str, SHA256.Create(), outputType);
        }

        /// <summary>
        /// 获取字符串HMACSHA256值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="secret">密钥</param>
        /// <param name="outputType">输出类型</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string GetHmacSha256Hash(string str, string secret, OutType outputType = OutType.Hex)
        {
            ValidateInput(str, secret);
            using var sha = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            return ComputeHash(str, sha, outputType);
        }

        /// <summary>
        /// 获取文件的sha256的值
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="outputType">输出类型</param>
        /// <returns></returns>
        public static string GetSha256Hash(Stream stream, OutType outputType = OutType.Hex)
        {
            using var sha256Hash = SHA256.Create();
            var hashResult = sha256Hash.ComputeHash(stream);
            return hashResult.GetString(outputType);
        }

        /// <summary>
        /// 获取字符串sha512值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="outputType">输出类型</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string GetSha512Hash(string str, OutType outputType = OutType.Hex)
        {
            ValidateInput(str);
            return ComputeHash(str, SHA512.Create(), outputType);
        }

        /// <summary>
        /// 获取字符串HMACSHA512值
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="secret">密钥</param>
        /// <param name="outputType">输出类型</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string GetHmacSha512Hash(string plaintext, string secret, OutType outputType = OutType.Hex)
        {
            ValidateInput(plaintext, secret);
            using var sha = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
            return ComputeHash(plaintext, sha, outputType);
        }

        /// <summary>
        /// 获取文件的sha512的值
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="outputType">输出类型</param>
        /// <returns></returns>
        public static string GetSha512Hash(Stream stream, OutType outputType = OutType.Hex)
        {
            using var sha512Hash = SHA512.Create();
            var hashResult = sha512Hash.ComputeHash(stream);
            return hashResult.GetString(outputType);
        }

        /// <summary>
        /// 验证输入参数
        /// </summary>
        /// <param name="str"></param>
        /// <param name="secret"></param>
        private static void ValidateInput(string str, string secret = null)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            if (secret != null && secret == null)
                throw new ArgumentNullException(nameof(secret));
        }

        /// <summary>
        /// 计算哈希值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="hashAlgorithm"></param>
        /// <param name="outputType"></param>
        /// <returns></returns>
        private static string ComputeHash(string str, HashAlgorithm hashAlgorithm, OutType outputType)
        {
            var buffer = Encoding.UTF8.GetBytes(str);
            var hashResult = hashAlgorithm.ComputeHash(buffer);
            return hashResult.GetString(outputType);
        }
    }
}