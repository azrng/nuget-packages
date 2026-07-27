using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace Common.Cache.Redis
{
    /// <summary>
    /// 基于 StackExchange.Redis 的连接工厂：建立 <see cref="ConnectionMultiplexer"/> 并包装为 <see cref="IRedisConnection"/>。
    /// </summary>
    internal sealed class StackExchangeRedisConnectionFactory : IRedisConnectionFactory
    {
        private readonly ILogger _logger;

        public StackExchangeRedisConnectionFactory()
            : this(NullLogger.Instance)
        {
        }

        public StackExchangeRedisConnectionFactory(ILogger logger)
        {
            _logger = logger ?? NullLogger.Instance;
        }

        public async Task<IRedisConnection> ConnectAsync(ConfigurationOptions configurationOptions)
        {
            var connection = await ConnectionMultiplexer.ConnectAsync(configurationOptions);
            return new StackExchangeRedisConnection(connection, _logger);
        }
    }
}
