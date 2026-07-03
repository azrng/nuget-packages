using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Common.Cache.Redis
{
    internal interface IRedisConnectionFactory
    {
        Task<IRedisConnection> ConnectAsync(ConfigurationOptions configurationOptions);
    }

    internal interface IRedisConnection : IDisposable
    {
        IRedisDatabase GetDatabase();

        IRedisSubscriber GetSubscriber();
    }

    internal interface IRedisDatabase
    {
        Task<RedisValue> StringGetAsync(RedisKey key);

        Task<bool> StringSetAsync(RedisKey key, RedisValue value, TimeSpan? expiry = null);

        Task<bool> KeyDeleteAsync(RedisKey key);

        Task<long> KeyDeleteAsync(RedisKey[] keys);

        Task<bool> KeyExpireAsync(RedisKey key, TimeSpan? expiry);

        Task<bool> KeyExistsAsync(RedisKey key);

        Task<RedisScanResult> ScanAsync(ulong cursor, string pattern, int count);
    }

    internal readonly record struct RedisScanResult(ulong Cursor, RedisKey[] Keys);

    internal interface IRedisSubscriber
    {
        Task<long> PublishAsync(RedisChannel channel, RedisValue message);

        void Subscribe(RedisChannel channel, Action<RedisChannel, RedisValue> handler);

        void Unsubscribe(RedisChannel channel);
    }

    internal sealed class StackExchangeRedisConnectionFactory : IRedisConnectionFactory
    {
        private readonly ILogger _logger;

        public StackExchangeRedisConnectionFactory()
            : this(NullLogger.Instance)
        {
        }

        public StackExchangeRedisConnectionFactory(ILogger logger)
        {
            _logger = logger ?? NullLogger.Instance;
        }

        public async Task<IRedisConnection> ConnectAsync(ConfigurationOptions configurationOptions)
        {
            var connection = await ConnectionMultiplexer.ConnectAsync(configurationOptions);
            return new StackExchangeRedisConnection(connection, _logger);
        }
    }

    internal sealed class StackExchangeRedisConnection : IRedisConnection
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly ILogger _logger;

        public StackExchangeRedisConnection(ConnectionMultiplexer connectionMultiplexer, ILogger logger)
        {
            _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
            _logger = logger ?? NullLogger.Instance;
            SubscribeConnectionEvents();
        }

        public ConnectionMultiplexer ConnectionMultiplexer => _connectionMultiplexer;

        public IRedisDatabase GetDatabase()
        {
            return new StackExchangeRedisDatabase(_connectionMultiplexer.GetDatabase());
        }

        public IRedisSubscriber GetSubscriber()
        {
            return new StackExchangeRedisSubscriber(_connectionMultiplexer.GetSubscriber());
        }

        public void Dispose()
        {
            UnsubscribeConnectionEvents();
            _connectionMultiplexer.Dispose();
        }

        private void SubscribeConnectionEvents()
        {
            _connectionMultiplexer.ConnectionFailed += OnConnectionFailed;
            _connectionMultiplexer.ConnectionRestored += OnConnectionRestored;
            _connectionMultiplexer.ErrorMessage += OnErrorMessage;
            _connectionMultiplexer.InternalError += OnInternalError;
        }

        private void UnsubscribeConnectionEvents()
        {
            _connectionMultiplexer.ConnectionFailed -= OnConnectionFailed;
            _connectionMultiplexer.ConnectionRestored -= OnConnectionRestored;
            _connectionMultiplexer.ErrorMessage -= OnErrorMessage;
            _connectionMultiplexer.InternalError -= OnInternalError;
        }

        private void OnConnectionFailed(object? sender, ConnectionFailedEventArgs e)
        {
            _logger.LogWarning(e.Exception,
                "Redis连接失败 endpoint:{EndPoint} failureType:{FailureType} connectionType:{ConnectionType}",
                e.EndPoint, e.FailureType, e.ConnectionType);
        }

        private void OnConnectionRestored(object? sender, ConnectionFailedEventArgs e)
        {
            _logger.LogInformation(
                "Redis连接恢复 endpoint:{EndPoint} failureType:{FailureType} connectionType:{ConnectionType}",
                e.EndPoint, e.FailureType, e.ConnectionType);
        }

        private void OnErrorMessage(object? sender, RedisErrorEventArgs e)
        {
            _logger.LogWarning("Redis错误消息 endpoint:{EndPoint} message:{Message}", e.EndPoint, e.Message);
        }

        private void OnInternalError(object? sender, InternalErrorEventArgs e)
        {
            _logger.LogError(e.Exception,
                "Redis内部错误 endpoint:{EndPoint} origin:{Origin}",
                e.EndPoint, e.Origin);
        }
    }

    internal sealed class StackExchangeRedisDatabase : IRedisDatabase
    {
        private readonly IDatabase _database;

        public StackExchangeRedisDatabase(IDatabase database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public Task<RedisValue> StringGetAsync(RedisKey key)
        {
            return _database.StringGetAsync(key);
        }

        public Task<bool> StringSetAsync(RedisKey key, RedisValue value, TimeSpan? expiry = null)
        {
            return _database.StringSetAsync(key, value, expiry);
        }

        public Task<bool> KeyDeleteAsync(RedisKey key)
        {
            return _database.KeyDeleteAsync(key);
        }

        public Task<long> KeyDeleteAsync(RedisKey[] keys)
        {
            return _database.KeyDeleteAsync(keys);
        }

        public Task<bool> KeyExpireAsync(RedisKey key, TimeSpan? expiry)
        {
            return _database.KeyExpireAsync(key, expiry);
        }

        public Task<bool> KeyExistsAsync(RedisKey key)
        {
            return _database.KeyExistsAsync(key);
        }

        public async Task<RedisScanResult> ScanAsync(ulong cursor, string pattern, int count)
        {
            var result = await _database.ExecuteAsync("SCAN",
                cursor.ToString(System.Globalization.CultureInfo.InvariantCulture),
                "MATCH",
                pattern,
                "COUNT",
                count.ToString(System.Globalization.CultureInfo.InvariantCulture));

            var innerResult = (RedisResult[]?)result;
            if (innerResult == null || innerResult.Length < 2)
            {
                return new RedisScanResult(cursor, Array.Empty<RedisKey>());
            }

            if (!ulong.TryParse(innerResult[0].ToString(),
                    System.Globalization.NumberStyles.None,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var nextCursor))
            {
                nextCursor = 0;
            }

            var keys = (RedisKey[]?)innerResult[1] ?? Array.Empty<RedisKey>();
            return new RedisScanResult(nextCursor, keys);
        }
    }

    internal sealed class StackExchangeRedisSubscriber : IRedisSubscriber
    {
        private readonly ISubscriber _subscriber;

        public StackExchangeRedisSubscriber(ISubscriber subscriber)
        {
            _subscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));
        }

        public Task<long> PublishAsync(RedisChannel channel, RedisValue message)
        {
            return _subscriber.PublishAsync(channel, message);
        }

        public void Subscribe(RedisChannel channel, Action<RedisChannel, RedisValue> handler)
        {
            _subscriber.Subscribe(channel, handler);
        }

        public void Unsubscribe(RedisChannel channel)
        {
            _subscriber.Unsubscribe(channel);
        }
    }
}
