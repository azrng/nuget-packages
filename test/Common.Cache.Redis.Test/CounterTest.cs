using Azrng.Cache.Core;
using StackExchange.Redis;

namespace Common.Cache.Redis.Test;

/// <summary>
/// 原子计数器真实 Redis 集成测试：验证 INCRBY 端到端路径、并发原子性与非整数值错误。
/// </summary>
public class CounterTest
{
    private readonly ICacheProvider _cacheProvider;

    public CounterTest(ICacheProvider cacheProvider)
    {
        _cacheProvider = cacheProvider;
    }

    [RedisIntegrationFact]
    public async Task IncrementAsync_StartsFromZero_AndAppliesDelta()
    {
        var key = "incr:basic:" + Guid.NewGuid().ToString("N");

        Assert.False(await _cacheProvider.ExistAsync(key));

        var first = await _cacheProvider.IncrementAsync(key);
        var second = await _cacheProvider.IncrementAsync(key, 10);
        var third = await _cacheProvider.DecrementAsync(key, 2);

        Assert.Equal(1, first);
        Assert.Equal(11, second);
        Assert.Equal(9, third);

        // 计数器值可被 GetAsync 读取
        Assert.Equal(9L, await _cacheProvider.GetAsync<long>(key));

        await _cacheProvider.RemoveAsync(key);
        Assert.False(await _cacheProvider.ExistAsync(key));
    }

    [RedisIntegrationFact]
    public async Task IncrementAsync_Concurrent_AreAtomic()
    {
        var key = "incr:concurrent:" + Guid.NewGuid().ToString("N");
        const int workers = 8;
        const int perWorker = 500;

        // 并发自增全部走服务端 INCRBY，结果必须精确等于 workers * perWorker
        var tasks = Enumerable.Range(0, workers).Select(_ => Task.Run(async () =>
        {
            for (var i = 0; i < perWorker; i++)
            {
                await _cacheProvider.IncrementAsync(key);
            }
        }));
        await Task.WhenAll(tasks);

        Assert.Equal((long)workers * perWorker, await _cacheProvider.GetAsync<long>(key));

        await _cacheProvider.RemoveAsync(key);
    }

    [RedisIntegrationFact]
    public async Task IncrementAsync_OnNonIntegerValue_Throws()
    {
        var key = "incr:bad:" + Guid.NewGuid().ToString("N");
        await _cacheProvider.SetAsync(key, "not-a-number");

        // 已有值不是整数时，Redis 服务端返回 ERR，包装为 RedisServerException 抛出
        await Assert.ThrowsAsync<RedisServerException>(() => _cacheProvider.IncrementAsync(key));

        await _cacheProvider.RemoveAsync(key);
    }
}
