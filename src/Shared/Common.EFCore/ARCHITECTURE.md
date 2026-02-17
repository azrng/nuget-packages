# Common.EFCore 项目架构与原理说明

## 1. 项目概述

Common.EFCore 是一个功能完善的 Entity Framework Core 扩展库，提供了 Repository 模式、Unit of Work 模式，并针对 PostgreSQL 进行了优化，适合企业级应用开发。

### 1.1 核心设计理念

- **分层架构**：通过 Repository 模式实现数据访问层的抽象
- **工作单元模式**：统一管理事务和数据库操作，确保数据一致性
- **泛型设计**：支持泛型实体和泛型主键，提供灵活的类型支持
- **多上下文支持**：可同一应用中使用多个数据库上下文
- **扩展性**：提供虚方法和扩展点，便于自定义和扩展

## 2. 架构分层

```
┌─────────────────────────────────────────────────────────┐
│                    应用层 (Application Layer)            │
│  - Service / Business Logic                             │
└────────────────────┬────────────────────────────────────┘
                     │ 注入依赖
                     ▼
┌─────────────────────────────────────────────────────────┐
│                  工作单元层 (Unit of Work)               │
│  - IUnitOfWork / IUnitOfWork<TContext>                  │
│  - UnitOfWork<TContext>                                 │
│  - 事务管理、SQL执行、Repository获取                     │
└────────────────────┬────────────────────────────────────┘
                     │ 持有
                     ▼
┌─────────────────────────────────────────────────────────┐
│                   仓储层 (Repository)                    │
│  - IBaseRepository<TEntity>                             │
│  - BaseRepository<TEntity>                              │
│  - CRUD操作、查询、分页                                  │
└────────────────────┬────────────────────────────────────┘
                     │ 使用
                     ▼
┌─────────────────────────────────────────────────────────┐
│              数据访问层 (Entity Framework Core)          │
│  - DbContext                                            │
│  - DbSet<TEntity>                                       │
│  - Entity Framework ORM                                 │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│                   数据库 (Database)                      │
│  - PostgreSQL / SQL Server / 其他 EFCore 支持的数据库    │
└─────────────────────────────────────────────────────────┘
```

## 3. 核心组件详解

### 3.1 实体基类体系 (Entity Base Classes)

实体基类提供了统一的实体结构和行为，采用继承体系设计：

```
IEntity (标记接口)
  │
  ├── IdentityBaseEntity (带主键的基础实体)
  │     │
  │     └── IdentityOperatorEntity (包含审计字段)
  │           │
  │           └── IdentityOperatorStatusEntity (包含审计和状态)
  │
  └── IdentityBaseEntity<TKey> (泛型主键版本)
        │
        └── IdentityOperatorEntity<TKey> (泛型主键审计版本)
              │
              └── IdentityOperatorStatusEntity<TKey>
```

#### 关键特性：

1. **自动ID生成**
   ```csharp
   protected IdentityBaseEntity()
   {
       Id = IdHelper.GetLongId(); // 使用雪花算法生成分布式ID
   }
   ```

2. **审计字段**
   - `Creator` - 创建者账号
   - `CreateTime` - 创建时间
   - `Updater` - 修改人
   - `UpdateTime` - 修改时间

3. **审计方法**
   ```csharp
   public void SetCreator(string name, DateTime? dateTime = null, bool setUpdater = true)
   public void SetUpdater(string name, DateTime? dateTime = null)
   ```

**设计原理**：
- 使用模板方法模式，提供默认行为和可扩展点
- 自动生成ID避免主键冲突，支持分布式场景
- 审计字段自动管理，减少手动赋值工作

### 3.2 Repository 模式

#### 接口设计：[IBaseRepository.cs](IBaseRepository.cs)

```csharp
public interface IBaseRepository<TEntity> where TEntity : IEntity
{
    // 可查询属性
    IQueryable<TEntity> Entities { get; }           // 追踪查询
    IQueryable<TEntity> EntitiesNoTacking { get; }  // 非追踪查询

    // 查询方法
    Task<TEntity> GetByIdAsync(object id);
    Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> expression, bool isTracking = false);
    Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> expression, bool isTracking = false);
    Task<GetQueryPageResult<T>> GetPageListAsync<T>(IQueryable<T> query, GetPageSortRequest vm);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression = null);
    Task<int> CountAsync(Expression<Func<TEntity, bool>> expression = null);

    // 操作方法
    Task<int> AddAsync(TEntity entity, bool submit = false);
    Task<int> UpdateAsync(TEntity entity, bool submit = false);
    Task<int> DeleteAsync(TEntity entity, bool submit = false);
}
```

#### 实现类：[BaseRepository.cs](BaseRepository.cs)

**关键设计**：

1. **延迟保存机制**
   ```csharp
   public virtual async Task<int> AddAsync(TEntity entity, bool submit = false)
   {
       await _dbContext.Set<TEntity>().AddAsync(entity).ConfigureAwait(false);
       if (submit)
           return await _dbContext.SaveChangesAsync().ConfigureAwait(false);
       return 1;
   }
   ```
   - `submit=false`：仅添加到变更追踪器，不立即保存
   - `submit=true`：立即保存到数据库
   - 设计目的：支持批量操作和事务场景

2. **追踪/非追踪查询**
   ```csharp
   private IQueryable<TEntity> GetQueryable(bool isTracking = false)
   {
       return isTracking ? Entities : EntitiesNoTacking;
   }
   ```
   - 追踪查询：实体会被 EF Core 追踪，修改后可自动更新
   - 非追踪查询：只读查询，性能更好

3. **批量更新（.NET 7+）**
   ```csharp
   #if NET7_0_OR_GREATER && (!NET10_0_OR_GREATER)
   public async Task<int> UpdateAsync(Expression<Func<TEntity, bool>> predict,
       Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls)
   {
       return await _dbContext.Set<TEntity>().Where(predict).ExecuteUpdateAsync(setPropertyCalls);
   }
   #endif
   ```
   - 直接生成 SQL UPDATE 语句，不加载实体
   - 性能优于先查询后更新

**设计原理**：
- **仓储模式**：抽象数据访问，隔离业务逻辑与数据访问
- **泛型约束**：`where TEntity : IEntity` 确保类型安全
- **虚方法设计**：允许子类重写特定行为
- **异步优先**：所有方法都是异步，提高并发性能

### 3.3 Unit of Work 模式

#### 接口设计：[IUnitOfWork.cs](IUnitOfWork.cs) & [IUnitOfWorkOfT.cs](IUnitOfWorkOfT.cs)

```csharp
public interface IUnitOfWork
{
    DatabaseFacade GetDatabase();
    IBaseRepository<TEntity> GetRepository<TEntity>() where TEntity : IEntity;
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    void CommitTransaction(Action action, IsolationLevel isolationLevel);
    Task CommitTransactionAsync(Func<Task> func, IsolationLevel isolationLevel);
    // SQL 执行方法...
}

public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    TContext DbContext { get; }
}
```

#### 实现类：[UnitOfWork.cs](UnitOfWork.cs)

**关键特性**：

1. **Repository 缓存机制**
   ```csharp
   public IBaseRepository<TEntity> GetRepository<TEntity>() where TEntity : IEntity
   {
       var key = _context.GetType().FullName + typeof(TEntity).FullName;
       if (EfCoreGlobalConfig.Repositories.TryGetValue(key, out var repository))
       {
           return (IBaseRepository<TEntity>)repository;
       }

       lock (_repositoryObj)
       {
           // Double-check locking
           var curdRepository = new BaseRepository<TEntity>(DbContext);
           EfCoreGlobalConfig.Repositories.Add(key, curdRepository);
           return curdRepository;
       }
   }
   ```
   - 使用全局字典缓存 Repository 实例
   - 双重检查锁定确保线程安全
   - 同一实体类型共享 Repository 实例

2. **事务管理**
   ```csharp
   public async Task CommitTransactionAsync(Func<Task> func, IsolationLevel isolationLevel)
   {
       var transaction = await _context.Database.BeginTransactionAsync(isolationLevel);
       try
       {
           await func();
           await transaction.CommitAsync();
       }
       catch (Exception ex)
       {
           _logger.LogError(ex, "CommitTransaction failed");
           await transaction.RollbackAsync();
           throw;
       }
       finally
       {
           await transaction.DisposeAsync();
       }
   }
   ```
   - 自动管理事务生命周期
   - 异常时自动回滚
   - 使用 try-finally 确保资源释放

3. **直接 SQL 执行**
   ```csharp
   public DataTable SqlQueryDataTable(string sql, params object[] parameters)
   {
       return _context.Database.SqlQueryDataTable(sql, parameters);
   }

   public List<T> SqlQueryList<T>(string sql, params object[] parameters) where T : class, new()
   {
       return _context.Database.SqlQueryList<T>(sql, parameters);
   }
   ```
   - 支持执行任意 SQL 查询
   - 返回 DataTable 或实体列表
   - 用于复杂查询或性能优化场景

**设计原理**：
- **工作单元模式**：管理多个 Repository 操作的事务边界
- **依赖注入**：通过 DI 容器注入 DbContext
- **资源管理**：实现 IDisposable，清理缓存资源

### 3.4 实体配置 (Entity Type Configuration)

#### 配置基类体系：[EntityTypeConfigurationIdentity.cs](EntityTypeConfigurations/EntityTypeConfigurationIdentity.cs)

```csharp
public class EntityTypeConfigurationIdentity<T, TKey> : IEntityTypeConfiguration<T>
    where T : IdentityBaseEntity<TKey>
    where TKey : IEquatable<TKey>
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        // 根据 Schema 配置表名
        if (EfCoreGlobalConfig.DbType == DatabaseType.PostgresSql &&
            !string.IsNullOrWhiteSpace(EfCoreGlobalConfig.Schema))
        {
            builder.ToTable(genericType.Name.ToLowerInvariant(), EfCoreGlobalConfig.Schema);
        }
        else
        {
            builder.ToTable(genericType.Name.ToLowerInvariant());
        }

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).IsRequired().HasMaxLength(36).HasComment("主键");
    }
}
```

**可用配置基类**：
- `EntityTypeConfigurationIdentity<T>` - 基础实体配置
- `EntityTypeConfigurationIdentity<T, TKey>` - 泛型主键配置
- `EntityTypeConfigurationIdentityOperator<T, TKey>` - 包含审计字段配置
- `EntityTypeConfigurationIdentityOperatorStatus<T, TKey>` - 完整实体配置

**设计原理**：
- **约定优于配置**：自动配置表名（小写）和主键
- **Schema 支持**：PostgreSQL 支持自定义 Schema
- **继承扩展**：基类配置通用属性，子类配置特定属性

### 3.5 扩展方法

#### 3.5.1 分页扩展：[IQueryableExtensions.cs](Extensions/IQueryableExtensions.cs)

```csharp
public static async Task<List<T>> ToPageListAsync<T>(
    this IQueryable<T> queryable,
    int pageIndex,
    int pageSize,
    RefAsync<int> totalNumber) where T : class
{
    var num = await queryable.CountAsync();
    refAsync.Value = num;
    var rows = await queryable.Skip(pageSize * (pageIndex - 1))
                              .Take(pageSize)
                              .ToListAsync();
    return rows;
}
```

**设计原理**：
- 扩展方法模式，增强 IQueryable 功能
- 先 Count 后 Skip/Take，避免二次查询
- 支持 RefAsync 返回总数

#### 3.5.2 条件更新扩展：[ConditionalUpdateExtensions.cs](Extensions/ConditionalUpdateExtensions.cs)

仅支持 .NET 10+，提供条件批量更新能力：

```csharp
// 当值不为 null 时才更新
public static UpdateSettersBuilder<TSource> SetPropertyIfNotNull<TSource, TProperty>(
    this UpdateSettersBuilder<TSource> updateSettersBuilder,
    Expression<Func<TSource, TProperty>> propertyExpression,
    TProperty valueExpression) where TProperty : class
{
    if (valueExpression is null)
        return updateSettersBuilder;

    updateSettersBuilder.SetProperty(propertyExpression,
        Expression.Constant(valueExpression, typeof(TProperty)));
    return updateSettersBuilder;
}
```

**使用示例**：
```csharp
await repository.UpdateAsync(
    x => x.Id == userId,
    x => x.SetPropertyIfNotNull(u => u.Email, email)
         .SetPropertyIfNotNull(u => u.Phone, phone)
         .SetPropertyIfTrue(isActive, u => u.IsActive, true)
);
```

**设计原理**：
- 链式调用模式，流畅接口
- 运行时条件判断，避免无效更新
- 使用表达式树构建更新语句

### 3.6 服务注册：[ServiceCollectionExtensions.cs](ServiceCollectionExtensions.cs)

```csharp
public static IServiceCollection AddUnitOfWork<TContext>(this IServiceCollection services)
    where TContext : DbContext
{
    services.AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();
    return services;
}
```

**设计原理**：
- 扩展方法模式，简洁的 API
- Scoped 生命周期，与 HTTP 请求绑定
- 泛型类型参数支持多 DbContext 场景

## 4. 工作流程

### 4.1 查询流程

```
┌──────────────┐
│   Service    │
│              │ GetQueryable()
└──────┬───────┘
       │
       ▼
┌──────────────────────┐
│   IUnitOfWork        │ GetRepository<TEntity>()
│                      │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────┐
│  BaseRepository      │ Entities / EntitiesNoTacking
│                      │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────┐
│   DbContext          │ DbSet<TEntity>
│                      │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────┐
│   EF Core Query      │ LINQ → SQL
│                      │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────┐
│    Database          │
│                      │
└──────────────────────┘
```

### 4.2 事务流程

```
┌──────────────┐
│   Service    │
└──────┬───────┘
       │ CommitTransactionAsync(async () => {
       │     // 1. 获取 Repository
       │     var orderRep = _unitOfWork.GetRepository<Order>();
       │     var itemRep = _unitOfWork.GetRepository<OrderItem>();
       │
       │     // 2. 执行操作
       │     await orderRep.AddAsync(order);
       │     await itemRep.AddAsync(item);
       │ })
       │
       ▼
┌──────────────────────────┐
│  UnitOfWork              │ BeginTransactionAsync()
│                          │
└──────┬───────────────────┘
       │
       ▼
┌──────────────────────────┐
│  执行委托                 │ 执行传入的操作
│                          │
└──────┬───────────────────┘
       │ 成功
       ├──────────┐
       │          ▼
       │   ┌────────────────┐
       │   │ CommitAsync()  │
       │   └────────────────┘
       │
       │ 失败
       └──────────┐
                  ▼
          ┌────────────────┐
          │ RollbackAsync()│
          └────────────────┘
```

### 4.3 多 DbContext 场景

```csharp
// 配置
services.AddEntityFramework<MainDbContext>(config => { ... })
        .AddUnitOfWork<MainDbContext>();

services.AddEntityFramework<LogDbContext>(config => { ... })
        .AddUnitOfWork<LogDbContext>();

// 使用
public class MultiDbService
{
    private readonly IBaseRepository<User, MainDbContext> _userRep;
    private readonly IBaseRepository<Log, LogDbContext> _logRep;
    private readonly IUnitOfWork<MainDbContext> _mainUow;
    private readonly IUnitOfWork<LogDbContext> _logUow;
}
```

**设计原理**：
- 每个 DbContext 有独立的 UnitOfWork 实例
- Repository 缓存 key 包含 DbContext 类型
- 支持跨库操作，但事务需要分布式事务协调

## 5. 关键技术点

### 5.1 分布式 ID 生成

使用雪花算法（Snowflake）生成分布式唯一 ID：
- 64位 Long 类型
- 包含时间戳、机器ID、序列号
- 通过 `IdHelper.GetLongId()` 生成

### 5.2 PostgreSQL 优化

1. **Schema 支持**
   ```csharp
   builder.ToTable(genericType.Name.ToLowerInvariant(), EfCoreGlobalConfig.Schema);
   ```

2. **时区处理**
   - 默认使用 `timestamp without time zone`
   - 避免时区转换问题
   - 通过 `ToUnspecifiedDateTime()` 转换

3. **蛇形命名**
   ```csharp
   config.IsSnakeCaseNaming = true; // 可选配置
   ```

### 5.3 线程安全

Repository 缓存使用双重检查锁定：
```csharp
lock (_repositoryObj)
{
    if (EfCoreGlobalConfig.Repositories.TryGetValue(key, out var repository1))
    {
        return (IBaseRepository<TEntity>)repository1;
    }
    // 创建并缓存
}
```

### 5.4 异步编程

- 所有 I/O 操作都是异步方法
- 使用 `ConfigureAwait(false)` 避免上下文切换
- 支持 `CancellationToken` 传递

## 6. 最佳实践

### 6.1 使用实体基类

```csharp
// 推荐：继承基类，自动获得ID和审计功能
public class User : IdentityOperatorEntity
{
    public string Name { get; set; }
}

// 创建时自动生成ID
var user = new User { Name = "Admin" };
```

### 6.2 事务管理

```csharp
// 推荐：使用 UnitOfWork 管理事务
await _unitOfWork.CommitTransactionAsync(async () =>
{
    await orderRep.AddAsync(order);
    await itemRep.AddAsync(item);
    // 自动提交或回滚
});
```

### 6.3 查询优化

```csharp
// 只读查询使用非追踪
var users = await _userRep.GetListAsync(x => x.IsActive, isTracking: false);

// 或使用 AsNoTracking
var users = await _userRep.EntitiesNoTacking.Where(x => x.IsActive).ToListAsync();
```

### 6.4 批量更新

```csharp
// .NET 7+ 批量更新（推荐）
await _userRep.UpdateAsync(
    x => x.IsActive == false,
    x => x.SetProperty(u => u.Status, UserStatus.Inactive)
);

// .NET 10+ 条件批量更新
await _userRep.UpdateAsync(
    x => x.Id == userId,
    x => x.SetPropertyIfNotNull(u => u.Email, email)
         .SetPropertyIfNotNull(u => u.Phone, phone)
);
```

## 7. 扩展性设计

### 7.1 自定义 Repository

```csharp
// 继承并重写特定方法
public class CustomRepository<TEntity> : BaseRepository<TEntity>
    where TEntity : IEntity
{
    public override async Task<TEntity> GetByIdAsync(object id)
    {
        // 自定义逻辑
        return await base.GetByIdAsync(id);
    }
}
```

### 7.2 自定义实体配置

```csharp
public class UserConfiguration : EntityTypeConfigurationIdentity<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder); // 保留基类配置

        // 自定义配置
        builder.Property(e => e.Name).IsRequired().HasMaxLength(50);
        builder.HasIndex(e => e.Email).IsUnique();
    }
}
```

## 8. 版本兼容性

| .NET 版本 | 特性支持 |
|----------|---------|
| .NET 6.0 | 基础功能 |
| .NET 7.0 | 批量更新 (ExecuteUpdateAsync) |
| .NET 8.0 | 完整支持 |
| .NET 9.0 | 完整支持 |
| .NET 10.0 | 条件批量更新扩展 |

## 9. 参考资料

- [Entity Framework Core 官方文档](https://learn.microsoft.com/zh-cn/ef/core/)
- [Repository 模式](https://docs.microsoft.com/en-us/azure/architecture/patterns/repository)
- [Unit of Work 模式](https://docs.microsoft.com/en-us/azure/architecture/patterns/unit-of-work)
- [工作单元模式详解](https://martinfowler.com/eaaCatalog/unitOfWork.html)