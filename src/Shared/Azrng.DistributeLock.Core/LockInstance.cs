using Microsoft.Extensions.Logging;

namespace Azrng.DistributeLock.Core
{
    /// <summary>
    /// 锁实例
    /// </summary>
    public sealed class LockInstance : IAsyncDisposable
    {
        /// <summary>
        /// 分布式锁提供者
        /// </summary>
        private readonly ILockDataSourceProvider _lockDataSourceProvider;

        /// <summary>
        /// 锁定键
        /// </summary>
        private readonly string _lockKey;

        /// <summary>
        /// 锁定值
        /// </summary>
        private readonly string _lockValue;

        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// 是否已经释放
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// 是否获取到锁
        /// </summary>
        private bool _lockTook;

        /// <summary>
        /// 自动延长锁
        /// </summary>
        private readonly bool _autoExtendLock;

        public LockInstance(ILockDataSourceProvider lockDataSourceProvider, string lockKey, string lockValue,
            ILogger logger, bool autoExtend)
        {
            _lockDataSourceProvider = lockDataSourceProvider;
            _lockKey = lockKey;
            _lockValue = lockValue;
            _logger = logger;
            _autoExtendLock = autoExtend;
        }

        /// <summary>
        /// 释放锁
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 获取锁
        /// </summary>
        /// <returns></returns>
        public async Task<bool> LockAsync(TimeSpan expire, TimeSpan getLockTimeOut)
        {
            try
            {
                var flag = await _lockDataSourceProvider.TakeLockAsync(_lockKey, _lockValue, expire, getLockTimeOut);
                if (!flag)
                {
                    return false;
                }

                _lockTook = true;
                _ = AutoExtendStart();
                return true;
            }
            catch (TaskCanceledException ex)
            {
                // 因为一直获取不到锁才导致的报错
                _logger.LogError(ex, $"等待后仍获取分布式锁失败：Key:{_lockKey},value:{_lockValue}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取分布式锁失败：Key:{_lockKey},value:{_lockValue}");
                throw;
            }
        }

        /// <summary>
        /// 自动延期锁
        /// </summary>
        private async Task AutoExtendStart()
        {
            if (!_autoExtendLock) return;

            while (!_isDisposed)
            {
                await Task.Delay(1000);
                await _lockDataSourceProvider.ExtendLockAsync(_lockKey, _lockValue, TimeSpan.FromSeconds(2));
            }
        }

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="disposing"></param>
        private async Task DisposeAsync(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing && _lockTook)
            {
                try
                {
                    await _lockDataSourceProvider.ReleaseLockAsync(_lockKey, _lockValue);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"释放分布式锁崩溃：Key:{_lockKey},value:{_lockValue}");
                }
            }

            _isDisposed = true;
        }

        /// <summary>
        /// 终结器，避免使用者忘记释放资源
        /// </summary>
        ~LockInstance()
        {
            DisposeAsync(false).GetAwaiter().GetResult();
        }
    }
}