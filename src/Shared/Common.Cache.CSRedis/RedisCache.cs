using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Cache.CSRedis
{
    public class RedisCache : IRedisCache
    {
        ///<inheritdoc cref="IRedisCache.ExistAsync(string)"/>
        public async Task<bool> ExistAsync(string key)
        {
            return await RedisHelper.ExistsAsync(key).ConfigureAwait(false);
        }

        #region string

        ///<inheritdoc cref="IRedisCache.GetAsync{T}(string)"/>
        public async Task<T> GetAsync<T>(string key)
        {
            return await RedisHelper.GetAsync<T>(key).ConfigureAwait(false);
        }

        ///<inheritdoc cref="IRedisCache.RemoveAsync(string)"/>
        public async Task RemoveAsync(string key)
        {
            await RedisHelper.DelAsync(key).ConfigureAwait(false);
        }

        ///<inheritdoc cref="IRedisCache.SetAsync(string, object, int)"/>
        public async Task SetAsync(string key, object value, int second = 600)
        {
            await RedisHelper.SetAsync(key, value, second).ConfigureAwait(false);
        }

        #endregion

        #region Hash
        ///<inheritdoc cref="IRedisCache.HGetAsync{T}(string, string)"/>
        public async Task<T> HGetAsync<T>(string key, string field = "data")
        {
            return await RedisHelper.HGetAsync<T>(key, field).ConfigureAwait(false);
        }

        ///<inheritdoc cref="IRedisCache.HGetALLAsync{T}(string)"/>
        public async Task<Dictionary<string, T>> HGetALLAsync<T>(string key) where T : class
        {
            return await RedisHelper.HGetAllAsync<T>(key).ConfigureAwait(false);
        }

        ///<inheritdoc cref="IRedisCache.HGetALLAsync(string)"/>
        public async Task<Dictionary<string, string>> HGetALLAsync(string key)
        {
            return await RedisHelper.HGetAllAsync(key).ConfigureAwait(false);
        }

        ///<inheritdoc cref="IRedisCache.HSetAsync{T}(string, string, T, double)"/>
        public async Task HSetAsync<T>(string key, string field, T value, double second = 600.0)
        {
            await RedisHelper.HSetAsync(key, "data", value).ConfigureAwait(false);
            await RedisHelper.ExpireAsync(key, TimeSpan.FromMinutes(second)).ConfigureAwait(false);
        }



        #endregion

        //public void Subscribe(string channel, string message)
        //{
        //    await RedisHelper.PublishAsync(channel, message);
        //}
    }
}