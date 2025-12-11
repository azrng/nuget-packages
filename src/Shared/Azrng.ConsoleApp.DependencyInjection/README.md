# Azrng.ConsoleApp.DependencyInjection

控制台依赖注入扩展

* 支持读取appsettings.json配置文件
* 默认使用了Serilog日志输出

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
