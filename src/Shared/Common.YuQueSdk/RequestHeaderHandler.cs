using Common.YuQueSdk.Dto;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Common.YuQueSdk
{
    /// <summary>
    /// 请求头拦截处理
    /// </summary>
    public class RequestHeaderHandler : DelegatingHandler
    {
        private readonly YuQueConfig _config;

        public RequestHeaderHandler(IOptions<YuQueConfig> options)
        {
            _config = options.Value;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.Add("User-Agent", _config.UserAgent);
            request.Headers.Add("X-Auth-Token", _config.AuthToken);

            return base.SendAsync(request, cancellationToken);
        }
    }
}