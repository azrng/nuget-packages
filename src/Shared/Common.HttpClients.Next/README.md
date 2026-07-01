# Common.HttpClients

> 基于 Microsoft.Extensions.Http.Resilience 和 Polly 的 HTTP 客户端库，所有方法返回 `IHttpResult<T>` 结构化结果

## 主要特性

- 所有请求方法返回 `IHttpResult<T>`，包含 `IsSuccess`、`Data`、`ErrorMessage`、`StatusCode`、`RawBody` 等结构化信息
- 支持通过匿名对象、`IDictionary<string, string>`、`NameValueCollection` 自动构建 URL 查询参数
- 内置文件下载方法 `DownloadFileAsync`
- 认证信息统一通过 `headers` 传递，不再局限于 Bearer Token
- 提供 `HttpHelperExtensions.CreateBearerHeaders` 辅助方法，自动构造 Bearer Token 请求头
- 智能日志记录和审计（包含请求前后日志）
- 完整的 Polly 弹性策略（降级、并发限制、重试、熔断器、超时）
- 分布式追踪支持（X-Trace-Id 自动传播）
- 可扩展的日志脱敏（支持自定义敏感头和字段）

## 安装

```bash
dotnet add package Common.HttpClients --version 3.0.1
```

## 项目结构

```text
Common.HttpClients.Next/
├── Abstractions/        # 接口与抽象类型（IHttpHelper、IHttpResult、HttpClientOptions 等）
├── Client/              # IHttpHelper 默认实现（HttpClientHelper、HttpHelperFactory、HttpResult）
├── Extensions/          # DI 扩展（AddHttpClientService）与 HttpHelperExtensions
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
    options.FailThrowException = false;              // 失败时不抛出异常
    options.Timeout = 30;                            // 超时时间（秒）
    options.MaxRetryAttempts = 3;                    // 最大重试次数
    options.RetryDelaySeconds = 1;                   // 重试基础延迟（秒）
    options.ConcurrencyLimit = 100;                  // 并发限制
});
```

### 命名客户端与 IHttpHelperFactory（多服务 / 多 BaseAddress）

需要同时对接多个服务端、或为不同服务配置不同弹性策略时，使用命名重载按名注册：

```csharp
// 按名注册多个客户端（各自独立 BaseAddress / 超时 / 重试等）
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
        // 相对路径会自动拼接各自 BaseAddress
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
if (result.IsSuccess)
{
    var user = result.Data;
}

// 方式2：检查状态码
if (result.StatusCode == HttpStatusCode.NotFound)
{
    // 处理 404
}

// 方式3：区分降级响应
if (!result.IsSuccess && result.IsFallbackResponse)
{
    // Polly 所有重试都失败后的降级响应
}
```

## 请求方法

### GET 请求

```csharp
// 返回反序列化对象
var result = await _httpHelper.GetAsync<User>("https://api.example.com/users/1");

// 返回字符串
var result = await _httpHelper.GetAsync("https://api.example.com/users/1");

// 获取文件流
var result = await _httpHelper.GetStreamAsync("https://api.example.com/files/1");
if (result.IsSuccess)
{
    using var stream = result.Data;
    // 处理流...
}
```

### 查询参数

所有方法支持通过 `queryParameters` 自动构建 URL 查询字符串，支持匿名对象、`IDictionary<string, string>`、`NameValueCollection`：

```csharp
// 匿名对象
var result = await _httpHelper.GetAsync<List<User>>(
    "https://api.example.com/users",
    queryParameters: new { page = 1, pageSize = 20, keyword = "test" }
);
// => https://api.example.com/users?page=1&pageSize=20&keyword=test

// IDictionary
var params = new Dictionary<string, string>
{
    ["page"] = "1",
    ["pageSize"] = "20"
};
var result = await _httpHelper.GetAsync<List<User>>("https://api.example.com/users", queryParameters: params);

// 集合参数自动展开
var result = await _httpHelper.GetAsync<string>(
    "https://api.example.com/filter",
    queryParameters: new { ids = new[] { 1, 2, 3 } }
);
// => https://api.example.com/filter?ids=1&ids=2&ids=3
```

### POST 请求

```csharp
// JSON 格式（传递对象）
var user = new User { Name = "张三", Age = 25 };
var result = await _httpHelper.PostAsync<User>("https://api.example.com/users", user);

// JSON 格式（传递字符串）
var json = "{\"name\":\"张三\",\"age\":25}";
var result = await _httpHelper.PostAsync<string>("https://api.example.com/users", json);
```

### POST Form-Data

```csharp
// 传递文本参数
var data = new Dictionary<string, string>
{
    ["username"] = "admin",
    ["password"] = "123456"
};
var result = await _httpHelper.PostFormDataAsync<LoginResponse>("https://api.example.com/login", data);

// 上传单个文件
using var stream = File.OpenRead("photo.jpg");
var result = await _httpHelper.PostFormDataAsync<UploadResponse>(
    "https://api.example.com/upload",
    "file", stream, "photo.jpg"
);

// 上传多个文件/混合参数
using var form = new MultipartFormDataContent();
using var fileContent = new ByteArrayContent(fileBytes);
fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
{
    Name = "file",
    FileName = "document.pdf"
};
form.Add(fileContent);
form.Add(new StringContent("备注信息"), "remark");

var result = await _httpHelper.PostFormDataAsync<UploadResponse>("https://api.example.com/upload", form);
```

### PUT / PATCH / DELETE

```csharp
// PUT
var result = await _httpHelper.PutAsync<User>("https://api.example.com/users/1", updatedUser);

// PATCH
var result = await _httpHelper.PatchAsync<User>("https://api.example.com/users/1", new { name = "李四" });

// DELETE（返回字符串）
var result = await _httpHelper.DeleteAsync("https://api.example.com/users/1");

// DELETE（返回反序列化对象）
var result = await _httpHelper.DeleteAsync<DeleteResponse>("https://api.example.com/users/1");
```

### 文件下载

```csharp
var result = await _httpHelper.DownloadFileAsync(
    "https://api.example.com/files/report.pdf",
    @"C:\Downloads\report.pdf"
);

if (result.IsSuccess)
{
    Console.WriteLine($"下载完成: {result.Data.FilePath}");
    Console.WriteLine($"文件大小: {result.Data.FileSize} bytes");
}
```

下载失败时会自动清理不完整的文件。

### SOAP 请求

```csharp
var xml = @"<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <GetUser xmlns=""http://example.com"">
            <Id>1</Id>
        </GetUser>
    </soap:Body>
</soap:Envelope>";

var result = await _httpHelper.PostSoapAsync<SoapResponse>("https://api.example.com/soap", xml);
```

### Send（底层方法）

```csharp
// 使用 HttpRequestEnum
var result = await _httpHelper.SendAsync(HttpRequestEnum.Post, url, httpContent);

// 逃生舱口：直接操作 HttpRequestMessage
using var request = new HttpRequestMessage(HttpMethod.Get, url);
request.Headers.Add("X-Custom", "value");
HttpResponseMessage response = await _httpHelper.SendAsync(request);
```

## 认证

### 通过 headers 传递

```csharp
// Bearer Token
var headers = new Dictionary<string, string>
{
    ["Authorization"] = "Bearer your-token-here"
};
var result = await _httpHelper.GetAsync<User>(url, headers: headers);

// API Key
var headers = new Dictionary<string, string>
{
    ["X-Api-Key"] = "your-api-key"
};
var result = await _httpHelper.GetAsync<User>(url, headers: headers);

// Basic Auth
var headers = new Dictionary<string, string>
{
    ["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("user:pass"))
};
var result = await _httpHelper.GetAsync<User>(url, headers: headers);
```

### 使用 HttpHelperExtensions.CreateBearerHeaders（Bearer Token 便利构造）

3.x 不再为每个 HTTP 方法单独提供 `bearerToken` 重载，而是提供一个静态辅助方法 `HttpHelperExtensions.CreateBearerHeaders(token)`，自动补全 `"Bearer "` 前缀，返回可直接传入 `headers` 参数的字典：

```csharp
using Common.HttpClients;

// CreateBearerHeaders 会自动补 "Bearer " 前缀，返回 Authorization 头字典
var headers = HttpHelperExtensions.CreateBearerHeaders("your-token-here");
// => { ["Authorization"] = "Bearer your-token-here" }

// 已带前缀时不会重复添加
var headers2 = HttpHelperExtensions.CreateBearerHeaders("Bearer your-token-here");
// => 同样是 { ["Authorization"] = "Bearer your-token-here" }

var result = await _httpHelper.GetAsync<User>(url, headers: headers);
var result = await _httpHelper.PostAsync<User>(url, data, headers: headers);
var result = await _httpHelper.DownloadFileAsync(url, filePath, headers: headers);
```

> 注：`CreateBearerHeaders` 仅为构造请求头的便捷方法，不改变请求行为；它适用于所有接受 `headers` 参数的请求方法。

## 请求头

所有方法支持通过 `headers` 参数传递自定义请求头：

```csharp
var headers = new Dictionary<string, string>
{
    ["X-Trace-Id"] = "custom-trace-id",
    ["X-Tenant-Id"] = "tenant-001",
    ["Accept-Language"] = "zh-CN"
};

var result = await _httpHelper.GetAsync<User>(url, headers: headers);
```

## 配置选项 HttpClientOptions

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `BaseAddress` | string? | null | 基础地址，请求 URL 为相对路径时自动拼接 |
| `DefaultHeaders` | IDictionary\<string,string\>? | null | 每个请求自动携带的默认请求头（per-request headers 优先覆盖） |
| `UserAgent` | string? | null | 自定义 User-Agent |
| `AuditLog` | bool | true | 是否启用审计日志 |
| `FailThrowException` | bool | false | 失败时是否抛出异常。false 时返回 IHttpResult（IsSuccess=false），true 时抛出 HttpRequestException |
| `EnableLogRedaction` | bool | true | 是否启用日志脱敏 |
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

启用日志脱敏（`EnableLogRedaction = true`，默认开启）时，默认脱敏器会自动遮蔽以下内容，无需额外配置：

- 默认敏感请求头：`Authorization`、`Proxy-Authorization`、`Cookie`、`Set-Cookie`、`X-Api-Key`、`Api-Key`、`X-Auth-Token`
- 默认敏感字段（JSON key 与 `key=value` 文本）：`password`、`passwd`、`pwd`、`secret`、`token`、`access_token`、`refresh_token`、`client_secret`、`api_key`、`api-key`
- Bearer Token 值（形如 `Bearer xxx` 的字符串）

可通过 `AdditionalSensitiveHeaders` / `AdditionalSensitiveFields` 追加，或注册自定义 `IHttpLogRedactor` 完全替换脱敏逻辑。

## 异常处理

### FailThrowException = false（默认）

```csharp
options.FailThrowException = false;

var result = await _httpHelper.GetAsync<User>(url);
if (!result.IsSuccess)
{
    _logger.LogWarning("请求失败: {StatusCode} - {Error}", result.StatusCode, result.ErrorMessage);
}
```

### FailThrowException = true

```csharp
options.FailThrowException = true;

try
{
    var result = await _httpHelper.GetAsync<User>(url);
    // 成功时 result.IsSuccess 一定为 true
}
catch (HttpRequestException ex)
{
    // 请求失败时抛出异常
}
```

## JSON 序列化

请求体序列化与响应反序列化统一基于 `System.Text.Json`，约定如下：

- 序列化使用 camelCase 命名策略，并启用 `UnsafeRelaxedJsonEscaping`（中文等非 ASCII 字符不转义）
- 反序列化在上述基础上额外启用 `JsonStringEnumConverter`（枚举以字符串形式处理）
- 容忍注释与尾随逗号

> 与 Newtonsoft.Json 行为有差异，迁移时请注意命名策略与枚举处理。

## 弹性策略

本库使用 Polly 实现了完整的弹性策略链，按以下顺序执行（从外层到内层）：

1. **降级处理（Fallback）** - 所有策略失败时返回 503 响应（`IsFallbackResponse = true`）或重新抛出异常
2. **总超时（Timeout）** - 覆盖整条重试链的总耗时上限
3. **并发限制（Concurrency Limiter）** - 限制同时进行的 HTTP 请求数量（`ConcurrencyLimit = 0` 时跳过）
4. **熔断器（Circuit Breaker）** - 错误率达到阈值时暂时停止请求
5. **重试策略（Retry）** - 自动重试 5xx、408、超时等失败请求

> 整个请求链（含所有重试）受单次 `Timeout` 上限约束；超时后由 Fallback 兜底。

## 日志

### 跳过请求日志

```csharp
var result = await _httpHelper.PostAsync<string>(url, data,
    headers: new Dictionary<string, string> { { "X-Skip-Logger", "" } });
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

### 3.0.1

- **[修复]** 移除 `ServiceCollectionExtensions` 中多余的 `TryAddTransient<LoggingHandler>()` 死注册。该注册因 `LoggingHandler` 构造函数首参为 `string clientName` 无法被 DI 容器直接激活，在开启 `ValidateOnBuild` 的环境（如 ASP.NET Core Development 默认行为）下会导致 `builder.Build()` 抛出 `Unable to resolve service for type 'System.String'` 启动异常。实际 `LoggingHandler` 由 `AddHttpMessageHandler` 通过 `ActivatorUtilities` 注入客户端名称创建，不受此改动影响

### 3.0.0

- **[破坏性变更]** 所有方法返回 `IHttpResult<T>` 包装结果，不再返回 `T`（失败时为 null）
- **[破坏性变更]** 移除 `bearerToken` 参数，认证统一通过 `headers` 传递
- 新增 `queryParameters` 参数，支持匿名对象/IDictionary/NameValueCollection 自动构建 URL 查询字符串
- 新增 `DownloadFileAsync` 文件下载方法
- 新增 `HttpHelperExtensions` 扩展方法，提供 Bearer Token 便利重载
- 新增 `IHttpResult<T>` 接口，包含 `IsSuccess`、`Data`、`ErrorMessage`、`StatusCode`、`RawBody`、`IsFallbackResponse`
- 新增命名客户端与 `IHttpHelperFactory`：`AddHttpClientService(name, configure)` 注册多个独立客户端，`IHttpHelperFactory.CreateClient(name)` 按名获取，支持不同 BaseAddress / 弹性策略
- `HttpClientOptions` 新增 `BaseAddress`（相对路径自动拼接）、`UserAgent`、`DefaultHeaders`（默认请求头）

### 从 2.x 迁移到 3.0

```csharp
// 2.x - 直接返回 T，失败时为 null
var user = await _httpHelper.GetAsync<User>(url, bearerToken: "xxx");
if (user != null) { ... }

// 3.0 - 返回 IHttpResult<T>，认证统一走 headers
var result = await _httpHelper.GetAsync<User>(url, headers: new Dictionary<string, string>
{
    ["Authorization"] = "Bearer xxx"
});
if (result.IsSuccess) { var user = result.Data; }

// 3.0 - 或用 CreateBearerHeaders 简便构造请求头
var headers = HttpHelperExtensions.CreateBearerHeaders("xxx");
var result = await _httpHelper.GetAsync<User>(url, headers: headers);
if (result.IsSuccess) { var user = result.Data; }
```
