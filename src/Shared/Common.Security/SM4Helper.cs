using Common.Security.Enums;
using Common.Security.Extensions;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Text;

namespace Common.Security
{
    /// <summary>
    /// Sm4 对称加密算法
    /// </summary>
    /// <remarks>
    /// 对标AES(取代DES)、DES算法
    /// 加密和解密结构相同，只不过，解密密钥是加密密钥的逆序
    /// 可以通过该网站在线生成：https://lzltool.cn/SM4
    /// </remarks>
    public static class Sm4Helper
    {
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="plaintext">加密的内容</param>
        /// <param name="secretKey">加密的key(文本模式十六位)</param>
        /// <param name="sm4CryptoEnum">加密类型</param>
        /// <param name="iv">十六位文本</param>
        /// <param name="outType">输出格式</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string Encrypt(string plaintext, string secretKey,
            Sm4CryptoEnum sm4CryptoEnum = Sm4CryptoEnum.ECB,
            string iv = "", OutType outType = OutType.Base64, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(plaintext))
                throw new ArgumentNullException(nameof(plaintext));
            if (string.IsNullOrEmpty(secretKey))
                throw new ArgumentNullException(nameof(secretKey));
            if (sm4CryptoEnum == Sm4CryptoEnum.CBC && string.IsNullOrWhiteSpace(iv))
                throw new ArgumentNullException(nameof(iv));

            encoding ??= Encoding.UTF8;

            var plaintextBytes = encoding.GetBytes(plaintext);
            var secretBytes = encoding.GetBytes(secretKey);

            var engine = new SM4Engine();
            var secretParameter = new KeyParameter(secretBytes);

            if (sm4CryptoEnum == Sm4CryptoEnum.ECB)
            {
                var cipher = new PaddedBufferedBlockCipher(new EcbBlockCipher(engine));
                cipher.Init(true, secretParameter);
                var encryptedBytes = cipher.DoFinal(plaintextBytes);
                return encryptedBytes.GetString(outType);
            }
            else
            {
                var cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(engine));
                cipher.Init(true, new ParametersWithIV(secretParameter, encoding.GetBytes(iv)));
                var encryptedBytes = cipher.DoFinal(plaintextBytes);
                return encryptedBytes.GetString(outType);
            }
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="encryptedText">密文</param>
        /// <param name="secretKey">解密的key(文本模式十六位)</param>
        /// <param name="sm4CryptoEnum">解密的类型</param>
        /// <param name="iv">十六位文本</param>
        /// <param name="inputType">输入密文的类型</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string Decrypt(string encryptedText, string secretKey,
            Sm4CryptoEnum sm4CryptoEnum = Sm4CryptoEnum.ECB,
            string iv = "", OutType inputType = OutType.Base64, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(encryptedText))
                throw new ArgumentNullException(nameof(encryptedText));
            if (string.IsNullOrEmpty(secretKey))
                throw new ArgumentNullException(nameof(secretKey));
            if (sm4CryptoEnum == Sm4CryptoEnum.CBC && string.IsNullOrWhiteSpace(iv))
                throw new ArgumentNullException(nameof(iv));

            encoding ??= Encoding.UTF8;

            var encryptedTextBytes = encryptedText.GetBytes(inputType);
            var secretBytes = encoding.GetBytes(secretKey);

            var engine = new SM4Engine();
            var secretParameter = new KeyParameter(secretBytes);

            if (sm4CryptoEnum == Sm4CryptoEnum.ECB)
            {
                var cipher = new PaddedBufferedBlockCipher(new EcbBlockCipher(engine));
                cipher.Init(false, secretParameter);
                var decryptedBytes = cipher.DoFinal(encryptedTextBytes);
                return encoding.GetString(decryptedBytes);
            }
            else
            {
                var cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(engine));
                cipher.Init(false, new ParametersWithIV(secretParameter, encoding.GetBytes(iv)));
                var decryptedBytes = cipher.DoFinal(encryptedTextBytes);
                return encoding.GetString(decryptedBytes);
            }
        }
    }
}