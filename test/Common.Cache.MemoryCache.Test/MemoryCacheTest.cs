using Azrng.Cache.MemoryCache;
using Common.Cache.MemoryCache.Test.Model;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Common.Cache.MemoryCache.Test;

public class MemoryCacheTest
{
    [Fact]
    public async Task GetOrCreateAsync_CachesDefaultValue()
    {
        using var context = CreateContext();
        var loadCount = 0;

        var firstResult = await context.Provider.GetOrCreateAsync("counter", () =>
        {
            loadCount++;
            return 0;
        }, TimeSpan.FromMinutes(1));

        var secondResult = await context.Provider.GetOrCreateAsync("counter", () =>
        {
            loadCount++;
            return 1;
        }, TimeSpan.FromMinutes(1));

        Assert.Equal(0, firstResult);
        Assert.Equal(0, secondResult);
        Assert.Equal(1, loadCount);
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenFactoryThrows_Rethrows()
    {
        using var context = CreateContext();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            context.Provider.GetOrCreateAsync<string>("broken",
                () => Task.FromException<string>(new InvalidOperationException("boom"))));
    }

    [Fact]
    public async Task GetOrCreateAsync_PreventsConcurrentFactoryExecution()
    {
        using var context = CreateContext();
        var loadCount = 0;

        var tasks = Enumerable.Range(0, 10).Select(_ =>
            context.Provider.GetOrCreateAsync("shared", async () =>
            {
                var current = Interlocked.Increment(ref loadCount);
                await Task.Delay(100);
                return current;
            }, TimeSpan.FromMinutes(1)));

        var results = await Task.WhenAll(tasks);

        Assert.Equal(1, loadCount);
        Assert.All(results, result => Assert.Equal(1, result));
    }

    [Fact]
    public async Task SetAsync_CachesFalseValue()
    {
        using var context = CreateContext();

        var saved = await context.Provider.SetAsync("bool:false", false, TimeSpan.FromMinutes(1));
        var result = await context.Provider.GetAsync<bool>("bool:false");

        Assert.True(saved);
        Assert.False(result);
    }

    [Fact]
    public async Task SetAsync_NullGenericValue_ReturnsFalse()
    {
        using var context = CreateContext();

        var saved = await context.Provider.SetAsync<UserInfo>("user:null", null!, TimeSpan.FromMinutes(1));
        var exists = await context.Provider.ExistAsync("user:null");

        Assert.False(saved);
        Assert.False(exists);
    }

    [Fact]
    public async Task RemoveAsync_Many_IgnoresInvalidKeysAndReturnsRemovedCount()
    {
        using var context = CreateContext();
        await context.Provider.SetAsync("remove:1", "value");
        await context.Provider.SetAsync("remove:2", "value");

        var removedCount = await context.Provider.RemoveAsync(new[] { "remove:1", "remove:1", "", null!, "missing" });

        Assert.Equal(1, removedCount);
        Assert.False(await context.Provider.ExistAsync("remove:1"));
        Assert.True(await context.Provider.ExistAsync("remove:2"));
    }

    [Fact]
    public async Task RemoveMatchKeyAsync_UsesWildcardPattern()
    {
        using var context = CreateContext();
        await context.Provider.SetAsync("user_1", "v1");
        await context.Provider.SetAsync("user_2", "v2");
        await context.Provider.SetAsync("profile_1", "v3");

        var removed = await context.Provider.RemoveMatchKeyAsync("user_*");

        Assert.True(removed);
        Assert.False(await context.Provider.ExistAsync("user_1"));
        Assert.False(await context.Provider.ExistAsync("user_2"));
        Assert.True(await context.Provider.ExistAsync("profile_1"));
    }

    [Fact]
    public async Task GetAllKeys_TracksProviderManagedEntries()
    {
        using var context = CreateContext();

        await context.Provider.SetAsync("tracked:key", "value");
        await context.Provider.SetAsync("tracked:other", "value");
        Assert.Contains("tracked:key", context.Provider.GetAllKeys());
        Assert.Contains("tracked:other", context.Provider.GetAllKeys());

        await context.Provider.RemoveAsync("tracked:key");

        Assert.DoesNotContain("tracked:key", context.Provider.GetAllKeys());
        Assert.Contains("tracked:other", context.Provider.GetAllKeys());
    }

    [Fact]
    public async Task CacheEmptyCollections_Disabled_DoesNotCacheEmptyList()
    {
        using var context = CreateContext(options =>
        {
            options.CacheEmptyCollections = false;
        });

        var result = await context.Provider.GetOrCreateAsync("empty:list", () => new List<UserInfo>(), TimeSpan.FromMinutes(1));
        var cached = await context.Provider.GetAsync<List<UserInfo>>("empty:list");

        Assert.NotNull(result);
        Assert.Null(cached);
    }

    [Fact]
    public async Task CacheEmptyCollections_Enabled_CachesEmptyList()
    {
        using var context = CreateContext(options =>
        {
            options.CacheEmptyCollections = true;
        });

        var result = await context.Provider.GetOrCreateAsync("empty:list", () => new List<UserInfo>(), TimeSpan.FromMinutes(1));
        var cached = await context.Provider.GetAsync<List<UserInfo>>("empty:list");

        Assert.NotNull(result);
        Assert.NotNull(cached);
        Assert.Empty(cached);
    }

    private static TestContext CreateContext(Action<MemoryConfig>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMemoryCacheStore(options =>
        {
            options.DefaultExpiry = TimeSpan.FromSeconds(5);
            options.CacheEmptyCollections = false;
            configure?.Invoke(options);
        });

        var serviceProvider = services.BuildServiceProvider();
        return new TestContext(serviceProvider, serviceProvider.GetRequiredService<IMemoryCacheProvider>());
    }

    private sealed class TestContext(IServiceProvider serviceProvider, IMemoryCacheProvider provider) : IDisposable
    {
        public IServiceProvider ServiceProvider { get; } = serviceProvider;

        public IMemoryCacheProvider Provider { get; } = provider;

        public void Dispose()
        {
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
