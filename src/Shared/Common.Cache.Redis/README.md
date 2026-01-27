# Common.Cache.Redis

这是一个基于 StackExchange.Redis 的 Redis 缓存封装库，提供了更加便捷的缓存操作接口。

## 功能特性

- 基于 StackExchange.Redis 实现
- 支持多种缓存操作方法
- 支持缓存空值配置
- 支持批量操作
- 支持模糊匹配删除
- 支持 Key 前缀配置
- 支持多框架：.NET 6.0 / 7.0 / 8.0 / 9.0 / 10.0

## 安装

通过 NuGet 安装:

```
Install-Package Common.Cache.Redis
```

或通过 .NET CLI:

```
dotnet add package Common.Cache.Redis
```

## 使用方法

### 基本配置

在 Program.cs 中配置服务：

```csharp
// 使用推荐的新方法
services.AddRedisCacheStore(options =>
{
    options.ConnectionString = "localhost:6379,password=123456,DefaultDatabase=0";
    options.KeyPrefix = "myapp";
    options.CacheEmptyCollections = true; // 是否缓存空集合和空字符串数据
});

// 或使用旧方法（已过时）
services.AddRedisCacheService(options =>
{
    options.ConnectionString = "localhost:6379,password=123456,DefaultDatabase=0";
    options.KeyPrefix = "myapp";
    options.CacheEmptyCollections = true; // 是否缓存空集合和空字符串数据
});
```

### 在服务中使用

注入 [ICacheProvider]() 或 [IRedisProvider]() 接口并在代码中使用：

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

// 模糊匹配删除
await _cacheProvider.RemoveMatchKeyAsync("user_*");
```

### 配置选项

[RedisConfig]() 类提供了以下配置选项：

- `ConnectionString`: Redis 连接字符串
- `KeyPrefix`: Key 前缀
- `InitErrorIntervalSecond`: 初始化错误间隔时间（秒）
- `CacheEmptyCollections`: 是否缓存空集合和空字符串数据（默认为true）

### 模糊匹配规则

模糊匹配支持以下通配符：

- `*` 表示可以匹配多个任意字符
- `?` 表示可以匹配单个任意字符
- `[]` 表示可以匹配指定范围内的字符

例如：
- `user_*` 匹配所有以 "user_" 开头的键
- `user_?` 匹配类似 "user_a", "user_1" 这样的键
- `user_[1-9]` 匹配 user_1 到 user_9 的键

## 版本更新记录

* 1.3.2
  * 更新GetOrCreateAsync方法
* 1.3.1
  * 更新异常信息输出
* 1.3.0
  * 更新正式包
* 1.2.0-beta9
  * 引用.Net10正式包
* 1.2.0-beta8
  * 适配.net10
* 1.2.0-beta7
    * 设置不缓存空值的时候问题修复
* 1.2.0-beta6
    * 增加可设置是否存储空字符串或者空集合选项，默认存储
* 1.2.0-beta5
    * 修复GetOrCreateAsync读取不到缓存还存储redis的问题
* 1.2.0-beta4
    * 支持.Net9
    * 增加扩展方法AddRedisCacheStore
* 1.2.0-beta-3
    * 修改方法KeyDeleteInBatchAsync为RemoveMatchKeyAsync
* 1.2.0-beta2
    * 依赖基类包：Azrng.Cache.Core
    * 优化代码
* 1.2.0-beta1
    * 支持netstandard2.1;net6.0;net7.0;net8.0
    * 将公共的缓存接口定义封装
* 1.1.1
    * 修改redis操作管理类
* 1.1.0
    * 更新版本为5.0
* 1.0.0
    * 3.1版本的redis公共库