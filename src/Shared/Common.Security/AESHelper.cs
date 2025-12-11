using Common.Security.Enums;
using Common.Security.Extensions;
using System;
using System.IO;
using System.Security.Cryptography;

namespace Common.Security
{
    /// <summary>
    /// AES对称加解密算法助手类
    /// </summary>
    /// <remarks>
    /// https://zhuanlan.zhihu.com/p/131324301
    /// 网站互认：https://lzltool.cn/AES
    /// </remarks>
    public static class AesHelper
    {
        /// <summary>
        /// Aes密钥产生
        /// <param name="outType">输出密钥的类型</param>
        /// </summary>
        public static (string, string) ExportSecretAndIv(OutType outType = OutType.Base64)
        {
            using var aes = Aes.Create();
            aes.GenerateKey();
            var secretKey = aes.Key.GetString(outType);

            aes.GenerateIV();
            var iv = aes.IV.GetString(outType);

            return (secretKey, iv);
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="plaintext">要加密的文本</param>
        /// <param name="secretKey">密钥</param>
        /// <param name="iv">iv</param>
        /// <param name="cipherMode">模式</param>
        /// <param name="paddingMode">填充</param>
        /// <param name="secretType">密钥类型</param>
        /// <param name="outType">输出类型</param>
        /// <remarks>对key和iv有要求  示例："lB2BxrJdI4UUjK3KEZyQ0obuSgavB1SYJuAFq9oVw0Y=", "6lra6ceX26Fazwj1R4PCOg=="</remarks>
        /// <returns></returns>
        public static string Encrypt(string plaintext, string secretKey, string iv = "",
                                     CipherMode cipherMode = CipherMode.ECB, PaddingMode paddingMode = PaddingMode.None,
                                     SecretType secretType = SecretType.Base64,
                                     OutType outType = OutType.Base64)
        {
            if (string.IsNullOrEmpty(plaintext))
                throw new ArgumentNullException(nameof(plaintext));
            if (cipherMode != CipherMode.ECB && string.IsNullOrWhiteSpace(iv))
                throw new ArgumentException("IV不能为空");
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException("密钥不能为空");

            using var mStream = new MemoryStream();
            using var aes = Aes.Create();
            aes.Mode = cipherMode;
            aes.Padding = paddingMode;
            aes.Key = secretKey.GetBytes(secretType);
            if (cipherMode != CipherMode.ECB)
                aes.IV = iv.GetBytes(secretType);
            var transform = cipherMode == CipherMode.ECB ? aes.CreateEncryptor() : aes.CreateEncryptor(aes.Key, aes.IV);
            using var cryptoStream = new CryptoStream(mStream, transform, CryptoStreamMode.Write);
            using (var writer = new StreamWriter(cryptoStream))
            {
                writer.Write(plaintext);
            }

            return mStream.ToArray().GetString(outType);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="cipherText">要解密的文本</param>
        /// <param name="secretKey">密钥</param>
        /// <param name="iv"></param>
        /// <param name="cipherMode">模式</param>
        /// <param name="paddingMode">填充</param>
        /// <param name="secretType">密钥类型</param>
        /// <param name="cipherTextType">输入密文的类型</param>
        /// <remarks>对key和iv有要求  示例："lB2BxrJdI4UUjK3KEZyQ0obuSgavB1SYJuAFq9oVw0Y=", "6lra6ceX26Fazwj1R4PCOg=="</remarks>
        /// <returns></returns>
        public static string Decrypt(string cipherText, string secretKey, string iv = "",
                                     CipherMode cipherMode = CipherMode.ECB,
                                     PaddingMode paddingMode = PaddingMode.None, SecretType secretType = SecretType.Base64,
                                     OutType cipherTextType = OutType.Base64)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException(nameof(cipherText));
            if (cipherMode != CipherMode.ECB && string.IsNullOrWhiteSpace(iv))
                throw new ArgumentException("IV不能为空");
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException("密钥不能为空");

            var data = cipherText.GetBytes(cipherTextType);
            using var mStream = new MemoryStream(data);
            using var aes = Aes.Create();
            aes.Mode = cipherMode;
            aes.Padding = paddingMode;
            aes.Key = secretKey.GetBytes(secretType);

            if (cipherMode != CipherMode.ECB)
                aes.IV = iv.GetBytes(secretType);

            var transform = cipherMode == CipherMode.ECB ? aes.CreateDecryptor() : aes.CreateDecryptor(aes.Key, aes.IV);
            using var crypto = new CryptoStream(mStream, transform, CryptoStreamMode.Read);
            using var reader = new StreamReader(crypto);
            return reader.ReadToEnd();
        }
    }
}