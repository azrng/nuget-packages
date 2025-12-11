using Azrng.DistributeLock.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azrng.DistributeLock.Redis
{
    /// <summary>
    /// redis锁提供者
    /// </summary>
    public class RedisLockProvider : ILockProvider
    {
        private readonly ILogger<RedisLockProvider> _logger;
        private readonly RedisLockOptions _options;
        private readonly RedisLockDataSourceProvider _redisLockDataSourceProvider;

        public RedisLockProvider(IOptions<RedisLockOptions> options, ILogger<RedisLockProvider> logger)
        {
            _options = options.Value;
            _logger = logger;

            try
            {
                _redisLockDataSourceProvider = new RedisLockDataSourceProvider(_options.ConnectionString);
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

            var lockData = new LockInstance(_redisLockDataSourceProvider, lockKey, lockValue, _logger,
                autoExtend);
            var flag = await lockData.LockAsync(expire.Value, getLockTimeOut.Value);

            return flag ? lockData : null;
        }
    }
}