using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private readonly MemoryConfig _memoryConfig;
        private readonly ILogger<MemoryCacheProvider> _logger;

        public MemoryCacheProvider(IMemoryCache memoryCache, IOptions<MemoryConfig> options, ILogger<MemoryCacheProvider> logger)
        {
            _cache = memoryCache;
            _logger = logger;
            _memoryConfig = options.Value;
        }

        public Task<string> GetAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            return Task.FromResult(_cache.Get<string>(key));
        }

        public Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            return Task.FromResult(_cache.Get<T>(key));
        }

        public Task<T> GetOrCreateAsync<T>(string key, Func<T> getData, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            ValidateValueType<T>();

            if (_cache.TryGetValue(key, out T result))
            {
                return Task.FromResult(result);
            }

            expiry ??= _memoryConfig.DefaultExpiry;

            result = getData.Invoke();

            // 根据配置决定是否缓存空集合和空字符串
            if (ShouldCacheValue(result))
            {
                _cache.Set<T>(key, result,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry, Priority = CacheItemPriority.NeverRemove });
            }

            return Task.FromResult(result);
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> getData, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            ValidateValueType<T>();

            if (_cache.TryGetValue(key, out T result))
            {
                return result;
            }

            expiry ??= _memoryConfig.DefaultExpiry;

            result = await getData();

            // 根据配置决定是否缓存空集合和空字符串
            if (ShouldCacheValue(result))
            {
                _cache.Set<T>(key, result,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry, Priority = CacheItemPriority.NeverRemove });
            }

            return result;
        }

        public Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            expiry ??= _memoryConfig.DefaultExpiry;
            if (ShouldCacheValue(value))
            {
                _cache.Set(key, value,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry, Priority = CacheItemPriority.NeverRemove });
                return Task.FromResult(true);
            }

            _logger.LogInformation($"空值/空集合不存储到Cache：key:{key}");
            return Task.FromResult(false);
        }

        public Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            expiry ??= _memoryConfig.DefaultExpiry;
            if (ShouldCacheValue(value))
            {
                _cache.Set(key, value,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry, Priority = CacheItemPriority.NeverRemove });
                return Task.FromResult(true);
            }

            _logger.LogInformation($"空值/空集合不存储到Cache：key:{key}");
            return Task.FromResult(false);
        }

        public Task<bool> RemoveAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            _cache.Remove(key);
            return Task.FromResult(true);
        }

        public Task<int> RemoveAsync(IEnumerable<string> keys)
        {
            if (keys is null)
                throw new ArgumentNullException(nameof(keys));
            try
            {
                var successCount = 0;
                foreach (var item in keys)
                {
                    _cache.Remove(item);
                    successCount++;
                }

                _logger.LogInformation($"批量删除缓存完成，成功删除 {successCount} 个key，涉及{keys.Count()} 个key");
                return Task.FromResult(successCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"内存缓存报错 message:{ex.Message}");
                return Task.FromResult(0);
            }
        }

        public async Task<bool> RemoveMatchKeyAsync(string prefixMatchStr)
        {
            var cacheKeys = GetAllKeys();
            var list = cacheKeys.Where(k => Regex.IsMatch(k, prefixMatchStr)).ToList();
            return await RemoveAsync(list) > 0;
        }

        public Task<bool> ExpireAsync(string key, TimeSpan expire)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (_cache.TryGetValue(key, out var value))
            {
                // 如果key存在，则更新过期时间
                _cache.Set(key, value,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = expire, Priority = CacheItemPriority.NeverRemove });
                return Task.FromResult(true);
            }

            // 如果key不存在，返回false
            return Task.FromResult(false);
        }

        public Task<bool> ExistAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            return Task.FromResult(_cache.TryGetValue(key, out var _));
        }

        /// <summary>
        /// 验证值类型
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <exception cref="InvalidOperationException"></exception>
        private static void ValidateValueType<TResult>()
        {
            //因为IEnumerable、IQueryable等有延迟执行的问题，造成麻烦，因此禁止用这些类型
            var typeResult = typeof(TResult);
            if (typeResult.IsGenericType) //如果是IEnumerable<String>这样的泛型类型，则把String这样的具体类型信息去掉，再比较
            {
                typeResult = typeResult.GetGenericTypeDefinition();
            }

            //注意用相等比较，不要用IsAssignableTo
            if (typeResult == typeof(IEnumerable<>) ||
                typeResult == typeof(IEnumerable) ||
                typeResult == typeof(IAsyncEnumerable<TResult>) ||
                typeResult == typeof(IQueryable<TResult>) ||
                typeResult == typeof(IQueryable))
            {
                throw new InvalidOperationException($"TResult of {typeResult} is not allowed, please use List<T> or T[] instead.");
            }
        }

        public Task<Dictionary<string, object>> GetAllAsync(IEnumerable<string> keys)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            var dict = new Dictionary<string, object>();
            foreach (var item in keys)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    var value = _cache.Get(item);
                    dict[item] = value;
                }
            }

            return Task.FromResult(dict);
        }

        public Task RemoveAllKeyAsync()
        {
            var keys = GetAllKeys();

            keys.ToList().ForEach(item => _cache.Remove(item));
            return Task.CompletedTask;
        }

        public List<string> GetAllKeys()
        {
            var keys = new List<string>();

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

#if NET7_0_OR_GREATER

            // .NET 7及以上版本使用_coherentState字段
            var coherentStateField = _cache.GetType().GetField("_coherentState", flags);
            if (coherentStateField != null)
            {
                var coherentStateValue = coherentStateField.GetValue(_cache);
                var entriesProperty = coherentStateValue?.GetType().GetProperty("EntriesCollection", flags) ??
                                      coherentStateValue?.GetType().GetProperty("StringEntriesCollection", flags);
                var entriesCollection = entriesProperty?.GetValue(coherentStateValue);

                if (entriesCollection != null)
                {
                    // 使用反射获取集合中的键
                    foreach (var item in (IEnumerable)entriesCollection)
                    {
                        var keyProp = item.GetType().GetProperty("Key");
                        var key = keyProp?.GetValue(item);
                        if (key != null)
                        {
                            keys.Add(key.ToString());
                        }
                    }
                }
            }
#else
            // .NET 6及以下版本使用_entries字段
            var entriesField = _cache.GetType().GetField("_entries", flags);
            if (entriesField is not null)
            {
                // .NET 6及以下版本
                var entries = entriesField.GetValue(_cache);
                if (entries is IDictionary dictionary)
                {
                    foreach (DictionaryEntry cacheItem in dictionary)
                    {
                        keys.Add(cacheItem.Key.ToString());
                    }
                }
            }

#endif
            return keys;
        }

        /// <summary>
        /// 初始化缓存 设置缓存时间
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="baseExpireSeconds"></param>
        private static void InitCacheEntry(ICacheEntry entry, int baseExpireSeconds)
        {
#if NET6_0_OR_GREATER

            //过期时间.Random.Shared 是.NET6新增的
            var sec = NextDouble(Random.Shared, baseExpireSeconds, baseExpireSeconds * 1.5);
#else
            var sec = NextDouble(new Random(), baseExpireSeconds, baseExpireSeconds * 1.5);
#endif

            var expiration = TimeSpan.FromSeconds(sec);
            entry.AbsoluteExpirationRelativeToNow = expiration;
        }

        /// <summary>
        ///  Returns a random integer that is within a specified range.
        /// </summary>
        /// <param name="random"></param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
        /// <returns></returns>
        private static double NextDouble(Random random, double minValue, double maxValue)
        {
            if (minValue >= maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(minValue), "minValue cannot be bigger than maxValue");
            }

            //https://stackoverflow.com/questions/65900931/c-sharp-random-number-between-double-minvalue-and-double-maxvalue
            var x = random.NextDouble();
            return x * maxValue + (1 - x) * minValue;
        }

        /// <summary>
        /// 检查类型是否为集合类型
        /// </summary>
        /// <param name="type">要检查的类型</param>
        /// <returns>如果是集合类型返回true，否则返回false</returns>
        private bool IsCollectionType(Type type)
        {
            return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
        }

        /// <summary>
        /// 检查值是否为空集合或空字符串
        /// </summary>
        /// <param name="value">要检查的值</param>
        /// <returns>如果是空集合、null或空字符串返回true，否则返回false</returns>
        private bool IsEmptyCollectionOrString<T>(T value)
        {
            if (value == null || value.Equals(default(T)))
                return true;

            // 检查空字符串
            if (value is string str)
                return string.IsNullOrEmpty(str);

            var type = typeof(T);
            if (!IsCollectionType(type))
                return false;

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

        /// <summary>
        /// 检查是否应该缓存该值
        /// </summary>
        /// <param name="value">要检查的值</param>
        /// <returns>如果应该缓存返回true，否则返回false</returns>
        private bool ShouldCacheValue<T>(T value)
        {
            if (value == null || value.Equals(default(T)))
                return false;

            return _memoryConfig.CacheEmptyCollections || !IsEmptyCollectionOrString(value);
        }
    }
}