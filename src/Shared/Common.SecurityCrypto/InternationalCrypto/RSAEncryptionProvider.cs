using Common.SecurityCrypto.Extensions;
using Common.SecurityCrypto.Model;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Common.SecurityCrypto.InternationalCrypto
{
    public static class RSAEncryptionProvider
    {
        public static RSAKey CreateKey(RSAKeySizeType size = RSAKeySizeType.L2048, RSAKeyType keyType = RSAKeyType.Xml)
        {
            using (var rsa = new RSACryptoServiceProvider((int)size))
            {
                var str1 = keyType == RSAKeyType.Json ? rsa.ToJsonString(false) : rsa.ToExtXmlString(false);
                var str2 = keyType == RSAKeyType.Json ? rsa.ToJsonString(true) : rsa.ToExtXmlString(true);
                return new RSAKey()
                {
                    PublickKey = str1,
                    PrivateKey = str2,
                    Exponent = rsa.ExportParameters(false).Exponent.ToHexString(),
                    Modulus = rsa.ExportParameters(false).Modulus.ToHexString()
                };
            }
        }

        public static RSA RsaFromString(string rsaKey, RSAKeyType keyType = RSAKeyType.Xml)
        {
            if (string.IsNullOrWhiteSpace(rsaKey))
                throw new ArgumentNullException(nameof(keyType));
            var rsa = RSA.Create();
            switch (keyType)
            {
                case RSAKeyType.Xml:
                    rsa.FromExtXmlString(rsaKey);
                    break;

                case RSAKeyType.Base64:
                    rsa.FromBase64StringByPrivateKey(rsaKey);
                    break;

                default:
                    rsa.FromJsonString(rsaKey);
                    break;
            }
            return rsa;
        }

        public static string GetPrivateKey(string certFile, string password)
        {
            if (!File.Exists(certFile))
                throw new FileNotFoundException(nameof(certFile));
            return new X509Certificate2(certFile, password, X509KeyStorageFlags.Exportable).PrivateKey.ToXmlString(true);
        }

        public static string GetPublicKey(string certFile)
        {
            if (!File.Exists(certFile))
                throw new FileNotFoundException(nameof(certFile));
            return new X509Certificate2(certFile).PublicKey.Key.ToXmlString(false);
        }

        public static string Encrypt(
          string value,
          string publicKey,
          Encoding encoding = null,
          OutType outType = OutType.Base64,
          RSAKeyType keyType = RSAKeyType.Xml)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            var numArray = Encrypt(encoding.GetBytes(value), publicKey, keyType);
            return outType == OutType.Base64 ? Convert.ToBase64String(numArray) : numArray.ToHexString();
        }

        public static byte[] Encrypt(byte[] sourceBytes, string publicKey, RSAKeyType keyType = RSAKeyType.Xml)
        {
            using var rsa = RSA.Create();
            switch (keyType)
            {
                case RSAKeyType.Xml:
                    rsa.FromExtXmlString(publicKey);
                    break;

                case RSAKeyType.Base64:
                    rsa.FromBase64StringByPrivateKey(publicKey);
                    break;

                default:
                    rsa.FromJsonString(publicKey);
                    break;
            }
            return rsa.Encrypt(sourceBytes, RSAEncryptionPadding.Pkcs1);
        }

        public static string Decrypt(
          string value,
          string privateKey,
          Encoding encoding = null,
          OutType outType = OutType.Base64,
          RSAKeyType keyType = RSAKeyType.Xml)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            var bytes = Decrypt(value.GetEncryptBytes(outType), privateKey, keyType);
            return encoding.GetString(bytes);
        }

        public static byte[] Decrypt(byte[] sourceBytes, string privateKey, RSAKeyType keyType = RSAKeyType.Xml)
        {
            using (var rsa = RSA.Create())
            {
                switch (keyType)
                {
                    case RSAKeyType.Xml:
                        rsa.FromExtXmlString(privateKey);
                        break;

                    case RSAKeyType.Base64:
                        rsa.FromBase64StringByPrivateKey(privateKey);
                        break;

                    default:
                        rsa.FromJsonString(privateKey);
                        break;
                }
                return rsa.Decrypt(sourceBytes, RSAEncryptionPadding.Pkcs1);
            }
        }

        public static string SignData(
          string source,
          string privateKey,
          Encoding encoding = null,
          OutType outType = OutType.Base64,
          RSAType rsaType = RSAType.RSA,
          RSAKeyType keyType = RSAKeyType.Xml)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            var numArray = SignData(encoding.GetBytes(source), privateKey, rsaType, keyType);
            return outType == OutType.Base64 ? Convert.ToBase64String(numArray) : numArray.ToHexString();
        }

        public static byte[] SignData(
          byte[] source,
          string privateKey,
          RSAType rsaType = RSAType.RSA,
          RSAKeyType keyType = RSAKeyType.Xml)
        {
            using var rsa = RSA.Create();
            switch (keyType)
            {
                case RSAKeyType.Xml:
                    rsa.FromExtXmlString(privateKey);
                    break;

                case RSAKeyType.Base64:
                    rsa.FromBase64StringByPrivateKey(privateKey);
                    break;

                default:
                    rsa.FromJsonString(privateKey);
                    break;
            }
            return rsa.SignData(source, rsaType == RSAType.RSA ? HashAlgorithmName.SHA1 : HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        public static bool VerifyData(
          string source,
          string signData,
          string publicKey,
          Encoding encoding = null,
          OutType outType = OutType.Base64,
          RSAType rsaType = RSAType.RSA,
          RSAKeyType keyType = RSAKeyType.Xml)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            return VerifyData(encoding.GetBytes(source), signData.GetEncryptBytes(outType), publicKey, rsaType, keyType);
        }

        public static bool VerifyData(
          byte[] source,
          byte[] signData,
          string publicKey,
          RSAType rsaType = RSAType.RSA,
          RSAKeyType keyType = RSAKeyType.Xml)
        {
            using (var rsa = RSA.Create())
            {
                switch (keyType)
                {
                    case RSAKeyType.Xml:
                        rsa.FromExtXmlString(publicKey);
                        break;

                    case RSAKeyType.Base64:
                        rsa.FromBase64StringByPublicKey(publicKey);
                        break;

                    default:
                        rsa.FromJsonString(publicKey);
                        break;
                }
                return rsa.VerifyData(source, signData, rsaType == RSAType.RSA ? HashAlgorithmName.SHA1 : HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
        }
    }
}