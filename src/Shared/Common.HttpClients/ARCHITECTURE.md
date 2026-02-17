# Common.HttpClients 架构与原理说明

## 1. 项目概述

`Common.HttpClients` 是一个基于 .NET 的功能丰富的 HTTP 客户端库，它构建在 `Microsoft.Extensions.Http` 和 `Polly` 弹性框架之上，提供了一套完整的 HTTP 请求解决方案，包括智能日志记录、审计追踪、弹性策略和分布式追踪支持。

### 1.1 设计目标

- **弹性**：通过 Polly 策略提供自动重试、熔断、超时等弹性能力
- **可观测性**：提供完整的请求/响应审计日志和分布式追踪
- **易用性**：简洁的 API 设计，开箱即用的默认配置
- **灵活性**：支持多种请求方式、自定义配置和扩展点

## 2. 核心架构

### 2.1 整体架构图

```
┌─────────────────────────────────────────────────────────────────┐
│                         应用层 (Application)                      │
│                    使用 IHttpHelper 接口                         │
└─────────────────────────────┬───────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      服务注册层 (Registration)                    │
│                 ServiceCollectionExtensions                      │
│                  AddHttpClientService(...)                       │
└─────────────────────────────┬───────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      HttpClientFactory                           │
│                  (IHttpClientFactory - named "default")          │
└─────────────────────────────┬───────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    处理器管道 (Handler Pipeline)                  │
├─────────────────────────────────────────────────────────────────┤
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Polly Resilience Handler (弹性策略处理器)                   │ │
│  │  ┌──────────────────────────────────────────────────────┐  │ │
│  │  │ 1. Fallback (降级处理)                                │  │ │
│  │  │ 2. ConcurrencyLimiter (并发限制: 100)                 │  │ │
│  │  │ 3. Retry (重试: 最多3次, 指数退避)                     │  │ │
│  │  │ 4. CircuitBreaker (熔断器)                            │  │ │
│  │  │ 5. Timeout (超时控制)                                 │  │ │
│  │  └──────────────────────────────────────────────────────┘  │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                    │
│                              ▼                                    │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  LoggingHandler (日志处理器)                                │ │
│  │  - 请求前日志                                               │ │
│  │  - 响应后审计日志                                           │ │
│  │  - TraceId 传播                                            │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                    │
│                              ▼                                    │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  HttpClientHandler (主要消息处理器)                          │ │
│  │  - SSL 证书验证控制                                        │ │
│  │  - 实际网络通信                                            │ │
│  └────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                        网络层 (Network)                          │
│                      HTTP/HTTPS 请求                            │
└─────────────────────────────────────────────────────────────────┘
```

### 2.2 核心组件

#### 2.2.1 IHttpHelper 接口

定义了所有 HTTP 操作的契约，支持：
- GET/POST/PUT/PATCH/DELETE 等标准方法
- 返回字符串或泛型类型
- 支持 Bearer Token 认证
- 支持自定义请求头
- 支持取消令牌 (CancellationToken)

**文件**: [IHttpHelper.cs](IHttpHelper.cs)

#### 2.2.2 HttpClientHelper 实现

`IHttpHelper` 的具体实现类，负责：
- 从 `IHttpClientFactory` 获取命名的 HttpClient 实例
- 参数验证和请求头处理
- 响应结果的转换和异常处理
- JSON 序列化/反序列化（通过 `JsonHelper`）

**文件**: [HttpClientHelper.cs](HttpClientHelper.cs)

#### 2.2.3 HttpClientOptions 配置

提供灵活的配置选项：

| 配置项 | 类型 | 默认值 | 说明 |
|--------|------|--------|------|
| `AuditLog` | bool | true | 是否启用审计日志 |
| `FailThrowException` | bool | false | 失败时是否抛出异常 |
| `Timeout` | int | 100 | 超时时间（秒） |
| `MaxOutputResponseLength` | int | 0 | 日志最大输出长度（0=不限制） |
| `IgnoreUntrustedCertificate` | bool | false | 是否忽略不安全SSL证书 |
| `RetryOnUnauthorized` | bool | false | 401错误时是否重试 |

**文件**: [HttpClientOptions.cs](HttpClientOptions.cs)

#### 2.2.4 ServiceCollectionExtensions 扩展

提供服务的注册和配置：

```csharp
services.AddHttpClientService(options => {
    options.AuditLog = true;
    options.Timeout = 30;
    // ...
});
```

**文件**: [ServiceCollectionExtensions.cs](ServiceCollectionExtensions.cs)

## 3. 处理器管道详解

### 3.1 处理器执行顺序

处理器按照从外层到内层的顺序执行（类似洋葱模型）：

```
请求流向: Application → Resilience → Logging → HttpClientHandler → Network
响应流向: Network → HttpClientHandler → Logging → Resilience → Application
```

### 3.2 HttpClientHandler (最底层)

作为主要消息处理器，负责实际的网络通信。

**关键配置**:
- `ServerCertificateCustomValidationCallback`: 控制 SSL 证书验证
- 当 `IgnoreUntrustedCertificate = true` 时，接受所有证书（仅用于开发/测试）

**代码位置**: [ServiceCollectionExtensions.cs:49-61](ServiceCollectionExtensions.cs#L49-L61)

```csharp
.ConfigurePrimaryHttpMessageHandler(serviceProvider =>
{
    var config = serviceProvider.GetService<IOptions<HttpClientOptions>>().Value;
    var handler = new HttpClientHandler();

    if (config.IgnoreUntrustedCertificate)
    {
        handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
    }

    return handler;
})
```

### 3.3 LoggingHandler (日志处理器)

提供完整的请求/响应审计日志功能。

**核心职责**:

1. **TraceId 传播**:
   - 优先级：请求头 X-Trace-Id → HttpContext → 自动生成 GUID
   - 确保分布式追踪的连续性

2. **请求前日志**:
   ```
   Http请求开始.TraceId：{traceId} Url：{url} Method：{method}
   ```

3. **响应后审计日志**:
   ```
   Http请求审计日志.TraceId：{traceId}
   Url：{url} Method：{method} StatusCode：{status} 耗时：{ms}
   RequestHeader：{headers}
   RequestContent：{body}
   ResponseHeader：{headers}
   ResponseContent：{body}
   ```

4. **日志控制**:
   - 通过 `X-Skip-Logger` 或 `X-Logger: none/skip` 请求头跳过日志
   - 支持响应内容长度限制（`MaxOutputResponseLength`）
   - 自动过滤二进制内容（图片、视频、文件等）

**文件**: [LoggingHandler.cs](LoggingHandler.cs)

**生命周期**: `Transient` - 每次请求创建新实例，避免状态污染

### 3.4 ResilienceHandler (弹性策略处理器)

基于 Polly 框架的弹性策略链，按以下顺序执行：

#### 策略执行顺序（从外到内）

```
┌─────────────────────────────────────────────────────────────┐
│                    请求进入                                  │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│  1. Fallback (降级策略) - 最外层                            │
│     如果所有策略失败，根据配置决定返回空响应或抛出异常        │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│  2. ConcurrencyLimiter (并发限制)                           │
│     限制最大并发请求数为 100，防止资源耗尽                    │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│  3. Retry (重试策略)                                         │
│     - 最多重试 3 次                                          │
│     - 初始延迟 1 秒，指数退避 (1s → 2s → 4s)                 │
│     - 重试条件：                                             │
│       * HTTP 5xx 服务器错误                                  │
│       * HTTP 408 请求超时                                    │
│       * HTTP 401 未授权（可选）                              │
│       * 超时异常                                             │
│       * HTTP 请求异常                                        │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│  4. CircuitBreaker (熔断器)                                 │
│     当错误率达到阈值时暂时停止请求，保护下游系统              │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│  5. Timeout (超时策略) - 最内层                             │
│     单次请求超时控制，每次重试都会重新应用                    │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                    实际 HTTP 请求                            │
└─────────────────────────────────────────────────────────────┘
```

**代码位置**: [ServiceCollectionExtensions.cs:73-144](ServiceCollectionExtensions.cs#L73-L144)

#### 重试机制详解

**为什么超时策略放在最内层？**

超时策略放在最内层确保每次重试都会应用超时限制。例如：

```
配置: Timeout = 30 秒, 最大重试 3 次

第 1 次: 请求 30 秒超时 → 触发重试 → 延迟 1 秒
第 2 次: 请求 30 秒超时 → 触发重试 → 延迟 2 秒
第 3 次: 请求 30 秒超时 → 失败 → Fallback

总耗时: 30s + 1s + 30s + 2s + 30s = 93 秒
```

**代码位置**: [ServiceCollectionExtensions.cs:109-136](ServiceCollectionExtensions.cs#L109-L136)

## 4. 请求处理流程

### 4.1 完整请求流程图

```
用户调用
   │
   ▼
HttpClientHelper.GetAsync<T>(url, token, headers)
   │
   ├─► VerifyParam() - 参数验证和请求头设置
   │   ├─ 清空默认请求头
   │   ├─ 设置 Authorization (如果有 token)
   │   └─ 添加自定义请求头
   │
   ▼
_client.GetAsync(url)
   │
   ▼
ResilienceHandler (Polly 策略链)
   │
   ├─► ConcurrencyLimiter - 检查并发限制
   │
   ├─► Retry - 准备重试上下文
   │
   ├─► CircuitBreaker - 检查熔断器状态
   │
   └─► Timeout - 设置超时
       │
       ▼
   LoggingHandler
   │
   ├─► AddOrGetTraceId() - 获取/生成 TraceId
   │   ├─ 从请求头获取
   │   ├─ 从 HttpContext 获取
   │   └─ 生成新 GUID
   │
   ├─► LogRequestStart() - 记录请求开始
   │
   ▼
HttpClientHandler
   │
   ├─► SSL 证书验证 (根据配置)
   │
   ▼
实际网络请求
   │
   ├─► DNS 解析
   ├─► TCP 连接
   ├─► TLS 握手
   └─► HTTP 请求/响应
   │
   ▼
响应返回
   │
   ▼
LoggingHandler
   │
   ├─► LogAuditAsync() - 记录审计日志
   │   ├─ 读取请求头/内容
   │   ├─ 读取响应头/内容
   │   ├─ 截断过长内容
   │   └─ 计算耗时
   │
   ▼
ResilienceHandler
   │
   ├─► 检查超时 → 触发重试或失败
   ├─► 检查错误率 → 更新熔断器状态
   ├─► 检查响应状态 → 决定是否重试
   │
   ▼
HttpClientHelper
   │
   ├─► ConvertResponseResult<T>()
   │   ├─ 检查 FailThrowException 配置
   │   ├─ 处理错误响应
   │   └─ 反序列化响应内容
   │
   ▼
返回结果给用户
```

### 4.2 参数处理机制

**VerifyParam 方法**: [HttpClientHelper.cs:314-337](HttpClientHelper.cs#L314-L337)

```csharp
private void VerifyParam(string url, string bearerToken, IDictionary<string, string> headers)
{
    // 1. 清空默认请求头（避免上次请求的 headers 污染本次请求）
    _client.DefaultRequestHeaders.Clear();

    // 2. 验证 URL
    if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof(url));

    // 3. 设置 Bearer Token（自动添加 "Bearer " 前缀）
    if (!string.IsNullOrWhiteSpace(bearerToken))
    {
        var bearerTokenStr = bearerToken.StartsWith("Bearer ")
            ? bearerToken
            : "Bearer " + bearerToken;
        _client.DefaultRequestHeaders.Add("Authorization", bearerTokenStr);
    }

    // 4. 添加自定义请求头
    if (headers?.Count > 0)
    {
        foreach (var (key, value) in headers)
        {
            _client.DefaultRequestHeaders.Add(key, value);
        }
    }
}
```

### 4.3 响应处理机制

**ConvertResponseResult 方法**: [HttpClientHelper.cs:257-283](HttpClientHelper.cs#L257-L283)

```csharp
private async Task<T> ConvertResponseResult<T>(HttpResponseMessage response, string url)
{
    // 1. 根据 FailThrowException 配置处理错误
    if (_httpConfig.FailThrowException)
    {
        response.EnsureSuccessStatusCode(); // 抛出异常
    }
    else if (!response.IsSuccessStatusCode)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        _logger.LogError($"API:{url} error: {(int)response.StatusCode} - {errorContent}");
        return default; // 返回 default(T)
    }

    // 2. 读取响应内容
    var resStr = await response.Content.ReadAsStringAsync();

    // 3. 处理空响应（来自 Fallback）
    if (string.IsNullOrEmpty(resStr))
        return default;

    // 4. 反序列化
    if (typeof(T) == typeof(string))
        return (T)Convert.ChangeType(resStr, typeof(string));

    return JsonHelper.ToObject<T>(resStr);
}
```

## 5. 分布式追踪机制

### 5.1 TraceId 传播流程

```
┌──────────────────┐
│  原始请求         │
│  X-Trace-Id: A1  │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  ASP.NET Core    │
│  HttpContext     │
│  TraceId: A1     │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  业务代码         │
│  IHttpHelper     │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  LoggingHandler  │
│  1. 尝试从请求头获取 → 失败                    │
│  2. 从 HttpContext 获取 → 成功: A1             │
│  3. 添加到 HTTP 请求头                         │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  外部 API 请求    │
│  X-Trace-Id: A1  │
└──────────────────┘
```

### 5.2 TraceId 获取优先级

**代码位置**: [LoggingHandler.cs:54-89](LoggingHandler.cs#L54-L89)

```csharp
private string AddOrGetTraceId(HttpRequestMessage request)
{
    string traceId = null;

    // 1. 从 HTTP 请求头获取
    if (request.Headers.TryGetValues("X-Trace-Id", out var traceIds))
    {
        traceId = traceIds.FirstOrDefault();
    }

    // 2. 从 HttpContext 获取
    if (string.IsNullOrEmpty(traceId) && _httpContextAccessor?.HttpContext != null)
    {
        if (httpContext.Request.Headers.TryGetValue("X-Trace-Id", out var contextTraceId))
        {
            traceId = contextTraceId.FirstOrDefault();
        }
    }

    // 3. 生成新的 GUID
    if (string.IsNullOrEmpty(traceId))
    {
        traceId = Guid.NewGuid().ToString("N");
    }

    // 4. 添加到请求头
    if (!request.Headers.Contains("X-Trace-Id"))
    {
        request.Headers.Add("X-Trace-Id", traceId);
    }

    return traceId;
}
```

## 6. 工具类

### 6.1 JsonHelper

基于 Newtonsoft.Json 的 JSON 序列化工具。

**特性**:
- 驼峰命名转换（`CamelCasePropertyNamesContractResolver`）
- ISO 8601 日期格式（`yyyy-MM-dd HH:mm:ss`）

**文件**: [Utils/JsonHelper.cs](Utils/JsonHelper.cs)

### 6.2 HttpRequestEnum

HTTP 请求方法枚举，用于灵活的 `SendAsync` 调用。

**文件**: [HttpRequestEnum.cs](HttpRequestEnum.cs)

## 7. 依赖关系

### 7.1 NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `Microsoft.Extensions.Http` | HttpClient 工厂和依赖注入支持 |
| `Microsoft.Extensions.Http.Resilience` | Polly 弹性策略集成 |
| `Newtonsoft.Json` | JSON 序列化/反序列化 |
| `Microsoft.AspNetCore.Http` | HttpContext 访问器 |

### 7.2 框架支持

- .NET 6.0
- .NET 7.0
- .NET 8.0
- .NET 9.0
- .NET 10.0

## 8. 使用最佳实践

### 8.1 服务注册

```csharp
// Program.cs 或 Startup.cs
services.AddHttpClientService(options =>
{
    // 生产环境建议配置
    options.AuditLog = true;
    options.FailThrowException = false;  // 由业务代码决定异常处理
    options.Timeout = 30;                // 根据业务需求设置
    options.MaxOutputResponseLength = 1024 * 1024;  // 1MB
    options.IgnoreUntrustedCertificate = false;     // 生产环境必须为 false
    options.RetryOnUnauthorized = true;    // 如果使用 token 刷新机制
});
```

### 8.2 使用建议

1. **依赖注入**: 始终通过构造函数注入 `IHttpHelper`
2. **异常处理**: 根据业务场景选择 `FailThrowException` 模式
3. **日志控制**: 对于高频率的轮询请求，使用 `X-Skip-Logger` 跳过日志
4. **超时设置**: 根据接口特性合理设置超时时间
5. **取消令牌**: 对于长时间运行的请求，传递 `CancellationToken`

### 8.3 常见场景配置

```csharp
// 场景 1: 调用外部 API，需要重试和熔断
options.Timeout = 10;
options.RetryOnUnauthorized = true;

// 场景 2: 上传大文件
options.Timeout = 300;
options.MaxOutputResponseLength = 0;  // 不限制

// 场景 3: 开发环境忽略 SSL 证书
options.IgnoreUntrustedCertificate = true;  // 仅开发环境！

// 场景 4: 高频率心跳检测
// 使用时跳过日志：
await _httpHelper.GetAsync(url, headers: new Dictionary<string, string>
{
    { "X-Skip-Logger", "" }
});
```

## 9. 扩展性

### 9.1 自定义处理器

可以通过实现 `DelegatingHandler` 来添加自定义功能：

```csharp
public class CustomHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 请求前处理
        // ...

        var response = await base.SendAsync(request, cancellationToken);

        // 响应后处理
        // ...

        return response;
    }
}

// 注册
services.AddTransient<CustomHandler>();
// 在 AddHttpClientService 链中添加
.AddHttpMessageHandler<CustomHandler>();
```

### 9.2 自定义弹性策略

修改 `ServiceCollectionExtensions.cs` 中的策略配置：

```csharp
.AddRetry(new HttpRetryStrategyOptions
{
    MaxRetryAttempts = 5,  // 自定义重试次数
    Delay = TimeSpan.FromSeconds(2),
    // ...
})
```

## 10. 故障排查

### 10.1 常见问题

| 问题 | 原因 | 解决方案 |
|------|------|----------|
| 请求超时 | `Timeout` 配置过小 | 增加超时时间或优化接口 |
| 重试不生效 | 错误不在重试条件中 | 检查 HTTP 状态码或异常类型 |
| 日志过多 | `AuditLog = true` 且请求频繁 | 使用 `X-Skip-Logger` 请求头 |
| SSL 错误 | 自签名证书 | 仅在开发环境设置 `IgnoreUntrustedCertificate = true` |
| 返回 null | `FailThrowException = false` 且请求失败 | 检查日志或设置 `FailThrowException = true` |

### 10.2 调试建议

1. 启用完整日志记录
2. 检查 TraceId 链路追踪
3. 监控重试和熔断状态
4. 分析请求耗时分布

---

**文档版本**: 1.0
**最后更新**: 2025-02-17
**维护者**: Azrng
