# Azrng.AspNetCore.Core 架构设计文档

## 一、项目概述

`Azrng.AspNetCore.Core` 是一个 ASP.NET Core 应用开发的通用辅助库，提供了一系列开箱即用的功能，包括统一返回结果封装、模型验证、全局异常处理、审计日志、跨域支持等。该项目从 `Common.Mvc` 迁移而来，经过多次迭代优化，旨在简化 ASP.NET Core API 开发流程。

### 1.1 设计目标

- 提供统一的 API 返回结果格式
- 简化模型验证和错误处理
- 自动记录请求/响应审计日志
- 便捷的依赖注入批量注册
- 提供常用的中间件和扩展方法

### 1.2 技术栈

- **目标框架**: net6.0; net7.0; net8.0; net9.0; net10.0
- **AOT 兼容**: 支持 Native AOT 编译
- **核心依赖**:
  - `Microsoft.AspNetCore.App` (ASP.NET Core 框架引用)
  - `Azrng.Core` 1.8.4 (核心工具库，提供异常、结果封装等)

---

## 二、项目结构

```
Azrng.AspNetCore.Core/
├── Attributes/                           # 自定义验证特性
│   ├── CollectionNotEmptyAttribute.cs   # 集合非空验证
│   └── MinValueAttribute.cs             # 最小值验证
│
├── AuditLog/                             # 审计日志模块
│   ├── ILoggerService.cs                # 日志服务接口
│   ├── DefaultLoggerService.cs          # 默认日志服务实现
│   ├── LogInfo.cs                       # 日志信息模型
│   └── AuditLogOptions.cs               # 审计日志配置选项
│
├── Extension/                            # 扩展方法
│   ├── HttpContextExtensions.cs         # HttpContext 扩展
│   ├── IApplicationBuilderExtensions.cs # IApplicationBuilder 扩展
│   ├── PreConfigureExtensions.cs        # 预配置扩展
│   └── CustomContractResolver.cs        # JSON 序列化契约解析器
│
├── Filter/                               # MVC 过滤器
│   ├── ModelVerifyFilter.cs             # 模型验证过滤器
│   └── CustomResultPackFilter.cs        # 返回结果包装过滤器
│
├── Helper/                               # 帮助类
│   ├── AppSettings.cs                   # 配置帮助类
│   ├── HttpContextManager.cs            # HttpContext 管理器
│   ├── ServiceProviderHelper.cs         # 服务提供者帮助类
│   └── SessionHelper.cs                 # Session 帮助类
│
├── Middleware/                           # 中间件
│   ├── AuditLogMiddleware.cs            # 审计日志中间件
│   ├── CustomExceptionMiddleware.cs     # 全局异常处理中间件
│   ├── CustomExceptionMiddlewareExtensions.cs # 异常中间件扩展
│   ├── MiddlewareExtensions.cs          # 中间件扩展方法
│   ├── RequestIdMiddleware.cs           # 请求 ID 中间件
│   └── ShowAllServicesMiddleware.cs     # 服务显示中间件
│
├── Model/                                # 数据模型
│   └── ShowServiceConfig.cs             # 服务显示配置
│
├── PreConfigure/                         # 预配置功能
│   ├── IObjectAccessor.cs               # 对象访问器接口
│   └── PreConfigureActionList.cs        # 预配置动作列表
│
├── JsonConverters/                       # JSON 转换器
│   └── LongToStringConverter.cs         # Long 转 String 转换器
│
├── CommonMvcConfig.cs                    # MVC 通用配置
└── Azrng.AspNetCore.Core.csproj
```

---

## 三、核心组件架构

### 3.1 整体架构图

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         ASP.NET Core 应用程序                            │
│                    (Controllers / Minimal APIs)                          │
└────────────────────────────────────┬────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                          MVC 过滤器层                                    │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  CustomResultPackFilter (返回结果包装过滤器)                     │   │
│  │  - 包装非 ResultModel 类型的返回值                               │   │
│  │  - 支持 NoWrapperAttribute 排除特定 Action                       │   │
│  │  - 可配置忽略路由前缀                                            │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  ModelVerifyFilter (模型验证过滤器)                             │   │
│  │  - 拦截模型验证错误                                              │   │
│  │  - 返回统一的 ResultModel 错误格式                               │   │
│  │  - 支持自定义验证特性 (MinValue, CollectionNotEmpty)             │   │
│  └─────────────────────────────────────────────────────────────────┘   │
└────────────────────────────────────┬────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                          中间件层                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  CustomExceptionMiddleware (全局异常处理)                       │   │
│  │  - 捕获所有未处理异常                                            │   │
│  │  - 根据异常类型返回不同状态码                                    │   │
│  │  - 统一错误响应格式                                              │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  AuditLogMiddleware (审计日志中间件)                            │   │
│  │  - 记录请求/响应详情                                              │   │
│  │  - 可配置记录的 HTTP 方法                                        │   │
│  │  - 可配置忽略路由                                                │   │
│  │  - 可限制响应体大小                                              │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│  ┌────────────────────┬───────────────────────┬──────────────────────┐  │
│  │ RequestIdMiddleware│ ShowAllServices      │   AnyCors           │  │
│  │ (请求 ID 传递)     │ (服务列表显示)        │   (跨域处理)        │  │
│  └────────────────────┴───────────────────────┴──────────────────────┘  │
└────────────────────────────────────┬────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                          扩展和工具层                                     │
│  ┌────────────────────┬───────────────────────┬──────────────────────┐  │
│  │ServiceCollection   │ HttpContextExtensions│  PreConfigure       │  │
│  │Extensions          │ (IP获取、URL解析等)    │  (预配置系统)        │  │
│  │(批量服务注册)       │                       │                      │  │
│  └────────────────────┴───────────────────────┴──────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  自定义验证特性                                                    │   │
│  │  - MinValueAttribute: 最小值验证                                  │   │
│  │  - CollectionNotEmptyAttribute: 集合非空验证                      │   │
│  └──────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
```

### 3.2 核心组件说明

#### 3.2.1 CustomResultPackFilter (返回结果包装过滤器)

**功能**: 自动包装 API 返回值，统一返回格式。

**包装规则**:
1. 检查路由前缀是否在忽略列表中
2. 检查 Action 是否标记 `[NoWrapper]`
3. 对于不同返回类型的处理：
   - `EmptyResult` → 包装为成功响应
   - `FileResult` → 不包装
   - `ResultModel` 类型 → 不重复包装
   - `ProblemDetails` → 不包装
   - 其他 `ObjectResult` → 包装为 `ResultModel<T>`
   - `ContentResult`、`StatusCodeResult` → 不包装

**执行时机**: 在 Action 执行后、响应发送前 (`OnResultExecuting`)

#### 3.2.2 ModelVerifyFilter (模型验证过滤器)

**功能**: 拦截模型验证错误，返回统一错误格式。

**工作流程**:
```
请求到达 → 模型绑定 → ModelState.IsValid?
                              ├─ Yes → 继续
                              └─ No → 收集错误 → 返回 ResultModel 错误
```

**错误信息结构**:
```json
{
  "isSuccess": false,
  "code": "400",
  "message": "参数格式不正确",
  "errors": [
    { "field": "UserName", "message": "用户名不能为空" },
    { "field": "Password", "message": "密码长度不能少于6位" }
  ]
}
```

#### 3.2.3 CustomExceptionMiddleware (全局异常处理中间件)

**功能**: 捕获应用中的未处理异常，返回统一错误响应。

**异常类型映射**:

| 异常类型 | HTTP 状态码 | 说明 |
|---------|------------|------|
| `ForbiddenException` | 401 | 身份验证失败 |
| `NotFoundException` | 404 | 资源未找到 |
| `ParameterException` | 400 | 参数错误 |
| `LogicBusinessException` | 400 | 业务逻辑错误 |
| `InternalServerException` | 500 | 服务器内部错误 |
| `BaseException` | 500 | 自定义基础异常 |
| `Exception` | 500 | 未知异常 |

**工作流程**:
```
请求 → try { 执行后续管道 } catch (Exception)
                                      │
                                      ▼
                              根据异常类型映射状态码
                                      │
                                      ▼
                              记录日志 (包含 TraceId)
                                      │
                                      ▼
                              返回统一错误响应
```

#### 3.2.4 AuditLogMiddleware (审计日志中间件)

**功能**: 记录请求/响应的详细信息，用于审计和调试。

**记录内容**:
- 路由、HTTP 方法
- 请求/响应体
- 请求/响应头
- 执行时间
- 用户信息 (Claims)
- IP 地址、User Agent
- 响应状态码

**配置选项** (`AuditLogOptions`):

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `LogOnlyApiRoutes` | `bool` | `false` | 是否只记录 `/api/` 开头的路由 |
| `IncludeHttpMethods` | `List<string>` | `["POST", "PUT", "DELETE"]` | 需要记录的 HTTP 方法 |
| `IgnoreRoutePrefix` | `List<string>` | `[]` | 忽略的路由前缀 |
| `MaxResponseBodySize` | `int` | `10240` | 最大响应体大小 (字节) |
| `FormatJson` | `bool` | `false` | 是否格式化 JSON 输出 |

**工作流程**:
```
请求到达 → 检查是否应该记录 → 读取请求信息
                                    │
                                    ▼
                            替换 Response.Body 为 MemoryStream
                                    │
                                    ▼
                            执行后续管道
                                    │
                                    ▼
                            读取响应信息
                                    │
                                    ▼
                            Response.OnCompleted 回调:
                              - 停止计时
                              - 组装 AuditLogInfo
                              - 调用 ILoggerService.Write()
```

**自定义日志存储**:

```csharp
public class CustomLoggerService : ILoggerService
{
    public void Write(AuditLogInfo log)
    {
        // 写入数据库、文件、消息队列等
    }
}

// 注册
services.AddSingleton<ILoggerService, CustomLoggerService>();
```

---

## 四、依赖注入批量注册

### 4.1 设计思想

通过标记接口约定服务的生命周期，实现批量自动注册：

| 标记接口 | 生命周期 | 说明 |
|---------|---------|------|
| `ITransientDependency` | Transient | 每次请求创建新实例 |
| `IScopedDependency` | Scoped | 同一 HTTP 请求内共享实例 |
| `ISingletonDependency` | Singleton | 应用程序生命周期内共享实例 |

### 4.2 注册方式

#### 方式一: 按程序集名称匹配

```csharp
services.RegisterBusinessServices("MyApp.Services.dll");
services.RegisterBusinessServices("MyApp.*.dll"); // 支持通配符

// 忽略特定命名空间
services.RegisterBusinessServices("MyApp.dll", "MyApp.Internal,MyApp.Tests");
```

#### 方式二: 按程序集数组

```csharp
var assemblies = new[]
{
    typeof(UserService).Assembly,
    typeof(OrderService).Assembly
};

services.RegisterBusinessServices(assemblies);
```

### 4.3 注册规则

1. 类必须实现对应的标记接口 (`ITransientDependency` / `IScopedDependency` / `ISingletonDependency`)
2. 自动忽略 `Microsoft.*` 和 `System.*` 命名空间下的接口
3. 如果类实现了其他接口，将注册所有非忽略的接口
4. 如果类没有实现其他接口，将注册类本身

**示例**:

```csharp
public class UserService : IScopedDependency, IUserService, IEmailSender
{
    // 将被注册为:
    // - services.AddScoped<IUserService, UserService>()
    // - services.AddScoped<IEmailSender, UserService>()
}
```

---

## 五、预配置系统 (PreConfigure)

### 5.1 设计目的

在 Options 正式配置之前执行预配置动作，适用于：

- 设置默认配置值
- 多个模块需要配置同一选项时，按优先级组合配置
- 在应用启动前修改配置

### 5.2 执行顺序

```
1. PreConfigure 动作 (按添加顺序执行)
2. Configure 动作 (正式配置)
3. IOptions<T> 注入到容器
```

### 5.3 使用示例

```csharp
// 模块 A 设置默认值
services.PreConfigure<DatabaseOptions>(options =>
{
    options.CommandTimeout = 30;
});

// 模块 B 设置连接字符串
services.PreConfigure<DatabaseOptions>(options =>
{
    options.ConnectionString = "Server=localhost;";
});

// 最终配置 (会覆盖 PreConfigure 的值)
services.Configure<DatabaseOptions>(configuration.GetSection("Database"));

// 或手动执行所有预配置
var preConfigActions = services.GetPreConfigureActions<DatabaseOptions>();
var configuredOptions = preConfigActions.Configure();
```

### 5.4 对象访问器 (ObjectAccessor)

将已创建的对象实例直接添加到服务容器：

```csharp
// 注册对象
services.AddObjectAccessor(new AppOptions
{
    AppName = "MyApp",
    Version = "1.0.0"
});

// 获取对象
var options = services.GetObjectOrNull<AppOptions>();
Console.WriteLine($"App: {options?.AppName}");
```

**特点**:
- 直接添加对象实例，不依赖依赖注入容器创建
- 只能通过 `GetObjectOrNull<T>` 方法获取
- 同一类型只能注册一次，重复注册会抛出异常

---

## 六、自定义验证特性

### 6.1 MinValueAttribute

验证数值类型的最小值：

```csharp
public class RequestDto
{
    [MinValue(1)]
    public int Id { get; set; }
}
```

### 6.2 CollectionNotEmptyAttribute

验证集合不能为空：

```csharp
public class RequestDto
{
    [CollectionNotEmpty]
    public List<string> Items { get; set; }
}
```

---

## 七、HttpContext 扩展

### 7.1 IP 地址获取

```csharp
// 获取本地 IPv4
var ipv4 = HttpContext.GetLocalIpAddressToIPv4();

// 获取本地 IPv6
var ipv6 = HttpContext.GetLocalIpAddressToIPv6();

// 获取远程 IP
var remoteIp = HttpContext.GetRemoteIpAddress();

// 获取完整 URL
var url = HttpContext.Request.GetRequestUrlAddress();
```

### 7.2 请求体重复读取

```csharp
// 启用请求体重复读取
app.UseRequestBodyRepetitionRead();

// 之后可以多次读取请求体
var body1 = await HttpContext.Request.ReadBodyAsync();
var body2 = await HttpContext.Request.ReadBodyAsync();
```

---

## 八、配置系统

### 8.1 CommonMvcConfig

MVC 通用配置类，控制核心行为：

| 属性 | 默认值 | 说明 |
|------|--------|------|
| `EnabledCustomerResultPack` | `true` | 是否启用返回结果包装 |
| `EnabledModelVerify` | `true` | 是否启用模型验证 |
| `UseHttpStateCode` | `true` | 是否使用 HTTP 状态码 |

**配置方式**:

```csharp
services.AddDefaultControllers(options =>
{
    options.EnabledCustomerResultPack = true;
    options.EnabledModelVerify = true;
    options.UseHttpStateCode = false; // 所有响应返回 200，错误信息在 body 中
});
```

### 8.2 AuditLogOptions

审计日志配置选项（详见 3.2.4 节表格）

**配置方式**:

```csharp
// 方式一: 使用委托配置
app.UseAutoAuditLog(options =>
{
    options.LogOnlyApiRoutes = true;
    options.IncludeHttpMethods = new[] { "POST", "PUT", "DELETE" };
    options.MaxResponseBodySize = 20480;
    options.FormatJson = true;
});

// 方式二: 配置后注册
services.Configure<AuditLogOptions>(configuration.GetSection("AuditLog"));
app.UseAutoAuditLog();
```

---

## 九、中间件执行顺序

### 9.1 推荐顺序

```csharp
var app = builder.Build();

// 1. 异常处理 (最外层)
app.UseGlobalException();

// 2. 请求 ID 传递
app.UseRequestIdMiddleware();

// 3. 跨域
app.UseAnyCors();

// 4. 审计日志
app.UseAutoAuditLog();

// 5. 请求体重复读取 (如需)
app.UseRequestBodyRepetitionRead();

// 6. 路由
app.UseRouting();

// 7. 静态文件
app.UseStaticFiles();

// 8. 认证授权
app.UseAuthentication();
app.UseAuthorization();

// 9. 服务显示 (开发环境)
if (app.Environment.IsDevelopment())
{
    app.UseShowAllServicesMiddleware();
}

// 10. 映射控制器
app.MapControllers();

app.Run();
```

### 9.2 执行顺序说明

```
请求 → GlobalException → RequestId → CORS → AuditLog → Routing →
                                                         │
                                                         ▼
                                           Controller (过滤器执行)
                                                         │
                                                         ▼
                                              Authentication → Authorization
                                                         │
                                                         ▼
                                                   Action 执行
                                                         │
                                                         ▼
                                        AuditLog 记录响应 → 返回响应
```

---

## 十、版本演进

| 版本 | 主要变更 |
|------|----------|
| 1.2.1 | 更新异常中间件 |
| 1.2.0 | 支持 .NET 10；新增 `AuditLogOptions`；支持配置记录的 HTTP 方法；支持最大响应体大小限制；优化 `CustomResultPackFilter` 不包装 `ProblemDetails` |
| 1.1.0 | 移除 `Microsoft.AspNetCore.Mvc.NewtonsoftJson` 依赖；使用 `System.Text.Json` 替换 |
| 1.0.0 | 更新正式版本 |
| 0.1.2 | 优化 `ResultModel` 相关依赖 |
| 0.1.1 | 修复 `CustomResultPackFilter` 报错问题；增加审计日志中间件 |
| 0.1.0-beta8 | 修复 `MinValue` 特性 bug |
| 0.1.0-beta7 | 增加 `RequestBodyAsync` 扩展和请求体重复读取中间件 |
| 0.1.0-beta6 | 升级支持 .NET 8 |
| 0.1.0-beta5 | 修复批量注入问题 |
| 0.1.0-beta4 | 增加 `HttpContext` 扩展、`CollectionNotEmpty`/`MinValue` 特性、批量服务注册 |
| 0.1.0-beta3 | 支持 .NET 8；支持显示所有服务 |
| 0.1.0-beta2 | 优化代码 |
| 0.1.0-beta1 | 升级支持 .NET 7 |

---

## 十一、使用场景示例

### 11.1 完整配置示例

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// 配置服务
builder.Services.AddDefaultControllers(options =>
{
    options.EnabledCustomerResultPack = true;
    options.EnabledModelVerify = true;
});

builder.Services.AddAnyCors();
builder.Services.AddShowAllServices("/allservices");
builder.Services.AddMvcResultPackFilter("/swagger", "/health");

// 批量注册业务服务
builder.Services.RegisterBusinessServices("MyApp.Services.dll");

// 配置审计日志
builder.Services.Configure<AuditLogOptions>(options =>
{
    options.LogOnlyApiRoutes = true;
    options.IncludeHttpMethods = new[] { "POST", "PUT", "DELETE" };
    options.FormatJson = true;
});

var app = builder.Build();

// 配置中间件管道
app.UseGlobalException();
app.UseRequestIdMiddleware();
app.UseAnyCors();
app.UseAutoAuditLog();
app.UseRouting();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseShowAllServicesMiddleware();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
```

### 11.2 Controller 示例

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IResultModel<UserDto>> GetAsync(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        return ResultModel<UserDto>.Success(user);
    }

    [HttpPost]
    public async Task<IResultModel<long>> CreateAsync([FromBody] CreateUserDto dto)
    {
        var userId = await _userService.CreateAsync(dto);
        return ResultModel<long>.Success(userId);
    }

    [HttpGet("no-wrapper")]
    [NoWrapper] // 不包装返回值
    public IActionResult GetRaw()
    {
        return Ok("raw response");
    }
}
```

---

## 十二、注意事项

### 12.1 安全建议

1. **CORS 配置**: `AddAnyCors()` 仅适用于开发环境，生产环境应配置具体的允许来源
2. **审计日志**: 避免记录敏感信息（密码、Token 等），可在 `ILoggerService` 实现中脱敏
3. **异常信息**: 生产环境应避免返回详细异常堆栈，防止信息泄露

### 12.2 性能考虑

1. **审计日志**: 记录大量日志可能影响性能，建议：
   - 仅记录必要的 HTTP 方法 (默认不记录 GET)
   - 设置合理的 `MaxResponseBodySize`
   - 考虑使用异步日志存储 (`ILoggerService.WriteAsync`)
2. **返回结果包装**: 对于文件下载等大响应场景，使用 `[NoWrapper]` 或忽略路由前缀

### 12.3 兼容性

1. **System.Text.Json**: 从 1.1.0 版本开始，使用 `System.Text.Json` 替代 `Newtonsoft.Json`
2. **AOT 兼容**: 项目支持 Native AOT 编译，但需注意：
   - 避免使用反射
   - 使用 `JsonSerializerContext` 进行序列化

---

## 十三、参考资料

- [ASP.NET Core 过滤器](https://learn.microsoft.com/zh-cn/aspnet/core/mvc/controllers/filters)
- [ASP.NET Core 中间件](https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/middleware/)
- [ASP.NET Core 依赖注入](https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/dependency-injection)
- [System.Text.Json 迁移指南](https://learn.microsoft.com/zh-cn/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to)
