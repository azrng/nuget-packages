using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Azrng.Core.Extension
{
    /// <summary>
    /// 字符串扩展
    /// </summary>
    public static class StringExtension
    {
        #region 基础类型判断

        /// <summary>
        /// 是否是bool类型
        /// </summary>
        /// <param name="boolStr"></param>
        /// <returns></returns>
        public static bool IsBool(this string? boolStr)
        {
            if (boolStr.IsNullOrWhiteSpace())
                return false;
            return boolStr!.Trim().ToLowerInvariant() switch
            {
                "0" => false,
                "1" => true,
                "是" => true,
                "否" => false,
                "yes" => true,
                _ => false
            };
        }

        /// <summary>
        /// 判断字符串是否是int类型
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsIntFormat(this string? str)
        {
            return int.TryParse(str, out _);
        }

        /// <summary>
        /// 字符串转int
        /// </summary>
        /// <param name="source"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public static int ToInt(this string? source, int @default = 0)
        {
            return source.ToInt32(@default);
        }

        /// <summary>
        /// 字符串转int
        /// </summary>
        /// <param name="source"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public static int ToInt32(this string? source, int @default = 0)
        {
            var result = int.TryParse(source, out var value);
            return result ? value : @default;
        }

        /// <summary>
        /// 判断字符串是否是float类型
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsFloatFormat(this string? str)
        {
            return float.TryParse(str, out _);
        }

        /// <summary>
        /// 字符串转float
        /// </summary>
        /// <param name="source"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public static float ToFloat(this string? source, float @default = 0)
        {
            var result = float.TryParse(source, out var value);
            return result ? value : @default;
        }

        /// <summary>
        /// 判断字符串是否是double类型
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDoubleFormat(this string? str)
        {
            return double.TryParse(str, out _);
        }

        /// <summary>
        /// 字符串转float
        /// </summary>
        /// <param name="source"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public static double ToDouble(this string? source, double @default = 0)
        {
            var result = double.TryParse(source, out var value);
            return result ? value : @default;
        }

        /// <summary>
        /// 判断字符串是否是long类型
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsInt64Format(this string? str)
        {
            return long.TryParse(str, out _);
        }

        /// <summary>
        /// 字符串转long
        /// </summary>
        /// <param name="source"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public static long ToInt64(this string? source, long @default = 0)
        {
            var result = long.TryParse(source, out var value);
            return result ? value : @default;
        }

        /// <summary>
        /// 判断字符串是否是decimal类型
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDecimalFormat(this string? str)
        {
            return decimal.TryParse(str, out _);
        }

        /// <summary>
        /// 字符串转decimal
        /// </summary>
        /// <param name="source"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this string? source, decimal @default = 0)
        {
            var result = decimal.TryParse(source, out var value);
            return result ? value : @default;
        }

        /// <summary>
        /// 判断字符串是否是日期类型
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDateFormat(this string? str)
        {
            return DateTime.TryParse(str, out _);
        }

        /// <summary>
        /// 字符串转可空日期类型
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static DateTime? ToDateTime(this string? str)
        {
            var datetime = DateTime.TryParse(str, out var date);
            if (datetime)
            {
                return date;
            }

            return null;
        }

        /// <summary>
        /// 十六进制字符串转二进制
        /// </summary>
        /// <param name="hexStr"></param>
        /// <returns></returns>
        public static byte[] ToBytesFromHexString(this string hexStr)
        {
            if (hexStr.Length == 0)
                return new byte[1];
            if (hexStr.Length % 2 == 1)
                hexStr = "0" + hexStr;
            var bytes = new byte[hexStr.Length / 2];
            for (var index = 0; index < hexStr.Length / 2; ++index)
                bytes[index] = byte.Parse(hexStr.Substring(2 * index, 2), NumberStyles.AllowHexSpecifier);
            return bytes;
        }

        /// <summary>
        /// 字符串转二进制
        /// </summary>
        /// <param name="str"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this string str, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return encoding.GetBytes(str);
        }

        /// <summary>
        /// 字符串转 Guid
        /// </summary>
        public static Guid ToGuid(this string? source, Guid @default = default)
        {
            var result = Guid.TryParse(source, out var value);
            return result ? value : @default;
        }

        #endregion

        /// <summary>
        /// 判断字符串是否包含中文
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasChinese(this string? str)
        {
            return !str.IsNullOrWhiteSpace() && Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
        }

        #region 值判断

        /// <summary>
        /// 判断字符串不是  null、空和空白字符
        /// </summary>
        /// <param name="currentString"></param>
        /// <returns></returns>
        public static bool IsNotNullOrWhiteSpace(this string? currentString)
        {
            return !string.IsNullOrWhiteSpace(currentString);
        }

        /// <summary>
        /// 判断字符串 是  null、空和空白字符
        /// </summary>
        /// <param name="currentString"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string? currentString)
        {
            return string.IsNullOrWhiteSpace(currentString);
        }

        /// <summary>
        /// 判断字符串是  null、空
        /// </summary>
        /// <param name="currentString"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string? currentString)
        {
            return string.IsNullOrEmpty(currentString);
        }

        /// <summary>
        /// 判断字符串不是  null、空
        /// </summary>
        /// <param name="currentString"></param>
        /// <returns></returns>
        public static bool IsNotNullOrEmpty(this string? currentString)
        {
            return !string.IsNullOrEmpty(currentString);
        }

        #endregion

        /// <summary>
        /// 获取值或者返回默认值
        /// </summary>
        /// <param name="currStr"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetOrDefault(this string? currStr, string defaultValue)
        {
            return (currStr.IsNullOrWhiteSpace() ? defaultValue : currStr)!;
        }

        /// <summary>
        /// 字符串分割成字符串数组。
        /// </summary>
        /// <param name="str"></param>
        /// <param name="separator">分隔符，默认为逗号。</param>
        /// <returns></returns>
        public static string[] ToStrArray(this string str, string separator = ",")
        {
            return str.Split([separator], StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// 获取特定位置的字符串
        /// </summary>
        /// <param name="str">源字符串</param>
        /// <param name="index">位置</param>
        /// <returns>超出范围返回空字符串</returns>
        public static string GetByIndex(this string str, int index)
        {
            if (index < 0 || index > str.Length - 1)
            {
                return string.Empty;
            }

            return str[index].ToString();
        }

        /// <summary>
        /// 忽略大小写的字符串比较
        /// </summary>
        /// <param name="aimStr">目标字符串</param>
        /// <param name="compareStr">对比字符串</param>
        /// <param name="stringComparison"><inheritdoc cref="StringComparison"/></param>
        /// <returns></returns>
        public static bool EqualsNoCase(this string? aimStr, string? compareStr,
                                        StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
        {
            if (aimStr is null || compareStr is null)
                return false;
            return aimStr.Equals(compareStr, stringComparison);
        }

        /// <summary>
        /// 移除控制字符(不可见Unicode字符)
        /// </summary>
        /// <remarks>UniCode编码表及部分不可见字符过滤方案 - https://www.cnblogs.com/fan-yuan/p/8176886.html</remarks>
        public static string? RemoveControlChars(this string? text)
        {
            return text == null ? null : string.Concat(text.Where(c => !char.IsControl(c)));
        }

        /// <summary>
        /// 字符串首字母大写
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static unsafe string? ToUpperFirst(this string? str)
        {
            if (str == null) return null;
            var ret = string.Copy(str);
            fixed (char* ptr = ret)
                *ptr = char.ToUpper(*ptr);
            return ret;
        }

        #region Base64

        /// <summary>
        /// 将base64格式字符串转为byte[]
        /// </summary>
        /// <param name="base64Str"></param>
        /// <returns></returns>
        public static byte[] ToBytesByBase64(this string base64Str)
        {
            return Convert.FromBase64String(base64Str);
        }

        /// <summary>
        /// Base64编码
        /// </summary>
        /// <param name="source">待编码的明文</param>
        /// <param name="encodeType">加密采用的编码方式</param>
        /// <returns></returns>
        public static string ToBase64Encode(this string source, Encoding? encodeType = null)
        {
            encodeType ??= Encoding.UTF8;
            var bytes = encodeType.GetBytes(source);
            try
            {
                return Convert.ToBase64String(bytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Base64解码
        /// </summary>
        /// <param name="result">待解密的密文</param>
        /// <param name="encodeType">解密采用的编码方式，注意和加密时采用的方式一致</param>
        /// <returns>解密后的字符串</returns>
        public static string FromBase64Decode(this string result, Encoding? encodeType = null)
        {
            encodeType ??= Encoding.UTF8;
            result = result.Replace(" ", "+");

            var mod4 = result.Length % 4;
            if (mod4 > 0)
            {
                result += new string('=', 4 - mod4);
            }

            var bytes = Convert.FromBase64String(result);

            try
            {
                return encodeType.GetString(bytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion

        /// <summary>
        /// 根据版本字符串转数值类型版本号
        /// </summary>
        /// <param name="versionNumber">1.1.1输出数值格式：10101</param>
        /// <returns></returns>
        public static int ToVersionNumber(this string? versionNumber)
        {
            if (versionNumber.IsNullOrWhiteSpace())
                return 0;
            var splitVersion = versionNumber!.ToLower()
                                            .Replace("v", "")
                                            .Replace(".txt", "")
                                            .Split('.');

            if (splitVersion.Length < 3) return 0;

            if (splitVersion.Length == 3)
            {
                var firstNumber = splitVersion[0].ToInt();
                var secondNumber = splitVersion[1].ToInt();
                var thirdNumber = splitVersion[2].ToInt();
                return firstNumber * 10000 + secondNumber * 100 + thirdNumber;
            }
            else
            {
                var firstNumber = splitVersion[0].ToInt();
                var secondNumber = splitVersion[1].ToInt();
                var thirdNumber = splitVersion[2].ToInt();
                var fourthNumber = splitVersion[3].ToInt();
                return firstNumber * 1000000 + secondNumber * 10000 + thirdNumber * 100 + fourthNumber;
            }
        }
    }
}