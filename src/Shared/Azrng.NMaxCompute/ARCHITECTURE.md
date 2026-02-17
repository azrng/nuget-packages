# Azrng.NMaxCompute 项目架构设计文档

## 1. 项目概述

Azrng.NMaxCompute 是一个基于 ADO.NET 接口标准的 MaxCompute 数据访问层库。项目采用**接口与实现分离**的架构设计，定义了标准的 ADO.NET 接口和数据模型，使开发者可以像使用传统数据库一样使用阿里云 MaxCompute。

### 1.1 核心特性

- **ADO.NET 标准**: 完全实现 ADO.NET 接口规范，支持 `DbConnection`、`DbCommand`、`DbDataReader` 等核心接口
- **接口与实现分离**: 仅定义接口和抽象层，具体查询执行由使用者通过 `IQueryExecutor` 接口实现
- **Dapper 支持**: 可与 Dapper ORM 配合使用
- **多种连接方式**: 支持配置对象、连接字符串、参数化构造三种方式创建连接

### 1.2 设计理念

```
┌─────────────────────────────────────────────────────────────┐
│                    应用层 (Application)                       │
│                    (使用 Dapper 或原生 ADO.NET)               │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                 ADO.NET 抽象层 (本库提供)                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │ Connection   │  │   Command    │  │    DataReader    │  │
│  └──────────────┘  └──────────────┘  └──────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│               查询执行器接口 (IQueryExecutor)                  │
│                   (由使用者实现)                               │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                  实际数据源 (MaxCompute)                       │
│            (通过 HTTP/REST API 或 JDBC)                       │
└─────────────────────────────────────────────────────────────┘
```

## 2. 核心组件

### 2.1 IQueryExecutor 接口

**位置**: [Adapter/IQueryExecutor.cs](Adapter/IQueryExecutor.cs)

**职责**: 查询执行器接口，是连接 ADO.NET 层与实际数据源的桥梁。

```csharp
public interface IQueryExecutor
{
    Task<QueryResult> ExecuteQueryAsync(MaxComputeConfig config, string sql, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(MaxComputeConfig config, CancellationToken cancellationToken = default);
}
```

**设计要点**:
- 将查询执行逻辑委托给使用者实现
- 支持异步操作和取消令牌
- 返回统一的 `QueryResult` 数据模型

**典型实现场景**:
- 使用 Python MaxCompute SDK 包装为 HTTP 服务
- 直接调用 MaxCompute REST API
- 通过 JDBC 桥接实现

### 2.2 MaxComputeConnection

**位置**: [MaxComputeConnection.cs](MaxComputeConnection.cs)

**继承关系**: `DbConnection`

**核心职责**:
- 管理 MaxCompute 连接状态
- 创建 `DbCommand` 实例
- 处理连接字符串解析和构建

**关键实现**:

| 属性/方法 | 说明 |
|---------|------|
| `ConnectionString` | 不支持运行时修改 |
| `Database` | 返回配置的 Project 名称 |
| `DataSource` | 返回 ServerUrl |
| `ConnectionTimeout` | 固定为 30 秒 |
| `State` | 连接状态 (Closed/Open) |
| `CreateDbCommand()` | 创建命令实例 |
| `BeginDbTransaction()` | 不支持，抛出 `NotSupportedException` |
| `ChangeDatabase()` | 不支持，抛出 `NotSupportedException` |

**连接字符串格式**:
```
Url=http://endpoint;AccessId=your_id;SecretKey=your_key;JdbcUrl=jdbc://url;Project=project;MaxRows=1000
```

### 2.3 MaxComputeCommand

**位置**: [MaxComputeCommand.cs](MaxComputeCommand.cs)

**继承关系**: `DbCommand`

**核心职责**:
- 执行 SQL 查询
- 处理参数化查询
- 返回不同类型的结果

**关键方法**:

| 方法 | 返回类型 | 说明 |
|------|---------|------|
| `ExecuteNonQuery()` | `int` | 执行非查询命令，返回行数 |
| `ExecuteReader()` | `DbDataReader` | 返回数据读取器 |
| `ExecuteScalar()` | `object?` | 返回首行首列值 |
| `Cancel()` | `void` | 不支持，仅记录日志 |
| `Prepare()` | `void` | 不支持，仅记录日志 |

**参数化查询处理**:
```csharp
private string ProcessParameters()
{
    // 支持 @param、:param、{param} 三种格式
    sql = sql.Replace($"@{paramName}", paramValue);
    sql = sql.Replace($":{paramName}", paramValue);
    sql = sql.Replace($"{{{paramName}}}", paramValue);
}
```

**参数值格式化**:
- `NULL` → `NULL`
- `string` → `'value'` (自动转义单引号)
- `DateTime` → `'yyyy-MM-dd HH:mm:ss'`
- `bool` → `1` 或 `0`
- `byte[]` → `'<BINARY>'`

### 2.4 MaxComputeDataReader

**位置**: [MaxComputeDataReader.cs](MaxComputeDataReader.cs)

**继承关系**: `DbDataReader`

**核心职责**:
- 提供只进、只读的数据流访问
- 实现行遍历和字段访问
- 类型转换和数据读取

**关键特性**:

| 属性/方法 | 说明 |
|----------|------|
| `HasRows` | 是否有数据行 |
| `FieldCount` | 列数量 |
| `Depth` | 固定返回 0 |
| `RecordsAffected` | 固定返回 0 |
| `Read()` | 移动到下一行 |
| `NextResult()` | 固定返回 `false` (不支持多结果集) |
| `GetString/GetInt32/...` | 类型化读取器 |
| `IsDBNull()` | 检查是否为 NULL |

**实现细节**:
- 使用 `_currentRowIndex` 跟踪当前行位置
- 所有值以 `object` 类型存储，通过 `Convert` 类进行类型转换
- 默认数据类型为 `string`

### 2.5 MaxComputeParameter & MaxComputeParameterCollection

**位置**:
- [MaxComputeParameter.cs](MaxComputeParameter.cs)
- [MaxComputeParameterCollection.cs](MaxComputeParameterCollection.cs)

**继承关系**:
- `MaxComputeParameter`: `DbParameter`
- `MaxComputeParameterCollection`: `DbParameterCollection`

**核心职责**:
- 封装查询参数
- 管理参数集合

**MaxComputeParameter 特性**:
- 默认 `DbType` 为 `String`
- 默认 `Direction` 为 `Input`
- 支持 `IsNullable` 设置

**MaxComputeParameterCollection 功能**:
- 支持按名称或索引访问参数
- 提供 `Add(string name, object value)` 便捷方法
- 线程安全的枚举器 (通过 `SyncRoot`)

### 2.6 MaxComputeConnectionFactory

**位置**: [MaxComputeConnectionFactory.cs](MaxComputeConnectionFactory.cs)

**核心职责**: 提供多种方式创建连接实例

**重载方法**:

```csharp
// 方式 1: 使用配置对象
CreateConnection(IQueryExecutor queryExecutor, MaxComputeConfig config)

// 方式 2: 使用连接字符串
CreateConnection(IQueryExecutor queryExecutor, string connectionString)

// 方式 3: 使用参数
CreateConnection(IQueryExecutor queryExecutor, string url, string accessId,
                string secretKey, string jdbcUrl, string? project, int maxRows)
```

## 3. 数据模型

### 3.1 MaxComputeConfig

**位置**: [Models/MaxComputeConfig.cs](Models/MaxComputeConfig.cs)

**属性**:

| 属性 | 类型 | 必需 | 说明 |
|------|------|:----:|------|
| `ServerUrl` | `string` | ✓ | REST API 地址 |
| `AccessId` | `string` | ✓ | 阿里云 Access ID |
| `SecretKey` | `string` | ✓ | 阿里云 Secret Key |
| `JdbcUrl` | `string` | ✓ | JDBC 连接字符串 |
| `MaxRows` | `int` | | 最大返回行数，默认 1000 |
| `Project` | `string?` | | 项目名称 |

**验证方法**:
```csharp
public virtual bool IsValid()
{
    return !string.IsNullOrWhiteSpace(ServerUrl) &&
           !string.IsNullOrWhiteSpace(AccessId) &&
           !string.IsNullOrWhiteSpace(SecretKey) &&
           !string.IsNullOrWhiteSpace(JdbcUrl);
}
```

### 3.2 QueryResult

**位置**: [Models/QueryResult.cs](Models/QueryResult.cs)

**属性**:

| 属性 | 类型 | 说明 |
|------|------|------|
| `Columns` | `string[]` | 列名数组 |
| `Rows` | `object[][]` | 行数据二维数组 |
| `RowCount` | `int` | 行数 |
| `ExecutionTime` | `string?` | 执行耗时 |

**数据结构示例**:
```json
{
  "Columns": ["id", "name", "age"],
  "Rows": [
    [1, "Alice", 25],
    [2, "Bob", 30]
  ],
  "RowCount": 2
}
```

### 3.3 QueryResponse<T>

**位置**: [Models/QueryResult.cs](Models/QueryResult.cs)

**用途**: 统一响应包装器

```csharp
public class QueryResponse<T>
{
    public string Status { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}
```

### 3.4 适配器层模型 (Adapter.Ho)

**位置**: [Adapter/Ho/QuerySingleSqlHo.cs](Adapter/Ho/QuerySingleSqlHo.cs)

**QuerySqlBase**: 查询基础配置类
- `Url`: 接口地址
- `AccessId`: 访问 ID (JSON 序列化为 `access_id`)
- `SecretKey`: 密钥 (JSON 序列化为 `secret_key`)
- `JdbcUrl`: JDBC URL (JSON 序列化为 `jdbc_url`)
- `MaxRows`: 最大行数 (JSON 序列化为 `max_rows`)

**QuerySingleSqlRequestHo**: 单 SQL 查询请求
- 继承自 `QuerySqlBase`
- 添加 `Sql` 属性 (JSON 序列化为 `sql`)

## 4. 工作流程

### 4.1 典型查询流程

```
┌──────────────────────────────────────────────────────────────────┐
│ 1. 创建连接                                                        │
│    MaxComputeConnectionFactory.CreateConnection(executor, config) │
└──────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────┐
│ 2. 打开连接                                                        │
│    await connection.OpenAsync()                                   │
│    ├─ 验证配置                                                     │
│    └─ 调用 executor.TestConnectionAsync()                         │
└──────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────┐
│ 3. 创建命令                                                        │
│    var command = connection.CreateCommand()                       │
│    command.CommandText = "SELECT * FROM users"                    │
└──────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────┐
│ 4. 执行查询                                                        │
│    var reader = await command.ExecuteReaderAsync()                │
│    ├─ 处理参数 (ProcessParameters)                                │
│    └─ 调用 executor.ExecuteQueryAsync()                           │
└──────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────┐
│ 5. 读取数据                                                        │
│    while (await reader.ReadAsync())                               │
│    {                                                              │
│        var id = reader.GetInt32(0);                               │
│        var name = reader.GetString(1);                            │
│    }                                                              │
└──────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────┐
│ 6. 清理资源                                                        │
│    await reader.DisposeAsync()                                    │
│    await connection.CloseAsync()                                  │
└──────────────────────────────────────────────────────────────────┘
```

### 4.2 Dapper 集成流程

```csharp
// 1. 创建连接
using var connection = MaxComputeConnectionFactory.CreateConnection(executor, config);
await connection.OpenAsync();

// 2. 使用 Dapper 查询
var users = await connection.QueryAsync<User>(
    "SELECT * FROM users WHERE age > @minAge",
    new { minAge = 18 }
);

// Dapper 内部流程:
// - 获取 DbCommand
// - 添加参数
// - 执行 ExecuteReader
// - 读取并映射数据
```

### 4.3 参数化查询处理流程

```
SQL: "SELECT * FROM users WHERE age > @minAge AND name = @name"
参数: { minAge: 18, name: "O'Brien" }

         │
         ▼
┌─────────────────────────────────────────┐
│ 1. 遍历参数集合                          │
│    foreach (parameter in _parameters)   │
└─────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│ 2. 格式化参数值                          │
│    FormatParameterValue(18) → "18"       │
│    FormatParameterValue("O'Brien")      │
│       → "'O''Brien'"                    │
└─────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│ 3. 替换 SQL 中的参数占位符               │
│    "@minAge" → "18"                     │
│    "@name" → "'O''Brien'"               │
└─────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│ 4. 生成最终 SQL                          │
│    "SELECT * FROM users WHERE age > 18  │
│     AND name = 'O''Brien'"              │
└─────────────────────────────────────────┘
```

## 5. 架构设计原则

### 5.1 依赖倒置原则 (DIP)

- 高层模块（ADO.NET 实现）不依赖低层模块（具体查询实现）
- 两者都依赖抽象（`IQueryExecutor` 接口）

### 5.2 开闭原则 (OCP)

- 对扩展开放：使用者可通过实现 `IQueryExecutor` 扩展功能
- 对修改封闭：核心 ADO.NET 实现无需修改

### 5.3 单一职责原则 (SRP)

| 类 | 单一职责 |
|----|---------|
| `MaxComputeConnection` | 连接管理 |
| `MaxComputeCommand` | 命令执行 |
| `MaxComputeDataReader` | 数据读取 |
| `MaxComputeParameterCollection` | 参数管理 |
| `MaxComputeConnectionFactory` | 对象创建 |

### 5.4 接口隔离原则 (ISP)

- `IQueryExecutor` 仅定义两个核心方法
- ADO.NET 基类提供完整的标准接口

## 6. 扩展性设计

### 6.1 自定义查询执行器

使用者可以实现 `IQueryExecutor` 来支持不同的查询场景：

**场景 1: HTTP 服务包装**
```csharp
public class HttpQueryExecutor : IQueryExecutor
{
    private readonly IHttpClientFactory _httpClientFactory;

    public async Task<QueryResult> ExecuteQueryAsync(MaxComputeConfig config, string sql, CancellationToken ct)
    {
        // 调用内部 HTTP 服务
        var response = await _httpClientFactory.CreateClient()
            .PostAsync(config.Url + "/api/execute", new { sql }, ct);

        return await response.Content.ReadFromJsonAsync<QueryResult>(ct);
    }
}
```

**场景 2: 直接调用 REST API**
```csharp
public class RestApiExecutor : IQueryExecutor
{
    public async Task<QueryResult> ExecuteQueryAsync(MaxComputeConfig config, string sql, CancellationToken ct)
    {
        // 直接调用 MaxCompute REST API
        // 参考: https://help.aliyun.com/zh/maxcompute/developer-reference/api-odps-sql
    }
}
```

**场景 3: JDBC 桥接**
```csharp
public class JdbcBridgeExecutor : IQueryExecutor
{
    public async Task<QueryResult> ExecuteQueryAsync(MaxComputeConfig config, string sql, CancellationToken ct)
    {
        // 通过 JDBC 或 ODBC 桥接
    }
}
```

### 6.2 配置扩展

`MaxComputeConfig.IsValid()` 是虚方法，允许子类扩展验证逻辑：

```csharp
public class CustomMaxComputeConfig : MaxComputeConfig
{
    public string Region { get; set; }

    public override bool IsValid()
    {
        return base.IsValid() && !string.IsNullOrWhiteSpace(Region);
    }
}
```

## 7. 限制与注意事项

### 7.1 ADO.NET 限制

| 特性 | 状态 | 说明 |
|------|------|------|
| 事务 (Transaction) | ❌ 不支持 | MaxCompute REST API 不支持事务 |
| 更改数据库 (ChangeDatabase) | ❌ 不支持 | 需创建新连接切换项目 |
| 多结果集 (NextResult) | ❌ 不支持 | 始终返回 false |
| 存储过程 | ❌ 不支持 | MaxCompute 不支持存储过程 |
| 异步取消 (Cancel) | ❌ 不支持 | 仅记录警告日志 |
| 预编译语句 (Prepare) | ❌ 不支持 | 仅记录警告日志 |

### 7.2 安全考虑

**SQL 注入风险**:
- 当前参数化查询使用字符串替换
- 存在潜在 SQL 注入风险
- 建议：在 `IQueryExecutor` 实现层进行参数化处理

**敏感信息**:
- `AccessId` 和 `SecretKey` 是敏感信息
- 连接字符串可能包含明文凭据
- 建议：使用配置管理工具（如 Azure Key Vault）管理凭据

### 7.3 性能考虑

- 所有数据一次性加载到内存（`object[][]`）
- 不支持流式读取大结果集
- `MaxRows` 配置可用于限制返回行数

## 8. 使用示例

### 8.1 基本使用

```csharp
// 1. 实现查询执行器
public class MyQueryExecutor : IQueryExecutor
{
    public async Task<QueryResult> ExecuteQueryAsync(MaxComputeConfig config, string sql, CancellationToken ct)
    {
        // 实现查询逻辑
    }

    public async Task<bool> TestConnectionAsync(MaxComputeConfig config, CancellationToken ct)
    {
        // 实现连接测试
    }
}

// 2. 注册服务
builder.Services.AddScoped<IQueryExecutor, MyQueryExecutor>();

// 3. 使用连接
var config = new MaxComputeConfig
{
    ServerUrl = "http://endpoint",
    AccessId = "your_id",
    SecretKey = "your_key",
    JdbcUrl = "jdbc://url"
};

using var connection = MaxComputeConnectionFactory.CreateConnection(_executor, config);
await connection.OpenAsync();

var users = await connection.QueryAsync<User>("SELECT * FROM users");
```

### 8.2 ASP.NET Core 集成

```csharp
// Program.cs
builder.Services.AddScoped<IQueryExecutor, HttpQueryExecutor>();
builder.Services.AddScoped(sp =>
{
    var executor = sp.GetRequiredService<IQueryExecutor>();
    var config = sp.GetRequiredService<IConfiguration>()
        .GetSection("MaxCompute").Get<MaxComputeConfig>();
    return MaxComputeConnectionFactory.CreateConnection(executor, config);
});

// appsettings.json
{
  "MaxCompute": {
    "ServerUrl": "http://endpoint",
    "AccessId": "your_id",
    "SecretKey": "your_key",
    "JdbcUrl": "jdbc://url",
    "Project": "my_project",
    "MaxRows": 1000
  }
}
```

## 9. 目录结构

```
Azrng.NMaxCompute/
├── Adapter/                    # 适配器层
│   ├── IQueryExecutor.cs      # 查询执行器接口
│   └── Ho/                    # Handle Object (DTO)
│       └── QuerySingleSqlHo.cs
├── Models/                     # 数据模型
│   ├── MaxComputeConfig.cs    # 配置模型
│   └── QueryResult.cs         # 查询结果模型
├── MaxComputeConnection.cs     # 连接类
├── MaxComputeCommand.cs        # 命令类
├── MaxComputeDataReader.cs     # 数据读取器
├── MaxComputeParameter.cs      # 参数类
├── MaxComputeParameterCollection.cs  # 参数集合
├── MaxComputeConnectionFactory.cs    # 连接工厂
└── MaxComputeDapperExtensions.cs      # Dapper 扩展（已注释）
```

## 10. 依赖关系图

```
┌─────────────────────────────────────────────────────────────────┐
│                         System.Data.Common                      │
│  (DbConnection, DbCommand, DbDataReader, DbParameter, ...)      │
└─────────────────────────────────────────────────────────────────┘
                              ▲
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
┌───────┴────────┐  ┌─────────┴─────────┐  ┌───────┴────────┐
│ MaxCompute     │  │ MaxCompute        │  │ MaxCompute     │
│ Connection     │  │ Command           │  │ DataReader     │
└───────┬────────┘  └─────────┬─────────┘  └───────┬────────┘
        │                     │                     │
        └─────────────────────┼─────────────────────┘
                              │
                              ▼
                    ┌─────────────────────┐
                    │ MaxComputeConfig    │
                    │ QueryResult         │
                    └─────────────────────┘
                              │
                              ▼
                    ┌─────────────────────┐
                    │  IQueryExecutor     │
                    │ (由使用者实现)       │
                    └─────────────────────┘
```

## 11. 总结

Azrng.NMaxCompute 采用了清晰的分层架构设计：

1. **接口层**: 定义 `IQueryExecutor` 抽象查询执行
2. **ADO.NET 层**: 实现标准 ADO.NET 接口
3. **模型层**: 提供配置和数据传输对象
4. **工厂层**: 统一对象创建逻辑

这种设计实现了高度的灵活性和可扩展性，使开发者能够根据实际需求选择合适的 MaxCompute 访问方式，同时享受标准 ADO.NET 接口带来的便利性。
