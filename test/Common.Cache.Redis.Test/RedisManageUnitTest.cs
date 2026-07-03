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

        // 验证 Dispose 会等待单个进行中的重连任务结束后再释放底层连接资源。
        // connectEntered 在工厂被调用时置完成，确定性确认重连已进入临界区，替代盲等 sleep。
        // 一小段 Task.Delay 仅用于让 Dispose 执行到 _disposeCts.Cancel() + WaitForActiveConnectAsync：
        // Dispose(bool) 内 _disposed=true→Cancel→等待 是连续同步语句，到达后 ThrowIfCancellationRequested
        // 会让重连任务在 SetResult 后抛 OperationCanceledException，从而 HasConnection=false、reconnectTask 抛"redis连接不可用"。
        [Fact]
        public async Task Dispose_WhenReconnectInProgress_WaitsForConnectTaskBeforeDisposingResources()
        {
            var database = new FakeRedisDatabase();
            var subscriber = new FakeRedisSubscriber();
            var connectedConnection = new FakeRedisConnection(database, subscriber);

            var blockedReconnect = new TaskCompletionSource<IRedisConnection>(TaskCreationOptions.RunContinuationsAsynchronously);
            var connectEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var firstFailure = true;

            var connectionFactory = new BlockingRedisConnectionFactory(_ =>
            {
                if (firstFailure)
                {
                    firstFailure = false;
                    return Task.FromException<IRedisConnection>(new InvalidOperationException("connect failed"));
                }

                connectEntered.TrySetResult(true);
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
            // 确定性地等待重连任务进入工厂调用（即已进入 semaphore 临界区），不再盲等固定时长。
            await connectEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));

            var disposeTask = Task.Run(redisManage.Dispose);
            // 让 Dispose 执行到 Cancel + WaitForActiveConnectAsync（同步连续语句）。
            await Task.Delay(100);
            // 重连任务仍未完成，Dispose 必然还在等待它。
            Assert.False(disposeTask.IsCompleted);

            blockedReconnect.SetResult(connectedConnection);
            await disposeTask.WaitAsync(TimeSpan.FromSeconds(5));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => reconnectTask);
            Assert.Equal("redis连接不可用", exception.Message);
            Assert.Equal(1, connectedConnection.DisposeCallCount);
        }

        // 验证 Dispose 会等待多个并发连接任务（一个持锁、一个排队等锁）全部结束后再返回。
        // 在 semaphore 串行化下，A 持锁跑 ConnectAsync、B 排队 WaitAsync，两者在发起时即被登记到 _activeConnectTasks。
        // Dispose 设置 _disposed=true 并 Cancel cts 后：
        //   - A 仍在工厂调用中阻塞（未观察 cts），其 task 未完成；
        //   - B 拿到锁后在 ThrowIfDisposed 处退出（B 的 task 因 OCE/Dispose 异常结束）。
        // A 未完成时 Dispose 不可能完成——这是 WaitForActiveConnectAsync 的逻辑必然，不依赖 Dispose 时序。
        // 因此即使 Dispose 启动很快，只要 A 仍被 firstReconnectBlocked 阻塞，disposeCompleted 就不可能为 true。
        // 让 A 以 SetException 失败（确定性），避免 SetResult 在 Dispose cancel 前后产生竞态。
        [Fact]
        public async Task Dispose_WhenMultipleReconnectsInProgress_WaitsForAllConnectTasks()
        {
            var firstReconnectBlocked = new TaskCompletionSource<IRedisConnection>(TaskCreationOptions.RunContinuationsAsynchronously);
            // A 进入 ConnectAsync（持锁）时置完成，作为"A 已进入临界区"的确定性信号。
            var firstConnectEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var disposeCompleted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var initialFailure = true;
            var firstReconnectServed = false;

            var connectionFactory = new BlockingRedisConnectionFactory(_ =>
            {
                if (initialFailure)
                {
                    initialFailure = false;
                    return Task.FromException<IRedisConnection>(new InvalidOperationException("connect failed"));
                }

                if (!firstReconnectServed)
                {
                    firstReconnectServed = true;
                    firstConnectEntered.TrySetResult(true);
                    return firstReconnectBlocked.Task;
                }

                // B 不应走到这里：Dispose 后 _disposed=true，B 在 ThrowIfDisposed 处退出。
                return Task.FromException<IRedisConnection>(new InvalidOperationException("unexpected second connect"));
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

            // 并发发起两个重连：A 持锁在 ConnectAsync 内阻塞，B 在 semaphore 内排队（同样被登记到 _activeConnectTasks）。
            var firstReconnect = redisManage.GetDatabaseAsync();
            await firstConnectEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var secondReconnect = redisManage.GetDatabaseAsync();

            var disposeTask = Task.Run(() =>
            {
                try
                {
                    redisManage.Dispose();
                }
                finally
                {
                    disposeCompleted.TrySetResult(true);
                }
            });
            // A 仍被 firstReconnectBlocked 阻塞、其 task 未完成，Dispose 不可能完成。
            Assert.False(disposeCompleted.Task.IsCompleted);

            // 让 A 失败（确定性，避免 SetResult 与 cancel 的竞态）。A 失败后两个连接 task 都结束，Dispose 才完成。
            firstReconnectBlocked.SetException(new InvalidOperationException("first connect failed"));
            await disposeTask.WaitAsync(TimeSpan.FromSeconds(5));

            // A、B 的 GetDatabaseAsync 均因 Dispose 抛"redis连接不可用"。
            await Assert.ThrowsAsync<InvalidOperationException>(() => firstReconnect);
            await Assert.ThrowsAsync<InvalidOperationException>(() => secondReconnect);
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
        }
    }
}
