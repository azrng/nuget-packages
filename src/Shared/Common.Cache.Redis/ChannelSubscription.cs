using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace Common.Cache.Redis
{
    /// <summary>
    /// 频道订阅信息，管理一个 Redis 频道的多个订阅者
    /// </summary>
    internal sealed class ChannelSubscription : IDisposable
    {
        private readonly ConcurrentDictionary<Guid, SubscriberInfo> _subscribers = new();
        private int _isClosing;
        private int _disposed;

        public string Channel { get; }

        public CancellationTokenSource CancellationTokenSource { get; }

        public int SubscriberCount => _subscribers.Count;

        public bool IsClosing => Volatile.Read(ref _isClosing) == 1;

        public ChannelSubscription(string channel, CancellationTokenSource cancellationTokenSource)
        {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            CancellationTokenSource = cancellationTokenSource ?? throw new ArgumentNullException(nameof(cancellationTokenSource));
        }

        public bool TryAddSubscriber(SubscriberInfo subscriber)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            if (IsClosing)
            {
                return false;
            }

            return _subscribers.TryAdd(subscriber.Id, subscriber);
        }

        public bool RemoveSubscriber(Guid subscriberId, out SubscriberInfo subscriber, out int remainingCount)
        {
            var removed = _subscribers.TryRemove(subscriberId, out subscriber);
            remainingCount = _subscribers.Count;
            return removed;
        }

        public SubscriberInfo[] RemoveAllSubscribers()
        {
            var removedSubscribers = _subscribers.Values.ToArray();
            _subscribers.Clear();
            return removedSubscribers;
        }

        public bool TryBeginClose()
        {
            return Interlocked.CompareExchange(ref _isClosing, 1, 0) == 0;
        }

        public void Broadcast(RedisChannel channel, RedisValue value, ILogger logger)
        {
            foreach (var subscriber in _subscribers.Values.ToArray())
            {
                if (subscriber.CancellationToken.IsCancellationRequested)
                {
                    continue;
                }

                try
                {
                    subscriber.Handler?.Invoke(channel, value);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "订阅者处理消息失败，频道：{Channel}，订阅者ID：{SubscriberId}",
                        channel.ToString(), subscriber.Id);
                }
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
            {
                return;
            }

            CancellationTokenSource.Dispose();
        }
    }

    internal sealed class SubscriberInfo : IDisposable
    {
        private int _disposed;
        private CancellationTokenRegistration _cancellationRegistration;

        public Guid Id { get; init; }

        public Action<RedisChannel, RedisValue> Handler { get; init; }

        public CancellationToken CancellationToken { get; init; }

        public void SetCancellationRegistration(CancellationTokenRegistration cancellationRegistration)
        {
            _cancellationRegistration = cancellationRegistration;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
            {
                return;
            }

            _cancellationRegistration.Dispose();
        }
    }
}
