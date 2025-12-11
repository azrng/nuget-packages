using Common.SecurityCrypto.Extensions;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;

namespace Common.SecurityCrypto.SMCrypto
{
    public class SM2Cipher
    {
        private int ct;
        private ECPoint p2;
        private SM3Digest sm3keybase;
        private SM3Digest sm3c3;
        private byte[] key;
        private byte keyOff;

        public SM2Cipher()
        {
            ct = 1;
            key = new byte[32];
            keyOff = 0;
        }

        private void Reset()
        {
            sm3keybase = new SM3Digest();
            sm3c3 = new SM3Digest();
            var byteArray1 = p2.Normalize().XCoord.ToBigInteger().ToByteArray();
            sm3keybase.update(byteArray1, 0, byteArray1.Length);
            sm3c3.update(byteArray1, 0, byteArray1.Length);
            var byteArray2 = p2.Normalize().YCoord.ToBigInteger().ToByteArray();
            sm3keybase.update(byteArray2, 0, byteArray2.Length);
            ct = 1;
            NextKey();
        }

        private void NextKey()
        {
            var sm3Digest = new SM3Digest(sm3keybase);
            sm3Digest.update((byte)(ct >> 24 & byte.MaxValue));
            sm3Digest.update((byte)(ct >> 16 & byte.MaxValue));
            sm3Digest.update((byte)(ct >> 8 & byte.MaxValue));
            sm3Digest.update((byte)(ct & byte.MaxValue));
            sm3Digest.doFinal(key, 0);
            keyOff = 0;
            ++ct;
        }

        public virtual ECPoint Init_enc(SM2CryptoServiceProvider sm2, ECPoint userKey)
        {
            var str = userKey.GetEncoded().byteToHex();
            if (str.Length > 64)
                str = str.Substring(0, 64);
            var bigInteger = new BigInteger(str, 16);
            var ecPoint = sm2.ecc_point_g.Multiply(bigInteger);
            p2 = userKey.Multiply(bigInteger);
            Reset();
            return ecPoint;
        }

        public virtual void Encrypt(byte[] data)
        {
            sm3c3.update(data, 0, data.Length);
            for (var index = 0; index < data.Length; ++index)
            {
                if (keyOff == key.Length)
                    NextKey();
                data[index] ^= key[keyOff++];
            }
        }

        public virtual void Init_dec(BigInteger userD, ECPoint c1)
        {
            p2 = c1.Multiply(userD);
            Reset();
        }

        public virtual void Decrypt(byte[] data)
        {
            for (var index = 0; index < data.Length; ++index)
            {
                if (keyOff == key.Length)
                    NextKey();
                data[index] ^= key[keyOff++];
            }
            sm3c3.update(data, 0, data.Length);
        }

        public virtual void Dofinal(byte[] c3)
        {
            var byteArray = p2.Normalize().YCoord.ToBigInteger().ToByteArray();
            sm3c3.update(byteArray, 0, byteArray.Length);
            sm3c3.doFinal(c3, 0);
            Reset();
        }
    }
}