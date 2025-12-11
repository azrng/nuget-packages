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

        /// <summary>
        /// redis连接对象
        /// </summary>
        private ConnectionMultiplexer _connection;

        /// <summary>
        /// 是否释放了资源
        /// </summary>
        private bool _disposed;

        public ConnectionMultiplexer ConnectionMultiplexer => _connection;

        /// <summary>
        /// 使用信号量保证同步操作
        /// </summary>
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

        /// <summary>
        /// 最后一次连接失败的时间
        /// </summary>
        private DateTime _initConnectErrorTime = DateTime.UtcNow;

        public RedisManage(ILogger<RedisManage> logger, IOptions<RedisConfig> options)
        {
            _logger = logger;
            _redisConfig = options.Value;
            try
            {
                _logger.LogInformation("redis开始初始化连接");
                ConnectAsync().GetAwaiter().GetResult();
                _logger.LogInformation("redis开始初始化建立成功");
            }
            catch (Exception ex)
            {
                // 如果第一次连接失败后续会再次连接
                _initConnectErrorTime = DateTime.UtcNow;
                _logger.LogError(ex,
                    $"redis初始化报错失败 连接字符串：{_redisConfig.ConnectionString} message:{ex.Message} stackTrace:{ex.StackTrace}");
            }
        }

        private IDatabase _database;

        /// <summary>
        /// 数据库对象
        /// </summary>
        public IDatabase Database
        {
            get
            {
                if (_database != null)
                    return _database;

                // 如果上次失败时间距离现在不够指定间隔 那么就提示连接不可用
                if (_initConnectErrorTime.AddSeconds(_redisConfig.InitErrorIntervalSecond) >= DateTime.UtcNow)
                {
                    _logger.LogInformation($"忽略连接，连接不可用 {DateTime.UtcNow} {_initConnectErrorTime}");
                    throw new Exception("redis连接不可用");
                }

                _logger.LogInformation($"重新尝试建立redis连接 {DateTime.UtcNow} {_initConnectErrorTime}");
                try
                {
                    ConnectAsync().GetAwaiter().GetResult();
                    return _database;
                }
                catch (Exception ex)
                {
                    _initConnectErrorTime = DateTime.UtcNow;
                    _logger.LogError(ex,
                        $"redis建立连接失败 连接字符串：{_redisConfig.ConnectionString} message:{ex.Message} stackTrace:{ex.StackTrace}");
                    throw;
                }
            }
        }

        /// <summary>
        /// 连接
        /// </summary>
        private async Task ConnectAsync()
        {
            try
            {
                await _semaphoreSlim.WaitAsync();
                if (_database == null)
                {
                    var configurationOptions = ConfigurationOptions.Parse(_redisConfig.ConnectionString);
                    _connection = await ConnectionMultiplexer.ConnectAsync(configurationOptions);
                    _database = _connection.GetDatabase();
                }
            }
            catch (Exception)
            {
                // 连接失败不会报错，而是等待下次重试
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        ~RedisManage()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            // 在此释放托管资源
            Dispose(true);
            // 通知垃圾回收机制不在调用终结器
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // 释放托管资源
                _connection?.Dispose();
            }

            _disposed = true;
        }
    }
}