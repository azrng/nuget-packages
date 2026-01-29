using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        // 使用 ConcurrentDictionary 保证线程安全，无需额外锁
        private readonly ConcurrentDictionary<string, ChannelSubscription> _activeSubscriptions = new();

        public RedisProvider(IOptions<RedisConfig> options, RedisManage redisManage,
                             ILogger<RedisProvider> logger)
        {
            _redisManage = redisManage;
            _logger = logger;
            _redisConfig = options.Value;
        }

        public async Task<string> GetAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                var redisKey = GetKey(key);
                return await _redisManage.Database.StringGetAsync(redisKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"redis缓存报错 key:{key} message:{ex.GetExceptionAndStack()}");
                return null;
            }
        }

        public async Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                var redisKey = GetKey(key);
                var value = await _redisManage.Database.StringGetAsync(redisKey);
                return value.HasValue ? GetObject<T>(value) : default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"redis缓存报错 key:{key} message:{ex.GetExceptionAndStack()}");
                return default;
            }
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<T> getData, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                var redisKey = GetKey(key);

                // 直接从 Redis 获取原始字符串值，避免重复调用 GetKey
                var rawValue = await _redisManage.Database.StringGetAsync(redisKey);
                T value = default;

                if (rawValue.HasValue)
                {
                    value = GetObject<T>(rawValue);
                }

                if (value is null || value.Equals(default(T)))
                {
                    _logger.LogInformation($"redis读取为空，开始执行查询操作：key:{key}");
                    value = getData();

                    // 检查是否应该缓存该值
                    if (ShouldCacheValue(value))
                    {
                        await _redisManage.Database.StringSetAsync(redisKey, GetJsonStr(value), expiry);
                    }
                    else
                    {
                        var reason = value == null || value.Equals(default(T)) ? "查询结果为空或默认值" :
                            !_redisConfig.CacheEmptyCollections && IsEmptyCollectionOrString(value) ? "查询结果为空集合/空字符串且配置不缓存" : "其他原因";
                        _logger.LogInformation($"{reason}，不存储到Redis：key:{key}");
                    }
                }

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"redis缓存报错 key:{key} message:{ex.GetExceptionAndStack()}");
                return default;
            }
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> getData, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                var redisKey = GetKey(key);

                // 直接从 Redis 获取原始字符串值，避免重复调用 GetKey
                var rawValue = await _redisManage.Database.StringGetAsync(redisKey);
                T value = default;

                if (rawValue.HasValue)
                {
                    value = GetObject<T>(rawValue);
                }

                if (value is null || value.Equals(default(T)))
                {
                    _logger.LogInformation($"redis读取为空，开始执行查询操作：key:{key}");
                    value = await getData();

                    // 检查是否应该缓存该值
                    if (ShouldCacheValue(value))
                    {
                        await _redisManage.Database.StringSetAsync(redisKey, GetJsonStr(value), expiry);
                    }
                    else
                    {
                        var reason = value == null || value.Equals(default(T)) ? "查询结果为空或默认值" :
                            !_redisConfig.CacheEmptyCollections && IsEmptyCollectionOrString(value) ? "查询结果为空集合/空字符串且配置不缓存" : "其他原因";
                        _logger.LogInformation($"{reason}，不存储到Redis：key:{key}");
                    }
                }

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"redis缓存报错 key:{key} message:{ex.GetExceptionAndStack()}");
                return default;
            }
        }

        public async Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var redisKey = GetKey(key);
            try
            {
                if (ShouldCacheValue(value))
                    return await _redisManage.Database.StringSetAsync(redisKey, value, expiry);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"redis缓存报错 key:{key} message:{ex.GetExceptionAndStack()}");
                return default;
            }
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var redisKey = GetKey(key);
            try
            {
                if (ShouldCacheValue(value))
                    return await _redisManage.Database.StringSetAsync(redisKey, GetJsonStr(value), expiry);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"redis缓存报错 key:{key} message:{ex.GetExceptionAndStack()}");
                return default;
            }
        }

        public async Task<bool> RemoveAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            try
            {
                var redisKey = GetKey(key);

                return await _redisManage.Database.KeyDeleteAsync(redisKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"redis缓存报错 key:{key} message:{ex.GetExceptionAndStack()}");
                return default;
            }
        }

        public async Task<int> RemoveAsync(IEnumerable<string> keys)
        {
            if (keys is null)
                throw new ArgumentNullException(nameof(keys));
            try
            {
                var successCount = 0;
                foreach (var item in keys)
                {
                    var redisKey = GetKey(item);

                    var result = await _redisManage.Database.KeyDeleteAsync(redisKey);
                    if (result)
                        successCount++;
                }

                _logger.LogInformation($"批量删除完成，成功删除 {successCount} 个key，总共 {keys.Count()} 个key");
                return successCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"redis缓存报错 message:{ex.GetExceptionAndStack()}");
                return 0;
            }
        }

        public async Task<bool> RemoveMatchKeyAsync(string prefixMatchStr)
        {
            if (string.IsNullOrWhiteSpace(prefixMatchStr))
                throw new ArgumentNullException(nameof(prefixMatchStr));
            try
            {
                await _redisManage.Database.KeyDeleteAsync(await SearchRedisKeys(prefixMatchStr));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"根据前缀匹配符,批量删除Key异常，前缀匹配符:{prefixMatchStr}");
                return false;
            }
        }

        public async Task<bool> ExpireAsync(string key, TimeSpan expire)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            try
            {
                var redisKey = GetKey(key);
                return await _redisManage.Database.KeyExpireAsync(redisKey, expire);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"redis缓存报错 message:{ex.GetExceptionAndStack()}");
                return default;
            }
        }

        public async Task<bool> ExistAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                var redisKey = GetKey(key);
                return await _redisManage.Database.KeyExistsAsync(redisKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"redis缓存报错 message:{ex.GetExceptionAndStack()}");
                return default;
            }
        }

        /// <summary>
        /// 根据前缀匹配符,批量删除Key
        /// * 表示可以匹配多个任意字符
        /// ? 表示可以匹配单个任意字符
        /// [] 表示可以匹配指定范围内的字符
        /// </summary>
        /// <param name="prefixMatchStr">前缀匹配符</param>
        /// <returns></returns>
        private async Task<RedisKey[]> SearchRedisKeys(string prefixMatchStr)
        {
            var keys = new HashSet<RedisKey>();

            try
            {
                var nextCursor = 0;
                do
                {
                    var redisResult = await _redisManage.Database.ExecuteAsync("SCAN", nextCursor.ToString(),
                        "MATCH",
                        prefixMatchStr, "COUNT", "1000");

                    var innerResult = (RedisResult[])redisResult;
                    if (innerResult is null || innerResult.Length < 2)
                    {
                        _logger.LogWarning($"SCAN命令返回结果长度异常：{prefixMatchStr}");
                        break;
                    }

                    if (!int.TryParse(innerResult[0].ToString(), out nextCursor))
                    {
                        _logger.LogWarning($"SCAN命令返回游标格式异常：{prefixMatchStr}");
                        break;
                    }

                    var resultLines = (RedisKey[])innerResult[1];
                    if (resultLines is not null)
                    {
                        keys.UnionWith(resultLines);
                    }
                } while (nextCursor != 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SCAN命令执行异常，前缀匹配符:{prefixMatchStr} message:{ex.GetExceptionAndStack()}");
            }

            return keys.ToArray();
        }

        /// <summary>
        /// 获取redis key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string GetKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return key;

            if (string.IsNullOrWhiteSpace(_redisConfig?.KeyPrefix))
            {
                return key;
            }

            // 检查 key 是否已经包含前缀（包括冒号）
            var fullPrefix = _redisConfig.KeyPrefix + ":";
            if (key.StartsWith(fullPrefix, StringComparison.Ordinal))
            {
                return key;
            }

            // 添加前缀
            return fullPrefix + key;
        }

        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <param name="str"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T GetObject<T>(string str)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(str))
                    return default;

                return JsonConvert.DeserializeObject<T>(str);
            }
            catch (Exception ex)
            {
                var preview = str != null && str.Length > 200 ? str[..200] : str;
                _logger.LogError(ex, "JSON反序列化失败，目标类型：{TypeName}，数据长度：{DataLength}，数据内容：{DataPreview}...",
                    typeof(T).Name, str?.Length ?? 0, preview);
                return default;
            }
        }

        /// <summary>
        /// 对象序列化为字符串
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private string GetJsonStr<T>(T value)
        {
            try
            {
                if (value == null)
                    return null;

                return JsonConvert.SerializeObject(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"JSON序列化失败：{value}");
                return null;
            }
        }

        /// <summary>
        /// 检查类型是否为集合类型
        /// </summary>
        /// <param name="type">要检查的类型</param>
        /// <returns>如果是集合类型返回true，否则返回false</returns>
        private bool IsCollectionType(Type type)
        {
            if (type == typeof(string))
                return false;

            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        /// <summary>
        /// 检查值是否为空集合或空字符串
        /// </summary>
        /// <param name="value">要检查的值</param>
        /// <returns>如果是空集合、null或空字符串返回true，否则返回false</returns>
        private bool IsEmptyCollectionOrString<T>(T value)
        {
            if (value == null)
                return true;

            // 检查空字符串
            if (value is string str)
                return string.IsNullOrEmpty(str);

            var type = typeof(T);
            if (!IsCollectionType(type))
                return false;

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

        /// <summary>
        /// 检查是否应该缓存该值
        /// </summary>
        /// <param name="value">要检查的值</param>
        /// <returns>如果应该缓存返回true，否则返回false</returns>
        private bool ShouldCacheValue<T>(T value)
        {
            if (value == null || value.Equals(default(T)))
                return false;

            return _redisConfig.CacheEmptyCollections || !IsEmptyCollectionOrString(value);
        }

        #region 发布/订阅

        /// <summary>
        /// 发布消息到指定频道
        /// </summary>
        public async Task<long> PublishAsync<T>(string channel, T message)
        {
            if (string.IsNullOrWhiteSpace(channel))
                throw new ArgumentNullException(nameof(channel));

            try
            {
                var subscriber = _redisManage.ConnectionMultiplexer.GetSubscriber();
                var jsonMessage = GetJsonStr(message);

                if (string.IsNullOrEmpty(jsonMessage))
                {
                    _logger.LogWarning("发布消息失败，消息序列化为空，频道：{Channel}", channel);
                    return 0;
                }

                var result = await subscriber.PublishAsync(RedisChannel.Literal(channel), jsonMessage);
                _logger.LogInformation("发布消息成功，频道：{Channel}，订阅者数量：{SubscriberCount}", channel, result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发布消息失败，频道：{Channel}，消息：{Message}", channel, ex.GetExceptionAndStack());
                return 0;
            }
        }

        /// <summary>
        /// 订阅频道，当有消息发布时触发回调，返回订阅ID
        /// </summary>
        public Task<Guid> SubscribeAsync<T>(string channel, Action<T> handler, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(channel))
                throw new ArgumentNullException(nameof(channel));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            try
            {
                var subscriber = _redisManage.ConnectionMultiplexer.GetSubscriber();

                // 使用 GetOrAdd 原子操作，避免竞态条件
                var subscription = _activeSubscriptions.GetOrAdd(channel, ch =>
                {
                    // 创建新的订阅
                    var cts = new CancellationTokenSource();
                    var newSubscription = new ChannelSubscription(ch, cts);

                    // 订阅 Redis 频道
                    subscriber.Subscribe(RedisChannel.Literal(ch), (redisCh, value) =>
                    {
                        if (cts.Token.IsCancellationRequested)
                            return;

                        // 分发消息给所有订阅者
                        newSubscription.Broadcast(value, _logger, this);
                    });

                    _logger.LogInformation("创建新订阅，频道：{Channel}", ch);
                    return newSubscription;
                });

                // 生成订阅ID并添加订阅者
                var subscriptionId = Guid.NewGuid();
                var subscriberInfo = new SubscriberInfo
                {
                    Id = subscriptionId,
                    Handler = (msg) =>
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            try
                            {
                                var message = GetObject<T>(msg);
                                if (message != null)
                                {
                                    handler(message);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "处理订阅消息失败，频道：{Channel}", channel);
                            }
                        }
                    },
                    CancellationToken = cancellationToken
                };

                subscription.AddSubscriber(subscriberInfo);
                _logger.LogInformation("添加订阅者，频道：{Channel}，订阅者ID：{SubscriberId}，当前订阅者数量：{Count}",
                    channel, subscriberInfo.Id, subscription.SubscriberCount);

                // 启动后台任务监控取消令牌
                _ = Task.Run(async () =>
                {
                    try
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            await Task.Delay(1000, cancellationToken);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // 正常取消，不记录错误
                    }
                    finally
                    {
                        // 移除订阅者（如果这是最后一个订阅者，会自动取消 Redis 订阅）
                        await RemoveSubscriberAsync(channel, subscriptionId);
                    }
                }, cancellationToken);

                // 立即返回订阅ID
                return Task.FromResult(subscriptionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "订阅频道失败，频道：{Channel}，消息：{Message}", channel, ex.GetExceptionAndStack());
                return Task.FromResult(Guid.Empty);
            }
        }

        /// <summary>
        /// 内部方法：广播消息给所有订阅者
        /// </summary>
        private void BroadcastMessage<T>(string channel, IEnumerable<SubscriberInfo> subscribers, RedisValue value, ILogger logger)
        {
            foreach (var subscriber in subscribers.ToArray()) // ToArray 避免在枚举时修改集合
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
                    logger.LogError(ex, "订阅者处理消息失败，频道：{Channel}，订阅者ID：{SubscriberId}", channel, subscriber.Id);
                }
            }
        }

        /// <summary>
        /// 取消指定订阅者的订阅
        /// </summary>
        public Task UnsubscribeAsync(string channel, Guid subscriptionId)
        {
            if (string.IsNullOrWhiteSpace(channel))
                throw new ArgumentNullException(nameof(channel));

            return RemoveSubscriberAsync(channel, subscriptionId);
        }

        /// <summary>
        /// 强制取消频道的所有订阅（紧急情况使用）
        /// </summary>
        public Task UnsubscribeAllAsync(string channel)
        {
            if (string.IsNullOrWhiteSpace(channel))
                throw new ArgumentNullException(nameof(channel));

            try
            {
                if (_activeSubscriptions.TryGetValue(channel, out var subscription))
                {
                    // 强制取消 Redis 订阅
                    subscription.CancellationTokenSource.Cancel();
                    subscription.CancellationTokenSource.Dispose();
                    _activeSubscriptions.TryRemove(channel, out _);

                    var subscriber = _redisManage.ConnectionMultiplexer.GetSubscriber();
                    subscriber.Unsubscribe(RedisChannel.Literal(channel));

                    _logger.LogInformation("强制取消频道所有订阅成功：{Channel}，取消时订阅者数量：{Count}",
                        channel, subscription.SubscriberCount);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "强制取消频道订阅失败，频道：{Channel}，消息：{Message}", channel, ex.GetExceptionAndStack());
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// 移除订阅者（内部方法，当订阅者取消时调用）
        /// </summary>
        private Task RemoveSubscriberAsync(string channel, Guid subscriberId)
        {
            try
            {
                if (_activeSubscriptions.TryGetValue(channel, out var subscription))
                {
                    subscription.RemoveSubscriber(subscriberId);
                    _logger.LogInformation("移除订阅者，频道：{Channel}，订阅者ID：{SubscriberId}，剩余订阅者数量：{Count}",
                        channel, subscriberId, subscription.SubscriberCount);

                    // 如果没有订阅者了，取消整个频道订阅
                    if (subscription.SubscriberCount == 0)
                    {
                        subscription.CancellationTokenSource.Cancel();
                        subscription.CancellationTokenSource.Dispose();
                        _activeSubscriptions.TryRemove(channel, out _);

                        var subscriber = _redisManage.ConnectionMultiplexer.GetSubscriber();
                        subscriber.Unsubscribe(RedisChannel.Literal(channel));

                        _logger.LogInformation("频道 {Channel} 没有订阅者了，已取消 Redis 订阅", channel);
                    }
                }
                else
                {
                    _logger.LogWarning("尝试移除订阅者失败，频道 {Channel} 不存在或已被移除", channel);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除订阅者失败，频道：{Channel}，消息：{Message}", channel, ex.GetExceptionAndStack());
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// 订阅匹配模式的频道，返回订阅ID
        /// 注意：模式订阅暂时只支持单一订阅者
        /// </summary>
        public Task<Guid> SubscribePatternAsync<T>(string pattern, Action<string, T> handler, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                throw new ArgumentNullException(nameof(pattern));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            try
            {
                var subscriber = _redisManage.ConnectionMultiplexer.GetSubscriber();
                var patternKey = $"pattern:{pattern}";

                // 如果已经订阅过该模式，先取消订阅
                if (_activeSubscriptions.TryGetValue(patternKey, out var existingSubscription))
                {
                    existingSubscription.CancellationTokenSource.Cancel();
                    existingSubscription.CancellationTokenSource.Dispose();
                    _activeSubscriptions.TryRemove(patternKey, out _);
                }

                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var subscriptionId = Guid.NewGuid();
                var subscription = new ChannelSubscription(patternKey, cts);

                _activeSubscriptions[patternKey] = subscription;

                // 订阅模式
                subscriber.Subscribe(RedisChannel.Pattern(pattern), (ch, value) =>
                {
                    if (cts.Token.IsCancellationRequested)
                        return;

                    try
                    {
                        var message = GetObject<T>(value);
                        if (message != null)
                        {
                            var channelName = ch.ToString();
                            handler(channelName, message);
                            _logger.LogDebug("收到模式消息，模式：{Pattern}，频道：{Channel}，消息类型：{MessageType}",
                                pattern, channelName, typeof(T).Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "处理模式订阅消息失败，模式：{Pattern}", pattern);
                    }
                });

                _logger.LogInformation("订阅频道模式成功：{Pattern}，订阅ID：{SubscriptionId}", pattern, subscriptionId);

                // 启动后台任务监控取消令牌
                _ = Task.Run(async () =>
                {
                    try
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            await Task.Delay(1000, cancellationToken);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // 正常取消，不记录错误
                    }
                    finally
                    {
                        await UnsubscribePatternAllAsync(pattern);
                    }
                }, cancellationToken);

                return Task.FromResult(subscriptionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "订阅频道模式失败，模式：{Pattern}，消息：{Message}", pattern, ex.GetExceptionAndStack());
                return Task.FromResult(Guid.Empty);
            }
        }

        /// <summary>
        /// 取消指定模式订阅者的订阅
        /// 注意：模式订阅会强制取消所有订阅者
        /// </summary>
        public Task UnsubscribePatternAsync(string pattern, Guid subscriptionId)
        {
            // 模式订阅暂时只支持单一订阅者，所以直接取消所有
            return UnsubscribePatternAllAsync(pattern);
        }

        /// <summary>
        /// 强制取消模式的所有订阅
        /// </summary>
        public Task UnsubscribePatternAllAsync(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                throw new ArgumentNullException(nameof(pattern));

            try
            {
                var patternKey = $"pattern:{pattern}";

                if (_activeSubscriptions.TryGetValue(patternKey, out var subscription))
                {
                    subscription.CancellationTokenSource.Cancel();
                    subscription.CancellationTokenSource.Dispose();
                    _activeSubscriptions.TryRemove(patternKey, out _);
                }

                var subscriber = _redisManage.ConnectionMultiplexer.GetSubscriber();
                subscriber.Unsubscribe(RedisChannel.Pattern(pattern));

                _logger.LogInformation("取消订阅频道模式成功：{Pattern}", pattern);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消订阅频道模式失败，模式：{Pattern}，消息：{Message}", pattern, ex.GetExceptionAndStack());
                return Task.CompletedTask;
            }
        }

        #endregion
    }
}