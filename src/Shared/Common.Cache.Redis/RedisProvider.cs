using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Cache.Redis
{
    public class RedisProvider : IRedisProvider
    {
        private readonly RedisConfig _redisConfig;
        private readonly RedisManage _redisManage;
        private readonly ILogger<RedisProvider> _logger;

        public RedisProvider(IOptions<RedisConfig> options, RedisManage redisManage,
                             ILogger<RedisProvider> logger)
        {
            _redisManage = redisManage;
            _logger = logger;
            _redisConfig = options.Value;
        }

        public async Task<string> GetAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                var redisKey = GetKey(key);
                return await _redisManage.Database.StringGetAsync(redisKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"redis缓存报错 key:{key} message:{ex.Message}");
                return null;
            }
        }

        public async Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                var redisKey = GetKey(key);
                var value = await _redisManage.Database.StringGetAsync(redisKey);
                return value.HasValue ? GetObject<T>(value) : default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"redis缓存报错 key:{key} message:{ex.Message}");
                return default;
            }
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<T> getData, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                var redisKey = GetKey(key);
                var value = await GetAsync<T>(redisKey);
                if (value is null || value.Equals(default(T)))
                {
                    _logger.LogInformation($"redis读取为空，开始执行查询操作：key:{key}");
                    value = getData();

                    // 检查是否应该缓存该值
                    if (ShouldCacheValue(value))
                    {
                        await _redisManage.Database.StringSetAsync(redisKey, GetJsonStr(value), expiry);
                    }
                    else
                    {
                        var reason = value == null || value.Equals(default(T)) ? "查询结果为空或默认值" :
                            !_redisConfig.CacheEmptyCollections && IsEmptyCollectionOrString(value) ? "查询结果为空集合/空字符串且配置不缓存" : "其他原因";
                        _logger.LogInformation($"{reason}，不存储到Redis：key:{key}");
                    }
                }

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"redis缓存报错 key:{key} message:{ex.Message}");
                return default;
            }
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> getData, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                var redisKey = GetKey(key);

                var value = await GetAsync<T>(redisKey);
                if (value is null || value.Equals(default(T)))
                {
                    _logger.LogInformation($"redis读取为空，开始执行查询操作：key:{key}");
                    value = await getData();

                    // 检查是否应该缓存该值
                    if (ShouldCacheValue(value))
                    {
                        await _redisManage.Database.StringSetAsync(redisKey, GetJsonStr(value), expiry);
                    }
                    else
                    {
                        var reason = value == null || value.Equals(default(T)) ? "查询结果为空或默认值" :
                            !_redisConfig.CacheEmptyCollections && IsEmptyCollectionOrString(value) ? "查询结果为空集合/空字符串且配置不缓存" : "其他原因";
                        _logger.LogInformation($"{reason}，不存储到Redis：key:{key}");
                    }
                }

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"redis缓存报错 key:{key} message:{ex.Message}");
                return default;
            }
        }

        public async Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var redisKey = GetKey(key);
            try
            {
                if (ShouldCacheValue(value))
                    return await _redisManage.Database.StringSetAsync(redisKey, value, expiry);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"redis缓存报错 key:{key} message:{ex.Message}");
                return default;
            }
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var redisKey = GetKey(key);
            try
            {
                if (ShouldCacheValue(value))
                    return await _redisManage.Database.StringSetAsync(redisKey, GetJsonStr(value), expiry);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"redis缓存报错 key:{key} message:{ex.Message}");
                return default;
            }
        }

        public async Task<bool> RemoveAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            try
            {
                var redisKey = GetKey(key);

                return await _redisManage.Database.KeyDeleteAsync(redisKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"redis缓存报错 key:{key} message:{ex.Message}");
                return default;
            }
        }

        public async Task<int> RemoveAsync(IEnumerable<string> keys)
        {
            if (keys is null)
                throw new ArgumentNullException(nameof(keys));
            try
            {
                var successCount = 0;
                foreach (var item in keys)
                {
                    var redisKey = GetKey(item);

                    var result = await _redisManage.Database.KeyDeleteAsync(redisKey);
                    if (result)
                        successCount++;
                }

                _logger.LogInformation($"批量删除完成，成功删除 {successCount} 个key，总共 {keys.Count()} 个key");
                return successCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"redis缓存报错 message:{ex.Message}");
                return 0;
            }
        }

        public async Task<bool> RemoveMatchKeyAsync(string prefixMatchStr)
        {
            if (string.IsNullOrWhiteSpace(prefixMatchStr))
                throw new ArgumentNullException(nameof(prefixMatchStr));
            try
            {
                await _redisManage.Database.KeyDeleteAsync(await SearchRedisKeys(prefixMatchStr));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"根据前缀匹配符,批量删除Key异常，前缀匹配符:{prefixMatchStr}");
                return false;
            }
        }

        public async Task<bool> ExpireAsync(string key, TimeSpan expire)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            try
            {
                var redisKey = GetKey(key);
                return await _redisManage.Database.KeyExpireAsync(redisKey, expire);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"redis缓存报错 message:{ex.Message}");
                return default;
            }
        }

        public async Task<bool> ExistAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                var redisKey = GetKey(key);
                return await _redisManage.Database.KeyExistsAsync(redisKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"redis缓存报错 message:{ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// 根据前缀匹配符,批量删除Key
        /// * 表示可以匹配多个任意字符
        /// ? 表示可以匹配单个任意字符
        /// [] 表示可以匹配指定范围内的字符
        /// </summary>
        /// <param name="prefixMatchStr">前缀匹配符</param>
        /// <returns></returns>
        private async Task<RedisKey[]> SearchRedisKeys(string prefixMatchStr)
        {
            var keys = new HashSet<RedisKey>();

            try
            {
                var nextCursor = 0;
                do
                {
                    var redisResult = await _redisManage.Database.ExecuteAsync("SCAN", nextCursor.ToString(),
                        "MATCH",
                        prefixMatchStr, "COUNT", "1000");

                    var innerResult = (RedisResult[])redisResult;
                    if (innerResult is null || innerResult.Length < 2)
                    {
                        _logger.LogWarning($"SCAN命令返回结果长度异常：{prefixMatchStr}");
                        break;
                    }

                    if (!int.TryParse(innerResult[0].ToString(), out nextCursor))
                    {
                        _logger.LogWarning($"SCAN命令返回游标格式异常：{prefixMatchStr}");
                        break;
                    }

                    var resultLines = (RedisKey[])innerResult[1];
                    if (resultLines is not null)
                    {
                        keys.UnionWith(resultLines);
                    }
                } while (nextCursor != 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SCAN命令执行异常，前缀匹配符:{prefixMatchStr}");
            }

            return keys.ToArray();
        }

        /// <summary>
        /// 获取redis key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string GetKey(string key)
        {
            if (string.IsNullOrWhiteSpace(_redisConfig?.KeyPrefix))
            {
                return key;
            }

            if (!key.StartsWith(_redisConfig.KeyPrefix))
            {
                return _redisConfig.KeyPrefix + ":" + key;
            }

            return key;
        }

        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <param name="str"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T GetObject<T>(string str)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(str))
                    return default;

                return JsonConvert.DeserializeObject<T>(str);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"JSON反序列化失败：{str}");
                return default;
            }
        }

        /// <summary>
        /// 对象序列化为字符串
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private string GetJsonStr<T>(T value)
        {
            try
            {
                if (value == null)
                    return null;

                return JsonConvert.SerializeObject(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"JSON序列化失败：{value}");
                return null;
            }
        }

        /// <summary>
        /// 检查类型是否为集合类型
        /// </summary>
        /// <param name="type">要检查的类型</param>
        /// <returns>如果是集合类型返回true，否则返回false</returns>
        private bool IsCollectionType(Type type)
        {
            if (type == typeof(string))
                return false;

            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        /// <summary>
        /// 检查值是否为空集合或空字符串
        /// </summary>
        /// <param name="value">要检查的值</param>
        /// <returns>如果是空集合、null或空字符串返回true，否则返回false</returns>
        private bool IsEmptyCollectionOrString<T>(T value)
        {
            if (value == null)
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

            return _redisConfig.CacheEmptyCollections || !IsEmptyCollectionOrString(value);
        }
    }
}