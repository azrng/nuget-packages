using System.Net.NetworkInformation;

namespace Common.Windows.Core;

/// <summary>
/// 网络检测
/// </summary>
public static class NetWorkHelper
{
    /// <summary>
    /// 通过ping检测网络是否连接
    /// </summary>
    /// <returns></returns>
    /// <remarks>优点是简单易用，缺点是可能会受到防火墙的影响，导致测试结果不准确</remarks>
    public static bool PingCheckNet()
    {
        try
        {
            var ping = new Ping();
            var pr = ping.Send("www.baidu.com");
            return pr.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 通过InternetGetConnectedState检测网络是否可用  （暂时测试失败）
    /// </summary>
    /// <returns></returns>
    /// <remarks>优点是可以提供更详细的网络连接信息，缺点是需要使用DllImport来调用API函数，使用稍微麻烦。</remarks>
    public static bool IsNetworkAvailable()
    {
        return Win32ApiConfigHelper.InternetGetConnectedState(out _, 0);
    }
}