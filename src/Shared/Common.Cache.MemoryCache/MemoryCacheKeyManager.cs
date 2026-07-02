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
        private readonly ConcurrentDictionary<string, KeyLock> _keyLocks = new(StringComparer.Ordinal);

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

            var keyLock = RentKeyLock(key);
            await keyLock.Semaphore.WaitAsync();
            try
            {
                return await action();
            }
            finally
            {
                keyLock.Semaphore.Release();
                ReleaseKeyLock(key, keyLock);
            }
        }

        internal int SynchronizedKeyCount => _keyLocks.Count;

        private KeyLock RentKeyLock(string key)
        {
            while (true)
            {
                var keyLock = _keyLocks.GetOrAdd(key, _ => new KeyLock());
                lock (keyLock.SyncRoot)
                {
                    if (!keyLock.Removed)
                    {
                        keyLock.RefCount++;
                        return keyLock;
                    }
                }
            }
        }

        private void ReleaseKeyLock(string key, KeyLock keyLock)
        {
            lock (keyLock.SyncRoot)
            {
                keyLock.RefCount--;
                if (keyLock.RefCount != 0)
                {
                    return;
                }

                keyLock.Removed = true;
                _keyLocks.TryRemove(new KeyValuePair<string, KeyLock>(key, keyLock));
            }

            keyLock.Semaphore.Dispose();
        }

        private sealed class KeyLock
        {
            public SemaphoreSlim Semaphore { get; } = new(1, 1);

            public object SyncRoot { get; } = new();

            public int RefCount { get; set; }

            public bool Removed { get; set; }
        }
    }
}
