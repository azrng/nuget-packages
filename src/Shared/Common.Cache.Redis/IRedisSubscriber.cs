using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Common.Cache.Redis
{
    /// <summary>
    /// Redis 发布/订阅抽象。
    /// </summary>
    internal interface IRedisSubscriber
    {
        Task<long> PublishAsync(RedisChannel channel, RedisValue message);

        void Subscribe(RedisChannel channel, Action<RedisChannel, RedisValue> handler);

        /// <summary>handler 为 null 时移除该频道所有处理器，否则只移除指定处理器</summary>
        void Unsubscribe(RedisChannel channel, Action<RedisChannel, RedisValue>? handler);
    }
}
