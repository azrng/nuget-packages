using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using System;

namespace Common.Cache.Redis
{
    /// <summary>
    /// 基于 StackExchange.Redis 的连接包装：暴露 Database/Subscriber，并订阅连接事件用于日志观测。
    /// </summary>
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
}
