# Azrng.AspNetCore.Core 代码审查报告

- **审查日期**: 2026-05-26
- **库版本**: 1.3.1
- **审查范围**: 全部源码（27 个文件）

---

## 整体评价

这是一个功能完备的 ASP.NET Core 基础设施库，提供全局异常处理、返回值包装、模型校验、审计日志、CORS、DI 批量注册等能力。设计思路清晰，API 易用。以下按类别列出发现的问题。

---

## 1. 安全问题

### 1.1 `ShowAllServicesMiddleware` 存在 XSS 风险

**文件**: `Middleware/ShowAllServicesMiddleware.cs:34`

直接将 `ServiceType.FullName` 拼接到 HTML 中，未做 HTML 编码：

```csharp
stringBuilder.Append("<td>" + service.ServiceType.FullName + "</td>");
```

如果服务类型名包含恶意字符，存在 XSS 注入风险。应使用 `System.Net.WebUtility.HtmlEncode()` 编码后再拼接。

### 1.2 `ShowAllServicesMiddleware` 无鉴权保护  暂不处理

该中间件暴露了整个应用的 DI 注册信息，但没有任何认证/授权检查。生产环境误开将泄露内部架构。建议增加环境检查或 token 验证。

### 1.3 `JavaScriptEncoder.UnsafeRelaxedJsonEscaping` 的使用  如果是看日志的，那么不能修改

**文件**: `Middleware/CustomExceptionMiddlewareExtensions.cs:121`

使用了 `UnsafeRelaxedJsonEscaping`，这会关闭 JSON 中特殊字符的转义。虽然异常响应中不太可能有用户注入的 XSS 内容，但仍需注意此配置不应泄漏到其他序列化场景。

---

## 2. 代码质量问题

### 2.1 `Obsolete` 特性标注错误

**文件**: `Middleware/MiddlewareExtensions.cs:40`

`UseShowAllServicesMiddleware` 的 `[Obsolete]` 消息写成了 `"改为使用UseGlobalException"`，应该是 `"改为使用UseShowAllServices"`：

```csharp
[Obsolete("改为使用UseGlobalException")]  // 错误：应该是 UseShowAllServices
public static IApplicationBuilder UseShowAllServicesMiddleware(...)
```

### 2.2 `AuditLogMiddleware` 使用共享的 `Stopwatch` 实例

**文件**: `Middleware/AuditLogMiddleware.cs:28`

中间件作为单例注入，但 `_stopwatch` 是实例字段。在并发请求下，多个请求共享同一个 `Stopwatch`，会导致计时数据混乱。应改为在 `Invoke` 方法内部创建局部 `Stopwatch`。

### 2.3 `AuditLogMiddleware` 的响应流替换存在泄漏风险

**文件**: `Middleware/AuditLogMiddleware.cs:68-69`

替换了 `Response.Body`，但如果中间件后续抛出异常（在 `responseBody.CopyToAsync` 之前），原始流不会被恢复。建议用 `try/finally` 确保恢复。

### 2.4 `CustomExceptionMiddleware` 使用字符串插值做日志

**文件**: `Middleware/CustomExceptionMiddlewareExtensions.cs:62`

使用了 `$"..."` 字符串插值写日志，而非结构化日志模板。这会导致每次调用都分配字符串，且不利于日志系统的结构化查询：

```csharp
// 当前写法
_logger.LogError($@"统一日志记录异常-{context.Request.GetUrl()} ...");

// 建议改为
_logger.LogError(exception, "统一日志记录异常 {Url} request had an exception, xRequestId: {TraceId}",
    context.Request.GetUrl(), xRequested);
```

### 2.5 `AppSettings` 静态配置类的 `GetValue` 吞掉异常

**文件**: `Helper/AppSettings.cs:33`

`catch (Exception) { // ignored }` 静默吞掉所有异常，配置读取失败时返回空字符串，难以排查问题。

### 2.6 `MyHttpContext` 等旧模式的 Helper 类仍在

**文件**: `Helper/HttpContextManager.cs`、`Helper/ServiceProviderHelper.cs`

使用静态 `IServiceProvider` 模式，这是 ASP.NET Core 中的反模式（Service Locator）。建议在文档中标记为过时，引导使用 DI 注入。

### 2.7 `CustomResultPackFilter` 同时继承 `Attribute` 和实现 `IResultFilter`

**文件**: `Filter/CustomResultPackFilter.cs:11`

该类既是 `Attribute` 又是 `IResultFilter`，但实际使用中是通过 `options.Filters.Add<CustomResultPackFilter>()` 或 `new CustomResultPackFilter()` 注册的，从未作为 Attribute 使用。继承 `Attribute` 是多余的。

---

## 3. 设计问题

### 3.1 两套模型校验机制并存

`ServiceCollectionExtensions` 中存在两套模型校验：

- `AddDefaultControllers` 内部通过 `ModelVerifyFilter`（Action Filter）
- `AddMvcModelVerifyFilter` 通过 `InvalidModelStateResponseFactory`

两者功能相同但实现路径不同，容易让使用者困惑。建议统一为一种实现，将另一种标记为过时。

将AddDefaultControllers标记为过时吧

### 3.2 `ResultModel` 已包装判断不完整

**文件**: `Filter/CustomResultPackFilter.cs:56`

判断 `objectResult.Value is ResultModel` 只检查了非泛型版本。如果返回的是 `ResultModel<T>`（继承自 `ResultModel`），子类匹配也能工作，但如果有其他类型恰好实现了 `IResultModel` 接口但不是 `ResultModel` 子类，则会被重复包装。建议改为检查 `IResultModel` 接口。

### 3.3 `Request.GetUrl()` 缺少对 `Host` 为空的防护

**文件**: `Extension/HttpContextExtensions.cs:78`

直接使用 `httpRequest.Host.Host`，在某些场景（如后台任务、健康检查请求）中 `Host` 可能为空，导致 URL 构建异常。

---

## 4. README 与文档问题

### 4.1 `GetRequestUrlAddress` 方法名在文档中不存在

README 中示例 `HttpContext.Request.GetRequestUrlAddress()` 但实际代码中方法名为 `GetUrl()`，文档与实现不一致。

### 4.2 `MyHttpContext` 未在代码中找到

README 中提到 `MyHttpContext.Current`，但在源码中未找到 `MyHttpContext` 类。如果已迁移走，README 应同步更新。

### 4.3 版本记录格式不一致

部分条目使用 `*`，部分使用 `-`，缩进层级混乱。

---

## 5. 兼容性与维护


### 5.2 `NoWarn` 抑制了有意义的警告

`IL2026`、`IL3050` 是 AOT 兼容性警告，但项目设置了 `IsAotCompatible=true`。同时抑制这些警告意味着实际 AOT 运行时可能出现问题。

### 5.3 `OptionsWrapper` 自定义实现

**文件**: `Middleware/MiddlewareExtensions.cs:89`

自定义了 `OptionsWrapper<TOptions>`，但 `Microsoft.Extensions.Options` 已有 `Options.Create<T>()` 可以达到同样效果，减少重复代码。

---

## 6. 改进建议优先级

| 优先级 | 问题 | 建议 |
|--------|------|------|
| **高** | Stopwatch 并发安全 | 改为方法内局部变量 |
| **高** | Obsolete 消息错误 | 修正为 `UseShowAllServices` |
| **高** | ShowAllServices XSS | HTML 编码输出内容 |
| **中** | 日志字符串插值 | 改用结构化日志 |
| **中** | 响应流替换的异常安全 | 添加 try/finally |
| **中** | 两套模型校验 | 统一并废弃其中一套 |
| **低** | 文档与实现不一致 | 同步更新 README |
| **低** | 静态 Helper 类 | 标记为过时 |
