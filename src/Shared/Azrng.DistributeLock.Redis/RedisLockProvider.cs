using Azrng.DistributeLock.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Azrng.DistributeLock.Redis
{
    /// <summary>
    /// redis锁提供者
    /// </summary>
    public class RedisLockProvider : ILockProvider
    {
        private readonly ILogger<RedisLockProvider> _logger;
        private readonly RedisLockOptions _options;
        private readonly ConnectionMultiplexer _connection;
        private readonly IDatabase _database;

        public RedisLockProvider(IOptions<RedisLockOptions> options, ILogger<RedisLockProvider> logger,
            ConnectionMultiplexer connection)
        {
            _options = options.Value;
            _logger = logger;
            _connection = connection;

            try
            {
                _database = _connection.GetDatabase();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Redis分布式锁初始化失败。options:{_options.ConnectionString}");
                throw;
            }
        }

        public async Task<IAsyncDisposable?> LockAsync(string lockKey, TimeSpan? expire = null,
            TimeSpan? getLockTimeOut = null, bool autoExtend = true)
        {
            var lockValue = Guid.NewGuid().ToString();
            expire ??= _options.DefaultExpireTime;
            getLockTimeOut ??= TimeSpan.FromSeconds(5);

            var dataSource = new RedisLockDataSourceProvider(_database, _connection);
            var lockData = new LockInstance(dataSource, lockKey, lockValue, _logger,
                autoExtend, expire.Value);
            var flag = await lockData.LockAsync(expire.Value, getLockTimeOut.Value);

            return flag ? lockData : null;
        }
    }
}