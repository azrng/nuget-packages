# Azrng.DistributeLock.Redis

这是一个基于 Redis 实现的分布式锁库，适用于分布式环境。它是 Azrng.DistributeLock.Core 的具体实现之一。

## 功能特性

- 基于 Redis 的分布式锁实现
- 适用于多台机器部署的分布式环境
- 支持锁的自动续期机制
- 支持可配置的锁过期时间和获取锁超时时间
- 使用 Redis 原生的锁机制保证原子性
- 支持多框架：.NET 6.0 / 7.0 / 8.0 / 10.0

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

## 使用方法

### 注册服务

在 Program.cs 中注册服务：

```csharp
// 添加 Redis 分布式锁服务
services.AddRedisLockProvider(
    connectionString: "127.0.0.1:6379,defaultDatabase=1,connectTimeout=100000,syncTimeout=100000,connectRetry=50",
    defaultExpireTime: TimeSpan.FromSeconds(30) // 可选，默认为5秒
);
```

### 在服务中使用

注入 [ILockProvider](ILockProvider)
接口并在代码中使用：

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
        // 获取分布式锁
        using var lockInstance = await _lockProvider.LockAsync("my_lock_key");

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
        using var lockInstance = await _lockProvider.LockAsync(
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
}
```

### 配置选项

[RedisLockOptions](RedisLockOptions.cs)
类提供了以下配置选项：

- `ConnectionString`: Redis 连接字符串
- `DefaultExpireTime`: 默认的锁过期时间，默认为 5 秒

### 参数说明

[LockAsync](ILockProvider) 方法参数：

- `lockKey`: 分布式锁的唯一标识
- `expire`: 锁的过期时间，默认使用配置中的 DefaultExpireTime
- `getLockTimeOut`: 获取锁的等待时间，默认5秒
- `autoExtend`: 是否自动延期，默认开启

## 实现原理

Redis 分布式锁基于 Redis 原生的 `SET lock_key lock_value EX expire_time NX` 命令实现，通过以下方式保证分布式锁的正确性：

1. 使用 Redis 原生的 LockTake/LockRelease 方法实现原子性加锁和解锁
2. 使用 GUID 作为锁的值，确保唯一性和安全性
3. 在获取锁失败时通过轮询机制继续尝试获取锁
4. 锁会在 using 语句结束时通过 LockRelease 方法自动释放

## 适用场景

- 多台机器部署的分布式应用
- 需要跨进程、跨机器协调的场景
- 高性能要求的分布式锁需求
- 生产环境的分布式锁需求

## 注意事项

1. 需要确保 Redis 连接字符串正确配置
2. Redis 集群环境下需要特别注意锁的语义
3. 在网络分区等异常情况下可能会出现锁失效的情况
4. 建议设置合理的过期时间，避免死锁

## 版本更新记录

* 0.2.0
  * 更新README.md
* 0.1.0
    * 支持.Net10
* 0.0.1
    * Init