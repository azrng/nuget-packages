using System.Collections.Concurrent;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Common.Cache.Redis.Test
{
    public class RedisProviderUnitTest
    {
        [Fact]
        public async Task GetOrCreateAsync_CachesDefaultValue()
        {
            var provider = CreateProvider(out _, out _);
            var loadCount = 0;

            var firstResult = await provider.GetOrCreateAsync("counter", () =>
            {
                loadCount++;
                return 0;
            }, TimeSpan.FromMinutes(1));

            var secondResult = await provider.GetOrCreateAsync("counter", () =>
            {
                loadCount++;
                return 1;
            }, TimeSpan.FromMinutes(1));

            Assert.Equal(0, firstResult);
            Assert.Equal(0, secondResult);
            Assert.Equal(1, loadCount);
        }

        [Fact]
        public async Task RemoveMatchKeyAsync_UsesPrefixedPattern()
        {
            var provider = CreateProvider(out var database, out _, new RedisConfig
            {
                ConnectionString = "localhost:6379,DefaultDatabase=0",
                KeyPrefix = "tenant",
                CacheEmptyCollections = false
            });

            await provider.SetAsync("user_1", "v1");
            await provider.SetAsync("user_2", "v2");

            var removed = await provider.RemoveMatchKeyAsync("user_*");

            Assert.True(removed);
            Assert.Single(database.ScanCalls);
            Assert.Equal("tenant:user_*", database.ScanCalls[0].Pattern);
            Assert.False(await provider.ExistAsync("user_1"));
            Assert.False(await provider.ExistAsync("user_2"));
        }

        [Fact]
        public async Task GetAsync_WhenRedisFails_Throws()
        {
            var provider = CreateProvider(out var database, out _);
            database.StringGetException = new InvalidOperationException("boom");

            await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetAsync("broken"));
        }

        [Fact]
        public async Task SubscribeAsync_AllowsDefaultValuePayloads_AndOnlyUnsubscribesOnce()
        {
            var provider = CreateProvider(out _, out var subscriber);
            var channel = "test:channel";
            var receivedMessages = new ConcurrentQueue<bool>();

            var subscriptionId1 = await provider.SubscribeAsync<bool>(channel, value => receivedMessages.Enqueue(value));
            var subscriptionId2 = await provider.SubscribeAsync<bool>(channel, value => receivedMessages.Enqueue(value));

            var publishCount = await provider.PublishAsync(channel, false);
            await Task.WhenAll(
                provider.UnsubscribeAsync(channel, subscriptionId1),
                provider.UnsubscribeAsync(channel, subscriptionId2));

            Assert.Equal(1, publishCount);
            Assert.Equal(2, receivedMessages.Count);
            Assert.All(receivedMessages, value => Assert.False(value));
            Assert.Equal(1, subscriber.UnsubscribedChannels.Count(name => name == channel));
        }

        private static RedisProvider CreateProvider(out FakeRedisDatabase database,
                                                    out FakeRedisSubscriber subscriber,
                                                    RedisConfig? redisConfig = null)
        {
            database = new FakeRedisDatabase();
            subscriber = new FakeRedisSubscriber();
            var configuredDatabase = database;
            var configuredSubscriber = subscriber;
            var effectiveConfig = redisConfig ?? new RedisConfig
            {
                ConnectionString = "localhost:6379,DefaultDatabase=0",
                KeyPrefix = "default",
                CacheEmptyCollections = false,
                InitErrorIntervalSecond = 0
            };

            var redisManage = new RedisManage(
                NullLogger<RedisManage>.Instance,
                Options.Create(effectiveConfig),
                new FakeRedisConnectionFactory(() => new FakeRedisConnection(configuredDatabase, configuredSubscriber)));

            return new RedisProvider(
                Options.Create(effectiveConfig),
                redisManage,
                NullLogger<RedisProvider>.Instance);
        }
    }
}
