using Common.SecurityCrypto.Extensions;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using System;

namespace Common.SecurityCrypto.SMCrypto
{
    public class SM2CryptoServiceProvider
    {
        public static readonly string[] sm2_param = new string[6]
        {
      "fffffffeffffffffffffffffffffffffffffffff00000000ffffffffffffffff",
      "fffffffeffffffffffffffffffffffffffffffff00000000fffffffffffffffc",
      "28e9fa9e9d9f5e344d5a9e4bcf6509a7f39789f515ab8f92ddbcbd414d940e93",
      "fffffffeffffffffffffffffffffffff7203df6b21c6052b53bbf40939d54123",
      "32c4ae2c1f1981195f9904466a39c9948fe30bbff2660be1715a4589334c74c7",
      "bc3736a2f4f6779c59bdcee36b692153d0a9877cc62a474002df32e52139f0a0"
        };

        public string[] ecc_param = sm2_param;
        public readonly BigInteger ecc_p;
        public readonly BigInteger ecc_a;
        public readonly BigInteger ecc_b;
        public readonly BigInteger ecc_n;
        public readonly BigInteger ecc_gx;
        public readonly BigInteger ecc_gy;
        public readonly ECCurve ecc_curve;
        public readonly ECPoint ecc_point_g;
        public readonly ECDomainParameters ecc_bc_spec;
        public readonly ECKeyPairGenerator ecc_key_pair_generator;

        public static SM2CryptoServiceProvider Instance => new SM2CryptoServiceProvider();

        public SM2CryptoServiceProvider()
        {
            ecc_param = sm2_param;
            ecc_p = new BigInteger(sm2_param[0], 16);
            ecc_a = new BigInteger(sm2_param[1], 16);
            ecc_b = new BigInteger(sm2_param[2], 16);
            ecc_n = new BigInteger(sm2_param[3], 16);
            ecc_gx = new BigInteger(sm2_param[4], 16);
            ecc_gy = new BigInteger(sm2_param[5], 16);
            var fpFieldElement1 = new FpFieldElement(ecc_p, ecc_gx);
            var fpFieldElement2 = new FpFieldElement(ecc_p, ecc_gy);
            ecc_curve = new FpCurve(ecc_p, ecc_a, ecc_b);
            ecc_point_g = new FpPoint(ecc_curve, fpFieldElement1, fpFieldElement2);
            ecc_bc_spec = new ECDomainParameters(ecc_curve, ecc_point_g, ecc_n);
            var generationParameters = new ECKeyGenerationParameters(ecc_bc_spec, new SecureRandom());
            ecc_key_pair_generator = new ECKeyPairGenerator();
            ecc_key_pair_generator.Init(generationParameters);
        }

        public virtual byte[] Sm2GetZ(byte[] userId, ECPoint userKey)
        {
            var sm3Digest = new SM3Digest();
            var num = userId.Length * 8;
            sm3Digest.update((byte)(num >> 8 & byte.MaxValue));
            sm3Digest.update((byte)(num & byte.MaxValue));
            sm3Digest.update(userId, 0, userId.Length);
            var in1_1 = byteConvert32Bytes(ecc_a);
            sm3Digest.update(in1_1, 0, in1_1.Length);
            var in1_2 = byteConvert32Bytes(ecc_b);
            sm3Digest.update(in1_2, 0, in1_2.Length);
            var in1_3 = byteConvert32Bytes(ecc_gx);
            sm3Digest.update(in1_3, 0, in1_3.Length);
            var in1_4 = byteConvert32Bytes(ecc_gy);
            sm3Digest.update(in1_4, 0, in1_4.Length);
            var in1_5 = byteConvert32Bytes(userKey.Normalize().XCoord.ToBigInteger());
            sm3Digest.update(in1_5, 0, in1_5.Length);
            var in1_6 = byteConvert32Bytes(userKey.Normalize().YCoord.ToBigInteger());
            sm3Digest.update(in1_6, 0, in1_6.Length);
            var out1 = new byte[sm3Digest.getDigestSize()];
            sm3Digest.doFinal(out1, 0);
            return out1;
        }

        public static byte[] byteConvert32Bytes(BigInteger n)
        {
            var numArray = new byte[0];
            if (n == null)
                return null;
            byte[] destinationArray;
            if (n.ToByteArray().Length == 33)
            {
                destinationArray = new byte[32];
                Array.Copy(n.ToByteArray(), 1, destinationArray, 0, 32);
            }
            else if (n.ToByteArray().Length == 32)
            {
                destinationArray = n.ToByteArray();
            }
            else
            {
                destinationArray = new byte[32];
                for (var index = 0; index < 32 - n.ToByteArray().Length; ++index)
                    destinationArray[index] = 0;
                Array.Copy(n.ToByteArray(), 0, destinationArray, 32 - n.ToByteArray().Length, n.ToByteArray().Length);
            }
            return destinationArray;
        }

        public static BigInteger byteConvertInteger(byte[] b)
        {
            if (b[0] >= 0)
                return new BigInteger(b);
            var destinationArray = new byte[b.Length + 1];
            destinationArray[0] = 0;
            Array.Copy(b, 0, destinationArray, 1, b.Length);
            return new BigInteger(destinationArray);
        }

        public void sm2Sign(byte[] md, BigInteger userD, ECPoint userKey, SM2Result sm2Result)
        {
            var bigInteger1 = new BigInteger(1, md);
            BigInteger bigInteger2;
            BigInteger bigInteger3;
            do
            {
                BigInteger d;
                do
                {
                    var keyPair = ecc_key_pair_generator.GenerateKeyPair();
                    var privateKeyParameters = (ECPrivateKeyParameters)keyPair.Private;
                    var publicKeyParameters = (ECPublicKeyParameters)keyPair.Public;
                    d = privateKeyParameters.D;
                    var q = publicKeyParameters.Q;
                    bigInteger2 = bigInteger1.Add(q.XCoord.ToBigInteger()).Mod(ecc_n);
                }
                while (bigInteger2.Equals(BigInteger.Zero) || bigInteger2.Add(d).Equals(ecc_n) || bigInteger2.ToByteArray().ToHexString().Length != 64);
                var bigInteger4 = userD.Add(BigInteger.One).ModInverse(ecc_n);
                var bigInteger5 = bigInteger2.Multiply(userD);
                var bigInteger6 = d.Subtract(bigInteger5).Mod(ecc_n);
                bigInteger3 = bigInteger4.Multiply(bigInteger6).Mod(ecc_n);
            }
            while (bigInteger3.Equals(BigInteger.Zero) || bigInteger3.ToByteArray().ToHexString().Length != 64);
            sm2Result.r = bigInteger2;
            sm2Result.s = bigInteger3;
        }

        public void sm2Verify(
          byte[] md,
          ECPoint userKey,
          BigInteger r,
          BigInteger s,
          SM2Result sm2Result)
        {
            sm2Result.R = null;
            var bigInteger1 = new BigInteger(1, md);
            var bigInteger2 = r.Add(s).Mod(ecc_n);
            if (bigInteger2.Equals(BigInteger.Zero))
                return;
            var ecPoint = ecc_point_g.Multiply(sm2Result.s).Add(userKey.Multiply(bigInteger2));
            sm2Result.R = bigInteger1.Add(ecPoint.Normalize().XCoord.ToBigInteger()).Mod(ecc_n);
        }
    }
}