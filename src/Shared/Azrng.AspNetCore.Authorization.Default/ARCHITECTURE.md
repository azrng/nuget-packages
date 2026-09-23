# Azrng.AspNetCore.Authorization.Default 项目架构与原理说明

## 目录

- [项目概述](#项目概述)
- [架构设计](#架构设计)
- [核心组件](#核心组件)
- [授权流程](#授权流程)
- [实现原理](#实现原理)
- [扩展点](#扩展点)
- [最佳实践](#最佳实践)
- [常见问题](#常见问题)
- [性能优化](#性能优化)
- [安全考虑](#安全考虑)

---

## 项目概述

`Azrng.AspNetCore.Authorization.Default` 是一个基于路径的 ASP.NET Core 授权库，提供了灵活的权限验证机制。它通过实现 ASP.NET Core 的授权接口，实现了基于请求路径的访问控制。

> **1.3.0 当前实现说明**：默认策略通过 `AuthorizationOptions` 配置，不再注册或覆盖宿主的 `IAuthorizationPolicyProvider`。认证由 ASP.NET Core 的策略和认证中间件负责；权限处理器只构造 `PermissionContext` 并调用 `IPermissionEvaluator`。`IPermissionVerifyService` 通过内部适配器继续兼容旧的路径权限实现。Endpoint 上的 `IPermissionMetadata` 优先于旧的匿名路径列表。

### 特点

- ✅ **基于路径的权限验证** - 根据请求的 URL 路径进行权限判断
- ✅ **自定义权限逻辑** - 通过接口实现自定义的权限验证服务
- ✅ **匿名路径支持** - 配置允许匿名访问的路径列表
- ✅ **结构化日志** - 内置详细的日志记录，便于调试
- ✅ **可空引用类型** - 完全支持 C# 8.0+ 的可空引用类型
- ✅ **多框架支持** - 支持 .NET 6.0 / 7.0 / 8.0 / 9.0 / 10.0

### 设计目标

1. **简洁性** - 提供简单的 API，快速集成到现有项目
2. **灵活性** - 支持自定义权限验证逻辑
3. **可扩展性** - 基于 ASP.NET Core 的授权基础设施
4. **可维护性** - 清晰的代码结构和完善的文档

---

## 架构设计

### 分层架构

```
Endpoint / Controller Metadata
            │ [Authorize] / RequirePermission
            ▼
ASP.NET Core Authentication + Authorization Middleware
            │ 标准 DefaultPolicy / NamedPolicy
            ▼
PermissionAuthorizationHandler
            │ PermissionContext + RequestAborted
            ▼
IPermissionEvaluator
       ┌────┴────┐
       │         │
旧接口适配器    业务评估器
IPermission     Endpoint、用户、路由和业务资源
VerifyService
```

认证失败和授权挑战由 ASP.NET Core 处理器负责，权限评估器只返回 `AuthorizationDecision`。`DependencyError`、`NotConfigured` 和 `Denied` 均默认 fail-closed。

### 设计模式

1. **策略模式 (Strategy Pattern)**
   - `IPermissionVerifyService` 接口定义权限验证策略
   - 用户可以实现不同的权限验证逻辑

2. **处理器模式 (Handler Pattern)**
   - `PermissionAuthorizationHandler` 实现 `IAuthorizationHandler`
   - 处理 `PermissionRequirement` 授权需求

3. **标准策略配置**
   - 通过 `AuthorizationOptions.DefaultPolicy` 配置默认权限策略
   - 不覆盖宿主的 `IAuthorizationPolicyProvider`

4. **依赖注入 (Dependency Injection)**
   - 所有核心组件都注册到 DI 容器
   - 支持替换和扩展

---

## 核心组件

### 1. PermissionRequirement (授权需求)

**职责**：
- 定义旧路径模式允许匿名访问的路径列表
- 实现 `IAuthorizationRequirement` 接口

**代码**：
```csharp
public class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(params string[] allowAnonymousPaths)
    {
        AllowAnonymousPaths = allowAnonymousPaths;
    }

    /// <summary>
    /// 允许匿名访问的路径数组，读取和设置时防御性复制并规范化
    /// </summary>
    public string[] AllowAnonymousPaths { get; set; }
}
```

**特点**：
- 是一个简单的数据载体（POCO）
- 包含允许匿名访问的路径列表
- 路径匹配使用 `StartsWithSegments`，按路径段边界且不区分大小写

### 2. PermissionAuthorizationHandler (授权处理器)

**职责**：
- 实现授权逻辑
- 构造 `PermissionContext`
- 调用构造函数注入的 `IPermissionEvaluator`
- 返回授权结果

**核心流程**：
```csharp
var permissionContext = new PermissionContext(
    httpContext,
    httpContext.GetEndpoint(),
    requestPath.Value!.ToLowerInvariant(),
    httpContext.Request.Method,
    httpContext.Request.RouteValues,
    context.User,
    permissionMetadata);

var decision = await _evaluator.AuthorizeAsync(
    permissionContext,
    httpContext.RequestAborted);

if (decision.IsAllowed)
    context.Succeed(requirement);
else
    context.Fail();
```

**依赖注入**：
```csharp
services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
```

### 3. IPermissionVerifyService (权限验证服务接口)

**职责**：
- 定义权限验证的抽象接口
- 允许用户自定义权限验证逻辑

**接口定义**：
```csharp
public interface IPermissionVerifyService
{
    /// <summary>
    /// 验证当前用户是否有访问指定路径的权限
    /// </summary>
    /// <param name="path">请求的路径（已转换为小写）</param>
    /// <returns>如果用户有权限返回 true，否则返回 false</returns>
    Task<bool> HasPermission(string path);
}
```

**实现示例**：
```csharp
public class MyPermissionService : IPermissionVerifyService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public async Task<bool> HasPermission(string path)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userId = httpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return false;

        // 从数据库获取用户权限
        var permissions = await _repository
            .GetUserPermissionsAsync(userId);

        // 检查权限
        var requestPath = new PathString(path);
        return permissions.Any(p =>
            requestPath.StartsWithSegments(new PathString(p.Path), StringComparison.OrdinalIgnoreCase));
    }
}
```

**注册为 Scoped 服务**：
```csharp
services.AddScoped<IPermissionVerifyService, MyPermissionService>();
```

### 4. AuthorizationOptions (标准策略配置)

本包不再注册自定义 `IAuthorizationPolicyProvider`。`AddPermissionAuthorization` 通过标准 `AuthorizationOptions` 配置 `DefaultPolicy` 和 `DefaultPermissionPolicy` 命名策略，因此宿主已有的命名策略、动态策略 Provider 和 `FallbackPolicy` 不会被包覆盖。

默认策略包含：

- `RequireAuthenticatedUser()`，未认证请求由框架挑战并返回 401；
- `PermissionRequirement`，负责调用权限评估器；
- 旧路径模式的匿名路径兼容行为。

### 5. ServiceCollectionExtensions (服务扩展)

**职责**：
- 提供便捷的服务注册方法
- 配置授权策略
- 注册所有必要的服务

**核心方法**：
```csharp
public static IServiceCollection AddPathBasedAuthorization<TPermissionService>(
    this IServiceCollection services,
    params string[] allowAnonymousPaths)
    where TPermissionService : class, IPermissionVerifyService
{
    // 1. 注册旧接口适配器
    services.AddScoped<IPermissionVerifyService, TPermissionService>();
    services.AddScoped<IPermissionEvaluator, LegacyPermissionEvaluator>();

    // 新接口使用 AddPermissionAuthorization<TPermissionEvaluator>() 注册评估器
    services.AddAuthorization(options =>
    {
        var permissionRequirement = new PermissionRequirement(allowAnonymousPaths);
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddPermissionRequirement(permissionRequirement)
            .Build();
        options.DefaultPolicy = policy;
        options.AddPolicy(ServiceCollectionExtensions.DefaultPolicyName, policy);
    });

    // 2. 注册 HTTP 上下文访问器
    services.AddHttpContextAccessor();

    // 3. 注册 Scoped 授权处理器
    services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

    return services;
}
```

---

## 授权流程

### 完整的授权流程

```
┌─────────────────────────────────────────────────────────┐
│ 1. HTTP 请求进入                                       │
│    GET /api/user/profile                               │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ↓
┌─────────────────────────────────────────────────────────┐
│ 2. Authentication Middleware (认证中间件)                │
│    - 验证 JWT Token / Cookie                            │
│    - 创建 ClaimsPrincipal                                │
│    - 设置 context.User                                   │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ↓
┌─────────────────────────────────────────────────────────┐
│ 3. Authorization Middleware (授权中间件)                 │
│    - 查找 endpoint / controller 上的 [Authorize] 特性    │
│    - 调用 IAuthorizationService.AuthorizeAsync()        │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ↓
┌─────────────────────────────────────────────────────────┐
│ 4. IAuthorizationService                                │
│    - 调用 IAuthorizationPolicyProvider.GetDefaultPolicy()│
│    - 获取 DefaultPermissionPolicy                       │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ↓
┌─────────────────────────────────────────────────────────┐
│ 5. 标准 AuthorizationOptions                             │
│    - 返回 DefaultPolicy 和命名策略                      │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ↓
┌─────────────────────────────────────────────────────────┐
│ 6. Authorization Service 调用所有授权处理器              │
│    - 找到 PermissionAuthorizationHandler                │
│    - 调用 HandleRequirementAsync()                      │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ↓
┌─────────────────────────────────────────────────────────┐
│ 7. PermissionAuthorizationHandler 授权检查              │
└──────────────────────┬──────────────────────────────────┘
                       │
         ┌─────────────┴──────────────┐
         ↓                            ↓
┌──────────────────────┐    ┌──────────────────────────────┐
│ 8. Endpoint / 路径元数据 │    │ 9. 策略认证需求                │
│ PermissionMetadata    │    │ RequireAuthenticatedUser()     │
│                      │    │                              │
│ [是] ────────────────┼────┼──> [是]                      │
│   │                  │    │   │                         │
│   │                  │    │   ↓                         │
│   │                  │    │ 10. 权限上下文评估            │
│   │                  │    │ IPermissionEvaluator         │
│   │                  │    │ .AuthorizeAsync(context)     │
│   ↓                  │    │   │                         │
│ 授权成功              │    │   [有权限]                  │
│ context.Succeed()    │    │     │                       │
│                      │    │     ↓                       │
│                      │    │   授权成功                   │
│                      │    │ context.Succeed()          │
│                      │    │                             │
│                      │    └──> [无权限]                 │
│                      │          ↓                       │
│                      │        授权失败                  │
│                      │        context.Fail()            │
│                      │                                 │
│                      └──> [未认证]                     │
│                            ↓                           │
│                          授权失败                       │
│                          context.Fail()                 │
```

### 决策树

```
开始
  │
  ├─> Endpoint 有 [AllowAnonymous]？ ── Yes ──> 框架跳过授权 ✓
  │
  └─> 默认策略要求认证？
       │
       ├─ No ──> 授权失败 ✗ (401 Unauthorized)
       │
       └─ Yes ──> 有显式权限元数据？
                    │
                    ├─ No 且命中旧路径列表 ──> 权限需求通过 ✓
                    │
                    └─ 否 ──> 调用 IPermissionEvaluator
                                  │
                                  ├─ 拒绝 / 异常 ──> 403 Forbidden
                                  └─ 允许 ──> 授权成功 ✓
  │
```

---

## 实现原理

### 1. 授权策略的注册

在 ASP.NET Core 中，授权策略通过 `AuthorizationOptions` 注册：

```csharp
services.AddAuthorization(options =>
{
    // 创建授权需求
    var requirement = new PermissionRequirement(
        "/api/login",
        "/api/register"
    );

    // 创建策略并添加需求
    options.AddPolicy("DefaultPermissionPolicy", policy =>
    {
        policy.Requirements.Add(requirement);
    });
});
```

**结果**：
- 策略名称：`DefaultPermissionPolicy`
- 包含需求：`PermissionRequirement`
- 需求包含：`AllowAnonymousPaths = ["/api/login", "/api/register"]`

### 2. 授权策略的提供

当使用 `[Authorize]` 特性时（不指定策略名称），ASP.NET Core 会调用 `GetDefaultPolicyAsync()`：

```csharp
public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
{
    // 返回名为 "DefaultPermissionPolicy" 的策略
    return Task.FromResult(
        _options.GetPolicy(ServiceCollectionExtensions.DefaultPolicyName)
        ?? new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()  // 回退到默认需求
            .Build()
    );
}
```

**效果**：
- 所有使用 `[Authorize]` 的地方都会使用 `DefaultPermissionPolicy`
- 策略包含 `PermissionRequirement`
- 授权时会调用 `PermissionAuthorizationHandler`

### 3. 授权处理器的执行

ASP.NET Core 的授权服务会找到所有实现了 `IAuthorizationHandler<PermissionRequirement>` 的处理器：

```csharp
internal class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // 实现授权逻辑...
    }
}
```

**执行顺序**：
1. 检查匿名路径
2. 检查用户认证
3. 调用权限验证服务
4. 返回授权结果

### 4. 路径匹配逻辑

路径匹配使用 `PathString.StartsWithSegments` 按路径段前缀匹配，且不区分大小写：

```csharp
var requestPath = httpContext.Request.Path;

// 检查是否在允许匿名访问的路径列表中
if (requirement.AllowAnonymousPaths.Any(t =>
    requestPath.StartsWithSegments(new PathString(t), StringComparison.OrdinalIgnoreCase)))
{
    context.Succeed(requirement);
    return;
}
```

**匹配示例**：
- 配置：`AllowAnonymousPaths = ["/api/login", "/api/public"]`
- `/api/login` → ✓ 匹配
- `/api/login/callback` → ✓ 匹配（路径段前缀）
- `/api/public/data` → ✓ 匹配（路径段前缀）
- `/api/private` → ✗ 不匹配
- `/api/login-export` → ✗ 不匹配（不是路径段边界）
- `/admin/api/login/delete` → ✗ 不匹配（不是前缀）
- `/API/LOGIN` → ✓ 匹配（不区分大小写）

---

## 扩展点

### 1. 自定义权限验证逻辑

通过实现 `IPermissionVerifyService` 接口：

```csharp
public class DatabasePermissionService : IPermissionVerifyService
{
    private readonly IUserPermissionRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public async Task<bool> HasPermission(string path)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userId = httpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return false;

        // 从数据库获取权限
        var permissions = await _repository
            .GetUserPermissionsAsync(userId);

        var requestPath = new PathString(path);
        return permissions.Any(p =>
            requestPath.StartsWithSegments(new PathString(p.Path), StringComparison.OrdinalIgnoreCase));
    }
}
```

**注册**：
```csharp
services.AddScoped<IPermissionVerifyService, DatabasePermissionService>();
```

### 2. 多种权限验证策略

可以注册多个权限验证服务，使用委托模式选择：

```csharp
public class CompositePermissionService : IPermissionVerifyService
{
    private readonly IEnumerable<IPermissionVerifyService> _services;

    public CompositePermissionService(
        IEnumerable<IPermissionVerifyService> services)
    {
        _services = services;
    }

    public async Task<bool> HasPermission(string path)
    {
        // 依次尝试所有服务，只要有一个返回 true 就成功
        foreach (var service in _services)
        {
            if (await service.HasPermission(path))
                return true;
        }

        return false;
    }
}
```

### 3. 基于角色的权限验证

结合角色和路径进行验证：

```csharp
public class RoleBasedPermissionService : IPermissionVerifyService
{
    public async Task<bool> HasPermission(string path)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var user = httpContext?.User;

        if (user == null)
            return false;

        // 管理员可以访问所有路径
        if (user.IsInRole("Admin"))
            return true;

        // 检查角色权限
        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

        return userRole switch
        {
            "User" => path.StartsWith("/api/user"),
            "Guest" => path.StartsWith("/api/public"),
            _ => false
        };
    }
}
```

### 4. 使用缓存优化性能

缓存应由业务权限评估器或 ACL 适配层实现。本包不固定缓存 SDK、缓存键或过期时间，避免将通用授权基础设施与具体业务权限服务耦合。

---

## 最佳实践

### 1. 路径配置规范

**推荐**：使用明确的路径前缀

```csharp
services.AddPathBasedAuthorization<MyPermissionService>(
    "/api/auth/login",       // 登录相关
    "/api/auth/register",    // 注册相关
    "/api/public",           // 公开接口
    "/health",               // 健康检查
    "/swagger"               // API 文档
);
```

**不推荐**：使用根路径或过于宽泛的路径

```csharp
// ❌ 不推荐：过于宽泛
services.AddPathBasedAuthorization<MyPermissionService>(
    "/",        // 会匹配所有路径
    "/api"      // 会匹配所有 /api 开头的路径
);
```

### 2. 权限验证服务设计

**推荐**：使用 Scoped 生命周期

```csharp
services.AddScoped<IPermissionVerifyService, MyPermissionService>();
```

**原因**：
- 可以访问作用域内的服务（如 `HttpContext`）
- 符合 ASP.NET Core 的服务生命周期最佳实践

**不推荐**：使用 Singleton 生命周期

```csharp
// ❌ 不推荐
services.AddSingleton<IPermissionVerifyService, MyPermissionService>();
```

**原因**：
- 可能无法访问 Scoped 服务（如 `DbContext`）
- 可能导致内存泄漏

### 3. 异步操作

**推荐**：使用异步方法

```csharp
public async Task<bool> HasPermission(string path)
{
    // 从数据库异步获取权限
    var permissions = await _repository.GetUserPermissionsAsync(userId);
    var requestPath = new PathString(path);
    return permissions.Any(p =>
        requestPath.StartsWithSegments(new PathString(p.Path), StringComparison.OrdinalIgnoreCase));
}
```

**不推荐**：使用同步方法

```csharp
// ❌ 不推荐
public Task<bool> HasPermission(string path)
{
    // 同步数据库查询会阻塞线程
    var permissions = _repository.GetUserPermissions(userId);
    return Task.FromResult(permissions.Any(...));
}
```

### 4. 错误处理

**推荐**：优雅地处理错误

```csharp
public async Task<bool> HasPermission(string path)
{
    try
    {
        var permissions = await _repository.GetUserPermissionsAsync(userId);
        var requestPath = new PathString(path);
        return permissions.Any(p =>
            requestPath.StartsWithSegments(new PathString(p.Path), StringComparison.OrdinalIgnoreCase));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "权限验证失败");
        return false; // 失败时拒绝访问（安全优先）
    }
}
```

### 5. 日志记录

**推荐**：记录关键操作

```csharp
public async Task<bool> HasPermission(string path)
{
    var userId = GetCurrentUserId();
    _logger.LogInformation("验证用户 {UserId} 对路径 {Path} 的访问权限", userId, path);

    var hasPermission = await _repository.HasPermissionAsync(userId, path);

    _logger.LogInformation(
        "用户 {UserId} 对路径 {Path} 的访问权限验证结果：{Result}",
        userId, path, hasPermission);

    return hasPermission;
}
```

---

## 常见问题

### 1. 401 Unauthorized vs 403 Forbidden

| 状态码 | 含义 | 触发条件 |
|-------|------|---------|
| 401 Unauthorized | 未认证 | 用户未登录或 Token 无效 |
| 403 Forbidden | 禁止访问 | 用户已登录但无权限 |

**示例**：
```csharp
// 401: 用户未登录
GET /api/user/profile
Authorization: (空)
→ 401 Unauthorized

// 403: 用户已登录但无权限
GET /api/admin/settings
Authorization: Bearer <valid_token>
→ 403 Forbidden
```

### 2. 路径大小写问题

**问题**：Windows 不区分大小写，Linux 区分大小写

**解决方案**：使用带 `StringComparison.OrdinalIgnoreCase` 的路径段前缀匹配

```csharp
var requestPath = httpContext.Request.Path;

if (requirement.AllowAnonymousPaths.Any(t =>
    requestPath.StartsWithSegments(new PathString(t), StringComparison.OrdinalIgnoreCase)))
{
    // 使用 OrdinalIgnoreCase 确保大小写行为一致
}
```

### 3. 路径参数问题

**问题**：路径参数可能导致匹配失败

**示例**：
```csharp
// 请求
GET /api/users/123

// 检查
queryUrl = "/api/users/123"
AllowAnonymousPaths = ["/api/users"]  // ✓ 匹配成功（路径段前缀）
```

**建议**：
- 使用路径前缀匹配（`/api/users`）
- 避免使用具体路径（`/api/users/123`）

### 4. 多个 [Authorize] 特性

**问题**：多个策略的合并行为

```csharp
[Authorize]
[Authorize(Policy = "AdminOnly")]
public IActionResult AdminOnlyAction()
{
    // 需要同时满足两个策略
}
```

**结果**：AND 逻辑（所有策略都必须满足）

### 5. 与 ASP.NET Core 内置授权的对比

| 特性 | ASP.NET Core 内置 | 本库 |
|------|------------------|------|
| 基于角色 | `[Authorize(Roles = "Admin")]` | 需要自定义实现 |
| 基于策略 | `[Authorize(Policy = "PolicyName")]` | 支持且简化 |
| 基于路径 | 不支持 | ✅ 原生支持 |
| 动态权限 | 需要自定义 | ✅ 通过接口实现 |

---

## 性能优化

### 1. 使用缓存缓存权限数据

```csharp
public class CachedPermissionService : IPermissionVerifyService
{
    private readonly IPermissionVerifyService _inner;
    private readonly IMemoryCache _cache;

    public async Task<bool> HasPermission(string path)
    {
        var userId = GetCurrentUserId();
        var cacheKey = $"permissions:{userId}";

        // 缓存用户的所有权限
        var permissions = await _cache.GetOrCreateAsync(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return _inner.GetAllPermissions(userId);
        });

        var requestPath = new PathString(path);
        return permissions.Any(p =>
            requestPath.StartsWithSegments(new PathString(p.Path), StringComparison.OrdinalIgnoreCase));
    }
}
```

**优势**：
- 减少数据库查询
- 提高响应速度
- 降低数据库负载

### 2. 使用预编译的路径匹配

```csharp
public class OptimizedPermissionService : IPermissionVerifyService
{
    private readonly List<PathString> _compiledPatterns;

    public OptimizedPermissionService()
    {
        // 预编译路径模式（使用正则表达式或其他方法）
        _compiledPatterns = new List<PathString>
        {
            new("/api/users"),
            new("/api/products")
        };
    }

    public Task<bool> HasPermission(string path)
    {
        // 使用预编译的模式进行匹配
        var requestPath = new PathString(path);
        return Task.FromResult(_compiledPatterns.Any(p =>
            requestPath.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)));
    }
}
```

### 3. 异步数据库查询

**推荐**：使用异步方法

```csharp
// ✓ 推荐：异步查询
public async Task<bool> HasPermission(string path)
{
    var permissions = await _repository
        .GetUserPermissionsAsync(userId);
    var requestPath = new PathString(path);
    return permissions.Any(p =>
        requestPath.StartsWithSegments(new PathString(p.Path), StringComparison.OrdinalIgnoreCase));
}
```

**不推荐**：使用同步方法

```csharp
// ✗ 不推荐：同步查询（阻塞线程）
public Task<bool> HasPermission(string path)
{
    var permissions = _repository.GetUserPermissions(userId);
    var requestPath = new PathString(path);
    return Task.FromResult(permissions.Any(p =>
        requestPath.StartsWithSegments(new PathString(p.Path), StringComparison.OrdinalIgnoreCase)));
}
```

### 4. 批量加载权限

```csharp
public class BatchPermissionService : IPermissionVerifyService
{
    private readonly ConcurrentDictionary<string, List<string>> _userPermissions = new();

    public async Task<bool> HasPermission(string path)
    {
        var userId = GetCurrentUserId();

        // 如果未缓存，批量加载用户的所有权限
        if (!_userPermissions.ContainsKey(userId))
        {
            var permissions = await _repository
                .GetUserPermissionsAsync(userId);
            _userPermissions[userId] = permissions.Select(p => p.Path).ToList();
        }

        var requestPath = new PathString(path);
        return _userPermissions[userId].Any(p =>
            requestPath.StartsWithSegments(new PathString(p), StringComparison.OrdinalIgnoreCase));
    }
}
```

---

## 安全考虑

### 1. 路径遍历攻击

**问题**：恶意用户可能使用路径遍历绕过权限检查

**示例**：
```http
GET /api/users/../admin/settings
```

**防护**：
- ASP.NET Core 会自动规范化路径
- 权限检查使用规范化后的路径

### 2. 信息泄露

**问题**：错误信息可能泄露敏感信息

**推荐**：记录详细日志，返回通用错误

```csharp
if (!hasPermission)
{
    // 记录详细日志
    _logger.LogWarning("用户 {UserId} 对路径 {Path} 没有访问权限",
        userId, path);

    // 返回通用错误（不泄露具体原因）
    return false;
}
```

### 3. 时序攻击

**问题**：通过响应时间推断路径是否存在

**防护**：
- 使用恒定时间比较
- 添加随机延迟（不推荐，影响性能）

### 4. 权限提升

**问题**：用户尝试提升权限

**防护**：
- 始终从可信源（数据库）获取权限
- 不要信任客户端传入的权限信息
- 验证 Token 的完整性

### 5. 匿名路径配置

**推荐**：最小化匿名路径

```csharp
// ✓ 推荐：只包含必要的匿名路径
services.AddPathBasedAuthorization<MyPermissionService>(
    "/api/auth/login",
    "/api/auth/register",
    "/health",
    "/swagger"
);

// ✗ 不推荐：包含过多匿名路径
services.AddPathBasedAuthorization<MyPermissionService>(
    "/",           // 所有路径
    "/api"         // 所有 API 路径
);
```

---

## 总结

`Azrng.AspNetCore.Authorization.Default` 是一个简洁、灵活的 ASP.NET Core 授权库，通过基于路径的权限验证机制，提供了细粒度的访问控制。

**核心优势**：
- ✅ 简单易用 - 几行代码即可集成
- ✅ 灵活可扩展 - 通过接口自定义权限逻辑
- ✅ 完善的日志 - 便于调试和监控
- ✅ 性能优化 - 支持缓存和异步操作

**适用场景**：
- 需要基于路径的权限控制
- 需要自定义权限验证逻辑
- 需要支持匿名路径配置
- 需要详细的授权日志

**使用建议**：
- 结合数据库或缓存实现权限验证
- 使用 Scoped 生命周期注册服务
- 合理配置匿名路径
- 记录详细的授权日志
- 定期审查和优化权限配置

通过正确使用此库，可以实现安全、高效的 ASP.NET Core 应用授权机制。
