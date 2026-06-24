# Azrng.ConsoleApp.DependencyInjection

> 🚀 **现代化的控制台应用开发框架** - 简化依赖注入配置，专注业务逻辑

[![NuGet](https://img.shields.io/nuget/v/Azrng.ConsoleApp.DependencyInjection)](https://www.nuget.org/packages/Azrng.ConsoleApp.DependencyInjection/)
[![License](https://img.shields.io/github/license/azrng/nuget-packages)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-%3E%3D%208.0-purple.svg)](https://docs.microsoft.com/en-us/dotnet/core/)

## 📋 项目简介

控制台依赖注入扩展，为 .NET 控制台应用提供现代化的依赖注入和配置管理能力。

### ✨ 核心特性

- ⚙️ **支持读取 appsettings.json 配置文件**
- 📝 **默认使用 Microsoft.Extensions.Logging（Console + Debug + 本地文件）日志输出**
- 🎯 **简化的依赖注入配置**
- 🔧 **灵活的服务注册方式**
- 🌍 **支持环境配置（appsettings.{Environment}.json）**
- ✅ **启用容器校验，确保依赖关系正确**

## 构建方法

### 方式1: 简单方式，不需要依赖注入

```csharp
var builder = new ConsoleAppServer(args);
await using var sp = builder.Build<TempService>();
await sp.RunAsync();
```

### 方式2: 注入通用配置

```csharp
var builder = new ConsoleAppServer(args);

// 注入自定义配置
builder.Services.AddHttpClient();

await using var sp = builder.Build<UrlSortService>();

await sp.RunAsync();
Console.Read();
```

### 方式3: 委托方式注册服务
```csharp
var builder = new ConsoleAppServer(args);
await using var sp = builder.Build<JsonTempService>(services =>
{
    services.ConfigureDefaultJson();
});
await sp.RunAsync();
```

## 高级用法

### 自定义日志配置（ConfigureLogging）
默认启用 Console + Debug + ExtensionsLoggerProvider 三种日志 Provider。若需接入 Serilog 等第三方日志，或只保留部分 Provider，传入委托进行完全自定义：

```csharp
var builder = new ConsoleAppServer(args);

// 传 null 或不传：使用默认日志 Provider（Console + Debug + ExtensionsLoggerProvider）
builder.ConfigureLogging();

// 传委托：完全自定义日志配置，委托内自行决定添加哪些 Provider
builder.ConfigureLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddConsole();
    // loggingBuilder.AddSerilog(); // 接入其它日志框架
});

await using var sp = builder.Build<TempService>();
await sp.RunAsync();
```

> 注意：传入委托后默认 Provider（Console + Debug + ExtensionsLoggerProvider）不会自动添加，需在委托内自行配置。

### 绑定强类型选项配置（Configure）
`Configure<TOption>` 封装了 `Services.Configure<TOption>(Configuration.GetSection(...))`，避免手写样板代码：

```csharp
public class MyOptions
{
    public string Url { get; set; } = string.Empty;
    public int Timeout { get; set; }
}

var builder = new ConsoleAppServer(args);

// 指定配置节名称
builder.Configure<MyOptions>("MyOptions");

// 或不传节名，默认以类型名 MyOptions 作为配置节
builder.Configure<MyOptions>();

await using var sp = builder.Build<TempService>();
// 在服务中注入 IOptions<MyOptions> 即可读取
```

## 版本更新记录

* 1.3.5
  * 新增 `ConfigureLogging(Action<ILoggingBuilder>?)` 委托重载，支持自定义日志 Provider（接入 Serilog 等），传 null 时保持默认行为
  * 新增 `Configure<TOption>` 便捷方法，封装 `Services.Configure<TOption>(Configuration.GetSection(...))`，支持链式调用
  * 优化 `ExtensionsLogger` 日志分发：switch 分支改为静态字典查表，统一走 `LocalLogHelper.WriteMyLogs` 入口
  * 在 csproj 通过 `NoWarn` 抑制泛型 `Configure<T>` 的 SYSLIB1104 诊断（微软官方泛型配置绑定在 AOT 下的已知限制，见 dotnet/runtime#89273）
* 1.3.4
  * `ConsoleAppServer` 构造函数参数支持可空，默认值为 `null`
  * `IServiceStart.Title` 属性简化为隐式 public 访问修饰符
  * 优化代码格式
* 1.3.3
  * 配置文件基路径改为 `AppContext.BaseDirectory`，并支持按环境加载 `appsettings.{Environment}.json`
  * 优化 DI 启动流程：`IServiceStart` 使用作用域解析并启用容器校验（`ValidateOnBuild`、`ValidateScopes`）
  * 修复 `ExtensionsLogger` 的可空签名告警并完善异常日志内容
  * 文档修正：默认日志实现为 `Microsoft.Extensions.Logging`，不再描述为 Serilog
* 1.3.2
  * 发布正式版
* 1.3.2-beta3
  * 移除Serilog相关包测试
* 1.3.2-beta2
  * 引用.Net10正式包
* 1.3.2-beta1
  * 适配.Net10
* 1.3.1
  * 适配Azrng.Core的更新
* 1.3.0
  * 支持Build重载，支持针对Service注入特定配置
* 1.2.0
  * 支持将默认ILogger日志输出到本地文件
* 1.1.0
  * 更新依赖项
* 1.0.1
  * 读取环境变量需增加变量前缀：ASPNETCORE_
* 1.1.0-beta1
  * 测试进一步缩小打包文件的大小
* 1.0.0
  * 基础控制台依赖注入开发
