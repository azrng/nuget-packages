using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Cache.Redis
{
    public class RedisProvider : IRedisProvider
    {
        private readonly RedisConfig _redisConfig;
        private readonly RedisManage _redisManage;
        private readonly ILogger<RedisProvider> _logger;
        private readonly ConcurrentDictionary<string, ChannelSubscription> _activeSubscriptions = new();

        public RedisProvider(IOptions<RedisConfig> options,
                             RedisManage redisManage,
                             ILogger<RedisProvider> logger)
        {
            _redisConfig = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _redisManage = redisManage ?? throw new ArgumentNullException(nameof(redisManage));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GetAsync(string key)
        {
            EnsureKey(key);

            try
            {
                var redisValue = await _redisManage.Database.StringGetAsync(GetKey(key));
                return redisValue.HasValue ? redisValue.ToString() : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "redis缓存读取失败 key:{Key} message:{Message}", key, ex.GetExceptionAndStack());
                throw;
            }
        }

        public async Task<T> GetAsync<T>(string key)
        {
            EnsureKey(key);

            try
            {
                var redisValue = await _redisManage.Database.StringGetAsync(GetKey(key));
                return redisValue.HasValue && TryGetObject(redisValue, out T value) ? value : default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "redis缓存读取失败 key:{Key} message:{Message}", key, ex.GetExceptionAndStack());
                throw;
            }
        }

        public Task<T> GetOrCreateAsync<T>(string key, Func<T> getData, TimeSpan? expiry = null)
        {
            if (getData == null)
            {
                throw new ArgumentNullException(nameof(getData));
            }

            return GetOrCreateInternalAsync(key, () => Task.FromResult(getData()), expiry);
        }

        public Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> getData, TimeSpan? expiry = null)
        {
            if (getData == null)
            {
                throw new ArgumentNullException(nameof(getData));
            }

            return GetOrCreateInternalAsync(key, getData, expiry);
        }

        private async Task<T> GetOrCreateInternalAsync<T>(string key, Func<Task<T>> getData, TimeSpan? expiry)
        {
            EnsureKey(key);

            try
            {
                var redisKey = GetKey(key);
                var database = _redisManage.Database;
                var rawValue = await database.StringGetAsync(redisKey);

                if (rawValue.HasValue && TryGetObject(rawValue, out T cachedValue))
                {
                    return cachedValue;
                }

                _logger.LogInformation("redis读取为空，开始执行查询操作：key:{Key}", key);
                var value = await getData();

                if (ShouldCacheValue(value))
                {
                    await database.StringSetAsync(redisKey, GetJsonStr(value), expiry);
                }
                else
                {
                    _logger.LogInformation("{Reason}，不存储到Redis：key:{Key}", GetSkipCacheReason(value), key);
                }

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "redis缓存GetOrCreate失败 key:{Key} message:{Message}", key, ex.GetExceptionAndStack());
                throw;
            }
        }

        public async Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            EnsureKey(key);

            try
            {
                if (!ShouldCacheValue(value))
                {
                    return false;
                }

                return await _redisManage.Database.StringSetAsync(GetKey(key), value, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "redis缓存写入失败 key:{Key} message:{Message}", key, ex.GetExceptionAndStack());
                throw;
            }
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            EnsureKey(key);

            try
            {
                if (!ShouldCacheValue(value))
                {
                    return false;
                }

                return await _redisManage.Database.StringSetAsync(GetKey(key), GetJsonStr(value), expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "redis缓存写入失败 key:{Key} message:{Message}", key, ex.GetExceptionAndStack());
                throw;
            }
        }

        public async Task<bool> RemoveAsync(string key)
        {
            EnsureKey(key);

            try
            {
                return await _redisManage.Database.KeyDeleteAsync(GetKey(key));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "redis缓存删除失败 key:{Key} message:{Message}", key, ex.GetExceptionAndStack());
                throw;
            }
        }

        public async Task<int> RemoveAsync(IEnumerable<string> keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            try
            {
                var redisKeys = keys
                    .Where(key => !string.IsNullOrWhiteSpace(key))
                    .Select(GetKey)
                    .Distinct(StringComparer.Ordinal)
                    .Select(key => (RedisKey)key)
                    .ToArray();

                if (redisKeys.Length == 0)
                {
                    return 0;
                }

                var deletedCount = await _redisManage.Database.KeyDeleteAsync(redisKeys);
                _logger.LogInformation("批量删除完成，成功删除 {DeletedCount} 个key，总共 {TotalCount} 个key",
                    deletedCount, redisKeys.Length);
                return (int)deletedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "redis缓存批量删除失败 message:{Message}", ex.GetExceptionAndStack());
                throw;
            }
        }

        public async Task<bool> RemoveMatchKeyAsync(string prefixMatchStr)
        {
            EnsureKey(prefixMatchStr, nameof(prefixMatchStr));

            try
            {
                var matchedKeys = await SearchRedisKeys(GetKey(prefixMatchStr));
                if (matchedKeys.Length == 0)
                {
                    return true;
                }

                await _redisManage.Database.KeyDeleteAsync(matchedKeys);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据前缀匹配符批量删除Key异常，前缀匹配符:{PrefixMatchStr}", prefixMatchStr);
                throw;
            }
        }

        public async Task<bool> ExpireAsync(string key, TimeSpan expire)
        {
            EnsureKey(key);

            try
            {
                return await _redisManage.Database.KeyExpireAsync(GetKey(key), expire);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "redis缓存过期时间设置失败 key:{Key} message:{Message}", key, ex.GetExceptionAndStack());
                throw;
            }
        }

        public async Task<bool> ExistAsync(string key)
        {
            EnsureKey(key);

            try
            {
                return await _redisManage.Database.KeyExistsAsync(GetKey(key));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "redis缓存存在性检查失败 key:{Key} message:{Message}", key, ex.GetExceptionAndStack());
                throw;
            }
        }

        private async Task<RedisKey[]> SearchRedisKeys(string prefixMatchStr)
        {
            var keys = new HashSet<RedisKey>();

            try
            {
                ulong nextCursor = 0;
                do
                {
                    var scanResult = await _redisManage.Database.ScanAsync(nextCursor, prefixMatchStr, 1000);
                    nextCursor = scanResult.Cursor;
                    keys.UnionWith(scanResult.Keys ?? Array.Empty<RedisKey>());
                } while (nextCursor != 0);

                return keys.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SCAN命令执行异常，前缀匹配符:{PrefixMatchStr} message:{Message}",
                    prefixMatchStr, ex.GetExceptionAndStack());
                throw;
            }
        }

        private string GetKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return key;
            }

            if (string.IsNullOrWhiteSpace(_redisConfig.KeyPrefix))
            {
                return key;
            }

            var fullPrefix = _redisConfig.KeyPrefix + ":";
            return key.StartsWith(fullPrefix, StringComparison.Ordinal) ? key : fullPrefix + key;
        }

        private bool TryGetObject<T>(string str, out T value)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                value = default;
                return false;
            }

            try
            {
                value = JsonConvert.DeserializeObject<T>(str);
                return value != null || typeof(T).IsValueType;
            }
            catch (Exception ex)
            {
                var preview = str.Length > 200 ? str[..200] : str;
                _logger.LogError(ex,
                    "JSON反序列化失败，目标类型：{TypeName}，数据长度：{DataLength}，数据内容：{DataPreview}...",
                    typeof(T).Name, str.Length, preview);
                value = default;
                return false;
            }
        }

        private string GetJsonStr<T>(T value)
        {
            try
            {
                return JsonConvert.SerializeObject(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JSON序列化失败：{Value}", value);
                throw;
            }
        }

        private static void EnsureKey(string key, string paramName = "key")
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(paramName);
            }
        }

        private static bool IsCollectionType(Type type)
        {
            return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
        }

        private static bool IsEmptyCollectionOrString<T>(T value)
        {
            if (value == null)
            {
                return true;
            }

            if (value is string str)
            {
                return string.IsNullOrEmpty(str);
            }

            var runtimeType = value.GetType();
            if (!IsCollectionType(runtimeType))
            {
                return false;
            }

            if (value is IEnumerable enumerable)
            {
                foreach (var _ in enumerable)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private bool ShouldCacheValue<T>(T value)
        {
            if (value == null)
            {
                return false;
            }

            return _redisConfig.CacheEmptyCollections || !IsEmptyCollectionOrString(value);
        }

        private string GetSkipCacheReason<T>(T value)
        {
            if (value == null)
            {
                return "查询结果为空";
            }

            if (!_redisConfig.CacheEmptyCollections && IsEmptyCollectionOrString(value))
            {
                return "查询结果为空集合/空字符串且配置不缓存";
            }

            return "其他原因";
        }

        #region 发布/订阅

        public async Task<long> PublishAsync<T>(string channel, T message)
        {
            EnsureKey(channel, nameof(channel));

            if (message == null)
            {
                _logger.LogWarning("发布消息失败，消息为空，频道：{Channel}", channel);
                return 0;
            }

            try
            {
                var jsonMessage = GetJsonStr(message);
                if (string.IsNullOrWhiteSpace(jsonMessage))
                {
                    _logger.LogWarning("发布消息失败，消息序列化为空，频道：{Channel}", channel);
                    return 0;
                }

                var result = await _redisManage.Subscriber.PublishAsync(RedisChannel.Literal(channel), jsonMessage);
                _logger.LogInformation("发布消息成功，频道：{Channel}，订阅者数量：{SubscriberCount}", channel, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发布消息失败，频道：{Channel}，消息：{Message}", channel, ex.GetExceptionAndStack());
                throw;
            }
        }

        public Task<Guid> SubscribeAsync<T>(string channel, Action<T> handler, CancellationToken cancellationToken = default)
        {
            EnsureKey(channel, nameof(channel));
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            return SubscribeCoreAsync<T>(
                subscriptionKey: channel,
                redisChannel: RedisChannel.Literal(channel),
                handler: (actualChannel, message) => handler(message),
                cancellationToken: cancellationToken);
        }

        public Task UnsubscribeAsync(string channel, Guid subscriptionId)
        {
            EnsureKey(channel, nameof(channel));
            return RemoveSubscriberAsync(channel, RedisChannel.Literal(channel), subscriptionId, throwOnError: true);
        }

        public Task UnsubscribeAllAsync(string channel)
        {
            EnsureKey(channel, nameof(channel));
            return UnsubscribeAllCoreAsync(channel, RedisChannel.Literal(channel), throwOnError: true);
        }

        public Task<Guid> SubscribePatternAsync<T>(string pattern,
                                                   Action<string, T> handler,
                                                   CancellationToken cancellationToken = default)
        {
            EnsureKey(pattern, nameof(pattern));
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            return SubscribeCoreAsync<T>(
                subscriptionKey: GetPatternSubscriptionKey(pattern),
                redisChannel: RedisChannel.Pattern(pattern),
                handler: (actualChannel, message) => handler(actualChannel, message),
                cancellationToken: cancellationToken);
        }

        public Task UnsubscribePatternAsync(string pattern, Guid subscriptionId)
        {
            EnsureKey(pattern, nameof(pattern));
            return RemoveSubscriberAsync(GetPatternSubscriptionKey(pattern),
                RedisChannel.Pattern(pattern),
                subscriptionId,
                throwOnError: true);
        }

        public Task UnsubscribePatternAllAsync(string pattern)
        {
            EnsureKey(pattern, nameof(pattern));
            return UnsubscribeAllCoreAsync(GetPatternSubscriptionKey(pattern),
                RedisChannel.Pattern(pattern),
                throwOnError: true);
        }

        private async Task<Guid> SubscribeCoreAsync<T>(string subscriptionKey,
                                                       RedisChannel redisChannel,
                                                       Action<string, T> handler,
                                                       CancellationToken cancellationToken)
        {
            try
            {
                while (true)
                {
                    var subscription = GetOrCreateSubscription(subscriptionKey, redisChannel);
                    var subscriptionId = Guid.NewGuid();
                    var subscriberInfo = new SubscriberInfo
                    {
                        Id = subscriptionId,
                        Handler = (actualChannel, value) =>
                        {
                            if (TryGetObject(value, out T message))
                            {
                                handler(actualChannel.ToString(), message);
                            }
                        },
                        CancellationToken = cancellationToken
                    };

                    if (!subscription.TryAddSubscriber(subscriberInfo))
                    {
                        subscriberInfo.Dispose();
                        continue;
                    }

                    if (cancellationToken.CanBeCanceled)
                    {
                        var registration = cancellationToken.Register(static state =>
                        {
                            var registrationState = (SubscriptionCancellationState)state;
                            registrationState.Provider.RemoveSubscriberSafe(
                                registrationState.SubscriptionKey,
                                registrationState.RedisChannel,
                                registrationState.SubscriptionId);
                        }, new SubscriptionCancellationState(this, subscriptionKey, redisChannel, subscriptionId));

                        subscriberInfo.SetCancellationRegistration(registration);
                    }

                    _logger.LogInformation("添加订阅者，频道：{Channel}，订阅者ID：{SubscriberId}，当前订阅者数量：{Count}",
                        subscriptionKey, subscriptionId, subscription.SubscriberCount);
                    return subscriptionId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "订阅频道失败，频道：{Channel}，消息：{Message}", subscriptionKey, ex.GetExceptionAndStack());
                throw;
            }
        }

        private ChannelSubscription GetOrCreateSubscription(string subscriptionKey, RedisChannel redisChannel)
        {
            while (true)
            {
                if (_activeSubscriptions.TryGetValue(subscriptionKey, out var existingSubscription))
                {
                    if (!existingSubscription.IsClosing)
                    {
                        return existingSubscription;
                    }

                    _activeSubscriptions.TryRemove(subscriptionKey, out _);
                    continue;
                }

                var createdSubscription = new ChannelSubscription(subscriptionKey, new CancellationTokenSource());
                if (!_activeSubscriptions.TryAdd(subscriptionKey, createdSubscription))
                {
                    createdSubscription.Dispose();
                    continue;
                }

                try
                {
                    _redisManage.Subscriber.Subscribe(redisChannel, (channel, value) =>
                    {
                        if (createdSubscription.IsClosing || createdSubscription.CancellationTokenSource.IsCancellationRequested)
                        {
                            return;
                        }

                        createdSubscription.Broadcast(channel, value, _logger);
                    });

                    _logger.LogInformation("创建新订阅，频道：{Channel}", subscriptionKey);
                    return createdSubscription;
                }
                catch
                {
                    _activeSubscriptions.TryRemove(subscriptionKey, out _);
                    createdSubscription.TryBeginClose();
                    createdSubscription.CancellationTokenSource.Cancel();
                    createdSubscription.Dispose();
                    throw;
                }
            }
        }

        private Task RemoveSubscriberAsync(string subscriptionKey,
                                           RedisChannel redisChannel,
                                           Guid subscriberId,
                                           bool throwOnError)
        {
            try
            {
                if (!_activeSubscriptions.TryGetValue(subscriptionKey, out var subscription))
                {
                    _logger.LogWarning("尝试移除订阅者失败，频道 {Channel} 不存在或已被移除", subscriptionKey);
                    return Task.CompletedTask;
                }

                if (!subscription.RemoveSubscriber(subscriberId, out var removedSubscriber, out var remainingCount))
                {
                    _logger.LogWarning("尝试移除订阅者失败，频道 {Channel} 中不存在订阅者 {SubscriberId}",
                        subscriptionKey, subscriberId);
                    return Task.CompletedTask;
                }

                removedSubscriber.Dispose();
                _logger.LogInformation("移除订阅者，频道：{Channel}，订阅者ID：{SubscriberId}，剩余订阅者数量：{Count}",
                    subscriptionKey, subscriberId, remainingCount);

                if (remainingCount == 0)
                {
                    CloseSubscription(subscriptionKey, redisChannel, subscription);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除订阅者失败，频道：{Channel}，消息：{Message}",
                    subscriptionKey, ex.GetExceptionAndStack());

                if (throwOnError)
                {
                    throw;
                }

                return Task.CompletedTask;
            }
        }

        private Task UnsubscribeAllCoreAsync(string subscriptionKey, RedisChannel redisChannel, bool throwOnError)
        {
            try
            {
                if (!_activeSubscriptions.TryRemove(subscriptionKey, out var subscription))
                {
                    return Task.CompletedTask;
                }

                CloseSubscription(subscriptionKey, redisChannel, subscription);
                _logger.LogInformation("取消订阅成功：{Channel}", subscriptionKey);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消订阅失败，频道：{Channel}，消息：{Message}",
                    subscriptionKey, ex.GetExceptionAndStack());

                if (throwOnError)
                {
                    throw;
                }

                return Task.CompletedTask;
            }
        }

        private void CloseSubscription(string subscriptionKey, RedisChannel redisChannel, ChannelSubscription subscription)
        {
            if (!subscription.TryBeginClose())
            {
                return;
            }

            try
            {
                _activeSubscriptions.TryRemove(subscriptionKey, out _);

                foreach (var subscriber in subscription.RemoveAllSubscribers())
                {
                    subscriber.Dispose();
                }

                subscription.CancellationTokenSource.Cancel();
                _redisManage.Subscriber.Unsubscribe(redisChannel);
                _logger.LogInformation("频道 {Channel} 没有订阅者了，已取消 Redis 订阅", subscriptionKey);
            }
            finally
            {
                subscription.Dispose();
            }
        }

        private void RemoveSubscriberSafe(string subscriptionKey, RedisChannel redisChannel, Guid subscriptionId)
        {
            _ = RemoveSubscriberAsync(subscriptionKey, redisChannel, subscriptionId, throwOnError: false);
        }

        private static string GetPatternSubscriptionKey(string pattern)
        {
            return $"pattern:{pattern}";
        }

        private sealed record SubscriptionCancellationState(
            RedisProvider Provider,
            string SubscriptionKey,
            RedisChannel RedisChannel,
            Guid SubscriptionId);

        #endregion
    }
}
