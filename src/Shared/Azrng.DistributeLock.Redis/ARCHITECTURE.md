# Azrng.DistributeLock.Redis 项目架构与原理说明

## 目录

- [项目概述](#项目概述)
- [架构设计](#架构设计)
- [核心组件](#核心组件)
- [实现原理](#实现原理)
- [Redis 分布式锁算法](#redis-分布式锁算法)
- [时序图](#时序图)
- [并发与分布式控制](#并发与分布式控制)
- [网络分区与故障处理](#网络分区与故障处理)
- [适用场景](#适用场景)
- [性能优化](#性能优化)
- [最佳实践](#最佳实践)
- [与其他实现的对比](#与其他实现的对比)

---

## 项目概述

`Azrng.DistributeLock.Redis` 是一个基于 Redis 实现的分布式锁，作为 `Azrng.DistributeLock.Core` 的具体实现之一。它使用 Redis 的原子操作和 Lua 脚本来实现跨进程、跨机器的分布式锁功能。

### 特点

- ✅ **真正的分布式锁** - 支持跨进程、跨机器的互斥访问
- ✅ **高性能** - 基于 Redis 的内存操作，延迟极低
- ✅ **原子性保证** - 使用 Redis 原生命令和 Lua 脚本
- ✅ **锁值验证** - 防止误解锁，确保只释放自己持有的锁
- ✅ **自动过期** - 防止死锁，即使客户端崩溃也能自动释放
- ✅ **自动续期** - 支持长时间任务的锁自动续期
- ⚠️ **依赖 Redis** - 需要稳定的 Redis 服务

---

## 架构设计

### 分层架构

```
┌─────────────────────────────────────────────────────────┐
│                   应用层 (Application)                    │
│              MyService, Controller, Worker               │
└──────────────────────┬──────────────────────────────────┘
                       │ 注入并使用 ILockProvider
                       ↓
┌─────────────────────────────────────────────────────────┐
│                  接口层 (Core Interface)                  │
│                   ILockProvider                          │
│  - LockAsync(lockKey, expire, getLockTimeOut, autoExtend)│
└──────────────────────┬──────────────────────────────────┘
                       │ 实现
         ┌─────────────┴──────────────┐
         ↓                            ↓
┌─────────────────────┐    ┌──────────────────────────────┐
│   RedisLockProvider  │    │     LockInstance (Core)      │
│    (业务逻辑层)      │    │    (锁实例管理)               │
│  - 生成唯一锁值      │    │  - 自动续期逻辑               │
│  - 获取 Redis 连接   │    │  - 释放资源管理               │
│  - 调用 LockInstance │    │  - 续期失败处理               │
└──────────┬───────────┘    └──────────┬───────────────────┘
           │                            │
           │ 创建                        │ 使用
           ↓                            ↓
┌─────────────────────────────────────────────────────────┐
│          RedisLockDataSourceProvider                      │
│                 (数据访问层)                              │
│  - TakeLockAsync()    : 尝试获取锁                        │
│  - ReleaseLockAsync() : 释放锁（Lua 脚本）                │
│  - ExtendLockAsync()  : 续期锁                            │
│  - IsConnected        : 检查连接状态                       │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ↓
┌─────────────────────────────────────────────────────────┐
│              StackExchange.Redis                         │
│                 (Redis 客户端)                            │
│  - IDatabase.LockTakeAsync()    : 原子加锁                │
│  - IDatabase.LockReleaseAsync() : 原子解锁                │
│  - IDatabase.LockExtendAsync()  : 原子续期                │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ↓
┌─────────────────────────────────────────────────────────┐
│                   Redis 服务器                            │
│                  SET NX EX 命令                           │
│                  Lua 脚本执行                            │
└─────────────────────────────────────────────────────────┘
```

### 设计模式

1. **策略模式 (Strategy Pattern)**
   - `ILockProvider` 接口定义锁策略
   - `RedisLockProvider` 提供 Redis 实现策略

2. **外观模式 (Facade Pattern)**
   - `RedisLockProvider` 封装 Redis 操作细节
   - 提供简洁的 `LockAsync()` 接口

3. **单例模式 (Singleton Pattern)**
   - `ConnectionMultiplexer` 注册为 Singleton
   - `RedisLockProvider` 注册为 Singleton
   - 所有请求共享同一个 Redis 连接

4. **选项模式 (Options Pattern)**
   - 使用 `IOptions<RedisLockOptions>` 注入配置
   - 支持配置的集中管理

---

## 核心组件

### 1. RedisLockProvider (业务逻辑层)

**职责**：
- 实现 `ILockProvider` 接口
- 生成唯一的锁值（GUID）
- 管理 Redis 连接
- 协调 `LockInstance` 和数据源提供者

**关键代码**：
```csharp
public class RedisLockProvider : ILockProvider
{
    private readonly ILogger<RedisLockProvider> _logger;
    private readonly RedisLockOptions _options;
    private readonly ConnectionMultiplexer _connection;
    private readonly IDatabase _database;

    public RedisLockProvider(
        IOptions<RedisLockOptions> options,
        ILogger<RedisLockProvider> logger,
        ConnectionMultiplexer connection)
    {
        _options = options.Value;
        _logger = logger;
        _connection = connection;
        _database = _connection.GetDatabase();
    }

    public async Task<IAsyncDisposable?> LockAsync(
        string lockKey,
        TimeSpan? expire = null,
        TimeSpan? getLockTimeOut = null,
        bool autoExtend = true)
    {
        // 1. 生成唯一锁值（防止误解锁）
        var lockValue = Guid.NewGuid().ToString();

        // 2. 设置默认值
        expire ??= _options.DefaultExpireTime;
        getLockTimeOut ??= TimeSpan.FromSeconds(5);

        // 3. 创建数据源提供者
        var dataSource = new RedisLockDataSourceProvider(_database, _connection);

        // 4. 创建锁实例
        var lockData = new LockInstance(dataSource, lockKey, lockValue,
            _logger, autoExtend, expire.Value);

        // 5. 尝试获取锁
        var flag = await lockData.LockAsync(expire.Value, getLockTimeOut.Value);

        return flag ? lockData : null;
    }
}
```

### 2. RedisLockDataSourceProvider (数据访问层)

**职责**：
- 实现 `ILockDataSourceProvider` 接口
- 执行 Redis 原子操作
- 检查连接状态

**关键代码**：
```csharp
internal class RedisLockDataSourceProvider : ILockDataSourceProvider
{
    private readonly IDatabase _database;
    private readonly ConnectionMultiplexer _connection;

    public bool IsConnected => _connection.IsConnected;

    public async Task<bool> TakeLockAsync(
        string lockKey,
        string lockValue,
        TimeSpan expireTime,
        TimeSpan getLockTimeOut)
    {
        // 1. 检查连接状态
        if (!IsConnected)
        {
            throw new InvalidOperationException("Redis连接不可用");
        }

        // 2. 首次尝试获取锁（原子操作）
        var flag = await _database.LockTakeAsync(lockKey, lockValue, expireTime);
        if (flag)
            return true;

        // 3. 使用 CancellationToken 处理超时
        using var tokenSource = new CancellationTokenSource(getLockTimeOut);
        var cancellationToken = tokenSource.Token;

        // 4. 循环重试
        while (true)
        {
            // 尝试获取锁
            flag = await _database.LockTakeAsync(lockKey, lockValue, expireTime);
            if (flag)
                return true;

            // 等待后重试，超时时抛出 TaskCanceledException
            await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);
        }
    }

    public async Task ReleaseLockAsync(string lockKey, string lockValue)
    {
        // 使用 LockReleaseAsync 确保只删除自己的锁
        await _database.LockReleaseAsync(lockKey, lockValue);
    }

    public async Task<bool> ExtendLockAsync(string lockKey, string lockValue, TimeSpan extendTime)
    {
        // 使用 LockExtendAsync 原子性地延长锁的过期时间
        return await _database.LockExtendAsync(lockKey, lockValue, extendTime);
    }
}
```

### 3. LockInstance (Core 层)

**职责**：
- 管理单个锁的生命周期
- 实现自动续期机制
- 实现 `IAsyncDisposable` 接口

**关键功能**：
- 获取锁（调用 `TakeLockAsync`）
- 启动自动续期任务（如果启用）
- 释放锁（调用 `ReleaseLockAsync`）
- 停止自动续期任务
- 记录续期失败次数

### 4. RedisLockOptions (配置)

**职责**：
- 存储 Redis 连接配置
- 存储默认过期时间

**属性**：
```csharp
public class RedisLockOptions
{
    /// <summary>
    /// Redis 连接字符串
    /// 示例：127.0.0.1:6379,defaultDatabase=1,connectTimeout=100000,syncTimeout=100000,connectRetry=50
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// 默认的锁过期时间
    /// </summary>
    public TimeSpan DefaultExpireTime { get; set; }
}
```

---

## 实现原理

### 加锁原理

#### 1. Redis LockTakeAsync 实现

`StackExchange.Redis` 的 `LockTakeAsync` 方法封装了以下 Redis 命令：

```
SET lock_key lock_value EX expire_time NX
```

**参数说明**：
- `lock_key`: 锁的键名
- `lock_value`: 锁的值（使用 GUID，防止误解锁）
- `EX expire_time`: 设置过期时间（秒），防止死锁
- `NX`: 仅当键不存在时设置

**原子性保证**：
- Redis 保证 `SET NX EX` 是原子操作
- 多个客户端同时执行时，只有一个能成功

#### 2. 自旋重试机制

```csharp
while (true)
{
    // 尝试获取锁
    flag = await _database.LockTakeAsync(lockKey, lockValue, expireTime);
    if (flag)
        return true;

    // 短暂等待后重试
    await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);
}
```

**特点**：
- 每次重试间隔 10ms
- 使用 `CancellationToken` 处理超时
- 超时时抛出 `TaskCanceledException`

#### 3. 超时处理

```csharp
using var tokenSource = new CancellationTokenSource(getLockTimeOut);
var cancellationToken = tokenSource.Token;

// Task.Delay 会在超时时抛出 TaskCanceledException
await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);
```

### 解锁原理

#### 1. Redis LockReleaseAsync 实现

`StackExchange.Redis` 的 `LockReleaseAsync` 方法使用 Lua 脚本确保原子性：

```lua
if redis.call("get", KEYS[1]) == ARGV[1] then
    return redis.call("del", KEYS[1])
else
    return 0
end
```

**执行流程**：
1. 获取锁的当前值
2. 比较是否与传入的 `lockValue` 相等
3. 如果相等，删除锁
4. 如果不等，不做任何操作

**原子性保证**：
- Redis 保证 Lua 脚本的原子执行
- 不会被其他命令打断

**安全性保证**：
- 只删除自己持有的锁（通过 `lockValue` 验证）
- 防止误删其他客户端的锁

### 续期原理

#### 1. Redis LockExtendAsync 实现

`StackExchange.Redis` 的 `LockExtendAsync` 方法也使用 Lua 脚本：

```lua
if redis.call("get", KEYS[1]) == ARGV[1] then
    return redis.call("pexpire", KEYS[1], ARGV[2])
else
    return 0
end
```

**执行流程**：
1. 获取锁的当前值
2. 比较是否与传入的 `lockValue` 相等
3. 如果相等，延长过期时间
4. 如果不等，返回失败

#### 2. 自动续期机制

在 `LockInstance` 中实现：

```csharp
private async Task AutoExtendStart(CancellationToken cancellationToken)
{
    // 续期间隔：使用过期时间的 1/3
    var extendInterval = TimeSpan.FromSeconds(
        Math.Max(1, Math.Min(10, _expireTime.TotalSeconds / 3))
    );

    while (!cancellationToken.IsCancellationRequested && !_isDisposed)
    {
        try
        {
            await Task.Delay(extendInterval, cancellationToken);

            // 续期锁
            var extendSuccess = await _lockDataSourceProvider
                .ExtendLockAsync(_lockKey, _lockValue, _expireTime);

            if (extendSuccess)
            {
                _extendFailureCount = 0; // 重置失败计数器
            }
            else
            {
                _extendFailureCount++;

                // 连续失败 3 次后停止续期
                if (_extendFailureCount >= MaxExtendFailureCount)
                {
                    _logger.LogError("分布式锁续期连续失败，停止续期");
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            break; // 正常取消
        }
        catch (Exception ex)
        {
            _extendFailureCount++;
            // 错误处理...
        }
    }
}
```

**续期间隔计算**：
- 基础间隔：`expireTime / 3`
- 最小间隔：1 秒
- 最大间隔：10 秒

**失败处理**：
- 连续失败 3 次后停止续期
- 记录错误日志
- 通知上层应用

---

## Redis 分布式锁算法

### 完整算法流程

```
┌─────────────────────────────────────────────────────────┐
│                  客户端 A                                │
└──────────────────────┬──────────────────────────────────┘
                       │
                       │ 1. SET lock_key uuid_A EX 30 NX
                       ├─────────────────────────────────> Redis
                       │                                   │
                       │                    [lock_key 不存在]
                       │                    [设置成功，返回 true]
                       │<───────────────────────────────────┤
                       │                                   │
                 [获取锁成功]                              │
                       │                                   │
                       │ ... 执行业务逻辑 ...               │
                       │                                   │
                       │ 2. Lua Script:                    │
                       │    if get(lock_key) == uuid_A     │
                       │    then del(lock_key)             │
                       ├─────────────────────────────────> Redis
                       │                                   │
                       │                    [uuid 匹配]
                       │                    [删除成功]
                       │<───────────────────────────────────┤
                       │                                   │
                 [释放锁成功]                              │
                       │                                   │


┌─────────────────────────────────────────────────────────┐
│                  客户端 B                                │
└──────────────────────┬──────────────────────────────────┘
                       │
                       │ 1. SET lock_key uuid_B EX 30 NX
                       ├─────────────────────────────────> Redis
                       │                                   │
                       │                    [lock_key 已存在]
                       │                    [设置失败，返回 false]
                       │<───────────────────────────────────┤
                       │                                   │
                 [获取锁失败]                              │
                       │                                   │
                       │ 2. 等待 10ms                       │
                       │                                   │
                       │ 3. SET lock_key uuid_B EX 30 NX  │
                       ├─────────────────────────────────> Redis
                       │                                   │
                       │                    [lock_key 仍存在]
                       │                    [设置失败，返回 false]
                       │<───────────────────────────────────┤
                       │                                   │
                 [继续重试...]                              │
                       │                                   │
```

### 为什么需要锁值验证？

#### 场景：锁过期导致误解锁

```
时间线：
T0: 客户端 A 获取锁（lock_value = uuid_A, expire = 30s）
T1: 客户端 A 执行业务逻辑...
T2: 30秒过去，锁自动过期
T3: 客户端 B 获取锁（lock_value = uuid_B, expire = 30s）
T4: 客户端 A 执行完成，尝试释放锁
T5: 客户端 A 发送 DEL lock_key（误删了客户端 B 的锁！）
T6: 客户端 C 获取锁（lock_value = uuid_C）
    ↑ 这时客户端 B 还在执行，但锁已经被删了！
```

#### 解决方案：Lua 脚本验证

```lua
if redis.call("get", KEYS[1]) == ARGV[1] then
    return redis.call("del", KEYS[1])
else
    return 0
end
```

**执行流程**：
1. 获取锁的当前值
2. 与传入的 `lockValue` 比较
3. 只有相等时才删除

**效果**：
- 客户端 A 尝试释放锁时，发现锁值是 `uuid_B` 而不是 `uuid_A`
- 不会删除锁
- 客户端 B 的锁得到保护

---

## 时序图

### 获取锁的完整流程

```
应用层        RedisLockProvider      LockInstance    RedisLockDataSourceProvider      Redis
 │                  │                    │                  │                         │
 │ LockAsync()      │                    │                  │                         │
 ├─────────────────>│                    │                  │                         │
 │                  │ 生成 GUID 锁值     │                  │                         │
 │                  │ 创建数据源提供者   │                  │                         │
 │                  │ 创建 LockInstance  │                  │                         │
 │                  ├───────────────────>│                  │                         │
 │                  │                    │ LockAsync()      │                         │
 │                  │                    ├─────────────────>│                         │
 │                  │                    │                  │ LockTakeAsync()          │
 │                  │                    │                  ├────────────────────────>│
 │                  │                    │                  │                         │ SET NX EX
 │                  │                    │                  │                         │ [检查键是否存在]
 │                  │                    │                  │                         │ [不存在: 添加并返回 true]
 │                  │                    │                  │<────────────────────────┤
 │                  │                    │<─────────────────┤                         │
 │                  │                    │                  │                         │
 │                  │                    │ [启用自动续期?]   │                         │
 │                  │                    │ Yes: 启动后台任务│                         │
 │                  │<───────────────────┤                  │                         │
 │<─────────────────┤                    │                  │                         │
 │                  │                    │                  │                         │
 │ [返回 LockInstance]│                  │                  │                         │
 │<─────────────────┤                    │                  │                         │
```

### 释放锁的完整流程

```
应用层        LockInstance       RedisLockDataSourceProvider           Redis
 │                  │                      │                         │
 │ DisposeAsync()   │                      │                         │
 ├─────────────────>│                      │                         │
 │                  │ 停止自动续期任务       │                         │
 │                  │ 等待任务完成          │                         │
 │                  │                      │                         │
 │                  │ ReleaseLockAsync()    │                         │
 │                  ├─────────────────────>│                         │
 │                  │                      │ LockReleaseAsync()       │
 │                  │                      ├────────────────────────>│
 │                  │                      │                         │ Lua Script:
 │                  │                      │                         │ if get(key) == val
 │                  │                      │                         │ then del(key)
 │                  │                      │<────────────────────────┤
 │                  │<─────────────────────┤                         │
 │<─────────────────┤                      │                         │
```

### 自动续期流程

```
应用层        LockInstance       RedisLockDataSourceProvider           Redis
 │                  │                      │                         │
 │ [后台任务]       │                      │                         │
 │                  │ Loop:                 │                         │
 │                  │                      │                         │
 │                  │                      │ ExtendLockAsync()        │
 │                  ├─────────────────────>│                         │
 │                  │                      │ LockExtendAsync()        │
 │                  │                      ├────────────────────────>│
 │                  │                      │                         │ Lua Script:
 │                  │                      │                         │ if get(key) == val
 │                  │                      │                         │ then pexpire(...)
 │                  │                      │<────────────────────────┤
 │                  │                      │                         │
 │                  │ [成功? 重置计数器]    │                         │
 │                  │                      │                         │
 │                  │ [失败? 增加计数器]    │                         │
 │                  │                      │                         │
 │                  │ [计数器 >= 3? 停止]  │                         │
 │                  │                      │                         │
 │                  │ Delay(extendInterval)│                         │
 │                  │                      │                         │
```

---

## 并发与分布式控制

### 竞争条件处理

#### 场景：多个客户端同时获取锁

```
客户端 A              客户端 B              客户端 C              Redis
   │                    │                    │                    │
   │ SET key uuid_A NX  │                    │                    │
   ├────────────────────┼────────────────────┼──────────────────>│
   │                    │                    │                    [原子操作]
   │                    │                    │                    [成功: uuid_A]
   │<───────────────────┴──────────────────┴────────────────────┤
   │                    │                    │                    │
[获取锁成功]            │                    │                    │
   │                    │                    │                    │
   │                    │ SET key uuid_B NX  │                    │
   │                    ├────────────────────┼──────────────────>│
   │                    │                    │                    [失败: 键已存在]
   │                    │<───────────────────┴────────────────────┤
   │                    │                    │                    │
   │               [获取锁失败]             │                    │
   │                    │                    │                    │
   │                    │ SET key uuid_B NX  │                    │
   │                    ├────────────────────┼──────────────────>│
   │                    │                    │                    [失败: 键仍存在]
   │                    │<───────────────────┴────────────────────┤
   │                    │                    │                    │
   │               [继续重试...]            │                    │
```

**关键点**：
- Redis 保证 `SET NX` 的原子性
- 只有一个客户端能成功
- 其他客户端收到失败响应
- 失败的客户端进入重试循环

### 时钟问题

#### 问题：Redis 服务器与客户端时钟不同步

虽然 Redis 服务器负责锁的过期时间计算，但客户端的时钟可能会影响：
- 续期时机
- 超时判断

**解决方案**：
1. **依赖服务器时钟**：过期时间由 Redis 服务器计算
2. **宽松的续期策略**：在过期时间的 1/3 处续期
3. **多次重试**：允许续期失败重试

---

## 网络分区与故障处理

### 网络分区场景

#### 场景 1：客户端与 Redis 分区

```
时间线：
T0: 客户端 A 持有锁
T1: 网络分区，客户端 A 与 Redis 失去连接
T2: 锁自动过期（30秒后）
T3: 客户端 B 获取锁
T4: 客户端 A 恢复连接
    → A 以为还持有锁，但实际上已经丢失
```

**后果**：
- 两个客户端同时认为持有锁
- 可能导致数据不一致

**缓解措施**：
1. **设置合理的过期时间**：大于业务执行时间
2. **使用自动续期**：持续续期直到业务完成
3. **业务层校验**：使用版本号或时间戳检测冲突

#### 场景 2：Redis 主从切换

```
架构：
  客户端 A
     │
     ├─> Redis Master (持有锁)
     │
     └─> Redis Slave (无锁)

T0: 客户端 A 在 Master 获取锁
T1: Master 崩溃
T2: Slave 晋升为 Master
    → 锁信息丢失！
T3: 客户端 B 在新 Master 获取锁成功
    → 两个客户端同时持有锁！
```

**解决方案**：
1. **使用 Redis Cluster**：多主架构，降低单点故障风险
2. **使用 Redlock 算法**：在多个 Redis 实例上获取锁（本实现未使用）
3. **故障检测与快速失败**：检测到分区时立即停止业务

### 故障恢复策略

#### 1. 自动续期失败

```csharp
if (extendSuccess)
{
    _extendFailureCount = 0; // 重置失败计数器
}
else
{
    _extendFailureCount++;

    if (_extendFailureCount >= MaxExtendFailureCount)
    {
        _logger.LogError("分布式锁续期连续失败，停止续期");
        break; // 停止续期，让锁自然过期
    }
}
```

#### 2. 连接断开检测

```csharp
public bool IsConnected => _connection.IsConnected;

public async Task<bool> TakeLockAsync(...)
{
    if (!IsConnected)
    {
        throw new InvalidOperationException("Redis连接不可用");
    }
    // ...
}
```

---

## 适用场景

### ✅ 推荐使用场景

1. **分布式系统**
   - 微服务架构
   - 多服务器部署
   - 跨进程协调

2. **高性能要求**
   - 需要低延迟的锁操作
   - 高并发场景
   - 短时间的临界区保护

3. **需要可靠性**
   - 数据库迁移
   - 定时任务去重
   - 资源分配

4. **长时间任务**
   - 需要自动续期
   - 任务执行时间不确定

### ❌ 不推荐使用场景

1. **单机应用**
   - 使用 `InMemoryLockProvider` 更合适
   - 避免外部依赖

2. **已有数据库**
   - 如果已有 PostgreSQL，考虑使用数据库锁
   - 避免引入新组件

3. **极高可用性要求**
   - Redis 单点故障可能影响业务
   - 考虑使用 Redlock 或数据库锁

---

## 性能优化

### 1. 连接池管理

```csharp
// ConnectionMultiplexer 注册为单例
services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<RedisLockOptions>>();
    var configurationOptions = ConfigurationOptions.Parse(options.Value.ConnectionString);
    return ConnectionMultiplexer.Connect(configurationOptions);
});
```

**优势**：
- 复用 TCP 连接
- 减少连接建立开销
- 提高并发性能

### 2. 减少网络往返

**不推荐**：
```csharp
// 多次网络往返
if (await db.KeyExistsAsync(lockKey))
{
    if (await db.StringGetAsync(lockKey) == lockValue)
    {
        await db.KeyDeleteAsync(lockKey);
    }
}
```

**推荐**：
```csharp
// 单次网络往返（Lua 脚本）
await db.LockReleaseAsync(lockKey, lockValue);
```

### 3. 合理的过期时间

**原则**：
- 过期时间 > 业务执行时间
- 过期时间 < 业务可接受的最大等待时间
- 启用自动续期时，过期时间可以设置较短

**示例**：
```csharp
// 短时间任务 + 自动续期
await _lockProvider.LockAsync("key",
    expire: TimeSpan.FromSeconds(10),   // 较短的过期时间
    autoExtend: true);                   // 启用自动续期

// 长时间任务 + 手动管理
await _lockProvider.LockAsync("key",
    expire: TimeSpan.FromMinutes(5),     // 足够长的过期时间
    autoExtend: false);                  // 禁用自动续期
```

---

## 最佳实践

### 1. 锁键命名规范

```csharp
// 推荐：使用命名空间
var lockKey = "myapp:orders:process:order_12345";

// 推荐：包含业务标识
var lockKey = $"myapp:{resource}:{operation}:{identifier}";

// 不推荐：过于简单
var lockKey = "lock"; // 容易冲突
```

### 2. 设置合理的超时

```csharp
// 获取锁超时：应该大于业务平均等待时间
var getLockTimeOut = TimeSpan.FromSeconds(10);

// 锁过期时间：应该大于业务执行时间
var expire = TimeSpan.FromMinutes(5);
```

### 3. 始终使用 using 语句

```csharp
// 推荐：自动释放锁
await using var lockInstance = await _lockProvider.LockAsync("key");
if (lockInstance != null)
{
    // 执行业务逻辑
}

// 不推荐：手动管理
var lockInstance = await _lockProvider.LockAsync("key");
try
{
    // 执行业务逻辑
}
finally
{
    await lockInstance.DisposeAsync();
}
```

### 4. 处理获取锁失败

```csharp
await using var lockInstance = await _lockProvider.LockAsync("key");
if (lockInstance == null)
{
    // 获取锁失败的处理
    _logger.LogWarning("无法获取锁，可能其他实例正在执行");
    return; // 或者抛出异常，或者重试
}
```

### 5. 监控续期失败

```csharp
// LockInstance 提供了续期失败计数
if (lockInstance.ExtendFailureCount > 0)
{
    _logger.LogWarning($"锁续期失败 {lockInstance.ExtendFailureCount} 次");
}
```

---

## 与其他实现的对比

| 特性 | Redis 实现 | PostgreSQL 实现 | InMemory 实现 |
|------|-----------|----------------|---------------|
| **适用环境** | 分布式/多机 | 分布式/多机 | 单机/单进程 |
| **性能** | 极高（1-10ms） | 中等（10-100ms） | 极高（< 1ms） |
| **可靠性** | 依赖 Redis HA | 依赖数据库 HA | 进程级别 |
| **部署要求** | 需要 Redis | 需要数据库 | 无需额外组件 |
| **网络依赖** | 有 | 有 | 无 |
| **进程隔离** | ✅ | ✅ | ❌ |
| **锁值验证** | ✅ | ✅ | ❌ |
| **自动过期** | ✅ | ✅ | ❌ |
| **自动续期** | ✅ | ✅ | ✅ |
| **故障恢复** | Sentinel/Cluster | 数据库HA | N/A |
| **运维复杂度** | 中等 | 中等 | 低 |
| **推荐场景** | 生产环境 | 有数据库的场景 | 测试/开发 |

### 选择决策树

```
                   ┌─────────────────────────┐
                   │    是否需要分布式锁？      │
                   └──────────┬──────────────┘
                              │
                 ┌────────────┴────────────┐
                 │                         │
                No                        Yes
                 │                         │
                 ↓                         ↓
         ┌──────────────┐        ┌─────────────────────┐
         │  有 Redis？   │        │   已有 PostgreSQL？ │
         └──────┬───────┘        └──────────┬──────────┘
                │                            │
       ┌────────┴────────┐                  │
       │                 │                  │
      Yes               No                 Yes
       │                 │                  │
       ↓                 ↓                  ↓
┌──────────────┐  ┌──────────┐   ┌──────────────┐
│ Redis Lock   │  │ InMemory │   │ PostgreSQL   │
│ (推荐)       │  │ Lock     │   │ Lock         │
└──────────────┘  └──────────┘   └──────────────┘
       │                 │
       │                 │
       No                │
       │                 │
       ↓                 ↓
┌──────────────┐  ┌──────────┐
│ PostgreSQL   │  │ InMemory │
│ Lock         │  │ Lock     │
└──────────────┘  └──────────┘
```

---

## 总结

`Azrng.DistributeLock.Redis` 是一个高性能、可靠的分布式锁实现，适合生产环境的分布式应用。它通过 Redis 的原子操作和 Lua 脚本实现了完整的分布式锁功能，包括锁值验证、自动过期、自动续期等特性。

**核心优势**：
- ✅ 真正的分布式锁
- ✅ 高性能低延迟
- ✅ 完善的原子性保证
- ✅ 自动续期支持

**使用建议**：
- 适合生产环境的分布式应用
- 需要稳定的 Redis 服务
- 建议配合 Redis Sentinel 或 Cluster 使用
- 注意网络分区和故障恢复场景

对于单机应用或测试环境，建议使用 `Azrng.DistributeLock.InMemory`。对于已有数据库的应用，也可以考虑使用 `Azrng.DistributeLock.PostgreSQL`。
