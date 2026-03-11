# DevLogDashboard

**DevLogDashboard** 是一个面向 .NET 开发者的轻量级日志查看 NuGet 包，专为 API 项目在开发调试阶段提供便捷的日志面板界面。

## 特点

- **零配置**: 无需数据库等外部存储，开箱即用
- **内存存储**: 高效轻量，自动管理
- **实时查看**: 开发调试阶段快速查看日志
- **请求追踪**: 通过 RequestId 查看完整请求链路
- **结构化日志**: 支持记录和查看结构化数据
- **HTTP 上下文捕获**: 自动记录 RequestId、ConnectionId、RequestPath、RequestMethod、ResponseStatusCode 等请求信息

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

### 基础配置

| 选项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `EndpointPath` | string | `/dev-logs` | 仪表板访问路径 |
| `MaxLogCount` | int | 10000 | 最大存储日志条数 |
| `OnlyLogErrors` | bool | false | 是否只记录错误日志 |
| `MinLogLevel` | LogLevel | Trace | 最低日志级别 |
| `ApplicationName` | string | - | 应用名称 |
| `ApplicationVersion` | string | - | 应用版本 |
| `IgnoredPaths` | ICollection<string> | [/health, /healthz, /ready, /metrics, /dev-logs, /favicon.ico] | 忽略的路径 |
| `IgnoredMethods` | ICollection<string> | [OPTIONS] | 忽略的 HTTP 方法 |

### 授权配置

| 选项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `AuthorizationFilter` | Func<HttpContext, Task<bool>> | null | 授权过滤器，返回 false 则拒绝访问 |

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
- 按时间排序
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

## 技术架构

- **目标框架**: .NET 6.0 / 7.0 / 8.0 / 9.0 / 10.0
- **核心组件**:
  - `DevLogDashboardMiddleware` - 仪表板中间件
  - `DevLogDashboardLogger` - 自定义日志记录器
  - `InMemoryLogStore` - 内存日志存储
- **前端**: 原生 JavaScript（无框架依赖）
- **存储方式**: 内存 (ConcurrentBag)，线程安全

## API 端点

| 端点 | 方法 | 说明 |
|------|------|------|
| `/dev-logs-api/dashboard` | GET | 获取仪表板首页统计数据 |
| `/dev-logs-api/logs` | GET | 查询日志列表（分页） |
| `/dev-logs-api/logs/{id}` | GET | 获取单条日志详情 |
| `/dev-logs-api/traces` | GET | 获取请求追踪汇总列表 |
| `/dev-logs-api/traces/{requestId}` | GET | 获取特定 RequestId 的所有日志 |
| `/dev-logs-api/clear` | POST | 清空所有日志 |

## 注意事项

1. **仅用于开发环境**: 本产品设计用于开发调试，生产环境请使用专业的日志系统（如 ELK、Seq 等）
2. **内存限制**: 默认最大存储 10000 条日志，超出后自动清理最旧的 10% 日志
3. **线程安全**: 使用 ConcurrentBag 和 lock 保证多线程并发安全
4. **授权安全**: 建议配置授权过滤器保护敏感信息
5. **自动清理**: 超出最大日志数量时自动清理旧日志，无需手动维护

## 版本历史

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
