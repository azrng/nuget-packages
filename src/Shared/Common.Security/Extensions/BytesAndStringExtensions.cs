using Common.Security.Enums;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Globalization;
using System.Text;

namespace Common.Security.Extensions
{
    internal static class BytesAndStringExtensions
    {
        /// <summary>
        /// 转十六进制
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static string ToHexString(this byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            // ToHexString和BitConverter的区别是前者输出为小写，后者为大写

#if NET6_0_OR_GREATER
            return Convert.ToHexString(bytes);
#endif

            return Hex.ToHexString(bytes);

            // Convert.ToHexString() // .net6以及之上版本支持
            //return BitConverter.ToString(bytes).Replace("-", string.Empty);
        }

        /// <summary>
        /// 字符串转二进制
        /// </summary>
        /// <param name="data"></param>
        /// <param name="outType"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal static byte[] GetBytes(this string data, OutType outType)
        {
            return outType switch
            {
                OutType.Base64 => Convert.FromBase64String(data),
                OutType.Hex => Hex.Decode(data),
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// 字符串转二进制
        /// </summary>
        /// <param name="data"></param>
        /// <param name="secretType"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal static byte[] GetBytes(this string data, SecretType secretType)
        {
            return secretType switch
            {
                SecretType.Text => Encoding.UTF8.GetBytes(data),
                SecretType.Base64 => Convert.FromBase64String(data),
                SecretType.Hex => Hex.Decode(data),
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// 二进制转字符串
        /// </summary>
        /// <param name="data"></param>
        /// <param name="outType"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal static string GetString(this byte[] data, OutType outType)
        {
            return outType switch
            {
                OutType.Base64 => Convert.ToBase64String(data),
                OutType.Hex => data.ToHexString(),
                _ => throw new ArgumentOutOfRangeException(nameof(outType), outType, null)
            };
        }

        /// <summary>
        /// 十六进制转二进制
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        internal static byte[] ToBytesFromHex(this string hex)
        {
            if (hex.Length == 0)
                return new byte[1];
            if (hex.Length % 2 == 1)
                hex = "0" + hex;
            var bytes = new byte[hex.Length / 2];
            for (var index = 0; index < hex.Length / 2; ++index)
                bytes[index] = byte.Parse(hex.Substring(2 * index, 2), NumberStyles.AllowHexSpecifier);
            return bytes;
        }

        internal static byte[] GetBytes(this string strUtf8) =>
            string.IsNullOrEmpty(strUtf8) || strUtf8.Length == 0
                ? new byte[1]
                : Encoding.UTF8.GetBytes(strUtf8);

        public static byte[] HexToByte(this string hex) => Hex.Decode(hex);

        public static string ByteToHex(this byte[] b) =>
            b != null
                ? Hex.ToHexString(b)
                : throw new ArgumentException("Argument b ( byte array ) is null! ");

        internal static byte[] AsciiBytes(string s)
        {
            var numArray = new byte[s.Length];
            for (var index = 0; index < s.Length; ++index)
                numArray[index] = (byte)s[index];
            return numArray;
        }

        internal static byte[] HexToByteArray(this string hexString)
        {
            var byteArray = new byte[hexString.Length / 2];
            for (var startIndex = 0; startIndex < hexString.Length; startIndex += 2)
            {
                var s = hexString.Substring(startIndex, 2);
                byteArray[startIndex / 2] = byte.Parse(s, NumberStyles.HexNumber, null);
            }

            return byteArray;
        }

        private static string ByteArrayToHex(this byte[] bytes)
        {
            var stringBuilder = new StringBuilder(bytes.Length * 2);
            foreach (var num in bytes)
                stringBuilder.Append(num.ToString("X2"));
            return stringBuilder.ToString();
        }

        public static string ByteArrayToHex(this byte[] bytes, int len) => bytes.ByteArrayToHex().Substring(0, len * 2);

        public static byte[] RepeatByte(byte b, int count)
        {
            var numArray = new byte[count];
            for (var index = 0; index < count; ++index)
                numArray[index] = b;
            return numArray;
        }

        public static byte[] SubBytes(this byte[] bytes, int startIndex, int length)
        {
            var destinationArray = new byte[length];
            Array.Copy(bytes, startIndex, destinationArray, 0, length);
            return destinationArray;
        }

        public static byte[] XOR(this byte[] value)
        {
            var numArray = new byte[value.Length];
            for (var index = 0; index < value.Length; ++index)
                numArray[index] ^= value[index];
            return numArray;
        }

        public static byte[] XOR(this byte[] valueA, byte[] valueB)
        {
            var length = valueA.Length;
            var numArray = new byte[length];
            for (var index = 0; index < length; ++index)
                numArray[index] = (byte)(valueA[index] ^ (uint)valueB[index]);
            return numArray;
        }

        public static byte[] intToBytes(this int num) =>
            new byte[4]
            {
                (byte)(byte.MaxValue & num),
                (byte)(byte.MaxValue & num >> 8),
                (byte)(byte.MaxValue & num >> 16),
                (byte)(byte.MaxValue & num >> 24)
            };

        public static int byteToInt(this byte[] bytes) =>
            0 |
            byte.MaxValue & bytes[0] |
            (byte.MaxValue & bytes[1]) << 8 |
            (byte.MaxValue & bytes[2]) << 16 |
            (byte.MaxValue & bytes[3]) << 24;

        public static byte[] longToBytes(this long num)
        {
            var bytes = new byte[8];
            for (var index = 0; index < 8; ++index)
                bytes[index] = (byte)(byte.MaxValue & (ulong)(num >> index * 8));
            return bytes;
        }

        public static byte[] byteConvert32Bytes(this BigInteger n)
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

        public static BigInteger byteConvertInteger(this byte[] b)
        {
            if (b[0] >= 0)
                return new BigInteger(b);
            var destinationArray = new byte[b.Length + 1];
            destinationArray[0] = 0;
            Array.Copy(b, 0, destinationArray, 1, b.Length);
            return new BigInteger(destinationArray);
        }

        internal static TEnum ToEnum<TEnum>(this string value) where TEnum : struct
        {
            TEnum result;
            if (Enum.TryParse(value, true, out result))
                return result;
            foreach (var obj in Enum.GetValues(typeof(TEnum)))
            {
                if (obj.ToString().ToLower().Contains(value.ToLower()))
                    return (TEnum)obj;
            }

            return default;
        }
    }
}