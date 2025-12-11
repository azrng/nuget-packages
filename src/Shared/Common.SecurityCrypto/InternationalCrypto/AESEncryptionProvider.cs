using Common.SecurityCrypto.Core;
using Common.SecurityCrypto.Extensions;
using Common.SecurityCrypto.Model;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Common.SecurityCrypto.InternationalCrypto
{
    public sealed class AESEncryptionProvider : SymmetricEncryptionBase
    {
        private AESEncryptionProvider()
        {
        }

        public static AESKey CreateKey(AESKeySizeType size = AESKeySizeType.L256, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            var cryptoServiceProvider1 = new AesCryptoServiceProvider();
            cryptoServiceProvider1.KeySize = (int)size;
            using var cryptoServiceProvider2 = cryptoServiceProvider1;
            return new AESKey
            {
                Key = encoding.GetString(cryptoServiceProvider2.Key),
                IV = encoding.GetString(cryptoServiceProvider2.IV),
                Size = size
            };
        }

        public static string Encrypt(
          string value,
          string key,
          string iv = null,
          string salt = null,
          OutType outType = OutType.Base64,
          Encoding encoding = null,
          AESKeySizeType keySize = AESKeySizeType.L256)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (encoding == null)
                encoding = Encoding.UTF8;
            var numArray = SymmetricEncryptionBase.EncryptCore<AesCryptoServiceProvider>(encoding.GetBytes(value), ComputeRealValueFunc()(key)(salt)(encoding)((int)keySize), ComputeRealValueFunc()(iv)(salt)(encoding)(128));
            return outType == OutType.Base64 ? Convert.ToBase64String(numArray) : numArray.ToHexString();
        }

        public static string Encrypt(string value, AESKey key, OutType outType = OutType.Base64, Encoding encoding = null)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            return Encrypt(value, key.Key, key.IV, outType: outType, encoding: encoding, keySize: key.Size);
        }

        public static string Decrypt(
          string value,
          string key,
          string iv = null,
          string salt = null,
          OutType outType = OutType.Base64,
          Encoding encoding = null,
          AESKeySizeType keySize = AESKeySizeType.L256)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (encoding == null)
                encoding = Encoding.UTF8;
            var bytes = SymmetricEncryptionBase.DecryptCore<AesCryptoServiceProvider>(value.GetEncryptBytes(outType), ComputeRealValueFunc()(key)(salt)(encoding)((int)keySize), ComputeRealValueFunc()(iv)(salt)(encoding)(128));
            return encoding.GetString(bytes);
        }

        public static string Decrypt(string value, AESKey key, OutType outType = OutType.Base64, Encoding encoding = null)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            return Decrypt(value, key.Key, key.IV, outType: outType, encoding: encoding, keySize: key.Size);
        }
    }
}