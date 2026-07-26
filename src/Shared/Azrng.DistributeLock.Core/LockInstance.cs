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
    /// 释放标记：0 未释放，1 已释放（Interlocked 保证并发 Dispose 只执行一次）
    /// </summary>
    private int _disposedFlag;

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
    /// 锁丢失通知源：续期连续失败导致锁可能已被他人获取时触发
    /// </summary>
    private readonly CancellationTokenSource _lockLostCts = new CancellationTokenSource();

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
    public LockInstance(ILockDataSourceProvider lockDataSourceProvider,
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
    public bool IsDisposed => Volatile.Read(ref _disposedFlag) == 1;

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
    /// 锁丢失通知令牌：自动续期连续失败、锁可能已被其他持有者获取时该令牌会被取消。
    /// 长耗时业务应监听该令牌并及时中止，避免互斥性失效
    /// </summary>
    public CancellationToken LockLostToken => _lockLostCts.Token;

    /// <summary>
    /// 获取锁
    /// </summary>
    /// <param name="expire">锁的过期时间</param>
    /// <param name="getLockTimeOut">获取锁的超时时间</param>
    /// <returns>获取成功返回 true，否则返回 false</returns>
    public async Task<bool> LockAsync(TimeSpan expire, TimeSpan getLockTimeOut)
    {
        if (expire <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(expire), "锁的过期时间必须大于0");
        if (getLockTimeOut < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(getLockTimeOut), "获取锁的超时时间不能小于0");

        try
        {
            var flag = await _lockDataSourceProvider.TakeLockAsync(_lockKey, _lockValue, expire, getLockTimeOut);
            if (!flag)
            {
                // 获取锁失败（超时或其他原因）是正常行为，不记录错误日志
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
        catch (OperationCanceledException)
        {
            // 操作被取消（正常行为，不记录错误）
            return false;
        }
        catch (Exception ex)
        {
            // 其他异常才记录错误日志
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
        // 续期间隔：过期时间的1/3，下限100毫秒保证短过期时间也能在过期前续上，上限10秒
        var extendInterval =
            TimeSpan.FromMilliseconds(Math.Max(100, Math.Min(10_000, _expireTime.TotalMilliseconds / 3)));

        while (!cancellationToken.IsCancellationRequested && !IsDisposed)
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
                        NotifyLockLost();
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
                    NotifyLockLost();
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
    /// 续期连续失败达到上限：停止续期并通过 <see cref="LockLostToken"/> 通知业务锁可能已丢失
    /// </summary>
    private void NotifyLockLost()
    {
        _logger.LogError("分布式锁续期连续失败{MaxCount}次，停止续期并通知锁丢失：Key:{LockKey}, Value:{LockValue}",
            MaxExtendFailureCount, _lockKey, _lockValue);

        try
        {
            _lockLostCts.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // 实例已释放，无需再通知
        }
    }

    /// <summary>
    /// 释放锁
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposedFlag, 1) == 1)
        {
            return;
        }

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

        if (_lockTook)
        {
            try
            {
                await _lockDataSourceProvider.ReleaseLockAsync(_lockKey, _lockValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "释放分布式锁崩溃：Key:{LockKey}, Value:{LockValue}", _lockKey, _lockValue);
            }
        }

        _lockLostCts.Dispose();
    }
}
