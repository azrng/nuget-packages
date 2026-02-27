using Common.Security.Enums;
using Common.Security.Extensions;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Common.Security
{
    /// <summary>
    /// RSA非对称加解密助手类
    /// </summary>
    /// <remarks>
    /// RSA加密过程中使用了随机数生成器，这会导致每次加密时都生成不同的随机化因子。随机化因子在加密中起到重要作用，使得每次加密的结果都是不同的，即使明文相同
    /// 可以通过该网站生成公私钥：https://www.toolhelper.cn/AsymmetricEncryption/RsaGenerate
    /// </remarks>
    public static class RsaHelper
    {
        private const int DefaultKeySize = 2048;
        private const int Sha256HashSize = 32;

        /*
         * RSA 算法的加密和解密操作都要对大整数进行计算，在数据量很大时，一次性进行加解密操作会导致内存不足或计算时间过长。因此，需要将数据分成若干个小块，分别进行加解密，最后再将结果合并起来。
         * 对于加密操作来说，如果明文长度超过了 RSA 密钥对中公钥的长度，那么就需要将明文切分成若干个较小的部分，每个部分长度不能超过公钥长度减去一定的余量
         *
         * 例如，在使用 2048 位 RSA 公钥解密时，要求明文最多只能包含 245 字节（2048/8-11），所以当原始明文长度超过 245 字节时，就需要将其分成若干个小块，每个小块的长度不超过 245 字节，然后分别进行加密。
         * 对于解密操作，同样需要将密文分成若干个小块，每个小块长度不能超过私钥长度减去一定的余量。例如，在使用 2048 位 RSA 私钥解密时，要求密文最多只能包含 256 字节（2048/8），所以当密文过长时，就需要将其切分成若干个小块，然后分别进行解密。
         *
         *  感觉这个包文章里面的写法有可以我参考的，但是可能目前我使用场景有限，觉得目前的包沟通，等不沟通了再去里面找找有啥可以参考的
         *  https://www.cnblogs.com/stulzq/p/12053976.html
         */

        /// <summary>
        /// RSA的XML密钥产生
        /// </summary>
        /// <remarks>该方式生成的公私钥只支持FromXmlString方式导入</remarks>
        public static (string, string) ExportXmlRsaKey()
        {
            using var rsa = RSA.Create();
            return (rsa.ToXmlString(false), rsa.ToXmlString(true));
        }

        /// <summary>
        /// RSA的Base64密钥产生
        /// </summary>
        /// <returns>(公钥, 私钥)</returns>
        public static (string PublicKey, string PrivateKey) ExportBase64RsaKey()
        {
            using var rsa = RSA.Create(DefaultKeySize);
            return (
                Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo()),
                Convert.ToBase64String(rsa.ExportPkcs8PrivateKey())
            );
        }

        /// <summary>
        /// RSA的PEM密钥产生
        /// </summary>
        /// <param name="privateType">私钥格式类型</param>
        /// <returns>公私钥</returns>
        /// <remarks>和该网站互认：https://www.toolhelper.cn/AsymmetricEncryption/RsaGenerate</remarks>
        public static (string, string) ExportPemRsaKey(RsaKeyFormat privateType = RsaKeyFormat.PKCS8)
        {
            using var rsa = RSA.Create();

            var privateKey = privateType == RsaKeyFormat.PKCS8
                ? rsa.ExportPkcs8PrivateKey()
                : rsa.ExportRSAPrivateKey();
            return (Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo()),
                Convert.ToBase64String(privateKey));
        }

        /// <summary>
        /// 快速签名（使用SHA256和Base64编码）
        /// </summary>
        public static string QuickSign(string data, string privateKey)
        {
            return SignData(data, privateKey, HashAlgorithmName.SHA256);
        }

        /// <summary>
        /// 快速验证签名（使用SHA256和Base64编码）
        /// </summary>
        public static bool QuickVerify(string data, string sign, string publicKey)
        {
            return VerifyData(data, sign, publicKey, HashAlgorithmName.SHA256);
        }

        /// <summary>
        /// 快速签名（使用SHA256 + PSS和Base64编码）
        /// </summary>
        public static string QuickSignPss(string data, string privateKey)
        {
            return SignDataPss(data, privateKey, HashAlgorithmName.SHA256);
        }

        /// <summary>
        /// 快速验证签名（使用SHA256 + PSS和Base64编码）
        /// </summary>
        public static bool QuickVerifyPss(string data, string sign, string publicKey)
        {
            return VerifyDataPss(data, sign, publicKey, HashAlgorithmName.SHA256);
        }

        /// <summary>
        /// 使用 OAEP-SHA256 加密（长文本会自动分段）
        /// </summary>
        public static string EncryptOaepSha256(string plaintext, string publicKey, RSAKeyType keyType = RSAKeyType.PEM,
                                               OutType outputType = OutType.Base64)
        {
            if (plaintext == null)
                throw new ArgumentNullException(nameof(plaintext));
            if (string.IsNullOrWhiteSpace(publicKey))
                throw new ArgumentNullException(nameof(publicKey));

            var valueBytes = Encoding.UTF8.GetBytes(plaintext);
            using var rsa = InitializeRsa(publicKey, keyType);
            var keyByteSize = rsa.KeySize / 8;
            var maxEncryptBlockSize = keyByteSize - (2 * Sha256HashSize) - 2;
            if (maxEncryptBlockSize <= 0)
                throw new InvalidOperationException("RSA key size is too small for OAEP-SHA256.");

            if (valueBytes.Length <= maxEncryptBlockSize)
            {
                var cipherBytes = rsa.Encrypt(valueBytes, RSAEncryptionPadding.OaepSHA256);
                return cipherBytes.GetString(outputType);
            }

            using var plaiStream = new MemoryStream(valueBytes);
            using var crypStream = new MemoryStream();
            var buffer = new byte[maxEncryptBlockSize];
            var blockSize = plaiStream.Read(buffer, 0, maxEncryptBlockSize);
            while (blockSize > 0)
            {
                var toEncrypt = new byte[blockSize];
                Array.Copy(buffer, 0, toEncrypt, 0, blockSize);
                var cryptograph = rsa.Encrypt(toEncrypt, RSAEncryptionPadding.OaepSHA256);
                crypStream.Write(cryptograph, 0, cryptograph.Length);
                blockSize = plaiStream.Read(buffer, 0, maxEncryptBlockSize);
            }

            return crypStream.ToArray().GetString(outputType);
        }

        /// <summary>
        /// 使用 OAEP-SHA256 解密（长文本会自动分段）
        /// </summary>
        public static string DecryptOaepSha256(string encryptStr, string privateKey, RSAKeyType keyType = RSAKeyType.PEM,
                                               RsaKeyFormat privateKeyFormat = RsaKeyFormat.PKCS8, OutType inputType = OutType.Base64)
        {
            if (encryptStr == null)
                throw new ArgumentNullException(nameof(encryptStr));
            if (string.IsNullOrWhiteSpace(privateKey))
                throw new ArgumentNullException(nameof(privateKey));

            var valueBytes = encryptStr.GetBytes(inputType);
            using var rsa = InitializeRsa(privateKey, keyType, privateKeyFormat);
            var maxBlockSize = rsa.KeySize / 8;
            if (valueBytes.Length <= maxBlockSize)
            {
                var cipherBytes = rsa.Decrypt(valueBytes, RSAEncryptionPadding.OaepSHA256);
                return Encoding.UTF8.GetString(cipherBytes);
            }

            if (valueBytes.Length % maxBlockSize != 0)
                throw new ArgumentException("Invalid RSA encrypted payload length.", nameof(encryptStr));

            using var crypStream = new MemoryStream(valueBytes);
            using var plaiStream = new MemoryStream();
            var buffer = new byte[maxBlockSize];
            var blockSize = crypStream.Read(buffer, 0, maxBlockSize);
            while (blockSize > 0)
            {
                if (blockSize != maxBlockSize)
                    throw new ArgumentException("Invalid RSA encrypted block length.", nameof(encryptStr));

                var toDecrypt = new byte[blockSize];
                Array.Copy(buffer, 0, toDecrypt, 0, blockSize);
                var cryptograph = rsa.Decrypt(toDecrypt, RSAEncryptionPadding.OaepSHA256);
                plaiStream.Write(cryptograph, 0, cryptograph.Length);
                blockSize = crypStream.Read(buffer, 0, maxBlockSize);
            }

            return Encoding.UTF8.GetString(plaiStream.ToArray());
        }

        /// <summary>
        /// 使用 RSA-PSS 签名
        /// </summary>
        public static string SignDataPss(string data, string privateKey, HashAlgorithmName hash,
                                         Encoding encoding = null, OutType privateKeyType = OutType.Base64,
                                         OutType outputType = OutType.Base64)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (privateKey == null)
                throw new ArgumentNullException(nameof(privateKey));

            encoding ??= Encoding.UTF8;
            var dataBytes = encoding.GetBytes(data);
            var privateKeyBytes = privateKey.GetBytes(privateKeyType);
            using var rsa = RSA.Create();
            try
            {
                rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
            }
            catch (CryptographicException)
            {
                rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
            }

            var signatureBytes = rsa.SignData(dataBytes, hash, RSASignaturePadding.Pss);
            return signatureBytes.GetString(outputType);
        }

        /// <summary>
        /// 使用 RSA-PSS 验证签名
        /// </summary>
        public static bool VerifyDataPss(string data, string sign, string publicKey, HashAlgorithmName hash,
                                         Encoding encoding = null, OutType signType = OutType.Base64,
                                         OutType publicKeyType = OutType.Base64)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (sign == null)
                throw new ArgumentNullException(nameof(sign));
            if (publicKey == null)
                throw new ArgumentNullException(nameof(publicKey));

            encoding ??= Encoding.UTF8;
            var dataBytes = encoding.GetBytes(data);
            var signBytes = sign.GetBytes(signType);
            var publicKeyBytes = publicKey.GetBytes(publicKeyType);
            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
            return rsa.VerifyData(dataBytes, signBytes, hash, RSASignaturePadding.Pss);
        }

        /// <summary>
        /// 加密(长文本会进行分段，分段长度默认分段长度是 私钥长度/8-11)
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="publicKey">pem格式的密钥</param>
        /// <param name="keyType"></param>
        /// <param name="outputType">输出类型</param>
        /// <returns></returns>
        /// <remarks>keySize：2048  pkcs1</remarks>
        public static string Encrypt(string plaintext, string publicKey, RSAKeyType keyType = RSAKeyType.PEM,
                                     OutType outputType = OutType.Base64)
        {
            if (plaintext == null)
                throw new ArgumentNullException(nameof(plaintext));
            if (string.IsNullOrWhiteSpace(publicKey))
                throw new ArgumentNullException(nameof(publicKey));

            var valueBytes = Encoding.UTF8.GetBytes(plaintext);

            using var rsa = RSA.Create();
            switch (keyType)
            {
                case RSAKeyType.Xml:
                    rsa.FromXmlString(publicKey);
                    break;

                case RSAKeyType.PEM:
                    publicKey = publicKey.Replace("-----BEGIN PUBLIC KEY-----", "")
                                         .Replace("-----END PUBLIC KEY-----", "")
                                         .Replace("\r", "")
                                         .Replace("\n", "")
                                         .Trim();
                    rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);
                    break;
            }

            var keyByteSize = rsa.KeySize / 8;
            var maxEncryptBlockSize = keyByteSize - 11; // PKCS#1 v1.5 padding overhead
            if (valueBytes.Length <= maxEncryptBlockSize)
            {
                var cipherBytes = rsa.Encrypt(valueBytes, RSAEncryptionPadding.Pkcs1);
                return cipherBytes.GetString(outputType);
            }

            using var plaiStream = new MemoryStream(valueBytes);
            using var crypStream = new MemoryStream();
            var buffer = new byte[maxEncryptBlockSize];
            var blockSize = plaiStream.Read(buffer, 0, maxEncryptBlockSize);
            while (blockSize > 0)
            {
                var toEncrypt = new byte[blockSize];
                Array.Copy(buffer, 0, toEncrypt, 0, blockSize);
                var cryptograph = rsa.Encrypt(toEncrypt, RSAEncryptionPadding.Pkcs1);
                crypStream.Write(cryptograph, 0, cryptograph.Length);
                blockSize = plaiStream.Read(buffer, 0, maxEncryptBlockSize);
            }

            return crypStream.ToArray().GetString(outputType);
        }

        /// <summary>
        /// 解密(长文本会进行分段，分段长度默认分段长度是 私钥长度/8-11)
        /// </summary>
        /// <param name="encryptStr"></param>
        /// <param name="privateKey">pem格式的私钥</param>
        /// <param name="keyType">密钥类型</param>
        /// <param name="privateKeyFormat">私钥格式</param>
        /// <param name="inputType">输入类型</param>
        /// <returns></returns>
        /// <remarks>keySize：2048  pkcs1</remarks>
        public static string Decrypt(string encryptStr, string privateKey, RSAKeyType keyType = RSAKeyType.PEM,
                                     RsaKeyFormat privateKeyFormat = RsaKeyFormat.PKCS8, OutType inputType = OutType.Base64)
        {
            if (encryptStr == null)
                throw new ArgumentNullException(nameof(encryptStr));
            if (string.IsNullOrWhiteSpace(privateKey))
                throw new ArgumentNullException(nameof(privateKey));

            var valueBytes = encryptStr.GetBytes(inputType);
            using var rsa = RSA.Create();
            switch (keyType)
            {
                case RSAKeyType.Xml:
                    rsa.FromXmlString(privateKey);
                    break;

                case RSAKeyType.PEM:
                    privateKey = privateKey.Replace("-----BEGIN RSA PRIVATE KEY-----", "")
                                           .Replace("-----END RSA PRIVATE KEY-----", "")
                                           .Replace("-----BEGIN PRIVATE KEY-----", "")
                                           .Replace("-----END PRIVATE KEY-----", "")
                                           .Replace("\r", "")
                                           .Replace("\n", "")
                                           .Trim();
                    if (privateKeyFormat == RsaKeyFormat.PKCS8)
                        rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);
                    else if (privateKeyFormat == RsaKeyFormat.PKCS1)
                        rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(keyType), keyType, null);
            }

            var maxBlockSize = rsa.KeySize / 8; // 解密块最大长度
            if (valueBytes.Length <= maxBlockSize)
            {
                var cipherBytes = rsa.Decrypt(valueBytes, RSAEncryptionPadding.Pkcs1);
                return Encoding.UTF8.GetString(cipherBytes);
            }

            if (valueBytes.Length % maxBlockSize != 0)
                throw new ArgumentException("Invalid RSA encrypted payload length.", nameof(encryptStr));

            using var crypStream = new MemoryStream(valueBytes);
            using var plaiStream = new MemoryStream();
            var buffer = new byte[maxBlockSize];
            var blockSize = crypStream.Read(buffer, 0, maxBlockSize);
            while (blockSize > 0)
            {
                if (blockSize != maxBlockSize)
                    throw new ArgumentException("Invalid RSA encrypted block length.", nameof(encryptStr));

                var toDecrypt = new byte[blockSize];
                Array.Copy(buffer, 0, toDecrypt, 0, blockSize);
                var cryptograph = rsa.Decrypt(toDecrypt, RSAEncryptionPadding.Pkcs1);
                plaiStream.Write(cryptograph, 0, cryptograph.Length);
                blockSize = crypStream.Read(buffer, 0, maxBlockSize);
            }

            return Encoding.UTF8.GetString(plaiStream.ToArray());
        }

        /// <summary>
        /// 使用私钥签名
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="privateKey">私钥(pkcs1 pem)</param>
        /// <param name="hash">rsa是sha1 rsa2是sha256</param>
        /// <param name="encoding"></param>
        /// <param name="privateKeyType">私钥类型</param>
        /// <param name="outputType">输入类型</param>
        /// <returns></returns>
        public static string SignData(string data, string privateKey, HashAlgorithmName hash,
                                      Encoding encoding = null, OutType privateKeyType = OutType.Base64,
                                      OutType outputType = OutType.Base64)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (privateKey == null)
                throw new ArgumentNullException(nameof(privateKey));

            encoding ??= Encoding.UTF8;
            var dataBytes = encoding.GetBytes(data);

            var privateKeyBytes = privateKey.GetBytes(privateKeyType);
            using var rsa = RSA.Create();
            try
            {
                rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
            }
            catch (CryptographicException)
            {
                rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
            }

            var signatureBytes = rsa.SignData(dataBytes, hash, RSASignaturePadding.Pkcs1);

            return signatureBytes.GetString(outputType);
        }

        /// <summary>
        /// 使用公钥验证签名
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="sign">签名</param>
        /// <param name="publicKey">公钥（pem）</param>
        /// <param name="hash">rsa是sha1 rsa2是sha256</param>
        /// <param name="encoding"></param>
        /// <param name="publicKeyType"></param>
        /// <param name="signType"></param>
        /// <returns></returns>
        public static bool VerifyData(string data, string sign, string publicKey, HashAlgorithmName hash,
                                      Encoding encoding = null, OutType signType = OutType.Base64, OutType publicKeyType = OutType.Base64)
        {
            encoding ??= Encoding.UTF8;

            var dataBytes = encoding.GetBytes(data);
            var signBytes = sign.GetBytes(signType);

            var publicKeyBytes = publicKey.GetBytes(publicKeyType);
            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
            return rsa.VerifyData(dataBytes, signBytes, hash, RSASignaturePadding.Pkcs1);
        }

        /// <summary>
        /// 初始化rsa
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyType"></param>
        /// <param name="keyFormat"></param>
        /// <returns></returns>
        private static RSA InitializeRsa(string key, RSAKeyType keyType, RsaKeyFormat? keyFormat = null)
        {
            var rsa = RSA.Create();
            key = key.Replace("-----BEGIN PUBLIC KEY-----", "")
                     .Replace("-----END PUBLIC KEY-----", "")
                     .Replace("-----BEGIN RSA PRIVATE KEY-----", "")
                     .Replace("-----END RSA PRIVATE KEY-----", "")
                     .Replace("-----BEGIN PRIVATE KEY-----", "")
                     .Replace("-----END PRIVATE KEY-----", "")
                     .Replace("\r", "")
                     .Replace("\n", "")
                     .Trim();

            switch (keyType)
            {
                case RSAKeyType.Xml:
                    rsa.FromXmlString(key);
                    break;
                case RSAKeyType.PEM:
                    var keyBytes = Convert.FromBase64String(key);
                    if (keyFormat == RsaKeyFormat.PKCS8)
                        rsa.ImportPkcs8PrivateKey(keyBytes, out _);
                    else if (keyFormat == RsaKeyFormat.PKCS1)
                        rsa.ImportRSAPrivateKey(keyBytes, out _);
                    else
                        rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);
                    break;
            }

            return rsa;
        }
    }
}
