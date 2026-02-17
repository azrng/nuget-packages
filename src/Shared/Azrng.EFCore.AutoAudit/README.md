# Azrng.EFCore.AutoAudit

一个基于 EF Core 的自动审计包，用于自动记录数据库操作的审计日志。

## 功能特性

- **自动审计**: 自动记录 INSERT、UPDATE、DELETE 操作的变更
- **敏感属性过滤**: 自动过滤敏感字段（如 Password、Token、Secret 等）
- **灵活的存储方式**: 支持数据库存储、文件存储，可自定义存储实现
- **可配置过滤**: 支持实体级别和属性级别的过滤
- **用户追踪**: 支持自定义用户 ID 提供者
- **多数据库支持**: 支持 PostgreSQL、MySQL、SQLite 等多种数据库

## 前置准备

### 创建审计表

需要手动提前创建审计表，PostgreSQL 示例如下：

```sql
CREATE TABLE public.audit_record
(
    id             varchar(50)              NOT NULL
        CONSTRAINT audit_record_pk
            PRIMARY KEY,
    table_name     varchar(120),
    operation_type integer,
    object_id      text,
    origin_value   text,
    new_value      text,
    extra          text,
    updater        text,
    update_time    timestamp with time zone NOT NULL,
    is_success      boolean                  NOT NULL
);

COMMENT ON TABLE public.audit_record IS '审计记录表';

COMMENT ON COLUMN public.audit_record.table_name IS '表名';

COMMENT ON COLUMN public.audit_record.operation_type IS '操作类型：0查询，1添加，2修改，3删除';

COMMENT ON COLUMN public.audit_record.origin_value IS '修改前的值';

COMMENT ON COLUMN public.audit_record.new_value IS '修改后的值';

COMMENT ON COLUMN public.audit_record.extra IS '扩展信息';

COMMENT ON COLUMN public.audit_record.updater IS '操作人';

COMMENT ON COLUMN public.audit_record.update_time IS '操作时间';

COMMENT ON COLUMN public.audit_record.is_success IS '是否成功';
```

其他数据库请参考上述结构创建相应表。

## 快速开始

### 基础配置

```csharp
// 1. 添加审计数据库上下文
builder.Services.AddDbContext<OpenDbContext>((provider, options) =>
{
    options.UseNpgsql(conn);
    options.AddAuditInterceptor(provider);
});

// 2. 添加审计服务
builder.Services.AddEFCoreAutoAudit(config =>
{
    config.WithAuditRecordsDbContextStore(options =>
    {
        options.UseNpgsql(conn);
    });
}, DatabaseType.PostgreSql);
```

### 使用不同的数据库

#### MySQL

```csharp
builder.Services.AddEFCoreAutoAudit(config =>
{
    config.WithAuditRecordsDbContextStore(options =>
    {
        options.UseMySql(conn, ServerVersion.AutoDetect(conn));
    });
}, DatabaseType.MySql);
```

#### SQLite

```csharp
builder.Services.AddEFCoreAutoAudit(config =>
{
    config.WithAuditRecordsDbContextStore(options =>
    {
        options.UseSqlite("Data Source=audit.db");
    });
}, DatabaseType.Sqlite);
```

## 高级配置

### 自定义用户 ID 提供者

```csharp
// 实现 IUserIdProvider 接口
public class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId()
    {
        // 从当前上下文获取用户 ID
        return _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
    }
}

// 注册提供者
builder.Services.AddEFCoreAutoAudit(config =>
{
    config.WithUserIdProvider(new CustomUserIdProvider())
          .WithAuditRecordsDbContextStore(options => options.UseNpgsql(conn));
}, DatabaseType.PostgreSql);
```

### 忽略指定实体或表

```csharp
builder.Services.AddEFCoreAutoAudit(config =>
{
    // 方式一：通过实体类型过滤
    config.IgnoreEntity<Test2Entity>()
          .WithAuditRecordsDbContextStore(options => options.UseNpgsql(conn));

    // 方式二：通过表名过滤
    config.IgnoreTable("test2")
          .WithAuditRecordsDbContextStore(options => options.UseNpgsql(conn));
}, DatabaseType.PostgreSql);
```

### 使用属性过滤器

```csharp
builder.Services.AddEFCoreAutoAudit(config =>
{
    // 只审计指定的属性
    config.WithPropertyFilter((entity, property) =>
            property.Metadata.Name == "Name" ||
            property.Metadata.Name == "Email")
          .WithAuditRecordsDbContextStore(options => options.UseNpgsql(conn));
}, DatabaseType.PostgreSql);
```

### 使用自定义存储

```csharp
// 实现 IAuditStore 接口
public class CustomAuditStore : IAuditStore
{
    public async Task SaveAsync(ICollection<AuditEntryDto> auditEntries)
    {
        // 自定义存储逻辑，例如写入 Elasticsearch、发送到消息队列等
        await WriteToCustomDestination(auditEntries);
    }
}

// 注册自定义存储
builder.Services.AddEFCoreAutoAudit(config =>
{
    config.WithStore(new CustomAuditStore());
}, DatabaseType.PostgreSql);
```

### 使用文件存储

```csharp
builder.Services.AddEFCoreAutoAudit(config =>
{
    config.WithStore<AuditFileStore>();
}, DatabaseType.PostgreSql);
```

### 保存未修改的属性

```csharp
builder.Services.AddEFCoreAutoAudit(config =>
{
    // 默认只记录修改的属性，开启后记录所有属性
    config.WithUnmodifiedProperty(true)
          .WithAuditRecordsDbContextStore(options => options.UseNpgsql(conn));
}, DatabaseType.PostgreSql);
```

## 敏感属性过滤

以下属性名称会被自动识别为敏感属性，不会记录其值：

- `Password`、`Token`、`Secret`
- `Key`、`ApiKey`、`AccessToken`
- `RefreshToken`、`PrivateKey`、`PublicKey`
- `CreditCard`、`SSN`、`SocialSecurityNumber`
- `Pin`、`Otp`、`VerificationCode`

## 操作类型枚举

```csharp
public enum DataOperationType
{
    Query = 0,   // 查询
    Add = 1,     // 添加
    Update = 2,  // 修改
    Delete = 3   // 删除
}
```

## API 参考

### IAuditConfigBuilder

| 方法 | 说明 |
|------|------|
| `WithUserIdProvider(IUserIdProvider)` | 设置用户 ID 提供者 |
| `WithUserIdProvider(Func<IServiceProvider, IUserIdProvider>)` | 使用工厂方法设置用户 ID 提供者 |
| `WithUnmodifiedProperty(bool)` | 是否保存未修改的属性 |
| `WithStore(IAuditStore)` | 设置审计存储服务 |
| `WithStore<TStore>()` | 设置泛型审计存储服务 |
| `WithEntityFilter(Func<EntityEntry, bool>)` | 添加实体过滤器 |
| `WithPropertyFilter(Func<EntityEntry, PropertyEntry, bool>)` | 添加属性过滤器 |

## 版本历史

### 1.0.0
- EFCore 自动审计包初始版本
- 支持 INSERT、UPDATE、DELETE 操作审计
- 支持多数据库存储
- 内置敏感属性过滤
- 支持实体和属性级别过滤
### 1.0.0-beta1
- EFCore自动审计包