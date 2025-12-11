using Azrng.DistributeLock.Core;
using StackExchange.Redis;

namespace Azrng.DistributeLock.Redis
{
    /// <summary>
    /// redis锁数据源提供者实现
    /// </summary>
    internal class RedisLockDataSourceProvider : ILockDataSourceProvider
    {
        private readonly IDatabase _database;

        public RedisLockDataSourceProvider(string connectionString)
        {
            var configurationOptions = ConfigurationOptions.Parse(connectionString);
            var connection = ConnectionMultiplexer.Connect(configurationOptions);
            _database = connection.GetDatabase();
        }

        public async Task<bool> TakeLockAsync(string lockKey, string lockValue, TimeSpan expireTime,
            TimeSpan getLockTimeOut)
        {
            var flag = await _database.LockTakeAsync(lockKey, lockValue, expireTime);
            if (flag)
                return true;

            using var tokenSource = new CancellationTokenSource(getLockTimeOut);
            var cancellationToken = tokenSource.Token;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                flag = await _database.LockTakeAsync(lockKey, lockValue, expireTime);
                if (flag)
                {
                    break;
                }

                await Task.Delay(10, cancellationToken);
            }

            return flag;
        }

        public async Task ReleaseLockAsync(string lockKey, string lockValue)
        {
            await _database.LockReleaseAsync(lockKey, lockValue);
        }

        public async Task ExtendLockAsync(string lockKey, string lockValue, TimeSpan extendTime)
        {
            await _database.LockExtendAsync(lockKey, lockValue, extendTime);
        }
    }
}