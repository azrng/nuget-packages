using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Common.Cache.Redis
{
    /// <summary>
    /// Redis 数据库操作抽象，仅暴露 Provider 实际使用的命令，便于测试替换。
    /// </summary>
    internal interface IRedisDatabase
    {
        Task<RedisValue> StringGetAsync(RedisKey key);

        Task<bool> StringSetAsync(RedisKey key, RedisValue value, TimeSpan? expiry = null);

        Task<bool> KeyDeleteAsync(RedisKey key);

        Task<long> KeyDeleteAsync(RedisKey[] keys);

        Task<bool> KeyExpireAsync(RedisKey key, TimeSpan? expiry);

        Task<bool> KeyExistsAsync(RedisKey key);

        Task<long> StringIncrementAsync(RedisKey key, long value);

        Task<RedisScanResult> ScanAsync(ulong cursor, string pattern, int count);
    }
}
