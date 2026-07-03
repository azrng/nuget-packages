using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
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
            _initialConnectTask = Task.Run(ConnectAsync);
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
            catch
            {
                // 首次连接失败已记录，此处吞掉，避免每次访问都重抛同一个历史异常。
            }

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
                await ConnectAsync().ConfigureAwait(false);
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

        private async Task ConnectAsync()
        {
            await _semaphoreSlim.WaitAsync();
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
                createdConnection = await _connectionFactory.ConnectAsync(configurationOptions);
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

        ~RedisManage()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Interlocked.Exchange(ref _connection, null)?.Dispose();
                _database = null;
                _subscriber = null;
                _semaphoreSlim.Dispose();
            }

            _disposed = true;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

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
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
