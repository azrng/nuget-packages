# Azrng.DistributeLock.PostgreSql

这是一个基于 PostgreSQL 实现的分布式锁库，适用于真正的分布式环境。它是 Azrng.DistributeLock.Core 的具体实现之一。

## 功能特性

- 基于 PostgreSQL 数据库的分布式锁实现
- 适用于多台机器部署的真正分布式环境
- 支持锁的自动续期机制
- 支持可配置的锁过期时间和获取锁超时时间
- 自动创建所需的数据库表和模式
- 支持多框架：.NET 6.0 / 7.0 / 8.0 / 10.0

## 安装

通过 NuGet 安装:

```
Install-Package Azrng.DistributeLock.PostgreSql
```

或通过 .NET CLI:

```
dotnet add package Azrng.DistributeLock.PostgreSql
```

## 使用方法

### 注册服务

在 Program.cs 中注册服务：

```csharp
// 添加 PostgreSQL 分布式锁服务
services.AddDbLockProvider(
    connectionString: "Host=localhost;Port=5432;Database=mydb;Username=user;Password=password",
    schema: "locks",  // 可选，默认为 "public"
    table: "distributed_locks"  // 可选，默认为 "distribute_lock"
);
```

### 在服务中使用

注入 [ILockProvider]() 接口并在代码中使用：

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

[DbLockOptions]() 类提供了以下配置选项：

- `ConnectionString`: PostgreSQL 数据库连接字符串
- `Schema`: 锁表所在的数据库模式，默认为 "public"
- `Table`: 锁表的名称，默认为 "distribute_lock"
- `DefaultExpireTime`: 默认的锁过期时间，默认为 5 秒

### 参数说明

[LockAsync]() 方法参数：

- `lockKey`: 分布式锁的唯一标识
- `expire`: 锁的过期时间，默认使用配置中的 DefaultExpireTime
- `getLockTimeOut`: 获取锁的等待时间，默认5秒
- `autoExtend`: 是否自动延期，默认开启

## 实现原理

PostgreSQL 分布式锁基于数据库表实现，通过以下方式保证分布式锁的正确性：

1. 使用 PostgreSQL 的 `ON CONFLICT DO NOTHING` 语法实现原子性加锁
2. 在获取锁之前清理过期的锁记录
3. 使用数据库事务保证操作的原子性
4. 锁会在 using 语句结束时通过 DELETE 语句自动释放

创建的表结构如下：

```sql
CREATE TABLE distribute_lock (
    key TEXT NOT NULL PRIMARY KEY,
    value TEXT NOT NULL,
    expire_time TIMESTAMP WITHOUT TIME ZONE NOT NULL
);
```

## 适用场景

- 多台机器部署的分布式应用
- 需要跨进程、跨机器协调的场景
- 生产环境的分布式锁需求
- 需要持久化锁状态的场景

## 注意事项

1. 需要确保数据库连接字符串正确配置
2. 确保应用程序有足够的数据库权限创建表和模式
3. 在高并发场景下，性能可能不如 Redis 等内存数据库

## 版本更新记录

* 0.2.0
  * 更新README.md
* 0.1.1
  * pgsql包更新正式版
  * 纠正版本问题
* 0.1.0
  * 支持.Net10
* 0.0.1
  * 基础示例发版