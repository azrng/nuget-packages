using Azrng.Cache.Core;
using Common.Cache.Redis.Test.Model;
using Xunit.Abstractions;

namespace Common.Cache.Redis.Test;

public class CacheTest
{
    private readonly ICacheProvider _cacheProvider;
    private readonly ITestOutputHelper _testOutputHelper;

    public CacheTest(ICacheProvider cacheProvider, ITestOutputHelper testOutputHelper)
    {
        _cacheProvider = cacheProvider;
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// 获取字符然后设置字符串最后删除字符串
    /// </summary>
    [Fact]
    public async Task GetStringAndSet_ReturnOk()
    {
        var key = Guid.NewGuid().ToString("N");
        var result = await _cacheProvider.GetAsync(key);
        Assert.Null(result);

        var value = "result";
        await _cacheProvider.SetAsync(key, value);
        var result2 = await _cacheProvider.GetAsync(key);
        Assert.NotNull(result2);
        Assert.Equal(value, result2);

        await _cacheProvider.RemoveAsync(key);
        var result3 = await _cacheProvider.GetAsync(key);
        Assert.Null(result3);
    }

    /// <summary>
    /// 测试缓存时间
    /// </summary>
    [Fact]
    public async Task Exist_ReturnOk()
    {
        var key = Guid.NewGuid().ToString("N");
        await _cacheProvider.SetAsync(key, "default", TimeSpan.FromSeconds(2));
        var result = await _cacheProvider.ExistAsync(key);
        Assert.True(result);

        await Task.Delay(2 * 2000);

        result = await _cacheProvider.ExistAsync(key);
        Assert.False(result);
    }

    [Fact]
    public async Task Get_Int()
    {
        var key = Guid.NewGuid().ToString("N");

        await _cacheProvider.SetAsync(key, 123456, TimeSpan.FromSeconds(2));
        var result = await _cacheProvider.GetAsync<int>(key);
        Assert.Equal(123456, result);

        await Task.Delay(2 * 2000);

        result = await _cacheProvider.GetAsync<int>(key);
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Get_Object()
    {
        var key = Guid.NewGuid().ToString("N");

        await _cacheProvider.SetAsync(key, new UserInfo("张三", 1), TimeSpan.FromSeconds(2));
        var result = await _cacheProvider.GetAsync<UserInfo>(key);
        Assert.NotNull(result);

        Assert.Equal("张三", result.UserName);

        await Task.Delay(2 * 2000);
        result = await _cacheProvider.GetAsync<UserInfo>(key);
        Assert.Null(result);
    }

    /// <summary>
    /// 测试没有重置过期时间
    /// </summary>
    [Fact]
    public async Task GetOrCreate_NotResetExpiryTime_Test()
    {
        var key = Guid.NewGuid().ToString("N");
        var isExist = await _cacheProvider.ExistAsync(key);
        Assert.False(isExist);

        var result = await _cacheProvider.GetOrCreateAsync(key, () => new UserInfo("王五", 2),
            TimeSpan.FromSeconds(5));
        Assert.NotNull(result);
        Assert.Equal("王五", result.UserName);

        await Task.Delay(2 * 1000);

        var result2 = await _cacheProvider.GetOrCreateAsync(key, () => new UserInfo("王五2", 2),
            TimeSpan.FromSeconds(5));
        Assert.Equal("王五", result2.UserName);

        await Task.Delay(4 * 1000);
        result = await _cacheProvider.GetAsync<UserInfo>(key);
        Assert.Null(result);
    }

    /// <summary>
    /// 测试默认没有 然后存储然后 到点过期
    /// </summary>
    [Fact]
    public async Task GetOrCreate_Async_ObjectAsync()
    {
        var key = Guid.NewGuid().ToString("N");
        var isExist = await _cacheProvider.ExistAsync(key);
        Assert.False(isExist);

        var result = await _cacheProvider.GetOrCreateAsync(key, () => Task.FromResult(new UserInfo("王五", 2)),
            TimeSpan.FromSeconds(2));

        Assert.NotNull(result);

        Assert.Equal("王五", result.UserName);

        await Task.Delay(2 * 2000);
        result = await _cacheProvider.GetAsync<UserInfo>(key);
        Assert.Null(result);
    }

    /// <summary>
    /// 测试获取数据异常
    /// </summary>
    [Fact]
    public async Task GetOrCreate_Async_ExceptionAsync()
    {
        var key = Guid.NewGuid().ToString("N");
        var isExist = await _cacheProvider.ExistAsync(key);
        Assert.False(isExist);

        try
        {
            var result = await _cacheProvider.GetOrCreateAsync(key, async () =>
            {
                await Task.Delay(100);
                string aa = null;
                return aa.ToString();
            }, TimeSpan.FromSeconds(2));
        }
        catch (Exception e)
        {
            _testOutputHelper.WriteLine(e.ToString());
        }
    }

    /// <summary>
    /// 测试获取数据返回null
    /// </summary>
    [Fact]
    public async Task GetOrCreate_Async_NullAsync()
    {
        var key = Guid.NewGuid().ToString("N");
        var isExist = await _cacheProvider.ExistAsync(key);
        Assert.False(isExist);

        var result = await _cacheProvider.GetOrCreateAsync(key, async () =>
            {
                await Task.Delay(100);
                UserInfo aa = null;
                return aa;
            },
            TimeSpan.FromSeconds(2));

        Assert.Null(result);
    }

    /// <summary>
    /// 测试没有重置过期时间
    /// </summary>
    [Fact]
    public async Task GetOrCreate_Async_NotResetExpiryTime_Test()
    {
        var key = Guid.NewGuid().ToString("N");
        var isExist = await _cacheProvider.ExistAsync(key);
        Assert.False(isExist);

        var result = await _cacheProvider.GetOrCreateAsync(key, () => Task.FromResult(new UserInfo("王五", 2)),
            TimeSpan.FromSeconds(5));
        Assert.NotNull(result);
        Assert.Equal("王五", result.UserName);

        await Task.Delay(2 * 1000);

        var result2 = await _cacheProvider.GetOrCreateAsync(key, () => Task.FromResult(new UserInfo("王五2", 2)),
            TimeSpan.FromSeconds(5));
        Assert.Equal("王五", result2.UserName);

        await Task.Delay(4 * 1000);
        result = await _cacheProvider.GetAsync<UserInfo>(key);
        Assert.Null(result);
    }

    [Fact]
    public async Task Remove_Async()
    {
        var key = Guid.NewGuid().ToString("N");
        await _cacheProvider.SetAsync(key, new UserInfo("张三", 1), TimeSpan.FromSeconds(2));

        var result = await _cacheProvider.GetAsync<UserInfo>(key);
        Assert.NotNull(result);

        Assert.Equal("张三", result.UserName);

        await _cacheProvider.RemoveAsync(key);
        result = await _cacheProvider.GetAsync<UserInfo>(key);
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveMany_Async()
    {
        var key1 = Guid.NewGuid().ToString("N");
        await _cacheProvider.SetAsync(key1, new UserInfo("张三", 1), TimeSpan.FromSeconds(2));

        var key2 = Guid.NewGuid().ToString("N");
        await _cacheProvider.SetAsync(key2, new UserInfo("张三", 1), TimeSpan.FromSeconds(2));

        await _cacheProvider.RemoveAsync(new List<string> { key1, key2 });

        var existKey1 = await _cacheProvider.ExistAsync(key1);
        Assert.False(existKey1);

        var existKey2 = await _cacheProvider.ExistAsync(key2);
        Assert.False(existKey2);
    }
}