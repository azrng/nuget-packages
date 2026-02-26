using Common.Security.Enums;
using Common.Security.Extensions;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.Text;

namespace Common.Security
{
    /// <summary>
    /// sm2 非对称加密算法（因为在加密过程中会引入随机数（比如加密时的临时私钥）以增加安全性，导致每次加密的结果都是不同的）
    /// </summary>
    /// <remarks>
    /// 对标RSA、RSA4096
    /// 可以通过该网站在线生成：https://lzltool.cn/SM2
    /// </remarks>
    public static class Sm2Helper
    {
        /// <summary>
        /// 导出公私钥
        /// </summary>
        /// <returns></returns>
        public static (string, string) ExportKey(OutType outType = OutType.Hex)
        {
            // 使用SM2标准推荐的椭圆曲线参数
            var curve = CustomNamedCurves.GetByName("sm2p256v1");
            var domainParameters = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

            // 随机生成私钥
            var secureRandom = new SecureRandom();

            // 计算私钥
            var privateKeyValue =
                new ECPrivateKeyParameters(new BigInteger(1, secureRandom.GenerateSeed(32)), domainParameters);
            // 计算公钥
            var publicKeyValue =
                new ECPublicKeyParameters(domainParameters.G.Multiply(privateKeyValue.D), domainParameters);

            var keyPair = new AsymmetricCipherKeyPair(publicKeyValue, privateKeyValue);
            var privateKeyParameters = (ECPrivateKeyParameters)keyPair.Private;
            var publicKeyParameters = (ECPublicKeyParameters)keyPair.Public;
            var d = privateKeyParameters.D;

            var publicKey = publicKeyParameters.Q.GetEncoded().GetString(outType);
            var privateKey = ToFixedLengthPrivateKey(d, 32).GetString(outType);

            return (publicKey, privateKey);
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="plaintext">要加密的文本</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="publicKeyType">公钥类型</param>
        /// <param name="outType">输出类型</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string Encrypt(string plaintext, string publicKey, OutType publicKeyType = OutType.Hex,
            OutType outType = OutType.Hex,
            Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(plaintext))
                throw new ArgumentNullException(nameof(plaintext));
            if (string.IsNullOrEmpty(publicKey))
                throw new ArgumentNullException(nameof(publicKey));

            encoding ??= Encoding.UTF8;

            var plaintextBytes = encoding.GetBytes(plaintext);
            var publicKeyBytes = publicKey.GetBytes(publicKeyType);

            // 获取 SM2 曲线参数
            var curve = ECNamedCurveTable.GetByName("sm2p256v1");
            var q = curve.Curve.DecodePoint(publicKeyBytes);
            var domain = new ECDomainParameters(curve);
            var publicKeyParam = new ECPublicKeyParameters("EC", q, domain);

            var publicKeyParams = new ParametersWithRandom(publicKeyParam, new SecureRandom());

            // 创建sm2加速器
            var engine = new SM2Engine();
            engine.Init(true, publicKeyParams);
            // 执行加密操作
            var encryptedData = engine.ProcessBlock(plaintextBytes, 0, plaintextBytes.Length);
            return encryptedData.GetString(outType);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="encryptedText">要解密的内容</param>
        /// <param name="privateKey">私钥</param>
        /// <param name="privateKeyType">私钥类型</param>
        /// <param name="inputType">输入的类型</param>
        /// <param name="encoding">编码类型</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string Decrypt(string encryptedText, string privateKey, OutType privateKeyType = OutType.Hex,
            OutType inputType = OutType.Hex,
            Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(encryptedText))
                throw new ArgumentNullException(nameof(encryptedText));
            if (string.IsNullOrEmpty(privateKey))
                throw new ArgumentNullException(nameof(privateKey));

            encoding ??= Encoding.UTF8;

            var encryptedTextBytes = encryptedText.GetBytes(inputType);
            var privateKeyBytes = privateKey.GetBytes(privateKeyType);

            // 获取 SM2 曲线参数
            var dCurve = ECNamedCurveTable.GetByName("sm2p256v1");

            var dDomain = new ECDomainParameters(dCurve);
            var d = new BigInteger(1, privateKeyBytes);
            var privateKeyParam = new ECPrivateKeyParameters(d, dDomain);

            // 创建SM2加密器
            var sm2Engine = new SM2Engine();
            sm2Engine.Init(false, privateKeyParam);

            // 执行解密操作
            var decryptedData = sm2Engine.ProcessBlock(encryptedTextBytes, 0, encryptedTextBytes.Length);

            // 将解密结果转换为字符串
            return encoding.GetString(decryptedData);
        }

        private static byte[] ToFixedLengthPrivateKey(BigInteger d, int size)
        {
            var unsignedBytes = d.ToByteArrayUnsigned();
            if (unsignedBytes.Length == size)
                return unsignedBytes;

            if (unsignedBytes.Length > size)
            {
                var trimmed = new byte[size];
                Array.Copy(unsignedBytes, unsignedBytes.Length - size, trimmed, 0, size);
                return trimmed;
            }

            var padded = new byte[size];
            Array.Copy(unsignedBytes, 0, padded, size - unsignedBytes.Length, unsignedBytes.Length);
            return padded;
        }

        // public static string Sm2Sign(string source, string privateKey, bool isSignForSoft = true)
        // {
        //     source = Encoding.UTF8.GetBytes(source).ByteToHex();
        //     var sm2SignVo = SM2SignVerUtils.genSM2Signature(privateKey, source);
        //     return isSignForSoft ? sm2SignVo.getSm2_signForSoft() : sm2SignVo.getSm2_signForHard();
        // }
        //
        // public static bool Verify(string source, string signData, string publicKey, bool isSignForSoft = true)
        // {
        //     source = Encoding.UTF8.GetBytes(source).ByteToHex();
        //     return isSignForSoft
        //         ? SM2SignVerUtils.verifySM2Signature(publicKey, source, signData)
        //         : SM2SignVerUtils.verifySM2SignatureHard(publicKey, source, signData);
        // }
    }
}
