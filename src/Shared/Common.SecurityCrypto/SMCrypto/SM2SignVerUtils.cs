using Common.SecurityCrypto.Extensions;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;
using System;
using System.IO;

namespace Common.SecurityCrypto.SMCrypto
{
    internal class SM2SignVerUtils
    {
        public static string USER_ID = "1234567812345678";

        public static SM2SignVO Sign2SM2(byte[] privatekey, byte[] sourceData)
        {
            var sm2SignVo = new SM2SignVO();
            sm2SignVo.setSm2_type("sign");
            var instance = SM2CryptoServiceProvider.Instance;
            var userD = new BigInteger(privatekey);
            sm2SignVo.setSm2_userd(userD.ToByteArray().ToHexString());
            var userKey = instance.ecc_point_g.Multiply(userD);
            var sm3Digest = new SM3Digest();
            var z = instance.Sm2GetZ(USER_ID.GetBytes(), userKey);
            sm2SignVo.setSm3_z(z.ToHexString());
            sm2SignVo.setSign_express(sourceData.ToHexString());
            sm3Digest.update(z, 0, z.Length);
            sm3Digest.update(sourceData, 0, sourceData.Length);
            var numArray = new byte[32];
            sm3Digest.doFinal(numArray, 0);
            sm2SignVo.setSm3_digest(numArray.ToHexString());
            var sm2Result = new SM2Result();
            instance.sm2Sign(numArray, userD, userKey, sm2Result);
            sm2SignVo.setSign_r(sm2Result.r.ToByteArray().ToHexString());
            sm2SignVo.setSign_s(sm2Result.s.ToByteArray().ToHexString());
            var derInteger1 = new DerInteger(sm2Result.r);
            var derInteger2 = new DerInteger(sm2Result.s);
            var asn1EncodableVector = new Asn1EncodableVector(Array.Empty<Asn1Encodable>());
            asn1EncodableVector.Add(new Asn1Encodable[1]
            {
         derInteger1
            });
            asn1EncodableVector.Add(new Asn1Encodable[1]
            {
         derInteger2
            });
            var hex = new DerSequence(asn1EncodableVector).GetEncoded().ByteArrayToHex();
            sm2SignVo.setSm2_sign(hex);
            return sm2SignVo;
        }

        public static SM2SignVO VerifySignSM2(
          byte[] publicKey,
          byte[] sourceData,
          byte[] signData)
        {
            try
            {
                var sm2SignVo = new SM2SignVO();
                sm2SignVo.setSm2_type("verify");
                byte[] destinationArray;
                if (publicKey.Length == 64)
                {
                    destinationArray = new byte[65];
                    destinationArray[0] = 4;
                    Array.Copy(publicKey, 0, destinationArray, 1, publicKey.Length);
                }
                else
                    destinationArray = publicKey;
                var instance = SM2CryptoServiceProvider.Instance;
                var userKey = instance.ecc_curve.DecodePoint(destinationArray);
                var sm3Digest = new SM3Digest();
                var z = instance.Sm2GetZ(USER_ID.GetBytes(), userKey);
                sm2SignVo.setSm3_z(z.ToHexString());
                sm3Digest.update(z, 0, z.Length);
                sm3Digest.update(sourceData, 0, sourceData.Length);
                var numArray = new byte[32];
                sm3Digest.doFinal(numArray, 0);
                sm2SignVo.setSm3_digest(numArray.ToHexString());
                var enumerator = ((Asn1Sequence)new Asn1InputStream(new MemoryStream(signData)).ReadObject()).GetEnumerator();
                enumerator.MoveNext();
                var bigInteger1 = ((DerInteger)enumerator.Current).Value;
                enumerator.MoveNext();
                var bigInteger2 = ((DerInteger)enumerator.Current).Value;
                var sm2Result = new SM2Result();
                sm2Result.r = bigInteger1;
                sm2Result.s = bigInteger2;
                sm2SignVo.setVerify_r(sm2Result.r.ToByteArray().ToHexString());
                sm2SignVo.setVerify_s(sm2Result.s.ToByteArray().ToHexString());
                instance.sm2Verify(numArray, userKey, sm2Result.r, sm2Result.s, sm2Result);
                var isVerify = sm2Result.r.Equals(sm2Result.R);
                sm2SignVo.setVerify(isVerify);
                return sm2SignVo;
            }
            catch (ArgumentException ex)
            {
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static SM2SignVO genSM2Signature(string priKey, string sourceData) => Sign2SM2(priKey.hexToByte(), sourceData.hexToByte());

        public static bool verifySM2Signature(string pubKey, string sourceData, string softsign)
        {
            var sm2SignVo = VerifySignSM2(pubKey.HexToByteArray(), sourceData.hexToByte(), softsign.hexToByte());
            return sm2SignVo != null && sm2SignVo.isVerify;
        }

        public static bool verifySM2SignatureHard(string pubKey, string sourceData, string hardSign)
        {
            var soft = SM2SignHardToSoft(hardSign);
            return verifySM2Signature(pubKey, sourceData, soft);
        }

        public static string SM2SignHardToSoft(string hardSign)
        {
            var sourceArray = hardSign.hexToByte();
            var numArray1 = new byte[sourceArray.Length / 2];
            var numArray2 = new byte[sourceArray.Length / 2];
            Array.Copy(sourceArray, 0, numArray1, 0, sourceArray.Length / 2);
            Array.Copy(sourceArray, sourceArray.Length / 2, numArray2, 0, sourceArray.Length / 2);
            var derInteger1 = new DerInteger(SM2CryptoServiceProvider.byteConvertInteger(numArray1));
            var derInteger2 = new DerInteger(SM2CryptoServiceProvider.byteConvertInteger(numArray2));
            var asn1EncodableVector = new Asn1EncodableVector(Array.Empty<Asn1Encodable>());
            asn1EncodableVector.Add(new Asn1Encodable[1]
            {
         derInteger1
            });
            asn1EncodableVector.Add(new Asn1Encodable[1]
            {
         derInteger2
            });
            var derSequence = new DerSequence(asn1EncodableVector);
            string hex;
            try
            {
                hex = derSequence.GetEncoded().ByteArrayToHex();
            }
            catch (IOException ex)
            {
                throw ex;
            }
            return hex;
        }
    }
}