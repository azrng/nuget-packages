using Common.SecurityCrypto.Extensions;
using Common.SecurityCrypto.Model;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Text;

namespace Common.SecurityCrypto.InternationalCrypto
{
    public sealed class ThreeDESEncryptionProvider//: SymmetricEncryptionBase
    {
        private ThreeDESEncryptionProvider()
        {
        }

        public static string Encrypt(string value, string key,
          OutType outType = OutType.Base64,
          Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            encoding ??= Encoding.UTF8;

            var inCipher = CreateCipher(true, key);
            var inputArray = encoding.GetBytes(value);
            byte[] cipherData = inCipher.DoFinal(inputArray);

            return outType == OutType.Base64 ? Convert.ToBase64String(cipherData) : cipherData.ToHexString();
        }

        public static string Decrypt(string value, string key,
          OutType outType = OutType.Base64,
          Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            encoding ??= Encoding.UTF8;

            var inputArrary = outType == OutType.Base64 ? Convert.FromBase64String(value) : value.ToBytes();
            var outCipher = CreateCipher(false, key);
            var encryptedDataStream = new MemoryStream(inputArrary, false);
            var dataStream = new MemoryStream();
            var outCipherStream = new CipherStream(dataStream, null, outCipher);
            int ch;
            while ((ch = encryptedDataStream.ReadByte()) >= 0)
            {
                outCipherStream.WriteByte((byte)ch);
            }
            outCipherStream.Close();
            encryptedDataStream.Close();
            var dataBytes = dataStream.ToArray();
            return encoding.GetString(dataBytes);
        }

        /// <summary>
        ///加解密
        /// </summary>
        /// <param name="forEncryption">为true表示加密 false解密</param>
        /// <param name="key"></param>
        /// <param name="cipMode"></param>
        /// <returns></returns>
        private static IBufferedCipher CreateCipher(bool forEncryption, string key, string cipMode = "DESede/ECB/PKCS5Padding")
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