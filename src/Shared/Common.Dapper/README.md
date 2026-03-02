## Common.Dapper

这是一个基于 Dapper 封装的数据库访问库，提供了常用的数据库操作方法，支持同步和异步操作，并内置了事务处理和分页查询等功能。

### 功能特性

- 支持多框架：.NET 6.0 / 7.0 / 8.0 / 9.0 / 10.0
- 基于 Dapper 的轻量级 ORM 封装
- 支持同步和异步操作
- 内置事务处理支持
- 支持分页查询
- 支持多结果集查询
- 支持批量操作
- 支持自定义命令超时配置
- 支持查询时输出 SQL 和执行结果（通过日志）

### 安装

通过 NuGet 安装:

```
Install-Package Common.Dapper
```

或通过 .NET CLI:

```
dotnet add package Common.Dapper
```

### 使用方法

#### 基本配置

在 `Program.cs` 或 `Startup.cs` 中注册服务：

```csharp
// 基本配置
builder.Services.AddDapper();

// 自定义配置（设置默认命令超时时间）
builder.Services.AddDapper(options =>
{
    options.DefaultCommandTimeout = 30; // 30秒
});

// 注入数据库连接（以 PostgreSQL 为例）
var connectionString = "Host=localhost;Database=mydb;Username=postgres;Password=password";
builder.Services.AddScoped<System.Data.IDbConnection>(_ => new Npgsql.NpgsqlConnection(connectionString));
```

**注意**: 需要根据使用的数据库安装对应的驱动包，例如：
- PostgreSQL: `Npgsql`
- MySQL: `MySqlConnector`
- SQL Server: `Microsoft.Data.SqlClient`
- SQLite: `Microsoft.Data.Sqlite`

#### 在服务中使用

注入 `IDapperRepository` 接口并在代码中使用：

```csharp
public class UserService
{
    private readonly IDapperRepository _dapperRepository;

    public UserService(IDapperRepository dapperRepository)
    {
        _dapperRepository = dapperRepository;
    }

    // 查询列表
    public async Task<List<User>> GetUsersAsync()
    {
        string sql = "SELECT * FROM Users WHERE Status = @Status";
        return await _dapperRepository.QueryAsync<User>(sql, new { Status = 1 });
    }

    // 查询单条记录
    public async Task<User?> GetUserByIdAsync(long id)
    {
        string sql = "SELECT * FROM Users WHERE Id = @Id";
        return await _dapperRepository.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    // 插入数据
    public async Task<int> InsertUserAsync(User user)
    {
        string sql = "INSERT INTO Users (Name, Email) VALUES (@Name, @Email)";
        return await _dapperRepository.ExecuteAsync(sql, user);
    }

    // 更新数据
    public async Task<int> UpdateUserAsync(User user)
    {
        string sql = "UPDATE Users SET Name = @Name, Email = @Email WHERE Id = @Id";
        return await _dapperRepository.ExecuteAsync(sql, user);
    }

    // 删除数据
    public async Task<int> DeleteUserAsync(long id)
    {
        string sql = "DELETE FROM Users WHERE Id = @Id";
        return await _dapperRepository.ExecuteAsync(sql, new { Id = id });
    }
}
```

#### 分页查询

```csharp
public async Task<PagedResult<User>> GetUsersPagedAsync(int pageIndex, int pageSize)
{
    int offset = (pageIndex - 1) * pageSize;

    string dataSql = "SELECT * FROM Users ORDER BY Id OFFSET @Offset LIMIT @PageSize";
    string countSql = "SELECT COUNT(*) FROM Users";

    return await _dapperRepository.QueryPagedAsync<User>(
        dataSql,
        countSql,
        new { Offset = offset, PageSize = pageSize }
    );
}
```

#### 事务处理

```csharp
// 无返回值的事务
await _dapperRepository.ExecuteInTransactionAsync(async transaction =>
{
    await _dapperRepository.ExecuteAsyncAsync(
        "UPDATE Accounts SET Balance = Balance - 100 WHERE Id = @Id",
        new { Id = fromAccountId },
        transaction
    );
    await _dapperRepository.ExecuteAsyncAsync(
        "UPDATE Accounts SET Balance = Balance + 100 WHERE Id = @Id",
        new { Id = toAccountId },
        transaction
    );
});

// 有返回值的事务
var result = await _dapperRepository.ExecuteInTransactionAsync(async transaction =>
{
    var result1 = await _dapperRepository.QueryFirstOrDefaultAsync<int>(
        "SELECT Balance FROM Accounts WHERE Id = @Id",
        new { Id = accountId },
        transaction
    );
    return result1;
});
```

#### 批量操作

```csharp
// 批量插入
var users = new List<object>
{
    new { Name = "User1", Email = "user1@example.com" },
    new { Name = "User2", Email = "user2@example.com" },
    new { Name = "User3", Email = "user3@example.com" }
};

string sql = "INSERT INTO Users (Name, Email) VALUES (@Name, @Email)";
int affectedRows = await _dapperRepository.ExecuteBatchAsync(sql, users);
```

#### 多结果集查询

```csharp
// 查询两个结果集
var (users, orders) = await _dapperRepository.QueryMultipleAsync<User, Order>(
    "SELECT * FROM Users; SELECT * FROM Orders;"
);

// 查询三个结果集
var (users, orders, products) = await _dapperRepository.QueryMultipleAsync<User, Order, Product>(
    "SELECT * FROM Users; SELECT * FROM Orders; SELECT * FROM Products;"
);
```

### API 方法列表

#### 查询方法
- `Query<T>` - 查询列表（同步）
- `QueryAsync<T>` - 查询列表（异步）
- `QueryFirstOrDefault<T>` - 查询第一条（同步）
- `QueryFirstOrDefaultAsync<T>` - 查询第一条（异步）
- `QueryMultipleAsync<T1, T2>` - 查询两个结果集
- `QueryMultipleAsync<T1, T2, T3>` - 查询三个结果集
- `QueryPagedAsync<T>` - 分页查询

#### 执行方法
- `Execute` - 执行 SQL（同步）
- `ExecuteAsync` - 执行 SQL（异步）
- `ExecuteBatch` - 批量执行（同步）
- `ExecuteBatchAsync` - 批量执行（异步）
- `ExecuteScalar<T>` - 返回首行首列（同步）
- `ExecuteScalarAsync<T>` - 返回首行首列（异步）

#### 事务方法
- `ExecuteInTransaction` - 在事务中执行操作（同步）
- `ExecuteInTransaction<TResult>` - 在事务中执行操作并返回结果（同步）
- `ExecuteInTransactionAsync` - 在事务中执行操作（异步）
- `ExecuteInTransactionAsync<TResult>` - 在事务中执行操作并返回结果（异步）

### 版本更新记录

* 0.2.0
  * 支持查询的时候输出执行sql以及查询结果
  * 新增分页查询功能 `QueryPagedAsync`
  * 新增事务处理功能 `ExecuteInTransaction`
  * 新增批量操作功能 `ExecuteBatch`
  * 新增多结果集查询功能 `QueryMultipleAsync`
* 0.1.0
  * 支持查询的时候输出执行sql以及查询结果
* 0.0.1
  * 更新命名空间
* 0.0.1-beta1
  * 基础操作
