using System;
using System.Security.Cryptography;

namespace Azrng.Notification.QYWeiXinRobot
{
    /// <summary>
    /// 帮助类
    /// </summary>
    public class Helper
    {
        /// <summary>
        /// 文件流转base64
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string BytesToBase64(byte[] bytes)
        {
            if (bytes.Length == 0)
                return string.Empty;
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 获取文件mdm值
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string GetFileMd5Hash(byte[] bytes)
        {
            if (bytes.Length == 0)
                return string.Empty;

            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}