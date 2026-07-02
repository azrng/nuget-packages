using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Azrng.Cache.MemoryCache
{
    /// <summary>
    /// 内存缓存实现
    /// </summary>
    public class MemoryCacheProvider : IMemoryCacheProvider
    {
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheOptions _memoryConfig;
        private readonly ILogger<MemoryCacheProvider> _logger;
        private readonly MemoryCacheKeyManager _keyManager;

        public MemoryCacheProvider(IMemoryCache memoryCache,
                                   IOptions<MemoryCacheOptions> options,
                                   ILogger<MemoryCacheProvider> logger,
                                   MemoryCacheKeyManager keyManager)
        {
            _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _memoryConfig = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _keyManager = keyManager ?? throw new ArgumentNullException(nameof(keyManager));
        }

        public Task<string> GetAsync(string key)
        {
            EnsureKey(key);
#pragma warning disable CS8619 // ICacheProvider 接口未启用 nullable，契约约束保持非 null 返回
            return Task.FromResult(_cache.Get<string>(key));
#pragma warning restore CS8619
        }

        public Task<T> GetAsync<T>(string key)
        {
            EnsureKey(key);
#pragma warning disable CS8619
            return Task.FromResult(_cache.Get<T>(key));
#pragma warning restore CS8619
        }

        public Task<T> GetOrCreateAsync<T>(string key, Func<T> getData, TimeSpan? expiry = null)
        {
            ArgumentNullException.ThrowIfNull(getData);
            return GetOrCreateInternalAsync(key, () => Task.FromResult(getData()), expiry);
        }

        public Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> getData, TimeSpan? expiry = null)
        {
            ArgumentNullException.ThrowIfNull(getData);
            return GetOrCreateInternalAsync(key, getData, expiry);
        }

        public Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            EnsureKey(key);

            if (!ShouldCacheValue(value))
            {
                _logger.LogInformation("{Reason}，不写入内存缓存，key:{Key}", GetSkipCacheReason(value), key);
                return Task.FromResult(false);
            }

            SetCore(key, value, expiry ?? _memoryConfig.DefaultExpiry);
            return Task.FromResult(true);
        }

        public Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            EnsureKey(key);

            if (!ShouldCacheValue(value))
            {
                _logger.LogInformation("{Reason}，不写入内存缓存，key:{Key}", GetSkipCacheReason(value), key);
                return Task.FromResult(false);
            }

            SetCore(key, value, expiry ?? _memoryConfig.DefaultExpiry);
            return Task.FromResult(true);
        }

        public Task<bool> RemoveAsync(string key)
        {
            EnsureKey(key);

            _cache.Remove(key);
            _keyManager.UntrackKey(key);
            return Task.FromResult(true);
        }

        public Task<int> RemoveAsync(IEnumerable<string> keys)
        {
            if (keys is null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            var keyList = keys
                .Where(key => !string.IsNullOrWhiteSpace(key))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (keyList.Length == 0)
            {
                return Task.FromResult(0);
            }

            var removedCount = 0;
            foreach (var item in keyList)
            {
                if (_cache.TryGetValue(item, out _))
                {
                    removedCount++;
                }

                _cache.Remove(item);
                _keyManager.UntrackKey(item);
            }

            _logger.LogInformation("批量删除缓存完成，成功删除 {RemovedCount} 个key，总共 {TotalCount} 个key", removedCount, keyList.Length);
            return Task.FromResult(removedCount);
        }

        public async Task<bool> RemoveMatchKeyAsync(string prefixMatchStr)
        {
            EnsureKey(prefixMatchStr, nameof(prefixMatchStr));

            var matcher = BuildWildcardRegex(prefixMatchStr);
            var cacheKeys = GetAllKeys();
            var matchedKeys = cacheKeys.Where(key => matcher.IsMatch(key)).ToArray();

            if (matchedKeys.Length == 0)
            {
                return true;
            }

            await RemoveAsync(matchedKeys);
            return true;
        }

        public Task<bool> ExpireAsync(string key, TimeSpan expire)
        {
            EnsureKey(key);

            if (_cache.TryGetValue(key, out var value))
            {
                SetCore(key, value, expire);
                return Task.FromResult(true);
            }

            _keyManager.UntrackKey(key);
            return Task.FromResult(false);
        }

        public Task<bool> ExistAsync(string key)
        {
            EnsureKey(key);
            return Task.FromResult(_cache.TryGetValue(key, out _));
        }

        public Task<Dictionary<string, object>> GetAllAsync(IEnumerable<string> keys)
        {
            ArgumentNullException.ThrowIfNull(keys);

            var dict = new Dictionary<string, object>();
            foreach (var item in keys)
            {
                if (!string.IsNullOrWhiteSpace(item) && _cache.TryGetValue(item, out var value))
                {
#pragma warning disable CS8601 // TryGetValue 的 value 在 nullable 上下文为 object?，接口字典声明为非 null
                    dict[item] = value;
#pragma warning restore CS8601
                }
            }

            return Task.FromResult(dict);
        }

        public Task RemoveAllKeyAsync()
        {
            foreach (var key in GetAllKeys())
            {
                _cache.Remove(key);
                _keyManager.UntrackKey(key);
            }

            return Task.CompletedTask;
        }

        public List<string> GetAllKeys()
        {
            return _keyManager.GetAllKeys();
        }

        private async Task<T> GetOrCreateInternalAsync<T>(string key, Func<Task<T>> getData, TimeSpan? expiry)
        {
            EnsureKey(key);
            ValidateValueType<T>();

            try
            {
#pragma warning disable CS8600 // ICacheProvider 契约未启用 nullable，TryGetValue 的 out 在本上下文为 T?
                if (_cache.TryGetValue(key, out T cachedValue))
#pragma warning restore CS8600
                {
#pragma warning disable CS8603 // 命中缓存才会走到此分支，cachedValue 必有值
                    return cachedValue;
#pragma warning restore CS8603
                }

                var effectiveExpiry = expiry ?? _memoryConfig.DefaultExpiry;
#pragma warning disable CS8603 // T 未约束 notnull，按 ICacheProvider 契约返回 Task<T>
                return await _keyManager.ExecuteSynchronizedAsync(key, async () =>
                {
#pragma warning disable CS8600
                    if (_cache.TryGetValue(key, out T lockedCachedValue))
#pragma warning restore CS8600
                    {
#pragma warning disable CS8603
                        return lockedCachedValue;
#pragma warning restore CS8603
                    }

                    var value = await getData();
                    if (ShouldCacheValue(value))
                    {
                        SetCore(key, value, effectiveExpiry);
                    }
                    else
                    {
                        _logger.LogInformation("{Reason}，不写入内存缓存，key:{Key}", GetSkipCacheReason(value), key);
                    }

#pragma warning disable CS8603 // value 来自 getData()，按契约返回 T
                    return value;
#pragma warning restore CS8603
                });
#pragma warning restore CS8603
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "内存缓存执行失败 key:{Key} message:{Message}", key, ex.GetExceptionAndStack());
                if (_memoryConfig.FailThrowException)
                {
                    throw;
                }
#pragma warning disable CS8603 // 接口契约约束返回非 null，此处按文档语义返回默认值
                return default;
#pragma warning restore CS8603
            }
        }

        private void SetCore<T>(string key, T value, TimeSpan expiry)
        {
            var entryOptions = CreateEntryOptions(key, expiry);
            _cache.Set(key, value, entryOptions);
            _keyManager.TrackKey(key);
        }

        private MemoryCacheEntryOptions CreateEntryOptions(string key, TimeSpan expiry)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry
            };

            options.RegisterPostEvictionCallback(static (_, _, _, state) =>
            {
                if (state is CacheEntryRegistration registration &&
                    !registration.Cache.TryGetValue(registration.Key, out _))
                {
                    registration.KeyManager.UntrackKey(registration.Key);
                }
            }, new CacheEntryRegistration(key, _cache, _keyManager));

            return options;
        }

        /// <summary>
        /// 验证值类型
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <exception cref="InvalidOperationException"></exception>
        private static void ValidateValueType<TResult>()
        {
            var typeResult = typeof(TResult);
            if (typeResult == typeof(IEnumerable) || typeResult == typeof(IQueryable))
            {
                throw new InvalidOperationException($"TResult of {typeResult} is not allowed, please use List<T> or T[] instead.");
            }

            if (!typeResult.IsGenericType)
            {
                return;
            }

            var genericTypeDefinition = typeResult.GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(IEnumerable<>) ||
                genericTypeDefinition == typeof(IAsyncEnumerable<>) ||
                genericTypeDefinition == typeof(IQueryable<>))
            {
                throw new InvalidOperationException($"TResult of {typeResult} is not allowed, please use List<T> or T[] instead.");
            }
        }

        private static void EnsureKey(string key, string paramName = "key")
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(paramName);
            }
        }

        private static Regex BuildWildcardRegex(string pattern)
        {
            var builder = new StringBuilder("^");
            var insideCharacterGroup = false;

            foreach (var ch in pattern)
            {
                if (!insideCharacterGroup)
                {
                    switch (ch)
                    {
                        case '*':
                            builder.Append(".*");
                            continue;
                        case '?':
                            builder.Append('.');
                            continue;
                        case '[':
                            insideCharacterGroup = true;
                            builder.Append('[');
                            continue;
                    }
                }
                else if (ch == ']')
                {
                    insideCharacterGroup = false;
                    builder.Append(']');
                    continue;
                }

                builder.Append(insideCharacterGroup ? ch : Regex.Escape(ch.ToString()));
            }

            builder.Append('$');
            return new Regex(builder.ToString(), RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        private static bool IsCollectionType(Type type)
        {
            return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
        }

        private static bool IsEmptyCollectionOrString<T>(T value)
        {
            if (value == null)
            {
                return true;
            }

            if (value is string str)
            {
                return string.IsNullOrEmpty(str);
            }

            var runtimeType = value.GetType();
            if (!IsCollectionType(runtimeType))
            {
                return false;
            }

            if (value is IEnumerable enumerable)
            {
                foreach (var _ in enumerable)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private bool ShouldCacheValue<T>(T value)
        {
            if (value == null)
            {
                return false;
            }

            return _memoryConfig.CacheEmptyCollections || !IsEmptyCollectionOrString(value);
        }

        private string GetSkipCacheReason<T>(T value)
        {
            if (value == null)
            {
                return "查询结果为空";
            }

            if (!_memoryConfig.CacheEmptyCollections && IsEmptyCollectionOrString(value))
            {
                return "查询结果为空集合/空字符串且配置为不缓存";
            }

            return "其他原因";
        }

        private sealed record CacheEntryRegistration(string Key, IMemoryCache Cache, MemoryCacheKeyManager KeyManager);
    }
}
