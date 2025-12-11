using Common.Security.Enums;
using Common.Security.Extensions;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Security;
using System.IO;
using System.Text;

namespace Common.Security
{
    /// <summary>
    /// 3DES加密  3DES（也称为 TDEA，代表三重数据加密算法）是已发布的 DES 算法的升级版本。3DES 的开发是为了克服 DES 算法的缺点，并于 1990年代后期开始投入使用
    /// 到 2023年之后，将在所有新应用程序中废弃 3DES 的使用
    /// </summary>
    public class Des3Helper
    {
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="secretKey"></param>
        /// <param name="outType"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string Encrypt(string plaintext, string secretKey, OutType outType = OutType.Base64,
            Encoding encoding = null)
        {
            _ = encoding ?? Encoding.UTF8;
            var inCipher = CreateCipher(true, secretKey);
            var inputArray = encoding.GetBytes(plaintext);
            var cipherData = inCipher.DoFinal(inputArray);
            return cipherData.GetString(outType);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="source"></param>
        /// <param name="secretKey"></param>
        /// <param name="inputType">输入密文的类型</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string Decrypt(string source, string secretKey, OutType inputType = OutType.Base64,
            Encoding encoding = null)
        {
            _ = encoding ?? Encoding.UTF8;
            var inputArray = source.GetBytes(inputType);
            var outCipher = CreateCipher(false, secretKey);
            using var encryptedDataStream = new MemoryStream(inputArray, false);
            using var dataStream = new MemoryStream();
            using var outCipherStream = new CipherStream(dataStream, null, outCipher);
            int ch;
            while ((ch = encryptedDataStream.ReadByte()) >= 0)
            {
                outCipherStream.WriteByte((byte)ch);
            }

            var dataBytes = dataStream.ToArray();
            return encoding.GetString(dataBytes);
        }

        /// <summary>
        /// 加解密
        /// </summary>
        /// <param name="forEncryption">为true表示加密 false解密</param>
        /// <param name="key"></param>
        /// <param name="cipMode"></param>
        /// <returns></returns>
        private IBufferedCipher CreateCipher(bool forEncryption, string key, string cipMode = "DESede/ECB/PKCS5Padding")
        {
            var algorithmName = cipMode;
            if (cipMode.IndexOf('/') >= 0)
            {
                algorithmName = cipMode.Substring(0, cipMode.IndexOf('/'));
            }

            var cipher = CipherUtilities.GetCipher(cipMode);
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var keyParameter = ParameterUtilities.CreateKeyParameter(algorithmName, keyBytes);
            cipher.Init(forEncryption, keyParameter);
            return cipher;
        }
    }
}