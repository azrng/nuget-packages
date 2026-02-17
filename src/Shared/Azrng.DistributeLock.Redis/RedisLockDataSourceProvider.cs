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
        private readonly ConnectionMultiplexer _connection;

        public RedisLockDataSourceProvider(IDatabase database, ConnectionMultiplexer connection)
        {
            _database = database;
            _connection = connection;
        }

        /// <summary>
        /// 检查Redis连接是否可用
        /// </summary>
        public bool IsConnected => _connection.IsConnected;

        public async Task<bool> TakeLockAsync(string lockKey, string lockValue, TimeSpan expireTime,
            TimeSpan getLockTimeOut)
        {
            // 检查连接状态
            if (!IsConnected)
            {
                throw new InvalidOperationException("Redis连接不可用，无法获取锁");
            }

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

        public async Task<bool> ExtendLockAsync(string lockKey, string lockValue, TimeSpan extendTime)
        {
            return await _database.LockExtendAsync(lockKey, lockValue, extendTime);
        }
    }
}