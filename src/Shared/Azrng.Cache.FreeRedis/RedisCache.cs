using FreeRedis;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Azrng.Cache.FreeRedis
{
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
                throw new ArgumentNullException("key不能为空");

            return await _redis.ExistsAsync(key);
        }

        ///<inheritdoc cref="IRedisCache.GetStringAsync(string)"/>
        public async Task<string> GetStringAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key不能为空");

            return await _redis.GetAsync<string>(key);
        }

        ///<inheritdoc cref="IRedisCache.GetAsync{TEntity}(string)"/>
        public async Task<TEntity> GetAsync<TEntity>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key不能为空");

            return (await _redis.GetAsync<TEntity>(key)) ?? default;
        }

        ///<inheritdoc cref="IRedisCache.RemoveAsync(string)"/>
        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key不能为空");

            await _redis.DelAsync(key);
        }

        ///<inheritdoc cref="IRedisCache.SetAsync(string, object, int)"/>
        public async Task SetAsync<T>(string key, T value, int timeoutSeconds)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key不能为空");

            if (value is null)
                throw new ArgumentNullException("value不能为空");

            try
            {
                if (value.GetType().IsClass)
                {
                    await _redis.SetAsync(key, JsonConvert.SerializeObject(value), timeoutSeconds);
                }
                else
                {
                    await _redis.SetAsync(key, value, timeoutSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("存储redis报错", ex);
            }
        }

        ///<inheritdoc cref="IRedisCache.SetHashAsync(string, object, int)"/>
        public async Task SetHashAsync<T>(string key, T value, int timeoutSeconds)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key不能为空");

            if (value is null)
                throw new ArgumentNullException("value不能为空");

            try
            {
                //这处理还有问题
                // await _redis.LPushAsync(key, "aaa");
                await _redis.HMSetAsync(key, "aaaa", "bbb");
            }
            catch (Exception ex)
            {
                throw new ArgumentException("存储redis报错", ex);
            }
        }
    }
}