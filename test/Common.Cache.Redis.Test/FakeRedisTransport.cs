using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common.Cache.Redis.Test
{
    internal sealed class FakeRedisConnectionFactory : IRedisConnectionFactory
    {
        private readonly Queue<Func<IRedisConnection>> _connectionFactories;

        public FakeRedisConnectionFactory(params Func<IRedisConnection>[] connectionFactories)
        {
            _connectionFactories = new Queue<Func<IRedisConnection>>(connectionFactories);
        }

        public int ConnectCallCount { get; private set; }

        public Task<IRedisConnection> ConnectAsync(ConfigurationOptions configurationOptions)
        {
            ConnectCallCount++;

            if (_connectionFactories.Count == 0)
            {
                throw new InvalidOperationException("No fake Redis connection factory was configured.");
            }

            return Task.FromResult(_connectionFactories.Dequeue().Invoke());
        }
    }

    internal sealed class FakeRedisConnection : IRedisConnection
    {
        public FakeRedisConnection(FakeRedisDatabase database, FakeRedisSubscriber subscriber)
        {
            Database = database;
            Subscriber = subscriber;
        }

        public FakeRedisDatabase Database { get; }

        public FakeRedisSubscriber Subscriber { get; }

        public int DisposeCallCount { get; private set; }

        public IRedisDatabase GetDatabase()
        {
            return Database;
        }

        public IRedisSubscriber GetSubscriber()
        {
            return Subscriber;
        }

        public void Dispose()
        {
            DisposeCallCount++;
        }
    }

    internal sealed class FakeRedisDatabase : IRedisDatabase
    {
        private readonly ConcurrentDictionary<string, RedisValue> _values = new(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, DateTimeOffset?> _expirations = new(StringComparer.Ordinal);

        public Exception? StringGetException { get; set; }

        public Exception? ScanException { get; set; }

        public List<(ulong Cursor, string Pattern, int Count)> ScanCalls { get; } = new();

        public Task<RedisValue> StringGetAsync(RedisKey key)
        {
            if (StringGetException != null)
            {
                throw StringGetException;
            }

            var actualKey = key.ToString();
            if (_expirations.TryGetValue(actualKey, out var expiresAt) &&
                expiresAt.HasValue &&
                expiresAt.Value <= DateTimeOffset.UtcNow)
            {
                _values.TryRemove(actualKey, out _);
                _expirations.TryRemove(actualKey, out _);
                return Task.FromResult(RedisValue.Null);
            }

            return Task.FromResult(_values.TryGetValue(actualKey, out var value) ? value : RedisValue.Null);
        }

        public Task<bool> StringSetAsync(RedisKey key, RedisValue value, TimeSpan? expiry = null)
        {
            var actualKey = key.ToString();
            _values[actualKey] = value;
            _expirations[actualKey] = expiry.HasValue ? DateTimeOffset.UtcNow.Add(expiry.Value) : null;
            return Task.FromResult(true);
        }

        public Task<bool> KeyDeleteAsync(RedisKey key)
        {
            var actualKey = key.ToString();
            _expirations.TryRemove(actualKey, out _);
            return Task.FromResult(_values.TryRemove(actualKey, out _));
        }

        public async Task<long> KeyDeleteAsync(RedisKey[] keys)
        {
            long deletedCount = 0;
            foreach (var key in keys)
            {
                if (await KeyDeleteAsync(key))
                {
                    deletedCount++;
                }
            }

            return deletedCount;
        }

        public Task<bool> KeyExpireAsync(RedisKey key, TimeSpan? expiry)
        {
            var actualKey = key.ToString();
            if (!_values.ContainsKey(actualKey))
            {
                return Task.FromResult(false);
            }

            _expirations[actualKey] = expiry.HasValue ? DateTimeOffset.UtcNow.Add(expiry.Value) : null;
            return Task.FromResult(true);
        }

        public Task<bool> KeyExistsAsync(RedisKey key)
        {
            return Task.FromResult(_values.ContainsKey(key.ToString()));
        }

        private readonly object _incrementLock = new();

        public Task<long> StringIncrementAsync(RedisKey key, long value)
        {
            var actualKey = key.ToString();
            lock (_incrementLock)
            {
                long current = 0;
                if (_values.TryGetValue(actualKey, out var existing) && existing.HasValue)
                {
                    if (!long.TryParse(existing.ToString(), out current))
                    {
                        throw new RedisServerException("ERR value is not an integer or out of range");
                    }
                }

                var next = current + value;
                _values[actualKey] = next;
                if (!_expirations.ContainsKey(actualKey))
                {
                    _expirations[actualKey] = null;
                }

                return Task.FromResult(next);
            }
        }

        public Task<RedisScanResult> ScanAsync(ulong cursor, string pattern, int count)
        {
            if (ScanException != null)
            {
                throw ScanException;
            }

            ScanCalls.Add((cursor, pattern, count));

            var matchedKeys = _values.Keys
                .Where(key => PatternMatches(key, pattern))
                .Select(key => (RedisKey)key)
                .ToArray();

            return Task.FromResult(new RedisScanResult(0, matchedKeys));
        }

        private static bool PatternMatches(string value, string pattern)
        {
            var regexPattern = "^" +
                               Regex.Escape(pattern)
                                   .Replace("\\*", ".*")
                                   .Replace("\\?", ".") +
                               "$";
            return Regex.IsMatch(value, regexPattern, RegexOptions.CultureInvariant);
        }
    }

    internal sealed class FakeRedisSubscriber : IRedisSubscriber
    {
        private readonly object _lock = new();

        private readonly Dictionary<string, List<Action<RedisChannel, RedisValue>>> _literalHandlers =
            new(StringComparer.Ordinal);

        private readonly Dictionary<string, List<Action<RedisChannel, RedisValue>>> _patternHandlers =
            new(StringComparer.Ordinal);

        public ConcurrentBag<string> UnsubscribedChannels { get; } = new();

        public Task<long> PublishAsync(RedisChannel channel, RedisValue message)
        {
            var channelName = channel.ToString();
            var handlers = new List<Action<RedisChannel, RedisValue>>();

            lock (_lock)
            {
                if (_literalHandlers.TryGetValue(channelName, out var literalHandlers))
                {
                    handlers.AddRange(literalHandlers);
                }

                foreach (var patternHandler in _patternHandlers)
                {
                    if (PatternMatches(channelName, patternHandler.Key))
                    {
                        handlers.AddRange(patternHandler.Value);
                    }
                }
            }

            foreach (var handler in handlers)
            {
                handler(channel, message);
            }

            return Task.FromResult((long)handlers.Count);
        }

        public void Subscribe(RedisChannel channel, Action<RedisChannel, RedisValue> handler)
        {
            var key = channel.ToString();
            lock (_lock)
            {
                var store = IsPattern(key) ? _patternHandlers : _literalHandlers;
                if (!store.TryGetValue(key, out var handlers))
                {
                    handlers = new List<Action<RedisChannel, RedisValue>>();
                    store[key] = handlers;
                }

                handlers.Add(handler);
            }
        }

        public void Unsubscribe(RedisChannel channel, Action<RedisChannel, RedisValue>? handler)
        {
            var key = channel.ToString();
            UnsubscribedChannels.Add(key);

            lock (_lock)
            {
                var store = IsPattern(key) ? _patternHandlers : _literalHandlers;
                if (!store.TryGetValue(key, out var handlers))
                {
                    return;
                }

                if (handler == null)
                {
                    store.Remove(key);
                    return;
                }

                handlers.Remove(handler);
                if (handlers.Count == 0)
                {
                    store.Remove(key);
                }
            }
        }

        private static bool IsPattern(string value)
        {
            return value.Contains('*') || value.Contains('?');
        }

        private static bool PatternMatches(string value, string pattern)
        {
            var regexPattern = "^" +
                               Regex.Escape(pattern)
                                   .Replace("\\*", ".*")
                                   .Replace("\\?", ".") +
                               "$";
            return Regex.IsMatch(value, regexPattern, RegexOptions.CultureInvariant);
        }
    }
}
