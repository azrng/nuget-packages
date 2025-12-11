using Common.SecurityCrypto.Extensions;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Text;

namespace Common.SecurityCrypto.SMCrypto
{
    public class SM3
    {
        public static int[] Tj = new int[64];

        public static string Hash(string str)
        {
            var sm3Digest = new SM3Digest();
            var out1 = new byte[sm3Digest.getDigestSize()];
            var bytes = Encoding.UTF8.GetBytes(str);
            sm3Digest.update(bytes, 0, bytes.Length);
            sm3Digest.doFinal(out1, 0);
            return Encoding.UTF8.GetString(Hex.Encode(out1)).ToUpper();
        }

        static SM3()
        {
            for (var index = 0; index < 16; ++index)
                Tj[index] = 2043430169;
            for (var index = 16; index < 64; ++index)
                Tj[index] = 2055708042;
        }

        public static byte[] CF(byte[] V, byte[] B) => convert(CF(convert(V), convert(B)));

        private static int[] convert(byte[] arr)
        {
            var numArray1 = new int[arr.Length / 4];
            var numArray2 = new byte[4];
            for (var sourceIndex = 0; sourceIndex < arr.Length; sourceIndex += 4)
            {
                Array.Copy(arr, sourceIndex, numArray2, 0, 4);
                numArray1[sourceIndex / 4] = bigEndianByteToInt(numArray2);
            }
            return numArray1;
        }

        private static byte[] convert(int[] arr)
        {
            var destinationArray = new byte[arr.Length * 4];
            for (var index = 0; index < arr.Length; ++index)
                Array.Copy(bigEndianIntToByte(arr[index]), 0, destinationArray, index * 4, 4);
            return destinationArray;
        }

        public static int[] CF(int[] V, int[] B)
        {
            var num1 = V[0];
            var num2 = V[1];
            var Z1 = V[2];
            var num3 = V[3];
            var X1 = V[4];
            var num4 = V[5];
            var Z2 = V[6];
            var num5 = V[7];
            var numArray1 = expand(B);
            var numArray2 = numArray1[0];
            var numArray3 = numArray1[1];
            for (var index = 0; index < 64; ++index)
            {
                var num6 = bitCycleLeft(bitCycleLeft(num1, 12) + X1 + bitCycleLeft(Tj[index], index), 7);
                var num7 = num6 ^ bitCycleLeft(num1, 12);
                var num8 = FFj(num1, num2, Z1, index) + num3 + num7 + numArray3[index];
                var X2 = GGj(X1, num4, Z2, index) + num5 + num6 + numArray2[index];
                num3 = Z1;
                Z1 = bitCycleLeft(num2, 9);
                num2 = num1;
                num1 = num8;
                num5 = Z2;
                Z2 = bitCycleLeft(num4, 19);
                num4 = X1;
                X1 = P0(X2);
            }
            return new int[8]
            {
        num1 ^ V[0],
        num2 ^ V[1],
        Z1 ^ V[2],
        num3 ^ V[3],
        X1 ^ V[4],
        num4 ^ V[5],
        Z2 ^ V[6],
        num5 ^ V[7]
            };
        }

        private static int[][] expand(int[] B)
        {
            var numArray1 = new int[68];
            var numArray2 = new int[64];
            for (var index = 0; index < B.Length; ++index)
                numArray1[index] = B[index];
            for (var index = 16; index < 68; ++index)
                numArray1[index] = P1(numArray1[index - 16] ^ numArray1[index - 9] ^ bitCycleLeft(numArray1[index - 3], 15)) ^ bitCycleLeft(numArray1[index - 13], 7) ^ numArray1[index - 6];
            for (var index = 0; index < 64; ++index)
                numArray2[index] = numArray1[index] ^ numArray1[index + 4];
            return new int[2][] { numArray1, numArray2 };
        }

        private static byte[] bigEndianIntToByte(int num) => back(num.intToBytes());

        private static int bigEndianByteToInt(byte[] bytes) => back(bytes).byteToInt();

        private static int FFj(int X, int Y, int Z, int j) => j >= 0 && j <= 15 ? FF1j(X, Y, Z) : FF2j(X, Y, Z);

        private static int GGj(int X, int Y, int Z, int j) => j >= 0 && j <= 15 ? GG1j(X, Y, Z) : GG2j(X, Y, Z);

        private static int FF1j(int X, int Y, int Z) => X ^ Y ^ Z;

        private static int FF2j(int X, int Y, int Z) => X & Y | X & Z | Y & Z;

        private static int GG1j(int X, int Y, int Z) => X ^ Y ^ Z;

        private static int GG2j(int X, int Y, int Z) => X & Y | ~X & Z;

        private static int P0(int X)
        {
            rotateLeft(X, 9);
            var num1 = bitCycleLeft(X, 9);
            rotateLeft(X, 17);
            var num2 = bitCycleLeft(X, 17);
            return X ^ num1 ^ num2;
        }

        private static int P1(int X) => X ^ bitCycleLeft(X, 15) ^ bitCycleLeft(X, 23);

        public static byte[] padding(byte[] in1, int bLen)
        {
            var num1 = 448 - (8 * in1.Length + 1) % 512;
            if (num1 < 0)
                num1 = 960 - (8 * in1.Length + 1) % 512;
            var num2 = num1 + 1;
            var sourceArray1 = new byte[num2 / 8];
            sourceArray1[0] = 128;
            long num3 = in1.Length * 8 + bLen * 512;
            var destinationArray = new byte[in1.Length + num2 / 8 + 8];
            var num4 = 0;
            Array.Copy(in1, 0, destinationArray, 0, in1.Length);
            var destinationIndex1 = num4 + in1.Length;
            Array.Copy(sourceArray1, 0, destinationArray, destinationIndex1, sourceArray1.Length);
            var destinationIndex2 = destinationIndex1 + sourceArray1.Length;
            var sourceArray2 = back(num3.longToBytes());
            Array.Copy(sourceArray2, 0, destinationArray, destinationIndex2, sourceArray2.Length);
            return destinationArray;
        }

        private static byte[] back(byte[] in1)
        {
            var numArray = new byte[in1.Length];
            for (var index = 0; index < numArray.Length; ++index)
                numArray[index] = in1[numArray.Length - index - 1];
            return numArray;
        }

        public static int rotateLeft(int x, int n) => x << n | x >> 32 - n;

        private static int bitCycleLeft(int n, int bitLen)
        {
            bitLen %= 32;
            var numArray = bigEndianIntToByte(n);
            var byteLen = bitLen / 8;
            var len = bitLen % 8;
            if (byteLen > 0)
                numArray = byteCycleLeft(numArray, byteLen);
            if (len > 0)
                numArray = bitSmall8CycleLeft(numArray, len);
            return bigEndianByteToInt(numArray);
        }

        private static byte[] bitSmall8CycleLeft(byte[] in1, int len)
        {
            var numArray = new byte[in1.Length];
            for (var index = 0; index < numArray.Length; ++index)
            {
                int num = (byte)((byte)((in1[index] & byte.MaxValue) << len) | (uint)(byte)((in1[(index + 1) % numArray.Length] & byte.MaxValue) >> 8 - len));
                numArray[index] = (byte)num;
            }
            return numArray;
        }

        private static byte[] byteCycleLeft(byte[] in1, int byteLen)
        {
            var destinationArray = new byte[in1.Length];
            Array.Copy(in1, byteLen, destinationArray, 0, in1.Length - byteLen);
            Array.Copy(in1, 0, destinationArray, in1.Length - byteLen, byteLen);
            return destinationArray;
        }
    }
}