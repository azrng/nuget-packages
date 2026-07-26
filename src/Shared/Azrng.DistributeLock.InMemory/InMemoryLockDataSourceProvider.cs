using Azrng.DistributeLock.Core;
using System.Collections.Concurrent;

namespace Azrng.DistributeLock.InMemory
{
    internal class InMemoryLockDataSourceProvider : ILockDataSourceProvider
    {
        private readonly ConcurrentDictionary<string, LockEntry> _locks = new ConcurrentDictionary<string, LockEntry>();

        /// <summary>
        /// 锁条目：记录持有者值与过期时间（record 值相等性用于 TryUpdate / TryRemove 的原子比较）
        /// </summary>
        private sealed record LockEntry(string Value, DateTime ExpireAtUtc);

        public async Task<bool> TakeLockAsync(string lockKey, string lockValue, TimeSpan expireTime, TimeSpan getLockTimeOut)
        {
            if (TryTake(lockKey, lockValue, expireTime))
                return true;

            using var tokenSource = new CancellationTokenSource(getLockTimeOut);
            var cancellationToken = tokenSource.Token;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (TryTake(lockKey, lockValue, expireTime))
                    return true;

                await Task.Delay(10, cancellationToken);
            }
        }

        public Task<bool> ExtendLockAsync(string lockKey, string lockValue, TimeSpan extendTime)
        {
            while (true)
            {
                // 只允许持有者续期，且已过期的锁不再续期（与 Redis / PG 语义保持一致）
                if (!_locks.TryGetValue(lockKey, out var existing) || existing.Value != lockValue ||
                    existing.ExpireAtUtc < DateTime.UtcNow)
                {
                    return Task.FromResult(false);
                }

                var updated = existing with { ExpireAtUtc = DateTime.UtcNow.Add(extendTime) };
                if (_locks.TryUpdate(lockKey, updated, existing))
                    return Task.FromResult(true);
            }
        }

        public Task ReleaseLockAsync(string lockKey, string lockValue)
        {
            while (true)
            {
                // 只删除自己持有的锁，避免释放已被他人接管的锁
                if (!_locks.TryGetValue(lockKey, out var existing) || existing.Value != lockValue)
                    return Task.CompletedTask;

                if (_locks.TryRemove(KeyValuePair.Create(lockKey, existing)))
                    return Task.CompletedTask;
            }
        }

        /// <summary>
        /// 尝试加锁：键不存在直接加；已存在但过期则原子接管
        /// </summary>
        private bool TryTake(string lockKey, string lockValue, TimeSpan expireTime)
        {
            var entry = new LockEntry(lockValue, DateTime.UtcNow.Add(expireTime));
            if (_locks.TryAdd(lockKey, entry))
                return true;

            return _locks.TryGetValue(lockKey, out var existing) && existing.ExpireAtUtc < DateTime.UtcNow &&
                   _locks.TryUpdate(lockKey, entry, existing);
        }
    }
}
