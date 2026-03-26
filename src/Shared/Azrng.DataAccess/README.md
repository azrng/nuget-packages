# Azrng.DataAccess

一个功能完整、设计优秀的数据访问层框架，支持多种主流数据库（MySQL、SQL Server、PostgreSQL、SQLite、ClickHouse、Oracle）。

## 特性

- ✅ **多数据库支持** - 一套 API 支持 6 种主流数据库
- ✅ **异步操作** - 所有方法都是异步的，性能优异
- ✅ **参数化查询** - 内置 SQL 注入防护
- ✅ **分页支持** - 开箱即用的分页查询
- ✅ **Dapper ORM** - 轻量级、高性能
- ✅ **连接池** - 支持 MySQL/PostgreSQL 连接池管理
- ✅ **.NET 8.0/9.0/10.0** - 支持最新的 .NET 版本

## 安装

```bash
# Package Manager
Install-Package Azrng.DataAccess

# .NET CLI
dotnet add package Azrng.DataAccess
```

## 快速开始

### 方式一：依赖注入（推荐）

适用于 ASP.NET Core 或使用依赖注入的项目。

```csharp
// 1. 在 Startup.cs 或 Program.cs 中配置
services.ConfigureDataSource<DataSourceConfig>(options =>
{
    options.DatabaseType = DatabaseType.MySql;
    options.Host = "localhost";
    options.Port = 3306;
    options.DbName = "mydb";
    options.User = "root";
    options.Password = "password";
});

// 2. 注入并使用
public class UserService
{
    private readonly IDbHelper _dbHelper;

    public UserService(IDbHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task<User?> GetUserAsync(int userId)
    {
        var sql = "SELECT * FROM Users WHERE UserId = @UserId";
        return await _dbHelper.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });
    }
}
```

### 方式二：直接实例化

适用于控制台应用、WinForms/WPF 或不使用依赖注入的项目。

```csharp
// 1. 使用连接字符串直接创建
var connectionString = "Server=localhost;Database=mydb;User Id=root;Password=password;";
var bridge = DbBridgeFactory.CreateDbBridge(DatabaseType.MySql, connectionString);
var dbHelper = bridge.DbHelper;

// 2. 使用配置对象创建（推荐，支持更多配置）
var config = new DataSourceConfig
{
    Type = DatabaseType.MySql,
    Host = "localhost",
    Port = 3306,
    DbName = "mydb",
    User = "root",
    Password = "password",
    Pooling = true,
    MaxPoolSize = 100
};

var bridge = DbBridgeFactory.CreateDbBridge(config);
var dbHelper = bridge.DbHelper;

// 3. 直接使用
var user = await dbHelper.QueryFirstOrDefaultAsync<User>(
    "SELECT * FROM Users WHERE UserId = @UserId",
    new { UserId = 1 });
```

## 支持的数据库

| 数据库 | DatabaseType | 说明 |
|--------|--------------|------|
| MySQL | `DatabaseType.MySql` | 最流行的开源数据库 |
| SQL Server | `DatabaseType.SqlServer` | 微软官方数据库 |
| PostgreSQL | `DatabaseType.PostgresSql` | 功能强大的开源数据库 |
| SQLite | `DatabaseType.SqLite` | 轻量级嵌入式数据库 |
| ClickHouse | `DatabaseType.ClickHouse` | 大数据分析列式数据库 |
| Oracle | `DatabaseType.Oracle` | 企业级数据库 |

## 常用 API

```csharp
// 查询
Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameter = null);
Task<List<T>> QueryAsync<T>(string sql, object? parameter = null);

// 执行
Task<int> ExecuteAsync(string sql, object? parameter = null);

// 分页
Task<IEnumerable<T>> GetSplitPageDataAsync<T>(string sql, int pageIndex, int pageSize,
    string orderColumn, string orderDirection = "DESC");
Task<int> GetDataCountAsync(string sourceSql);

// 事务
Task<IDbTransaction> BeginTransactionAsync();
```

## 高级用法

### 连接池配置（MySQL/PostgreSQL）

```csharp
services.ConfigureDataSource<DataSourceConfig>(options =>
{
    options.Host = "localhost";
    options.Port = 3306;
    options.DbName = "mydb";
    options.User = "root";
    options.Password = "password";

    // 连接池配置
    options.Pooling = true;
    options.MinPoolSize = 5;
    options.MaxPoolSize = 100;
});
```

### 事务支持

```csharp
using var transaction = await _dbHelper.BeginTransactionAsync();
try
{
    await _dbHelper.ExecuteAsync("INSERT INTO Users ...", user1);
    await _dbHelper.ExecuteAsync("UPDATE Users SET ...", user2);
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
}
```

## 版本历史

### 1.0.0-beta1

- ✅ 支持 6 种主流数据库（MySQL、SQL Server、PostgreSQL、SQLite、ClickHouse、Oracle）
- ✅ 工厂模式创建数据库桥接
- ✅ 分页查询功能
- ✅ 参数化查询（防 SQL 注入）
- ✅ 异步操作
- ✅ 事务支持
- ✅ 连接池管理（MySQL/PostgreSQL）
- ✅ 基于 Dapper ORM
- ✅ 支持 .NET 8.0/9.0/10.0

## License

MIT License
