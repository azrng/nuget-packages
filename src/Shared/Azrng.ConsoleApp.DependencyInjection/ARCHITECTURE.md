# Azrng.ConsoleApp.DependencyInjection 架构设计文档

## 一、项目概述

`Azrng.ConsoleApp.DependencyInjection` 是一个为控制台应用程序提供依赖注入功能的轻量级框架。该库简化了控制台应用的开发流程，提供了开箱即用的配置管理、日志记录和依赖注入能力，同时支持 Native AOT 编译，使得控制台应用可以发布为体积更小的可执行文件。

### 1.1 设计目标

- 为控制台应用提供类似 ASP.NET Core 的依赖注入体验
- 支持 `appsettings.json` 配置文件读取
- 提供统一的日志记录接口（控制台 + 本地文件）
- 支持 Native AOT 编译，减小发布体积
- 简化控制台应用的启动流程

### 1.2 技术栈

- **目标框架**: net8.0; net9.0; net10.0
- **AOT 兼容**: 完全支持 Native AOT (`IsAotCompatible = true`)
- **裁剪支持**: 启用链接裁剪 (`TrimMode = link`)
- **核心依赖**:
  - `Microsoft.Extensions.Hosting` (依赖注入、配置、日志)
  - `Azrng.Core` (核心工具库)

---

## 二、项目结构

```
Azrng.ConsoleApp.DependencyInjection/
├── ConsoleAppServer.cs                    # 核心服务构建器
├── IStartService.cs                       # 服务启动接口
├── ConsoleTool.cs                         # 控制台工具类
├── Logger/                                # 日志模块
│   ├── ExtensionsLogger.cs               # 自定义日志记录器
│   └── ExtensionsLoggerProvider.cs       # 日志提供者
└── Azrng.ConsoleApp.DependencyInjection.csproj
```

---

## 三、核心组件架构

### 3.1 整体架构图

```
┌─────────────────────────────────────────────────────────────────────┐
│                         控制台应用程序                                │
│                      (用户实现的 IServiceStart)                      │
└────────────────────────────────────┬────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      ConsoleAppServer (构建器)                       │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │  构造阶段                                                     │   │
│  │  - 加载配置 (appsettings.json + 环境变量 + 命令行参数)         │   │
│  │  - 创建 ServiceCollection                                    │   │
│  │  - 配置日志 (Console + Debug + File)                         │   │
│  └──────────────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │  Build<T>() 方法                                             │   │
│  │  - 注册 IServiceStart 服务                                    │   │
│  │  - 执行可选的服务注册委托                                     │   │
│  │  - 构建 ServiceProvider                                      │   │
│  └──────────────────────────────────────────────────────────────┘   │
└────────────────────────────────────┬────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    ServiceProviderExtensions                        │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │  RunAsync() 方法                                             │   │
│  │  - 获取 IServiceStart 实例                                    │   │
│  │  - 打印应用标题                                               │   │
│  │  - 调用 RunAsync() 启动应用                                   │   │
│  │  - 捕获并记录异常                                             │   │
│  └──────────────────────────────────────────────────────────────┘   │
└────────────────────────────────────┬────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────┐
│                         配置系统                                     │
│  ┌──────────────┬──────────────┬────────────────────────────────┐  │
│  │ appsettings. │ Environment  │  Command Line Arguments        │  │
│  │    json      │  Variables   │  (命令行参数)                   │  │
│  │  (JSON文件)  │ (ASPNETCORE_) │                                │  │
│  └──────────────┴──────────────┴────────────────────────────────┘  │
│                           │                                         │
│                           ▼                                         │
│                    IConfiguration                                  │
└─────────────────────────────────────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────┐
│                         日志系统                                     │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │  ILoggerFactory → AddLogging()                               │   │
│  │    ├─ AddConsole()      (控制台输出)                          │   │
│  │    ├─ AddDebug()        (调试窗口输出)                        │   │
│  │    └─ ExtensionsLoggerProvider (本地文件输出)                 │   │
│  └──────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

### 3.2 核心组件说明

#### 3.2.1 ConsoleAppServer (服务构建器)

**功能**: 控制台应用程序的核心构建器，负责初始化配置、服务和日志。

**构造流程**:

```csharp
public ConsoleAppServer(string[] args)
{
    // 1. 构建配置
    var configBuilder = new ConfigurationBuilder();
    configBuilder.SetBasePath(Environment.CurrentDirectory);
    configBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    configBuilder.AddEnvironmentVariables("ASPNETCORE_");  // 环境变量前缀
    configBuilder.AddCommandLine(args);                    // 命令行参数

    // 2. 创建服务容器
    Services = new ServiceCollection();
    Configuration = config;
    Services.AddSingleton<IConfiguration>(config);

    // 3. 配置日志
    ConfigureLogging();
}
```

**配置加载优先级**（从低到高）:

1. `appsettings.json` - 基础配置
2. 环境变量 (带 `ASPNETCORE_` 前缀) - 环境相关配置
3. 命令行参数 - 最高优先级

**示例**:

```json
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AppSettings": {
    "ConnectionString": "localhost"
  }
}
```

```bash
# 环境变量
export ASPNETCORE_AppSettings__ConnectionString=prod-server

# 命令行参数
--AppSettings:ConnectionString "override-server"
```

最终 `ConnectionString` 的值为 `"override-server"`。

#### 3.2.2 Build 方法

**功能**: 构建 ServiceProvider 并注册启动服务。

**方法签名**:

```csharp
// 方式一: 无额外服务注册
public ServiceProvider Build<T>() where T : class, IServiceStart

// 方式二: 使用委托注册额外服务
public ServiceProvider Build<TStart>(
    Action<IServiceCollection>? registerServicesAction
) where TStart : class, IServiceStart
```

**执行流程**:

```
1. 注册 IServiceStart → T 映射
2. (可选) 执行 registerServicesAction 委托
3. 调用 Services.BuildServiceProvider()
4. 返回 ServiceProvider
```

**AOT 兼容性**:

方法使用 `[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]` 特性，
确保在 AOT 编译时保留公共构造函数的元数据，避免运行时反射失败。

**使用示例**:

```csharp
// 方式一: 简单场景
var builder = new ConsoleAppServer(args);
await using var sp = builder.Build<MyService>();
await sp.RunAsync();

// 方式二: 需要额外服务
var builder = new ConsoleAppServer(args);
await using var sp = builder.Build<MyService>(services =>
{
    services.AddHttpClient();
    services.AddSingleton<IMyRepository, MyRepository>();
});
await sp.RunAsync();
```

#### 3.2.3 IServiceStart 接口

**功能**: 定义控制台应用的启动契约。

```csharp
public interface IServiceStart
{
    /// <summary>
    /// 标题 (显示在应用启动时)
    /// </summary>
    string Title { get; }

    /// <summary>
    /// 应用程序入口
    /// </summary>
    Task RunAsync();
}
```

**实现示例**:

```csharp
public class MyService : IServiceStart
{
    private readonly ILogger<MyService> _logger;
    private readonly IConfiguration _config;

    public MyService(ILogger<MyService> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public string Title => "我的控制台应用";

    public async Task RunAsync()
    {
        _logger.LogInformation("应用已启动");

        var connectionString = _config["AppSettings:ConnectionString"];
        _logger.LogInformation("连接字符串: {ConnectionString}", connectionString);

        // 业务逻辑...

        await Task.CompletedTask;
    }
}
```

#### 3.2.4 ServiceProviderExtensions

**功能**: 提供 ServiceProvider 的扩展方法，启动应用程序。

```csharp
public static async Task RunAsync(this ServiceProvider serviceProvider)
{
    try
    {
        var service = serviceProvider.GetRequiredService<IServiceStart>();
        ConsoleTool.PrintTitle(service.Title);
        await service.RunAsync();
    }
    catch (Exception ex)
    {
        await LocalLogHelper.WriteMyLogsAsync("ERROR", "未处理异常" + ex.GetExceptionAndStack());
        throw;
    }
}
```

**工作流程**:

```
1. 从容器获取 IServiceStart 实例
2. 打印应用标题 (带分隔线)
3. 调用 RunAsync() 执行业务逻辑
4. 捕获异常并记录到本地日志文件
```

---

## 四、日志系统

### 4.1 日志架构

```
┌─────────────────────────────────────────────────────────────────┐
│                       ILoggerFactory                            │
│                    (在 ConsoleAppServer 中创建)                  │
└────────────────────────────────────┬────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────┐
│                        日志提供者                                │
│  ┌────────────┬────────────┬────────────────────────────────┐   │
│  │  Console   │   Debug    │  ExtensionsLoggerProvider      │   │
│  │  Logger    │   Logger   │  (自定义文件日志)               │   │
│  └────────────┴────────────┴────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

### 4.2 日志配置

在 `ConsoleAppServer` 构造函数中自动配置：

```csharp
private void ConfigureLogging()
{
    Services.AddLogging(loggingBuilder =>
    {
        // 从 appsettings.json 的 Logging 节点读取配置
        loggingBuilder.AddConfiguration(Configuration.GetSection("Logging"));

        // 控制台输出
        loggingBuilder.AddConsole();

        // 调试窗口输出 (Visual Studio)
        loggingBuilder.AddDebug();

        // 自定义文件输出
        loggingBuilder.AddProvider(new ExtensionsLoggerProvider());
    });
}
```

### 4.3 ExtensionsLogger (自定义文件日志)

**功能**: 将日志输出到本地文件，通过 `Azrng.Core` 的 `LocalLogHelper` 实现。

**日志级别判断**:

```csharp
public bool IsEnabled(LogLevel logLevel)
{
    return logLevel != LogLevel.None &&
           (int)logLevel >= (int)CoreGlobalConfig.MinimumLevel;
}
```

支持通过 `CoreGlobalConfig.MinimumLevel` 配置最低日志级别。

**日志映射**:

| LogLevel | 方法 |
|----------|------|
| `Trace` | `LocalLogHelper.LogTrace()` |
| `Debug` | `LocalLogHelper.LogDebug()` |
| `Information` | `LocalLogHelper.LogInformation()` |
| `Warning` | `LocalLogHelper.LogWarning()` |
| `Error` | `LocalLogHelper.LogError()` |
| `Critical` | `LocalLogHelper.LogCritical()` |

**日志格式**:

```
{CategoryName} {FormattedMessage}
```

例如：`MyApp.Services.DataService 正在连接数据库...`

### 4.4 日志配置示例

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

---

## 五、配置系统

### 5.1 配置源优先级

配置系统使用 Microsoft.Extensions.Configuration，支持多个配置源：

| 配置源 | 优先级 | 说明 |
|--------|--------|------|
| `appsettings.json` | 低 | 基础配置文件 |
| 环境变量 (ASPNETCORE_) | 中 | 环境相关配置 |
| 命令行参数 | 高 | 运行时覆盖 |

### 5.2 环境变量配置

环境变量需要添加 `ASPNETCORE_` 前缀：

```bash
# 设置环境变量
export ASPNETCORE_AppSettings__ConnectionString="server=localhost"
export ASPNETCORE_AppSettings__Timeout=30

# 读取方式
var connectionString = Configuration["AppSettings:ConnectionString"];
var timeout = Configuration.GetValue<int>("AppSettings:Timeout");
```

**注意**: `AddEnvironmentVariables("ASPNETCORE_")` 会自动移除前缀，
所以环境变量 `ASPNETCORE_AppSettings__ConnectionString` 对应配置键 `AppSettings:ConnectionString`。

### 5.3 命令行参数配置

```bash
# 等号格式
--AppSettings:ConnectionString="server=localhost"

# 空格格式
--AppSettings:ConnectionString "server=localhost"

# 切换参数格式
/AppSettings:ConnectionString "server=localhost"
```

---

## 六、使用场景

### 6.1 简单任务处理

```csharp
public class DataImportService : IServiceStart
{
    private readonly ILogger<DataImportService> _logger;

    public DataImportService(ILogger<DataImportService> logger)
    {
        _logger = logger;
    }

    public string Title => "数据导入工具";

    public async Task RunAsync()
    {
        _logger.LogInformation("开始导入数据");

        // 导入逻辑...

        _logger.LogInformation("数据导入完成");
    }
}

// Program.cs
var builder = new ConsoleAppServer(args);
await using var sp = builder.Build<DataImportService>();
await sp.RunAsync();
```

### 6.2 带 HTTP 请求的服务

```csharp
public class WeatherService : IServiceStart
{
    private readonly ILogger<WeatherService> _logger;
    private readonly HttpClient _httpClient;

    public WeatherService(ILogger<WeatherService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public string Title => "天气查询工具";

    public async Task RunAsync()
    {
        var response = await _httpClient.GetAsync("https://api.weather.com/current");
        var weather = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("当前天气: {Weather}", weather);
    }
}

// Program.cs
var builder = new ConsoleAppServer(args);
await using var sp = builder.Build<WeatherService>(services =>
{
    services.AddHttpClient();
});
await sp.RunAsync();
```

### 6.3 带配置的服务

```csharp
public class EmailService : IServiceStart
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _config;

    public EmailService(ILogger<EmailService> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public string Title => "邮件发送服务";

    public async Task RunAsync()
    {
        var smtpServer = _config["Email:SmtpServer"];
        var port = _config.GetValue<int>("Email:Port");

        _logger.LogInformation("连接到邮件服务器: {Server}:{Port}", smtpServer, port);

        // 发送邮件逻辑...
    }
}

// appsettings.json
{
  "Email": {
    "SmtpServer": "smtp.example.com",
    "Port": 587
  }
}
```

---

## 七、Native AOT 支持

### 7.1 AOT 编译优势

- **更快的启动速度**: 预编译为本地代码，无需 JIT 编译
- **更小的内存占用**: 移除不需要的元数据和 IL 代码
- **更小的发布体积**: 链接裁剪移除未使用的代码

### 7.2 AOT 配置

项目已配置 AOT 兼容性：

```xml
<PropertyGroup>
    <IsAotCompatible>true</IsAotCompatible>
    <PublishTrimmed>true</PublishTrimmed>
    <TrimMode>link</TrimMode>
    <EnableTrimAnalyzer>true</EnableTrimAnalyzer>
</PropertyGroup>
```

### 7.3 AOT 发布命令

```bash
# 发布为 AOT 可执行文件
dotnet publish -c Release -r win-x64 --self-contained /p:PublishAot=true

# Linux x64
dotnet publish -c Release -r linux-x64 --self-contained /p:PublishAot=true
```

### 7.4 AOT 注意事项

1. **避免使用反射**: `Build<T>()` 方法使用 `DynamicallyAccessedMembers` 特性确保构造函数可用
2. **使用泛型约束**: `where T : class, IServiceStart` 确保类型安全
3. **避免动态加载**: 不使用 `Assembly.Load` 等动态加载技术

---

## 八、工作流程

### 8.1 完整启动流程

```
┌─────────────────────────────────────────────────────────────────┐
│  1. 创建 ConsoleAppServer                                       │
│     - 加载配置 (appsettings.json + 环境变量 + 命令行)             │
│     - 创建 ServiceCollection                                     │
│     - 配置日志                                                   │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│  2. 调用 Build<T>()                                            │
│     - 注册 IServiceStart → T                                     │
│     - (可选) 执行服务注册委托                                    │
│     - 构建 ServiceProvider                                      │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│  3. 调用 RunAsync()                                             │
│     - 获取 IServiceStart 实例                                    │
│     - 打印应用标题                                               │
│     - 调用 RunAsync() 执行业务逻辑                               │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│  4. 执行业务逻辑                                                │
│     - 使用依赖注入的服务                                         │
│     - 记录日志                                                   │
│     - 读取配置                                                   │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│  5. 退出                                                        │
│     - 释放 ServiceProvider                                      │
│     - 刷新日志缓冲                                               │
└─────────────────────────────────────────────────────────────────┘
```

### 8.2 异常处理流程

```
RunAsync() 执行
      │
      ├─ 正常执行 → 完成任务
      │
      └─ 抛出异常
            │
            ▼
      捕获异常
            │
            ▼
      记录到本地日志
            │
            ▼
      重新抛出异常
```

---

## 九、版本演进

| 版本 | 主要变更 |
|------|----------|
| 1.3.2 | 发布正式版 |
| 1.3.2-beta3 | 移除 Serilog 相关包测试 |
| 1.3.2-beta2 | 引用 .NET 10 正式包 |
| 1.3.2-beta1 | 适配 .NET 10 |
| 1.3.1 | 适配 `Azrng.Core` 的更新 |
| 1.3.0 | 支持 `Build` 重载，支持针对 Service 注入特定配置 |
| 1.2.0 | 支持将默认 `ILogger` 日志输出到本地文件 |
| 1.1.0 | 更新依赖项 |
| 1.0.1 | 读取环境变量需增加变量前缀：`ASPNETCORE_` |
| 1.0.0 | 基础控制台依赖注入开发 |

---

## 十、最佳实践

### 10.1 服务注册建议

```csharp
// 推荐：使用 Build<T>(Action<IServiceCollection>) 注册额外服务
var builder = new ConsoleAppServer(args);
await using var sp = builder.Build<MyService>(services =>
{
    // 注册 HttpClient
    services.AddHttpClient();

    // 注册应用服务
    services.AddScoped<IRepository, SqlRepository>();
    services.AddScoped<IUserService, UserService>();

    // 注册选项
    services.Configure<AppOptions>(config.GetSection("App"));
});
await sp.RunAsync();
```

### 10.2 配置文件建议

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "AppSettings": {
    "Name": "MyApp",
    "Version": "1.0.0"
  }
}
```

### 10.3 日志使用建议

```csharp
public class MyService : IServiceStart
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> _logger)
    {
        _logger = _logger;  // 使用泛型 ILogger<T>
    }

    public async Task RunAsync()
    {
        // 使用结构化日志
        _logger.LogInformation("处理用户 {UserId} 的请求", userId);
        _logger.LogError(ex, "处理用户 {UserId} 时发生错误", userId);
    }
}
```

### 10.4 资源清理建议

```csharp
// 使用 await using 确保资源正确释放
var builder = new ConsoleAppServer(args);
await using var sp = builder.Build<MyService>();
await sp.RunAsync();
```

---

## 十一、注意事项

### 11.1 配置文件位置

`appsettings.json` 必须放在可执行文件所在目录，或使用 `SetBasePath` 指定路径：

```csharp
configBuilder.SetBasePath(AppContext.BaseDirectory);
```

### 11.2 环境变量前缀

所有环境变量必须添加 `ASPNETCORE_` 前缀：

```bash
# 错误
export ConnectionString="server=localhost"

# 正确
export ASPNETCORE_ConnectionString="server=localhost"
```

### 11.3 AOT 限制

- 避免使用 `Activator.CreateInstance` 等动态实例化
- 避免使用反射获取类型信息
- 使用 `Build<T>()` 而非反射创建实例

### 11.4 日志文件位置

本地日志文件由 `Azrng.Core` 的 `LocalLogHelper` 管理，默认在应用程序目录。

---

## 十二、参考资料

- [Microsoft.Extensions.DependencyInjection 文档](https://learn.microsoft.com/zh-cn/dotnet/core/extensions/dependency-injection)
- [Microsoft.Extensions.Configuration 文档](https://learn.microsoft.com/zh-cn/dotnet/core/extensions/configuration)
- [Microsoft.Extensions.Logging 文档](https://learn.microsoft.com/zh-cn/dotnet/core/extensions/logging)
- [Native AOT 文档](https://learn.microsoft.com/zh-cn/dotnet/core/deploying/native-aot/)
