using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Cache.Redis
{
    /// <summary>
    /// redis管理
    /// </summary>
    public class RedisManage : IDisposable, IAsyncDisposable
    {
        private readonly RedisCacheOptions _redisConfig;
        private readonly ILogger<RedisManage> _logger;
        private readonly IRedisConnectionFactory _connectionFactory;
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        private readonly Task _initialConnectTask;
        private readonly CancellationTokenSource _disposeCts = new();

        // 跟踪当前正在进行的连接任务（首次或后续重连）。Dispose 时等待它们结束，
        // 避免释放 _semaphoreSlim 后仍有 in-flight 的 ConnectAsync 触碰已释放对象。
        private readonly object _activeConnectTasksLock = new();
        private readonly HashSet<Task> _activeConnectTasks = new();

        private IRedisConnection? _connection;
        private IRedisDatabase? _database;
        private IRedisSubscriber? _subscriber;
        private Exception? _lastConnectionException;
        private DateTimeOffset _nextRetryAt = DateTimeOffset.MinValue;
        private bool _disposed;

        public RedisManage(ILogger<RedisManage> logger, IOptions<RedisCacheOptions> options)
            : this(logger, options, new StackExchangeRedisConnectionFactory())
        {
        }

        internal RedisManage(ILogger<RedisManage> logger,
                             IOptions<RedisCacheOptions> options,
                             IRedisConnectionFactory connectionFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _redisConfig = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

            // 启动后台连接，不阻塞构造函数；首次操作时由 EnsureConnectedAsync 等待其结果。
            // 这样既保留了"启动即连"的语义，又避免了 sync-over-async 在带同步上下文的环境（如测试宿主）中死锁。
            // _disposeCts 协调后台任务与释放：Dispose 时取消，避免释放后后台任务仍写入连接字段。
            _initialConnectTask = StartConnectTracked(_disposeCts.Token);
        }

        /// <summary>
        /// 启动一个被跟踪的连接任务：把自身登记到 _activeConnectTasks，供 Dispose 等待。
        /// 返回原始的 ConnectCoreAsync 任务，调用方可直接 await 感知连接结果与异常；
        /// 任务结束后通过 fire-and-forget continuation 从集合移除自身。
        /// </summary>
        private Task StartConnectTracked(CancellationToken cancellationToken)
        {
            Task task;
            lock (_activeConnectTasksLock)
            {
                if (_disposed)
                {
                    return Task.FromException(new ObjectDisposedException(nameof(RedisManage)));
                }

                task = ConnectCoreAsync(cancellationToken);
                _activeConnectTasks.Add(task);
            }

            // 任务完成后清除标记（无论成功失败）。Dispose 可能在任务进行中读取此字段并 await。
            // 此 continuation 不 rethrow，仅做集合移除；原始 task 的异常通过返回值交给调用方观察。
            task.ContinueWith(
                _ =>
                {
                    lock (_activeConnectTasksLock)
                    {
                        _activeConnectTasks.Remove(task);
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
            return task;
        }

        public ConnectionMultiplexer? ConnectionMultiplexer => (_connection as StackExchangeRedisConnection)?.ConnectionMultiplexer;

        internal async Task<IRedisDatabase> GetDatabaseAsync()
        {
            await EnsureConnectedAsync().ConfigureAwait(false);
            return _database ?? throw CreateUnavailableException();
        }

        internal async Task<IRedisSubscriber> GetSubscriberAsync()
        {
            await EnsureConnectedAsync().ConfigureAwait(false);
            return _subscriber ?? throw CreateUnavailableException();
        }

        private bool HasConnection => _database != null && _subscriber != null;

        private async Task EnsureConnectedAsync()
        {
            ThrowIfDisposed();

            // 等待后台首次连接"完成"（无论成功失败）。首次连接的失败已由 ConnectAsync 内部
            // 通过 MarkConnectionFailure 记录（写入 _lastConnectionException 与 _nextRetryAt），
            // 这里不再重复抛首次异常，而是交给下面的 HasConnection 检查与重连逻辑统一处理。
            try
            {
                await _initialConnectTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // 释放导致的取消，视为连接不可用，交给下方统一处理。
            }
            catch
            {
                // 首次连接失败已记录，此处吞掉，避免每次访问都重抛同一个历史异常。
            }

            ThrowIfDisposed();

            if (HasConnection)
            {
                return;
            }

            if (DateTimeOffset.UtcNow < _nextRetryAt)
            {
                _logger.LogInformation("忽略连接，连接不可用 {Now} {NextRetryAt}",
                    DateTimeOffset.UtcNow, _nextRetryAt);
                throw CreateUnavailableException();
            }

            _logger.LogInformation("重新尝试建立redis连接 {Now}", DateTimeOffset.UtcNow);

            try
            {
                await StartConnectTracked(_disposeCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw CreateUnavailableException();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "redis建立连接失败 连接字符串：{ConnectionString} message:{Message}",
                    _redisConfig.ConnectionString, ex.Message);
                throw CreateUnavailableException(ex);
            }

            if (!HasConnection)
            {
                throw CreateUnavailableException();
            }
        }

        private async Task ConnectCoreAsync(CancellationToken cancellationToken)
        {
            await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
            IRedisConnection? createdConnection = null;

            try
            {
                ThrowIfDisposed();

                if (HasConnection)
                {
                    return;
                }

                _logger.LogInformation("redis开始初始化连接");
                var configurationOptions = ConfigurationOptions.Parse(_redisConfig.ConnectionString);
                createdConnection = await _connectionFactory.ConnectAsync(configurationOptions).ConfigureAwait(false);

                // 建立连接期间可能被 Dispose，放弃本次结果。
                cancellationToken.ThrowIfCancellationRequested();
                ThrowIfDisposed();

                var createdDatabase = createdConnection.GetDatabase();
                var createdSubscriber = createdConnection.GetSubscriber();

                var previousConnection = Interlocked.Exchange(ref _connection, createdConnection);
                _database = createdDatabase;
                _subscriber = createdSubscriber;
                _lastConnectionException = null;
                _nextRetryAt = DateTimeOffset.MinValue;
                createdConnection = null;

                previousConnection?.Dispose();
                _logger.LogInformation("redis初始化连接建立成功");
            }
            catch (OperationCanceledException)
            {
                // Dispose 期间的取消：不记为连接失败、不设退避窗口，仅释放本次已建连接后透出。
                createdConnection?.Dispose();
                throw;
            }
            catch (Exception ex)
            {
                MarkConnectionFailure(ex);
                createdConnection?.Dispose();
                throw;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        /// <summary>
        /// 等待当前正在进行的连接任务（首次或后续重连）全部结束。
        /// Dispose 调用，确保释放 _semaphoreSlim 前所有 in-flight 的 ConnectAsync 都已退出。
        /// </summary>
        private async Task WaitForActiveConnectAsync()
        {
            while (true)
            {
                Task[] activeTasks;
                lock (_activeConnectTasksLock)
                {
                    activeTasks = new Task[_activeConnectTasks.Count];
                    _activeConnectTasks.CopyTo(activeTasks);
                }

                if (activeTasks.Length == 0)
                {
                    return;
                }

                try
                {
                    await Task.WhenAll(activeTasks).ConfigureAwait(false);
                }
                catch
                {
                    // 连接任务因取消或失败结束，忽略异常，继续检查是否还有后续任务。
                }

                // await 返回后，若活动任务集合已清空则退出，否则继续等剩余任务。
                // 由于 Dispose 进入前已置 _disposed，新的 ConnectAsync 会在 ThrowIfDisposed 处退出，不会无限新增。
            }
        }

        private void MarkConnectionFailure(Exception ex)
        {
            _lastConnectionException = ex;
            var retryDelaySeconds = Math.Max(0, _redisConfig.InitErrorIntervalSecond);
            _nextRetryAt = DateTimeOffset.UtcNow.AddSeconds(retryDelaySeconds);
        }

        private InvalidOperationException CreateUnavailableException(Exception? ex = null)
        {
            return new InvalidOperationException("redis连接不可用", ex ?? _lastConnectionException);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(RedisManage));
            }
        }

        private bool TryBeginDispose()
        {
            lock (_activeConnectTasksLock)
            {
                if (_disposed)
                {
                    return false;
                }

                _disposed = true;
                return true;
            }
        }

        // 不提供终结器：RedisManage 不持有非托管资源，且生产中由 DI Singleton 托管，
        // ServiceProvider 释放时调用 Dispose()/DisposeAsync()。提供终结器会在终结器线程上
        // 触发 Dispose(false) 的 sync-over-async 等待（WaitForActiveConnectAsync().GetAwaiter().GetResult()），
        // 有阻塞终结器线程的风险，故移除。

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!TryBeginDispose())
            {
                return;
            }

            // 先标记释放，阻止新的连接任务进入 ConnectAsync。
            // 取消所有连接任务（首次 + 后续重连），并等待其结束，避免释放后仍写入连接字段或 fault。
            _disposeCts.Cancel();
            WaitForActiveConnectAsync().GetAwaiter().GetResult();

            if (disposing)
            {
                Interlocked.Exchange(ref _connection, null)?.Dispose();
                _database = null;
                _subscriber = null;
                _semaphoreSlim.Dispose();
                _disposeCts.Dispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!TryBeginDispose())
            {
                return;
            }

            // 先标记释放，阻止新的连接任务进入 ConnectAsync。
            // 取消所有连接任务（首次 + 后续重连），并等待其结束。
            _disposeCts.Cancel();
            await WaitForActiveConnectAsync().ConfigureAwait(false);

            var connection = Interlocked.Exchange(ref _connection, null);
            if (connection is IAsyncDisposable asyncConnection)
            {
                await asyncConnection.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                connection?.Dispose();
            }

            _database = null;
            _subscriber = null;
            _semaphoreSlim.Dispose();
            _disposeCts.Dispose();
        }
    }
}
