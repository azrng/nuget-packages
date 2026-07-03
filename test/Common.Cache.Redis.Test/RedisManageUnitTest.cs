using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Common.Cache.Redis.Test
{
    public class RedisManageUnitTest
    {
        [Fact]
        public async Task Database_WhenInitialConnectFails_ThrowsUntilRetryWindowExpires()
        {
            var database = new FakeRedisDatabase();
            var subscriber = new FakeRedisSubscriber();
            var connectionFactory = new FakeRedisConnectionFactory(
                () => throw new InvalidOperationException("connect failed"),
                () => new FakeRedisConnection(database, subscriber));

            var redisManage = new RedisManage(
                NullLogger<RedisManage>.Instance,
                Options.Create(new RedisCacheOptions
                {
                    ConnectionString = "localhost:6379,DefaultDatabase=0",
                    InitErrorIntervalSecond = 1
                }),
                connectionFactory);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => redisManage.GetDatabaseAsync());

            Assert.Equal("redis连接不可用", exception.Message);
            Assert.Equal(1, connectionFactory.ConnectCallCount);

            Thread.Sleep(1100);

            var connectedDatabase = await redisManage.GetDatabaseAsync();
            Assert.Same(database, connectedDatabase);
            Assert.Equal(2, connectionFactory.ConnectCallCount);
        }
    }
}
