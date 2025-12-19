# Common.Cache.MemoryCache

这是一个基于 Microsoft.Extensions.Caching.Memory 的内存缓存封装库，提供了更加便捷的缓存操作接口。

## 功能特性

- 基于 ASP.NET Core 内存缓存实现
- 支持多种缓存操作方法
- 支持缓存空值配置
- 支持批量操作
- 支持模糊匹配删除
- 支持获取所有缓存键
- 支持多框架：.NET 6.0 / 7.0 / 8.0 / 9.0 / 10.0

## 安装

通过 NuGet 安装:

```
Install-Package Common.Cache.MemoryCache
```

或通过 .NET CLI:

```
dotnet add package Common.Cache.MemoryCache
```

## 使用方法

### 基本配置

在 Program.cs 中配置服务：

```csharp
// 使用推荐的新方法
services.AddMemoryCacheStore(options =>
{
    options.DefaultExpiry = TimeSpan.FromSeconds(30); // 默认缓存过期时间
    options.CacheEmptyCollections = true; // 是否缓存空集合和空字符串数据
});

// 或使用旧方法（已过时）
services.AddMemoryCacheExtension(options =>
{
    options.DefaultExpiry = TimeSpan.FromSeconds(30); // 默认缓存过期时间
    options.CacheEmptyCollections = true; // 是否缓存空集合和空字符串数据
});
```

### 在服务中使用

注入 [ICacheProvider]() 或 [IMemoryCacheProvider]() 接口并在代码中使用：

```csharp
public class MyService
{
    private readonly ICacheProvider _cacheProvider;

    public MyService(ICacheProvider cacheProvider)
    {
        _cacheProvider = cacheProvider;
    }

    public async Task<string> GetDataAsync(string key)
    {
        // 获取缓存
        var cachedValue = await _cacheProvider.GetAsync<string>(key);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        // 获取实际数据
        var data = await GetDataFromDatabase(key);

        // 设置缓存
        await _cacheProvider.SetAsync(key, data, TimeSpan.FromMinutes(10));

        return data;
    }

    public async Task<T> GetOrCreateDataAsync<T>(string key, Func<Task<T>> factory)
    {
        // 获取或创建缓存
        return await _cacheProvider.GetOrCreateAsync(key, factory, TimeSpan.FromMinutes(10));
    }
}
```

### 批量操作

```csharp
// 批量删除缓存
var keysToDelete = new[] { "key1", "key2", "key3" };
await _cacheProvider.RemoveAsync(keysToDelete);

// 模糊匹配删除缓nawait _cacheProvider.RemoveMatchKeyAsync("user_.*");

// 获取所有缓存键
var allKeys = ((IMemoryCacheProvider)_cacheProvider).GetAllKeys();

// 获取所有缓存值
var allValues = await ((IMemoryCacheProvider)_cacheProvider).GetAllAsync(keys);

// 删除所有缓存
await ((IMemoryCacheProvider)_cacheProvider).RemoveAllKeyAsync();
```

### 配置选项

[MemoryConfig](file:///C:/Work/gitee/nuget-packages/src/Shared/Common.Cache.MemoryCache/MemoryConfig.cs#L6-L19) 类提供了以下配置选项：

- `DefaultExpiry`: 默认缓存过期时间（默认5秒）
- `CacheEmptyCollections`: 是否缓存空集合和空字符串数据（默认为true）

## 版本更新记录

* 1.3.2
  * 更新异常信息输出
* 1.3.1
  * 更新批量删除缓存的日志输出
* 1.3.0
  * 修复.Net8模糊匹配生效问题
  * 引用.Net10正式包
* 1.3.0-beta9
    * 更新.Net10
* 1.3.0-beta8
    * 设置不缓存空值的时候问题修复
* 1.3.0-beta7
    * 修复GetOrCreateAsync读取不到缓存还存储的问题
* 1.3.0-beta6
    * 更新命名空间
* 1.3.0-beta5
    * 支持.Net9
    * 移除对netstandard2.1的支持
    * 更新注入方法AddMemoryCacheStore
* 1.3.0-beta4
    * 修改方法KeyDeleteInBatchAsync为RemoveMatchKeyAsync
    * 修改方法GetAllCacheKeys为GetAllKeys
    * 修改方法RemoveCacheAllAsync改为RemoveAllKeyAsync