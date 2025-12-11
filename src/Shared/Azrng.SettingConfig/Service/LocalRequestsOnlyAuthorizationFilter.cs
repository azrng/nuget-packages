namespace Azrng.SettingConfig.Service
{
    /// <summary>
    /// 本地请求授权过滤器
    /// </summary>
    internal class LocalRequestsOnlyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // if unknown, assume not local
            if (string.IsNullOrEmpty(context.Request.RemoteIpAddress))
                return false;

            // check if localhost
            if (context.Request.RemoteIpAddress == "127.0.0.1" || context.Request.RemoteIpAddress == "::1")
                return true;

            // compare with local address
            return context.Request.RemoteIpAddress == context.Request.LocalIpAddress;
        }
    }
}