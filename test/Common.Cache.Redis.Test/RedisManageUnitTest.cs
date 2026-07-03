using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

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

        [Fact]
        public async Task Dispose_WhenReconnectInProgress_WaitsForAllConnectTasksBeforeDisposingResources()
        {
            var database = new FakeRedisDatabase();
            var subscriber = new FakeRedisSubscriber();
            var connectedConnection = new FakeRedisConnection(database, subscriber);
            var blockedReconnect = new TaskCompletionSource<IRedisConnection>(TaskCreationOptions.RunContinuationsAsynchronously);
            var firstFailure = true;
            var connectionFactory = new BlockingRedisConnectionFactory(_ =>
            {
                if (firstFailure)
                {
                    firstFailure = false;
                    return Task.FromException<IRedisConnection>(new InvalidOperationException("connect failed"));
                }

                return blockedReconnect.Task;
            });

            var redisManage = new RedisManage(
                NullLogger<RedisManage>.Instance,
                Options.Create(new RedisCacheOptions
                {
                    ConnectionString = "localhost:6379,DefaultDatabase=0",
                    InitErrorIntervalSecond = 1
                }),
                connectionFactory);

            await Assert.ThrowsAsync<InvalidOperationException>(() => redisManage.GetDatabaseAsync());

            await Task.Delay(1100);
            var reconnectTask = redisManage.GetDatabaseAsync();
            await connectionFactory.WaitForConnectCallCountAsync(2);

            var disposeTask = Task.Run(redisManage.Dispose);
            await Task.Delay(100);

            Assert.False(disposeTask.IsCompleted);

            blockedReconnect.SetResult(connectedConnection);
            await disposeTask.WaitAsync(TimeSpan.FromSeconds(5));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => reconnectTask);
            Assert.Equal("redis连接不可用", exception.Message);
            Assert.Equal(1, connectedConnection.DisposeCallCount);
        }

        private sealed class BlockingRedisConnectionFactory : IRedisConnectionFactory
        {
            private readonly Func<ConfigurationOptions, Task<IRedisConnection>> _connect;

            public BlockingRedisConnectionFactory(Func<ConfigurationOptions, Task<IRedisConnection>> connect)
            {
                _connect = connect;
            }

            public int ConnectCallCount { get; private set; }

            public Task<IRedisConnection> ConnectAsync(ConfigurationOptions configurationOptions)
            {
                ConnectCallCount++;
                return _connect(configurationOptions);
            }

            public async Task WaitForConnectCallCountAsync(int expectedCount)
            {
                while (ConnectCallCount < expectedCount)
                {
                    await Task.Delay(10).WaitAsync(TimeSpan.FromSeconds(5));
                }
            }
        }
    }
}
