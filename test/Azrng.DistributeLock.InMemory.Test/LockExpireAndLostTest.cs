using Azrng.DistributeLock.Core;
using Microsoft.Extensions.Logging;

namespace Azrng.DistributeLock.InMemory.Test;

/// <summary>
/// 过期语义与锁丢失通知测试
/// </summary>
public class LockExpireAndLostTest
{
    private readonly ILockProvider _lockProvider;
    private readonly ILogger<LockExpireAndLostTest> _logger;

    public LockExpireAndLostTest(ILockProvider lockProvider, ILogger<LockExpireAndLostTest> logger)
    {
        _lockProvider = lockProvider;
        _logger = logger;
    }

    /// <summary>
    /// 锁过期后可被其他持有者接管，且旧实例释放不会误删新持有者的锁
    /// </summary>
    [Fact]
    public async Task ExpiredLock_CanBeTakenOver_And_StaleReleaseIsNoop()
    {
        var lockKey = Guid.NewGuid().ToString();

        // 持有者1：1秒过期且不续期，不主动释放
        var lock1 = await _lockProvider.LockAsync(lockKey, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(200), autoExtend: false);
        Assert.NotNull(lock1);

        // 持有者2：等待过期后应能接管
        var lock2 = await _lockProvider.LockAsync(lockKey, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(3), autoExtend: false);
        Assert.NotNull(lock2);

        // 旧实例此时释放，不应删掉持有者2的锁
        await lock1!.DisposeAsync();
        var lock3 = await _lockProvider.LockAsync(lockKey, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(300), autoExtend: false);
        Assert.Null(lock3);

        // 持有者2正常释放后可再次获取
        await lock2!.DisposeAsync();
        await using var lock4 = await _lockProvider.LockAsync(lockKey, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(300), autoExtend: false);
        Assert.NotNull(lock4);
    }

    /// <summary>
    /// 锁未过期时其他请求获取失败
    /// </summary>
    [Fact]
    public async Task HoldingLock_BlocksOthers_UntilExpire()
    {
        var lockKey = Guid.NewGuid().ToString();

        await using var lock1 = await _lockProvider.LockAsync(lockKey, TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(200), autoExtend: false);
        Assert.NotNull(lock1);

        var lock2 = await _lockProvider.LockAsync(lockKey, TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(500), autoExtend: false);
        Assert.Null(lock2);
    }

    /// <summary>
    /// 自动续期开启时，短过期时间的锁也能被持续续住
    /// </summary>
    [Fact]
    public async Task AutoExtend_KeepsShortExpireLockAlive()
    {
        var lockKey = Guid.NewGuid().ToString();

        await using var lock1 = await _lockProvider.LockAsync(lockKey, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(200));
        Assert.NotNull(lock1);

        // 超过原始过期时间后锁仍被续期持有，其他请求获取失败
        await Task.Delay(TimeSpan.FromSeconds(2));
        var lock2 = await _lockProvider.LockAsync(lockKey, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(300), autoExtend: false);
        Assert.Null(lock2);
    }

    /// <summary>
    /// 续期连续失败达到上限后，LockLostToken 被取消通知业务锁已丢失
    /// </summary>
    [Fact]
    public async Task LockLostToken_Cancelled_WhenExtendKeepsFailing()
    {
        var instance = new LockInstance(new AlwaysFailExtendProvider(), "lost-key", Guid.NewGuid().ToString(),
            _logger, autoExtend: true, TimeSpan.FromSeconds(1));
        Assert.True(await instance.LockAsync(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)));

        var lost = new TaskCompletionSource();
        await using var registration = instance.LockLostToken.Register(() => lost.TrySetResult());

        // 续期间隔约333ms，连续失败3次约1秒后应触发通知
        await Task.WhenAny(lost.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        Assert.True(lost.Task.IsCompleted);
        Assert.Equal(3, instance.ExtendFailureCount);

        await instance.DisposeAsync();
        Assert.True(instance.IsDisposed);
    }

    /// <summary>
    /// 非法的过期时间参数直接抛出异常
    /// </summary>
    [Fact]
    public async Task InvalidExpire_Throws()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _lockProvider.LockAsync(Guid.NewGuid().ToString(), TimeSpan.Zero));
    }

    /// <summary>
    /// 续期恒失败的假数据源，用于验证锁丢失通知
    /// </summary>
    private sealed class AlwaysFailExtendProvider : ILockDataSourceProvider
    {
        public Task<bool> TakeLockAsync(string lockKey, string lockValue, TimeSpan expireTime, TimeSpan getLockTimeOut)
            => Task.FromResult(true);

        public Task<bool> ExtendLockAsync(string lockKey, string lockValue, TimeSpan extendTime)
            => Task.FromResult(false);

        public Task ReleaseLockAsync(string lockKey, string lockValue)
            => Task.CompletedTask;
    }
}
