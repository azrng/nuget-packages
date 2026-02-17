# Azrng.AspNetCore.DbEnvConfig 架构设计文档

## 目录

- [概述](#概述)
- [架构设计](#架构设计)
- [核心组件](#核心组件)
- [工作原理](#工作原理)
- [线程安全机制](#线程安全机制)
- [JSON 配置解析](#json-配置解析)
- [扩展性设计](#扩展性设计)
- [依赖关系](#依赖关系)
- [时序图](#时序图)

---

## 概述

`Azrng.AspNetCore.DbEnvConfig` 是一个基于 ASP.NET Core Configuration 系统的数据库配置提供程序，实现了 `IConfigurationProvider` 接口，允许应用程序从数据库表中动态加载配置信息，并支持配置的热更新。

### 核心特性

1. **非侵入式集成**：通过标准的 `IConfigurationProvider` 接口集成到 ASP.NET Core 配置系统
2. **热更新支持**：后台线程定期轮询数据库，自动刷新配置变更
3. **JSON 解析**：支持复杂 JSON 结构的配置值，自动展开为层次化配置
4. **多数据库支持**：通过抽象接口支持多种关系型数据库
5. **线程安全**：使用读写锁保证配置访问的线程安全

---

## 架构设计

### 分层架构

```
┌─────────────────────────────────────────────────────────┐
│         Application Layer (用户代码)                      │
│   builder.Configuration.AddDbConfiguration(...)         │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│         Extension Layer (扩展层)                         │
│   DbConfigurationProviderExtensions                     │
│   - 提供 fluent API                                     │
│   - 参数验证                                             │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│         Configuration Layer (配置层)                     │
│   DbConfigurationSource (单例模式)                       │
│   - 创建 ConfigurationProvider 实例                      │
│   - 确保同一配置源只创建一个 Provider                     │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│         Provider Layer (提供者层)                        │
│   DbConfigurationProvider                               │
│   - 实现 ConfigurationProvider                          │
│   - 数据加载与刷新                                       │
│   - 线程安全控制                                         │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│         Data Layer (数据层)                              │
│   Database (PostgreSQL/SQL Server/MySQL)                │
└─────────────────────────────────────────────────────────┘
```

### 设计模式

| 设计模式 | 应用位置 | 说明 |
|---------|---------|------|
| **工厂模式** | `CreateDbConnection` | 通过委托函数创建数据库连接，支持多种数据库 |
| **单例模式** | `DbConfigurationSource` | 确保同一配置源只创建一个 Provider 实例 |
| **策略模式** | `IScriptService` | 通过接口抽象不同的数据库初始化脚本策略 |
| **模板方法** | `DbConfigurationProvider.Load()` | 定义加载流程，子类可覆盖特定行为 |
| **读写锁模式** | `ReaderWriterLockSlim` | 多个读操作可并发，写操作独占访问 |

---

## 核心组件

### 1. DbConfigurationProviderExtensions

**文件**: [DBConfigurationProviderExtensions.cs](DBConfigurationProviderExtensions.cs)

**职责**：
- 提供扩展方法 `AddDbConfiguration`，为 `IConfigurationBuilder` 添加数据库配置源
- 验证配置参数的有效性
- 创建并组装 `DbConfigurationSource` 实例

**关键代码**:
```csharp
public static IConfigurationBuilder AddDbConfiguration(
    this IConfigurationBuilder builder,
    Action<DBConfigOptions> action,
    IScriptService? scriptService = null)
{
    var setup = new DBConfigOptions();
    action(setup);
    setup.ParamVerify();  // 参数验证
    return builder.Add(new DbConfigurationSource(setup, scriptService ?? new DefaultScriptService()));
}
```

---

### 2. DbConfigurationSource

**文件**: [DBConfigurationSource.cs](DBConfigurationSource.cs)

**职责**：
- 实现 `IConfigurationSource` 接口
- 采用**双重检查锁定**的单例模式，确保同一配置源只创建一个 Provider 实例
- 持有配置选项和脚本服务的引用

**单例实现**:
```csharp
public IConfigurationProvider Build(IConfigurationBuilder builder)
{
    if (_uniqueInstance is null)
    {
        lock (_locker)
        {
            _uniqueInstance ??= new DbConfigurationProvider(_options, _scriptService);
        }
    }
    return _uniqueInstance!;
}
```

---

### 3. DbConfigurationProvider

**文件**: [DBConfigurationProvider.cs](DBConfigurationProvider.cs)

**职责**：
- 继承 `ConfigurationProvider` 并实现 `IDisposable`
- 从数据库加载配置数据
- 支持配置的热更新（后台轮询）
- 提供 JSON 配置值的解析和展开
- 通过读写锁保证线程安全

**核心属性**:
```csharp
private readonly ReaderWriterLockSlim _lockObj = new ReaderWriterLockSlim();
private readonly DBConfigOptions _options;
private readonly IScriptService _scriptService;
private bool _isDisposed;
```

---

### 4. DBConfigOptions

**文件**: [DBConfigOptions.cs](DBConfigOptions.cs)

**职责**：
- 封装所有配置选项
- 提供参数验证功能 `ParamVerify()`
- 解析表名的 Schema 信息

**配置项**:

| 属性 | 类型 | 默认值 | 说明 |
|-----|------|--------|------|
| `CreateDbConnection` | `Func<IDbConnection>` | 必填 | 数据库连接工厂 |
| `TableName` | `string` | `"config.system_config"` | 表名（支持 schema.table 格式） |
| `ConfigKeyField` | `string` | `"code"` | 配置键字段名 |
| `ConfigValueField` | `string` | `"value"` | 配置值字段名 |
| `ReloadOnChange` | `bool` | `true` | 是否自动刷新 |
| `ReloadInterval` | `TimeSpan` | `5秒` | 刷新间隔 |
| `FilterWhere` | `string?` | `null` | SQL 筛选条件 |
| `IsConsoleQueryLog` | `bool` | `true` | 是否输出查询日志 |

---

### 5. IScriptService / DefaultScriptService

**文件**: [IScriptService.cs](IScriptService.cs), [DefaultScriptService.cs](DefaultScriptService.cs)

**职责**：
- 定义数据库初始化脚本接口
- `DefaultScriptService` 提供 PostgreSQL 的默认实现
- 支持自定义其他数据库（SQL Server、MySQL 等）的脚本

**接口定义**:
```csharp
public interface IScriptService
{
    string GetInitTableScript(string tableName, string field, string value, string? schema = null);
    string GetInitTableDataScript();
}
```

---

### 6. Helper

**文件**: [Helper.cs](Helper.cs)

**职责**：
- 提供字典深拷贝功能
- 检测配置字典是否发生变化
- 从 `JsonElement` 提取配置值

**关键方法**:
```csharp
public static bool IsChanged(IDictionary<string, string?> oldDict, IDictionary<string, string?> newDict)
public static string? GetValueForConfig(this JsonElement e)
```

---

## 工作原理

### 初始化流程

```
应用程序
   │
   │ AddDbConfiguration(options)
   ▼
DbConfigurationProviderExtensions
   │
   │ 创建 DBConfigOptions
   │ ParamVerify() 验证参数
   ▼
DbConfigurationSource
   │
   │ Build() 被调用
   ▼
DbConfigurationProvider
   │
   │ InitTable() 创建表（如果不存在）
   │ 启动后台刷新线程（如果 ReloadOnChange=true）
   │ Load() 加载配置数据
   ▼
数据库
   │
   └── 返回配置数据
```

### 配置加载流程

**方法**: `DbConfigurationProvider.DoLoad()`

```
1. 创建数据库连接
   └──> options.CreateDbConnection()

2. 构建查询 SQL
   └──> SELECT {ConfigKeyField}, {ConfigValueField} FROM {FullTableName} WHERE 1=1 {FilterWhere}

3. 执行查询并遍历结果集
   ├──> 读取配置键 (field 0)
   ├──> 读取配置值 (field 1)
   └──> 值处理：
       ├──> JSON 检测 (以 [ 开头或 { 开头)
       │   ├──> 解析为 JSON
       │   └──> 递归展开为扁平配置
       └──> 普通字符串
           └──> 直接存入 Data 字典

4. 存储到 Data 字典 (继承自 ConfigurationProvider)
```

### JSON 配置展开机制

**方法**: `DbConfigurationProvider.LoadJsonElement()`

将 JSON 结构展开为 ASP.NET Core 配置系统的扁平键值对格式：

**输入 JSON**:
```json
{
  "Database": {
    "ConnectionString": "localhost:5432",
    "Timeout": 30
  },
  "AllowedHosts": ["localhost", "example.com"]
}
```

**展开结果**:
```
Database:ConnectionString = "localhost:5432"
Database:Timeout = "30"
AllowedHosts:0 = "localhost"
AllowedHosts:1 = "example.com"
```

**展开规则**：

| JSON 类型 | 展开规则 | 示例 |
|----------|---------|------|
| Object | 递归展开，使用 `:` 分隔 | `{"a":{"b":"v"}}` → `a:b:v` |
| Array | 使用数字索引，使用 `:` 分隔 | `{"a":[1,2]}` → `a:0:1`, `a:1:2` |
| Primitive | 直接存储 | `{"a":"text"}` → `a:text` |

---

## 线程安全机制

### 读写锁 (ReaderWriterLockSlim)

`DbConfigurationProvider` 使用 `ReaderWriterLockSlim` 实现线程安全的配置访问：

```
┌──────────────────────────────────────────────────────┐
│              ReaderWriterLockSlim                     │
├──────────────────────────────────────────────────────┤
│  读锁 (EnterReadLock)                                 │
│  ├──> TryGet()      - 获取单个配置值                  │
│  └──> GetChildKeys() - 获取子键集合                   │
│                                                       │
│  写锁 (EnterWriteLock)                                │
│  └──> Load()        - 加载/刷新配置数据               │
└──────────────────────────────────────────────────────┘
```

**关键点**：

1. **多个读操作可并发**：提高读取性能
2. **写操作独占访问**：保证数据一致性
3. **锁分离**：OnReload() 不能在写锁内调用（避免死锁）

### Load() 方法的异常处理

```csharp
public override void Load()
{
    IDictionary<string, string?> clonedData = new Dictionary<string, string?>();
    try
    {
        _lockObj.EnterWriteLock();
        clonedData = Data!.Clone();      // 备份原始数据
        Data.Clear();
        DoLoad();                         // 从数据库加载
    }
    catch (DbException ex)
    {
        this.Data = clonedData;           // 恢复原始数据
        // 记录错误日志
    }
    finally
    {
        _lockObj.ExitWriteLock();
    }

    // 在锁外调用 OnReload，避免死锁
    if (Helper.IsChanged(clonedData, Data))
    {
        OnReload();  // 触发配置变更事件
    }
}
```

---

## 扩展性设计

### 支持其他数据库

通过实现 `IScriptService` 接口，可以支持不同的数据库：

```csharp
// SQL Server 示例
public class SqlServerScriptService : IScriptService
{
    public string GetInitTableScript(string tableName, string field, string value, string? schema = null)
    {
        return $@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '{tableName}')
            CREATE TABLE {tableName} (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                {field} NVARCHAR(50) NOT NULL,
                {value} NVARCHAR(MAX) NOT NULL,
                IsDelete BIT NOT NULL DEFAULT 0
            )";
    }

    public string GetInitTableDataScript() => string.Empty;
}
```

### 自定义配置值处理

继承 `DbConfigurationProvider` 并覆盖 `TryLoadAsJson` 方法，可以实现自定义的值处理逻辑。

---

## 依赖关系

### NuGet 依赖

```
Microsoft.Extensions.Configuration (框架抽象)
├── IConfigurationBuilder
├── IConfigurationSource
└── ConfigurationProvider

System.Data.Common (数据库抽象)
└── IDbConnection

System.Text.Json (JSON 解析)
└── JsonDocument
```

### 类关系图

```
┌───────────────────────────────────────────────────────────┐
│                   IConfigurationSource                    │
│                   <<interface>>                           │
├───────────────────────────────────────────────────────────┤
│ + Build(IConfigurationBuilder): IConfigurationProvider   │
└───────────────────────────────────────────────────────────┘
                            ▲
                            │
┌───────────────────────────────────────────────────────────┐
│              DbConfigurationSource                        │
├───────────────────────────────────────────────────────────┤
│ - _options: DBConfigOptions                              │
│ - _scriptService: IScriptService                         │
│ - _uniqueInstance: DbConfigurationProvider               │
├───────────────────────────────────────────────────────────┤
│ + Build(IConfigurationBuilder): IConfigurationProvider   │
└───────────────────────────────────────────────────────────┘
                            │
                            │ 创建
                            ▼
┌───────────────────────────────────────────────────────────┐
│              ConfigurationProvider                        │
│              <<abstract>>                                │
├───────────────────────────────────────────────────────────┤
│ # Data: IDictionary<string, string?>                     │
├───────────────────────────────────────────────────────────┤
│ # Load()                                                 │
│ + TryGet(string, out string?): bool                      │
│ + GetChildKeys(...): IEnumerable<string>                 │
└───────────────────────────────────────────────────────────┘
                            ▲
                            │
┌───────────────────────────────────────────────────────────┐
│             DbConfigurationProvider                      │
│             <<implements IDisposable>>                  │
├───────────────────────────────────────────────────────────┤
│ - _options: DBConfigOptions                              │
│ - _scriptService: IScriptService                         │
│ - _lockObj: ReaderWriterLockSlim                         │
├───────────────────────────────────────────────────────────┤
│ + Load()                                                 │
│ + TryGet(...): bool                                      │
│ + GetChildKeys(...): IEnumerable<string>                 │
│ + Dispose()                                              │
│ - DoLoad()                                               │
│ - InitTable()                                            │
│ - TryLoadAsJson(...)                                     │
└───────────────────────────────────────────────────────────┘

┌───────────────────────────────────────────────────────────┐
│              IScriptService <<interface>>                 │
├───────────────────────────────────────────────────────────┤
│ + GetInitTableScript(...): string                        │
│ + GetInitTableDataScript(): string                       │
└───────────────────────────────────────────────────────────┘
                            ▲
                            │
┌───────────────────────────────────────────────────────────┐
│           DefaultScriptService                           │
├───────────────────────────────────────────────────────────┤
│ + GetInitTableScript(...): string                        │
│ + GetInitTableDataScript(): string                       │
└───────────────────────────────────────────────────────────┘
```

---

## 时序图

### 配置读取流程

```
用户代码 ────> IConfiguration ────> DbConfigurationProvider
                                           │
                                           ▼
                                    ReaderWriterLockSlim
                                    (EnterReadLock)
                                           │
                                           ▼
                                    Data 字典
                                    (TryGetValue)
                                           │
                                           ▼
                                    ReaderWriterLockSlim
                                    (ExitReadLock)
                                           │
                                           ▼
                                    返回配置值
```

### 配置刷新流程

```
后台线程 ────> DbConfigurationProvider ────> ReaderWriterLockSlim
                                               │
                                               ▼
                                        备份当前 Data
                                               │
                                               ▼
                                        清空 Data
                                               │
                                               ▼
                                        数据库查询
                                               │
                                               ▼
                                        填充新数据
                                               │
                                               ▼
                                        ReaderWriterLockSlim
                                        (ExitWriteLock)
                                               │
                                               ▼
                                        比较新旧数据
                                               │
                               ┌───────────────┴───────────────┐
                               │                               │
                          数据变化                       数据未变
                               │                               │
                               ▼                               │
                        OnReload()                            │
                        通知变更                              │
                               │                               │
                               └───────────────┬───────────────┘
```

---

## 关键设计决策

### 1. 为什么使用 ReaderWriterLockSlim 而不是 lock？

- **读多写少场景**：配置读取远多于刷新
- **并发性能**：多个读操作可以并发执行
- **写操作独占**：保证数据一致性

### 2. 为什么 OnReload 在锁外调用？

```csharp
// ❌ 错误：在锁内调用可能导致死锁
lock (_lockObj)
{
    Load();
    OnReload();  // 可能触发其他读取操作，尝试获取读锁
}

// ✅ 正确：在锁外调用
lock (_lockObj)
{
    Load();
}
OnReload();  // 安全
```

### 3. 为什么使用单例模式？

- **避免重复连接**：同一配置源创建多个 Provider 会导致多个数据库连接
- **配置一致性**：确保所有配置请求来自同一个数据源
- **资源节约**：减少后台刷新线程数量

### 4. 为什么克隆数据而不是直接修改？

- **异常恢复**：如果数据库查询失败，可以恢复到原始配置
- **变更检测**：比较新旧数据决定是否触发 OnReload

---

## 文件清单

| 文件 | 行数 | 说明 |
|-----|------|------|
| [DBConfigurationProvider.cs](DBConfigurationProvider.cs) | 249 | 核心提供者，实现配置加载和刷新 |
| [DBConfigurationSource.cs](DBConfigurationSource.cs) | 47 | 配置源，单例管理 Provider 实例 |
| [DBConfigOptions.cs](DBConfigOptions.cs) | 115 | 配置选项类 |
| [DBConfigurationProviderExtensions.cs](DBConfigurationProviderExtensions.cs) | 53 | 扩展方法，提供 fluent API |
| [IScriptService.cs](IScriptService.cs) | 35 | 脚本服务接口 |
| [DefaultScriptService.cs](DefaultScriptService.cs) | 41 | PostgreSQL 脚本服务默认实现 |
| [Helper.cs](Helper.cs) | 69 | 辅助工具类 |

---

## 总结

`Azrng.AspNetCore.DbEnvConfig` 通过以下设计实现了灵活、可靠的数据库配置管理：

1. **标准化集成**：实现 ASP.NET Core Configuration 抽象
2. **线程安全**：读写锁保护并发访问
3. **热更新**：后台轮询 + 事件通知
4. **多数据库支持**：通过接口抽象实现可扩展性
5. **JSON 展开**：自动处理复杂配置结构

该组件遵循 SOLID 原则，提供了清晰的扩展点，适合在各种 ASP.NET Core 项目中使用。
