# Common.EFCore

[![NuGet](https://img.shields.io/nuget/v/Common.EFCore.svg)](https://www.nuget.org/packages/Common.EFCore)

一个功能完善的 Entity Framework Core 扩展库，提供了 Repository 模式、Unit of Work 模式，并针对 PostgreSQL 进行了优化，适合企业级应用开发。

## 特性

- **Repository 模式** - 提供通用的增删改查接口
- **Unit of Work 模式** - 统一管理事务和数据库操作
- **多 DbContext 支持** - 可在同一应用中使用多个数据库上下文
- **实体基类** - 提供常用的基础实体类型（带主键、审计字段等）
- **ID 生成策略** - 集成分布式 ID 生成器（IdHelper）
- **PostgreSQL 优化** - 针对 PostgreSQL 的专门支持和优化
- **条件批量更新** - .NET 10+ 支持条件属性更新
- **原生 SQL 支持** - 支持执行任意 SQL 查询
- **分页查询** - 多种分页查询方式

## 支持的 .NET 版本

- .NET 6.0
- .NET 7.0
- .NET 8.0
- .NET 9.0
- .NET 10.0

## 安装

### 基础包

```bash
dotnet add package Common.EFCore
```

### PostgreSQL 数据库支持

```bash
dotnet add package Common.EFCore.PostgreSQL
```

## 快速开始

### 1. 配置服务

在 `Startup.cs` 或 `Program.cs` 中配置服务：

```csharp
// 配置自增ID生成器
services.AddAutoGenerationId();

// 配置 EF Core 和 DbContext
builder.Services.AddEntityFramework<OpenDbContext>(config =>
{
    config.ConnectionString = builder.Configuration["ConnectionStrings:Default"];
    config.Schema = "public"; // PostgreSQL Schema
    config.WorkId = 1; // 机器ID，用于分布式ID生成
})
.AddUnitOfWork<OpenDbContext>();
```

### 2. 创建实体类

#### 使用基础实体类

```csharp
/// <summary>
/// 用户信息
/// </summary>
public class User : IdentityBaseEntity
{
    private User() { }

    public User(string account, string password, bool isValid) : this()
    {
        Account = account;
        Password = password;
        IsValid = isValid;
        UserName = account;
        CreateTime = DateTime.Now.ToUnspecifiedDateTime();
    }

    /// <summary>
    /// 账号
    /// </summary>
    public string Account { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 是否有效
    /// </summary>
    public bool IsValid { get; set; }
}
```

#### 可用的实体基类

| 基类                                   | 说明         | 包含字段                                                                |
|--------------------------------------|------------|---------------------------------------------------------------------|
| `IdentityBaseEntity`                 | 带主键的基础实体   | `Id`                                                                |
| `IdentityBaseEntity<TKey>`           | 泛型主键的基础实体  | `Id`                                                                |
| `IdentityOperatorEntity<TKey>`       | 包含审计字段的实体  | `Id`, `Creator`, `CreateTime`                                       |
| `IdentityOperatorStatusEntity<TKey>` | 包含审计和状态的实体 | `Id`, `Creator`, `CreateTime`, `Updater`, `UpdateTime`, `IsDeleted` |

### 3. 创建 DbContext

```csharp
public class OpenDbContext : DbContext
{
    public OpenDbContext(DbContextOptions<OpenDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
```

或者继承 `BaseDbContext`（会自动配置 `OnModelCreating`）：

```csharp
public class OpenDbContext : BaseDbContext
{
    public OpenDbContext(DbContextOptions<OpenDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
}
```

### 4. 创建实体配置（可选）

```csharp
public class UserConfiguration : EntityTypeConfigurationIdentity<User, long>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Account)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.UserName)
            .HasMaxLength(100);

        builder.HasIndex(e => e.Account).IsUnique();
    }
}
```

#### 可用的配置基类

- `EntityTypeConfigurationIdentity<T>` - 基础实体配置
- `EntityTypeConfigurationIdentity<T, TKey>` - 泛型主键实体配置
- `EntityTypeConfigurationIdentityOperator<T, TKey>` - 包含审计字段的实体配置
- `EntityTypeConfigurationIdentityOperatorStatus<T, TKey>` - 完整实体配置

### 5. 使用 Repository

```csharp
public class UserService
{
    private readonly IBaseRepository<User> _userRep;

    public UserService(IBaseRepository<User> userRep)
    {
        _userRep = userRep;
    }

    // 添加用户（自动保存）
    public async Task AddAsync(User user)
    {
        await _userRep.AddAsync(user, true);
    }

    // 获取用户
    public async Task<User?> GetByIdAsync(long id)
    {
        return await _userRep.GetByIdAsync(id);
    }

    // 查询用户
    public async Task<User?> GetByAccountAsync(string account)
    {
        return await _userRep.GetAsync(x => x.Account == account);
    }

    // 分页查询
    public async Task<(List<User> items, int total)> GetPageListAsync(int pageIndex, int pageSize)
    {
        int total = 0;
        var items = await _userRep.GetPageListAsync(
            x => true,
            pageIndex,
            pageSize,
            x => x.CreateTime,
            false,
            ref total
        );
        return (items, total);
    }

    // 更新用户
    public async Task UpdateAsync(User user)
    {
        await _userRep.UpdateAsync(user, true);
    }

    // 删除用户
    public async Task DeleteAsync(long id)
    {
        await _userRep.DeleteAsync(x => x.Id == id, true);
    }

    // 批量删除
    public async Task DeleteByAccountAsync(string account)
    {
        await _userRep.DeleteAsync(x => x.Account == account, true);
    }
}
```

### 6. 使用 UnitOfWork 管理事务

```csharp
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBaseRepository<Order> _orderRep;
    private readonly IBaseRepository<OrderItem> _orderItemRep;

    public OrderService(
        IUnitOfWork unitOfWork,
        IBaseRepository<Order> orderRep,
        IBaseRepository<OrderItem> orderItemRep)
    {
        _unitOfWork = unitOfWork;
        _orderRep = orderRep;
        _orderItemRep = orderItemRep;
    }

    // 方式1：使用 CommitTransactionAsync（Lambda表达式方式）
    public async Task CreateOrderAsync(Order order, List<OrderItem> items)
    {
        await _unitOfWork.CommitTransactionAsync(async () =>
        {
            await _orderRep.AddAsync(order);
            foreach (var item in items)
            {
                item.OrderId = order.Id;
                await _orderItemRep.AddAsync(item);
            }
        });
    }

    // 方式2：使用 BeginTransactionScopeAsync（显式事务作用域，推荐）
    public async Task CreateOrderWithScopeAsync(Order order, List<OrderItem> items)
    {
        await using var scope = await _unitOfWork.BeginTransactionScopeAsync();
        try
        {
            await _orderRep.AddAsync(order);
            foreach (var item in items)
            {
                item.OrderId = order.Id;
                await _orderItemRep.AddAsync(item);
            }
            await scope.CommitAsync();
        }
        catch
        {
            await scope.RollbackAsync();
            throw;
        }
    }

    // 方式3：手动管理事务
    public async Task CreateOrderAsync2(Order order, List<OrderItem> items)
    {
        await using var tran = await _unitOfWork.GetDatabase().BeginTransactionAsync();
        try
        {
            await _orderRep.AddAsync(order);
            foreach (var item in items)
            {
                item.OrderId = order.Id;
                await _orderItemRep.AddAsync(item);
            }
            await _unitOfWork.SaveChangesAsync();
            await tran.CommitAsync();
        }
        catch
        {
            await tran.RollbackAsync();
            throw;
        }
    }
}
```

## 高级用法

### 多 DbContext 支持

当需要使用多个数据库时：

```csharp
// 配置多个 DbContext
services.AddEntityFramework<MainDbContext>(config =>
{
    config.ConnectionString = builder.Configuration["ConnectionStrings:Main"];
    config.Schema = "public";
})
.AddUnitOfWork<MainDbContext>();

services.AddEntityFramework<LogDbContext>(config =>
{
    config.ConnectionString = builder.Configuration["ConnectionStrings:Log"];
    config.Schema = "public";
})
.AddUnitOfWork<LogDbContext>();

// 使用时指定 DbContext 类型
public class MultiDbService
{
    private readonly IBaseRepository<User, MainDbContext> _userRep;
    private readonly IBaseRepository<OperationLog, LogDbContext> _logRep;
    private readonly IUnitOfWork<MainDbContext> _mainUnitOfWork;
    private readonly IUnitOfWork<LogDbContext> _logUnitOfWork;

    public MultiDbService(
        IBaseRepository<User, MainDbContext> userRep,
        IBaseRepository<OperationLog, LogDbContext> logRep,
        IUnitOfWork<MainDbContext> mainUnitOfWork,
        IUnitOfWork<LogDbContext> logUnitOfWork)
    {
        _userRep = userRep;
        _logRep = logRep;
        _mainUnitOfWork = mainUnitOfWork;
        _logUnitOfWork = logUnitOfWork;
    }
}
```

### 条件批量更新（.NET 10+）

在批量更新时，根据某些条件决定是否更新特定字段：

```csharp
// 示例1：当值不为 null 时才更新
string? email = GetUserEmailInput();
string? phone = GetUserPhoneInput();

await repository.UpdateAsync(
    x => x.Id == userId,
    x => x.SetPropertyIfNotNull(u => u.Email, email)
         .SetPropertyIfNotNull(u => u.Phone, phone)
);

// 示例2：当字符串不为空白时才更新
string? userName = GetUserNameInput();

await repository.UpdateAsync(
    x => x.Id == userId,
    x => x.SetPropertyIfNotNullOrWhiteSpace(u => u.UserName, userName)
         .SetProperty(u => u.UpdateTime, DateTime.Now) // 无条件更新
);

// 示例3：根据布尔条件决定是否更新
bool shouldActivate = CheckActivationCondition();
await repository.UpdateAsync(
    x => x.Id == userId,
    x => x.SetPropertyIfTrue(shouldActivate, u => u.IsActive, true)
         .SetPropertyIfTrue(shouldActivate, u => u.ActivationDate, DateTime.Today)
);

// 示例4：使用自定义条件
int newScore = GetScore();
await repository.UpdateAsync(
    x => x.Id == userId,
    x => x.SetPropertyIf(score => score > 0, u => u.Score, newScore)
);

// 示例5：混合使用普通 SetProperty 和条件更新
await repository.UpdateAsync(
    x => x.IsActive == false,
    x => x.SetProperty(u => u.UpdateTime, DateTime.Now) // 无条件更新
         .SetPropertyIfNotNull(u => u.LastLoginIp, userIp) // 条件更新
         .SetPropertyIfNotNullOrWhiteSpace(u => u.Comment, comment) // 条件更新
);
```

#### 条件更新方法说明

| 方法                                                  | 描述                             | 适用版本     |
|-----------------------------------------------------|--------------------------------|----------|
| `SetPropertyIfTrue(condition, property, value)`     | 当 condition 为 true 时才设置属性      | .NET 10+ |
| `SetPropertyIfNotNull(property, value)`             | 当 value 不为 null 时才设置属性（引用类型）   | .NET 10+ |
| `SetPropertyIfNotNullOrWhiteSpace(property, value)` | 当 value 不为 null 或空白时才设置属性（字符串） | .NET 10+ |
| `SetPropertyIf(condition, property, value)`         | 使用自定义条件判断是否设置属性                | .NET 10+ |

### 原生 SQL 查询

```csharp
public class SqlQueryService
{
    private readonly IUnitOfWork _unitOfWork;

    public SqlQueryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // 执行 SQL 命令
    public async Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
    {
        return await _unitOfWork.ExecuteSqlCommand(sql, parameters);
    }

    // 查询标量值
    public async Task<T> ExecuteScalarAsync<T>(string sql, params object[] parameters)
    {
        return await _unitOfWork.ExecuteScalar<T>(sql, parameters);
    }

    // SQL 查询列表
    public async Task<List<User>> SqlQueryListAsync(string sql, params object[] parameters)
    {
        return await _unitOfWork.GetDatabase().SqlQueryList<User>(sql, parameters);
    }

    // SQL 查询 DataTable
    public async Task<DataTable> SqlQueryDataTableAsync(string sql, params object[] parameters)
    {
        return await _unitOfWork.GetDatabase().SqlQueryDataTable(sql, parameters);
    }
}
```

### 分页查询扩展

```csharp
// 使用 GetPageRequest 进行分页
var request = new GetPageRequest
{
    PageIndex = 1,
    PageSize = 20,
    OrderField = "CreateTime",
    OrderType = "desc"
};

int total = 0;
var users = await _userRep.GetPageListAsync(request, ref total);

// 或者使用 IQueryable 分页
IQueryable<User> query = _userRep.GetQueryable();
var (items, totalCount) = await query.ToPageListAsync(1, 20);
```

## API 接口说明

### IBaseRepository\<TEntity>

#### 查询方法

| 方法                        | 说明                    |
|---------------------------|-----------------------|
| `GetByIdAsync(id)`        | 根据ID获取实体              |
| `GetAsync(predicate)`     | 根据条件获取单个实体            |
| `GetListAsync(predicate)` | 根据条件获取实体列表            |
| `GetPageListAsync(...)`   | 分页查询                  |
| `AnyAsync(predicate)`     | 判断是否存在符合条件的记录         |
| `CountAsync(predicate)`   | 统计符合条件的记录数            |
| `GetQueryable()`          | 获取 IQueryable 用于自定义查询 |

#### 操作方法

| 方法                                                        | 说明   |
|-----------------------------------------------------------|------|
| `AddAsync(entity, saveChanges = false)`                   | 添加实体 |
| `AddAsync(entities, saveChanges = false)`                 | 批量添加 |
| `UpdateAsync(entity, saveChanges = false)`                | 更新实体 |
| `UpdateAsync(predicate, expression, saveChanges = false)` | 批量更新 |
| `DeleteAsync(entity, saveChanges = false)`                | 删除实体 |
| `DeleteAsync(predicate, saveChanges = false)`             | 批量删除 |

### IUnitOfWork

| 方法                                             | 说明                    |
|------------------------------------------------|-----------------------|
| `SaveChangesAsync()`                             | 保存更改                  |
| `CommitTransactionAsync(action)`                 | 在事务中执行操作（Lambda方式）      |
| `BeginTransactionScopeAsync()`                   | 开启显式事务作用域（推荐）          |
| `GetDatabase()`                                  | 获取数据库上下文               |
| `ExecuteSqlCommand(sql, parameters)`             | 执行 SQL 命令             |
| `ExecuteScalar<T>(sql, parameters)`              | 查询标量值                  |
| `GetRepository<TEntity>()`                       | 获取指定实体的仓储              |

### ITransactionScope（事务作用域接口）

| 方法              | 说明   |
|-----------------|------|
| `CommitAsync()`  | 提交事务 |
| `RollbackAsync()` | 回滚事务 |

## 配置选项

### EfCoreConnectOption

| 选项                  | 类型     | 默认值      | 说明                   |
|---------------------|--------|----------|----------------------|
| `ConnectionString`  | string | -        | 数据库连接字符串             |
| `Schema`            | string | "public" | PostgreSQL Schema 名称 |
| `WorkId`            | int    | 0        | 机器ID，用于分布式ID生成       |
| `IsSnakeCaseNaming` | bool   | false    | 是否使用蛇形命名             |

## 常见问题

### 1. PostgreSQL 时间类型问题

该库默认使用无时区 DateTime (`timestamp without time zone`)，以避免 PostgreSQL 时区问题。如需使用有时区的时间，可以在配置中使用
`IsTimeZoneDateTime`。

```csharp
builder.Property(x => x.CreateTime)
    .IsTimeZoneDateTime(); // 使用有时区的时间
```

### 2. 多 DbContext 时如何获取 Repository

当有多个 DbContext 时，需要指定 DbContext 类型：

```csharp
IBaseRepository<User, MainDbContext> repository
```

### 3. 如何使用追踪查询和非追踪查询

```csharp
// 追踪查询（默认）
var user1 = await _userRep.GetByIdAsync(id);

// 非追踪查询（只读场景性能更好）
var user2 = await _userRep.GetQueryable()
    .AsNoTracking()
    .FirstOrDefaultAsync(x => x.Id == id);
```

### 4. 如何处理软删除

使用 `IdentityOperatorStatusEntity` 基类，它包含 `IsDeleted` 字段：

```csharp
// 软删除
await _userRep.UpdateAsync(
    x => x.Id == userId,
    x => x.SetProperty(u => u.IsDeleted, true)
);

// 查询时过滤已删除
var users = await _userRep.GetListAsync(x => !x.IsDeleted);
```

### 5. 三种事务管理方式如何选择

该库提供了三种事务管理方式，各有适用场景：

#### Lambda表达式方式（`CommitTransactionAsync`）
```csharp
await _unitOfWork.CommitTransactionAsync(async () =>
{
    // 业务逻辑
});
```
**适用场景**：简单事务，代码量少的操作

#### 显式事务作用域（`BeginTransactionScopeAsync`）推荐
```csharp
await using var scope = await _unitOfWork.BeginTransactionScopeAsync();
try
{
    // 业务逻辑
    await scope.CommitAsync();
}
catch
{
    await scope.RollbackAsync();
    throw;
}
```
**适用场景**：
- 需要更清晰的事务边界
- 复杂业务逻辑，代码量大
- 需要在事务前/后执行其他操作
- 便于调试和错误追踪

#### 手动管理事务
```csharp
await using var tran = await _unitOfWork.GetDatabase().BeginTransactionAsync();
try
{
    // 业务逻辑
    await tran.CommitAsync();
}
catch
{
    await tran.RollbackAsync();
    throw;
}
```
**适用场景**：需要直接访问 EF Core 事务对象

**推荐使用显式事务作用域**，因为它提供了：
- 更清晰的代码结构（using语法）
- 自动资源管理（Dispose自动回滚未提交的事务）
- 防止重复提交/回滚的状态检查
- 完整的日志记录

## 版本更新记录

### 1.6.2

- 新增显式事务作用域支持：
    - 添加 `ITransactionScope` 接口，提供更清晰的事务管理方式
    - 新增 `BeginTransactionScope` 和 `BeginTransactionScopeAsync` 方法
    - 支持显式提交和回滚操作
    - 自动资源管理，Dispose 时自动回滚未提交的事务
    - 防止重复提交/回滚的状态检查
    - 完整的日志记录功能
    - 保持向后兼容，原有的 `CommitTransactionAsync` 方法继续可用

### 1.6.1

- 更新说明文档

### 1.6.0

- 新增条件批量更新扩展方法（仅支持 .NET 10+）：
    - `SetPropertyIfTrue` - 当条件为 true 时才设置属性
    - `SetPropertyIfNotNull` - 当值不为 null 时才设置属性
    - `SetPropertyIfNotNullOrWhiteSpace` - 当字符串值不为 null 或空白时才设置属性
    - `SetPropertyIf` - 支持自定义条件的属性设置
    - 支持链式调用，可与普通 `SetProperty` 混合使用

### 1.5.0

- 支持 .NET 10

### 1.4.3

- 修复 `IsUnTimeZoneDateTime` 方法默认值问题

### 1.4.2

- 支持旧版字段命名（creator、modifyer、modify_time）

### 1.4.1

- 更新 `GetListAsync` 方法响应

### 1.4.0

- 调整目录结构

### 1.4.0-beta8

- `IBaseRepository` 支持指定 DbContext

### 1.4.0-beta7

- 机器 ID 使用随机数生成

### 1.4.0-beta6

- 暴露 `GetDatabase` 方法

### 1.4.0-beta5

- 修改更新人字段

### 1.4.0-beta4

- 修复 long 类型时 ID 没有生成的问题

### 1.4.0-beta3

- 增加更多对 `ToPageListAsync` 的扩展

### 1.4.0-beta2

- 移除针对 netstandard2.1 版本的支持

### 1.4.0-beta1

- 支持 .NET 9

### 1.3.2

- 修复 `IUnitOfWork<IEntity>` 在多上下文中保存失败的问题

### 1.3.1

- 移出调用工作单元的时候才添加 `IUnitOfWork`，默认会添加一个 `IUnitOfWork`

### 1.3.0

- 适配 Common.Db.Core 的 0.1.0 版本
- 增加分页扩展 `ToPageListAsync`

### 1.3.0-beta4

- 修改方法 `SetDelete` 为 `SetDeleted`
- 默认设置创建时间的时候使用无时区时间，防止 PostgreSQL 出问题

### 1.3.0-beta3

- 迁移 Common.EfCore 的类到 DBCore 中

### 1.3.0-beta2

- 升级到 .NET 8

### 1.3.0-beta1

- 模型类优化
- 将 PostgreSQL 中列的 `PropertyBuilderExtensions` 迁移到该程序集
- 增加 `BaseRepository` 作为公共的操作，且方法为虚方法
- 移除 `IBaseRepository` 中的同步方法

### 1.2.1

- 查询请求类优化
- `QueryableExtensions` 类更新

### 1.2.0

- `GetPageRequest` 增加一个查询关键字
- 将 `EFCoreExtension` 内容迁移到工作单元下
- 工作单元类需要单独注入，如 `services.AddUnitOfWork<BaseDbContext>()`

### 1.2.0-beta2

- 将创建时间修改时间等改为传入方案，用来应对 PostgreSQL 的时间区分有时区无时区方案

### 1.2.0-beta1

- 升级支持 .NET 7

### 1.0.0-beta8

- 增加表达式树扩展方法，替换 nuget 包 `System.Linq.Dynamic.Core`

### 1.0.0-beta7

- 增加执行 SQL 扩展
- 增加非追踪

### 1.0.0-beta5

- 更新注册的方法从 `AddEntityBase` 变更为 `AddIdHelper()`

### 1.0.0-beta4

- 支持主键自定义类型

### 1.1.0-beta3

- 增加分页相关的类
- 去除 common 包的依赖

### 1.1.0-beta2

- 更新因为 Common 包升级导致的问题

### 1.1.0-beta1

- 修改版本支持 .NET 5、.NET 6、.netstandard2.1
- 修改 `OrderBy` 排序方法

### 1.0.6

- 修改 `QueryableExtensions` 扩展，分页支持返回总条数，如果参数错误抛出异常

### 1.0.5

- 修改 `QueryableExtensions` 扩展

### 1.0.4

- 增加默认注入，支持单独使用该库的 model 类 `AddEntityBase`
- 主键 ID 修改类型为 long 类型

### 1.0.3

- 基本的 base 类封装
- `IBaseRepository` 接口编写
- 工作单元封装
