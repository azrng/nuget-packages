# Common.HttpClients

> 基于 Microsoft.Extensions.Http.Resilience 和 Polly 的 HTTP 客户端库，所有方法返回 `IHttpResult<T>` 结构化结果

## 主要特性

- 所有请求方法返回 `IHttpResult<T>`，包含 `IsSuccess`、`Data`、`ErrorMessage`、`StatusCode`、`RawBody` 等结构化信息；失败统一返回失败结果，需要抛异常可调 `EnsureSuccess()`
- **统一请求签名**：查询参数与请求头收拢到 `HttpSendOptions`，所有动词方法参数顺序一致，告别"GET 的 query 在第 2 位、POST 的 query 在第 3 位"的记忆负担
- **多值请求头 `HttpHeaders`**：单值用索引器直接赋字符串，多值用 `Add` 追加，支持多个 `Accept`/`Set-Cookie` 等同名头
- **可配置 JSON 命名策略**：`JsonNamingPolicy` 支持 CamelCase / PascalCase / SnakeCaseLower / None，适配不同后端字段约定（默认 CamelCase）
- 支持通过匿名对象、`IDictionary<string, string>`、`NameValueCollection` 自动构建 URL 查询参数
- 内置文件下载方法 `DownloadFileAsync`
- `CreateBearerHeaders` 辅助方法自动构造 Bearer Token 头
- 智能日志记录和审计（包含请求前后日志）
- 完整的 Polly 弹性策略（降级、并发限制、重试、熔断器、超时）
- 分布式追踪支持（X-Trace-Id 自动传播）
- 可扩展的日志脱敏（支持自定义敏感头和字段）

## 安装

```bash
dotnet add package Common.HttpClients --version 4.0.0
```

## 项目结构

```text
Common.HttpClients.Next/
├── Abstractions/        # 接口与抽象类型（IHttpHelper、IHttpResult、HttpClientOptions、HttpSendOptions、HttpHeaders 等）
├── Client/              # IHttpHelper 默认实现（HttpClientHelper、HttpHelperFactory、HttpResult）
├── Extensions/          # DI 扩展（AddHttpClientService）、HttpHelperExtensions（CreateBearerHeaders / EnsureSuccess）
├── Internal/            # 内部常量（HTTP 头名称、请求选项键）
├── Logging/             # 审计日志处理器与默认脱敏器
└── Utils/               # JSON 序列化、查询字符串构建等工具
```

> 所有类型统一位于 `Common.HttpClients` 命名空间，文件夹仅用于按职责组织源码。

## 快速开始

### 1. 注册服务

```csharp
// 使用默认配置
services.AddHttpClientService();

// 或自定义配置
services.AddHttpClientService(options =>
{
    options.AuditLog = true;                        // 启用审计日志
    options.EnableLogRedaction = true;              // 启用日志脱敏
    options.Timeout = 30;                            // 超时时间（秒）
    options.MaxRetryAttempts = 3;                    // 最大重试次数
    options.RetryDelaySeconds = 1;                   // 重试基础延迟（秒）
    options.ConcurrencyLimit = 100;                  // 并发限制
    options.JsonNamingPolicy = JsonNamingPolicyType.CamelCase; // JSON 命名策略（默认 CamelCase）
});
```

### 命名客户端与 IHttpHelperFactory（多服务 / 多 BaseAddress）

需要同时对接多个服务端、或为不同服务配置不同弹性策略时，使用命名重载按名注册：

```csharp
services.AddHttpClientService("user-api", options =>
{
    options.BaseAddress = "https://user.example.com/";
});

services.AddHttpClientService("order-api", options =>
{
    options.BaseAddress = "https://order.example.com/";
    options.Timeout = 10;
    options.MaxRetryAttempts = 5;
});
```

注入 `IHttpHelperFactory`，按名取出对应的 `IHttpHelper`：

```csharp
public class MyService(IHttpHelperFactory factory)
{
    private readonly IHttpHelper _userApi = factory.CreateClient("user-api");
    private readonly IHttpHelper _orderApi = factory.CreateClient("order-api");

    public async Task RunAsync()
    {
        var user = await _userApi.GetAsync<User>("api/users/1");
        var order = await _orderApi.GetAsync<Order>("api/orders/1");
    }
}
```

> 注：命名重载 `AddHttpClientService(name, configure)` 仅注册命名客户端；若需要默认的 `IHttpHelper`（构造函数直接注入），使用无 name 的 `AddHttpClientService(options)` 或 `AddHttpClientService()` 重载，它们内部注册指向 `"default"` 的 `IHttpHelper`。

### 2. 使用 HTTP 客户端

```csharp
public class MyService
{
    private readonly IHttpHelper _httpHelper;

    public MyService(IHttpHelper httpHelper)
    {
        _httpHelper = httpHelper;
    }

    public async Task GetUserAsync()
    {
        var result = await _httpHelper.GetAsync<User>("https://api.example.com/users/1");

        if (result.IsSuccess)
        {
            var user = result.Data;
            Console.WriteLine($"Status: {result.StatusCode}");
        }
        else
        {
            Console.WriteLine($"Error: {result.ErrorMessage}");
            Console.WriteLine($"Status: {result.StatusCode}");
        }
    }
}
```

## HttpSendOptions

所有动词方法的查询参数与请求头都收拢到 `HttpSendOptions`，统一了参数顺序：

```csharp
public sealed class HttpSendOptions
{
    public object? Query { get; set; }   // 查询参数（匿名对象 / IDictionary / NameValueCollection）
    public HttpHeaders? Headers { get; set; } // 请求头（支持同名多值，覆盖客户端默认头）
}
```

```csharp
var result = await _httpHelper.GetAsync<User>("https://api.example.com/users",
    new HttpSendOptions
    {
        Query = new { page = 1, pageSize = 20 },
        Headers = HttpHelperExtensions.CreateBearerHeaders("your-token")
    });
```

## IHttpResult\<T\> 返回值

所有请求方法返回 `IHttpResult<T>`，提供结构化的响应信息：

```csharp
public interface IHttpResult<T>
{
    bool IsSuccess { get; }           // 请求是否成功
    T? Data { get; }                  // 反序列化后的响应数据
    string? ErrorMessage { get; }     // 错误信息（失败时）
    HttpStatusCode StatusCode { get; } // HTTP 状态码
    string? RawBody { get; }          // 原始响应体
    bool IsFallbackResponse { get; }  // 是否为 Polly 降级响应
}
```

### 判断请求结果

```csharp
var result = await _httpHelper.GetAsync<User>(url);

// 方式1：直接判断
if (result.IsSuccess) { var user = result.Data; }

// 方式2：检查状态码
if (result.StatusCode == HttpStatusCode.NotFound) { /* 处理 404 */ }

// 方式3：需要失败即抛异常的调用风格 —— 显式调用 EnsureSuccess()
var user = (await _httpHelper.GetAsync<User>(url)).EnsureSuccess().Data;

// 方式4：区分降级响应
if (!result.IsSuccess && result.IsFallbackResponse) { /* Polly 降级响应（503） */ }
```

## 请求方法

### GET 请求

```csharp
var result = await _httpHelper.GetAsync<User>("https://api.example.com/users/1");
var result = await _httpHelper.GetAsync<string>("https://api.example.com/users/1"); // 返回原始响应体
var result = await _httpHelper.GetStreamAsync("https://api.example.com/files/1");    // 文件流
```

### 查询参数

通过 `HttpSendOptions.Query` 自动构建 URL 查询字符串，支持匿名对象、`IDictionary<string, string>`、`NameValueCollection`：

```csharp
var result = await _httpHelper.GetAsync<List<User>>(
    "https://api.example.com/users",
    new HttpSendOptions { Query = new { page = 1, pageSize = 20, keyword = "test" } });
// => https://api.example.com/users?page=1&pageSize=20&keyword=test

// 集合参数自动展开
var result = await _httpHelper.GetAsync<string>(
    "https://api.example.com/filter",
    new HttpSendOptions { Query = new { ids = new[] { 1, 2, 3 } } });
// => https://api.example.com/filter?ids=1&ids=2&ids=3
```

### POST 请求

```csharp
var result = await _httpHelper.PostAsync<User>("https://api.example.com/users", new { name = "张三", age = 25 });
var result = await _httpHelper.PostAsync<string>("https://api.example.com/users", "{\"raw\":\"json\"}"); // 原样发送
```

### POST Form-Data

```csharp
var data = new Dictionary<string, string> { ["username"] = "admin", ["password"] = "123456" };
var result = await _httpHelper.PostFormDataAsync<LoginResponse>("https://api.example.com/login", data);

// 上传单个文件
using var stream = File.OpenRead("photo.jpg");
var result = await _httpHelper.PostFormDataAsync<UploadResponse>(
    "https://api.example.com/upload", "file", stream, "photo.jpg");

// 多文件/混合参数
using var form = new MultipartFormDataContent();
form.Add(new ByteArrayContent(fileBytes), "file", "document.pdf");
var result = await _httpHelper.PostFormDataAsync<UploadResponse>("https://api.example.com/upload", form);
```

### PUT / PATCH / DELETE

```csharp
var result = await _httpHelper.PutAsync<User>("https://api.example.com/users/1", updatedUser);
var result = await _httpHelper.PatchAsync<User>("https://api.example.com/users/1", new { name = "李四" });
var result = await _httpHelper.DeleteAsync<DeleteResponse>("https://api.example.com/users/1");
var result = await _httpHelper.DeleteAsync<string>("https://api.example.com/users/1"); // 原始响应体
```

### 文件下载

```csharp
var result = await _httpHelper.DownloadFileAsync(
    "https://api.example.com/files/report.pdf", @"C:\Downloads\report.pdf");
// 下载失败时自动清理不完整的文件
```

### SOAP 请求

```csharp
var result = await _httpHelper.PostSoapAsync<SoapResponse>("https://api.example.com/soap", xml);
```

### Send（底层逃生舱口）

```csharp
using var request = new HttpRequestMessage(HttpMethod.Get, url);
request.Headers.Add("X-Custom", "value");
HttpResponseMessage response = await _httpHelper.SendAsync(request); // 返回原始响应，自行处理
```

## 请求头（HttpHeaders）

通过 `HttpSendOptions.Headers`（类型 `HttpHeaders`）传递请求头。单值用索引器直接赋字符串，多值用 `Add` 追加：

```csharp
// 单值
var result = await _httpHelper.GetAsync<User>(url, new HttpSendOptions
{
    Headers = new HttpHeaders
    {
        ["X-Trace-Id"] = "custom-trace-id",
        ["Accept-Language"] = "zh-CN"
    }
});

// 同名多值（如多个 Accept）
var headers = new HttpHeaders { ["Authorization"] = "Bearer xxx" };
headers.Add("Accept", "application/json");
headers.Add("Accept", "text/plain");
// 或一次多值：headers.Add("Accept", new[] { "application/json", "text/plain" });

var result = await _httpHelper.GetAsync<User>(url, new HttpSendOptions { Headers = headers });
```

## 认证

认证统一通过 `HttpSendOptions.Headers` 传递：

```csharp
// Bearer Token（CreateBearerHeaders 返回 HttpHeaders）
var result = await _httpHelper.GetAsync<User>(url,
    new HttpSendOptions { Headers = HttpHelperExtensions.CreateBearerHeaders("your-token-here") });

// API Key
var result = await _httpHelper.GetAsync<User>(url,
    new HttpSendOptions { Headers = new HttpHeaders { ["X-Api-Key"] = "your-api-key" } });

// Basic Auth
var basic = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("user:pass"));
var result = await _httpHelper.GetAsync<User>(url,
    new HttpSendOptions { Headers = new HttpHeaders { ["Authorization"] = basic } });
```

### CreateBearerHeaders

`HttpHelperExtensions.CreateBearerHeaders(token)` 自动补全 `"Bearer "` 前缀，返回可直接传入 `HttpSendOptions.Headers` 的 `HttpHeaders`：

```csharp
var headers = HttpHelperExtensions.CreateBearerHeaders("your-token-here");
// => Headers["Authorization"] = "Bearer your-token-here"（已带前缀不重复添加）

var result = await _httpHelper.GetAsync<User>(url, new HttpSendOptions { Headers = headers });
```

## 配置选项 HttpClientOptions

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `BaseAddress` | string? | null | 基础地址，请求 URL 为相对路径时自动拼接 |
| `DefaultHeaders` | HttpHeaders? | null | 每个请求自动携带的默认请求头（per-request Headers 优先覆盖）；支持同名多值 |
| `UserAgent` | string? | null | 自定义 User-Agent |
| `AuditLog` | bool | true | 是否启用审计日志 |
| `EnableLogRedaction` | bool | true | 是否启用日志脱敏 |
| `JsonNamingPolicy` | JsonNamingPolicyType | CamelCase | JSON 命名策略：CamelCase / PascalCase / SnakeCaseLower / None |
| `Timeout` | int | 100 | 总超时（秒），覆盖整条重试链；范围：1-3600 |
| `ConcurrencyLimit` | int | 100 | 并发限制，范围：0-10000；`0` 表示禁用限制 |
| `MaxRetryAttempts` | int | 3 | 最大重试次数，范围：0-10 |
| `RetryDelaySeconds` | int | 1 | 重试基础延迟（秒），指数退避，范围：1-300 |
| `MaxRequestBodyLength` | int | 4096 | 请求体日志最大输出长度，≥0。0 表示不限制 |
| `MaxOutputResponseLength` | int | 4096 | 响应体日志最大输出长度，≥0。0 表示不限制 |
| `IgnoreUntrustedCertificate` | bool | false | 是否忽略不安全的SSL证书，仅建议开发/测试环境使用 |
| `RetryOnUnauthorized` | bool | false | 401未授权错误时是否重试 |
| `AdditionalSensitiveHeaders` | ICollection\<string\> | 空 | 额外需要脱敏的请求头 |
| `AdditionalSensitiveFields` | ICollection\<string\> | 空 | 额外需要脱敏的字段名 |

> 以上取值范围由内置的 `HttpClientOptionsValidator` 在启动时校验，超出范围会导致 options 校验失败。

### 内置默认脱敏清单

启用日志脱敏（`EnableLogRedaction = true`，默认开启）时，默认脱敏器会自动遮蔽以下内容：

- 默认敏感请求头：`Authorization`、`Proxy-Authorization`、`Cookie`、`Set-Cookie`、`X-Api-Key`、`Api-Key`、`X-Auth-Token`
- 默认敏感字段（JSON key 与 `key=value` 文本）：`password`、`passwd`、`pwd`、`secret`、`token`、`access_token`、`refresh_token`、`client_secret`、`api_key`、`api-key`
- Bearer Token 值（形如 `Bearer xxx` 的字符串）

可通过 `AdditionalSensitiveHeaders` / `AdditionalSensitiveFields` 追加，或注册自定义 `IHttpLogRedactor` 完全替换脱敏逻辑。

## 异常处理

4.0 起统一为结果对象模型：失败始终返回 `IHttpResult(IsSuccess=false)`，不再有"抛异常 / 返回结果"双开关。需要抛异常的调用风格，显式调用 `EnsureSuccess()`。

```csharp
var result = await _httpHelper.GetAsync<User>(url);
if (!result.IsSuccess)
{
    _logger.LogWarning("请求失败: {StatusCode} - {Error}", result.StatusCode, result.ErrorMessage);
    return;
}
var user = result.Data;

// 或失败即抛异常：
var user = (await _httpHelper.GetAsync<User>(url)).EnsureSuccess().Data;
```

## JSON 序列化

请求体序列化与响应反序列化统一基于 `System.Text.Json`：

- 默认 `CamelCase` 命名策略，可通过 `HttpClientOptions.JsonNamingPolicy` 切换为 `PascalCase` / `SnakeCaseLower` / `None`
- 启用 `UnsafeRelaxedJsonEscaping`（中文等非 ASCII 字符不转义）
- 反序列化额外启用 `JsonStringEnumConverter`（枚举以字符串形式处理）
- 容忍注释与尾随逗号

> 命名策略在 net6/7 上通过内置自定义 `JsonNamingPolicy` 子类实现（PascalCase / SnakeCaseLower），net8+ 行为一致。

## 弹性策略

本库使用 Polly 实现了完整的弹性策略链，按以下顺序执行（从外层到内层）：

1. **降级处理（Fallback）** - 所有策略失败时返回 503 降级响应（`IsFallbackResponse = true`）
2. **总超时（Timeout）** - 覆盖整条重试链的总耗时上限
3. **并发限制（Concurrency Limiter）** - 限制同时进行的 HTTP 请求数量（`ConcurrencyLimit = 0` 时跳过）
4. **熔断器（Circuit Breaker）** - 错误率达到阈值时暂时停止请求
5. **重试策略（Retry）** - 自动重试 5xx、408、超时等失败请求

> 整个请求链（含所有重试）受单次 `Timeout` 上限约束；超时后由 Fallback 兜底为 503 降级响应。

## 日志

### 跳过请求日志

```csharp
var result = await _httpHelper.PostAsync<string>(url, data,
    new HttpSendOptions { Headers = new HttpHeaders { ["X-Skip-Logger"] = "" } });
```

通过设置 `X-Skip-Logger` 或 `X-Logger` 值为 `none`/`skip` 跳过日志。

### 自定义日志脱敏

```csharp
public sealed class CustomHttpLogRedactor : IHttpLogRedactor
{
    public string RedactContent(string content) => content;
    public IDictionary<string, string> RedactHeaders(IDictionary<string, string>? headers) => headers ?? new Dictionary<string, string>();
}

services.AddSingleton<IHttpLogRedactor, CustomHttpLogRedactor>();
services.AddHttpClientService();
```

## 目标框架

支持 .NET 6.0 / 7.0 / 8.0 / 9.0 / 10.0

## 版本更新记录

### 4.0.0

- **[破坏性变更]** 统一所有动词方法签名：查询参数与请求头收拢到新增的 `HttpSendOptions`（`Query` / `Headers`），所有方法参数顺序一致
- **[破坏性变更]** 删除 `FailThrowException` 开关与"失败抛异常 / 返回结果"双错误模型：失败统一返回 `IHttpResult(IsSuccess=false)`；需要抛异常显式调用新增的 `EnsureSuccess()` 扩展方法
- **[破坏性变更]** 删除所有非泛型 `string` 版方法，统一用泛型版（返回字符串用 `GetAsync<string>()` 等）
- **[破坏性变更]** 删除 `HttpRequestEnum` 与 `SendAsync(HttpRequestEnum, …)` 重载；保留 `SendAsync(HttpRequestMessage)` 底层逃生舱口
- **[破坏性变更]** 请求头类型改为 `HttpHeaders`：`HttpSendOptions.Headers` / `HttpClientOptions.DefaultHeaders` / `CreateBearerHeaders` 返回值均改为 `HttpHeaders`，支持同名多值（单值用索引器，多值用 `Add`）
- **[新增]** `HttpHeaders` 多值请求头集合类型，单值场景不啰嗦、多值场景原生支持
- **[新增]** `HttpClientOptions.JsonNamingPolicy` 配置项（CamelCase / PascalCase / SnakeCaseLower / None），适配不同后端字段约定
- **[变更]** Polly Fallback 异常路径统一兜底为 503 降级响应（不再按 `FailThrowException` 分叉）

### 3.0.1

- **[修复]** 移除 `ServiceCollectionExtensions` 中多余的 `TryAddTransient<LoggingHandler>()` 死注册

### 3.0.0

- **[破坏性变更]** 所有方法返回 `IHttpResult<T>` 包装结果，不再返回 `T`（失败时为 null）
- **[破坏性变更]** 移除 `bearerToken` 参数，认证统一通过 `headers` 传递
- 新增 `queryParameters` 参数、`DownloadFileAsync`、`IHttpResult<T>`、命名客户端与 `IHttpHelperFactory`

## 迁移总览（2.x → 3.0 → 4.0）

| 版本 | 关键变化 | 调用方迁移要点 |
|------|----------|----------------|
| 2.x → 3.0 | 返回值 `T` → `IHttpResult<T>`；`bearerToken` 参数移除 | `if (user != null)` → `if (result.IsSuccess)`；认证改用 `headers` |
| 3.0 → 4.0 | 统一签名（`HttpSendOptions`）；删 `FailThrowException`；删非泛型 string 版；删 `HttpRequestEnum`；请求头改 `HttpHeaders`；新增 `JsonNamingPolicy` | query/headers 收进 `HttpSendOptions`；非泛型 `GetAsync()` → `GetAsync<string>()`；`FailThrowException=true` → `EnsureSuccess()`；`SendAsync(HttpRequestEnum,…)` → `SendAsync(HttpRequestMessage)`；`new Dictionary<string,string>` headers → `new HttpHeaders` |

### 从 3.x 迁移到 4.0

```csharp
// 3.x —— query / headers 是分散的位置参数或命名参数
var result = await _httpHelper.GetAsync<User>(url, queryParameters: new { page = 1 }, headers: bearHeaders);
var s = await _httpHelper.GetAsync(url);
await _httpHelper.SendAsync(HttpRequestEnum.Post, url, content);
options.FailThrowException = true;

// 4.0 —— query / headers 收拢到 HttpSendOptions，字符串用 <string>，枚举版删除
var result = await _httpHelper.GetAsync<User>(url,
    new HttpSendOptions { Query = new { page = 1 }, Headers = bearHeaders /* HttpHeaders */ });
var s = await _httpHelper.GetAsync<string>(url);
using var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
var resp = await _httpHelper.SendAsync(req);
var user = (await _httpHelper.GetAsync<User>(url)).EnsureSuccess().Data; // 失败即抛异常改用 EnsureSuccess()
```

### 从 2.x 迁移到 3.0

```csharp
// 2.x - 直接返回 T，失败时为 null
var user = await _httpHelper.GetAsync<User>(url, bearerToken: "xxx");
if (user != null) { ... }

// 3.0 - 返回 IHttpResult<T>
var result = await _httpHelper.GetAsync<User>(url, headers: new Dictionary<string, string> { ["Authorization"] = "Bearer xxx" });
if (result.IsSuccess) { var user = result.Data; }
```
