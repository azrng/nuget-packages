using Azrng.Cache.MemoryCache;
using Common.Cache.MemoryCache.Test.Model;

namespace Common.Cache.MemoryCache.Test;

/// <summary>
/// 基础使用测试
/// </summary>
public class MemoryCacheTest
{
    private readonly IMemoryCacheProvider _cacheProvider;

    public MemoryCacheTest(IMemoryCacheProvider memoryCacheProvider)
    {
        _cacheProvider = memoryCacheProvider;

        // 校验是否存在
        _cacheProvider.SetAsync("exist", "default", TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();

        // 设置string
        _cacheProvider.SetAsync("default", "default", TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();

        // 设置int
        _cacheProvider.SetAsync("default_int", 123456, TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();

        // 设置对象
        _cacheProvider.SetAsync<UserInfo>("userinfo", new UserInfo("张三", 1), TimeSpan.FromSeconds(2))
                      .GetAwaiter()
                      .GetResult();
    }

    // 测试方法的命名规则：测试内容的名字+测试的条件

    /// <summary>
    /// 测试key存在不
    /// </summary>
    [Fact]
    public async Task KeyExist_ReturnOk()
    {
        var result = await _cacheProvider.ExistAsync("exist");
        Assert.True(result);

        await Task.Delay(2 * 2000);

        result = await _cacheProvider.ExistAsync("exist");
        Assert.True(!result);
    }

    [Fact]
    public async Task Get_String()
    {
        var result = await _cacheProvider.GetAsync("default");
        Assert.Equal("default", result);

        await Task.Delay(2 * 2000);

        result = await _cacheProvider.GetAsync("default");
        Assert.Null(result);
    }

    [Fact]
    public async Task Get_Int()
    {
        var result = await _cacheProvider.GetAsync<int>("default_int");
        Assert.Equal(123456, result);

        await Task.Delay(2 * 2000);

        result = await _cacheProvider.GetAsync<int>("default_int");
        Assert.True(result == 0);
    }

    [Fact]
    public async Task Get_Object()
    {
        var result = await _cacheProvider.GetAsync<UserInfo>("userinfo");
        Assert.NotNull(result);

        Assert.Equal("张三", result.userName);

        await Task.Delay(2 * 2000);
        result = await _cacheProvider.GetAsync<UserInfo>("userinfo");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrCreate_Object()
    {
        var dateTime = DateTime.Now.ToString();
        var result =
            await _cacheProvider.GetOrCreateAsync<UserInfo>(dateTime, () => new UserInfo("王五", 2),
                TimeSpan.FromSeconds(2));

        Assert.NotNull(result);

        Assert.Equal("王五", result.userName);

        await Task.Delay(2 * 2000);
        result = await _cacheProvider.GetAsync<UserInfo>(dateTime);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrCreate_ObjectAsync()
    {
        var dateTime = DateTime.Now.ToString();
        var result = await _cacheProvider.GetOrCreateAsync<UserInfo>(dateTime, () =>
        {
            return Task.FromResult(new UserInfo("王五", 2));
        }, TimeSpan.FromSeconds(2));

        Assert.NotNull(result);

        Assert.Equal("王五", result.userName);

        await Task.Delay(2 * 2000);
        result = await _cacheProvider.GetAsync<UserInfo>(dateTime);
        Assert.Null(result);
    }

    [Fact]
    public async Task Remove_Async()
    {
        var key = "test_remove";
        await _cacheProvider.SetAsync<UserInfo>(key, new UserInfo("张三", 1), TimeSpan.FromSeconds(2));
        var result = await _cacheProvider.GetAsync<UserInfo>(key);

        Assert.NotNull(result);

        Assert.Equal("张三", result.userName);

        await _cacheProvider.RemoveAsync(key);
        result = await _cacheProvider.GetAsync<UserInfo>(key);
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveMany_Async()
    {
        var key1 = "test_remove1";
        await _cacheProvider.SetAsync<UserInfo>(key1, new UserInfo("张三", 1), TimeSpan.FromSeconds(2));

        var key2 = "test_remove2";
        await _cacheProvider.SetAsync<UserInfo>(key2, new UserInfo("张三", 1), TimeSpan.FromSeconds(2));

        await _cacheProvider.RemoveAsync(new List<string> { key1, key2 });

        var existKey1 = await _cacheProvider.ExistAsync(key1);
        Assert.True(!existKey1);

        var existKey2 = await _cacheProvider.ExistAsync(key2);
        Assert.True(!existKey2);
    }

    /// <summary>
    /// 移除匹配的值
    /// </summary>
    [Fact]
    public async Task RemoveMatchKey_Return()
    {
        await _cacheProvider.RemoveAllKeyAsync();

        var key = "aaaaaaaa:xxxx";
        await _cacheProvider.SetAsync(key, key);

        var key2 = Guid.NewGuid().ToString();
        await _cacheProvider.SetAsync(key2, key2);

        var key3 = "aaaaaaaa:bbbbb";
        await _cacheProvider.SetAsync(key3, key3);

        var keys = _cacheProvider.GetAllKeys();
        Assert.True(keys.Count == 3);

        await _cacheProvider.RemoveMatchKeyAsync("aaaaaaaa:*");

        var keys2 = _cacheProvider.GetAllKeys();
        Assert.True(keys2.Count == 1);
    }

    /// <summary>
    /// 移除所有key
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task RemoveAlKey_ReturnOk()
    {
        var key = Guid.NewGuid().ToString();
        await _cacheProvider.SetAsync(key, key);
        var keys = _cacheProvider.GetAllKeys();
        Assert.True(keys.Count > 0);

        await _cacheProvider.RemoveAllKeyAsync();

        var keys2 = _cacheProvider.GetAllKeys();
        Assert.True(keys2.Count == 0);
    }

    /// <summary>
    /// 将key过期
    /// </summary>
    [Fact]
    public async Task ExpireKey_ReturnOk()
    {
        var key = Guid.NewGuid().ToString();
        await _cacheProvider.SetAsync(key, key, TimeSpan.FromHours(1));

        var exist = await _cacheProvider.ExistAsync(key);
        Assert.True(exist);

        await _cacheProvider.ExpireAsync(key, TimeSpan.FromSeconds(2));

        await Task.Delay(2000);

        var exist2 = await _cacheProvider.ExistAsync(key);
        Assert.True(!exist2);
    }

    /// <summary>
    /// 缓存空集合测试=>默认不缓存空集合
    /// </summary>
    [Fact]
    public async Task CacheEmptyCollections_Test()
    {
        var key = Guid.NewGuid().ToString("N");
        var isExist = await _cacheProvider.ExistAsync(key);
        Assert.False(isExist);

        var result = await _cacheProvider.GetOrCreateAsync(key, () => new List<UserInfo>(),
            TimeSpan.FromSeconds(2));
        Assert.NotNull(result);

        result = await _cacheProvider.GetAsync<List<UserInfo>>(key);
        Assert.Null(result);
    }

    /// <summary>
    /// 缓存空集合测试=>默认不缓存空集合
    /// </summary>
    [Fact]
    public async Task CacheEmptyCollections_Async_Test()
    {
        var key = Guid.NewGuid().ToString("N");
        var isExist = await _cacheProvider.ExistAsync(key);
        Assert.False(isExist);

        var result = await _cacheProvider.GetOrCreateAsync(key, () => Task.FromResult(new List<UserInfo>()),
            TimeSpan.FromSeconds(2));
        Assert.NotNull(result);

        result = await _cacheProvider.GetAsync<List<UserInfo>>(key);
        Assert.Null(result);
    }

    /// <summary>
    /// 缓存字符串测试=>不缓存
    /// </summary>
    [Fact]
    public async Task CacheEmptyString_Test()
    {
        var key = Guid.NewGuid().ToString("N");
        var isExist = await _cacheProvider.ExistAsync(key);
        Assert.False(isExist);

        var result = await _cacheProvider.GetOrCreateAsync(key, () => string.Empty,
            TimeSpan.FromSeconds(2));
        Assert.NotNull(result);

        result = await _cacheProvider.GetAsync<string>(key);
        Assert.Null(result);
    }

    /// <summary>
    /// 缓存字符串测试=>不缓存
    /// </summary>
    [Fact]
    public async Task CacheEmptyString_Async_Test()
    {
        var key = Guid.NewGuid().ToString("N");
        var isExist = await _cacheProvider.ExistAsync(key);
        Assert.False(isExist);

        var result = await _cacheProvider.GetOrCreateAsync(key, () => string.Empty,
            TimeSpan.FromSeconds(2));
        Assert.NotNull(result);

        result = await _cacheProvider.GetAsync<string>(key);
        Assert.Null(result);
    }
}