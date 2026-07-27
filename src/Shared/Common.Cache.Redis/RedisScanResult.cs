using StackExchange.Redis;

namespace Common.Cache.Redis
{
    /// <summary>
    /// SCAN 命令返回的游标与键集合。
    /// </summary>
    internal readonly record struct RedisScanResult(ulong Cursor, RedisKey[] Keys);
}
