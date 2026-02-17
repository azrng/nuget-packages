# Azrng.EFCore.AutoAudit 架构与原理说明

## 一、项目概述

`Azrng.EFCore.AutoAudit` 是一个基于 Entity Framework Core 的自动审计库，通过 EF Core 的拦截器机制，自动捕获数据库的 INSERT、UPDATE、DELETE 操作，并记录变更前后的人员、时间、数据等信息。

### 1.1 核心设计目标

- **透明性**: 对业务代码零侵入，通过 EF Core 拦截器自动捕获变更
- **可扩展性**: 支持多种存储方式（数据库、文件、自定义），支持多种数据库
- **安全性**: 内置敏感属性过滤机制
- **灵活性**: 支持实体级和属性级的过滤配置

---

## 二、项目目录结构

```
Azrng.EFCore.AutoAudit/
├── Config/                          # 配置相关
│   ├── AuditConfig.cs              # 审计配置静态类
│   └── AuditConfigOptions.cs       # 配置选项模型
├── Domain/                          # 领域模型
│   ├── AuditRecord.cs              # 审计记录实体
│   └── AuditRecordsDbContext.cs    # 审计数据库上下文
├── Service/                         # 存储服务
│   ├── IAuditStore.cs              # 审计存储接口
│   ├── AuditRecordsDbContextStore.cs # 数据库存储实现
│   └── AuditFileStore.cs           # 文件存储实现
├── Helper/                          # 辅助工具
│   ├── ApplicationHelper.cs        # 应用辅助类
│   └── JsonHelper.cs               # JSON 序列化辅助
├── AuditInterceptor.cs             # 核心拦截器
├── AuditEntryDto.cs                # 审计条目 DTO
├── IAuditConfigBuilder.cs          # 配置构建器接口
├── AuditConfigBuilder.cs           # 配置构建器实现
├── AuditExtensions.cs              # 扩展方法
├── ServiceCollectionExtension.cs   # DI 注册扩展
├── AutoAuditStartupFilter.cs       # 启动过滤器（表创建）
├── IUserIdProvider.cs              # 用户 ID 提供者接口
├── DataOperationType.cs            # 操作类型枚举
└── README.md                       # 使用文档
```

---

## 三、核心架构设计

### 3.1 分层架构图

```
┌─────────────────────────────────────────────────────────────────┐
│                         应用层 (Application)                      │
│                  AddDbContext / AddAuditInterceptor              │
└─────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────┐
│                       拦截层 (Interceptor)                       │
│                   AuditInterceptor : SaveChangesInterceptor      │
│   ┌───────────────┐   ┌───────────────┐   ┌───────────────┐    │
│   │ SavingChanges │──▶│SavedChanges   │   │SaveChanges    │    │
│   │   (Pre)       │   │   (Post)      │   │Failed         │    │
│   └───────────────┘   └───────────────┘   └───────────────┘    │
└─────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────┐
│                       配置层 (Configuration)                     │
│   ┌─────────────────┐   ┌─────────────────┐                     │
│   │ AuditConfig     │   │AuditConfig      │                     │
│   │   (静态配置)     │   │  Builder        │                     │
│   └─────────────────┘   └─────────────────┘                     │
└─────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────┐
│                       存储层 (Storage)                           │
│   ┌─────────────────┐   ┌─────────────────┐   ┌─────────────┐  │
│   │ IAuditStore     │◀──│AuditRecords     │   │AuditFile    │  │
│   │   (接口)         │   │DbContextStore   │   │Store        │  │
│   └─────────────────┘   └─────────────────┘   └─────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### 3.2 核心组件关系图

```
┌────────────────────────────────────────────────────────────────────┐
│                         DbContext                                   │
│                    (业务数据库上下文)                                 │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │                AddInterceptors(AuditInterceptor)              │  │
│  └──────────────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────────┘
                              │
                              │ SaveChangesAsync()
                              ▼
┌────────────────────────────────────────────────────────────────────┐
│                    AuditInterceptor                                 │
│  ┌──────────────────┐         ┌──────────────────┐                │
│  │  SavingChanges   │────▶   │ SavedChanges     │                │
│  │  (保存前)         │         │  (保存后)         │                │
│  │  - 收集变更       │         │  - 更新临时属性   │                │
│  │  - 创建审计条目   │         │  - 调用存储服务   │                │
│  └──────────────────┘         └──────────────────┘                │
└────────────────────────────────────────────────────────────────────┘
                              │
                              │ AuditEntryDto[]
                              ▼
┌────────────────────────────────────────────────────────────────────┐
│                    IAuditStore                                      │
│  ┌─────────────────┐   ┌─────────────────┐   ┌─────────────────┐  │
│  │DbContextStore   │   │  FileStore      │   │ CustomStore     │  │
│  │(数据库存储)      │   │  (文件存储)      │   │ (自定义存储)     │  │
│  └─────────────────┘   └─────────────────┘   └─────────────────┘  │
└────────────────────────────────────────────────────────────────────┘
```

---

## 四、工作原理详解

### 4.1 核心流程时序图

```
应用代码                  DbContext          AuditInterceptor        IAuditStore
   │                         │                      │                   │
   │   SaveChangesAsync()    │                      │                   │
   │───────────────────────▶│                      │                   │
   │                         │                      │                   │
   │                         │  SavingChangesAsync  │                   │
   │                         │─────────────────────▶│                   │
   │                         │                      │                   │
   │                         │                      │ PreSaveChanges()  │
   │                         │                      │  - 遍历ChangeTracker
   │                         │                      │  - 应用过滤器     │
   │                         │                      │  - 创建AuditEntry │
   │                         │                      │                   │
   │                         │  ◀────────────────── │                   │
   │                         │                      │                   │
   │                         │  执行数据库操作        │                   │
   │                         │  (INSERT/UPDATE/DELETE)                   │
   │                         │                      │                   │
   │                         │  SavedChangesAsync   │                   │
   │                         │─────────────────────▶│                   │
   │                         │                      │                   │
   │                         │                      │ PostSaveChanges() │
   │                         │                      │  - 更新临时属性    │
   │                         │                      │  - 设置用户/时间  │
   │                         │                      │                   │
   │                         │                      │ SaveAsync()       │
   │                         │                      │──────────────────▶│
   │                         │                      │                   │
   │                         │                      │  保存审计记录      │
   │                         │                      │◀─────────────────│
   │                         │  ◀────────────────── │                   │
   │   ◀──────────────────── │                      │                   │
   │                         │                      │                   │
```

### 4.2 AuditInterceptor 工作流程

#### 阶段一：保存前 (PreSaveChanges)

[ServiceCollectionExtension.cs:23](src/Shared/Azrng.EFCore.AutoAudit/ServiceCollectionExtension.cs#L23)
[AuditInterceptor.cs:152-187](src/Shared/Azrng.EFCore.AutoAudit/AuditInterceptor.cs#L152-L187)

```csharp
private void PreSaveChanges(DbContext dbContext)
{
    // 1. 检查审计是否启用
    if (!AuditConfig.Options.AuditEnabled) return;

    // 2. 检查是否有存储服务
    if (!_serviceProvider.GetServices<IAuditStore>().Any()) return;

    // 3. 遍历 ChangeTracker 中的所有实体
    foreach (var entityEntry in dbContext.ChangeTracker.Entries())
    {
        // 跳过未变更和分离的实体
        if (entityEntry.State is EntityState.Detached or EntityState.Unchanged)
            continue;

        // 应用实体过滤器
        if (AuditConfig.Options.EntityFilters.Any(f => f.Invoke(entityEntry) == false))
            continue;

        // 创建审计条目
        AuditEntryDtos.Add(new InternalAuditEntryDto(entityEntry));
    }
}
```

#### 阶段二：保存后 (PostSaveChanges)

[AuditInterceptor.cs:192-248](src/Shared/Azrng.EFCore.AutoAudit/AuditInterceptor.cs#L192-L248)

```csharp
private async Task PostSaveChanges()
{
    // 1. 获取当前用户 ID
    var auditUser = AuditConfig.Options.UserIdProviderFactory
        ?.Invoke(_serviceProvider)?.GetUserId();

    // 2. 更新临时属性（如数据库生成的 ID、时间戳等）
    foreach (var entry in AuditEntryDtos)
    {
        // 处理临时属性
        if (entry is InternalAuditEntryDto { TemporaryProperties.Count: > 0 } auditEntry)
        {
            // 跳过敏感属性
            if (IsSensitiveProperty(colName)) continue;

            // 更新主键、新值、旧值
            // ...
        }

        // 3. 设置审计元数据
        entry.UpdatedBy = auditUser;
        entry.UpdatedAt = DateTimeOffset.UtcNow;
        entry.Succeeded = true;
    }

    // 4. 并行保存到所有注册的存储服务
    await Task.WhenAll(_serviceProvider.GetServices<IAuditStore>()
        .Select(store => store.SaveAsync(AuditEntryDtos)));
}
```

### 4.3 InternalAuditEntryDto 构造过程

[AuditEntryDto.cs:73-142](src/Shared/Azrng.EFCore.AutoAudit/AuditEntryDto.cs#L73-L142)

```csharp
public InternalAuditEntryDto(EntityEntry entityEntry)
{
    // 1. 确定操作类型
    OperationType = entityEntry.State switch
    {
        EntityState.Added => DataOperationType.Add,
        EntityState.Deleted => DataOperationType.Delete,
        EntityState.Modified => DataOperationType.Update,
        _ => OperationType
    };

    // 2. 遍历所有属性
    foreach (var propertyEntry in entityEntry.Properties)
    {
        // 应用属性过滤器
        if (AuditConfig.Options.PropertyFilters.Any(f => f.Invoke(entityEntry, propertyEntry) == false))
            continue;

        // 3. 处理临时属性（数据库生成值）
        if (propertyEntry.IsTemporary)
        {
            TemporaryProperties.Add(propertyEntry);
            continue;
        }

        // 4. 根据操作类型收集属性值
        switch (entityEntry.State)
        {
            case EntityState.Added:
                NewValues[columnName] = propertyEntry.CurrentValue;
                break;
            case EntityState.Deleted:
                OriginalValues[columnName] = propertyEntry.OriginalValue;
                break;
            case EntityState.Modified:
                if (propertyEntry.IsModified || AuditConfig.Options.SaveUnModifiedProperties)
                {
                    OriginalValues[columnName] = propertyEntry.OriginalValue;
                    NewValues[columnName] = propertyEntry.CurrentValue;
                }
                break;
        }
    }
}
```

---

## 五、关键设计模式

### 5.1 拦截器模式 (Interceptor Pattern)

利用 EF Core 的 `SaveChangesInterceptor` 在数据库操作的生命周期中插入审计逻辑：

- **SavingChanges**: 在保存前捕获变更状态
- **SavedChanges**: 在保存后更新临时属性并持久化审计日志
- **SaveChangesFailed**: 处理保存失败场景

### 5.2 构建器模式 (Builder Pattern)

[IAuditConfigBuilder.cs](src/Shared/Azrng.EFCore.AutoAudit/IAuditConfigBuilder.cs) 和 [AuditConfigBuilder.cs](src/Shared/Azrng.EFCore.AutoAudit/AuditConfigBuilder.cs)

```csharp
// 链式调用配置
config
    .WithUserIdProvider(new CustomUserIdProvider())
    .WithUnmodifiedProperty(true)
    .IgnoreEntity<Test2Entity>()
    .WithAuditRecordsDbContextStore(options => options.UseNpgsql(conn));
```

### 5.3 策略模式 (Strategy Pattern)

[IAuditStore.cs](src/Shared/Azrng.EFCore.AutoAudit/Service/IAuditStore.cs)

定义存储策略接口，支持多种实现：

- `AuditRecordsDbContextStore`: 数据库存储
- `AuditFileStore`: 文件存储
- 自定义存储：实现 `IAuditStore` 接口

### 5.4 提供者模式 (Provider Pattern)

[IUserIdProvider.cs](src/Shared/Azrng.EFCore.AutoAudit/IUserIdProvider.cs)

```csharp
public interface IUserIdProvider
{
    string? GetUserId();
}
```

支持多种用户 ID 获取策略：
- `EnvironmentUserIdProvider`: 系统用户名
- `HttpUserIdProvider`: HTTP 请求用户
- 自定义实现

---

## 六、扩展点设计

### 6.1 实体过滤器

通过 `WithEntityFilter` 可以精确控制哪些实体需要被审计：

```csharp
config.WithEntityFilter(entityEntry =>
    entityEntry.Metadata.GetTableName() != "audit_record");
```

### 6.2 属性过滤器

通过 `WithPropertyFilter` 可以精确控制哪些属性需要被记录：

```csharp
config.WithPropertyFilter((entity, property) =>
    property.Metadata.Name != "Password");
```

### 6.3 存储扩展

实现 `IAuditStore` 接口即可自定义存储方式：

```csharp
public class ElasticsearchAuditStore : IAuditStore
{
    public async Task SaveAsync(ICollection<AuditEntryDto> auditEntries)
    {
        // 写入 Elasticsearch
    }
}
```

### 6.4 用户追踪扩展

实现 `IUserIdProvider` 接口即可自定义用户追踪：

```csharp
public class ClaimsUserIdProvider : IUserIdProvider
{
    public string? GetUserId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
    }
}
```

---

## 七、多数据库支持

### 7.1 支持的数据库

[AutoAuditStartupFilter.cs:43-65](src/Shared/Azrng.EFCore.AutoAudit/AutoAuditStartupFilter.cs#L43-L65)

- PostgreSQL (PostgreSql)
- MySQL
- SQLite
- SQL Server
- Oracle
- InMemory (用于测试)

### 7.2 表自动创建

通过 `IStartupFilter` 在应用启动时自动创建审计表：

```csharp
public class AutoAuditStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return builder =>
        {
            ExecuteAsync().GetAwaiter().GetResult();  // 创建表
            next(builder);
        };
    }
}
```

---

## 八、敏感属性过滤

### 8.1 内置敏感属性列表

[AuditInterceptor.cs:17-34](src/Shared/Azrng.EFCore.AutoAudit/AuditInterceptor.cs#L17-L34)

```csharp
private static readonly HashSet<string> SensitiveProperties = new(StringComparer.OrdinalIgnoreCase)
{
    "Password", "Token", "Secret", "Key", "ApiKey",
    "AccessToken", "RefreshToken", "PrivateKey", "PublicKey",
    "CreditCard", "SSN", "SocialSecurityNumber",
    "Pin", "Otp", "VerificationCode"
};
```

### 8.2 过滤逻辑

在收集属性值时自动跳过敏感属性：

```csharp
if (IsSensitiveProperty(colName))
{
    continue;  // 跳过敏感属性的记录
}
```

---

## 九、配置系统

### 9.1 配置选项模型

[AuditConfigOptions.cs](src/Shared/Azrng.EFCore.AutoAudit/Config/AuditConfigOptions.cs)

```csharp
internal sealed class AuditConfigOptions
{
    public bool AuditEnabled { get; set; } = true;
    public DatabaseType DatabaseType { get; set; }
    public bool SaveUnModifiedProperties { get; set; }
    public Func<IServiceProvider, IUserIdProvider>? UserIdProviderFactory { get; set; }
    public IReadOnlyCollection<Func<EntityEntry, bool>> EntityFilters { get; init; }
    public IReadOnlyCollection<Func<EntityEntry, PropertyEntry, bool>> PropertyFilters { get; init; }
}
```

### 9.2 静态配置管理

[AuditConfig.cs](src/Shared/Azrng.EFCore.AutoAudit/Config/AuditConfig.cs)

```csharp
public static class AuditConfig
{
    internal static AuditConfigOptions Options = new();

    public static void EnableAudit() => Options.AuditEnabled = true;
    public static void DisableAudit() => Options.AuditEnabled = false;
}
```

---

## 十、数据模型

### 10.1 AuditEntryDto

[AuditEntryDto.cs:12-71](src/Shared/Azrng.EFCore.AutoAudit/AuditEntryDto.cs#L12-L71)

传输对象，用于在内存中传递审计数据：

| 属性 | 类型 | 说明 |
|------|------|------|
| TableName | string | 表名 |
| OriginalValues | Dictionary<string, object?>? | 修改前的值 |
| NewValues | Dictionary<string, object?>? | 修改后的值 |
| KeyValues | Dictionary<string, object?> | 主键值 |
| OperationType | DataOperationType | 操作类型 |
| Properties | Dictionary<string, object?> | 扩展属性 |
| UpdatedAt | DateTimeOffset | 更新时间 |
| UpdatedBy | string? | 更新人 |
| Succeeded | bool | 是否成功 |

### 10.2 AuditRecord

[AuditRecord.cs](src/Shared/Azrng.EFCore.AutoAudit/Domain/AuditRecord.cs)

持久化实体，对应数据库表：

| 字段 | 数据库列 | 类型 | 说明 |
|------|---------|------|------|
| Id | id | varchar(50) | 主键 |
| TableName | table_name | varchar(120) | 表名 |
| OperationType | operation_type | integer | 操作类型 |
| ObjectId | object_id | text | 对象 ID (JSON) |
| OriginValue | origin_value | text | 旧值 (JSON) |
| NewValue | new_value | text | 新值 (JSON) |
| Extra | extra | text | 扩展信息 (JSON) |
| Updater | updater | text | 操作人 |
| UpdatedTime | update_time | timestamp with time zone | 操作时间 |
| IsSuccess | is_success | boolean | 是否成功 |

---

## 十一、线程安全与性能考虑

### 11.1 线程安全

- `AuditInterceptor` 注册为 Scoped 服务，每个请求一个实例
- `AuditEntryDtos` 是实例变量，不存在并发问题
- `AuditConfig.Options` 是静态变量，配置阶段只写，运行时只读

### 11.2 性能优化

1. **并行存储**: 使用 `Task.WhenAll` 并行写入多个存储服务
2. **按需收集**: 只在审计启用时才收集变更信息
3. **属性过滤**: 在收集阶段就过滤掉不需要审计的属性
4. **临时属性延迟处理**: 临时属性在保存后更新，避免额外的数据库查询

---

## 十二、最佳实践

### 12.1 配置建议

```csharp
// 1. 在 Program.cs/Startup.cs 中配置
builder.Services.AddDbContext<AppDbContext>((provider, options) =>
{
    options.UseNpgsql(conn);
    options.AddAuditInterceptor(provider);  // 添加拦截器
});

// 2. 配置审计存储
builder.Services.AddEFCoreAutoAudit(config =>
{
    // 使用数据库存储
    config.WithAuditRecordsDbContextStore(options =>
    {
        options.UseNpgsql(conn);
    });

    // 自定义用户追踪
    config.WithUserIdProvider<HttpUserIdProvider>();

    // 忽略不需要审计的表
    config.IgnoreTable("audit_record")
          .IgnoreTable("logs");

}, DatabaseType.PostgreSql);
```

### 12.2 生产环境建议

1. **定期清理**: 审计数据会持续增长，建议定期归档或清理历史数据
2. **独立存储**: 建议将审计数据存储在独立的数据库或文件系统中
3. **异步存储**: 对于高并发场景，建议使用消息队列异步处理审计日志
4. **索引优化**: 为 `table_name`、`update_time` 等常用查询字段添加索引

---

## 十三、常见问题

### 13.1 临时属性是什么？

临时属性是指在保存前值未知的属性，如：
- 数据库自增 ID
- 计算列
- 数据库生成的时间戳
- RowVersion

这些属性需要在 `SavedChanges` 阶段更新。

### 13.2 为什么需要 StartupFilter？

StartupFilter 用于在应用启动时自动创建审计表，避免手动执行 SQL 脚本。支持多种数据库的 DDL 语法。

### 13.3 如何禁用审计？

```csharp
// 全局禁用
AuditConfig.DisableAudit();

// 忽略特定表
config.IgnoreTable("logs");

// 忽略特定实体
config.IgnoreEntity<LogEntity>();
```

---

## 十四、总结

`Azrng.EFCore.AutoAudit` 通过 EF Core 拦截器机制实现了一个功能完整、扩展性强的自动审计解决方案。其核心设计理念包括：

1. **零侵入**: 通过拦截器自动捕获变更，业务代码无需修改
2. **高扩展**: 支持自定义存储、过滤器、用户追踪
3. **多数据库**: 支持 PostgreSQL、MySQL、SQLite、SQL Server、Oracle
4. **安全性**: 内置敏感属性过滤
5. **灵活性**: 支持实体级和属性级的精细化控制

适用于需要完整审计追踪的企业级应用场景。
