using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace Azrng.Core.Helpers;

/// <summary>
/// 系统帮助类
/// </summary>
public class SystemHelper
{
    /// <summary>
    /// 获取本机的计算机名
    /// </summary>
    public static string GetLocalHostName()
    {
        return Dns.GetHostName();
    }

    /// <summary>
    /// 获取本机全部IP
    /// </summary>
    /// <returns></returns>
    public static List<string> GetAllIpAddress()
    {
        var allIp = Dns.GetHostEntry(Dns.GetHostName())
                       .AddressList.Select(t => t.ToString())
                       .ToList();

        return allIp;
    }

    /// <summary>
    /// 获取本机 IPV4 地址
    /// </summary>
    /// <returns></returns>
    public static string GetIpv4Address()
    {
        var ipv4 = Dns.GetHostEntry(Dns.GetHostName())
                      .AddressList
                      .FirstOrDefault(address => address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                      ?.ToString();

        return ipv4;
    }

    /// <summary>
    /// 获取本机 IPV6 地址
    /// </summary>
    /// <returns></returns>
    public static string GetIpv6Address()
    {
        var ipv6 = Dns.GetHostEntry(Dns.GetHostName())
                      .AddressList.FirstOrDefault(address =>
                          address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                      ?.ToString();

        return ipv6;
    }

    /// <summary>
    /// Linux 运行 shell 脚本
    /// </summary>
    /// <param name="shell"></param>
    /// <returns></returns>
    public static string LinuxShell(string shell)
    {
        //创建一个ProcessStartInfo对象 使用系统shell 指定命令和参数 设置标准输出
        ProcessStartInfo psi = new("/bin/bash", "-c \"" + shell + "\"") { RedirectStandardOutput = true };

        using var proc = Process.Start(psi);
        if (proc == null)
        {
            return string.Empty;
        }

        var output = proc.StandardOutput.ReadToEnd();

        if (!proc.HasExited)
        {
            proc.Kill();
        }

        return output;
    }

    /// <summary>
    /// Windows 运行 shell 脚本
    /// </summary>
    /// <param name="shell"></param>
    /// <returns></returns>
    public static string WindowsShell(string shell)
    {
        //创建一个ProcessStartInfo对象 使用系统shell 指定命令和参数 设置标准输出
        ProcessStartInfo psi = new("powershell", "-c \"" + shell + "\"") { RedirectStandardOutput = true };

        using var proc = Process.Start(psi);
        if (proc == null)
        {
            return string.Empty;
        }

        var output = proc.StandardOutput.ReadToEnd();

        if (!proc.HasExited)
        {
            proc.Kill();
        }

        return output;
    }
}