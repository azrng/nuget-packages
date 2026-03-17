using System.Net;

namespace Azrng.SettingConfig.Service
{
    /// <summary>
    /// 本地请求授权过滤器
    /// </summary>
    internal class LocalRequestsOnlyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // 如果无法获取远程 IP 地址，拒绝访问
            if (string.IsNullOrEmpty(context.Request.RemoteIpAddress))
                return false;

            // 使用 IPAddress.TryParse 进行更安全的解析
            if (!IPAddress.TryParse(context.Request.RemoteIpAddress, out var remoteIp))
                return false;

            // 检查是否为回环地址（localhost/127.0.0.1/::1）
            if (IPAddress.IsLoopback(remoteIp))
                return true;

            // 检查本地 IP 地址是否匹配
            if (!string.IsNullOrEmpty(context.Request.LocalIpAddress) &&
                IPAddress.TryParse(context.Request.LocalIpAddress, out var localIp))
            {
                return remoteIp.Equals(localIp);
            }

            return false;
        }
    }
}