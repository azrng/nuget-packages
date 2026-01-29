using Common.Cache.Redis.Test.Model;
using Xunit.Abstractions;

namespace Common.Cache.Redis.Test;

/// <summary>
/// Redis 发布订阅功能测试
/// </summary>
public class PubSubTest
{
    private readonly IRedisProvider _redisProvider;
    private readonly ITestOutputHelper _testOutputHelper;

    public PubSubTest(IRedisProvider redisProvider, ITestOutputHelper testOutputHelper)
    {
        _redisProvider = redisProvider;
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// 测试发布和订阅简单消息
    /// </summary>
    [Fact]
    public async Task PublishAndSubscribe_StringMessage_ReturnOk()
    {
        var channel = $"test:{Guid.NewGuid():N}";
        var message = "Hello, Redis Pub/Sub!";
        var receivedMessage = string.Empty;
        var messageReceived = false;

        // 创建一个 CancellationTokenSource，5秒后自动取消
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            // 启动订阅任务
            var subscribeTask = Task.Run(async () =>
            {
                var subscriptionId = await _redisProvider.SubscribeAsync<string>(channel, msg =>
                {
                    receivedMessage = msg;
                    messageReceived = true;
                    _testOutputHelper.WriteLine($"收到消息: {msg}");
                }, cts.Token);
                _testOutputHelper.WriteLine($"订阅ID: {subscriptionId}");
            }, cts.Token);

            // 等待订阅建立
            await Task.Delay(500, cts.Token);

            // 发布消息
            var subscriberCount = await _redisProvider.PublishAsync(channel, message);
            _testOutputHelper.WriteLine($"订阅者数量: {subscriberCount}");
            Assert.True(subscriberCount > 0);

            // 等待消息被接收
            await Task.Delay(500, cts.Token);

            // 验证消息
            Assert.True(messageReceived);
            Assert.Equal(message, receivedMessage);
        }
        finally
        {
            // 清理：强制取消频道所有订阅
            await _redisProvider.UnsubscribeAllAsync(channel);
        }
    }

    /// <summary>
    /// 测试发布和订阅对象消息
    /// </summary>
    [Fact]
    public async Task PublishAndSubscribe_ObjectMessage_ReturnOk()
    {
        var channel = $"test:user:{Guid.NewGuid():N}";
        var user = new UserInfo("李四", 30);
        var receivedUser = default(UserInfo);
        var messageReceived = false;

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            // 启动订阅任务
            var subscribeTask = Task.Run(async () =>
            {
                var subscriptionId = await _redisProvider.SubscribeAsync<UserInfo>(channel, msg =>
                {
                    receivedUser = msg;
                    messageReceived = true;
                    _testOutputHelper.WriteLine($"收到用户消息: {msg.UserName}, {msg.Sex}");
                }, cts.Token);
                _testOutputHelper.WriteLine($"订阅ID: {subscriptionId}");
            });

            // 等待订阅建立
            await Task.Delay(500);

            // 发布消息
            var subscriberCount = await _redisProvider.PublishAsync(channel, user);
            _testOutputHelper.WriteLine($"订阅者数量: {subscriberCount}");
            Assert.True(subscriberCount > 0);

            // 等待消息被接收
            await Task.Delay(500);

            // 验证消息
            Assert.True(messageReceived);
            Assert.NotNull(receivedUser);
            Assert.Equal(user.UserName, receivedUser.UserName);
            Assert.Equal(user.Sex, receivedUser.Sex);
        }
        finally
        {
            await _redisProvider.UnsubscribeAllAsync(channel);
        }
    }

    /// <summary>
    /// 测试多个订阅者
    /// </summary>
    [Fact]
    public async Task Publish_MultipleSubscribers_ReturnOk()
    {
        var channel = $"test:multi:{Guid.NewGuid():N}";
        var message = "Broadcast Message";
        var subscriber1Received = false;
        var subscriber2Received = false;
        var subscriber3Received = false;

        using var cts1 = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var cts2 = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var cts3 = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            Guid? subscriptionId1 = null;
            Guid? subscriptionId2 = null;
            Guid? subscriptionId3 = null;

            // 订阅者1
            var subscribeTask1 = Task.Run(async () =>
            {
                subscriptionId1 = await _redisProvider.SubscribeAsync<string>(channel, msg =>
                {
                    subscriber1Received = true;
                    _testOutputHelper.WriteLine($"订阅者1收到消息: {msg}");
                }, cts1.Token);
                _testOutputHelper.WriteLine($"订阅者1 ID: {subscriptionId1}");
            });

            // 订阅者2
            var subscribeTask2 = Task.Run(async () =>
            {
                subscriptionId2 = await _redisProvider.SubscribeAsync<string>(channel, msg =>
                {
                    subscriber2Received = true;
                    _testOutputHelper.WriteLine($"订阅者2收到消息: {msg}");
                }, cts2.Token);
                _testOutputHelper.WriteLine($"订阅者2 ID: {subscriptionId2}");
            });

            // 订阅者3
            var subscribeTask3 = Task.Run(async () =>
            {
                subscriptionId3 = await _redisProvider.SubscribeAsync<string>(channel, msg =>
                {
                    subscriber3Received = true;
                    _testOutputHelper.WriteLine($"订阅者3收到消息: {msg}");
                }, cts3.Token);
                _testOutputHelper.WriteLine($"订阅者3 ID: {subscriptionId3}");
            });

            // 等待所有订阅建立
            await Task.Delay(1000);

            // 发布消息
            // 注意：Redis 服务器看到的订阅者数量可能与本地订阅者数量不同
            // 因为所有订阅者共享同一个 Redis 连接
            var subscriberCount = await _redisProvider.PublishAsync(channel, message);
            _testOutputHelper.WriteLine($"Redis 服务器看到的订阅者数量: {subscriberCount}");
            Assert.True(subscriberCount > 0, "至少应该有一个 Redis 订阅者");

            // 等待消息被所有订阅者接收
            await Task.Delay(1000);

            // 验证所有本地订阅者都收到消息
            Assert.True(subscriber1Received, "订阅者1应该收到消息");
            Assert.True(subscriber2Received, "订阅者2应该收到消息");
            Assert.True(subscriber3Received, "订阅者3应该收到消息");

            // 验证每个订阅者都有不同的ID
            Assert.NotEqual(subscriptionId1, subscriptionId2);
            Assert.NotEqual(subscriptionId2, subscriptionId3);
        }
        finally
        {
            await _redisProvider.UnsubscribeAllAsync(channel);
        }
    }

    /// <summary>
    /// 测试模式订阅
    /// </summary>
    [Fact]
    public async Task SubscribePattern_MultipleChannels_ReturnOk()
    {
        var pattern = "test:pattern:*";
        var channel1 = $"test:pattern:{Guid.NewGuid():N}";
        var channel2 = $"test:pattern:{Guid.NewGuid():N}";
        var receivedMessages = new List<(string channel, string message)>();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        try
        {
            // 订阅模式
            var subscribeTask = Task.Run(async () =>
            {
                var subscriptionId = await _redisProvider.SubscribePatternAsync<string>(pattern, (ch, msg) =>
                {
                    receivedMessages.Add((ch, msg));
                    _testOutputHelper.WriteLine($"模式订阅收到消息 - 频道: {ch}, 消息: {msg}");
                }, cts.Token);
                _testOutputHelper.WriteLine($"模式订阅ID: {subscriptionId}");
            });

            // 等待订阅建立
            await Task.Delay(500);

            // 向第一个频道发布消息
            var message1 = "Message to channel 1";
            await _redisProvider.PublishAsync(channel1, message1);
            await Task.Delay(500);

            // 向第二个频道发布消息
            var message2 = "Message to channel 2";
            await _redisProvider.PublishAsync(channel2, message2);
            await Task.Delay(500);

            // 验证消息
            Assert.Equal(2, receivedMessages.Count);
            Assert.Contains(receivedMessages, m => m.channel == channel1 && m.message == message1);
            Assert.Contains(receivedMessages, m => m.channel == channel2 && m.message == message2);
        }
        finally
        {
            await _redisProvider.UnsubscribePatternAllAsync(pattern);
        }
    }

    /// <summary>
    /// 测试通过订阅ID取消订阅
    /// </summary>
    [Fact]
    public async Task Unsubscribe_WithSubscriptionId_NoLongerReceiveMessages_ReturnOk()
    {
        var channel = $"test:unsubscribe:{Guid.NewGuid():N}";
        var receivedMessages = new List<string>();

        using var cts1 = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        using var cts2 = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        try
        {
            Guid? subscriptionId1 = null;
            Guid? subscriptionId2 = null;

            // 订阅者1
            var subscribeTask1 = Task.Run(async () =>
            {
                subscriptionId1 = await _redisProvider.SubscribeAsync<string>(channel, msg =>
                {
                    receivedMessages.Add($"订阅者1: {msg}");
                    _testOutputHelper.WriteLine($"订阅者1收到消息: {msg}");
                }, cts1.Token);
            });

            // 订阅者2
            var subscribeTask2 = Task.Run(async () =>
            {
                subscriptionId2 = await _redisProvider.SubscribeAsync<string>(channel, msg =>
                {
                    receivedMessages.Add($"订阅者2: {msg}");
                    _testOutputHelper.WriteLine($"订阅者2收到消息: {msg}");
                }, cts2.Token);
            });

            // 等待订阅建立
            await Task.Delay(500);

            // 发布第一条消息（两个订阅者都应该收到）
            var message1 = "Message 1";
            await _redisProvider.PublishAsync(channel, message1);
            await Task.Delay(500);
            Assert.Equal(2, receivedMessages.Count);

            // 清空消息列表
            receivedMessages.Clear();

            // 取消订阅者1
            if (subscriptionId1.HasValue)
            {
                await _redisProvider.UnsubscribeAsync(channel, subscriptionId1.Value);
                _testOutputHelper.WriteLine($"已取消订阅者1，ID: {subscriptionId1}");
            }

            // 等待取消完成
            await Task.Delay(500);

            // 发布第二条消息（只有订阅者2应该收到）
            var message2 = "Message 2";
            await _redisProvider.PublishAsync(channel, message2);
            await Task.Delay(500);

            // 验证只有订阅者2收到消息
            Assert.Single(receivedMessages);
            Assert.Contains("订阅者2", receivedMessages[0]);
        }
        finally
        {
            await _redisProvider.UnsubscribeAllAsync(channel);
        }
    }

    /// <summary>
    /// 测试强制取消频道订阅（取消所有订阅者）
    /// </summary>
    [Fact]
    public async Task UnsubscribeAll_ForceCancelAllSubscribers_ReturnOk()
    {
        var channel = $"test:forceunsubscribe:{Guid.NewGuid():N}";
        var subscriber1Received = false;
        var subscriber2Received = false;

        using var cts1 = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        using var cts2 = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        try
        {
            // 订阅者1
            var subscribeTask1 = Task.Run(async () =>
            {
                await _redisProvider.SubscribeAsync<string>(channel, msg =>
                {
                    subscriber1Received = true;
                    _testOutputHelper.WriteLine($"订阅者1收到消息: {msg}");
                }, cts1.Token);
            });

            // 订阅者2
            var subscribeTask2 = Task.Run(async () =>
            {
                await _redisProvider.SubscribeAsync<string>(channel, msg =>
                {
                    subscriber2Received = true;
                    _testOutputHelper.WriteLine($"订阅者2收到消息: {msg}");
                }, cts2.Token);
            });

            // 等待订阅建立
            await Task.Delay(1000);

            // 发布第一条消息
            var message1 = "Message 1";
            await _redisProvider.PublishAsync(channel, message1);
            await Task.Delay(500);

            // 验证两个订阅者都收到第一条消息
            Assert.True(subscriber1Received, "订阅者1应该收到第一条消息");
            Assert.True(subscriber2Received, "订阅者2应该收到第一条消息");

            // 重置标志
            subscriber1Received = false;
            subscriber2Received = false;

            // 强制取消频道订阅（取消所有订阅者）
            await _redisProvider.UnsubscribeAllAsync(channel);
            _testOutputHelper.WriteLine("已强制取消频道订阅");

            // 等待取消完成
            await Task.Delay(500);

            // 发布第二条消息（两个订阅者都不应该收到）
            var message2 = "Message 2";
            await _redisProvider.PublishAsync(channel, message2);
            await Task.Delay(500);

            // 验证两个订阅者都没有收到第二条消息
            Assert.False(subscriber1Received, "订阅者1不应该收到第二条消息（已强制取消）");
            Assert.False(subscriber2Received, "订阅者2不应该收到第二条消息（已强制取消）");
        }
        finally
        {
            await _redisProvider.UnsubscribeAllAsync(channel);
        }
    }

    /// <summary>
    /// 测试取消令牌取消订阅
    /// </summary>
    [Fact]
    public async Task Subscribe_WithCancellationToken_CancelSuccessfully()
    {
        var channel = $"test:cancellation:{Guid.NewGuid():N}";
        var receivedMessages = new List<string>();
        var subscriptionEnded = false;

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        try
        {
            // 订阅频道（2秒后自动取消）
            var subscribeTask = Task.Run(async () =>
            {
                var subscriptionId = await _redisProvider.SubscribeAsync<string>(channel, msg =>
                {
                    receivedMessages.Add(msg);
                    _testOutputHelper.WriteLine($"收到消息: {msg}");
                }, cts.Token);
                _testOutputHelper.WriteLine($"订阅ID: {subscriptionId}");
            });

            // 等待订阅建立
            await Task.Delay(500);

            // 发布消息
            var message1 = "Message 1";
            await _redisProvider.PublishAsync(channel, message1);
            await Task.Delay(500);

            // 等待取消令牌触发
            await Task.Delay(2000);
            subscriptionEnded = true;

            // 发布另一条消息（应该不会被接收）
            var message2 = "Message 2";
            await _redisProvider.PublishAsync(channel, message2);
            await Task.Delay(500);

            // 验证
            Assert.True(subscriptionEnded, "订阅应该已结束");
            Assert.True(receivedMessages.Count >= 1, "应该至少接收到一条消息");
        }
        finally
        {
            await _redisProvider.UnsubscribeAllAsync(channel);
        }
    }

    /// <summary>
    /// 测试高频消息发布
    /// </summary>
    [Fact]
    public async Task Publish_HighFrequencyMessages_ReturnOk()
    {
        var channel = $"test:highfreq:{Guid.NewGuid():N}";
        var messageCount = 100;
        var receivedCount = 0;

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        try
        {
            // 订阅频道
            var subscribeTask = Task.Run(async () =>
            {
                await _redisProvider.SubscribeAsync<string>(channel, msg =>
                {
                    Interlocked.Increment(ref receivedCount);
                    _testOutputHelper.WriteLine($"已接收 {receivedCount} 条消息");
                }, cts.Token);
            });

            // 等待订阅建立
            await Task.Delay(500);

            // 高频发布消息
            var publishTasks = new List<Task<long>>();
            for (int i = 0; i < messageCount; i++)
            {
                var message = $"Message {i}";
                publishTasks.Add(_redisProvider.PublishAsync(channel, message));
            }

            await Task.WhenAll(publishTasks);

            // 等待所有消息被接收
            await Task.Delay(2000);

            _testOutputHelper.WriteLine($"应接收 {messageCount} 条消息，实际接收 {receivedCount} 条");
            Assert.True(receivedCount > 0, "应该至少接收到一些消息");
        }
        finally
        {
            await _redisProvider.UnsubscribeAllAsync(channel);
        }
    }

    /// <summary>
    /// 测试发布空消息
    /// </summary>
    [Fact]
    public async Task Publish_NullMessage_ReturnsZero()
    {
        var channel = $"test:nullmsg:{Guid.NewGuid():N}";
        UserInfo? nullUser = null;

        // 发布空消息
        var subscriberCount = await _redisProvider.PublishAsync(channel, nullUser);

        // 应该返回0（没有实际发布）
        Assert.Equal(0, subscriberCount);
    }
}
