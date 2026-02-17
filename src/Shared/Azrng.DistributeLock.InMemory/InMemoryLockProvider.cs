using Azrng.DistributeLock.Core;
using Microsoft.Extensions.Logging;

namespace Azrng.DistributeLock.InMemory
{
    public class InMemoryLockProvider : ILockProvider
    {
        private readonly ILogger<InMemoryLockProvider> _logger;
        private readonly InMemoryLockDataSourceProvider _inMemoryLockDataSourceProvider;

        public InMemoryLockProvider(ILogger<InMemoryLockProvider> logger)
        {
            _logger = logger;
            _inMemoryLockDataSourceProvider = new InMemoryLockDataSourceProvider();
        }

        public async Task<IAsyncDisposable?> LockAsync(string lockKey, TimeSpan? expire = null, TimeSpan? getLockTimeOut = null,
                                                       bool autoExtend = true)
        {
            var lockValue = Guid.NewGuid().ToString();
            expire ??= TimeSpan.FromSeconds(30);
            getLockTimeOut ??= TimeSpan.FromSeconds(5);

            var lockData = new LockInstance(_inMemoryLockDataSourceProvider, lockKey, lockValue, _logger,
                autoExtend, getLockTimeOut.Value);
            var flag = await lockData.LockAsync(expire.Value, expire.Value);

            return flag ? lockData : null;
        }
    }
}