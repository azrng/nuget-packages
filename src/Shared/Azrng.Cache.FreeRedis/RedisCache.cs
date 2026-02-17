using FreeRedis;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Azrng.Cache.FreeRedis
{
    /// <summary>
    /// Redis 缓存异常类
    /// </summary>
    public class RedisCacheException : Exception
    {
        public RedisCacheException(string message) : base(message) { }
        public RedisCacheException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class RedisCache : IRedisCache
    {
        private readonly RedisClient _redis;

        public RedisCache(RedisClient redis)
        {
            _redis = redis;
        }

        ///<inheritdoc cref="IRedisCache.ExistAsync(string)"/>
        public async Task<bool> ExistAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key), "缓存键不能为空");

            return await _redis.ExistsAsync(key).ConfigureAwait(false);
        }

        ///<inheritdoc cref="IRedisCache.GetStringAsync(string)"/>
        public async Task<string> GetStringAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key), "缓存键不能为空");

            return await _redis.GetAsync<string>(key).ConfigureAwait(false);
        }

        ///<inheritdoc cref="IRedisCache.GetAsync{TEntity}(string)"/>
        public async Task<TEntity> GetAsync<TEntity>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key), "缓存键不能为空");

            return (await _redis.GetAsync<TEntity>(key).ConfigureAwait(false)) ?? default;
        }

        ///<inheritdoc cref="IRedisCache.RemoveAsync(string)"/>
        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key), "缓存键不能为空");

            await _redis.DelAsync(key).ConfigureAwait(false);
        }

        ///<inheritdoc cref="IRedisCache.SetAsync(string, object, int)"/>
        public async Task SetAsync<T>(string key, T value, int timeoutSeconds)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key), "缓存键不能为空");

            if (value is null)
                throw new ArgumentNullException(nameof(value), "缓存值不能为空");

            try
            {
                if (value.GetType().IsClass && value.GetType() != typeof(string))
                {
                    // 使用安全的 JSON 序列化设置
                    var json = JsonConvert.SerializeObject(value, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        // 防止序列化可能包含敏感数据的对象
                        TypeNameHandling = TypeNameHandling.None
                    });
                    await _redis.SetAsync(key, json, timeoutSeconds).ConfigureAwait(false);
                }
                else
                {
                    await _redis.SetAsync(key, value, timeoutSeconds).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                // 不暴露内部错误信息，记录到日志
                throw new RedisCacheException("缓存操作失败，请稍后重试", ex);
            }
        }

        ///<inheritdoc cref="IRedisCache.SetHashAsync(string, object, int)"/>
        public async Task SetHashAsync<T>(string key, T value, int timeoutSeconds)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key), "缓存键不能为空");

            if (value is null)
                throw new ArgumentNullException(nameof(value), "缓存值不能为空");

            try
            {
                // TODO: 实现正确的 Hash 存储逻辑
                // await _redis.LPushAsync(key, "aaa");
                await _redis.HMSetAsync(key, "data", "value").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // 不暴露内部错误信息
                throw new RedisCacheException("缓存操作失败，请稍后重试", ex);
            }
        }
    }
}