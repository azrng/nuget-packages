# Common.HttpClients

> 基于 Microsoft.Extensions.Http.Resilience 和 Polly 的 HTTP 客户端库，所有方法返回 `IHttpResult<T>` 结构化结果

## 主要特性

- 所有请求方法返回 `IHttpResult<T>`，包含 `IsSuccess`、`Data`、`ErrorMessage`、`StatusCode`、`RawBody` 等结构化信息
- 支持通过匿名对象、`IDictionary<string, string>`、`NameValueCollection` 自动构建 URL 查询参数
- 内置文件下载方法 `DownloadFileAsync`
- 认证信息统一通过 `headers` 传递，不再局限于 Bearer Token
- 提供 `HttpHelperExtensions` 扩展方法，Bearer Token 场景仍然简便
- 智能日志记录和审计（包含请求前后日志）
- 完整的 Polly 弹性策略（降级、并发限制、重试、熔断器、超时）
- 分布式追踪支持（X-Trace-Id 自动传播）
- 可扩展的日志脱敏（支持自定义敏感头和字段）

## 安装

```bash
dotnet add package Common.HttpClients --version 3.0.0
```

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

### 使用扩展方法（Bearer Token 便利重载）

```csharp
using Common.HttpClients;

// 直接传递 token 字符串，自动添加 "Bearer " 前缀
var result = await _httpHelper.GetAsync<User>(url, "your-token-here");

// 也支持带查询参数
var result = await _httpHelper.GetAsync<List<User>>(url, "your-token-here");

// POST + Token
var result = await _httpHelper.PostAsync<User>(url, data, "your-token-here");

// 下载 + Token
var result = await _httpHelper.DownloadFileAsync(url, filePath, "your-token-here");
```

扩展方法覆盖了所有 HTTP 方法（GET、POST、PUT、PATCH、DELETE、PATCH、GetStream、PostFormData、PostSoap、DownloadFile）。

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
| `AuditLog` | bool | true | 是否启用审计日志 |
| `FailThrowException` | bool | false | 失败时是否抛出异常。false 时返回 IHttpResult（IsSuccess=false），true 时抛出 HttpRequestException |
| `EnableLogRedaction` | bool | true | 是否启用日志脱敏 |
| `Timeout` | int | 100 | 超时时间（秒），范围：1-3600 |
| `ConcurrencyLimit` | int | 100 | 并发限制，范围：1-10000 |
| `MaxRetryAttempts` | int | 3 | 最大重试次数，范围：0-10 |
| `RetryDelaySeconds` | int | 1 | 重试基础延迟（秒），指数退避，范围：1-300 |
| `MaxRequestBodyLength` | int | 4096 | 请求体日志最大输出长度，≥0。0 表示不限制 |
| `MaxOutputResponseLength` | int | 4096 | 响应体日志最大输出长度，≥0。0 表示不限制 |
| `IgnoreUntrustedCertificate` | bool | false | 是否忽略不安全的SSL证书，仅建议开发/测试环境使用 |
| `RetryOnUnauthorized` | bool | false | 401未授权错误时是否重试 |
| `AdditionalSensitiveHeaders` | ICollection\<string\> | 空 | 额外需要脱敏的请求头 |
| `AdditionalSensitiveFields` | ICollection\<string\> | 空 | 额外需要脱敏的字段名 |

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

## 弹性策略

本库使用 Polly 实现了完整的弹性策略链，按以下顺序执行（从外层到内层）：

1. **降级处理（Fallback）** - 所有策略失败时返回 503 响应（`IsFallbackResponse = true`）或重新抛出异常
2. **并发限制（Concurrency Limiter）** - 限制同时进行的 HTTP 请求数量
3. **重试策略（Retry）** - 自动重试 5xx、408、超时等失败请求
4. **熔断器（Circuit Breaker）** - 错误率达到阈值时暂时停止请求
5. **超时策略（Timeout）** - 防止请求长时间阻塞

> 总超时上界约为：`Timeout × (MaxRetryAttempts + 1) + 重试延迟总和`

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
    public IDictionary<string, string> RedactHeaders(IDictionary<string, string> headers) => headers;
}

services.AddSingleton<IHttpLogRedactor, CustomHttpLogRedactor>();
services.AddHttpClientService();
```

## 目标框架

支持 .NET 6.0 / 7.0 / 8.0 / 9.0 / 10.0

## 版本更新记录

### 3.0.0

- **[破坏性变更]** 所有方法返回 `IHttpResult<T>` 包装结果，不再返回 `T`（失败时为 null）
- **[破坏性变更]** 移除 `bearerToken` 参数，认证统一通过 `headers` 传递
- 新增 `queryParameters` 参数，支持匿名对象/IDictionary/NameValueCollection 自动构建 URL 查询字符串
- 新增 `DownloadFileAsync` 文件下载方法
- 新增 `HttpHelperExtensions` 扩展方法，提供 Bearer Token 便利重载
- 新增 `IHttpResult<T>` 接口，包含 `IsSuccess`、`Data`、`ErrorMessage`、`StatusCode`、`RawBody`、`IsFallbackResponse`

### 从 2.x 迁移到 3.0

```csharp
// 2.x - 直接返回 T，失败时为 null
var user = await _httpHelper.GetAsync<User>(url, bearerToken: "xxx");
if (user != null) { ... }

// 3.0 - 返回 IHttpResult<T>
var result = await _httpHelper.GetAsync<User>(url, headers: new Dictionary<string, string>
{
    ["Authorization"] = "Bearer xxx"
});
if (result.IsSuccess) { var user = result.Data; }

// 3.0 - 使用扩展方法保持简便
var result = await _httpHelper.GetAsync<User>(url, "xxx");
if (result.IsSuccess) { var user = result.Data; }
```
