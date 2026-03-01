# Azrng.ConsoleApp.DependencyInjection

控制台依赖注入扩展

* 支持读取appsettings.json配置文件
* 默认使用 Microsoft.Extensions.Logging（Console + Debug + 本地文件）日志输出

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

## 版本更新记录

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
