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
    public class RedisManage : IDisposable
    {
        private readonly RedisConfig _redisConfig;
        private readonly ILogger<RedisManage> _logger;
        private readonly IRedisConnectionFactory _connectionFactory;
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        private IRedisConnection _connection;
        private IRedisDatabase _database;
        private IRedisSubscriber _subscriber;
        private Exception _lastConnectionException;
        private DateTimeOffset _nextRetryAt = DateTimeOffset.MinValue;
        private bool _disposed;

        public RedisManage(ILogger<RedisManage> logger, IOptions<RedisConfig> options)
            : this(logger, options, new StackExchangeRedisConnectionFactory())
        {
        }

        internal RedisManage(ILogger<RedisManage> logger,
                             IOptions<RedisConfig> options,
                             IRedisConnectionFactory connectionFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _redisConfig = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

            try
            {
                _logger.LogInformation("redis开始初始化连接");
                ConnectAsync().GetAwaiter().GetResult();
                _logger.LogInformation("redis初始化连接建立成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "redis初始化连接失败 连接字符串：{ConnectionString} message:{Message}",
                    _redisConfig.ConnectionString, ex.Message);
            }
        }

        public ConnectionMultiplexer ConnectionMultiplexer => (_connection as StackExchangeRedisConnection)?.ConnectionMultiplexer;

        internal IRedisDatabase Database => EnsureDatabase();

        internal IRedisSubscriber Subscriber => EnsureSubscriber();

        private bool HasConnection => _database != null && _subscriber != null;

        private IRedisDatabase EnsureDatabase()
        {
            EnsureConnected();
            return _database ?? throw CreateUnavailableException();
        }

        private IRedisSubscriber EnsureSubscriber()
        {
            EnsureConnected();
            return _subscriber ?? throw CreateUnavailableException();
        }

        private void EnsureConnected()
        {
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
                ConnectAsync().GetAwaiter().GetResult();
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
            IRedisConnection createdConnection = null;

            try
            {
                ThrowIfDisposed();

                if (HasConnection)
                {
                    return;
                }

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

        private InvalidOperationException CreateUnavailableException(Exception ex = null)
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
    }
}
