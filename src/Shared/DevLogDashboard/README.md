# DevLogDashboard

**DevLogDashboard** 是一个面向 .NET 开发者的轻量级日志查看 NuGet 包，专为 API 项目在开发调试阶段提供便捷的日志面板界面。

## 特点

- **零配置**: 无需数据库等外部存储，开箱即用
- **内存存储**: 高效轻量，自动管理
- **实时查看**: 开发调试阶段快速查看日志
- **请求追踪**: 通过 RequestId 查看完整请求链路
- **结构化日志**: 支持记录和查看结构化数据

## 快速开始

### 1. 安装 NuGet 包

```bash
dotnet add package DevLogDashboard
```

### 2. 配置服务

在 `Program.cs` 中添加：

```csharp
using DevLogDashboard.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 添加 DevLogDashboard 服务
builder.Services.AddDevLogDashboard(options =>
{
    options.EndpointPath = "/dev-logs";      // 仪表板访问路径
    options.MaxLogCount = 10000;             // 最大存储日志数
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

| 选项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `EndpointPath` | string | `/dev-logs` | 仪表板访问路径 |
| `MaxLogCount` | int | 10000 | 最大存储日志条数 |
| `RetentionPeriod` | TimeSpan | 24 小时 | 日志保留时间 |
| `EnableRequestTracking` | bool | true | 是否启用请求追踪 |
| `OnlyLogErrors` | bool | false | 是否只记录错误日志 |
| `MinLogLevel` | LogLevel | Trace | 最低日志级别 |
| `ApplicationName` | string | - | 应用名称 |
| `ApplicationVersion` | string | - | 应用版本 |
| `IgnoredPaths` | ICollection<string> | [/health, /metrics, ...] | 忽略的路径 |

## 高级用法

### 流式构建器配置

```csharp
builder.Services.AddDevLogDashboard()
    .WithEndpointPath("/logs")
    .WithMaxLogCount(5000)
    .WithApplicationName("MyApi")
    .WithAuthorization(async context =>
    {
        // 自定义授权逻辑
        return await Task.FromResult(context.User.Identity?.IsAuthenticated ?? false);
    });
```

### 搜索语法

仪表板支持强大的搜索语法：

- **精准查询**: `message="获取人员列表"`
- **模糊查询**: `message like "mdm"`
- **字段查询**: `RequestId = "0HNJ2POQECAU1"`
- **级别查询**: `level = "ERROR"`
- **组合查询**: `level = "ERROR" and message like "超时"`

支持字段：`message`, `level`, `requestId`, `requestPath`, `source`, `exception`, `application`

## 界面预览

### Logs 面板
- 实时查看日志列表
- 按级别、时间、应用筛选
- 展开查看详情和堆栈跟踪

### Traces 面板
- 按 RequestId 聚合显示请求链路
- 查看完整请求生命周期
- 分析请求耗时

## 技术架构

- **.NET 6/7/8+** 多目标框架
- **ASP.NET Core Middleware** 中间件技术
- **内存存储** 无需外部依赖
- **内置 Web UI** 无需额外配置

## 注意事项

1. **仅用于开发环境**: 本产品设计用于开发调试，生产环境请使用专业的日志系统（如 ELK、Seq 等）
2. **内存限制**: 默认最大存储 10000 条日志，超出后自动清理旧日志
3. **线程安全**: 支持多线程并发写入
4. **授权安全**: 建议配置授权过滤器保护敏感信息

## 依赖

- .NET 6.0 / 7.0 / 8.0
- ASP.NET Core

## License

MIT

## 参考项目

- [LogDashboard](https://github.com/logdashboard/LogDashboard)
- [MiniProfiler](https://github.com/MiniProfiler/dotnet)
