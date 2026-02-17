# Azrng.DistributeLock.PostgreSql 架构设计文档

## 项目概述

`Azrng.DistributeLock.PostgreSql` 是一个基于 PostgreSQL 数据库实现的分布式锁库，专为真正的分布式环境设计。该项目遵循提供者模式（Provider Pattern），通过实现 `Azrng.DistributeLock.Core` 中定义的核心接口来提供 PostgreSQL 特定的分布式锁实现。

## 整体架构

### 架构分层

```
┌─────────────────────────────────────────────────────────┐
│                   应用层 (Application)                    │
│           services.AddDbLockProvider(...)               │
└─────────────────────┬───────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────┐
│              接口抽象层 (Core Abstraction)                │
│                 ILockProvider                            │
│           Task<IAsyncDisposable?> LockAsync(...)         │
└─────────────────────┬───────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────┐
│              实现层 (PostgreSql Implementation)            │
│                  DbLockProvider                          │
│                 (单例, Singleton)                        │
└─────────────────────┬───────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────┐
│             数据源提供层 (Data Source Layer)              │
│           DbLockDataSourceProvider                       │
│          实现 ILockDataSourceProvider 接口               │
└─────────────────────┬───────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────┐
│              锁实例层 (Lock Instance)                     │
│                 LockInstance                             │
│           实现 IAsyncDisposable 接口                     │
└─────────────────────┬───────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────┐
│           PostgreSQL 数据库 (Database Layer)              │
│      {schema}.{table} (默认: public.distribute_lock)      │
└─────────────────────────────────────────────────────────┘
```

### 核心组件

#### 1. 配置层 - [DbLockOptions.cs](DbLockOptions.cs)

```csharp
public class DbLockOptions
{
    public string ConnectionString { get; set; }      // 数据库连接字符串
    public string Schema { get; set; }               // 默认: "public"
    public string Table { get; set; }                // 默认: "distribute_lock"
    public TimeSpan DefaultExpireTime { get; set; }  // 默认: 5秒
}
```

**职责**：封装 PostgreSQL 分布式锁的配置选项。

#### 2. 扩展注册层 - [LockProviderExtension.cs](LockProviderExtension.cs)

```csharp
public static class LockProviderExtension
{
    public static IServiceCollection AddDbLockProvider(
        this IServiceCollection services,
        string connectionString,
        string schema = "public",
        string table = "distribute_lock")
    {
        services.AddSingleton<ILockProvider, DbLockProvider>();
        services.AddOptions().Configure<DbLockOptions>(...);
        return services;
    }
}
```

**职责**：提供依赖注入扩展方法，将 `DbLockProvider` 注册为单例服务。

#### 3. 锁提供者层 - [DbLockProvider.cs](DbLockProvider.cs)

```csharp
public class DbLockProvider : ILockProvider
{
    private readonly DbLockDataSourceProvider _dbLockDataSourceProvider;
    private readonly ILogger<DbLockProvider> _logger;
    private readonly DbLockOptions _options;

    public async Task<IAsyncDisposable?> LockAsync(
        string lockKey,
        TimeSpan? expire = null,
        TimeSpan? getLockTimeOut = null,
        bool autoExtend = true)
    {
        var lockValue = Guid.NewGuid().ToString();
        var lockData = new LockInstance(...);
        var flag = await lockData.LockAsync(expire.Value, getLockTimeOut.Value);
        return flag ? lockData : null;
    }
}
```

**职责**：
- 实现 `ILockProvider` 接口
- 初始化数据源提供者
- 创建并返回 `LockInstance` 实例

#### 4. 数据源提供层 - [DbLockDataSourceProvider.cs](DbLockDataSourceProvider.cs)

```csharp
internal class DbLockDataSourceProvider : ILockDataSourceProvider
{
    // 核心方法
    public Task<bool> TakeLockAsync(...)      // 获取锁
    public Task ReleaseLockAsync(...)         // 释放锁
    public Task<bool> ExtendLockAsync(...)    // 续期锁
    public void Init()                        // 初始化数据库表
}
```

**职责**：
- 封装与 PostgreSQL 的所有交互逻辑
- 实现原子性的加锁、解锁和续期操作
- 防止 SQL 注入攻击

#### 5. 锁实例层 - [LockInstance.cs](../Azrng.DistributeLock.Core/LockInstance.cs)

位于 Core 库中，负责：
- 管理单个锁的生命周期
- 实现自动续期机制
- 实现 `IAsyncDisposable` 接口，确保锁的正确释放

## 核心设计原理

### 1. 数据库表结构

```sql
CREATE SCHEMA IF NOT EXISTS {schema};
CREATE TABLE IF NOT EXISTS {schema}.{table} (
    key TEXT NOT NULL CONSTRAINT {table}_pk PRIMARY KEY,
    value TEXT NOT NULL,
    expire_time TIMESTAMP WITHOUT TIME ZONE NOT NULL
);
```

**字段说明**：
- `key`: 锁的唯一标识（主键）
- `value`: 锁持有者标识（GUID），用于验证释放权限
- `expire_time`: 锁过期时间，用于自动清理失效锁

### 2. 加锁机制

#### 核心算法

```
1. 尝试插入锁记录
   INSERT INTO {table} (key, value, expire_time)
   VALUES (@lockKey, @lockValue, @expireTime)
   ON CONFLICT (key) DO NOTHING

2. 如果插入失败（num == 0）
   → 删除过期锁
   → 等待 10ms
   → 重试步骤 1

3. 如果插入成功（num > 0）
   → 加锁成功，返回 true
```

#### 关键实现代码

[DbLockDataSourceProvider.cs:39-79](DbLockDataSourceProvider.cs#L39-L79)

```csharp
public async Task<bool> TakeLockAsync(string lockKey, string lockValue,
    TimeSpan expireTime, TimeSpan getLockTimeOut)
{
    using var tokenSource = new CancellationTokenSource(getLockTimeOut);
    var insertSql = $"INSERT INTO {_schema}.{_table}(...) " +
                    $"ON CONFLICT (key) DO NOTHING;";
    var deleteExpiredSql = $"DELETE FROM {_schema}.{_table} " +
                          $"WHERE expire_time < @currentTime;";

    while (true)
    {
        tokenSource.Token.ThrowIfCancellationRequested();
        var num = await connection.ExecuteAsync(insertSql, ...);

        if (num == 0)
        {
            // 锁已被占用，尝试清理过期锁并重试
            await connection.ExecuteAsync(deleteExpiredSql, ...);
            await Task.Delay(10, cancellationToken);
            continue;
        }

        return true;  // 加锁成功
    }
}
```

**原子性保证**：
- 使用 PostgreSQL 的 `ON CONFLICT DO NOTHING` 语法保证插入的原子性
- 主键约束确保同一时间只有一个客户端能持有特定 key 的锁

### 3. 解锁机制

```
DELETE FROM {table}
WHERE key = @lockKey AND value = @lockValue
```

**关键点**：
- 只删除匹配 `lockKey` 和 `lockValue` 的记录
- 防止误删其他客户端持有的锁
- 防止过期锁被误删

### 4. 续期机制

#### 续期算法

```
续期间隔 = max(1秒, min(10秒, 过期时间 / 3))

循环直到取消：
1. 等待续期间隔
2. 更新过期时间
   UPDATE {table}
   SET expire_time = @newExpireTime
   WHERE key = @lockKey AND value = @lockValue

3. 如果更新成功 → 重置失败计数器
   如果更新失败 → 增加失败计数器
                  失败次数 >= 3 → 停止续期
```

#### 续期任务启动

[LockInstance.cs:146-150](../Azrng.DistributeLock.Core/LockInstance.cs#L146-L150)

```csharp
if (_autoExtendLock)
{
    _cancellationTokenSource = new CancellationTokenSource();
    _autoExtendTask = AutoExtendStart(_cancellationTokenSource.Token);
}
```

#### 续期核心实现

[LockInstance.cs:171-233](../Azrng.DistributeLock.Core/LockInstance.cs#L171-L233)

### 5. 资源释放机制

#### 释放流程

```
DisposeAsync()
    ↓
标记 _isDisposed = true
    ↓
取消自动续期任务
    ↓
等待续期任务完成（最多2秒）
    ↓
调用 ReleaseLockAsync() 释放数据库锁
```

#### 核心代码

[LockInstance.cs:239-291](../Azrng.DistributeLock.Core/LockInstance.cs#L239-L291)

## 安全特性

### 1. SQL 注入防护

**标识符验证** [DbLockDataSourceProvider.cs:152-163](DbLockDataSourceProvider.cs#L152-L163)

```csharp
private static bool IsValidIdentifier(string identifier)
{
    if (string.IsNullOrWhiteSpace(identifier)) return false;
    if (identifier.Length > 63) return false;
    return identifier.All(c => char.IsLetterOrDigit(c) || c == '_')
           && !char.IsDigit(identifier[0]);
}
```

**参数化查询**：
- 所有用户输入都使用 `@parameter` 参数化形式
- schema 和 table 名称在初始化时进行严格验证

### 2. 时区处理

[SystemDateTime.cs](SystemDateTime.cs)

```csharp
public static DateTime Now()
{
    return DateTime.SpecifyKind(
        DateTime.UtcNow.AddHours(8),  // 转换为 UTC+8
        DateTimeKind.Unspecified);
}
```

**设计目的**：
- 统一时间处理，避免时区混乱
- 使用 UTC+8 作为标准时区

## 并发控制

### 1. 重试机制

- **重试间隔**：10ms
- **最大重试时间**：由 `getLockTimeOut` 参数控制（默认5秒）
- **退避策略**：固定间隔重试

### 2. 线程安全

```csharp
private volatile bool _isDisposed;  // volatile 确保多线程可见性
```

## 自动续期详解

### 续期策略

| 锁过期时间 | 续期间隔 |
|-----------|---------|
| 3秒       | 1秒     |
| 9秒       | 3秒     |
| 30秒      | 10秒    |
| 60秒      | 10秒    |

**公式**：`续期间隔 = max(1, min(10, 过期时间 / 3))`

### 失败处理

- **最大连续失败次数**：3次
- **失败后行为**：停止续期，记录错误日志
- **失败恢复**：每次成功续期后重置计数器

## 使用流程图

```
应用程序
   │
   │ services.AddDbLockProvider(...)
   ↓
DbLockProvider (单例)
   │
   │ LockAsync("my_key")
   ↓
LockInstance (创建)
   │
   ├─→ [加锁] ─→ TakeLockAsync()
   │                │
   │                ├─→ 成功 ─→ 启动自动续期任务
   │                │
   │                └─→ 失败/超时 ─→ 返回 null
   │
   ├─→ [使用锁] ─→ 执行临界区代码
   │
   └─→ [释放锁] ─→ DisposeAsync()
                     │
                     ├─→ 取消续期任务
                     ├─→ ReleaseLockAsync()
                     └─→ 返回
```

## 性能考虑

### 优点

1. **强一致性**：基于 ACID 事务保证
2. **持久化**：锁状态持久化到数据库
3. **可靠性**：即使应用程序崩溃，锁也会自动过期

### 缺点

1. **性能限制**：
   - 每次加锁/解锁需要数据库往返
   - 高并发下可能成为瓶颈
   - 续期操作增加数据库负载

2. **网络依赖**：依赖数据库连接稳定性

### 适用场景

✅ **适合**：
- 真正的多机器分布式环境
- 需要持久化锁状态的场景
- 中低并发场景（< 1000 TPS）
- 对一致性要求高于性能的场景

❌ **不适合**：
- 单机多进程环境（建议使用内存锁）
- 超高并发场景（建议使用 Redis）
- 对性能要求极高的场景

## 依赖关系

```
Azrng.DistributeLock.PostgreSql
    ↓ 依赖
Azrng.DistributeLock.Core (接口定义)
    ↓ 依赖
Microsoft.Extensions.DependencyInjection (DI)
Microsoft.Extensions.Logging.Abstractions (日志)
Microsoft.Extensions.Options (选项模式)
Dapper (ORM)
Npgsql (PostgreSQL 驱动)
```

## 多目标框架

项目支持以下 .NET 版本：
- .NET 6.0
- .NET 7.0
- .NET 8.0
- .NET 10.0

## 最佳实践

### 1. 锁粒度

```csharp
// ❌ 过粗的锁粒度
using var lock1 = await _lockProvider.LockAsync("global_lock");

// ✅ 合适的锁粒度
using var lock2 = await _lockProvider.LockAsync($"update_user_{userId}");
```

### 2. 过期时间设置

```csharp
// ❌ 过长的过期时间
using var lock = await _lockProvider.LockAsync("key", expire: TimeSpan.FromMinutes(10));

// ✅ 合理的过期时间 + 自动续期
using var lock = await _lockProvider.LockAsync("key",
    expire: TimeSpan.FromSeconds(5),
    autoExtend: true);
```

### 3. 错误处理

```csharp
using var lockInstance = await _lockProvider.LockAsync("my_key");
if (lockInstance == null)
{
    // 获取锁失败的处理逻辑
    _logger.LogWarning("无法获取锁，可能被其他进程占用");
    return;
}

// 执行临界区代码
```

### 4. 避免死锁

```csharp
// ❌ 可能导致死锁
using var lock1 = await _lockProvider.LockAsync("lock_a");
using var lock2 = await _lockProvider.LockAsync("lock_b");

// ✅ 使用单个锁或按固定顺序获取
using var lock = await _lockProvider.LockAsync("lock_a_b");
```

## 监控与日志

### 日志级别

| 级别 | 场景 | 代码位置 |
|-----|------|---------|
| Error | 初始化失败 | DbLockProvider.cs:29 |
| Error | 获取锁异常 | LockInstance.cs:162 |
| Warning | 续期失败 | LockInstance.cs:192 |
| Error | 释放锁异常 | LockInstance.cs:288 |

## 版本历史

- **0.2.0**: 更新 README.md
- **0.1.1**: 正式版发布，修正版本问题
- **0.1.0**: 支持 .NET 10
- **0.0.1**: 基础版本发布

## 参考资料

- [PostgreSQL INSERT...ON CONFLICT](https://www.postgresql.org/docs/current/sql-insert.html#SQL-ON-CONFLICT)
- [Dapper ORM](https://github.com/DapperLib/Dapper)
- [Npgsql Documentation](https://www.npgsql.org/docs/)