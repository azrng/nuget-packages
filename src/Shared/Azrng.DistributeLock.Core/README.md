# Azrng.DistributeLock.Core

这是一个分布式锁核心库，提供了分布式锁的基础接口和实现，可用于构建基于不同数据存储的分布式锁解决方案。

## 功能特性

- 抽象的分布式锁接口设计
- 支持自动续期机制
- 支持可配置的锁过期时间和获取锁超时时间
- 基于异步 disposable 模式的安全资源释放
- 支持多框架：.NET 6.0 / 7.0 / 8.0 / 10.0
- 内置锁实例管理和异常处理

## 安装

通过 NuGet 安装:

```
Install-Package Azrng.DistributeLock.Core
```

或通过 .NET CLI:

```
dotnet add package Azrng.DistributeLock.Core
```

## 核心概念

### ILockProvider

[ILockProvider]() 是分布式锁的主要接口，提供了 [LockAsync]() 方法用于获取分布式锁。

参数说明：
- `lockKey`: 分布式锁的唯一标识
- `expire`: 锁的过期时间，默认5秒
- `getLockTimeOut`: 获取锁的等待时间，默认5秒
- `autoExtend`: 是否自动延期，默认开启

### ILockDataSourceProvider

[ILockDataSourceProvider]() 是数据源提供程序接口，定义了分布式锁底层操作：
- [TakeLockAsync]() 获取锁
- [ExtendLockAsync]() 延长锁
- [ReleaseLockAsync]() 释放锁

### LockInstance

[LockInstance]() 是锁的实例实现，实现了 `IAsyncDisposable` 接口，通过 using 语句可以自动释放锁资源。

## 使用方法

### 基本使用

```csharp
// 获取分布式锁
using var lockInstance = await _lockProvider.LockAsync("my_lock_key");

if (lockInstance != null)
{
    // 成功获取锁，执行临界区代码
    // 锁会在 using 语句结束时自动释放
    DoCriticalWork();
}
else
{
    // 获取锁失败
    Console.WriteLine("无法获取分布式锁");
}
```

### 带自定义参数的使用

```csharp
// 获取分布式锁，设置过期时间和等待时间
using var lockInstance = await _lockProvider.LockAsync(
    "my_lock_key",
    expire: TimeSpan.FromSeconds(10),
    getLockTimeOut: TimeSpan.FromSeconds(3),
    autoExtend: true);

if (lockInstance != null)
{
    // 成功获取锁，执行临界区代码
    DoCriticalWork();
}
```

## 实现示例

* 内存锁：Azrng.DistributeLock.InMemory
* 数据库分布式锁：Azrng.DistributeLock.Redis
* pg数据库分布式锁：Azrng.DistributeLock.PostgreSql

## 版本更新记录

* 0.2.0
  * 更新LockInstance
* 0.1.1
  * 纠正版本问题
* 0.1.0
  * 支持.Net10
* 0.0.1
  * 基础示例发版