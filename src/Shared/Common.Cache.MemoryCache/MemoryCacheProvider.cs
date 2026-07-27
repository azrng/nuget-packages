using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

        public Task<string?> GetAsync(string key)
        {
            EnsureKey(key);

            if (_cache.TryGetValue(key, out var raw) && raw is CounterBox box)
            {
                return Task.FromResult<string?>(
                    Interlocked.Read(ref box.Value).ToString(CultureInfo.InvariantCulture));
            }

            return Task.FromResult<string?>(_cache.Get<string>(key));
        }

        public Task<T?> GetAsync<T>(string key)
        {
            EnsureKey(key);

            if (_cache.TryGetValue(key, out var raw) && raw is CounterBox box)
            {
                var counter = Interlocked.Read(ref box.Value);
                var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                return Task.FromResult((T?)Convert.ChangeType(counter, targetType, CultureInfo.InvariantCulture));
            }

            return Task.FromResult<T?>(_cache.Get<T>(key));
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

        public Task<long> IncrementAsync(string key, long value = 1)
        {
            return IncrementCoreAsync(key, value);
        }

        public Task<long> DecrementAsync(string key, long value = 1)
        {
            return IncrementCoreAsync(key, -value);
        }

        private async Task<long> IncrementCoreAsync(string key, long delta)
        {
            EnsureKey(key);

            // 快路径：计数器已存在时直接原子累加，不重建缓存条目，已设置的过期时间保持不变。
            if (_cache.TryGetValue(key, out var existing) && existing is CounterBox fastBox)
            {
                return Interlocked.Add(ref fastBox.Value, delta);
            }

            // 慢路径：首次创建或从已有整数值迁移，按 key 加锁保证计数器只创建一次。
            return await _keyManager.ExecuteSynchronizedAsync(key, () =>
            {
                if (_cache.TryGetValue(key, out var current) && current is CounterBox lockedBox)
                {
                    return Task.FromResult(Interlocked.Add(ref lockedBox.Value, delta));
                }

                long initial = 0;
                if (current is not null && !TryConvertToInt64(current, out initial))
                {
                    throw new InvalidOperationException($"缓存值不是整数，无法执行自增/自减，key:{key}");
                }

                // 与 Redis INCR 语义对齐：新建计数器不设置过期时间，如需过期请调用 ExpireAsync。
                // 注意：从已有非计数器条目迁移为计数器时，原条目的剩余过期时间无法读取，会被清除。
                var box = new CounterBox { Value = initial + delta };
                SetCore(key, box, expiry: null);
                return Task.FromResult(box.Value);
            });
        }

        private static bool TryConvertToInt64(object value, out long result)
        {
            switch (value)
            {
                case long longValue:
                    result = longValue;
                    return true;
                case int intValue:
                    result = intValue;
                    return true;
                case short shortValue:
                    result = shortValue;
                    return true;
                case byte byteValue:
                    result = byteValue;
                    return true;
                case string str when long.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed):
                    result = parsed;
                    return true;
                default:
                    result = 0;
                    return false;
            }
        }

        public Task<Dictionary<string, object>> GetAllAsync(IEnumerable<string> keys)
        {
            ArgumentNullException.ThrowIfNull(keys);

            var dict = new Dictionary<string, object>();
            foreach (var item in keys)
            {
                if (!string.IsNullOrWhiteSpace(item) && _cache.TryGetValue(item, out var value))
                {
                    dict[item] = value is CounterBox box ? Interlocked.Read(ref box.Value) : value!;
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
                if (TryGetCachedValue(key, out T? cachedValue))
                {
                    return cachedValue!;
                }
            }
            catch (CacheProviderException ex)
            {
                _logger.LogError(ex, "内存缓存读取失败 key:{Key} message:{Message}", key, ex.GetExceptionAndStack());
                if (_memoryConfig.FailThrowException)
                {
                    throw;
                }

                // 缓存读失败降级为未命中：直接回源返回真实数据，不再尝试写缓存。
                return await getData();
            }

            var effectiveExpiry = expiry ?? _memoryConfig.DefaultExpiry;
            return (await _keyManager.ExecuteSynchronizedAsync(key, async () =>
            {
                try
                {
                    if (TryGetCachedValue(key, out T? lockedCachedValue))
                    {
                        return lockedCachedValue!;
                    }
                }
                catch (CacheProviderException ex)
                {
                    _logger.LogError(ex, "内存缓存读取失败 key:{Key} message:{Message}", key, ex.GetExceptionAndStack());
                    if (_memoryConfig.FailThrowException)
                    {
                        throw;
                    }

                    return await getData();
                }

                var value = await getData();
                if (ShouldCacheValue(value))
                {
                    try
                    {
                        SetCore(key, value, effectiveExpiry);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "内存缓存写入失败 key:{Key} message:{Message}", key, ex.GetExceptionAndStack());
                        if (_memoryConfig.FailThrowException)
                        {
                            throw new CacheProviderException("内存缓存写入失败", ex);
                        }

                        // 写失败时已取得真实数据，降级为不缓存，仍返回数据。
                    }
                }
                else
                {
                    _logger.LogInformation("{Reason}，不写入内存缓存，key:{Key}", GetSkipCacheReason(value), key);
                }

                return value!;
            }))!;
        }

        private bool TryGetCachedValue<T>(string key, out T? value)
        {
            try
            {
                return _cache.TryGetValue(key, out value);
            }
            catch (Exception ex)
            {
                throw new CacheProviderException("内存缓存读取失败", ex);
            }
        }

        private void SetCore<T>(string key, T value, TimeSpan? expiry)
        {
            var entryOptions = CreateEntryOptions(key, expiry);
            _cache.Set(key, value, entryOptions);
            _keyManager.TrackKey(key);
        }

        private MemoryCacheEntryOptions CreateEntryOptions(string key, TimeSpan? expiry)
        {
            var options = new MemoryCacheEntryOptions();
            if (expiry.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiry;
            }

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

            try
            {
                // 匹配删除是低频操作，无需 Compiled；加超时防御异常模式导致的回溯放大
                return new Regex(builder.ToString(), RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
            }
            catch (ArgumentException ex)
            {
                // 典型场景：[ 未闭合导致生成的正则非法
                throw new ArgumentException($"通配符模式不合法：{pattern}", nameof(pattern), ex);
            }
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

        /// <summary>
        /// 计数器容器：Value 通过 Interlocked 原子累加，条目本身在自增时不重建，从而保留过期时间。
        /// 使用字段而非属性，Interlocked 需要 ref 访问。
        /// </summary>
        private sealed class CounterBox
        {
            public long Value;
        }

        private sealed class CacheProviderException : Exception
        {
            public CacheProviderException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }
    }
}
