# Azrng.DistributeLock.Redis

这是一个基于 Redis 实现的分布式锁库，适用于分布式环境。它是 Azrng.DistributeLock.Core 的具体实现之一。

## 功能特性

- 基于 Redis 的分布式锁实现
- 适用于多台机器部署的分布式环境
- 支持锁的自动续期机制
- 支持可配置的锁过期时间和获取锁超时时间
- 使用 Redis 原生的锁机制保证原子性
- 支持多框架：.NET 6.0 / 7.0 / 8.0 / 10.0
- 使用 Lua 脚本确保解锁操作的原子性

## 安装

通过 NuGet 安装:

```
Install-Package Azrng.DistributeLock.Redis
```

或通过 .NET CLI:

```
dotnet add package Azrng.DistributeLock.Redis
```

## 基本要求

1. 分布式系统，一个锁在同一时间只能被一个服务器获取（这是分布式锁的基础）
2. 具备锁失效机制，防止死锁（防止某些意外，锁没有得到方式，别人也无法获取到锁）
3. 不要使用固定的string值作为锁标记着(
   比如设置该redis的值和当前业务相关起来，如果删除的时候，值是该业务的值，再执行删除操作)，而是使用一个不易被猜中的随机值，比如token
4. 不适用del命令释放锁，而是发送script去移除key

> 3、4是为了解决：锁提前过期，客户a还没有执行完命令，然后客户b获取锁去执行，这个时候a执行完然后删除锁的时候将锁着b的锁给删除了


## 分布式锁设计原则

本实现遵循以下分布式锁的设计原则：

1. **互斥性**：任意时刻，只有一个客户端能持有锁
2. **避免死锁**：具备锁失效机制，即使持有锁的客户端崩溃，锁也能被其他客户端获取
3. **安全性**：使用唯一的锁值（GUID）防止误解锁，确保只释放自己持有的锁
4. **原子性**：加锁和解锁操作都是原子性的，避免竞态条件

> **重要**：为什么不使用 `DEL` 命令释放锁？
>
> 使用 Lua 脚本释放锁是为了避免误删其他客户端的锁。考虑以下场景：
> 1. 客户端 A 获取锁，但业务执行时间过长
> 2. 锁过期，客户端 B 获取了同一个锁
> 3. 客户端 A 执行完成，使用 `DEL` 命令删除锁
> 4. 结果：客户端 B 的锁被误删！
>
> 使用 Lua 脚本可以确保只删除锁值匹配的锁，避免这种情况。

## 使用方法

### 注册服务

在 Program.cs 中注册服务：

```csharp
// 添加 Redis 分布式锁服务
services.AddRedisLockProvider(
    connectionString: "127.0.0.1:6379,defaultDatabase=1,connectTimeout=100000,syncTimeout=100000,connectRetry=50",
    defaultExpireTime: TimeSpan.FromSeconds(30) // 可选，默认为30秒
);
```

或使用选项模式：

```csharp
services.Configure<RedisLockOptions>(options =>
{
    options.ConnectionString = "127.0.0.1:6379,defaultDatabase=1";
    options.DefaultExpireTime = TimeSpan.FromSeconds(30);
});

services.AddRedisLockProvider();
```

### 在服务中使用

注入 `ILockProvider` 接口并在代码中使用：

```csharp
public class MyService
{
    private readonly ILockProvider _lockProvider;

    public MyService(ILockProvider lockProvider)
    {
        _lockProvider = lockProvider;
    }

    public async Task DoWorkAsync()
    {
        // 获取分布式锁（使用默认配置）
        await using var lockInstance = await _lockProvider.LockAsync("my_lock_key");

        if (lockInstance != null)
        {
            // 成功获取锁，执行临界区代码
            // 锁会在 using 语句结束时自动释放
            await DoCriticalWorkAsync();
        }
        else
        {
            // 获取锁失败
            Console.WriteLine("无法获取分布式锁");
        }
    }

    public async Task DoWorkWithCustomParamsAsync()
    {
        // 获取分布式锁，设置过期时间和等待时间
        await using var lockInstance = await _lockProvider.LockAsync(
            "my_lock_key",
            expire: TimeSpan.FromSeconds(10),
            getLockTimeOut: TimeSpan.FromSeconds(3),
            autoExtend: true);

        if (lockInstance != null)
        {
            // 成功获取锁，执行临界区代码
            await DoCriticalWorkAsync();
        }
    }

    public async Task DoLongRunningTaskAsync()
    {
        // 长时间运行的任务，启用自动续期
        await using var lockInstance = await _lockProvider.LockAsync(
            "long_running_task",
            expire: TimeSpan.FromSeconds(10),  // 基础过期时间
            getLockTimeOut: TimeSpan.FromSeconds(5),
            autoExtend: true);  // 启用自动续期

        if (lockInstance != null)
        {
            // 任务可能运行很长时间，锁会自动续期
            await DoLongRunningWorkAsync();
        }
    }
}
```

### 配置选项

`RedisLockOptions` 类提供了以下配置选项：

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `ConnectionString` | string | 无 | Redis 连接字符串（必需） |
| `DefaultExpireTime` | TimeSpan | 30秒 | 默认的锁过期时间 |

### LockAsync 方法参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `lockKey` | string | 无 | 分布式锁的唯一标识（必需） |
| `expire` | TimeSpan? | DefaultExpireTime | 锁的过期时间 |
| `getLockTimeOut` | TimeSpan? | 5秒 | 获取锁的等待超时时间 |
| `autoExtend` | bool | true | 是否启用自动续期 |

## 实现原理

Redis 分布式锁基于 Redis 原生的 `SET lock_key lock_value EX expire_time NX` 命令实现：

### 加锁原理

```
SET lock_key unique_token EX expire_time NX
```

- `lock_key`: 锁的键名
- `unique_token`: 使用 GUID 生成的唯一值，防止误解锁
- `EX expire_time`: 设置过期时间，防止死锁
- `NX`: 仅当键不存在时设置，确保互斥性

### 解锁原理

使用 Lua 脚本确保原子性：

```lua
if redis.call("get", KEYS[1]) == ARGV[1] then
    return redis.call("del", KEYS[1])
else
    return 0
end
```

只有当锁的值匹配时才删除，确保不会误删其他客户端的锁。

### 自动续期原理

当启用 `autoExtend: true` 时，后台任务会定期（默认为过期时间的 1/3）续期锁：

1. 续期间隔：`expireTime / 3`，最少 1 秒，最多 10 秒
2. 续期失败计数：连续失败 3 次后停止续期
3. 自动停止：锁被释放或任务完成时自动停止续期

## 适用场景

- ✅ **多台机器部署的分布式应用** - 跨进程、跨机器的互斥访问
- ✅ **高性能要求的分布式锁需求** - Redis 性能优异
- ✅ **生产环境的分布式锁需求** - 经过充分测试和验证
- ✅ **需要自动续期的长时间任务** - 避免任务执行期间锁过期

## 注意事项

1. **Redis 集群环境**：在 Redis Cluster 模式下，确保所有 slot 相关的键都在同一个分片
2. **网络分区**：在网络分区等异常情况下可能会出现锁失效，需要结合业务重试机制
3. **过期时间设置**：建议根据业务执行时间设置合理的过期时间，避免死锁或过早过期
4. **时钟漂移**：Redis 服务器和客户端的时钟差异可能影响锁的过期行为
5. **单点故障**：使用 Redis Sentinel 或 Redis Cluster 提供高可用性

## 与其他实现的对比

| 特性 | Redis 实现 | PostgreSQL 实现 | InMemory 实现 |
|------|-----------|----------------|---------------|
| 适用环境 | 分布式/多机 | 分布式/多机 | 单机/单进程 |
| 性能 | 极高 | 中等 | 极高 |
| 可靠性 | 依赖 Redis | 依赖数据库 | 进程级别 |
| 部署要求 | 需要 Redis | 需要数据库 | 无需额外组件 |
| 推荐场景 | 生产环境 | 有数据库的场景 | 测试/开发 |

## 版本更新记录

### 0.3.0 (最新)
  * ✨ 新增：支持 .NET 9.0
  * ✅ 改进：完善单元测试，添加锁延期、高并发等测试场景
  * ✅ 改进：更新文档，添加分布式锁设计原则说明
  * ✅ 改进：添加实现原理详细说明
  * ✅ 改进：添加自动续期功能说明

### 0.2.0
  * 更新 README.md

### 0.1.0
  * 支持 .NET 10

### 0.0.1
  * 初始版本

## 许可证

版权归 Azrng 所有
