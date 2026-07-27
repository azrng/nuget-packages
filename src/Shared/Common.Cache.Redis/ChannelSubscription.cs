using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Cache.Redis
{
    /// <summary>
    /// 频道订阅信息，管理一个 Redis 频道的多个订阅者
    /// </summary>
    internal sealed class ChannelSubscription : IDisposable
    {
        private readonly ConcurrentDictionary<Guid, SubscriberInfo> _subscribers = new();

        // 保护"添加订阅者"与"进入关闭状态"两个决策的互斥：
        // 若不互斥，订阅者数减到 0 后、关闭开始前，并发新增的订阅者会被关闭流程一并清掉，
        // 调用方却拿到了看似有效的订阅 ID（消息静默丢失）。
        private readonly object _stateLock = new();
        private int _isClosing;
        private int _disposed;

        // 表示底层 Redis Subscribe 是否完成。并发调用方拿到本对象后必须 await 此 Task，
        // 才能确认订阅真正建立；失败时此 Task 会带上异常，调用方据此重试。
        private readonly TaskCompletionSource<ChannelSubscription> _initialization =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public string Channel { get; }

        public CancellationTokenSource CancellationTokenSource { get; }

        /// <summary>
        /// 注册到底层 Redis 的消息处理器。关闭时用它做精确退订，
        /// 避免 Unsubscribe(channel) 把同频道上新建订阅的处理器一并移除。
        /// </summary>
        public Action<RedisChannel, RedisValue>? RedisHandler { get; set; }

        public int SubscriberCount => _subscribers.Count;

        public bool IsClosing => Volatile.Read(ref _isClosing) == 1;

        /// <summary>等待底层 Redis 订阅初始化完成。</summary>
        public Task<ChannelSubscription> Initialization => _initialization.Task;

        public ChannelSubscription(string channel, CancellationTokenSource cancellationTokenSource)
        {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            CancellationTokenSource = cancellationTokenSource ?? throw new ArgumentNullException(nameof(cancellationTokenSource));
        }

        /// <summary>标记 Redis 订阅初始化成功。</summary>
        public void CompleteInitialization()
        {
            _initialization.TrySetResult(this);
        }

        /// <summary>标记 Redis 订阅初始化失败。</summary>
        public void FailInitialization(Exception exception)
        {
            _initialization.TrySetException(exception);
        }

        public bool TryAddSubscriber(SubscriberInfo subscriber)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            lock (_stateLock)
            {
                if (IsClosing)
                {
                    return false;
                }

                return _subscribers.TryAdd(subscriber.Id, subscriber);
            }
        }

        public bool RemoveSubscriber(Guid subscriberId, out SubscriberInfo? subscriber, out int remainingCount)
        {
            lock (_stateLock)
            {
                var removed = _subscribers.TryRemove(subscriberId, out subscriber);
                remainingCount = _subscribers.Count;
                return removed;
            }
        }

        public SubscriberInfo[] RemoveAllSubscribers()
        {
            var removedSubscribers = _subscribers.Values.ToArray();
            _subscribers.Clear();
            return removedSubscribers;
        }

        public bool TryBeginClose()
        {
            lock (_stateLock)
            {
                if (IsClosing)
                {
                    return false;
                }

                Volatile.Write(ref _isClosing, 1);
                return true;
            }
        }

        /// <summary>
        /// 仅当当前没有任何订阅者时才进入关闭状态。
        /// 与 TryAddSubscriber 在同一把锁下互斥：要么新增先成功（本方法返回 false，不关闭），
        /// 要么关闭先开始（后续新增返回 false，调用方重建订阅）。
        /// </summary>
        public bool TryBeginCloseIfEmpty()
        {
            lock (_stateLock)
            {
                if (IsClosing || _subscribers.Count > 0)
                {
                    return false;
                }

                Volatile.Write(ref _isClosing, 1);
                return true;
            }
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

        public Action<RedisChannel, RedisValue>? Handler { get; init; }

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
