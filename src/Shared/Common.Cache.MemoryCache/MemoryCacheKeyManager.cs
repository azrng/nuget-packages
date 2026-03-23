using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Azrng.Cache.MemoryCache
{
    public sealed class MemoryCacheKeyManager
    {
        private readonly ConcurrentDictionary<string, byte> _trackedKeys = new(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _keyLocks = new(StringComparer.Ordinal);

        public void TrackKey(string key)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                _trackedKeys[key] = 0;
            }
        }

        public void UntrackKey(string key)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                _trackedKeys.TryRemove(key, out _);
            }
        }

        public List<string> GetAllKeys()
        {
            return _trackedKeys.Keys.OrderBy(key => key, StringComparer.Ordinal).ToList();
        }

        public async Task<T> ExecuteSynchronizedAsync<T>(string key, Func<Task<T>> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            var keyLock = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            await keyLock.WaitAsync();
            try
            {
                return await action();
            }
            finally
            {
                keyLock.Release();
            }
        }
    }
}
