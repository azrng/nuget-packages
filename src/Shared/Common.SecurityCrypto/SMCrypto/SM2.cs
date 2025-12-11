using Common.SecurityCrypto.Extensions;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.SecurityCrypto.SMCrypto
{
    public class SM2
    {
        public static Dictionary<string, string> GetKeyPair()
        {
            var keyPair = SM2CryptoServiceProvider.Instance.ecc_key_pair_generator.GenerateKeyPair();
            var privateKeyParameters = (ECPrivateKeyParameters)keyPair.Private;
            var publicKeyParameters = (ECPublicKeyParameters)keyPair.Public;
            var d = privateKeyParameters.D;
            return new Dictionary<string, string>()
      {
        {
          "公钥",
          Encoding.UTF8.GetString(Hex.Encode(publicKeyParameters.Q.GetEncoded())).ToUpper()
        },
        {
          "私钥",
          Encoding.UTF8.GetString(Hex.Encode(d.ToByteArray())).ToUpper()
        }
      };
        }

        public static void GenerateKeyPair(out string prik, out string pubk)
        {
            var keyPair = GetKeyPair();
            prik = keyPair["私钥"];
            pubk = keyPair["公钥"];
        }

        public static Tuple<string, string> GetTupleKeyPair()
        {
            var keyPair = GetKeyPair();
            var str = keyPair["私钥"];
            return new Tuple<string, string>(keyPair["公钥"], str);
        }

        public static string Encrypt(string publicKey, string data) => Encrypt(publicKey.hexToByte(), Encoding.UTF8.GetBytes(data));

        public static string Encrypt(byte[] publicKey, byte[] data)
        {
            if (publicKey == null || publicKey.Length == 0 || data == null || data.Length == 0)
                return null;
            var numArray1 = new byte[data.Length];
            Array.Copy(data, 0, numArray1, 0, data.Length);
            var sm2Cipher = new SM2Cipher();
            var instance = SM2CryptoServiceProvider.Instance;
            var userKey = instance.ecc_curve.DecodePoint(publicKey);
            var ecPoint = sm2Cipher.Init_enc(instance, userKey);
            sm2Cipher.Encrypt(numArray1);
            var numArray2 = new byte[32];
            sm2Cipher.Dofinal(numArray2);
            return (ecPoint.GetEncoded().byteToHex() + numArray1.byteToHex() + numArray2.byteToHex()).ToUpper();
        }

        public static byte[] Decrypt(string privateKey, string encryptedData) => Decrypt(privateKey.hexToByte(), encryptedData.hexToByte());

        public static byte[] Decrypt(byte[] privateKey, byte[] encryptedData)
        {
            if (privateKey == null || privateKey.Length == 0 || encryptedData == null || encryptedData.Length == 0)
                return null;
            var hex = encryptedData.byteToHex();
            var numArray = hex.Substring(0, 130).hexToByte();
            var num = encryptedData.Length - 97;
            var data = hex.Substring(130, 2 * num).hexToByte();
            var c3 = hex.Substring(130 + 2 * num, 64).hexToByte();
            var instance = SM2CryptoServiceProvider.Instance;
            var userD = new BigInteger(1, privateKey);
            var c1 = instance.ecc_curve.DecodePoint(numArray);
            var sm2Cipher = new SM2Cipher();
            sm2Cipher.Init_dec(userD, c1);
            sm2Cipher.Decrypt(data);
            sm2Cipher.Dofinal(c3);
            return data;
        }

        public static string Sm2Sign(string source, string privateKey, bool isSignForSoft = true)
        {
            source = Encoding.UTF8.GetBytes(source).byteToHex();
            var sm2SignVo = SM2SignVerUtils.genSM2Signature(privateKey, source);
            return isSignForSoft ? sm2SignVo.getSm2_signForSoft() : sm2SignVo.getSm2_signForHard();
        }

        public static bool Verify(
          string source,
          string signData,
          string publicKey,
          bool isSignForSoft = true)
        {
            source = Encoding.UTF8.GetBytes(source).byteToHex();
            return isSignForSoft ? SM2SignVerUtils.verifySM2Signature(publicKey, source, signData) : SM2SignVerUtils.verifySM2SignatureHard(publicKey, source, signData);
        }
    }
}