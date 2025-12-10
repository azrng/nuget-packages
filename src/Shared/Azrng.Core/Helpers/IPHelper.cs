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
        /// <returns></returns>
        public static long ToLongFromIp(string ip)
        {
            var items = ip.Split('.');
            return long.Parse(items[0]) << 24 | long.Parse(items[1]) << 16 | long.Parse(items[2]) << 8 | long.Parse(items[3]);
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