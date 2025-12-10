using Azrng.Core.Extension;
using System;
using System.Security.Cryptography;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// 公共帮助类
    /// </summary>
    public static class CommonHelper
    {
        /// <summary>
        /// 随机字符串，长度不定(示例：e/6LMJB+zyHK6iCcgOAZgu7dkE9fvBkAbAIy3pIE3RY=)
        /// </summary>
        /// <param name="length">二进制长度</param>
        /// <returns></returns>
        public static string GenerateRandomNumber(int length = 6)
        {
            var randomNumber = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return randomNumber.ToBase64();
        }

        /// <summary>
        /// 创建有序GUID
        /// </summary>
        /// <returns></returns>
        public static Guid SeqGuid()
        {
            var guidArray = Guid.NewGuid().ToByteArray();

            var baseDate = new DateTime(1900, 1, 1);
            var now = DateTime.Now;
            var days = new TimeSpan(now.Ticks - baseDate.Ticks);
            var msecs = now.TimeOfDay;

            var daysArray = BitConverter.GetBytes(days.Days);
            var msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

            Array.Reverse(daysArray);
            Array.Reverse(msecsArray);

            Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
            Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

            return new Guid(guidArray);
        }
    }
}