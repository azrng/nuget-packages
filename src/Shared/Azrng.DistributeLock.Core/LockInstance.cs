using Microsoft.Extensions.Logging;

namespace Azrng.DistributeLock.Core;

/// <summary>
/// 分布式锁实例
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
    /// 日志记录器
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 是否已经释放（使用 volatile 确保多线程可见性）
    /// </summary>
    private volatile bool _isDisposed;

    /// <summary>
    /// 是否获取到锁
    /// </summary>
    private bool _lockTook;

    /// <summary>
    /// 自动延长锁
    /// </summary>
    private readonly bool _autoExtendLock;

    /// <summary>
    /// 锁的过期时间
    /// </summary>
    private readonly TimeSpan _expireTime;

    /// <summary>
    /// 取消令牌源，用于取消自动续期任务
    /// </summary>
    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// 自动续期任务
    /// </summary>
    private Task? _autoExtendTask;

    /// <summary>
    /// 续期失败计数器
    /// </summary>
    private int _extendFailureCount;

    /// <summary>
    /// 最大连续续期失败次数
    /// </summary>
    private const int MaxExtendFailureCount = 3;

    /// <summary>
    /// 初始化 <see cref="LockInstance"/> 的新实例
    /// </summary>
    /// <param name="lockDataSourceProvider">分布式锁提供者</param>
    /// <param name="lockKey">锁定键</param>
    /// <param name="lockValue">锁定值</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="autoExtend">是否自动延长锁</param>
    /// <param name="expireTime">锁的过期时间</param>
    public LockInstance(
        ILockDataSourceProvider lockDataSourceProvider,
        string lockKey,
        string lockValue,
        ILogger logger,
        bool autoExtend,
        TimeSpan expireTime)
    {
        _lockDataSourceProvider = lockDataSourceProvider;
        _lockKey = lockKey;
        _lockValue = lockValue;
        _logger = logger;
        _autoExtendLock = autoExtend;
        _expireTime = expireTime;
    }

    /// <summary>
    /// 获取锁的键
    /// </summary>
    public string LockKey => _lockKey;

    /// <summary>
    /// 获取锁是否已释放
    /// </summary>
    public bool IsDisposed => _isDisposed;

    /// <summary>
    /// 获取是否启用自动续期
    /// </summary>
    public bool IsAutoExtendEnabled => _autoExtendLock;

    /// <summary>
    /// 获取锁的过期时间
    /// </summary>
    public TimeSpan ExpireTime => _expireTime;

    /// <summary>
    /// 获取续期失败次数
    /// </summary>
    public int ExtendFailureCount => _extendFailureCount;

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
    /// <param name="expire">锁的过期时间</param>
    /// <param name="getLockTimeOut">获取锁的超时时间</param>
    /// <returns>获取成功返回 true，否则返回 false</returns>
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
            if (_autoExtendLock)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _autoExtendTask = AutoExtendStart(_cancellationTokenSource.Token);
            }

            return true;
        }
        catch (TaskCanceledException ex)
        {
            // 因为一直获取不到锁才导致的报错
            _logger.LogError(ex, "等待后仍获取分布式锁失败：Key:{LockKey}, Value:{LockValue}", _lockKey, _lockValue);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取分布式锁失败：Key:{LockKey}, Value:{LockValue}", _lockKey, _lockValue);
            throw;
        }
    }

    /// <summary>
    /// 自动延期锁
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    private async Task AutoExtendStart(CancellationToken cancellationToken)
    {
        // 续期间隔：使用过期时间的1/3，至少1秒，最多10秒
        var extendInterval = TimeSpan.FromSeconds(Math.Max(1, Math.Min(10, _expireTime.TotalSeconds / 3)));

        while (!cancellationToken.IsCancellationRequested && !_isDisposed)
        {
            try
            {
                await Task.Delay(extendInterval, cancellationToken);

                // 使用原始过期时间进行续期
                var extendSuccess = await _lockDataSourceProvider.ExtendLockAsync(_lockKey, _lockValue, _expireTime);

                if (extendSuccess)
                {
                    _extendFailureCount = 0; // 重置失败计数器
                }
                else
                {
                    _extendFailureCount++;
                    _logger.LogWarning("分布式锁续期失败（第{FailureCount}次）：Key:{LockKey}, Value:{LockValue}",
                        _extendFailureCount, _lockKey, _lockValue);

                    if (_extendFailureCount >= MaxExtendFailureCount)
                    {
                        _logger.LogError("分布式锁续期连续失败{MaxCount}次，停止续期：Key:{LockKey}, Value:{LockValue}",
                            MaxExtendFailureCount, _lockKey, _lockValue);
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消，不记录日志
                break;
            }
            catch (Exception ex)
            {
                _extendFailureCount++;
                _logger.LogError(ex,
                    "分布式锁续期异常（第{FailureCount}次）：Key:{LockKey}, Value:{LockValue}",
                    _extendFailureCount, _lockKey, _lockValue);

                if (_extendFailureCount >= MaxExtendFailureCount)
                {
                    _logger.LogError("分布式锁续期异常连续失败{MaxCount}次，停止续期：Key:{LockKey}, Value:{LockValue}",
                        MaxExtendFailureCount, _lockKey, _lockValue);
                    break;
                }

                // 等待后重试
                try
                {
                    await Task.Delay(extendInterval, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
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

        // 标记为已释放
        _isDisposed = true;

        // 取消自动续期任务
        if (_cancellationTokenSource != null)
        {
            try
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // 已经被释放，忽略
            }

            _cancellationTokenSource = null;
        }

        // 等待自动续期任务完成
        if (_autoExtendTask != null)
        {
            try
            {
                await Task.WhenAny(_autoExtendTask, Task.Delay(TimeSpan.FromSeconds(2)));
            }
            catch (Exception)
            {
                // 忽略等待任务时的异常
            }

            _autoExtendTask = null;
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
    }

    /// <summary>
    /// 终结器，避免使用者忘记释放资源
    /// </summary>
    ~LockInstance()
    {
        DisposeAsync(false).GetAwaiter().GetResult();
    }
}