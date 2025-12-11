using Common.Security.Enums;
using Common.Security.Extensions;
using Common.Security.Model;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Common.Security
{
    /// <summary>
    /// DES加密  1976年推出，是最古老的对称加密方法之一   在 2005年，DES 被正式弃用，并被 AES 加密算法所取代
    /// DES 全称为 Data Encryption Standard，即数据加密标准，是一种使用密钥加密的块算法
    /// 在线网站：https://lzltool.cn/DES
    /// </summary>
    public class DesHelper : SymmetricEncryptionBase
    {
        /// <summary>
        /// ecb模式
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="secretKey">标准密钥为8位以上</param>
        /// <param name="outType"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string Encrypt(string plaintext, string secretKey, OutType outType = OutType.Base64,
            Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(plaintext))
                throw new ArgumentNullException(nameof(plaintext));
            if (string.IsNullOrEmpty(secretKey))
                throw new ArgumentNullException(nameof(secretKey));
            encoding ??= Encoding.UTF8;
            var numArray = EncryptCore<DESCryptoServiceProvider>(encoding.GetBytes(plaintext),
                ComputeRealValue(secretKey, encoding));
            return numArray.GetString(outType);
        }

        /// <summary>
        /// ecb模式
        /// </summary>
        /// <param name="source"></param>
        /// <param name="secretKey">标准密钥为8位以上</param>
        /// <param name="outType">输出类型</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string Decrypt(string source, string secretKey, OutType outType = OutType.Base64,
            Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(secretKey))
                throw new ArgumentNullException(nameof(secretKey));
            encoding ??= Encoding.UTF8;
            var bytes = DecryptCore<DESCryptoServiceProvider>(source.GetBytes(outType),
                ComputeRealValue(secretKey, encoding));
            return encoding.GetString(bytes);
        }

        /// <summary>
        /// des 处理
        /// </summary>
        /// <param name="originString"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        private static byte[] ComputeRealValue(string originString, Encoding encoding)
        {
            if (string.IsNullOrWhiteSpace(originString))
                return new byte[1];
            encoding ??= Encoding.UTF8;
            const int length = 8;
            var destinationArray = new byte[length];
            Array.Copy(encoding.GetBytes(originString.PadRight(length)), destinationArray, length);
            return destinationArray;
        }
    }
}