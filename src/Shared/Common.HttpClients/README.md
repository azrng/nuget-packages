# Common.HttpClients

一个功能丰富的HTTP客户端库，基于 Microsoft.Extensions.Http.Resilience 和 Polly，提供强大的弹性和韧性功能。

## 主要特性

- 🚀 高性能HTTP客户端
- 📝 智能日志记录和审计（包含请求前后日志）
- ⚙️ 灵活的配置管理（支持运行时验证）
- 🔒 请求/响应拦截
- 📊 响应内容长度控制
- 🎯 请求级别的日志控制
- 🔄 异常或超时自动重试（支持自定义超时时间、重试次数、延迟）
- 🛡️ 完整的 Polly 弹性策略（降级、并发限制、重试、熔断器、超时）
- 🔍 分布式追踪支持（X-Trace-Id 自动传播）
- 🔐 支持忽略不安全的SSL证书（仅建议开发/测试环境使用）
- ⚡ 401未授权错误可配置重试
- 🔏 可扩展的日志脱敏（支持自定义敏感头和字段）

## 安装

```bash
dotnet add package Common.HttpClients
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
    options.FailThrowException = false;              // 失败时不抛出异常，返回 null
    options.Timeout = 30;                            // 自定义超时时间（秒），范围：1-3600
    options.MaxRetryAttempts = 3;                    // 最大重试次数，范围：0-10
    options.RetryDelaySeconds = 1;                   // 重试基础延迟（秒），范围：1-300
    options.ConcurrencyLimit = 100;                  // 并发限制，范围：1-10000
    options.MaxRequestBodyLength = 4096;             // 请求体日志默认保留 4KB
    options.MaxOutputResponseLength = 4096;          // 响应体日志默认保留 4KB
    options.IgnoreUntrustedCertificate = false;      // ⚠️ 生产环境建议设为 false
    options.RetryOnUnauthorized = true;              // 401未授权错误时自动重试
    options.AdditionalSensitiveHeaders = new[] { "X-Secret" }; // 额外脱敏请求头
    options.AdditionalSensitiveFields = new[] { "mobile" };    // 额外脱敏字段
});

// ⚠️ 无效配置将抛出异常
// services.AddHttpClientService(options =>
// {
//     options.Timeout = 5000; // ❌ ArgumentOutOfRangeException: Timeout必须在1-3600秒之间
// });
```

### 2. 使用HTTP客户端

```csharp
public class MyService
{
    private readonly IHttpHelper _httpHelper;

    public MyService(IHttpHelper httpHelper)
    {
        _httpHelper = httpHelper;
    }

    public async Task<string> GetDataAsync()
    {
        var result = await _httpHelper.GetAsync<string>(Host + "/get?q1=11&q2=22");

        return result;
    }
}
```

## 配置选项 HttpClientOptions

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `AuditLog` | bool | true | 是否启用审计日志 |
| `FailThrowException` | bool | false | 失败时是否抛出异常。false 时返回 null，true 时抛出异常 |
| `EnableLogRedaction` | bool | true | 是否启用日志脱敏 |
| `Timeout` | int | 100 | 超时时间（秒），范围：1-3600 |
| `ConcurrencyLimit` | int | 100 | 并发限制，范围：1-10000，建议按下游容量调整 |
| `MaxRetryAttempts` | int | 3 | 最大重试次数，范围：0-10 |
| `RetryDelaySeconds` | int | 1 | 重试基础延迟（秒），指数退避，范围：1-300 |
| `MaxRequestBodyLength` | int | 4096 | 请求体日志最大输出长度，≥0。0 表示不限制 |
| `MaxOutputResponseLength` | int | 4096 | 响应体日志最大输出长度，≥0。0 表示不限制 |
| `IgnoreUntrustedCertificate` | bool | false | 是否忽略不安全的SSL证书，⚠️ 仅建议开发/测试环境使用 |
| `RetryOnUnauthorized` | bool | false | 401未授权错误时是否重试 |
| `AdditionalSensitiveHeaders` | ICollection\<string\> | 空 | 额外需要脱敏的请求头（不区分大小写） |
| `AdditionalSensitiveFields` | ICollection\<string\> | 空 | 额外需要脱敏的字段名（JSON/key=value，不区分大小写） |

> **配置验证**：所有参数都有范围限制，超出范围将抛出 `ArgumentOutOfRangeException`。


## 请求

下面示例已经注入IHttpHelper

### Get

```c#
var result = await _httpHelper.GetAsync<string>(Host + "/get?q1=11&q2=22");
```

还支持传递token以及传递请求头

### Post

#### Json格式

支持传递字符串以及对象

```c#
var content = "{\"q\":\"123456\",\"a\":\"222\"}";
var result = await _httpHelper.PostAsync<string>(Host + "/post", content);
```

#### PostFormData

* Task&lt;T&gt; PostFormDataAsync&lt;T&gt;(string url, MultipartFormDataContent formDataContent);

##### 请求示例

```c#
using var form = new MultipartFormDataContent();

// bytes为文件字节数组
using var fileContent = new ByteArrayContent(bytes);
fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                                         {
                                             Name = "file", // 表单字段名称
                                             FileName = fileName // 文件名
                                         };
fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
form.Add(fileContent);

// 其他参数
using var content = new StringContent("其他参数值");
form.Add(content, "其他参数名称");

var requestUrl = $"{_difyApiBase}/v1/files/upload";
var response = await _httpHelper.PostFormDataAsync<FileUploadResponse>(requestUrl, form,
    new Dictionary<string, string> { { "Authorization", $"Bearer {_difyApiKey}" } });
```

## 日志

可以设置配置AuditLog来设置是否启用审计日志，默认为启用状态。

```csharp
builder.Services.AddHttpClientService();
```

也可以为指定地址请求设置关闭审计日志，例如

```csharp
var result = await _httpHelper.PostAsync<string>(Host + "/anything", list,
    headers: new Dictionary<string, string>() { { "X-Logger", "skip" } });

var result2 = await _httpHelper.PostAsync<string>(Host + "/anything", list,
    headers: new Dictionary<string, string>() { { "X-Skip-Logger", "" } });
```

可以通过在请求头设置`X-Skip-Logger`或者设置`X-Logger`值为none、skip进行跳过日志

### 日志脱敏说明

- 默认脱敏字段包含：`password`、`token`、`access_token`、`refresh_token`、`api_key` 等常见字段
- 默认脱敏请求头包含：`Authorization`、`Cookie`、`X-Api-Key` 等
- 可通过 `AdditionalSensitiveFields`、`AdditionalSensitiveHeaders` 扩展脱敏范围
- 当前脱敏主要覆盖 JSON 和 `key=value` 文本，不保证覆盖所有嵌套/编码场景（例如复杂嵌套 JSON、base64 token）

> `GetStreamAsync` 会自动跳过响应体审计，避免流式读取场景下日志提前消费响应流。

## 弹性策略

本库使用 Polly 实现了完整的弹性策略链，按以下顺序执行（从外层到内层）：

### 1. 降级处理（Fallback）
当所有策略都失败时的最后保障：
- 如果 `FailThrowException = false`：返回 503 响应，方法返回 `null`
  - 响应包含 `X-Fallback-Response: true` 头，可区分真实服务端错误
- 如果 `FailThrowException = true`：重新抛出原始异常

### 2. 并发限制（Concurrency Limiter）
限制同时进行的HTTP请求数量，默认 `ConcurrencyLimit = 100`，可按业务压测结果调整

### 3. 重试策略（Retry）
自动重试失败的请求：
- **重试次数**：`MaxRetryAttempts`（默认 3）
- **重试延迟**：`RetryDelaySeconds`（默认 1 秒）作为基础值，使用指数退避
- **重试条件**：
  - HTTP 5xx 服务器错误
  - HTTP 408 请求超时
  - HTTP 401 未授权（如果 `RetryOnUnauthorized = true`）
  - 超时异常（`TimeoutException`、`TaskCanceledException`、`TimeoutRejectedException`）
  - HTTP 请求异常（`HttpRequestException`）

### 4. 熔断器（Circuit Breaker）
当错误率达到阈值时暂时停止请求，保护下游系统

### 5. 超时策略（Timeout）
防止请求长时间阻塞：
- 使用配置的 `Timeout` 值（默认 100 秒）
- 每次重试都会重新应用超时限制
- 超时后会触发重试机制

> **重要说明**：超时策略放在最内层，每次重试都会应用超时限制。总超时上界约为：`Timeout × (MaxRetryAttempts + 1) + 重试延迟总和`。

## 分布式追踪

本库自动支持分布式追踪，通过 `X-Trace-Id` 请求头传播追踪ID：

### 追踪ID获取优先级
1. 从当前请求的 `X-Trace-Id` 请求头获取
2. 从 `HttpContext.Request.Headers` 中获取 `X-Trace-Id`
3. 使用 ASP.NET Core 的 `HttpContext.TraceIdentifier`
4. 如果都没有，自动生成新的 GUID

### 日志示例
所有日志都包含 TraceId，方便追踪整个请求链路：

```
Http请求开始.TraceId：a1b2c3d4e5f6 Url：https://api.example.com/data Method：GET
Http请求审计日志.TraceId：a1b2c3d4e5f6 Url：https://api.example.com/data Method：GET StatusCode：OK 耗时：1234.56ms
```

## 超时与重试说明

### 超时机制
- **HttpClient.Timeout**：设置为无限，不控制超时
- **Polly Timeout 策略**：完全控制超时行为，使用配置的 `Timeout` 值
- **CancellationToken**：用于调用方主动取消请求，不与 Resilience 超时策略冲突

### 重试机制示例

#### 场景1：自定义超时 30 秒
```csharp
options.Timeout = 30;
options.MaxRetryAttempts = 2;
options.RetryDelaySeconds = 1;
```
- 单次请求超过 30 秒 → 触发重试
- 最多重试 2 次（共 3 次尝试），每次都有 30 秒超时限制
- 重试延迟：1s → 2s
- 总耗时：最长约 93 秒（30s×3 + 1s + 2s）

#### 场景2：使用默认超时 100 秒
```csharp
// 不设置 Timeout，使用默认值 100 秒
```
- 单次请求超过 100 秒 → 触发重试
- 默认最多重试 3 次（共 4 次尝试），每次都有 100 秒超时限制
- 默认总耗时：最长约 407 秒（100s×4 + 1s + 2s + 4s）

#### 场景3：401 未授权重试
```csharp
options.RetryOnUnauthorized = true;
```
- 收到 401 响应 → 触发重试
- 适用于 token 自动刷新场景
- 重试延迟：1s → 2s → 4s

## 异常处理

### FailThrowException = false（默认）
```csharp
options.FailThrowException = false;
```
- 请求失败或超时：返回 `null` 或 `default(T)`
- 错误信息记录在日志中
- 适合不需要中断业务流程的场景
- Fallback 产生的 503 响应包含 `X-Fallback-Response: true` 头，可据此判断是否为 Fallback 响应

### FailThrowException = true
```csharp
options.FailThrowException = true;
```
- 请求失败或超时：抛出异常
- 需要业务代码使用 try-catch 处理
- 适合需要明确处理错误的场景

### 识别 Fallback 响应示例

```csharp
// 使用原始 SendAsync 获取完整响应
using var request = new HttpRequestMessage(HttpMethod.Get, url);
var response = await _httpHelper.SendAsync(request);

if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
{
    var isFallback = response.Headers.Contains("X-Fallback-Response");
    if (isFallback)
    {
        // 这是 Fallback 产生的响应，说明所有重试都失败了
        _logger.LogWarning("All retries failed for {Url}", url);
    }
    else
    {
        // 这是真实的服务端 503 错误
        _logger.LogWarning("Service unavailable at {Url}", url);
    }
}
```

## 安全注意事项

### SSL证书验证

⚠️ **生产环境安全警告**：

```csharp
// ❌ 不要在生产环境使用
options.IgnoreUntrustedCertificate = true;
```

`IgnoreUntrustedCertificate` 选项会完全禁用SSL证书验证，这会使您的应用容易受到中间人攻击（MITM）。

**建议做法**：
- 仅在开发/测试环境使用此选项
- 生产环境应使用有效的SSL证书
- 考虑使用环境变量控制：
  ```csharp
  #if DEBUG
  options.IgnoreUntrustedCertificate = true;
  #else
  options.IgnoreUntrustedCertificate = false;
  #endif
  ```

### 敏感信息日志

默认情况下，库会自动脱敏常见的敏感字段和请求头。但如果您的 API 使用自定义字段名（如 `userSecret`、`apiKey` 等），请务必配置：

```csharp
options.AdditionalSensitiveFields = new[] { "userSecret", "customToken" };
options.AdditionalSensitiveHeaders = new[] { "X-Custom-Auth" };
```

## 版本更新记录

* 2.1.0
  * 新增 `MaxRequestBodyLength` 配置项，用于限制请求体日志输出长度
  * `MaxOutputResponseLength` 用于限制响应体日志输出长度
  * 审计日志默认仅保留请求体和响应体前 4096 个字符
  * 优化 `LoggingHandler` 的请求体和响应体日志截断逻辑
* 2.0.0
  * **[破坏性变更]** 移除 `IHttpHelper` 全部方法中的 `int? timeout` 参数，避免与 Resilience `Timeout` 策略冲突
  * 请求超时统一由 `AddHttpClientService(options => options.Timeout = xx)` 全局配置控制
  * 单次请求如需提前终止，请使用 `CancellationToken`
  * 新增可配置项：
    * `ConcurrencyLimit`：并发限制数量（默认 100）
    * `MaxRetryAttempts`：最大重试次数（默认 3）
    * `RetryDelaySeconds`：重试基础延迟（默认 1 秒）
    * `AdditionalSensitiveHeaders`：额外脱敏请求头
    * `AdditionalSensitiveFields`：额外脱敏字段
  * 日志脱敏支持自定义扩展字段与请求头
  * 新增配置参数验证（范围限制）
  * Fallback 响应添加 `X-Fallback-Response` 标识头
  * 优化 `JsonHelper` 性能（使用静态配置）
  * 优化 `ResponseStream.DisposeAsync` 释放顺序
  * 修复流式请求日志审计冲突（`GetStreamAsync` 自动跳过响应体审计）
* 1.3.3
  * 传递bearerToken的时候主动判断是否拼接Bearer头
* 1.3.2
  * 更新jwtToken命名为bearerToken
* 1.3.1
  * 支持.Net10
  * 支持超时或者错误后自动重试
* 1.3.1-beta3
  * 引用.Net10正式包
* 1.3.1-beta2
  * 重试测试
  * 日志输出增加请求耗时
* 1.3.1-beta1
    * 支持设置是否忽略不安全的SSL证书
* 1.3.0-beta9
    * 更新响应日志输出内容
    * 增加支持CancellationToken
    * 移除对.NetStandard2.1支持
* 1.3.0-beta8
    * 优化单独请求的日志输出
* 1.3.0-beta7
    * 修复调用接口报错在忽略异常的情况下扔抛出错误
* 1.3.0-beta6
    * 优化审计日志
* 1.3.0-beta5
    * 增加全局设置超时时间以及针对指定请求设置超时时间
* 1.3.0-beta4
    * 修改PostFormDataAsync方法，增加直接传递jwtToken入参
* 1.3.0-beta3
    * 修复LoggingHandler被错误重用的问题，将其生命周期改为Transient
* 1.3.0-beta2
    * 增加流式响应PostGetStreamAsync
    * 暴漏基础的SendAsync
* 1.3.0-beta1
    * 支持.Net9
    * 增加请求审计日志
* 1.2.3
    * 注入的时候支持设置是否异常直接抛出
* 1.2.2
    * 增加x-www-form-urlencoded请求方式代码
    * 升级支持.Net8
* 1.2.1
    * 增加get获取文件流的方法
* 1.2.0
    * 升级支持.net7
* 1.1.5
    * 修改put请求命名问题
    * 增加patch请求
* 1.1.4
    * 处理多个构造函数的报错
    * 增加更加灵活的请求方式Send
* 1.1.3
    * 增加http请求FormData形式去提交文件
    * 支持框架netstandard2.1、net6.0
* 1.1.2
    * 更新post方法同时兼容string和其他类型
* 1.1.1
    * 更新post方法,配置多个目标框架
* 1.1.0
    * 更新框架版本为5.0
* 1.0.0
    * 3.1版本的http请求公共库
