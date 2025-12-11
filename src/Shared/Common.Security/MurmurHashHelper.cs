using System.Text;

namespace Common.Security
{
    /// <summary>
    ///  Google 出品的 MurmurHash 算法，
    ///  MurmurHash 是一种非加密型哈希函数，适用于一般的哈希检索操作。
    ///  与其它流行的哈希函数相比，对于规律性较强的 key，MurmurHash 的随机分布特征表现更良好。
    ///  非加密意味着着相比 MD5，SHA 这些函数它的性能肯定更高（实际上性能是 MD5 等加密算法的十倍以上）
    /// </summary>
    public static class MurmurHashHelper
    {
        #region MakeHash64Bit加密

        /// <summary>
        /// MakeHash64Bit加密
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static ulong MakeHash64BValue(byte[] key, uint seed = 0xee6b27eb)
        {
            uint len = (uint)key.Length;
            const uint m = 0x5bd1e995;
            const int r = 24;

            uint h1 = seed ^ len;
            uint h2 = 0;
            int pos = 0;

            while (len >= 8)
            {
                uint k1 = System.BitConverter.ToUInt32(key, pos);
                pos += 4;
                k1 *= m;
                k1 ^= k1 >> r;
                k1 *= m;
                h1 *= m;
                h1 ^= k1;
                len -= 4;

                uint k2 = System.BitConverter.ToUInt32(key, pos);
                pos += 4;
                k2 *= m;
                k2 ^= k2 >> r;
                k2 *= m;
                h2 *= m;
                h2 ^= k2;
                len -= 4;
            }

            if (len >= 4)
            {
                uint k1 = System.BitConverter.ToUInt32(key, pos);
                pos += 4;
                k1 *= m;
                k1 ^= k1 >> r;
                k1 *= m;
                h1 *= m;
                h1 ^= k1;
                len -= 4;
            }

            if (len == 3)
            {
                h2 ^= (uint)key[2] << 16;
                h2 ^= (uint)key[1] << 8;
                h2 ^= key[0];
                h2 *= m;
            }
            else if (len == 2)
            {
                h2 ^= (uint)key[1] << 8;
                h2 ^= key[0];
                h2 *= m;
            }
            else if (len == 1)
            {
                h2 ^= key[0];
                h2 *= m;
            }

            h1 ^= h2 >> 18;
            h1 *= m;
            h2 ^= h1 >> 22;
            h2 *= m;
            h1 ^= h2 >> 17;
            h1 *= m;
            h2 ^= h1 >> 19;
            h2 *= m;

            ulong h = h1;

            h = (h << 32) | h2;

            return h;
        }

        #endregion MakeHash64Bit加密

        public static ulong StringToHashValue(string source)
        {
            var bytes = Encoding.UTF8.GetBytes(source);
            return MakeHash64BValue(bytes);
        }
    }
}