using Azrng.Core.Extension;
using Azrng.Core.Helpers;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace Common.Windows.Core;

/// <summary>
/// Hardware_Mac的摘要说明
/// </summary>
public class HardwareInfo
{
    /// <summary>
    /// 取机器名
    /// </summary>
    /// <returns></returns>
    public static string GetHostName()
    {
        return System.Net.Dns.GetHostName();
    }

    /// <summary>
    /// 取CPU编号
    /// </summary>
    /// <returns></returns>
    public static string GetCpuId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("Select ProcessorId From Win32_processor");
            using var items = searcher.Get();
            return items.Cast<ManagementObject>()
                        .Select(x => x["ProcessorId"]?.ToString())
                        .FirstOrDefault() ??
                   string.Empty;
        }
        catch (Exception ex)
        {
            LocalLogHelper.LogError($"message：{ex.GetExceptionAndStack()}");
            return string.Empty;
        }
    }

    /// <summary>
    /// 获取主硬盘编号
    /// </summary>
    /// <returns></returns>
    public static string GetMainDiskId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("Select SerialNumber From Win32_DiskDrive WHERE Index = 0");
            using var items = searcher.Get();
            return items.Cast<ManagementObject>()
                        .Select(x => x["SerialNumber"]?.ToString())
                        .FirstOrDefault() ??
                   string.Empty;
        }
        catch (Exception ex)
        {
            LocalLogHelper.LogError($"message：{ex.GetExceptionAndStack()}");
            return string.Empty;
        }
    }

    /// <summary>
    /// 获取mac地址
    /// </summary>
    /// <returns></returns>
    public static string GetMacAddress()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("Select MACAddress From Win32_NetworkAdapter WHERE MACAddress IS NOT NULL");
            using var items = searcher.Get();
            return items.Cast<ManagementObject>()
                        .Select(x => x["MACAddress"]?.ToString())
                        .FirstOrDefault() ??
                   string.Empty;
        }
        catch (Exception ex)
        {
            LocalLogHelper.LogError($"message：{ex.GetExceptionAndStack()}");
            return string.Empty;
        }
    }

    /// <summary>
    /// 获取BIOS编号
    /// </summary>
    /// <returns></returns>
    public static string GetBiosSerial()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("Select SerialNumber From Win32_BIOS");
            using var items = searcher.Get();
            return items.Cast<ManagementObject>()
                        .Select(x => x["SerialNumber"]?.ToString())
                        .FirstOrDefault() ??
                   string.Empty;
        }
        catch (Exception ex)
        {
            LocalLogHelper.LogError($"message：{ex.GetExceptionAndStack()}");
            return string.Empty;
        }
    }

    /// <summary>
    /// 生成指纹
    /// </summary>
    /// <param name="values">需要生成指纹的信息</param>
    /// <returns></returns>
    public static string GenerateFingerprint(params string[] values)
    {
        var components = new StringBuilder();

        if (values.Length > 0)
        {
            foreach (var value in values)
            {
                components.Append(value);
            }
        }
        else
        {
            components.Append(GetCpuId());
            components.Append(GetBiosSerial());
            components.Append(GetMainDiskId());
            components.Append(GetMacAddress());
        }

        // 添加应用特定盐值
        components.Append(WindowsConfigConst.Salt);

        // 计算SHA256哈希
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(components.ToString()));

        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// 检测是否安装某个软件，并返回软件的卸载安装路径
    /// </summary>
    /// <param name="softName">软件名</param>
    /// <param name="strExe">安装目录下启动服务地址</param>
    /// <param name="installPath">卸载安装路径</param>
    /// <returns></returns>
    public static bool CheckInstall(string softName, string strExe, out string installPath)
    {
        //即时刷新注册表
        Win32ApiConfigHelper.SHChangeNotify(0x8000000, 0, IntPtr.Zero, IntPtr.Zero);

        installPath = string.Empty;

        var isFind = false;
        var uninstallNode =
            Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall", false);
        if (uninstallNode != null)
        {
            //LocalMachine_64
            using (uninstallNode)
            {
                foreach (var subKeyName in uninstallNode.GetSubKeyNames())
                {
                    var subKey = uninstallNode.OpenSubKey(subKeyName);

                    var displayName = (subKey?.GetValue("DisplayName") ?? string.Empty).ToString();
                    if (string.IsNullOrWhiteSpace(displayName))
                        continue;

                    // 安装路径
                    var path = (subKey.GetValue("InstallLocation") ?? string.Empty).ToString();
#if DEBUG
                    Console.WriteLine($"文件{displayName} 路径{path} ");
#endif

                    if (displayName.Contains(softName, StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrWhiteSpace(path))
                    {
                        installPath = Path.Combine(Path.GetDirectoryName(path), strExe);
                        if (File.Exists(installPath))
                        {
                            isFind = true;
                            break;
                        }
                    }
                }
            }
        }

        if (!isFind)
        {
            //LocalMachine_32
            uninstallNode =
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", false);
            using (uninstallNode)
            {
                foreach (var subKeyName in uninstallNode.GetSubKeyNames())
                {
                    var subKey = uninstallNode.OpenSubKey(subKeyName);
                    var displayName = (subKey.GetValue("DisplayName") ?? string.Empty).ToString();
                    var path = (subKey.GetValue("InstallLocation") ?? string.Empty).ToString();
#if DEBUG
                    Console.WriteLine($"文件{displayName} 路径{path} ");
#endif
                    if (displayName.Contains(softName, StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrWhiteSpace(path))
                    {
                        installPath = Path.Combine(Path.GetDirectoryName(path), strExe);
                        if (File.Exists(installPath))
                        {
                            isFind = true;
                            break;
                        }
                    }
                }
            }
        }

        return isFind;
    }
}