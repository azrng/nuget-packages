using Common.Security.Enums;
using Common.Security.Extensions;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Common.Security
{
    /// <summary>
    /// AES GCM对称加解密算法助手类
    /// </summary>
    /// <remarks>
    /// https://zhuanlan.zhihu.com/p/131324301
    /// 网站互认：https://lzltool.cn/AES
    /// </remarks>
    public static class AesGcmHelper
    {
        // 配置常量
        public const int DefaultTagSize = 16; // 128位标签（推荐）
        public const int MinTagSize = 12; // 96位标签（最小安全值）
        public const int MaxTagSize = 16; // 128位标签（最大）
        public const int NonceSize = 12; // 96位Nonce（推荐）
        private static readonly int[] ValidAesKeySizes = { 16, 24, 32 };

        /// <summary>
        /// 组合字节数组：Nonce + Cipher + Tag
        /// </summary>
        private static byte[] Combine(byte[] nonce, byte[] cipher, byte[] tag)
        {
            var combined = new byte[nonce.Length + cipher.Length + tag.Length];
            Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
            Buffer.BlockCopy(cipher, 0, combined, nonce.Length, cipher.Length);
            Buffer.BlockCopy(tag, 0, combined, nonce.Length + cipher.Length, tag.Length);
            return combined;
        }

        /// <summary>
        /// 拆分合并的字节数组
        /// </summary>
        private static (byte[] Nonce, byte[] Cipher, byte[] Tag) Split(byte[] combined, int tagSize)
        {
            if (combined == null || combined.Length < NonceSize + tagSize)
                throw new ArgumentException("密文长度不合法，无法拆分出 Nonce 与 Tag");

            var nonce = new byte[NonceSize];
            Buffer.BlockCopy(combined, 0, nonce, 0, NonceSize);

            var tag = new byte[tagSize];
            Buffer.BlockCopy(combined, combined.Length - tagSize, tag, 0, tagSize);

            var cipherLen = combined.Length - NonceSize - tagSize;
            if (cipherLen < 0)
                throw new ArgumentException("密文长度不合法，数据区长度为负");

            var cipher = new byte[cipherLen];
            Buffer.BlockCopy(combined, NonceSize, cipher, 0, cipherLen);

            return (nonce, cipher, tag);
        }

        private static void ValidateAesKey(byte[] key)
        {
            if (key == null || Array.IndexOf(ValidAesKeySizes, key.Length) < 0)
                throw new ArgumentException("AES key length must be 16, 24 or 32 bytes.");
        }

        /// <summary>
        /// AES-GCM 加密
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key">加密密钥</param>
        /// <param name="tagSize">认证标签大小（字节）</param>
        /// <returns>加密结果（包含密文、Nonce和标签）</returns>
        private static (byte[] CipherText, byte[] Nonce, byte[] Tag) Encrypt(
            string plainText,
            byte[] key,
            int tagSize = DefaultTagSize)
        {
            // 验证标签大小
            if (tagSize < MinTagSize || tagSize > MaxTagSize)
                throw new ArgumentOutOfRangeException(nameof(tagSize),
                    $"Tag size must be between {MinTagSize} and {MaxTagSize} bytes");

            // 生成随机Nonce
            var nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);

            // 准备输出缓冲区
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = new byte[plainBytes.Length];
            var tag = new byte[tagSize];

            ValidateAesKey(key);

            // 执行加密
            using (var aesGcm = new AesGcm(key))
            {
                aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);
            }

            return (cipherBytes, nonce, tag);
        }

        /// <summary>
        /// AES-GCM 加密（字符串密钥与输出），输出为合并后的单一字符串（Nonce+Cipher+Tag）
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="secretKey">加密密钥</param>
        /// <param name="secretType">密钥类型</param>
        /// <param name="outType">输出类型</param>
        /// <param name="tagSize">认证标签大小（字节）</param>
        /// <returns>合并后的密文字符串（Nonce + Cipher + Tag）</returns>
        public static string Encrypt(
            string plainText,
            string secretKey,
            SecretType secretType = SecretType.Base64,
            OutType outType = OutType.Base64,
            int tagSize = DefaultTagSize)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException("密钥不能为空");

            var key = secretKey.GetBytes(secretType);
            var (cipher, nonce, tag) = Encrypt(plainText, key, tagSize);
            var combined = Combine(nonce, cipher, tag);
            return combined.GetString(outType);
        }

        /// <summary>
        /// AES-GCM 加密（字符串密钥），分别返回三个部分（字符串编码）
        /// </summary>
        public static (string CipherText, string Nonce, string Tag) EncryptToParts(
            string plainText,
            string secretKey,
            SecretType secretType = SecretType.Base64,
            OutType outType = OutType.Base64,
            int tagSize = DefaultTagSize)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException("密钥不能为空");

            var key = secretKey.GetBytes(secretType);
            var (cipher, nonce, tag) = Encrypt(plainText, key, tagSize);
            return (cipher.GetString(outType), nonce.GetString(outType), tag.GetString(outType));
        }

        /// <summary>
        /// AES-GCM 解密
        /// </summary>
        public static string Decrypt(
            byte[] cipherText,
            byte[] nonce,
            byte[] tag,
            byte[] key,
            int tagSize = DefaultTagSize)
        {
            // 验证标签大小
            if (tagSize < MinTagSize || tagSize > MaxTagSize)
                throw new ArgumentOutOfRangeException(nameof(tagSize),
                    $"Tag size must be between {MinTagSize} and {MaxTagSize} bytes");

            // 准备输出缓冲区
            var plainBytes = new byte[cipherText.Length];

            // 执行解密
            ValidateAesKey(key);
            if (nonce == null || nonce.Length != NonceSize)
                throw new ArgumentException($"Nonce size must be {NonceSize} bytes.");
            if (tag == null || tag.Length != tagSize)
                throw new ArgumentException($"Tag size must be {tagSize} bytes.");

            using (var aesGcm = new AesGcm(key))
            {
                aesGcm.Decrypt(nonce, cipherText, tag, plainBytes);
            }

            return Encoding.UTF8.GetString(plainBytes);
        }

        /// <summary>
        /// AES-GCM 解密（输入为单一合并字符串：Nonce+Cipher+Tag）
        /// </summary>
        public static string Decrypt(
            string cipherCombined,
            string secretKey,
            SecretType secretType = SecretType.Base64,
            OutType cipherTextType = OutType.Base64,
            int tagSize = DefaultTagSize)
        {
            if (string.IsNullOrEmpty(cipherCombined))
                throw new ArgumentNullException(nameof(cipherCombined));
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException("密钥不能为空");
            if (tagSize < MinTagSize || tagSize > MaxTagSize)
                throw new ArgumentOutOfRangeException(nameof(tagSize),
                    $"Tag size must be between {MinTagSize} and {MaxTagSize} bytes");

            var data = cipherCombined.GetBytes(cipherTextType);
            var (nonce, cipher, tag) = Split(data, tagSize);
            var key = secretKey.GetBytes(secretType);
            return Decrypt(cipher, nonce, tag, key, tagSize);
        }

        /// <summary>
        /// AES-GCM 解密（三段字符串输入：Cipher、Nonce、Tag）
        /// </summary>
        public static string DecryptFromParts(
            string cipherText,
            string nonce,
            string tag,
            string secretKey,
            SecretType secretType = SecretType.Base64,
            OutType cipherTextType = OutType.Base64,
            int tagSize = DefaultTagSize)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException(nameof(cipherText));
            if (string.IsNullOrEmpty(nonce))
                throw new ArgumentNullException(nameof(nonce));
            if (string.IsNullOrEmpty(tag))
                throw new ArgumentNullException(nameof(tag));
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException("密钥不能为空");

            var cipher = cipherText.GetBytes(cipherTextType);
            var nonceBytes = nonce.GetBytes(cipherTextType);
            var tagBytes = tag.GetBytes(cipherTextType);
            var key = secretKey.GetBytes(secretType);
            return Decrypt(cipher, nonceBytes, tagBytes, key, tagSize);
        }

        /// <summary>
        /// Aes密钥产生
        /// <param name="outType">输出密钥的类型</param>
        /// </summary>
        [Obsolete("Use AesHelper.ExportSecretAndIv instead.")]
        public static (string, string) ExportSecretAndIv(OutType outType = OutType.Base64)
        {
            return AesHelper.ExportSecretAndIv(outType);
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
        [Obsolete("This overload is legacy AES (non-GCM). Use AesHelper.Encrypt/EncryptCbcPkcs7 or AesGcmHelper.Encrypt for GCM.")]
        public static string Encrypt(string plaintext, string secretKey, string iv = "",
                                     CipherMode cipherMode = CipherMode.ECB, PaddingMode paddingMode = PaddingMode.None,
                                     SecretType secretType = SecretType.Base64,
                                     OutType outType = OutType.Base64)
        {
            return AesHelper.Encrypt(plaintext, secretKey, iv, cipherMode, paddingMode, secretType, outType);
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
        [Obsolete("This overload is legacy AES (non-GCM). Use AesHelper.Decrypt/DecryptCbcPkcs7 or AesGcmHelper.Decrypt for GCM.")]
        public static string Decrypt(string cipherText, string secretKey, string iv = "",
                                     CipherMode cipherMode = CipherMode.ECB,
                                     PaddingMode paddingMode = PaddingMode.None, SecretType secretType = SecretType.Base64,
                                     OutType cipherTextType = OutType.Base64)
        {
            return AesHelper.Decrypt(cipherText, secretKey, iv, cipherMode, paddingMode, secretType, cipherTextType);
        }
    }
}
