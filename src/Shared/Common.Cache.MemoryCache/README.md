# Common.Cache.MemoryCache

`Common.Cache.MemoryCache` 是一个基于 `Microsoft.Extensions.Caching.Memory` 的轻量封装，提供统一的缓存读写、批量删除和按模式删除能力。

## 功能特性

- 基于 ASP.NET Core `IMemoryCache`
- 支持字符串、对象、集合等多种缓存值
- 支持 `GetOrCreateAsync`
- 支持批量删除和按通配符删除
- 支持获取当前由 Provider 管理的全部缓存键
- 支持 .NET 6.0 / 7.0 / 8.0 / 9.0 / 10.0

## 安装

```powershell
Install-Package Common.Cache.MemoryCache
```

或：

```bash
dotnet add package Common.Cache.MemoryCache
```

## 服务注册

推荐使用 `AddMemoryCacheStore`：

```csharp
services.AddMemoryCacheStore(options =>
{
    options.DefaultExpiry = TimeSpan.FromSeconds(30);
    options.CacheEmptyCollections = false;
});
```

旧方法 `AddMemoryCacheExtension` 已标记为过时，但仍可继续使用：

```csharp
services.AddMemoryCacheExtension(options =>
{
    options.DefaultExpiry = TimeSpan.FromSeconds(30);
    options.CacheEmptyCollections = false;
});
```

## 基本使用

```csharp
public class MyService
{
    private readonly ICacheProvider _cacheProvider;

    public MyService(ICacheProvider cacheProvider)
    {
        _cacheProvider = cacheProvider;
    }

    public async Task<UserInfo> GetUserAsync(string userId)
    {
        return await _cacheProvider.GetOrCreateAsync(
            $"user:{userId}",
            async () => await LoadUserFromDatabase(userId),
            TimeSpan.FromMinutes(10));
    }
}
```

### 读写缓存

```csharp
await _cacheProvider.SetAsync("name", "azrng", TimeSpan.FromMinutes(5));

var name = await _cacheProvider.GetAsync("name");
var count = await _cacheProvider.GetAsync<int>("count");
var user = await _cacheProvider.GetAsync<UserInfo>("user:1");
```

### GetOrCreateAsync

```csharp
var count = await _cacheProvider.GetOrCreateAsync(
    "counter",
    () => 0,
    TimeSpan.FromMinutes(1));
```

```csharp
var user = await _cacheProvider.GetOrCreateAsync(
    "user:1",
    async () => await LoadUserFromDatabase("1"),
    TimeSpan.FromMinutes(10));
```

### 批量操作

```csharp
await _cacheProvider.RemoveAsync(new[] { "user:1", "user:2", "user:3" });

await _cacheProvider.RemoveMatchKeyAsync("user:*");

var allKeys = ((IMemoryCacheProvider)_cacheProvider).GetAllKeys();

var allValues = await ((IMemoryCacheProvider)_cacheProvider).GetAllAsync(allKeys);

await ((IMemoryCacheProvider)_cacheProvider).RemoveAllKeyAsync();
```

## 配置项

`MemoryConfig` 提供以下配置：

- `DefaultExpiry`: 默认过期时间，默认值为 5 秒
- `CacheEmptyCollections`: 是否缓存空集合和空字符串，默认值为 `true`

## 行为说明

### 1. 合法默认值会被正常缓存

从当前实现开始，`0`、`false`、`DateTime.MinValue` 这类合法业务值会被正常缓存，不再被误判为空值。

```csharp
await _cacheProvider.SetAsync("bool:false", false);
var value = await _cacheProvider.GetAsync<bool>("bool:false"); // false
```

### 2. `CacheEmptyCollections` 只控制空集合和空字符串

当 `CacheEmptyCollections = false` 时：

- `null` 不会缓存
- 空字符串不会缓存
- 空集合不会缓存
- `0`、`false` 等合法默认值仍然会缓存

### 3. `GetOrCreateAsync` 失败时会抛出异常

如果缓存读写异常，或者工厂方法本身抛出异常，`GetOrCreateAsync` 会记录日志后继续抛出异常，而不是返回 `default`。

这可以避免把真实故障误判成“缓存未命中”。

### 4. 并发访问同一个 Key 时会做单 Key 同步

同一个 Key 在高并发下首次未命中时，Provider 会按 Key 做同步，避免多个线程同时重复执行工厂方法。

这能减少数据库或远程调用的重复压力。

### 5. `RemoveMatchKeyAsync` 使用通配符语义

`RemoveMatchKeyAsync` 不是正则表达式，而是通配符匹配：

- `*` 匹配任意多个字符
- `?` 匹配任意单个字符
- `[]` 匹配指定字符范围

示例：

```csharp
await _cacheProvider.RemoveMatchKeyAsync("user:*");
await _cacheProvider.RemoveMatchKeyAsync("order:2026-03-??");
```

### 6. `GetAllKeys` 只返回当前 Provider 管理的键

`GetAllKeys` 不再通过反射读取 `MemoryCache` 内部私有字段，而是返回通过当前 Provider 写入并跟踪的键集合。

这意味着：

- 通过 `IMemoryCache` 直接写入的键不会出现在 `GetAllKeys` 中
- 通过 Provider 删除、覆盖、驱逐的键会同步更新跟踪集合
- 实现更稳定，不依赖 .NET 运行时内部结构

## 注意事项

- 不建议使用 `IEnumerable<T>`、`IQueryable<T>`、`IAsyncEnumerable<T>` 作为 `GetOrCreateAsync<T>` 的 `T`
- 如果需要缓存集合，请优先使用 `List<T>` 或数组
- `SetAsync<T>(key, null)` 会返回 `false`，不会写入缓存

## 版本更新记录

* 2.0.0
  * **破坏性更新**：`GetOrCreateAsync` 发生异常时不再吞掉异常并返回 `default`，改为记录日志后继续抛出异常
  * **破坏性更新**：`RemoveMatchKeyAsync` 改为按通配符语义匹配，支持 `*`、`?`、`[]`，不再直接把输入当作正则表达式
  * **破坏性更新**：`GetAllKeys` 改为返回当前 Provider 管理并跟踪的键，不再依赖反射读取 `MemoryCache` 内部私有字段
  * 修复：支持缓存合法默认值，例如 `0`、`false`、`DateTime.MinValue`
  * 修复：`GetOrCreateAsync` 增加单 Key 并发保护，避免并发未命中时重复执行工厂方法
  * 修复：`RemoveAsync(IEnumerable<string>)` 过滤空 key 和重复 key，并返回更准确的删除数量
  * 优化：依赖注入注册改为复用同一个作用域内的 `MemoryCacheProvider` 实例
* 1.3.2
  * 更新异常信息输出
* 1.3.1
  * 更新批量删除缓存的日志输出
* 1.3.0
  * 修复 .NET 8 模糊匹配生效问题
  * 引用 .NET 10 正式包
* 1.3.0-beta9
  * 更新 .NET 10
* 1.3.0-beta8
  * 修复设置不缓存空值时的问题
* 1.3.0-beta7
  * 修复 GetOrCreateAsync 读取不到缓存仍写入的问题
* 1.3.0-beta6
  * 更新命名空间
* 1.3.0-beta5
  * 支持 .NET 9
  * 移除对 netstandard2.1 的支持
  * 更新注入方法 `AddMemoryCacheStore`
* 1.3.0-beta4
  * 修改方法 `KeyDeleteInBatchAsync` 为 `RemoveMatchKeyAsync`
  * 修改方法 `GetAllCacheKeys` 为 `GetAllKeys`
  * 修改方法 `RemoveCacheAllAsync` 为 `RemoveAllKeyAsync`
