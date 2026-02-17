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

        /// <summary>
        /// 获取锁
        /// </summary>
        /// <param name="lockKey">锁键</param>
        /// <param name="lockValue">锁值</param>
        /// <param name="expireTime">过期时间</param>
        /// <param name="getLockTimeOut">获取锁超时时间</param>
        /// <returns>成功返回 true，超时抛出 TaskCanceledException</returns>
        /// <exception cref="TaskCanceledException">获取锁超时</exception>
        public async Task<bool> TakeLockAsync(string lockKey, string lockValue, TimeSpan expireTime,
            TimeSpan getLockTimeOut)
        {
            // 检查连接状态
            if (!IsConnected)
            {
                throw new InvalidOperationException("Redis连接不可用，无法获取锁");
            }

            // 首次尝试获取锁
            var flag = await _database.LockTakeAsync(lockKey, lockValue, expireTime);
            if (flag)
                return true;

            // 使用 CancellationToken 来处理超时
            using var tokenSource = new CancellationTokenSource(getLockTimeOut);
            var cancellationToken = tokenSource.Token;

            while (true)
            {
                // 尝试获取锁
                flag = await _database.LockTakeAsync(lockKey, lockValue, expireTime);
                if (flag)
                {
                    return true;
                }

                // 等待一小段时间后重试，超时时抛出 TaskCanceledException
                await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);
            }
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