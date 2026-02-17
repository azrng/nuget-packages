# Azrng.SqlMigration 架构与原理说明

## 一、项目概述

`Azrng.SqlMigration` 是一个基于 ASP.NET Core 的 SQL 脚本自动迁移 NuGet 包，目前支持 PostgreSQL 数据库。该项目通过版本化的 SQL 脚本文件管理数据库架构变更，实现数据库的自动化版本升级。

### 核心特性

- **版本化脚本管理**：支持语义化版本号（如 `1.0.0`、`1.0.0.0`）的 SQL 脚本文件
- **自动迁移**：应用启动时自动检测并执行数据库升级
- **多数据库支持**：支持同时迁移多个数据库
- **事务保护**：每个版本的迁移在独立事务中执行，失败自动回滚
- **分布式锁**：支持多实例部署环境下防止并发迁移冲突
- **回调扩展**：提供丰富的迁移生命周期回调接口

---

## 二、架构设计

### 2.1 整体架构图

```
┌─────────────────────────────────────────────────────────────────┐
│                      ASP.NET Core Application                    │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │           IStartupFilter (SqlMigrationStartupFilter)         ││
│  │                   应用启动触发迁移                            ││
│  └───────────────────────┬─────────────────────────────────────┘│
│                          │                                       │
│                          ▼                                       │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │          ISqlMigrationService (SqlMigrationService)          ││
│  │                      核心迁移服务                            ││
│  └───────────────────────┬─────────────────────────────────────┘│
│                          │                                       │
│          ┌───────────────┼───────────────┐                     │
│          ▼               ▼               ▼                     │
│  ┌───────────────┐ ┌──────────────┐ ┌────────────────────┐    │
│  │ IDbVersionService│ │IMigrationHandler││  IInitVersionSetter ││
│  │  (版本管理)   │ │ (回调处理)   │ │  (初始化版本)      │    │
│  └───────────────┘ └──────────────┘ └────────────────────┘    │
│          │                                                     │
│          ▼                                                     │
│  ┌───────────────────────────────────────────────────────────┐│
│  │                   Database (PostgreSQL)                   ││
│  │              ┌─────────────────────────────┐              ││
│  │              │   app_version_log 表        │              ││
│  │              │  (记录迁移版本历史)          │              ││
│  │              └─────────────────────────────┘              ││
│  └───────────────────────────────────────────────────────────┘│
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### 2.2 核心组件

#### 2.2.1 配置组件

**SqlMigrationOption** ([SqlMigrationOption.cs](SqlMigrationOption.cs))
```csharp
public class SqlMigrationOption
{
    // 数据库连接构建器
    Func<IServiceProvider, IDbConnection> ConnectionBuilder

    // 分布式锁提供器
    Func<IServiceProvider, Task<IAsyncDisposable?>>? LockProvider

    // SQL 脚本文件所在目录
    string? SqlRootPath

    // SQL 文件版本前缀（默认 "version"）
    string VersionPrefix

    // 数据库 Schema（默认 "public"）
    string Schema

    // 初始版本设置器类型
    Type? InitVersionSetterType
}
```

#### 2.2.2 服务接口

| 接口 | 实现类 | 职责 |
|------|--------|------|
| [ISqlMigrationService](ISqlMigrationService.cs) | [SqlMigrationService](Service/SqlMigrationService.cs) | 核心迁移逻辑 |
| [IDbVersionService](IDbVersionService.cs) | [PgSqlDbVersionService](Service/PgSqlDbService.cs) | 版本记录读写 |
| [IMigrationHandler](IMigrationHandler.cs) | 用户实现 | 迁移生命周期回调 |
| [IInitVersionSetter](IInitVersionSetter.cs) | 用户实现 | 初始版本获取 |

#### 2.2.3 启动组件

**SqlMigrationStartupFilter** ([SqlMigrationStartupFilter.cs](SqlMigrationStartupFilter.cs))
- 实现 `IStartupFilter` 接口
- 在 ASP.NET Core 应用启动管道中自动触发迁移
- 支持分布式锁保护

---

## 三、工作原理

### 3.1 迁移流程

```
                    应用启动
                       │
                       ▼
        ┌──────────────────────────┐
        │ SqlMigrationStartupFilter │
        │   Configure 方法触发      │
        └───────────┬──────────────┘
                    │
                    ▼
        ┌───────────────────────┐
        │  获取分布式锁（可选）  │
        └───────────┬───────────┘
                    │
                    ▼
        ┌───────────────────────┐
        │ SqlMigrationService.  │
        │   MigrateAsync()      │
        └───────────┬───────────┘
                    │
        ┌───────────┴───────────┐
        ▼                       ▼
┌───────────────┐     ┌─────────────────┐
│ 获取当前版本  │     │ 调用初始版本    │
│   从数据库    │     │   设置器（可选）│
└───────┬───────┘     └─────────────────┘
        │
        ▼
┌───────────────────┐
│  触发 BeforeMigrateAsync 回调
└───────┬───────────┘
        │
        ▼
┌───────────────────┐
│ 扫描 SQL 文件目录  │
│  筛选需升级版本    │
└───────┬───────────┘
        │
        ▼
┌───────────────────────┐
│ 遍历每个待升级版本     │
│  ┌─────────────────┐  │
│  │ VersionUpdate-  │  │
│  │ BeforeMigrateAsync│◄───回调：版本迁移前
│  └────────┬────────┘  │
│           ▼           │
│  ┌─────────────────┐  │
│  │ 开启事务        │  │
│  │ 执行 SQL 脚本   │  │
│  │ 写入版本日志    │  │
│  │ 提交事务        │  │
│  └────────┬────────┘  │
│           │           │
│      ┌────┴────┐      │
│      ▼         ▼      │
│  成功        失败      │
│  ┌───┴───┐  ┌──┴──┐   │
│  │回调   │  │回调  │   │
│  │Success│  │Failed│   │
│  └───────┘  └─────┘   │
│  (提交)    (回滚)      │
└───────────┬───────────┘
            │
            ▼
    ┌───────────────┐
    │ MigratedAsync/ │
    │ MigrateFailedAsync│
    │   回调通知     │
    └───────────────┘
```

### 3.2 版本号解析机制

**GetVersionNum 方法** ([Service/SqlMigrationService.cs:164-190](Service/SqlMigrationService.cs#L164-L190))

版本号转换规则：

| 版本格式 | 示例 | 转换结果 | 比较能力 |
|---------|------|---------|---------|
| 3 位版本 | `1.2.3` | `001002003` | 支持百位版本号 |
| 4 位版本 | `1.2.3.4` | `001002003004` | 支持细粒度版本 |

```csharp
// 版本号转数字示例：
// "1.0.0"     -> 001000000
// "1.2.10"    -> 001002010
// "1.2.3.4"   -> 001002003004
// "111.111.111" -> 111111111
```

### 3.3 版本记录管理

**app_version_log 表结构**

| 字段 | 类型 | 说明 |
|------|------|------|
| id | bigint | 主键（自增） |
| version | text | 版本号（唯一索引） |
| created_time | timestamp | 创建时间 |

**版本查询逻辑** ([Service/PgSqlDbService.cs:21-47](Service/PgSqlDbService.cs#L21-L47))

1. 检查 `app_version_log` 表是否存在
2. 不存在返回 `0.0.0` 初始版本
3. 存在则查询最新版本记录

---

## 四、关键特性实现

### 4.1 事务保护机制

每个版本的 SQL 脚本在独立事务中执行：

```csharp
using var uow = conn.BeginTransaction();
try
{
    await ExecuteSqlFromFile(migrationName, filePath);
    await dbVersionService.WriteVersionLogAsync(migrationName, version);
    uow.Commit();
    await CallbackAsync(migrationName, SqlMigrationStep.VersionUpdateSuccess, oldVersion, version);
}
catch (Exception)
{
    uow.Rollback();
    await CallbackAsync(migrationName, SqlMigrationStep.VersionUpdateFailed, oldVersion, version);
    throw;
}
```

**保证**：
- SQL 执行失败自动回滚
- 版本记录写入失败回滚
- 确保数据库与版本记录一致性

### 4.2 分布式锁支持

通过 `LockProvider` 委托接入分布式锁：

```csharp
if (_lockProvider != null)
{
    await using var _ = await _lockProvider(scope.ServiceProvider);
    await migrateService.MigrateAsync(migrationName);
}
```

**使用场景**：多实例部署时防止多个实例同时执行迁移

**推荐实现**：`Azrng.DistributeLock.Redis` 包

### 4.3 迁移回调机制

**SqlMigrationStep 枚举** ([SqlMigrationStep.cs](SqlMigrationStep.cs))

| 步骤 | 触发时机 | 返回值影响 |
|------|---------|-----------|
| Prepare | 迁移开始前 | false 则终止迁移 |
| VersionUpdatePrepare | 每个版本执行前 | false 则跳过该版本 |
| VersionUpdateSuccess | 每个版本成功后 | 无 |
| VersionUpdateFailed | 每个版本失败后 | 无 |
| Success | 全部迁移成功 | 无 |
| Failed | 迁移失败 | 无 |

**IMigrationHandler 接口** ([IMigrationHandler.cs](IMigrationHandler.cs))

```csharp
public interface IMigrationHandler
{
    // 准备迁移 - 可终止整个迁移
    Task<bool> BeforeMigrateAsync(string oloVersion);

    // 版本迁移前 - 可跳过单个版本
    Task<bool> VersionUpdateBeforeMigrateAsync(string version);

    // 版本迁移成功
    Task VersionUpdateMigratedAsync(string version);

    // 版本迁移失败
    Task VersionUpdateMigrateFailedAsync(string version);

    // 全部迁移成功
    Task MigratedAsync(string oldVersion, string version);

    // 迁移失败
    Task MigrateFailedAsync(string oldVersion, string version);
}
```

### 4.4 多数据库支持

通过 **Keyed Services** 实现多数据库迁移：

```csharp
// 注册
services.AddSqlMigrationService("default", config => { ... })
        .AddSqlMigrationService("default2", config => { ... });

// 解析
var service = serviceProvider.GetRequiredKeyedService<ISqlMigrationService>("default");
```

---

## 五、使用示例

### 5.1 基础配置

```csharp
builder.Services.AddSqlMigrationService("default", config =>
{
    config.Schema = "aa";
    config.VersionPrefix = string.Empty;
    config.SqlRootPath = Path.Combine(builder.Environment.WebRootPath, "MigrationSql");
    config.ConnectionBuilder = (sp) => new NpgsqlConnection(conn);
}).AddAutoMigration();
```

### 5.2 多数据库配置

```csharp
builder.Services.AddSqlMigrationService("default", config =>
{
    config.Schema = "aa";
    config.SqlRootPath = Path.Combine(builder.Environment.WebRootPath, "MigrationSql");
    config.ConnectionBuilder = (sp) => new NpgsqlConnection(conn);
}).AddSqlMigrationService("default2", config =>
{
    config.Schema = "bb";
    config.SqlRootPath = Path.Combine(builder.Environment.WebRootPath, "MigrationSql2");
    config.ConnectionBuilder = (sp) => new NpgsqlConnection(conn2);
}).AddAutoMigration();
```

### 5.3 带回调处理

```csharp
builder.Services.AddSqlMigrationService<DefaultMigrationHandler>("default", config =>
{
    config.Schema = "aa";
    config.SqlRootPath = Path.Combine(builder.Environment.WebRootPath, "MigrationSql");
    config.ConnectionBuilder = (sp) => new NpgsqlConnection(conn);
    config.LockProvider = x => x.GetRequiredService<ILockProvider>()
                                  .LockAsync("project_init", TimeSpan.FromMinutes(1));
}).AddAutoMigration();
```

### 5.4 初始版本设置

适用于中途集成迁移服务的项目：

```csharp
public class CustomInitVersionSetter : IInitVersionSetter
{
    public Task<string> GetCurrentVersionAsync()
    {
        // 从其他来源获取当前版本
        return Task.FromResult("1.5.0");
    }
}

// 配置
config.SetInitVersionSetter<CustomInitVersionSetter>();
```

---

## 六、文件结构

```
Azrng.SqlMigration/
├── Extension.cs                    # 扩展方法
├── IDBVersionService.cs            # 版本服务接口
├── IInitVersionSetter.cs           # 初始版本设置接口
├── IMigrationHandler.cs            # 迁移回调接口
├── ISqlMigrationService.cs         # 迁移服务接口
├── Service/
│   ├── PgSqlDbService.cs          # PostgreSQL 版本服务实现
│   └── SqlMigrationService.cs     # 核心迁移服务实现
├── SqlMigrationConst.cs            # 常量定义
├── SqlMigrationOption.cs           # 配置选项
├── SqlMigrationServiceExtension.cs # 服务注册扩展
├── SqlMigrationStartupFilter.cs    # 启动过滤器
└── SqlMigrationStep.cs             # 迁移步骤枚举
```

---

## 七、依赖项

- **Dapper**：轻量级 ORM，用于执行 SQL 脚本
- **Npgsql**：PostgreSQL ADO.NET 数据提供程序（由用户提供）
- **Microsoft.Extensions.***：ASP.NET Core 核心依赖注入、日志、选项模式

---

## 八、扩展建议

### 8.1 支持其他数据库

实现 `IDbVersionService` 接口：

```csharp
public class MySqlDbVersionService : IDbVersionService
{
    // 实现 MySQL 特定的版本表查询逻辑
}
```

### 8.2 自定义文件解析器

支持从其他来源（如嵌入资源、远程存储）获取 SQL 脚本。

---

## 九、注意事项

1. **SQL 文件命名规范**：必须以 `VersionPrefix` 开头，以 `.sql` 或 `.txt` 结尾
2. **版本号递增**：后续版本的数字必须大于当前版本
3. **事务隔离**：确保 SQL 脚本在同一事务中可重入执行
4. **多实例部署**：生产环境建议配置分布式锁
5. **Schema 权限**：数据库用户需具备创建表和写入权限