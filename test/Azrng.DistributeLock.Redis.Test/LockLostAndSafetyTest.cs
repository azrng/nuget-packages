using Azrng.DistributeLock.Core;
using StackExchange.Redis;

namespace Azrng.DistributeLock.Redis.Test;

/// <summary>
/// 锁丢失通知与安全性测试
/// </summary>
public class LockLostAndSafetyTest
{
    private readonly ILockProvider _lockProvider;
    private readonly IDatabase _database;

    public LockLostAndSafetyTest(ILockProvider lockProvider, ConnectionMultiplexer connection)
    {
        _lockProvider = lockProvider;
        _database = connection.GetDatabase();
    }

    /// <summary>
    /// 锁被外部删除后，续期连续失败应触发 LockLostToken 通知
    /// </summary>
    [Fact]
    public async Task LockLostToken_Cancelled_WhenKeyDeletedExternally()
    {
        var lockKey = Guid.NewGuid().ToString();

        var lockInstance = await _lockProvider.LockAsync(lockKey, TimeSpan.FromSeconds(2));
        Assert.NotNull(lockInstance);

        var lost = new TaskCompletionSource();
        await using var registration = lockInstance!.LockLostToken.Register(() => lost.TrySetResult());

        // 模拟锁被外部删除（如误删、故障恢复清理）
        await _database.KeyDeleteAsync(lockKey);

        // 续期间隔约666ms，连续失败3次约2秒后应触发通知
        await Task.WhenAny(lost.Task, Task.Delay(TimeSpan.FromSeconds(10)));
        Assert.True(lost.Task.IsCompleted, "锁被外部删除后应触发锁丢失通知");
        Assert.True(lockInstance.ExtendFailureCount >= 3);

        await lockInstance.DisposeAsync();
    }

    /// <summary>
    /// 锁过期被接管后，旧实例释放不会误删新持有者的锁
    /// </summary>
    [Fact]
    public async Task ExpiredLock_TakenOver_StaleReleaseIsNoop()
    {
        var lockKey = Guid.NewGuid().ToString();

        // 持有者1：1秒过期且不续期，不主动释放
        var lock1 = await _lockProvider.LockAsync(lockKey, TimeSpan.FromSeconds(1), autoExtend: false);
        Assert.NotNull(lock1);

        // 持有者2：等待 Redis TTL 过期后接管
        var lock2 = await _lockProvider.LockAsync(lockKey, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5), autoExtend: false);
        Assert.NotNull(lock2);

        // 旧实例此时释放，不应删掉持有者2的锁
        await lock1!.DisposeAsync();
        Assert.True(await _database.KeyExistsAsync(lockKey), "旧实例释放后新持有者的锁应仍然存在");

        var lock3 = await _lockProvider.LockAsync(lockKey, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(500), autoExtend: false);
        Assert.Null(lock3);

        await lock2!.DisposeAsync();
    }
}
