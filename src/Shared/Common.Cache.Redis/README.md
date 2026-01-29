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
- **支持发布/订阅功能**

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

### 发布订阅功能

Common.Cache.Redis 提供了完整的 Redis 发布订阅功能支持，支持频道订阅和模式订阅。

#### 发布消息

```csharp
// 发布简单字符串消息
var subscriberCount = await _redisProvider.PublishAsync("user-channel", "Hello World");

// 发布对象消息（会自动序列化为 JSON）
var user = new UserInfo { Name = "张三", Age = 25 };
await _redisProvider.PublishAsync("user-channel", user);
```

#### 订阅频道

订阅频道会返回一个订阅ID，可以用于后续取消订阅：

```csharp
// 订阅频道，返回订阅ID
var subscriptionId = await _redisProvider.SubscribeAsync<string>("user-channel", message =>
{
    Console.WriteLine($"收到消息: {message}");
});

// 订阅对象消息
await _redisProvider.SubscribeAsync<UserInfo>("user-channel", user =>
{
    Console.WriteLine($"用户: {user.Name}, 年龄: {user.Age}");
});
```

#### 多订阅者支持

同一频道可以有多个订阅者，每个订阅者都会收到消息：

```csharp
// 订阅者1
var subscriptionId1 = await _redisProvider.SubscribeAsync<string>("news-channel", message =>
{
    Console.WriteLine($"订阅者1收到: {message}");
});

// 订阅者2
var subscriptionId2 = await _redisProvider.SubscribeAsync<string>("news-channel", message =>
{
    Console.WriteLine($"订阅者2收到: {message}");
});

// 发布消息，两个订阅者都会收到
await _redisProvider.PublishAsync("news-channel", "Breaking news!");
```

#### 取消订阅

有两种方式取消订阅：

**1. 取消特定订阅者**

```csharp
// 取消指定ID的订阅
await _redisProvider.UnsubscribeAsync("user-channel", subscriptionId);
```

**2. 强制取消频道所有订阅**

```csharp
// 强制取消该频道的所有订阅（紧急情况使用）
await _redisProvider.UnsubscribeAllAsync("user-channel");
```

#### 使用取消令牌自动取消

推荐使用 `CancellationToken` 来管理订阅生命周期：

```csharp
using var cts = new CancellationTokenSource();

// 订阅并传入取消令牌
var subscriptionId = await _redisProvider.SubscribeAsync<string>("user-channel", message =>
{
    Console.WriteLine($"收到消息: {message}");
}, cts.Token);

// 当不再需要订阅时，取消令牌
cts.Cancel(); // 订阅会自动清理
```

#### 模式订阅

支持通配符模式订阅，可以匹配多个频道：

```csharp
// 订阅所有以 "user:" 开头的频道
var subscriptionId = await _redisProvider.SubscribePatternAsync<UserInfo>("user:*", (channel, user) =>
{
    Console.WriteLine($"频道 {channel} 收到用户消息: {user.Name}");
});

// 发布到不同频道
await _redisProvider.PublishAsync("user:1", new UserInfo { Name = "张三" });
await _redisProvider.PublishAsync("user:2", new UserInfo { Name = "李四" });
// 两个消息都会被接收到
```

**模式通配符规则：**
- `*` 匹配多个任意字符
- `?` 匹配单个任意字符
- `[]` 匹配指定范围内的字符

例如：
- `user_*` 匹配所有以 "user_" 开头的频道
- `user:_?` 匹配类似 "user:_a", "user:_1" 的频道
- `user:[1-3]` 匹配 user:_1, user:_2, user:_3

**取消模式订阅：**

```csharp
// 取消指定模式订阅
await _redisProvider.UnsubscribePatternAsync("user:*", subscriptionId);

// 强制取消模式所有订阅
await _redisProvider.UnsubscribePatternAllAsync("user:*");
```

#### 完整示例

```csharp
public class NewsService
{
    private readonly IRedisProvider _redisProvider;

    public NewsService(IRedisProvider redisProvider)
    {
        _redisProvider = redisProvider;
    }

    // 发布新闻
    public async Task PublishNewsAsync(string category, string news)
    {
        var channel = $"news:{category}";
        await _redisProvider.PublishAsync(channel, news);
        Console.WriteLine($"已发布新闻到频道 {channel}");
    }

    // 订阅新闻
    public async Task SubscribeNewsAsync(string category, CancellationToken cancellationToken = default)
    {
        var channel = $"news:{category}";
        var subscriptionId = await _redisProvider.SubscribeAsync<string>(channel, news =>
        {
            Console.WriteLine($"[{category}] 收到新闻: {news}");
            // 处理接收到的新闻...
        }, cancellationToken);

        Console.WriteLine($"已订阅频道 {channel}，订阅ID: {subscriptionId}");
        return subscriptionId;
    }

    // 订阅所有类别的新闻
    public async Task SubscribeAllNewsAsync(CancellationToken cancellationToken = default)
    {
        var pattern = "news:*";
        var subscriptionId = await _redisProvider.SubscribePatternAsync<string>(pattern, (channel, news) =>
        {
            var category = channel.Replace("news:", "");
            Console.WriteLine($"[{category}] 收到新闻: {news}");
        }, cancellationToken);

        Console.WriteLine($"已订阅所有新闻频道，订阅ID: {subscriptionId}");
        return subscriptionId;
    }
}

// 使用示例
var newsService = new NewsService(redisProvider);

// 发布新闻
await newsService.PublishNewsAsync("sports", "今日足球比赛结果");
await newsService.PublishNewsAsync("tech", "新款智能手机发布");

// 订阅体育新闻
using var cts = new CancellationTokenSource();
await newsService.SubscribeNewsAsync("sports", cts.Token);

// 订阅所有新闻
await newsService.SubscribeAllNewsAsync(cts.Token);
```

#### 注意事项

1. **订阅者数量**：`PublishAsync` 返回的订阅者数量是 Redis 服务器看到的连接数，可能与本地订阅者数量不同
2. **多订阅者**：同一频道的多个订阅者共享同一个 Redis 连接，但都能独立接收消息
3. **取消清理**：当最后一个订阅者取消后，Redis 订阅会自动清理
4. **线程安全**：订阅和取消操作都是线程安全的
5. **异常处理**：订阅回调中的异常会被捕获并记录，不会影响其他订阅者

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

* 1.4.0
  * **新增**：发布订阅功能支持
    * 支持频道订阅和模式订阅
    * 支持多订阅者管理
    * 支持通过订阅ID精确取消订阅
    * 支持强制取消所有订阅
  * 优化：修复 GetOrCreateAsync 重复调用 GetKey 的问题
  * 优化：改进 GetKey 方法的前缀检查逻辑
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