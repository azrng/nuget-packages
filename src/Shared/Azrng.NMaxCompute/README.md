# NMaxCompute

## 简介

NMaxCompute 是一个通用的 MaxCompute ADO.NET 提供程序**接口定义包**，采用接口与实现分离的架构设计。它定义了标准的 ADO.NET 接口和数据模型，支持 Dapper，可以像使用传统数据库一样使用 MaxCompute。

**注意**：NMaxCompute 只定义接口，具体实现由使用者（如 MaxComputeConsole）提供。

## 架构设计

```
NMaxCompute (接口层)          使用者实现层 (MaxComputeConsole)
├── IQueryExecutor            ├── HttpQueryExecutor (实现 IQueryExecutor)
├── MaxComputeConfig          ├── MaxComputeAdapter (HTTP 调用)
├── MaxComputeConnection      └── 具体业务逻辑
├── MaxComputeCommand
├── MaxComputeDataReader
└── MaxComputeParameter
```

## 核心概念

### 1. IQueryExecutor 接口

核心查询接口，由使用者实现：

```csharp
public interface IQueryExecutor
{
    Task<QueryResult> ExecuteQueryAsync(MaxComputeConfig config, string sql, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(MaxComputeConfig config, CancellationToken cancellationToken = default);
}
```

### 2. MaxComputeConfig

配置模型，包含连接所需的所有信息：

```csharp
public class MaxComputeConfig
{
    public string Url { get; set; }        // REST API 地址
    public string AccessId { get; set; }   // Access ID
    public string SecretKey { get; set; }  // Secret Key
    public string JdbcUrl { get; set; }    // JDBC URL
    public int MaxRows { get; set; }       // 最大返回行数
    public string? Project { get; set; }   // 项目名称
}
```

### 3. ADO.NET 实现

完整的 ADO.NET 基类继承：
- `MaxComputeConnection` : `DbConnection`
- `MaxComputeCommand` : `DbCommand`
- `MaxComputeDataReader` : `DbDataReader`
- `MaxComputeParameter` : `DbParameter`

## 快速开始

### 步骤 1：实现 IQueryExecutor

在你的项目中实现查询执行器：

```csharp
using System.Text;
using Microsoft.Extensions.Logging;
using NMaxCompute.Models;

public class HttpQueryExecutor : IQueryExecutor
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpQueryExecutor> _logger;

    public HttpQueryExecutor(IHttpClientFactory httpClientFactory, ILogger<HttpQueryExecutor> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<QueryResult> ExecuteQueryAsync(MaxComputeConfig config, string sql, CancellationToken cancellationToken = default)
    {
        using var client = _httpClientFactory.CreateClient();
        var url = config.Url.TrimEnd('/') + "/api/sql/execute";

        // 构建请求
        var request = new
        {
            access_id = config.AccessId,
            secret_key = config.SecretKey,
            jdbc_url = config.JdbcUrl,
            max_rows = config.MaxRows,
            sql = sql
        };

        var json = JsonConvert.SerializeObject(request);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        // 发送请求
        var response = await client.PostAsync(url, content, cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        // 解析响应
        var result = JsonConvert.DeserializeObject<QueryResponse<QueryResult>>(responseString);
        return result.Data;
    }

    public async Task<bool> TestConnectionAsync(MaxComputeConfig config, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await ExecuteQueryAsync(config, "SELECT 1", cancellationToken);
            return result.RowCount > 0;
        }
        catch
        {
            return false;
        }
    }
}
```

### 步骤 2：注册服务

```csharp
// Program.cs
using Microsoft.Extensions.DependencyInjection;
using NMaxCompute;

var builder = WebApplication.CreateBuilder(args);

// 添加 HttpClient
builder.Services.AddHttpClient();

// 注册查询执行器
builder.Services.AddScoped<IQueryExecutor, HttpQueryExecutor>();

var app = builder.Build();
app.Run();
```

### 步骤 3：创建连接

```csharp
public class MyService
{
    private readonly IQueryExecutor _queryExecutor;

    public MyService(IQueryExecutor queryExecutor)
    {
        _queryExecutor = queryExecutor;
    }

    public async Task QueryDataAsync()
    {
        // 创建配置
        var config = new MaxComputeConfig
        {
            Url = "http://your-maxcompute-endpoint",
            AccessId = "your_access_id",
            SecretKey = "your_secret_key",
            JdbcUrl = "your_jdbc_url",
            Project = "your_project",
            MaxRows = 1000
        };

        // 创建连接
        using var connection = MaxComputeConnectionFactory.CreateConnection(_queryExecutor, config);
        await connection.OpenAsync();

        // 查询数据
        var sql = "SELECT * FROM users WHERE age > 18 LIMIT 10";
        var users = await connection.QueryAsync<User>(sql);

        foreach (var user in users)
        {
            Console.WriteLine($"User: {user.Name}, Age: {user.Age}");
        }
    }
}
```

## 使用方式

### 方式 1：使用配置对象

```csharp
var config = new MaxComputeConfig
{
    Url = "http://mc-job-endpoint",
    AccessId = "LTAI5txxx",
    SecretKey = "xxx",
    JdbcUrl = "jdbc:odps://service.cn-shanghai.maxcompute.aliyun.com/api?project=xxx",
    Project = "my_project",
    MaxRows = 1000
};

using var connection = MaxComputeConnectionFactory.CreateConnection(_queryExecutor, config);
await connection.OpenAsync();
```

### 方式 2：使用连接字符串

```csharp
var connectionString = "Url=http://endpoint;" +
                       "AccessId=your_id;" +
                       "SecretKey=your_key;" +
                       "JdbcUrl=jdbc://url;" +
                       "Project=my_project;" +
                       "MaxRows=1000";

using var connection = MaxComputeConnectionFactory.CreateConnection(_queryExecutor, connectionString);
await connection.OpenAsync();
```

### 方式 3：使用参数

```csharp
using var connection = MaxComputeConnectionFactory.CreateConnection(
    _queryExecutor,
    url: "http://endpoint",
    accessId: "your_id",
    secretKey: "your_key",
    jdbcUrl: "jdbc://url",
    project: "my_project",
    maxRows: 1000
);
await connection.OpenAsync();
```

## 使用 Dapper

### 异步查询

```csharp
using Dapper;

// 查询列表
var users = await connection.QueryAsync<User>(
    "SELECT * FROM users WHERE age > @minAge",
    new { minAge = 18 }
);

// 查询单个对象
var user = await connection.QueryFirstOrDefaultAsync<User>(
    "SELECT * FROM users WHERE id = @id",
    new { id = 1 }
);

// 执行命令
var affectedRows = await connection.ExecuteAsync(
    "UPDATE users SET status = @status WHERE id = @id",
    new { status = "active", id = 1 }
);

// 查询标量值
var count = await connection.ExecuteScalarAsync<int>(
    "SELECT COUNT(*) FROM users WHERE status = @status",
    new { status = "active" }
);
```

### 同步查询

```csharp
// 查询列表
var users = connection.Query<User>(
    "SELECT * FROM users WHERE age > @minAge",
    new { minAge = 18 }
);

// 查询单个对象
var user = connection.QueryFirstOrDefault<User>(
    "SELECT * FROM users WHERE id = @id",
    new { id = 1 }
);
```

## 使用原生 ADO.NET API

### 创建命令

```csharp
await using var cmd = connection.CreateCommand();
cmd.CommandText = "SELECT * FROM users WHERE age > @minAge";

// 添加参数
var param = cmd.CreateParameter();
param.ParameterName = "minAge";
param.Value = 18;
cmd.Parameters.Add(param);

// 或者使用参数化对象
cmd.CommandText = "SELECT * FROM users WHERE age > @minAge AND status = @status";
cmd.Parameters.Add(new MaxComputeParameter { ParameterName = "minAge", Value = 18 });
cmd.Parameters.Add(new MaxComputeParameter { ParameterName = "status", Value = "active" });
```

### 执行查询

```csharp
// ExecuteReader
await using var reader = await cmd.ExecuteReaderAsync();
while (await reader.ReadAsync())
{
    var name = reader.GetString("name");
    var age = reader.GetInt32("age");
    Console.WriteLine($"User: {name}, Age: {age}");
}

// ExecuteScalar
var count = await cmd.ExecuteScalarAsync();
Console.WriteLine($"Total: {count}");

// ExecuteNonQuery
var affectedRows = await cmd.ExecuteNonQueryAsync();
Console.WriteLine($"Affected rows: {affectedRows}");
```

## 在 ASP.NET Core 中使用

### 注册服务

```csharp
// Program.cs
builder.Services.AddHttpClient();
builder.Services.AddScoped<IQueryExecutor, HttpQueryExecutor>();

// 注册连接
builder.Services.AddScoped(sp =>
{
    var executor = sp.GetRequiredService<IQueryExecutor>();
    var config = new MaxComputeConfig
    {
        Url = builder.Configuration["MaxCompute:Url"],
        AccessId = builder.Configuration["MaxCompute:AccessId"],
        SecretKey = builder.Configuration["MaxCompute:SecretKey"],
        JdbcUrl = builder.Configuration["MaxCompute:JdbcUrl"],
        Project = builder.Configuration["MaxCompute:Project"]
    };
    return MaxComputeConnectionFactory.CreateConnection(executor, config);
});
```

### 配置文件

```json
{
  "MaxCompute": {
    "Url": "http://your-endpoint",
    "AccessId": "your_access_id",
    "SecretKey": "your_secret_key",
    "JdbcUrl": "jdbc:odps://service.cn-shanghai.maxcompute.aliyun.com/api?project=my_project",
    "Project": "my_project",
    "MaxRows": 1000
  }
}
```

### 使用连接

```csharp
public class UserService
{
    private readonly MaxComputeConnection _connection;

    public UserService(MaxComputeConnection connection)
    {
        _connection = connection;
    }

    public async Task<List<User>> GetUsersAsync(int minAge)
    {
        var sql = "SELECT * FROM users WHERE age > @minAge LIMIT 100";
        return (await _connection.QueryAsync<User>(sql, new { minAge })).ToList();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        var sql = "SELECT * FROM users WHERE id = @id";
        return await _connection.QueryFirstOrDefaultAsync<User>(sql, new { id });
    }
}
```

## 连接字符串格式

```
Url=http://your-endpoint;AccessId=your_id;SecretKey=your_key;JdbcUrl=jdbc://url;Project=project;MaxRows=1000
```

### 参数说明

| 参数 | 类型 | 必需 | 说明 |
|------|------|------|------|
| Url | string | 是 | MaxCompute REST API 地址 |
| AccessId | string | 是 | 阿里云 Access ID |
| SecretKey | string | 是 | 阿里云 Secret Key |
| JdbcUrl | string | 是 | JDBC 连接字符串 |
| Project | string | 否 | 项目名称 |
| MaxRows | int | 否 | 最大返回行数，默认 1000 |

## 实体类示例

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

## 支持的方法

### MaxComputeConnection
- `Open()` / `OpenAsync(CancellationToken)` - 打开连接
- `Close()` - 关闭连接
- `CreateCommand()` - 创建命令
- `Dispose()` - 释放资源

### MaxComputeCommand
- `ExecuteReader()` / `ExecuteReaderAsync()` - 执行查询并返回 DataReader
- `ExecuteNonQuery()` / `ExecuteNonQueryAsync()` - 执行非查询 SQL
- `ExecuteScalar()` / `ExecuteScalarAsync()` - 执行查询并返回单个值
- `CreateParameter()` - 创建参数

### MaxComputeDataReader
- `Read()` / `ReadAsync()` - 读取下一行
- `GetString()` / `GetInt32()` / `GetDateTime()` 等 - 获取列值
- `GetName()` / `GetOrdinal()` - 获取列信息
- `FieldCount` - 列数
- `HasRows` - 是否有行
- `Close()` - 关闭读取器

## 高级用法

### 实现不同的查询执行器

你可以实现不同的 `IQueryExecutor` 来适应不同的场景：

```csharp
// 内存测试实现
public class InMemoryQueryExecutor : IQueryExecutor
{
    public async Task<QueryResult> ExecuteQueryAsync(MaxComputeConfig config, string sql, CancellationToken cancellationToken = default)
    {
        // 返回测试数据
        return new QueryResult
        {
            Columns = new[] { "id", "name" },
            Rows = new object[][] { new object[] { 1, "Test User" } },
            RowCount = 1
        };
    }

    public async Task<bool> TestConnectionAsync(MaxComputeConfig config, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(true);
    }
}

// 在测试中使用
builder.Services.AddScoped<IQueryExecutor, InMemoryQueryExecutor>();
```

### 批量操作

实现 `IBatchQueryExecutor` 接口支持批量操作：

```csharp
public interface IBatchQueryExecutor
{
    Task<BatchQueryResult> ExecuteBatchAsync(
        MaxComputeConfig config,
        IEnumerable<string> sqlStatements,
        CancellationToken cancellationToken = default);
}
```

## 注意事项

1. **架构设计**：NMaxCompute 只定义接口，具体实现由使用者提供
2. **事务支持**：MaxCompute REST API 不支持事务，所有事务操作都会抛出 NotSupportedException
3. **参数化查询**：当前使用字符串替换，请注意 SQL 注入风险
4. **ChangeDatabase**：不支持，请创建新连接来访问不同的项目
5. **多结果集**：`NextResult()` 始终返回 false，因为 MaxCompute REST API 不支持多个结果集
6. **异步操作**：所有异步方法都支持 `CancellationToken`

## 优势

1. **接口与实现分离** - NMaxCompute 只定义接口，使用者根据需求实现
2. **灵活性** - 可以实现不同的查询执行器（HTTP、gRPC、内存测试等）
3. **可测试性** - 轻松模拟 `IQueryExecutor` 进行单元测试
4. **兼容性** - 完整的 ADO.NET 实现，支持 Dapper 和其他 ORM
5. **可维护性** - 接口定义稳定，实现可以独立演进

## 更多信息

详细的架构设计说明请查看 [ARCHITECTURE.md](ARCHITECTURE.md)

## License

待定
