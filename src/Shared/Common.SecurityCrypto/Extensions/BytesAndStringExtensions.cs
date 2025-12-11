using Common.SecurityCrypto.Model;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Globalization;
using System.Text;

namespace Common.SecurityCrypto.Extensions
{
    internal static class BytesAndStringExtensions
    {
        internal static string ToHexString(this byte[] bytes)
        {
            // 或许使用BitConverter.ToString(hashResult).Replace("-", string.Empty);也行
            var stringBuilder = new StringBuilder();
            for (var index = 0; index < bytes.Length; ++index)
                stringBuilder.Append(bytes[index].ToString("X2"));
            return stringBuilder.ToString();
        }

        internal static byte[] GetEncryptBytes(this string data, OutType outType)
        {
            switch (outType)
            {
                case OutType.Base64:
                    return Convert.FromBase64String(data);

                case OutType.Hex:
                    return data.ToBytes();

                default:
                    throw new NotImplementedException();
            }
        }

        internal static byte[] ToBytes(this string hex)
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

        internal static byte[] GetBytes(this string strUtf8) => string.IsNullOrEmpty(strUtf8) || strUtf8.Length == 0 ? new byte[1] : Encoding.UTF8.GetBytes(strUtf8);

        public static byte[] hexToByte(this string hex) => Hex.Decode(hex);

        public static string byteToHex(this byte[] b) => b != null ? Hex.ToHexString(b) : throw new ArgumentException("Argument b ( byte array ) is null! ");

        public static byte[] AsciiBytes(string s)
        {
            var numArray = new byte[s.Length];
            for (var index = 0; index < s.Length; ++index)
                numArray[index] = (byte)s[index];
            return numArray;
        }

        public static byte[] HexToByteArray(this string hexString)
        {
            var byteArray = new byte[hexString.Length / 2];
            for (var startIndex = 0; startIndex < hexString.Length; startIndex += 2)
            {
                var s = hexString.Substring(startIndex, 2);
                byteArray[startIndex / 2] = byte.Parse(s, NumberStyles.HexNumber, null);
            }
            return byteArray;
        }

        public static string ByteArrayToHex(this byte[] bytes)
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

        public static byte[] intToBytes(this int num) => new byte[4]
        {
      (byte) ( byte.MaxValue & num),
      (byte) ( byte.MaxValue & num >> 8),
      (byte) ( byte.MaxValue & num >> 16),
      (byte) ( byte.MaxValue & num >> 24)
        };

        public static int byteToInt(this byte[] bytes) => 0 | byte.MaxValue & bytes[0] | (byte.MaxValue & bytes[1]) << 8 | (byte.MaxValue & bytes[2]) << 16 | (byte.MaxValue & bytes[3]) << 24;

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