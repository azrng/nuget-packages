using Azrng.Cache.MemoryCache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using AzrngMemoryCacheOptions = Azrng.Cache.MemoryCache.MemoryCacheOptions;

namespace Common.Cache.MemoryCache.Test;

public class MemoryCacheCounterAndFallbackTest
{
    [Fact]
    public async Task IncrementAsync_CreatesFromZero_AndDecrementWorks()
    {
        var provider = CreateProvider();

        var first = await provider.IncrementAsync("counter:basic");
        var second = await provider.IncrementAsync("counter:basic", 5);
        var third = await provider.DecrementAsync("counter:basic", 2);

        Assert.Equal(1, first);
        Assert.Equal(6, second);
        Assert.Equal(4, third);
    }

    [Fact]
    public async Task IncrementAsync_ConcurrentIncrements_AreAtomic()
    {
        var provider = CreateProvider();

        var tasks = Enumerable.Range(0, 8).Select(_ => Task.Run(async () =>
        {
            for (var i = 0; i < 1000; i++)
            {
                await provider.IncrementAsync("counter:atomic");
            }
        }));
        await Task.WhenAll(tasks);

        Assert.Equal(8000, await provider.GetAsync<long>("counter:atomic"));
    }

    [Fact]
    public async Task IncrementAsync_OnExistingIntegerEntry_MigratesAndContinues()
    {
        var provider = CreateProvider();
        await provider.SetAsync("counter:migrate", 10, System.TimeSpan.FromMinutes(1));

        var result = await provider.IncrementAsync("counter:migrate");

        Assert.Equal(11, result);
        Assert.Equal(11, await provider.GetAsync<int>("counter:migrate"));
    }

    [Fact]
    public async Task IncrementAsync_OnNonIntegerEntry_Throws()
    {
        var provider = CreateProvider();
        await provider.SetAsync("counter:bad", "not-a-number");

        await Assert.ThrowsAsync<System.InvalidOperationException>(() => provider.IncrementAsync("counter:bad"));
    }

    [Fact]
    public async Task Counter_GetAsyncString_ReturnsNumberText()
    {
        var provider = CreateProvider();
        await provider.IncrementAsync("counter:text", 42);

        Assert.Equal("42", await provider.GetAsync("counter:text"));
        Assert.True(await provider.ExistAsync("counter:text"));

        await provider.RemoveAsync("counter:text");
        Assert.False(await provider.ExistAsync("counter:text"));
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenCacheReadFailsAndFailThrowExceptionDisabled_FallsBackToSource()
    {
        var provider = CreateThrowingProvider(failThrowException: false);
        var loadCount = 0;

        var result = await provider.GetOrCreateAsync("broken", () =>
        {
            loadCount++;
            return "real-data";
        });

        // 缓存读失败且配置不抛异常时，应回源取真实数据，而不是返回 default
        Assert.Equal("real-data", result);
        Assert.Equal(1, loadCount);
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenCacheReadFailsAndFailThrowExceptionEnabled_Throws()
    {
        var provider = CreateThrowingProvider(failThrowException: true);

        var thrown = await Assert.ThrowsAnyAsync<System.Exception>(() =>
            provider.GetOrCreateAsync("broken", () => "real-data"));

        Assert.Equal("内存缓存读取失败", thrown.Message);
    }

    private static IMemoryCacheProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMemoryCacheStore(options =>
        {
            options.CacheEmptyCollections = false;
        });

        return services.BuildServiceProvider().GetRequiredService<IMemoryCacheProvider>();
    }

    private static MemoryCacheProvider CreateThrowingProvider(bool failThrowException)
    {
        return new MemoryCacheProvider(
            new ThrowingMemoryCache(),
            Options.Create(new AzrngMemoryCacheOptions { FailThrowException = failThrowException }),
            NullLogger<MemoryCacheProvider>.Instance,
            new MemoryCacheKeyManager());
    }

    private sealed class ThrowingMemoryCache : IMemoryCache
    {
        public ICacheEntry CreateEntry(object key)
        {
            throw new System.InvalidOperationException("create boom");
        }

        public void Dispose()
        {
        }

        public void Remove(object key)
        {
        }

        public bool TryGetValue(object key, out object? value)
        {
            throw new System.InvalidOperationException("read boom");
        }
    }
}
