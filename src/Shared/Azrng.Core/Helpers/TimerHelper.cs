using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// 时间帮助类
    /// </summary>
    public class TimerHelper
    {
        /// <summary>
        /// 测量
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TimeSpan Measure(Action action)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        /// <summary>
        /// 测量和打印
        /// </summary>
        /// <param name="action"></param>
        /// <param name="description"></param>
        public static void MeasureAndPrint(Action action, string description)
        {
            var timeSpan = Measure(action);
            Console.WriteLine($"{description}:{timeSpan.TotalMilliseconds}ms");
        }

        /// <summary>
        /// 测量并保存
        /// </summary>
        /// <param name="action"></param>
        /// <param name="fileName"></param>
        public static void MeasureAndSave(Action action, string fileName)
        {
            var timeSpan = Measure(action);
            File.WriteAllText(fileName, timeSpan.TotalMilliseconds.ToString());
        }

        /// <summary>
        /// 测量并保存
        /// </summary>
        /// <param name="action"></param>
        /// <param name="fileName"></param>
        public static void MeasureAndLog(Action action, string fileName)
        {
            var timeSpan = Measure(action);
            File.AppendAllText(fileName, $"{DateTime.Now}: {timeSpan.TotalMilliseconds} ms {Environment.NewLine}");
        }

        /// <summary>
        /// 获取NTP网络远程时间
        /// </summary>
        /// <returns></returns>
        public static DateTimeOffset GetNetworkTime()
        {
            var ntpServer = "ntp.tencent.com";

            var ntpData = new byte[48];

            ntpData[0] = 0x1B;

            var addresses = Dns.GetHostEntry(ntpServer).AddressList;

            //DNS 结果首个地址可能是 IPv6，优先取 IPv4，socket 地址族跟随选中地址
            var address = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
                          ?? addresses[0];
            var ipEndPoint = new IPEndPoint(address, 123);

            //using 确保异常路径也释放 socket 句柄
            using var socket = new Socket(address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            socket.ReceiveTimeout = 3000;

            socket.Connect(ipEndPoint);
            socket.Send(ntpData);
            socket.Receive(ntpData);

            const byte serverReplyTime = 40;

            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

            intPart = (uint)(((intPart & 0x000000ff) << 24) +
                             ((intPart & 0x0000ff00) << 8) +
                             ((intPart & 0x00ff0000) >> 8) +
                             ((intPart & 0xff000000) >> 24));
            fractPart = (uint)(((fractPart & 0x000000ff) << 24) +
                               ((fractPart & 0x0000ff00) << 8) +
                               ((fractPart & 0x00ff0000) >> 8) +
                               ((fractPart & 0xff000000) >> 24));

            var milliseconds = intPart * 1000 + fractPart * 1000 / 0x100000000L;

            var networkDateTime =
                new DateTime(1900, 1, 1, 0, 0,
                    0, DateTimeKind.Utc).AddMilliseconds((long)milliseconds);

            return networkDateTime;
        }
    }
}