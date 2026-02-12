# Azrng.DbOperator

一个功能完整、设计优秀的数据库操作框架，支持多种主流数据库。

## 功能特性

- 支持多种数据库：MySQL、SQL Server、PostgreSQL、SQLite、ClickHouse、Oracle
- 工厂模式：根据数据库类型自动创建对应的桥接实现
- 桥接模式：IBasicDbBridge 和 IDbHelper 接口清晰分离数据访问和业务逻辑
- 分页支持：内置完整的分页查询功能
- Dapper ORM：轻量级、高性能的数据库操作
- 异步操作：所有数据库操作方法都是异步的
- 参数化 SQL：防止 SQL 注入，支持动态参数
- 类型安全：使用泛型约束确保类型安全
- 连接池支持：支持 MySQL/PostgreSQL 连接池管理
- 可扩展性：易于添加新数据库支持（只需实现 IBasicDbBridge 和 IDbHelper）
- 适配 .NET 8.0

## 安装

通过 NuGet 安装:

```
Install-Package Azrng.DbOperator
```

或通过 .NET CLI:

```
dotnet add package Azrng.DbOperator
```

## 项目结构

```
Azrng.DbOperator/
├── DatabaseType.cs           # 数据库类型枚举
├── DataSourceConfig.cs       # 数据源配置类
├── SystemOperatorConst.cs    # 系统常量
├── GlobalUsings.cs         # 全局 using
├── DbBridgeFactory.cs       # 桥接工厂
├── DbBridge/               # 各数据库桥接实现
│   ├── BasicDbBridge.cs      # 基础桥接抽象
│   ├── MySqlDbBridge.cs       # MySQL 实现
│   ├── SqlServerDbBridge.cs    # SQL Server 实现
│   ├── PostgreDbBridge.cs    # PostgreSQL 实现
│   ├── OracleDbBridge.cs      # Oracle 实现
│   └── ClickHouseDbBridge.cs  # ClickHouse 实现
├── Helper/                 # 数据库帮助类
│   ├── DbHelperBase.cs       # 帮助基类
│   ├── MySQLDbHelper.cs       # MySQL 实现
│   ├── PostgreSqlDbHelper.cs # PostgreSQL 实现
│   ├── SqlServerDbHelper.cs   # SQL Server 实现
│   ├── OracleDbHelper.cs      # Oracle 实现
│   ├── SqliteDbHelper.cs     # SQLite 实现
│   └── ClickHouseDbHelper.cs  # ClickHouse 实现
├── Dto/                    # 数据传输对象
│   ├── ForeignModel.cs
│   ├── GetSchemaColumnInfoDto.cs
│   ├── GetSchemaListDto.cs
│   ├── GetTableInfoBySchemaDto.cs
│   ├── IndexModel.cs
│   ├── PrimaryModel.cs
│   ├── ProcModel.cs
│   ├── SchemaTableDto.cs
│   └── ViewModel.cs
├── IBasicDbBridge.cs        # 基础桥接接口
├── IDbHelper.cs            # 数据库操作帮助接口
└── README.md               # 项目文档
```

## 支持的数据库

### 1. MySQL

**特点**：
- 支持连接字符串格式化配置（host、port、database、user、password、pooling 等）
- 完整的 CRUD 操作支持
- 事务支持
- 分页查询支持
- 连接池支持

**使用示例**：
```csharp
services.ConfigureDataSource<DataSourceConfig>(options =>
{
    options.Host = "localhost";
    options.Port = 3306;
    options.DbName = "mydb";
    options.User = "root";
    options.Password = "password";
    options.TimeIsUtc = false;
});

services.AddTransient<IDbHelper, MySqlDbHelper>(serviceProvider => new MySqlDbHelper(connectionString));

// 查询用户
var user = await _dbHelper.QueryFirstOrDefaultAsync<User>(
    "SELECT * FROM Users WHERE UserId = @UserId",
    new { UserId = userId });
```

### 2. SQL Server

**特点**：
- ADO.NET 参数化查询
- 存储过程调用支持
- 事务管理
- 批量操作支持
- 高性能查询优化

**使用示例**：
```csharp
services.ConfigureDataSource<DataSourceConfig>(options =>
{
    options.DatabaseType = DatabaseType.SqlServer;
    options.Host = "localhost";
    options.Port = 1433;
    options.DbName = "mydb";
    options.User = "sa";
    options.Password = "password";
});

services.AddTransient<IDbHelper, SqlServerDbHelper>(serviceProvider => new SqlServerDbHelper(connectionString));

// 调用存储过程
var result = await _dbHelper.ExecuteAsync(
    "EXEC GetUserDetails @UserId",
    new { UserId = userId });
```

### 3. PostgreSQL

**特点**：
- 高性能查询优化
- 完整的 JSON/JSONB 类型支持
- COPY 命令支持（快速数据导入）
- 高级查询功能（如 LATERAL JOIN）
- 通知监听功能

**使用示例**：
```csharp
services.ConfigureDataSource<DataSourceConfig>(options =>
{
    options.DatabaseType = DatabaseType.PostgresSql;
    options.Host = "localhost";
    options.Port = 5432;
    options.DbName = "mydb";
    options.User = "postgres";
    options.Password = "password";
});

services.AddTransient<IDbHelper, PostgreSqlDbHelper>(serviceProvider => new PostgreSqlDbHelper(connectionString));

// 查询 JSON 列
var users = await _dbHelper.QueryAsync<User>(
    "SELECT * FROM users WHERE active = true",
    new { Active = true });
```

### 4. SQLite

**特点**：
- 轻量级嵌入式数据库
- 无需额外服务器部署
- 适用于移动和桌面应用
- 支持 LINQ to SQL

**使用示例**：
```csharp
services.ConfigureDataSource<DataSourceConfig>(options =>
{
    options.DatabaseType = DatabaseType.SqLite;
    options.DbName = "./data/myapp.db";
});

services.AddTransient<IDbHelper, SqliteDbHelper>(serviceProvider => new SqliteDbHelper(connectionString));
```

### 5. ClickHouse

**特点**：
- 列式数据库（OLAP）
- 高性能写入和查询
- 适用于大数据分析场景
- 支持 TTL 表引擎
- SQL 接口支持

**使用示例**：
```csharp
services.ConfigureDataSource<DataSourceConfig>(options =>
{
    options.DatabaseType = DatabaseType.ClickHouse;
    options.Host = "localhost";
    options.Port = 8123;
    options.DbName = "mydb";
    options.User = "default";
    options.Password = "";
});

services.AddTransient<IDbHelper, ClickHouseDbHelper>(serviceProvider => new ClickHouseDbHelper(connectionString));
```

### 6. Oracle

**特点**：
- 企业级数据库支持
- 存储过程调用
- 高级事务管理
- 适合大型企业应用

**使用示例**：
```csharp
services.ConfigureDataSource<DataSourceConfig>(options =>
{
    options.DatabaseType = DatabaseType.Oracle;
    options.Host = "localhost";
    options.Port = 1521;
    options.DbName = "ORCL";
    options.UserId = "your_user";
    options.Password = "your_password";
});

services.AddTransient<IDbHelper, OracleDbHelper>(serviceProvider => new OracleDbHelper(connectionString));
```

## 使用方法

### 配置数据源

```csharp
services.ConfigureDataSource<DataSourceConfig>(options =>
{
    // 根据数据库类型选择配置
    options.DatabaseType = DatabaseType.MySql; // 或 DatabaseType.SqlServer, 等

    // 连接字符串
    options.Host = "your_host";
    options.Port = 3306;
    options.DbName = "your_database";
    options.User = "username";
    options.Password = "password";

    // 时区配置（可选，默认 Asia/Shanghai）
    options.TimeZoneId = "Asia/Shanghai";

    // 连接池配置（可选）
    options.Pooling = true;
    options.MinPoolSize = 5;
    options.MaxPoolSize = 100;
});
```

### 注入数据库操作帮助类

```csharp
public class UserService
{
    private readonly IDbHelper _dbHelper;

    public UserService(IDbHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    // 查询用户
    public async Task<User?> GetUserAsync(int userId)
    {
        var sql = "SELECT * FROM Users WHERE UserId = @UserId";
        return await _dbHelper.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });
    }

    // 创建用户
    public async Task CreateAsync(User user)
    {
        var sql = "INSERT INTO Users (Name, Email, CreateTime) VALUES (@Name, @Email, @CreateTime)";
        return await _dbHelper.ExecuteAsync(sql, user);
    }

    // 分页查询
    public async Task<IEnumerable<User>> GetUsersAsync(int pageIndex, int pageSize)
    {
        return await _dbHelper.GetSplitPageDataAsync<User>(
            "SELECT * FROM Users",
            pageIndex,
            pageSize,
            orderColumn: "CreateTime",
            orderDirection: "DESC");
    }
}
```

### 高级功能

#### 分页查询

```csharp
// 分页查询
var users = await _dbHelper.GetSplitPageDataAsync<User>(
    "SELECT * FROM Users",
    pageIndex: 1,
    pageSize: 20,
    orderColumn: "CreateTime",
    orderDirection: "DESC");

// 获取总数和分页数据
var count = await _dbHelper.GetDataCountAsync("SELECT * FROM Users");
Console.WriteLine($"总记录数：{count}");
```

#### 批量操作

```csharp
// 批量插入
var users = new List<User>
{
    new User { Name = "张三", Email = "zhangsan@example.com" },
    new User { Name = "李四", Email = "lisi@example.com" }
};

var affectedRows = await _dbHelper.ExecuteAsync(
    "INSERT INTO Users (Name, Email) VALUES (@Name, @Email)",
    users);
```

#### 事务支持

```csharp
// 开启事务
using var transaction = await _dbHelper.BeginTransactionAsync();

try
{
    // 执行多个操作
    await _dbHelper.ExecuteAsync("INSERT INTO Users...", user1);
    await _dbHelper.ExecuteAsync("UPDATE Users SET ...", user2);

    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
}
```

## 最佳实践

### 1. 使用连接池

MySQL 和 PostgreSQL 配置中启用连接池以提高性能：

```csharp
services.ConfigureDataSource<DataSourceConfig>(options =>
{
    options.Host = "localhost";
    options.Port = 3306;
    options.DbName = "mydb";
    options.User = "root";
    options.Password = "password";
    options.Pooling = true;         // 启用连接池
    options.MinPoolSize = 5;       // 最小连接池大小
    options.MaxPoolSize = 100;      // 最大连接池大小
});
```

### 2. 参数化查询

始终使用参数化查询，防止 SQL 注入：

```csharp
public async Task<User?> GetUserAsync(int userId)
{
    var sql = "SELECT * FROM Users WHERE UserId = @UserId";
    return await _dbHelper.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });
}
```

### 3. 异步编程

始终使用 async/await 模式，避免阻塞：

```csharp
public async Task<IEnumerable<User>> GetAllUsersAsync()
{
    var sql = "SELECT * FROM Users";
    return await _dbHelper.QueryAsync<User>(sql);
}

public async Task<int> CreateAsync(User user)
{
    var sql = "INSERT INTO Users (Name, Email) VALUES (@Name, @Email)";
    return await _dbHelper.ExecuteAsync(sql, user);
}
```

### 4. 错误处理

```csharp
try
{
    var result = await _dbHelper.ExecuteAsync(sql, user);
    return (true, result);
}
catch (Exception ex)
{
    _logger.LogError(ex, "数据库操作失败");
    return (false, 0);
}
```

### 5. 分页最佳实践

```csharp
// 不要返回过大的页尺寸
const int MaxPageSize = 100;

// 在服务层验证页尺寸
if (input.PageSize > MaxPageSize)
{
    throw new ArgumentException($"页大小不能超过 {MaxPageSize}");
}

// 记录总数用于前端显示
var page = await _dbHelper.GetSplitPageDataAsync<User>(sql, pageIndex, pageSize);
var count = await _dbHelper.GetDataCountAsync("SELECT * FROM Users");
Console.WriteLine($"当前页：{pageIndex}，总记录数：{count}");
```

## 适用场景

- 多数据库类型支持的统一应用
- 需要数据库性能优化的企业应用
- 需要事务支持的复杂业务系统
- 需要连接池管理的高并发应用
- 移动和桌面应用（SQLite）
- 大数据分析场景（ClickHouse）


## 版本更新记录

* 1.0.0
  * 初始版本
  * 支持 .NET 8.0
  * 实现基础的数据库操作框架
  * 提供工厂模式创建数据库桥接
  * 支持 MySQL、SQL Server、PostgreSQL、SQLite 数据库
  * 提供分页查询功能
  * 使用 Dapper ORM 进行数据访问

## 相关链接

- GitHub 仓库：[https://github.com/azrng/nuget-packages](https://github.com/azrng/nuget-packages)
