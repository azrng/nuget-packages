using Azrng.ConsoleApp.DependencyInjection;
using Azrng.Core.Json;
using ConsoleAppDI;

var builder = new ConsoleAppServer(args);

// 注册JSON序列化服务
builder.Services.ConfigureDefaultJson();

// 方式1：没有依赖注入
// await using var sp = builder.Build<TempService>();

// 方式2: 使用委托方式注册服务 (完全AOT兼容，推荐)
// await using var sp = builder.Build<JsonTempService>((services) =>
// {
//     // 注册JSON序列化服务
//     services.ConfigureDefaultJson();
// });

// await using var sp = builder.Build<HttpRequestService>((services) =>
// {
//     services.AddHttpClientService();
// });
await using var sp = builder.Build<JsonTempService>();

await sp.RunAsync();
Console.Read();