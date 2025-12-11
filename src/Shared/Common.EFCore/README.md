# Common.EFCore

## 实体类配置

### 仅使用实体基类

Statrup的ConfigureServices方法添加

```
services.AddAutoGenerationId(); //增加自增ID
```

继承公共基类

```
IdentityBaseEntity、IdentityBaseEntity<TKey>
IdentityOperatorEntity、IdentityOperatorEntity<Tkey>
IdentityOperatorStatusEntity、  IdentityOperatorStatusEntity<TKey>
```

模型配置继承

```
EntityTypeConfigurationIdentity、EntityTypeConfigurationIdentity<T, TKey>
EntityTypeConfigurationIdentityOperator、EntityTypeConfigurationIdentityOperator<T,TKey>
EntityTypeConfigurationIdentityOperatorStatus、EntityTypeConfigurationIdentityOperatorStatus<T, TKey>
```

### 标准使用

配置用户实体

```csharp
/// <summary>
/// 用户信息
/// </summary>
public class User : IdentityBaseEntity
{
    private User() { }

    public User(string account, string passWord, bool isValid) : this()
    {
        Account = account;
        Password = passWord;
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

创建上下文OpenDbContext，继承DbContext，也可以继承自BaseDbContext(会自动配置OnModelCreating)

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

下面演示注入pgsql数据库的EFCore(需要安装包Common.EFCore.PostgresSQL)

```csharp
// default
var conn = builder.Configuration["Conn"];
builder.Services.AddEntityFramework<OpenDbContext>(config =>
       {
           config.ConnectionString = conn; // 连接字符串
           config.Schema = "azrng"; // 指定schema
       });

// 其他配置
var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;
var conn = builder.Configuration["Conn"];
builder.Services.AddEntityFramework<OpenDbContext>(config =>
       {
           config.ConnectionString = conn; // 连接字符串
           config.Schema = "azrng"; // 指定schema
       }, x =>
       {
           x.MigrationsAssembly(migrationsAssembly); // 指定迁移项目
       }, options =>
       {
           //options.UsePgToCharFunctions();
       })
       .AddUnitOfWork<OpenDbContext>();
```

然后注入IBaseRepository<User>即可使用，比如

```csharp
var content = Guid.NewGuid().ToString();
await _baseResp.AddAsync(new TestEntity(content), true); // 自带SaveChanges
```

也可以注入IUnitOfWork来进行保存，例如下面测试用例

```csharp
[Fact]
public async Task SingleDbContextAdd()
{
    var connectionStr = "Host=localhost;Username=postgres;Password=123456;Database=pgsql_test;port=5432";
    var service = new ServiceCollection();
    service.AddLogging(loggerBuilder =>
    {
        loggerBuilder.AddConsole();
    });
    service.AddEntityFramework<TestDbContext>(options =>
    {
        options.ConnectionString = connectionStr;
        options.Schema = "public";
    });

    await using var serviceProvider = service.BuildServiceProvider();
    using var scope = serviceProvider.CreateScope();

    var testRep = scope.ServiceProvider.GetRequiredService<IBaseRepository<TestEntity>>();
    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    var content = Guid.NewGuid().ToString();
    await testRep.AddAsync(new TestEntity(content));

    var flag = await unitOfWork.SaveChangesAsync();
    Assert.True(flag > 0);

    await testRep.DeleteAsync(t => t.Content == content);
}
```

## 操作例子

### 单个上下文使用Repository

```csharp
[Fact]
public async Task SingleDbContext()
{
    var connectionStr = "Host=localhost;Username=postgres;Password=123456;Database=pgsql_test;port=5432";
    var service = new ServiceCollection();
    service.AddLogging(loggerBuilder =>
    {
        loggerBuilder.AddConsole();
    });
    service.AddEntityFramework<TestDbContext>(options =>
    {
        options.ConnectionString = connectionStr;
        options.Schema = "public";
    });
    await using var serviceProvider = service.BuildServiceProvider();
    using var scope = serviceProvider.CreateScope();

    var testRep = scope.ServiceProvider.GetRequiredService<IBaseRepository<TestEntity>>();
    var content = Guid.NewGuid().ToString();
    await testRep.AddAsync(new TestEntity(content), true);

    var count = await testRep.CountAsync(t => true);
    Assert.True(count > 0);

    await testRep.DeleteAsync(t => t.Content == content);
}

```

### 单个上下文使用Repository和IUnitOfWork

```csharp
[Fact]
public async Task SingleDbContextAdd()
{
    var connectionStr = "Host=localhost;Username=postgres;Password=123456;Database=pgsql_test;port=5432";
    var service = new ServiceCollection();
    service.AddLogging(loggerBuilder =>
    {
        loggerBuilder.AddConsole();
    });
    service.AddEntityFramework<TestDbContext>(options =>
    {
        options.ConnectionString = connectionStr;
        options.Schema = "public";
    });

    await using var serviceProvider = service.BuildServiceProvider();
    using var scope = serviceProvider.CreateScope();

    var testRep = scope.ServiceProvider.GetRequiredService<IBaseRepository<TestEntity>>();
    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    var content = Guid.NewGuid().ToString();
    await testRep.AddAsync(new TestEntity(content));

    var flag = await unitOfWork.SaveChangesAsync();
    Assert.True(flag > 0);

    await testRep.DeleteAsync(t => t.Content == content);
}
```

### 多上下文使用Reposiroty

```csharp
[Fact]
public async Task MultiDbContext()
{
    var connectionStr = "Host=localhost;Username=postgres;Password=123456;Database=pgsql_test;port=5432";
    var connection2Str = "Host=localhost;Username=postgres;Password=123456;Database=pgsql_test2;port=5432";
    var service = new ServiceCollection();
    service.AddLogging(loggerBuilder =>
    {
        loggerBuilder.AddConsole();
    });
    service.AddEntityFramework<TestDbContext>(options =>
    {
        options.ConnectionString = connectionStr;
        options.Schema = "public";
    });
    service.AddEntityFramework<TestDb2Context>(options =>
    {
        options.ConnectionString = connection2Str;
        options.Schema = "public";
    });
    await using var serviceProvider = service.BuildServiceProvider();
    using var scope = serviceProvider.CreateScope();

    {
        var testRep = scope.ServiceProvider.GetRequiredService<IBaseRepository<TestEntity, TestDbContext>>();
        var content = Guid.NewGuid().ToString();
        await testRep.AddAsync(new TestEntity(content), true);

        var count = await testRep.CountAsync(t => true);
        Assert.True(count > 0);

        await testRep.DeleteAsync(t => t.Content == content);
    }

    {
        var testRep = scope.ServiceProvider.GetRequiredService<IBaseRepository<TestEntity, TestDb2Context>>();
        var content = Guid.NewGuid().ToString();
        await testRep.AddAsync(new TestEntity(content), true);

        var count = await testRep.CountAsync(t => true);
        Assert.True(count > 0);

        await testRep.DeleteAsync(t => t.Content == content);
    }
}
```

### 多上下文使用Reposiroty+IUnitOfWork

```csharp
[Fact]
public async Task MultiContextUseUnitOfWork()
{
    var connectionStr = "Host=localhost;Username=postgres;Password=123456;Database=pgsql_test;port=5432";
    var connection2Str = "Host=localhost;Username=postgres;Password=123456;Database=pgsql_test2;port=5432";
    var service = new ServiceCollection();
    service.AddLogging(loggerBuilder =>
    {
        loggerBuilder.AddConsole();
    });
    service.AddEntityFramework<TestDbContext>(options =>
           {
               options.ConnectionString = connectionStr;
               options.Schema = "public";
           })
           .AddUnitOfWork<TestDbContext>();

    service.AddEntityFramework<TestDb2Context>(options =>
           {
               options.ConnectionString = connection2Str;
               options.Schema = "public";
           })
           .AddUnitOfWork<TestDb2Context>();
    await using var serviceProvider = service.BuildServiceProvider();
    using var scope = serviceProvider.CreateScope();

    {
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<TestDbContext>>();
        var testDb1Rep = scope.ServiceProvider.GetRequiredService<IBaseRepository<TestEntity, TestDbContext>>();
        var content = Guid.NewGuid().ToString();
        await testDb1Rep.AddAsync(new TestEntity(content));
        var flag = await unitOfWork.SaveChangesAsync();
        Assert.True(flag > 0);

        var count = await testDb1Rep.CountAsync(t => true);
        Assert.True(count > 0);

        await testDb1Rep.DeleteAsync(t => t.Content == content);
    }

    {
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<TestDb2Context>>();
        var testDb2Rep = scope.ServiceProvider.GetRequiredService<IBaseRepository<TestEntity, TestDb2Context>>();
        var content = Guid.NewGuid().ToString();
        await testDb2Rep.AddAsync(new TestEntity(content));
        var flag = await unitOfWork.SaveChangesAsync();
        Assert.True(flag > 0);

        var count = await testDb2Rep.CountAsync(t => true);
        Assert.True(count > 0);

        await testDb2Rep.DeleteAsync(t => t.Content == content);
    }
}
```

### 事务操作

#### 直接注入IUnitOfWork使用

```csharp
[HttpGet]
public async Task<int> AddAsync()
{
    await using var tran = await _ofWork.GetDatabase().BeginTransactionAsync();
    try
    {
        var list = new List<User>
                   {
                       new User("admin1", "123456", true),
                       new User("admin2", "123456", true),
                       new User("admin3", "123456", true),
                       new User("admin4", "123456", true),
                       new User("admin5", "123456", true),
                       new User("admin6", "123456", true)
                   };

        await _userRep.AddAsync(list, true);

        var userAddress = new UserAddress { Name = "淘宝", UserId = list[0].Id, Address = "上海市" };
        await _userAddressRep.AddAsync(userAddress, true);

        await tran.CommitAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"添加失败 {ex.GetExceptionAndStack()}");
        await tran.RollbackAsync();
    }

    return 1;
}
```

#### 直接注入IUnitOfWork使用CommitTransactionAsync

```csharp
[HttpGet]
public async Task<int> Add2Async()
{
    await _ofWork.CommitTransactionAsync(async () =>
    {
        var list = new List<User>
                   {
                       new User("admin1", "123456", true),
                       new User("admin2", "123456", true),
                       new User("admin3", "123456", true),
                       new User("admin4", "123456", true),
                       new User("admin5", "123456", true),
                       new User("admin6", "123456", true)
                   };

        await _userRep.AddAsync(list, true);

        var userAddress = new UserAddress { Name = "淘宝", UserId = list[0].Id, Address = "上海市" };
        await _userAddressRep.AddAsync(userAddress, true);
    });
    return 1;
}
```

#### 使用unitOfWork的GetDatabase

```csharp
[Fact]
public async Task SingleDbContextAdd()
{
    var connectionStr = "Host=localhost;Username=postgres;Password=123456;Database=pgsql_test;port=5432";
    var service = new ServiceCollection();
    service.AddLogging(loggerBuilder =>
    {
        loggerBuilder.AddConsole();
    });
    service.AddEntityFramework<TestDbContext>(options =>
    {
        options.ConnectionString = connectionStr;
        options.Schema = "public";
    });

    await using var serviceProvider = service.BuildServiceProvider();
    using var scope = serviceProvider.CreateScope();

    var testRep = scope.ServiceProvider.GetRequiredService<IBaseRepository<TestEntity>>();
    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

    await using var tran = await unitOfWork.GetDatabase().BeginTransactionAsync();

    try
    {
        {
            var content = Guid.NewGuid().ToString();
            await testRep.AddAsync(new TestEntity(content));

            var flag = await unitOfWork.SaveChangesAsync();
            Assert.True(flag > 0);
        }

        {
            var content = Guid.NewGuid().ToString();
            await testRep.AddAsync(new TestEntity(content));

            var flag = await unitOfWork.SaveChangesAsync();
            Assert.True(flag > 0);
        }
        await tran.CommitAsync();
    }
    catch (Exception ex)
    {
        await tran.RollbackAsync();
        _testOutputHelper.WriteLine(ex.Message);
    }
}
```

## 版本更新记录

* 1.5.0
  * 支持.Net10
* 1.4.3
  * 修复IsUnTimeZoneDateTime方法默认值问题
* 1.4.2
  * 支持老的creater、modifyer、modify_time
* 1.4.1
  * 更新GetListAsync方法响应
* 1.4.0
  * 调整目录
* 1.4.0-beta8
  * IBaseRepository支持指定DbContext
* 1.4.0-beta7
  * 机器id使用随机数生成
* 1.4.0-beta6
  * 暴漏GetDatabase方法
* 1.4.0-beta5
  * 修改更新人
* 1.4.0-beta4
  * 修复long类型时候，ID没有生成问题
* 1.4.0-beta3
  * 增加更多对ToPageListAsync的扩展
* 1.4.0-beta2
  * 移除针对netstandard2.1版本的支持
* 1.4.0-beta1
  * 支持.Net9
* 1.3.2
    * 修复IUnitOfWork&lt;IEntity&gt;在多上下文中保存失败的问题
* 1.3.1
    * 移出调用工作单元的时候才添加IUnitOfWork，默认会添加一个IUnitOfWork
* 1.3.0
    * 适配Common.Db.Core的0.1.0版本
    * 增加分页扩展ToPageListAsync
* 1.3.0-beta4
    * 修改方法SetDelete为SetDeleted
    * 默认设置创建时间的时候使用无时区时间，防止pgsql出问题
* 1.3.0-beta3
    * 迁移Common.EfCore的类到DBCore中
* 1.3.0-beta2
    * 升级.Net8
* 1.3.0-beta1
    * 模型类优化
    * 将pgsql中列的PropertyBuilderExtensions迁移到该程序集
    * 增加BaseRepository作为公共的操作，且方法为虚方法
    * 移除IBaseRepository中的同步方法
* 1.2.1
    * 查询请求类优化

    * QueryableExtensions类更新
* 1.2.0
    * GetPageRequest增加一个查询关键字
    * 将EFCoreExtension内容迁移到工作单元下
    * 工作单元类需要单独注入，如services.AddUnitOfWork&lt;BaseDbContext&gt;();
* 1.2.0-beta2
    * 将创建时间修改时间等改为传入方案，用来应对pgsql的时间区分有时区无时区方案
* 1.2.0-beta1
    * 升级支持.net7
* 1.0.0-beta8
    * 增加表达式树扩展方法，替换nuget包System.Linq.Dynamic.Core
* 1.0.0-beta7
    * 增加执行SQL扩展
    * 增加非追踪
* 1.0.0-beta5
    * 更新注册的方法从AddEntityBase变更为AddIdHelper()
* 1.0.0-beta4
    * 支持主键自定义类型
* 1.1.0-beta3
    * 增加分页相关的类
    * 去除common包的依赖
* 1.1.0-beta2
    * 更新因为Common包升级导致的问题
* 1.1.0-beta1
    * 修改版本支持.net5、.net6、.netstandard2.1
    * 修改OrderBy排序方法
* 1.0.6
    * 修改QueryableExtensions扩展，分页支持返回总条数，如果参数错误抛出异常
* 1.0.5
    * 修改QueryableExtensions扩展
* 1.0.4
    * 增加默认注入，支持单独使用该库的model类AddEntityBase
    * 主键ID修改类型为long类型
* 1.0.3
    * 基本的base类封装
    * IBaseRepository接口编写
    * 工作单元封装