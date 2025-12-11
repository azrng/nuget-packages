using Azrng.DistributeLock.Core;
using System.Collections.Concurrent;

namespace Azrng.DistributeLock.InMemory
{
    internal class InMemoryLockDataSourceProvider : ILockDataSourceProvider
    {
        private readonly ConcurrentDictionary<string, string> _lockCounts = new ConcurrentDictionary<string, string>();

        public async Task<bool> TakeLockAsync(string lockKey, string lockValue, TimeSpan expireTime, TimeSpan getLockTimeOut)
        {
            var flag = _lockCounts.TryAdd(lockKey, lockValue);
            if (flag)
                return true;

            using var tokenSource = new CancellationTokenSource(getLockTimeOut);
            var cancellationToken = tokenSource.Token;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                flag = _lockCounts.TryAdd(lockKey, lockValue);
                if (flag)
                {
                    break;
                }

                await Task.Delay(10, cancellationToken);
            }

            return flag;
        }

        public Task ExtendLockAsync(string lockKey, string lockValue, TimeSpan extendTime)
        {
            return Task.CompletedTask;
        }

        public Task ReleaseLockAsync(string lockKey, string lockValue)
        {
            _lockCounts.TryRemove(lockKey, out var _);
            return Task.CompletedTask;
        }
    }
}