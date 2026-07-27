using StackExchange.Redis;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Common.Cache.Redis
{
    /// <summary>
    /// 基于 StackExchange.Redis <see cref="IDatabase"/> 的数据库操作实现。
    /// </summary>
    internal sealed class StackExchangeRedisDatabase : IRedisDatabase
    {
        private readonly IDatabase _database;

        public StackExchangeRedisDatabase(IDatabase database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public Task<RedisValue> StringGetAsync(RedisKey key)
        {
            return _database.StringGetAsync(key);
        }

        public Task<bool> StringSetAsync(RedisKey key, RedisValue value, TimeSpan? expiry = null)
        {
            return _database.StringSetAsync(key, value, expiry);
        }

        public Task<bool> KeyDeleteAsync(RedisKey key)
        {
            return _database.KeyDeleteAsync(key);
        }

        public Task<long> KeyDeleteAsync(RedisKey[] keys)
        {
            return _database.KeyDeleteAsync(keys);
        }

        public Task<bool> KeyExpireAsync(RedisKey key, TimeSpan? expiry)
        {
            return _database.KeyExpireAsync(key, expiry);
        }

        public Task<bool> KeyExistsAsync(RedisKey key)
        {
            return _database.KeyExistsAsync(key);
        }

        public Task<long> StringIncrementAsync(RedisKey key, long value)
        {
            return _database.StringIncrementAsync(key, value);
        }

        public async Task<RedisScanResult> ScanAsync(ulong cursor, string pattern, int count)
        {
            var result = await _database.ExecuteAsync("SCAN",
                cursor.ToString(CultureInfo.InvariantCulture),
                "MATCH",
                pattern,
                "COUNT",
                count.ToString(CultureInfo.InvariantCulture));

            var innerResult = (RedisResult[]?)result;
            if (innerResult == null || innerResult.Length < 2)
            {
                return new RedisScanResult(cursor, Array.Empty<RedisKey>());
            }

            if (!ulong.TryParse(innerResult[0].ToString(),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var nextCursor))
            {
                nextCursor = 0;
            }

            var keys = (RedisKey[]?)innerResult[1] ?? Array.Empty<RedisKey>();
            return new RedisScanResult(nextCursor, keys);
        }
    }
}
