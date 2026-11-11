using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// IP帮助类
    /// </summary>
    public static class IpHelper
    {
        /// <summary>
        /// ip地址转整型数据
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <exception cref="ArgumentException">ip 不是合法的 IPv4 地址</exception>
        public static long ToLongFromIp(string ip)
        {
            //原实现对 null/段数不足/非数字输入分别抛 NRE/IndexOutOfRange/FormatException，且不校验 0-255 范围
            //IPAddress.TryParse 对 "1.2.3" 这类输入会宽松补零为 1.2.3.0，需额外校验四段
            if (ip?.Split('.').Length != 4
                || !IPAddress.TryParse(ip, out var address)
                || address.AddressFamily != AddressFamily.InterNetwork)
                throw new ArgumentException($"非法的 IPv4 地址：{ip}", nameof(ip));

            var bytes = address.GetAddressBytes();
            return (long)bytes[0] << 24 | (long)bytes[1] << 16 | (long)bytes[2] << 8 | bytes[3];
        }

        /// <summary>
        /// 整型数据转ip地址
        /// </summary>
        /// <param name="ipLong"></param>
        /// <returns></returns>
        public static string ToIpFromLong(long ipLong)
        {
            var sb = new StringBuilder();
            sb.Append(ipLong >> 24 & 0xFF).Append('.');
            sb.Append(ipLong >> 16 & 0xFF).Append('.');
            sb.Append(ipLong >> 8 & 0xFF).Append('.');
            sb.Append(ipLong & 0xFF);
            return sb.ToString();
        }
    }
}