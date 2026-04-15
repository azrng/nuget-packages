# DevLogDashboard

**DevLogDashboard** 是一个面向 .NET 开发者的轻量级日志查看 NuGet 包，专为 API 项目在开发调试阶段提供便捷的日志面板界面。

## 特点

- **零配置**: 默认使用内存存储，无需数据库等外部依赖，开箱即用
- **可扩展存储**: 支持通过实现 `ILogStore` 接口自定义存储方案（如 PostgreSQL、Redis、文件等）
- **后台批量写入**: 默认使用后台队列批量写入日志，不阻塞业务线程，性能更优
- **实时查看**: 开发调试阶段快速查看日志
- **请求追踪**: 通过 RequestId 查看完整请求链路
- **结构化日志**: 支持记录和查看结构化数据
- **HTTP 上下文捕获**: 自动记录 RequestId、ConnectionId、RequestPath、RequestMethod、ResponseStatusCode 等请求信息
- **时间倒序显示**: 日志默认按时间倒序显示，最新的日志在前

## 快速开始

### 1. 安装 NuGet 包

```bash
dotnet add package Azrng.DevLogDashboard
```

### 2. 配置服务

在 `Program.cs` 中添加：

```csharp
using Azrng.DevLogDashboard.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 添加 DevLogDashboard 服务（默认使用内存存储）
builder.Services.AddDevLogDashboard(options =>
{
    options.EndpointPath = "/dev-logs";      // 仪表板访问路径
    options.MaxLogCount = 10000;             // 最大存储日志数（仅内存存储）
    options.ApplicationName = "MyApi";       // 应用名称
    options.ApplicationVersion = "1.0.0";    // 应用版本
});

var app = builder.Build();

// 使用 DevLogDashboard（推荐仅在开发环境使用）
if (app.Environment.IsDevelopment())
{
    app.UseDevLogDashboard();
}

app.Run();
```

### 3. 使用日志

```csharp
[ApiController]
[Route("api/[controller]")]
public class ValuesController : ControllerBase
{
    private readonly ILogger<ValuesController> _logger;

    public ValuesController(ILogger<ValuesController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        // 普通日志
        _logger.LogInformation("获取数据列表");

        // 结构化日志
        var data = new { Id = 1, Name = "Test" };
        _logger.LogInformation("返回数据：{@Data}", data);

        return Ok(new[] { "value1", "value2" });
    }

    [HttpGet("error")]
    public IActionResult Error()
    {
        try
        {
            throw new InvalidOperationException("测试错误");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发生异常");
            return StatusCode(500);
        }
    }
}
```

### 4. 访问仪表板

启动应用后访问：`http://localhost:{port}/dev-logs`

## 配置选项

### 基础配置

| 选项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `EndpointPath` | string | `/dev-logs` | 仪表板访问路径 |
| `MaxLogCount` | int | 10000 | 最大存储日志条数（仅内存存储） |
| `OnlyLogErrors` | bool | false | 是否只记录错误日志 |
| `MinLogLevel` | LogLevel | Trace | 最低日志级别 |
| `BasicAuthentication` | DevLogDashboardBasicAuthenticationOptions | null | 可选的 Basic 认证配置，配置后会按指定 scheme 执行认证 |
| `ApplicationName` | string | - | 应用名称 |
| `ApplicationVersion` | string | - | 应用版本 |
| `IgnoredPaths` | ICollection<string> | [/health, /healthz, /ready, /metrics, /dev-logs, /favicon.ico] | 忽略的路径 |

### 授权配置

| 选项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `BasicAuthentication` | DevLogDashboardBasicAuthenticationOptions | null | 配置后启用内置 Basic 认证；未配置时允许匿名访问 |

### BasicAuthentication 配置

| 选项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `UserName` | string | 空 | Basic 认证用户名 |
| `Password` | string | 空 | Basic 认证密码 |
| `Realm` | string | `DevLogDashboard` | 未认证时返回的 Basic realm |

### 后台写入配置

以下配置通过 `AddDevLogDashboard<TLogStore>(configureOptions, configureBackgroundOptions)` 重载设置：

| 选项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `BatchSize` | int | 100 | 每次批量写入的日志条数 |
| `PollInterval` | TimeSpan | 1 秒 | 队列空闲时的轮询间隔 |
| `ShutdownFlushTimeout` | TimeSpan | 5 秒 | 应用停止时允许 flush 剩余日志的最长时间 |

## 高级用法

### 只记录错误日志

```csharp
builder.Services.AddDevLogDashboard(options =>
{
    options.OnlyLogErrors = true;  // 只记录 Warning 及以上级别的日志
});
```

### 配置日志级别过滤

```csharp
builder.Services.AddDevLogDashboard(options =>
{
    options.MinLogLevel = LogLevel.Information;  // 只记录 Information 及以上级别的日志
});
```

### 忽略特定路径

```csharp
builder.Services.AddDevLogDashboard(options =>
{
    options.IgnoredPaths.Add("/health");
    options.IgnoredPaths.Add("/metrics");
});
```

### 添加访问授权

```csharp
builder.Services.AddDevLogDashboard(options =>
{
    options.BasicAuthentication = new DevLogDashboardBasicAuthenticationOptions
    {
        UserName = "admin",
        Password = "123456",
        Realm = "DevLogDashboard"
    };
});
```

## 自定义存储实现

DevLogDashboard 支持通过实现 `ILogStore` 接口来扩展存储方案。

### ILogStore 接口

```csharp
public interface ILogStore
{
    /// <summary>
    /// 初始化存储（例如创建数据库表、索引等）
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步添加日志
    /// </summary>
    ValueTask AddAsync(LogEntry? entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询日志（支持分页、过滤、排序）
    /// </summary>
    Task<PageResult<LogEntry>> QueryAsync(LogQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 RequestId 获取日志列表（用于请求追踪）
    /// </summary>
    Task<List<LogEntry>> GetByRequestIdAsync(string requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取追踪汇总列表（用于请求分析）
    /// </summary>
    Task<List<TraceLogSummary>> GetTraceSummariesAsync(DateTime? startTime, DateTime? endTime, CancellationToken cancellationToken = default);
}
```

### 方式一：使用泛型重载（支持依赖注入）

如果你的 `ILogStore` 实现的构造函数支持依赖注入：

```csharp
// 自定义实现
public class PgSqlLogStore : ILogStore
{
    public PgSqlLogStore(IConfiguration configuration, ILogger<PgSqlLogStore> logger)
    {
        // 从 DI 获取依赖
    }

    // 实现所有 ILogStore 方法...
}

// 注册使用
builder.Services.AddDevLogDashboard<PgSqlLogStore>(options =>
{
    options.EndpointPath = "/dev-logs";
    options.ApplicationName = "MyApi";
    options.ApplicationVersion = "1.0.0";
});
```

### 方式二：使用工厂函数（复杂场景）

如果需要手动构造 `ILogStore` 实例：

```csharp
builder.Services.AddDevLogDashboard(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

    var connectionString = config.GetConnectionString("LogConnection")
        ?? "Host=localhost;Port=5432;Username=postgres;Password=123456;Database=logs";

    var logger = loggerFactory.CreateLogger<MyDatabaseLogStore>();
    return new MyDatabaseLogStore(connectionString, logger);
}, options =>
{
    options.EndpointPath = "/dev-logs";
    options.ApplicationName = "MyApi";
});
```

### PostgreSQL 存储示例

以下是一个完整的 PostgreSQL 存储实现示例：

```csharp
using Azrng.DevLogDashboard.Storage;
using Azrng.DevLogDashboard.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Text.Json;

public class PgSqlLogStore : ILogStore
{
    private readonly string _connectionString;
    private readonly ILogger<PgSqlLogStore> _logger;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private volatile bool _initialized;

    public PgSqlLogStore(IConfiguration configuration, ILogger<PgSqlLogStore> logger)
    {
        var connectionString = configuration.GetConnectionString("PostgresConnection");
        _connectionString = string.IsNullOrEmpty(connectionString)
            ? "Host=localhost;Port=5432;Username=postgres;Password=123456;Database=dev_log"
            : connectionString;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            const string sql = @"
                CREATE TABLE IF NOT EXISTS dev_logs (
                    id VARCHAR(50) PRIMARY KEY,
                    request_id VARCHAR(50),
                    timestamp TIMESTAMP NOT NULL,
                    level VARCHAR(20) NOT NULL,
                    message TEXT,
                    request_path VARCHAR(500),
                    request_method VARCHAR(10),
                    response_status_code INTEGER,
                    elapsed_milliseconds BIGINT,
                    source VARCHAR(200),
                    exception TEXT,
                    stack_trace TEXT,
                    machine_name VARCHAR(200),
                    application VARCHAR(200),
                    app_version VARCHAR(50),
                    environment VARCHAR(50),
                    process_id INTEGER,
                    thread_id INTEGER,
                    logger VARCHAR(200),
                    action_id VARCHAR(100),
                    action_name VARCHAR(200),
                    properties JSONB,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );

                CREATE INDEX IF NOT EXISTS idx_dev_logs_timestamp ON dev_logs(timestamp DESC);
                CREATE INDEX IF NOT EXISTS idx_dev_logs_request_id ON dev_logs(request_id);
                CREATE INDEX IF NOT EXISTS idx_dev_logs_level ON dev_logs(level);
                CREATE INDEX IF NOT EXISTS idx_dev_logs_application ON dev_logs(application);

                CREATE INDEX IF NOT EXISTS idx_dev_logs_properties_gin ON dev_logs USING gin(properties);";

            await using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogInformation("PostgreSQL 日志表初始化成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化数据库失败：{Message}", ex.Message);
            throw;
        }
    }

    public async ValueTask AddAsync(LogEntry? entry, CancellationToken cancellationToken = default)
    {
        if (entry == null) return;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        const string sql = @"
            INSERT INTO dev_logs (
                id, request_id, timestamp, level, message,
                request_path, request_method, response_status_code, elapsed_milliseconds,
                source, exception, stack_trace, machine_name, application, app_version,
                environment, process_id, thread_id, logger, action_id, action_name, properties
            ) VALUES (
                @Id, @RequestId, @Timestamp, @Level, @Message,
                @RequestPath, @RequestMethod, @ResponseStatusCode, @ElapsedMilliseconds,
                @Source, @Exception, @StackTrace, @MachineName, @Application, @AppVersion,
                @Environment, @ProcessId, @ThreadId, @Logger, @ActionId, @ActionName,
                @Properties::jsonb
            )";

        await using var cmd = new NpgsqlCommand(sql, conn);
        // 添加参数...
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    // 实现其他方法...
}
```

使用 PostgreSQL 存储：

```csharp
builder.Services.AddDevLogDashboard<PgSqlLogStore>(options =>
{
    options.EndpointPath = "/dev-logs";
    options.ApplicationName = "MyApi";
    options.ApplicationVersion = "1.0.0";
});

var app = builder.Build();

// 初始化存储（如果需要）
using (var scope = app.Services.CreateScope())
{
    var logStore = scope.ServiceProvider.GetRequiredService<ILogStore>();
    try
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await logStore.InitializeAsync(cts.Token);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"LogStore 初始化失败: {ex.Message}");
        // 继续启动，不影响应用运行
    }
}
```

配置连接字符串（appsettings.json）：

```json
{
  "ConnectionStrings": {
    "PostgresConnection": "Host=localhost;Port=5432;Username=postgres;Password=your_password;Database=dev_log"
  }
}
```

## 搜索语法

仪表板支持强大的搜索语法：

- **精准查询**: `message="获取人员列表"`
- **模糊查询**: `message like "超时"`
- **字段查询**: `RequestId = "0HNJ2POQECAU1"`
- **级别查询**: `level = "ERROR"`
- **组合查询**: `level = "ERROR" and message like "超时"`
- **多条件组合**: `level = "ERROR" or level = "WARNING"`

### 支持搜索的字段

| 字段 | 说明 |
|------|------|
| `message` | 日志消息 |
| `level` | 日志级别 |
| `requestId` | 请求 ID |
| `requestPath` | 请求路径 |
| `requestMethod` | 请求方法 |
| `source` | 日志来源 |
| `exception` | 异常信息 |
| `application` | 应用名称 |

### 快捷搜索

在日志详情页，点击属性行首的 **✓** 符号，可快速搜索相同值的日志。

## 界面功能

### Logs 面板

- 实时查看日志列表
- 按级别筛选（支持级别范围筛选，选择某个级别后显示该级别及更高级别的日志）
- 时间倒序显示（最新的在前）
- 按日期范围筛选
- 展开查看详情和堆栈跟踪
- 查看完整的 HTTP 请求上下文信息

### Traces 面板

- 按 RequestId 聚合显示请求链路
- 查看完整请求生命周期
- 分析请求耗时
- 快速定位包含错误的请求

## 日志级别说明

日志级别从低到高依次为：

| 级别 | 说明 |
|------|------|
| Trace | 最详细的日志信息 |
| Debug | 调试信息 |
| Information | 一般信息 |
| Warning | 警告信息 |
| Error | 错误信息 |
| Critical | 严重错误 |

**级别筛选逻辑**: 选择某个级别后，将显示该级别及更高级别的所有日志。例如选择 `Warning`，将显示 `Warning`、`Error`、`Critical` 级别的日志。

## 注意事项

1. **仅用于开发环境**: 本产品设计用于开发调试，生产环境请使用专业的日志系统（如 ELK、Seq 等）
2. **内存限制**: 使用 `InMemoryLogStore` 时，默认最大存储 10000 条日志，超出后自动清理最旧的日志
3. **后台批量写入**: 日志默认通过后台队列批量写入，不阻塞业务线程，日志写入可能有轻微延迟
4. **线程安全**: `InMemoryLogStore` 使用 ReaderWriterLockSlim 保证多线程并发安全
5. **授权安全**: 默认允许匿名访问；如需保护仪表板，请配置 `BasicAuthentication`
6. **自定义存储**: 使用自定义存储实现时，请确保 `InitializeAsync` 方法幂等性，避免重复初始化
7. **时间倒序**: 日志列表默认按时间倒序显示（最新的在前）

## 版本历史

### 1.0.0-preview.5

- **后台批量写入**
  - 新增后台日志队列，日志自动批量写入，不阻塞业务线程
  - 添加 `BatchSize`、`PollInterval` 和 `ShutdownFlushTimeout` 配置选项
  - 移除 `UseBackgroundQueue` 选项，默认开启批量写入
  - 简化 `ILogStore` 接口，统一为异步批量写入
  - 修复日志排序问题，默认时间倒序显示

- **存储可扩展性**
  - 新增 `ILogStore` 接口，支持自定义存储实现
  - 添加 `InitializeAsync` 方法支持存储初始化（如数据库表创建）
  - 提供泛型重载 `AddDevLogDashboard<TLogStore>()` 支持依赖注入
  - 提供工厂函数重载支持复杂构造场景
  - 简化接口，移除 `Count` 和 `Clear` 方法
  - 移除 `IgnoredMethods` 配置选项

### 1.0.0-preview.4

- **界面优化**
  - 日志级别下拉列表改为使用正确的日志级别名称（Trace、Debug、Information、Warning、Error、Critical）
  - 日志级别标签固定宽度显示，保持视觉对齐
  - 移除日志级别后的红点指示器
  - 快捷时间选择优化为：最近 1 分钟、最近 30 分钟、最近 1 小时、最近 12 小时
  - 使用服务器时间同步日期范围，避免客户端时间不准确问题
  - 优化空状态显示图标
  - 改进标签页和模态框的 ARIA 无障碍属性
  - 替换 Emoji 为 SVG 图标，提升视觉效果一致性

### 1.0.0-preview.3

- **修复跳过自身请求逻辑**
  - 改进路径匹配逻辑，支持 `PathBase` 场景
  - 在日志记录器中添加请求过滤，避免记录仪表板自身的 API 请求
  - 支持按 HTTP 方法和路径进行双重过滤
  - 路径匹配改为不区分大小写

### 1.0.0-preview.2

- 修复记录日志重复问题
- 修复 Traces 标签页下点击刷新、搜索、清空按钮时调用错误 API 的问题

### 1.0.0-preview.1

- 初始版本发布
- 支持基础日志记录和查看功能
- 支持请求追踪
- 支持结构化日志
- 支持 HTTP 上下文信息捕获

## 依赖

- Microsoft.AspNetCore.App (FrameworkReference)

## License

MIT

## 参考项目

- [LogDashboard](https://github.com/logdashboard/LogDashboard)
- [MiniProfiler](https://github.com/MiniProfiler/dotnet)
