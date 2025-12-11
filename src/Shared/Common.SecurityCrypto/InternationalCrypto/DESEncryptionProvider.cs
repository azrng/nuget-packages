using Common.SecurityCrypto.Core;
using Common.SecurityCrypto.Extensions;
using Common.SecurityCrypto.Internals;
using Common.SecurityCrypto.Model;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Common.SecurityCrypto.InternationalCrypto
{
    public sealed class DESEncryptionProvider : SymmetricEncryptionBase
    {
        private DESEncryptionProvider()
        {
        }

        public static DESKey CreateKey() => new DESKey
        {
            Key = RandomStringGenerator.Generate(),
            IV = RandomStringGenerator.Generate()
        };

        public static string Encrypt(
          string value,
          string key,
          string iv,
          string salt = null,
          OutType outType = OutType.Base64,
          Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrEmpty(iv))
                throw new ArgumentNullException(nameof(iv));
            if (encoding == null)
                encoding = Encoding.UTF8;
            var numArray = SymmetricEncryptionBase.EncryptCore<DESCryptoServiceProvider>(encoding.GetBytes(value), ComputeRealValueFunc()(key)(salt)(encoding)(64), ComputeRealValueFunc()(iv)(salt)(encoding)(64));
            return outType == OutType.Base64 ? Convert.ToBase64String(numArray) : numArray.ToHexString();
        }

        public static string Encrypt(string value, DESKey key, OutType outType = OutType.Base64, Encoding encoding = null)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            return Encrypt(value, key.Key, key.IV, outType: outType, encoding: encoding);
        }

        public static string Decrypt(
          string value,
          string key,
          string iv = null,
          string salt = null,
          OutType outType = OutType.Base64,
          Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (encoding == null)
                encoding = Encoding.UTF8;
            var bytes = SymmetricEncryptionBase.DecryptCore<DESCryptoServiceProvider>(value.GetEncryptBytes(outType), ComputeRealValueFunc()(key)(salt)(encoding)(64), ComputeRealValueFunc()(iv)(salt)(encoding)(64));
            return encoding.GetString(bytes);
        }

        public static string Decrypt(string value, DESKey key, OutType outType = OutType.Base64, Encoding encoding = null)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            return Decrypt(value, key.Key, key.IV, outType: outType, encoding: encoding);
        }
    }
}