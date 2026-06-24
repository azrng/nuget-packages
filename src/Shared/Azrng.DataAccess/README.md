# Azrng.DataAccess

一个功能完整、设计优秀的数据访问层框架，支持多种主流数据库（MySQL、SQL Server、PostgreSQL、SQLite、ClickHouse、Oracle）。

## 特性

- ✅ **多数据库支持** - 一套 API 支持 6 种主流数据库
- ✅ **异步操作** - 所有方法都是异步的，性能优异
- ✅ **参数化查询** - 内置 SQL 注入防护
- ✅ **分页支持** - 开箱即用的分页查询
- ✅ **Dapper ORM** - 轻量级、高性能
- ✅ **连接池** - 支持 MySQL/PostgreSQL 连接池管理
- ✅ **连接字符串安全** - 统一构建连接字符串，内置敏感信息脱敏
- ✅ **.NET 8.0/9.0/10.0** - 支持最新的 .NET 版本

## 安装

```bash
# Package Manager
Install-Package Azrng.DataAccess -Version 1.0.0-beta5

# .NET CLI
dotnet add package Azrng.DataAccess --version 1.0.0-beta5
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
| SQLite | `DatabaseType.Sqlite` | 轻量级嵌入式数据库 |
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

### 动态 SQL 构建模块

`Azrng.DataAccess` 内置 `Azrng.Database.DynamicSqlBuilder` 模块，用于根据查询条件生成参数化 SQL 和 `Dapper.DynamicParameters`。该模块已并入 `Azrng.DataAccess` 包，不再作为独立包发布。

从 `Azrng.Database.DynamicSqlBuilder` 迁移时，只需要安装或引用 `Azrng.DataAccess`，原有命名空间和核心 API 保持不变：

> 方言支持：当前仅 PostgreSQL 方言经过支持与验证。`SqlBuilderOptions.Dialect` 和 `SqlDialectService` 已作为后续扩展点保留，但 MySQL、SQL Server、SQLite、ClickHouse、Oracle 等方言暂未承诺可用。

```csharp
using Azrng.Database.DynamicSqlBuilder;
using Azrng.Database.DynamicSqlBuilder.Model;

var whereClauses = new List<SqlWhereClauseInfoDto>
{
    new("status", new List<FieldValueInfoDto> { new(1) }, MatchOperator.Equal, valueType: typeof(int)),
    new("creator", new List<FieldValueInfoDto> { new("admin") }, MatchOperator.Like)
};

var sortFields = new List<SortFieldDto>
{
    new("create_time", asc: false)
};

var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric(
    tableName: "public.inventory_details",
    necessaryCondition: string.Empty,
    queryResultFields: new List<string> { "product_id", "creator", "create_time" },
    sqlWhereClauses: whereClauses,
    pageIndex: 1,
    pageSize: 20,
    sortFields: sortFields);

var rows = await dbHelper.QueryAsync<InventoryDetail>(sql, parameters);
```

当前 PostgreSQL 方言行为：

- `IN` 使用 `field = ANY(@parameter)`。
- `NOT IN` 使用 `field != ALL(@parameter)`。
- 分页使用 `LIMIT ... OFFSET ...`。
- 参数值全部通过 `DynamicParameters` 传递，字段名、表名、排序字段默认启用格式校验。

后续扩展其他数据库方言时，应在 `SqlDialectService` 中补齐并验证以下差异：

- `IN/NOT IN` 集合参数展开或数组参数策略。
- 分页语法与排序要求。
- 参数名前缀及 Dapper Provider 兼容性。
- `LIKE` 转义字符和转义子句。

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

### 连接字符串安全

`DataSourceConnectionStringBuilder` 统一负责连接字符串的构建与脱敏，避免各 DbHelper 分散拼接带来的安全隐患。

```csharp
// 使用 DataSourceConfig 构建连接字符串（推荐）
var connectionString = DataSourceConnectionStringBuilder.Build(DatabaseType.MySql, config);

// 脱敏连接字符串（用于日志输出等场景）
var masked = DataSourceConnectionStringBuilder.MaskConnectionString(connectionString);
// 输出示例: "Server=localhost;Database=mydb;User Id=***;Password=***;..."

// 通过 IDbHelper 扩展方法获取脱敏连接字符串
var maskedFromHelper = dbHelper.GetMaskedConnectionString();

// DbHelperBase 派生类也暴露了 MaskedConnectionString 属性
var maskedProperty = dbHelper.MaskedConnectionString;
```

**支持脱敏的字段**：`Password`、`Pwd`、`User Id`、`UserID`、`Username`、`User`、`UID`。

> 注意：直接传入明文连接字符串的构造方式（`new MySqlDbHelper(connectionString)`）仍然保留用于兼容，但运行时会持有完整连接字符串。推荐使用 `DataSourceConfig` 构造方式。

## 版本历史

### 1.0.0-beta5

- 合并 `Azrng.Database.DynamicSqlBuilder` 为 `Azrng.DataAccess` 内置动态 SQL 构建模块
- 保留 `Azrng.Database.DynamicSqlBuilder` 命名空间和 `DynamicSqlBuilderHelper` 等核心 API，降低迁移成本
- 补充动态 SQL 构建模块的 README 使用示例、迁移说明和方言支持边界
- 当前动态 SQL 构建模块仅声明支持并验证 PostgreSQL 方言，其他数据库方言保留扩展点但暂未承诺可用

### 1.0.0-beta4

- 新增 `DataSourceConnectionStringBuilder`，统一 6 种数据库的连接字符串构建逻辑
- 新增 `MaskConnectionString` 脱敏方法，支持日志安全输出
- 新增 `GetMaskedConnectionString()` 扩展方法与 `DbHelperBase.MaskedConnectionString` 属性
- 修正 `PostgresSql` 构建器 `PersistSecurityInfo` 为 `false`（增强安全性）

### 1.0.0-beta3

- 新增数据库名列表查询接口 `GetDatabaseNameListAsync`
- 新增表时间戳查询接口 `GetTableTimestampAsync`
- 补充 MySQL、SQL Server、PostgreSQL、Oracle 的数据库列表元数据查询支持
- 补充 MySQL、SQL Server 的表创建时间和修改时间查询支持

### 1.0.0-beta2

- 添加MySQL视图和函数操作

### 1.0.0-beta1

-  支持 6 种主流数据库（MySQL、SQL Server、PostgreSQL、SQLite、ClickHouse、Oracle）
-  工厂模式创建数据库桥接
-  分页查询功能
-  参数化查询（防 SQL 注入）
-  异步操作
-  事务支持
-  连接池管理（MySQL/PostgreSQL）
-  基于 Dapper ORM
-  支持 .NET 8.0/9.0/10.0

## License

MIT License
