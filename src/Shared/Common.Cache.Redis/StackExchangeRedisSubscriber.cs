using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Common.Cache.Redis
{
    /// <summary>
    /// 基于 StackExchange.Redis <see cref="ISubscriber"/> 的发布/订阅实现。
    /// </summary>
    internal sealed class StackExchangeRedisSubscriber : IRedisSubscriber
    {
        private readonly ISubscriber _subscriber;

        public StackExchangeRedisSubscriber(ISubscriber subscriber)
        {
            _subscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));
        }

        public Task<long> PublishAsync(RedisChannel channel, RedisValue message)
        {
            return _subscriber.PublishAsync(channel, message);
        }

        public void Subscribe(RedisChannel channel, Action<RedisChannel, RedisValue> handler)
        {
            _subscriber.Subscribe(channel, handler);
        }

        public void Unsubscribe(RedisChannel channel, Action<RedisChannel, RedisValue>? handler)
        {
            _subscriber.Unsubscribe(channel, handler);
        }
    }
}
