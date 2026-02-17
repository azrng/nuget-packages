# Azrng.DistributeLock.InMemory 项目架构与原理说明

## 目录

- [项目概述](#项目概述)
- [架构设计](#架构设计)
- [核心组件](#核心组件)
- [实现原理](#实现原理)
- [时序图](#时序图)
- [并发控制](#并发控制)
- [线程安全](#线程安全)
- [适用场景](#适用场景)
- [限制与注意事项](#限制与注意事项)
- [与其他实现的对比](#与其他实现的对比)

---

## 项目概述

`Azrng.DistributeLock.InMemory` 是一个基于内存的分布式锁实现，作为 `Azrng.DistributeLock.Core` 的具体实现之一。它使用 `ConcurrentDictionary` 来管理锁状态，适用于单机多线程或多进程共享内存的场景。

### 特点

- ✅ **零外部依赖** - 不需要 Redis、数据库等外部组件
- ✅ **高性能** - 内存操作，极低延迟
- ✅ **线程安全** - 使用 `ConcurrentDictionary` 保证线程安全
- ✅ **简单易用** - 开箱即用，无需配置
- ⚠️ **进程隔离** - 仅适用于同一进程内的线程同步

---

## 架构设计

### 分层架构

```
┌─────────────────────────────────────────────────────────┐
│                   应用层 (Application)                    │
│              MyService, HomeController, etc.            │
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
│  InMemoryLockProvider│    │     LockInstance (Core)      │
│   (业务逻辑层)       │    │    (锁实例管理)               │
│  - 生成唯一锁值      │    │  - 自动续期逻辑               │
│  - 调用 LockInstance │    │  - 释放资源管理               │
└──────────┬───────────┘    └──────────┬───────────────────┘
           │                            │
           │ 创建                        │ 使用
           ↓                            ↓
┌─────────────────────────────────────────────────────────┐
│              InMemoryLockDataSourceProvider               │
│                 (数据访问层)                              │
│  - TakeLockAsync()    : 尝试获取锁                        │
│  - ReleaseLockAsync() : 释放锁                            │
│  - ExtendLockAsync()  : 续期锁                            │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ↓
┌─────────────────────────────────────────────────────────┐
│              ConcurrentDictionary<string, string>         │
│                   (存储层)                                │
│         锁键 (lockKey) → 锁值 (lockValue)                │
└─────────────────────────────────────────────────────────┘
```

### 设计模式

1. **策略模式 (Strategy Pattern)**
   - `ILockProvider` 接口定义锁策略
   - `InMemoryLockProvider` 提供内存实现策略

2. **外观模式 (Facade Pattern)**
   - `InMemoryLockProvider` 封装复杂的锁操作
   - 提供简洁的 `LockAsync()` 接口

3. **单例模式 (Singleton Pattern)**
   - `InMemoryLockProvider` 注册为 Singleton 服务
   - 所有请求共享同一个 `ConcurrentDictionary` 实例

---

## 核心组件

### 1. InMemoryLockProvider (业务逻辑层)

**职责**：
- 实现 `ILockProvider` 接口
- 生成唯一的锁值（GUID）
- 协调 `LockInstance` 和数据源提供者

**关键代码**：
```csharp
public class InMemoryLockProvider : ILockProvider
{
    private readonly ILogger<InMemoryLockProvider> _logger;
    private readonly InMemoryLockDataSourceProvider _inMemoryLockDataSourceProvider;

    public InMemoryLockProvider(ILogger<InMemoryLockProvider> logger)
    {
        _logger = logger;
        _inMemoryLockDataSourceProvider = new InMemoryLockDataSourceProvider();
    }

    public async Task<IAsyncDisposable?> LockAsync(string lockKey, TimeSpan? expire,
        TimeSpan? getLockTimeOut, bool autoExtend)
    {
        // 1. 生成唯一锁值
        var lockValue = Guid.NewGuid().ToString();

        // 2. 设置默认值
        expire ??= TimeSpan.FromSeconds(30);
        getLockTimeOut ??= TimeSpan.FromSeconds(5);

        // 3. 创建锁实例
        var lockData = new LockInstance(_inMemoryLockDataSourceProvider,
            lockKey, lockValue, _logger, autoExtend, expire.Value);

        // 4. 尝试获取锁
        var flag = await lockData.LockAsync(expire.Value, getLockTimeOut.Value);

        return flag ? lockData : null;
    }
}
```

### 2. InMemoryLockDataSourceProvider (数据访问层)

**职责**：
- 实现 `ILockDataSourceProvider` 接口
- 管理 `ConcurrentDictionary` 存储
- 提供原子性的锁操作

**关键代码**：
```csharp
internal class InMemoryLockDataSourceProvider : ILockDataSourceProvider
{
    // 使用 ConcurrentDictionary 保证线程安全
    private readonly ConcurrentDictionary<string, string> _lockCounts =
        new ConcurrentDictionary<string, string>();

    // 尝试获取锁
    public async Task<bool> TakeLockAsync(string lockKey, string lockValue,
        TimeSpan expireTime, TimeSpan getLockTimeOut)
    {
        // 1. 首次尝试获取锁
        var flag = _lockCounts.TryAdd(lockKey, lockValue);
        if (flag)
            return true;

        // 2. 使用 CancellationToken 处理超时
        using var tokenSource = new CancellationTokenSource(getLockTimeOut);
        var cancellationToken = tokenSource.Token;

        // 3. 循环重试
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // 尝试添加锁（原子操作）
            flag = _lockCounts.TryAdd(lockKey, lockValue);
            if (flag)
                break;

            // 等待后重试
            await Task.Delay(10, cancellationToken);
        }

        return flag;
    }

    // 释放锁
    public Task ReleaseLockAsync(string lockKey, string lockValue)
    {
        _lockCounts.TryRemove(lockKey, out var _);
        return Task.CompletedTask;
    }

    // 续期锁（内存实现中总是返回 true）
    public Task<bool> ExtendLockAsync(string lockKey, string lockValue, TimeSpan extendTime)
    {
        // 内存锁不会过期，所以续期总是成功
        return Task.FromResult(true);
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

---

## 实现原理

### 加锁原理

#### 1. 原子性保证

使用 `ConcurrentDictionary.TryAdd()` 方法保证加锁的原子性：

```csharp
// TryAdd 是原子操作：
// - 如果 lockKey 不存在，添加键值对并返回 true
// - 如果 lockKey 已存在，不做任何操作并返回 false
var flag = _lockCounts.TryAdd(lockKey, lockValue);
```

#### 2. 自旋重试机制

当首次尝试失败时，进入重试循环：

```csharp
while (true)
{
    // 检查超时
    cancellationToken.ThrowIfCancellationRequested();

    // 再次尝试获取锁
    flag = _lockCounts.TryAdd(lockKey, lockValue);
    if (flag)
        break;

    // 短暂等待后重试（避免 CPU 空转）
    await Task.Delay(10, cancellationToken);
}
```

#### 3. 超时处理

使用 `CancellationTokenSource` 处理超时：

```csharp
using var tokenSource = new CancellationTokenSource(getLockTimeOut);
var cancellationToken = tokenSource.Token;

// 超时时自动抛出 OperationCanceledException
await Task.Delay(10, cancellationToken);
```

### 解锁原理

使用 `ConcurrentDictionary.TryRemove()` 方法安全移除锁：

```csharp
// TryRemove 是原子操作：
// - 如果 lockKey 存在，移除并返回 true
// - 如果 lockKey 不存在，返回 false
_lockCounts.TryRemove(lockKey, out var _);
```

**注意**：当前实现不验证 `lockValue`，这是与 Redis/PostgreSQL 实现的一个差异。

### 续期原理

内存锁实现中，`ExtendLockAsync` 总是返回 `true`：

```csharp
public Task<bool> ExtendLockAsync(string lockKey, string lockValue, TimeSpan extendTime)
{
    // 内存锁存储在进程内存中，不会自动过期
    // 因此续期操作总是成功
    return Task.FromResult(true);
}
```

**原因**：
- `ConcurrentDictionary` 中的条目不会自动过期
- 锁的生命周期由 `LockInstance` 管理
- 只有显式调用 `DisposeAsync()` 才会释放锁

---

## 时序图

### 获取锁的完整流程

```
应用层        InMemoryLockProvider      LockInstance         InMemoryLockDataSourceProvider    ConcurrentDictionary
 │                    │                     │                        │                              │
 │ LockAsync()        │                     │                        │                              │
 ├───────────────────>│                     │                        │                              │
 │                    │ 生成 GUID 锁值      │                        │                              │
 │                    │ 创建 LockInstance   │                        │                              │
 │                    ├────────────────────>│                        │                              │
 │                    │                     │ LockAsync()            │                              │
 │                    │                     ├───────────────────────>│                              │
 │                    │                     │                        │ TryAdd(lockKey, lockValue)   │
 │                    │                     │                        ├─────────────────────────────>│
 │                    │                     │                        │                              │ [检查键是否存在]
 │                    │                     │                        │                              │ [不存在: 添加并返回 true]
 │                    │                     │                        │<─────────────────────────────┤ (true)
 │                    │                     │<───────────────────────┤                              │
 │                    │                     │                        │                              │
 │                    │                     │ [启用自动续期?]         │                              │
 │                    │                     │ Yes: 启动后台任务      │                              │
 │                    │<────────────────────┤                        │                              │
 │<───────────────────┤                     │                        │                              │
 │                    │                     │                        │                              │
 │ [返回 LockInstance]│                     │                        │                              │
 │<───────────────────┤                     │                        │                              │
```

### 释放锁的完整流程

```
应用层        LockInstance           InMemoryLockDataSourceProvider    ConcurrentDictionary
 │                  │                         │                              │
 │ DisposeAsync()   │                         │                              │
 ├─────────────────>│                         │                              │
 │                  │ 停止自动续期任务         │                              │
 │                  │ 等待任务完成            │                              │
 │                  │                         │                              │
 │                  │ ReleaseLockAsync()      │                              │
 │                  ├────────────────────────>│                              │
 │                  │                         │ TryRemove(lockKey)          │
 │                  │                         ├─────────────────────────────>│
 │                  │                         │                              │ [移除键值对]
 │                  │                         │<─────────────────────────────┤
 │                  │<────────────────────────┤                              │
 │<─────────────────┤                         │                              │
 │                  │                         │                              │
```

---

## 并发控制

### 线程安全保证

`ConcurrentDictionary` 提供以下线程安全保证：

1. **TryAdd()**
   - 原子操作：检查键是否存在 + 添加键值对
   - 多线程同时调用时，只有一个会成功

2. **TryRemove()**
   - 原子操作：检查键是否存在 + 移除键值对
   - 多线程同时调用时，只有一个会成功

3. **内部细粒度锁**
   - `ConcurrentDictionary` 使用分段锁机制
   - 不同的键可以并发操作
   - 同一个键的操作被序列化

### 并发场景分析

#### 场景 1：多线程同时获取同一个锁

```
线程1                    线程2                    ConcurrentDictionary
 │                       │                              │
 │ TryAdd("key", "v1")   │                              │
 ├──────────────────────┼─────────────────────────────>│
 │                       │                              │ [成功添加，返回 true]
 │<──────────────────────┼─────────────────────────────┤
 │ (获取锁成功)          │                              │
 │                       │ TryAdd("key", "v2")         │
 │                       ├─────────────────────────────>│
 │                       │                              │ [键已存在，返回 false]
 │                       │<─────────────────────────────┤
 │                       │ (获取锁失败)                 │
 │                       │                              │
 │                       │ [进入重试循环]               │
 │                       │ TryAdd("key", "v2")         │
 │                       ├─────────────────────────────>│
 │                       │                              │ [键仍存在]
 │                       │<─────────────────────────────┤
 │                       │ ... (继续重试)               │
 │                       │                              │
 │ TryRemove("key")      │                              │
 ├──────────────────────┼─────────────────────────────>│
 │                       │                              │ [移除成功]
 │<──────────────────────┼─────────────────────────────┤
 │ (锁释放)             │                              │
 │                       │ TryAdd("key", "v2")         │
 │                       ├─────────────────────────────>│
 │                       │                              │ [成功添加，返回 true]
 │                       │<─────────────────────────────┤
 │                       │ (获取锁成功)                 │
```

#### 场景 2：多线程同时获取不同的锁

```
线程1                    线程2                    ConcurrentDictionary
 │                       │                              │
 │ TryAdd("key1", "v1")  │ TryAdd("key2", "v2")         │
 ├──────────────────────┼─────────────────────────────>│
 │                       ├─────────────────────────────>│
 │<──────────────────────┼─────────────────────────────┤
 │                       │<─────────────────────────────┤
 │ (同时成功)            │ (同时成功)                   │
```

**优势**：不同键的操作可以并发执行，互不影响。

---

## 线程安全

### 1. ConcurrentDictionary 的线程安全

`ConcurrentDictionary` 是 .NET 提供的线程安全字典，特点：

- **细粒度锁**：使用分段锁，而非全局锁
- **无锁读取**：读取操作通常不需要加锁
- **原子操作**：`TryAdd`、`TryRemove`、`TryUpdate` 等方法都是原子的

### 2. 异步操作的线程安全

- `Task.Delay()` 使用 `CancellationToken` 确保可取消
- `LockInstance` 使用 `volatile bool _isDisposed` 标记释放状态
- 自动续期任务使用 `CancellationTokenSource` 控制生命周期

### 3. 潜在的竞态条件

#### 问题：解锁时不验证 lockValue

当前实现：
```csharp
public Task ReleaseLockAsync(string lockKey, string lockValue)
{
    _lockCounts.TryRemove(lockKey, out var _);
    return Task.CompletedTask;
}
```

**潜在风险**：
- 如果一个线程持有锁 A，但另一个线程误调用了 `ReleaseLockAsync("A")`
- 持有锁的线程会失去锁保护

**建议**：
虽然当前实现不验证 `lockValue`，但在使用时应确保：
- 只在 `using` 块结束时自动释放
- 不要在多个线程间传递 `LockInstance`
- 避免手动调用 `DisposeAsync()`

---

## 适用场景

### ✅ 推荐使用场景

1. **单机多线程应用**
   - ASP.NET Core 应用（单实例部署）
   - 后台服务（单实例部署）
   - 控制台应用（多线程环境）

2. **开发测试环境**
   - 单元测试
   - 集成测试
   - 开发调试

3. **不需要持久化的锁**
   - 应用重启后锁可以丢失
   - 不需要跨进程同步
   - 不需要跨机器同步

### ❌ 不推荐使用场景

1. **多进程环境**
   - 多个独立的进程需要同步
   - 进程间无法共享内存

2. **分布式环境**
   - 多台服务器部署
   - 需要跨机器同步

3. **需要持久化的锁**
   - 应用重启后锁仍然有效
   - 需要故障恢复

---

## 限制与注意事项

### 1. 进程隔离

**限制**：锁只在当前进程内有效

**示例**：
```csharp
// 进程 A
using var lock1 = await lockProvider.LockAsync("my_lock");
// 进程 A 持有锁

// 进程 B (同时运行)
using var lock2 = await lockProvider.LockAsync("my_lock");
// 进程 B 也能获取锁（与进程 A 的锁无关！）
```

### 2. 应用重启丢失锁

**限制**：应用重启后，所有锁都会丢失

**后果**：
- 未完成的事务可能被其他线程访问
- 可能导致数据不一致

**建议**：确保应用重启时可以恢复到一致状态

### 3. 内存占用

**限制**：每个锁都会占用内存

**计算**：
- 每个锁大约占用：
  - `lockKey` (string): 约 50-100 字节
  - `lockValue` (string): 约 50 字节（GUID）
  - `ConcurrentDictionary` 开销: 约 50 字节
- 总计：约 150-200 字节/锁

**建议**：
- 及时释放不再使用的锁
- 避免创建大量长时间持有的锁

### 4. GC 压力

**限制**：大量锁的创建和释放会增加 GC 压力

**建议**：
- 复用锁键名称
- 避免在热循环中频繁创建和释放锁

---

## 与其他实现的对比

| 特性 | InMemory 实现 | Redis 实现 | PostgreSQL 实现 |
|------|--------------|-----------|----------------|
| **适用环境** | 单机/单进程 | 分布式/多机 | 分布式/多机 |
| **性能** | 极高（内存） | 极高（Redis） | 中等（数据库） |
| **延迟** | < 1ms | 1-10ms | 10-100ms |
| **可靠性** | 进程级别 | 依赖 Redis | 依赖数据库 |
| **持久化** | 无 | 可选（RDB/AOF） | 是（数据库） |
| **部署要求** | 无需额外组件 | 需要 Redis | 需要数据库 |
| **网络依赖** | 无 | 有 | 有 |
| **进程隔离** | ❌ | ✅ | ✅ |
| **锁值验证** | ❌ | ✅ | ✅ |
| **自动过期** | ❌ | ✅ | ✅ |
| **故障恢复** | ❌ | ✅（Sentinel/Cluster） | ✅（数据库HA） |
| **推荐场景** | 测试/开发/单机 | 生产环境 | 有数据库的场景 |

### 选择建议

```
                    ┌─────────────────────────────┐
                    │     需要分布式锁？            │
                    └──────────┬──────────────────┘
                               │
                  ┌────────────┴────────────┐
                  │                         │
                 No                       Yes
                  │                         │
                  ↓                         ↓
         ┌─────────────────┐     ┌─────────────────────┐
         │ 有 Redis？       │     │   已经有 PostgreSQL？│
         └────────┬────────┘     └──────────┬──────────┘
                  │                         │
         ┌────────┴────────┐               │
         │                 │               │
        Yes               No              Yes
         │                 │               │
         ↓                 ↓               ↓
┌────────────────┐  ┌──────────┐  ┌──────────────────┐
│ Redis Lock     │  │ InMemory │  │ PostgreSQL Lock  │
└────────────────┘  │ Lock     │  └──────────────────┘
                   └──────────┘
```

---

## 性能特性

### 1. 时间复杂度

- **TryAdd()**: O(1) 平均情况
- **TryRemove()**: O(1) 平均情况
- **重试循环**: O(n)，n 为重试次数

### 2. 空间复杂度

- O(n)，n 为锁的数量

### 3. 性能基准测试（参考）

| 操作 | 延迟 | 吞吐量 |
|------|------|--------|
| 获取锁（无竞争） | < 0.1ms | > 100,000 ops/s |
| 获取锁（有竞争） | 0.1-1ms | 10,000-50,000 ops/s |
| 释放锁 | < 0.1ms | > 100,000 ops/s |

---

## 总结

`Azrng.DistributeLock.InMemory` 是一个简单、高效、易用的内存锁实现，适合单机环境和测试场景。它通过 `ConcurrentDictionary` 提供线程安全的锁操作，但受限于进程隔离和应用重启丢失锁的特性。

**核心优势**：
- ✅ 零依赖，开箱即用
- ✅ 高性能，低延迟
- ✅ 简单可靠

**主要限制**：
- ❌ 无法跨进程同步
- ❌ 应用重启丢失锁
- ❌ 不支持锁值验证

**推荐用途**：
- 开发和测试环境
- 单机部署的应用
- 临时性锁需求

对于生产环境的分布式应用，建议使用 `Azrng.DistributeLock.Redis` 或 `Azrng.DistributeLock.PostgreSQL`。
