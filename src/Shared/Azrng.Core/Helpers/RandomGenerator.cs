using Azrng.Core.Model;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// 随机生成器
    /// </summary>
    public class RandomGenerator
    {
        /// <summary>
        /// 优点：线程安全、避免种子重复、性能优化(避免重复创建新的Random实例)
        /// </summary>
        [Obsolete("对于安全敏感的场景，请使用 RandomGenerator 方法，而不是直接访问 Random。")]
        public static readonly ThreadLocal<Random> Random = new(() =>
            new Random(RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue)));

        /// <summary>
        /// 随机字符
        /// </summary>
        private static string RandomString { set; get; } = "0123456789ABCDEFGHIJKMLNOPQRSTUVWXYZ";

        private static int NextInt32(int minValue, int maxValue)
        {
            return RandomNumberGenerator.GetInt32(minValue, maxValue);
        }

        private static int NextInt32(int maxValue)
        {
            return RandomNumberGenerator.GetInt32(maxValue);
        }

        private static double NextDouble()
        {
            Span<byte> buffer = stackalloc byte[8];
            RandomNumberGenerator.Fill(buffer);
            var value = BitConverter.ToUInt64(buffer);
            return value / ((double)ulong.MaxValue + 1d);
        }

        private static void Shuffle<T>(IList<T> items)
        {
            for (var i = items.Count - 1; i > 0; i--)
            {
                var swapIndex = NextInt32(i + 1);
                (items[i], items[swapIndex]) = (items[swapIndex], items[i]);
            }
        }

        #region 基础随机生成方法

        /// <summary>
        /// 自动生成编号
        /// </summary>
        /// <returns></returns>
        /// <remarks>201008251145409865</remarks>
        public static string GenerateNumber()
        {
            var strRandom = NextInt32(1000, 10000).ToString(); //生成编号
            return DateTime.Now.ToString("yyyyMMddHHmmss") + strRandom; //形如
        }

        /// <summary>
        /// 生成0-9随机数
        /// </summary>
        /// <param name="codeNum">生成长度</param>
        /// <returns></returns>
        public static string GenerateNumber(int codeNum)
        {
            var sb = new StringBuilder(codeNum);
            for (var i = 1; i < codeNum + 1; i++)
            {
                var t = NextInt32(10);
                sb.Append(t);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 生成一个0.0到1.0的随机小数
        /// </summary>
        public static double GenerateDoubleNumber()
        {
            return NextDouble();
        }

        /// <summary>
        /// 生成随机数值
        /// </summary>
        /// <returns></returns>
        public static double GenerateNumber(double start, double end)
        {
            if (start >= end)
                throw new ArgumentOutOfRangeException(nameof(start));

            return start + NextDouble() * (end - start);
        }

        /// <summary>
        /// 生成固定长度随机字符
        /// </summary>
        /// <param name="stringLength"></param>
        /// <returns></returns>
        public static string GenerateString(int stringLength = 6)
        {
            var returnValue = new StringBuilder();
            for (var i = 0; i < stringLength; i++)
            {
                var r = NextInt32(RandomString.Length);
                returnValue.Append(RandomString[r]);
            }

            return returnValue.ToString();
        }

        /// <summary>
        /// 生成随机数值
        /// </summary>
        /// <returns></returns>
        public static int GenerateNumber(int start, int end)
        {
            if (start >= end)
                throw new ArgumentOutOfRangeException(nameof(start));

            return NextInt32(start, end);
        }

        /// <summary>
        /// 得到随机日期
        /// </summary>
        /// <param name="startDateTime">起始日期</param>
        /// <param name="endDateTime">结束日期</param>
        /// <returns>间隔日期之间的 随机日期</returns>
        public static DateTime GenerateDateTime(DateTime startDateTime, DateTime endDateTime)
        {
            var ts = new TimeSpan(startDateTime.Ticks - endDateTime.Ticks);

            // 获取两个时间相隔的秒数
            var dTotalSeconds = ts.TotalSeconds;

            var totalSeconds = dTotalSeconds switch
            {
                > int.MaxValue => int.MaxValue,
                < int.MinValue => int.MinValue,
                _ => (int)dTotalSeconds
            };

            DateTime minTime;
            switch (totalSeconds)
            {
                case > 0:
                    minTime = endDateTime;
                    break;
                case < 0:
                    minTime = startDateTime;
                    break;
                default:
                    return startDateTime;
            }

            var maxValue = totalSeconds;

            if (totalSeconds <= int.MinValue)
                maxValue = int.MinValue + 1;

            var i = NextInt32(Math.Abs(maxValue));
            return minTime.AddSeconds(i);
        }

        #endregion

        #region 姓名生成

        /// <summary>
        /// 生成随机姓名（支持性别筛选）
        /// </summary>
        /// <param name="gender">0-中性名 1-男性名 2-女性名</param>
        public static string GenerateName(int gender = 0)
        {
            var firstName = RandomConfigDto.FirstNames[NextInt32(RandomConfigDto.FirstNames.Length)];

            int startIdx = 0, endIdx = RandomConfigDto.LastNames.Length;
            switch (gender)
            {
                case 1: startIdx = 220; break; // 取男性字
                case 2:
                    startIdx = 120;
                    endIdx = 220;
                    break; // 取女性字
            }

            return firstName + RandomConfigDto.LastNames[NextInt32(startIdx, endIdx)];
        }

        /// <summary>
        /// 批量生成（带性别筛选）
        /// </summary>
        /// <param name="count"></param>
        /// <param name="gender">0-中性名 1-男性名 2-女性名</param>
        /// <returns></returns>
        public static IEnumerable<string> GenerateNameBatch(int count, int gender = 0)
        {
            var startIdx = gender switch { 1 => 220, 2 => 120, _ => 0 };
            var endIdx = gender switch { 2 => 220, _ => RandomConfigDto.LastNames.Length };

            for (var i = 0; i < count; i++)
            {
                yield return RandomConfigDto.FirstNames[NextInt32(RandomConfigDto.FirstNames.Length)] +
                             RandomConfigDto.LastNames[NextInt32(startIdx, endIdx)];
            }
        }

        #endregion

        #region 身份证号生成

        /// <summary>
        /// 生成随机身份证号
        /// </summary>
        /// <param name="gender">性别：1-男 2-女 0-随机</param>
        /// <param name="minAge">最小年龄</param>
        /// <param name="maxAge">最大年龄</param>
        /// <returns></returns>
        public static string GenerateIdCard(int gender = 0, int minAge = 18, int maxAge = 65)
        {
            var areaCode = RandomConfigDto.AreaCodes[NextInt32(RandomConfigDto.AreaCodes.Length)];

            // 出生日期（7-14位）
            var age = NextInt32(minAge, maxAge + 1);
            var birthYear = DateTime.Now.Year - age;
            var birthMonth = NextInt32(1, 13);
            var birthDay = NextInt32(1, DateTime.DaysInMonth(birthYear, birthMonth) + 1);
            var birthDate = $"{birthYear:D4}{birthMonth:D2}{birthDay:D2}";

            // 顺序码（15-17位）
            var sequence = NextInt32(100, 1000);

            // 性别码（第17位）
            int genderCode;
            if (gender == 1) // 男性
                genderCode = sequence % 2 == 0 ? sequence + 1 : sequence;
            else if (gender == 2) // 女性
                genderCode = sequence % 2 == 1 ? sequence + 1 : sequence;
            else // 随机
                genderCode = sequence;

            var idCard17 = areaCode + birthDate + genderCode.ToString("D3");

            // 校验码（第18位）
            var checkCode = CalculateIdCardCheckCode(idCard17);

            return idCard17 + checkCode;
        }

        #endregion

        #region 手机号生成

        /// <summary>
        /// 生成随机手机号
        /// </summary>
        /// <returns></returns>
        public static string GeneratePhoneNumber()
        {
            var prefix = RandomConfigDto._phonePrefixes[NextInt32(RandomConfigDto._phonePrefixes.Length)];
            var suffix = NextInt32(10000000, 100000000).ToString();

            return prefix + suffix;
        }

        #endregion

        #region 地址生成

        /// <summary>
        /// 生成随机地址
        /// </summary>
        /// <returns></returns>
        public static string GenerateAddress()
        {
            var province = RandomConfigDto._phonePrefixes[NextInt32(RandomConfigDto._provinces.Length)];
            var city = RandomConfigDto._cities[NextInt32(RandomConfigDto._cities.Length)];
            var district = RandomConfigDto._districts[NextInt32(RandomConfigDto._districts.Length)];
            var street = RandomConfigDto._streets[NextInt32(RandomConfigDto._streets.Length)];
            var number = NextInt32(1, 999);
            var unit = NextInt32(1, 20);
            var room = NextInt32(1, 30);

            return $"{province}{city}{district}{street}{number}号{unit}单元{room}室";
        }

        #endregion

        #region 银行卡号生成

        /// <summary>
        /// 生成随机银行卡号
        /// </summary>
        /// <returns></returns>
        public static string GenerateBankCardNumber()
        {
            var binCode = RandomConfigDto._binCodes[NextInt32(RandomConfigDto._binCodes.Length)];

            // 生成中间9位数字
            var middle = NextInt32(100000000, 1000000000).ToString();

            // 生成最后3位数字
            var suffix = NextInt32(100, 1000).ToString();

            return binCode + middle + suffix;
        }

        #endregion

        #region 邮箱生成

        /// <summary>
        /// 生成随机邮箱
        /// </summary>
        /// <param name="name">指定用户名，为空则随机生成</param>
        /// <returns></returns>
        public static string GenerateEmail(string? name = null)
        {
            string username;
            if (string.IsNullOrEmpty(name))
            {
                // 随机生成用户名
                var usernameLength = NextInt32(5, 12);
                var usernameChars = "abcdefghijklmnopqrstuvwxyz0123456789";
                var sb = new StringBuilder();
                for (var i = 0; i < usernameLength; i++)
                {
                    sb.Append(usernameChars[NextInt32(usernameChars.Length)]);
                }

                username = sb.ToString();
            }
            else
            {
                username = name.ToLower();
            }

            var domain = RandomConfigDto._domains[NextInt32(RandomConfigDto._domains.Length)];

            return $"{username}@{domain}";
        }

        #endregion

        #region 原有方法

        /// <summary>
        /// 对一个数组进行随机排序
        /// </summary>
        /// <typeparam name="T">数组的类型</typeparam>
        /// <param name="arr">需要随机排序的数组</param>
        public static void GenerateArray<T>(T[] arr)
        {
            Shuffle(arr);
        }

        /// <summary>
        /// 生成验证码（生成次数越多越快，重复次数越多） 可选包含：数字、小写字母、大写字母、字符
        /// </summary>
        /// <param name="length">长度（最小四位）</param>
        /// <param name="hasNumber">是否包含数字</param>
        /// <param name="hasUpper">是否包含大写字母</param>
        /// <param name="hasLowercase">是否包含小写字母</param>
        /// <param name="hasNonAlphanumeric">是否包含特殊字符</param>
        /// <returns></returns>
        public static string GenerateVerifyCode(int length, bool hasNumber = true, bool hasUpper = false,
                                                bool hasLowercase = false,
                                                bool hasNonAlphanumeric = false)
        {
            if (length < 4) length = 4;

            var chars = new List<char>(length);

            const string numStr = "0123456789";
            const string upperStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerStr = "abcdefghijklmnopqrstuvwxyz";
            const string nonAlphaStr = "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

            var generateTypeRange = new List<int>(4);
            if (hasNumber)
            {
                generateTypeRange.Add(0);
                chars.Add(numStr[NextInt32(numStr.Length)]);
            }

            if (hasUpper)
            {
                generateTypeRange.Add(1);
                chars.Add(upperStr[NextInt32(upperStr.Length)]);
            }

            if (hasLowercase)
            {
                generateTypeRange.Add(2);
                chars.Add(lowerStr[NextInt32(lowerStr.Length)]);
            }

            if (hasNonAlphanumeric)
            {
                generateTypeRange.Add(3);
                chars.Add(nonAlphaStr[NextInt32(nonAlphaStr.Length)]);
            }

            if (generateTypeRange.Count == 0)
            {
                generateTypeRange.Add(0);
            }

            var num = length - chars.Count;
            for (var i = 0; i < num; i++)
            {
                switch (generateTypeRange[NextInt32(generateTypeRange.Count)])
                {
                    case 0:
                        chars.Add(numStr[NextInt32(numStr.Length)]);
                        break;
                    case 1:
                        chars.Add(upperStr[NextInt32(upperStr.Length)]);
                        break;
                    case 2:
                        chars.Add(lowerStr[NextInt32(lowerStr.Length)]);
                        break;
                    case 3:
                        chars.Add(nonAlphaStr[NextInt32(nonAlphaStr.Length)]);
                        break;
                }
            }

            Shuffle(chars);
            return new string(chars.ToArray());
        }

        #endregion

        /// <summary>
        /// 计算身份证校验码
        /// </summary>
        private static string CalculateIdCardCheckCode(string idCard17)
        {
            var sum = 0;
            for (var i = 0; i < 17; i++)
            {
                sum += int.Parse(idCard17[i].ToString()) * RandomConfigDto._weights[i];
            }

            return RandomConfigDto._checkCodes[sum % 11];
        }
    }
}