using Polly;

namespace Common.HttpClients
{
    /// <summary>
    /// 经 ResilienceContext.Properties 传递的调用点级弹性标记；
    /// ResilienceHandler 会复用请求上预置的 context，重试谓词通过 args.Context 读取这些标记
    /// </summary>
    internal static class HttpClientResilienceKeys
    {
        /// <summary>
        /// 禁用本次请求的重试（HttpSendOptions.EnableRetry=false 时设置）
        /// </summary>
        internal static readonly ResiliencePropertyKey<bool> SuppressRetry =
            new("Common.HttpClients.SuppressRetry");
    }
}
