# Azrng.DistributeLock.InMemory

这是一个基于内存实现的分布式锁库，适用于单机环境或多进程共享内存的场景。它是 Azrng.DistributeLock.Core 的具体实现之一。

## 功能特性

- 基于内存的分布式锁实现
- 适用于单机多进程环境
- 支持锁的自动续期机制
- 支持可配置的锁过期时间和获取锁超时时间
- 基于 ConcurrentDictionary 实现线程安全
- 支持多框架：.NET 6.0 / 7.0 / 8.0 / 10.0

## 安装

通过 NuGet 安装:

```
Install-Package Azrng.DistributeLock.InMemory
```

或通过 .NET CLI:

```
dotnet add package Azrng.DistributeLock.InMemory
```

## 使用方法

### 注册服务

在 Program.cs 中注册服务：

```csharp
// 添加内存分布式锁服务
services.AddInMemory();
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

### 参数说明

`LockAsync` 方法参数：

- `lockKey`: 分布式锁的唯一标识
- `expire`: 锁的过期时间，默认30秒
- `getLockTimeOut`: 获取锁的等待时间，默认5秒
- `autoExtend`: 是否自动延期，默认开启

## 实现原理

内存分布式锁基于 `ConcurrentDictionary` 实现，适用于单台机器上的多进程环境。当多个线程或进程尝试获取同一个锁时：

1. 第一个请求会成功获取锁，键值对会被添加到字典中
2. 后续请求会进入等待状态，直到锁被释放或超时
3. 锁会在 using 语句结束时自动释放，从字典中移除对应键值对

## 适用场景

- 单机部署的应用程序
- 多进程共享同一内存空间的场景
- 测试环境或开发环境
- 不需要跨机器协调的简单场景

## 注意事项

1. 该实现不适用于真正的分布式环境（多台机器）
2. 应用程序重启会导致所有锁丢失
3. 仅适用于同一进程域内的同步

## 版本更新记录

* 0.3.0
  * 更新延期代码
* 0.2.0
  * 更新README.md
* 0.1.1
  * 纠正版本问题
* 0.1.0
  * 支持.Net10
* 0.0.1
  * 基础示例发版