using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Common.Cache.Redis
{
    /// <summary>
    /// 频道订阅信息，管理一个 Redis 频道的多个订阅者
    /// </summary>
    internal class ChannelSubscription
    {
        private readonly Dictionary<Guid, SubscriberInfo> _subscribers = new();
        private readonly object _lock = new();

        public string Channel { get; }

        public CancellationTokenSource CancellationTokenSource { get; }

        public int SubscriberCount => _subscribers.Count;

        public ChannelSubscription(string channel, CancellationTokenSource cancellationTokenSource)
        {
            Channel = channel;
            CancellationTokenSource = cancellationTokenSource;
        }

        /// <summary>
        /// 添加订阅者
        /// </summary>
        public void AddSubscriber(SubscriberInfo subscriber)
        {
            lock (_lock)
            {
                _subscribers[subscriber.Id] = subscriber;
            }
        }

        /// <summary>
        /// 移除订阅者
        /// </summary>
        public void RemoveSubscriber(Guid subscriberId)
        {
            lock (_lock)
            {
                _subscribers.Remove(subscriberId);
            }
        }

        /// <summary>
        /// 广播消息给所有订阅者
        /// </summary>
        public void Broadcast(RedisValue value, ILogger logger, RedisProvider provider)
        {
            List<SubscriberInfo> subscribers;
            lock (_lock)
            {
                subscribers = _subscribers.Values.ToList();
            }

            foreach (var subscriber in subscribers)
            {
                if (subscriber.CancellationToken.IsCancellationRequested)
                {
                    continue;
                }

                try
                {
                    subscriber.Handler?.Invoke(value);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "订阅者处理消息失败，频道：{Channel}，订阅者ID：{SubscriberId}",
                        Channel, subscriber.Id);
                }
            }
        }
    }

    /// <summary>
    /// 订阅者信息
    /// </summary>
    internal class SubscriberInfo
    {
        public Guid Id { get; set; }

        public Action<RedisValue> Handler { get; set; } = null!;

        public CancellationToken CancellationToken { get; set; }
    }
}