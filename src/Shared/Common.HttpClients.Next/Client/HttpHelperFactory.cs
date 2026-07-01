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
            return _clients.GetOrAdd(name, static (n, sp) =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var client = httpClientFactory.CreateClient(n);
                var logger = sp.GetRequiredService<ILogger<HttpClientHelper>>();
                var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();
                var options = optionsMonitor.Get(n);

                return new HttpClientHelper(client, Options.Create(options), logger);
            }, _serviceProvider);
        }
    }
}