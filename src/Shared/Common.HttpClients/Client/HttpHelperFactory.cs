using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Net.Http;

namespace Common.HttpClients
{
    /// <summary>
    /// <see cref="IHttpHelperFactory"/> 的默认实现
    /// </summary>
    internal sealed class HttpHelperFactory : IHttpHelperFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<string, IHttpHelper> _clients = new();

        public HttpHelperFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IHttpHelper CreateClient(string name)
        {
            // HttpClientHelper 对客户端无状态（每次请求从 IHttpClientFactory 现取），缓存仅复用适配器本身
            return _clients.GetOrAdd(name, static (n, sp) =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var logger = sp.GetRequiredService<ILogger<HttpClientHelper>>();
                var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();

                return new HttpClientHelper(n, httpClientFactory, logger, optionsMonitor);
            }, _serviceProvider);
        }
    }
}
