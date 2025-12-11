using System;
using System.Collections.Generic;

namespace Common.SecurityCrypto.SMCrypto
{
    public class SM4CryptoServiceProviderJS : SM4CryptoServiceProvider
    {
        private int GET_ULONG_BE(byte[] b, int i) => (int)((long)((b[i] & byte.MaxValue) << 24 | (b[i + 1] & byte.MaxValue) << 16 | (b[i + 2] & byte.MaxValue) << 8) | b[i + 3] & byte.MaxValue & uint.MaxValue);

        private void PUT_ULONG_BE(int n, byte[] b, int i)
        {
            b[i] = (byte)(byte.MaxValue & n >> 24);
            b[i + 1] = (byte)(byte.MaxValue & n >> 16);
            b[i + 2] = (byte)(byte.MaxValue & n >> 8);
            b[i + 3] = (byte)(byte.MaxValue & n);
        }

        private int SHL(int x, int n) => (int)(x & uint.MaxValue) << n;

        private int ROTL(int x, int n) => SHL(x, n) | x >> 32 - n;

        private void SWAP(int[] sk, int i)
        {
            var num = sk[i];
            sk[i] = sk[31 - i];
            sk[31 - i] = num;
        }

        private byte sm4Sbox(byte inch) => SboxTable[inch & byte.MaxValue];

        private int sm4Lt(int ka)
        {
            var b1 = new byte[4];
            var b2 = new byte[4];
            PUT_ULONG_BE(ka, b1, 0);
            b2[0] = sm4Sbox(b1[0]);
            b2[1] = sm4Sbox(b1[1]);
            b2[2] = sm4Sbox(b1[2]);
            b2[3] = sm4Sbox(b1[3]);
            var ulongBe = GET_ULONG_BE(b2, 0);
            return ulongBe ^ ROTL(ulongBe, 2) ^ ROTL(ulongBe, 10) ^ ROTL(ulongBe, 18) ^ ROTL(ulongBe, 24);
        }

        private int sm4F(int x0, int x1, int x2, int x3, int rk) => x0 ^ sm4Lt(x1 ^ x2 ^ x3 ^ rk);

        private int sm4CalciRK(int ka)
        {
            var b1 = new byte[4];
            var b2 = new byte[4];
            PUT_ULONG_BE(ka, b1, 0);
            b2[0] = sm4Sbox(b1[0]);
            b2[1] = sm4Sbox(b1[1]);
            b2[2] = sm4Sbox(b1[2]);
            b2[3] = sm4Sbox(b1[3]);
            var ulongBe = GET_ULONG_BE(b2, 0);
            return ulongBe ^ ROTL(ulongBe, 13) ^ ROTL(ulongBe, 23);
        }

        private void sm4_setkey(int[] SK, byte[] key)
        {
            var numArray1 = new int[4];
            var numArray2 = new int[36];
            var index = 0;
            numArray1[0] = GET_ULONG_BE(key, 0);
            numArray1[1] = GET_ULONG_BE(key, 4);
            numArray1[2] = GET_ULONG_BE(key, 8);
            numArray1[3] = GET_ULONG_BE(key, 12);
            numArray2[0] = numArray1[0] ^ (int)FK[0];
            numArray2[1] = numArray1[1] ^ (int)FK[1];
            numArray2[2] = numArray1[2] ^ (int)FK[2];
            numArray2[3] = numArray1[3] ^ (int)FK[3];
            for (; index < 32; ++index)
            {
                numArray2[index + 4] = numArray2[index] ^ sm4CalciRK(numArray2[index + 1] ^ numArray2[index + 2] ^ numArray2[index + 3] ^ (int)CK[index]);
                SK[index] = numArray2[index + 4];
            }
        }

        private void sm4_one_round(int[] sk, byte[] input, byte[] output)
        {
            var index = 0;
            var numArray = new int[36];
            numArray[0] = GET_ULONG_BE(input, 0);
            numArray[1] = GET_ULONG_BE(input, 4);
            numArray[2] = GET_ULONG_BE(input, 8);
            numArray[3] = GET_ULONG_BE(input, 12);
            for (; index < 32; ++index)
                numArray[index + 4] = sm4F(numArray[index], numArray[index + 1], numArray[index + 2], numArray[index + 3], sk[index]);
            PUT_ULONG_BE(numArray[35], output, 0);
            PUT_ULONG_BE(numArray[34], output, 4);
            PUT_ULONG_BE(numArray[33], output, 8);
            PUT_ULONG_BE(numArray[32], output, 12);
        }

        private byte[] padding(byte[] input, int mode)
        {
            if (input == null)
                return null;
            byte[] destinationArray;
            if (mode == 1)
            {
                var num = 16 - input.Length % 16;
                destinationArray = new byte[input.Length + num];
                Array.Copy((Array)input, 0, (Array)destinationArray, 0, input.Length);
                for (var index = 0; index < num; ++index)
                    destinationArray[input.Length + index] = (byte)num;
            }
            else
            {
                var num = (int)input[input.Length - 1];
                destinationArray = new byte[input.Length - num];
                Array.Copy((Array)input, 0, (Array)destinationArray, 0, input.Length - num);
            }
            return destinationArray;
        }

        public void sm4_setkey_enc(SM4ContextJS ctx, byte[] key)
        {
            ctx.mode = 1;
            sm4_setkey(ctx.sk, key);
        }

        public void sm4_setkey_dec(SM4ContextJS ctx, byte[] key)
        {
            ctx.mode = 0;
            sm4_setkey(ctx.sk, key);
            for (var i = 0; i < 16; ++i)
                SWAP(ctx.sk, i);
        }

        public byte[] sm4_crypt_ecb(SM4ContextJS ctx, byte[] input)
        {
            if (ctx.isPadding && ctx.mode == 1)
                input = padding(input, 1);
            var length = input.Length;
            var numArray1 = new byte[length];
            Array.Copy((Array)input, 0, (Array)numArray1, 0, length);
            var numArray2 = new byte[length];
            var num = 0;
            while (length > 0)
            {
                var numArray3 = new byte[16];
                var numArray4 = new byte[16];
                Array.Copy((Array)numArray1, num * 16, (Array)numArray3, 0, length > 16 ? 16 : length);
                sm4_one_round(ctx.sk, numArray3, numArray4);
                Array.Copy((Array)numArray4, 0, (Array)numArray2, num * 16, length > 16 ? 16 : length);
                length -= 16;
                ++num;
            }
            if (ctx.isPadding && ctx.mode == 0)
                numArray2 = padding(numArray2, 0);
            return numArray2;
        }

        public byte[] sm4_crypt_cbc(SM4ContextJS ctx, byte[] iv, byte[] input)
        {
            if (ctx.isPadding && ctx.mode == 1)
                input = padding(input, 1);
            var length = input.Length;
            var numArray1 = new byte[length];
            Array.Copy((Array)input, 0, (Array)numArray1, 0, length);
            var byteList = new List<byte>();
            if (ctx.mode == 1)
            {
                var num = 0;
                while (length > 0)
                {
                    var destinationArray = new byte[16];
                    var input1 = new byte[16];
                    var numArray2 = new byte[16];
                    Array.Copy((Array)numArray1, num * 16, (Array)destinationArray, 0, length > 16 ? 16 : length);
                    for (var index = 0; index < 16; ++index)
                        input1[index] = (byte)(destinationArray[index] ^ (uint)iv[index]);
                    sm4_one_round(ctx.sk, input1, numArray2);
                    Array.Copy((Array)numArray2, 0, (Array)iv, 0, 16);
                    for (var index = 0; index < 16; ++index)
                        byteList.Add(numArray2[index]);
                    length -= 16;
                    ++num;
                }
            }
            else
            {
                var numArray3 = new byte[16];
                var num = 0;
                while (length > 0)
                {
                    var numArray4 = new byte[16];
                    var output = new byte[16];
                    var numArray5 = new byte[16];
                    Array.Copy((Array)numArray1, num * 16, (Array)numArray4, 0, length > 16 ? 16 : length);
                    Array.Copy((Array)numArray4, 0, (Array)numArray3, 0, 16);
                    sm4_one_round(ctx.sk, numArray4, output);
                    for (var index = 0; index < 16; ++index)
                        numArray5[index] = (byte)(output[index] ^ (uint)iv[index]);
                    Array.Copy((Array)numArray3, 0, (Array)iv, 0, 16);
                    for (var index = 0; index < 16; ++index)
                        byteList.Add(numArray5[index]);
                    length -= 16;
                    ++num;
                }
            }
            return ctx.isPadding && ctx.mode == 0 ? this.padding(byteList.ToArray(), 0) : byteList.ToArray();
        }
    }
}