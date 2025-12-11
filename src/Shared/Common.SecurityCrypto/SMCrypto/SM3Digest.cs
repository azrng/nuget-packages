using System;

namespace Common.SecurityCrypto.SMCrypto
{
    public class SM3Digest
    {
        private static int BYTE_LENGTH = 32;
        private static int BLOCK_LENGTH = 64;
        private static int BUFFER_LENGTH = BLOCK_LENGTH;
        private byte[] xBuf = new byte[BUFFER_LENGTH];
        private int xBufOff;

        public static byte[] iv = new byte[32]
        {
       115,
       128,
       22,
       111,
       73,
       20,
       178,
       185,
       23,
       36,
       66,
       215,
       218,
       138,
       6,
       0,
       169,
       111,
       48,
       188,
       22,
       49,
       56,
       170,
       227,
       141,
       238,
       77,
       176,
       251,
       14,
       78
        };

        private byte[] V = (byte[])iv.Clone();
        private int cntBlock = 0;

        public SM3Digest()
        {
        }

        public SM3Digest(SM3Digest t)
        {
            Array.Copy(t.xBuf, 0, xBuf, 0, t.xBuf.Length);
            xBufOff = t.xBufOff;
            Array.Copy(t.V, 0, V, 0, t.V.Length);
        }

        public int doFinal(byte[] out1, int outOff)
        {
            var sourceArray = doFinal();
            Array.Copy(sourceArray, 0, out1, 0, sourceArray.Length);
            return BYTE_LENGTH;
        }

        public void reset()
        {
            xBufOff = 0;
            cntBlock = 0;
            V = (byte[])iv.Clone();
        }

        public void update(byte[] in1, int inOff, int len)
        {
            var length1 = BUFFER_LENGTH - xBufOff;
            var length2 = len;
            var sourceIndex = inOff;
            if (length1 < length2)
            {
                Array.Copy(in1, sourceIndex, xBuf, xBufOff, length1);
                length2 -= length1;
                sourceIndex += length1;
                doUpdate();
                while (length2 > BUFFER_LENGTH)
                {
                    Array.Copy(in1, sourceIndex, xBuf, 0, BUFFER_LENGTH);
                    length2 -= BUFFER_LENGTH;
                    sourceIndex += BUFFER_LENGTH;
                    doUpdate();
                }
            }
            Array.Copy(in1, sourceIndex, xBuf, xBufOff, length2);
            xBufOff += length2;
        }

        private void doUpdate()
        {
            var numArray = new byte[BLOCK_LENGTH];
            for (var sourceIndex = 0; sourceIndex < BUFFER_LENGTH; sourceIndex += BLOCK_LENGTH)
            {
                Array.Copy(xBuf, sourceIndex, numArray, 0, numArray.Length);
                doHash(numArray);
            }
            xBufOff = 0;
        }

        private void doHash(byte[] B)
        {
            Array.Copy(SM3.CF(V, B), 0, V, 0, V.Length);
            ++cntBlock;
        }

        private byte[] doFinal()
        {
            var numArray1 = new byte[BLOCK_LENGTH];
            var numArray2 = new byte[xBufOff];
            Array.Copy(xBuf, 0, numArray2, 0, numArray2.Length);
            var sourceArray = SM3.padding(numArray2, cntBlock);
            for (var sourceIndex = 0; sourceIndex < sourceArray.Length; sourceIndex += BLOCK_LENGTH)
            {
                Array.Copy(sourceArray, sourceIndex, numArray1, 0, numArray1.Length);
                doHash(numArray1);
            }
            return V;
        }

        public void update(byte in1) => update(new byte[1]
        {
      in1
        }, 0, 1);

        public int getDigestSize() => BYTE_LENGTH;
    }
}