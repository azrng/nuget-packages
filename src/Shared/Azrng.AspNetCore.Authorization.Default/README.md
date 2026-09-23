# Azrng.AspNetCore.Authorization.Default

一个基于路径的 ASP.NET Core 授权库，实现了灵活的权限验证机制。

## NuGet 包

```
dotnet add package Azrng.AspNetCore.Authorization.Default
```

## 功能特性

- ✅ 基于请求路径的权限验证
- ✅ 支持自定义权限验证逻辑
- ✅ 支持 Endpoint Metadata 和 MVC / Minimal API 声明式权限
- ✅ 支持权限上下文、权限结果和请求取消令牌
- ✅ 内置结构化日志记录
- ✅ 支持允许匿名访问的路径配置
- ✅ 可空引用类型支持
- ✅ 支持 .NET 6.0 / 7.0 / 8.0 / 9.0 / 10.0

## 快速开始

### 1. 配置认证服务

首先需要配置认证服务（如 JWT Bearer 认证）：

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearerAuthentication(options =>
    {
        options.JwtAudience = "your-audience";
        options.JwtIssuer = "your-issuer";
        options.JwtSecretKey = "your-secret-key";
    });

app.UseAuthentication();
app.UseAuthorization();
```

### 2. 实现权限验证服务

创建一个实现 `IPermissionVerifyService` 接口的类：

```csharp
public class MyPermissionService : IPermissionVerifyService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<MyPermissionService> _logger;

    public MyPermissionService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<MyPermissionService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<bool> HasPermission(string path)
    {
        // 获取当前用户 ID
        var httpContext = _httpContextAccessor.HttpContext;
        var userId = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return false;

        // 从数据库或缓存获取用户权限
        // 示例：硬编码的权限列表
        var userPermissions = new Dictionary<string, List<string>>
        {
            ["user1"] = new List<string> { "/api/user", "/api/product" },
            ["admin"] = new List<string> { "/api" } // admin 可以访问所有 /api 开头的路径
        };

        if (!userPermissions.ContainsKey(userId))
            return false;

        // 检查用户是否有访问该路径的权限
        var permissions = userPermissions[userId];
        var requestPath = new PathString(path);
        return permissions.Any(p =>
            requestPath.StartsWithSegments(new PathString(p), StringComparison.OrdinalIgnoreCase));
    }
}
```

### 3. 注册授权服务

```csharp
// 注册基于路径的授权服务（内部已注册 IPermissionVerifyService，无需重复注册）
services.AddPathBasedAuthorization<MyPermissionService>(
    "/api/login",        // 允许匿名访问的路径
    "/api/register",
    "/api/health"
);
```

`allowAnonymousPaths` 是旧路径模式的兼容配置：已认证请求命中后可以跳过权限评估，但不会绕过默认策略的认证要求。需要真正允许未认证访问时，请使用 `[AllowAnonymous]` 或不要为该 Endpoint 添加授权策略。

旧接口会自动适配为 `IPermissionEvaluator`。新代码可以直接实现上下文评估器：

```csharp
public sealed class MyPermissionEvaluator : IPermissionEvaluator
{
    public Task<AuthorizationDecision> AuthorizeAsync(
        PermissionContext context,
        CancellationToken cancellationToken = default)
    {
        var allowed = context.User.IsInRole("Admin")
            || context.RequiredPermissions.Any(permission =>
                permission.Permissions.Contains("orders.read"));

        return Task.FromResult(allowed
            ? AuthorizationDecision.Allow()
            : AuthorizationDecision.Deny());
    }
}

services.AddPermissionAuthorization<MyPermissionEvaluator>();
```

MVC 和 Minimal API 都可以声明权限元数据：

```csharp
[RequirePermission("orders.read")]
public IActionResult GetOrders() => Ok();

app.MapGet("/orders", () => Results.Ok())
    .RequirePermission("orders.read");
```

多个权限码默认全部满足，也可以指定任意一个满足：

```csharp
app.MapGet("/orders", () => Results.Ok())
    .RequirePermission(PermissionMatchMode.Any, "orders.read", "orders.manage");
```

### 4. 使用授权

在 Controller 或 Action 上使用 `[Authorize]` 特性：

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    // 需要权限验证
    [HttpGet("profile")]
    [Authorize] // 使用默认策略
    public IActionResult GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(new { UserId = userId });
    }

    // 公共接口：不添加 [Authorize] 即可访问
    [HttpGet("public")]
    public IActionResult GetPublicData()
    {
        return Ok(new { Message = "这是公开数据" });
    }
}
```

## 高级用法

### 动态权限验证

从数据库获取用户权限：

```csharp
public class DatabasePermissionService : IPermissionVerifyService
{
    private readonly IUserPermissionRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DatabasePermissionService(
        IUserPermissionRepository repository,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> HasPermission(string path)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userId = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return false;

        // 从数据库获取用户权限
        var permissions = await _repository.GetUserPermissionsAsync(userId);

        // 检查是否有权限访问该路径
        var requestPath = new PathString(path);
        return permissions.Any(p =>
            requestPath.StartsWithSegments(new PathString(p.Path), StringComparison.OrdinalIgnoreCase));
    }
}
```

### 基于角色的权限验证

结合角色和路径进行权限验证：

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

        // 检查用户角色是否有权限访问该路径
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

### 缓存权限结果

缓存属于业务评估器或 ACL 适配层，不由本包固定缓存实现、缓存键或过期时间。使用旧接口时，请在 `IPermissionVerifyService` 实现内部缓存；使用新接口时，请在 `IPermissionEvaluator` 实现内部按用户、权限码、Endpoint 和租户等业务维度设计缓存。

## API 参考

### ServiceCollectionExtensions

| 方法 | 说明 |
|------|------|
| `AddPathBasedAuthorization<TPermissionService>(services, allowAnonymousPaths)` | 添加基于路径的授权服务 |
| `AddPermissionAuthorization<TPermissionEvaluator>(services, allowAnonymousPaths)` | 添加基于权限上下文的授权服务 |

### IPermissionVerifyService

| 方法 | 说明 |
|------|------|
| `HasPermission(string path)` | 验证当前用户是否有访问指定路径的权限 |

### IPermissionEvaluator

| 方法 | 说明 |
|------|------|
| `AuthorizeAsync(PermissionContext, CancellationToken)` | 基于请求、Endpoint 和权限元数据返回授权决策 |

### Endpoint 权限声明

| 类型 / 方法 | 说明 |
|------|------|
| `RequirePermissionAttribute` | MVC Controller / Action 权限特性 |
| `RequirePermission(...)` | Minimal API Endpoint 权限扩展 |
| `PermissionMatchMode` | 多权限码的 `All` / `Any` 匹配方式 |

### PermissionRequirement

| 属性 | 类型 | 说明 |
|------|------|------|
| `AllowAnonymousPaths` | `string[]` | 允许匿名访问的路径数组 |

## 工作原理

1. **请求到达** → Authentication Middleware 根据 Endpoint 策略完成认证
2. **策略合并** → ASP.NET Core 使用默认策略和 Endpoint Metadata 合并授权需求
3. **权限上下文** → 包构造 `PermissionContext`，包含路径、方法、路由参数、用户和权限元数据
4. **权限检查** → 调用 `IPermissionEvaluator.AuthorizeAsync()`；旧接口通过适配器继续按路径判断
5. **授权结果** → 认证失败由框架返回 401，权限拒绝返回 403，依赖异常默认 fail-closed

## 版本历史

### 1.3.0 (最新)
- 🔒 修复：默认策略显式要求认证，处理器不再重复调用默认认证 Scheme
- 🔒 修复：不再覆盖宿主的 `IAuthorizationPolicyProvider`，保留命名策略和 `FallbackPolicy`
- 🆕 新增：`IPermissionEvaluator`、`PermissionContext` 和 `AuthorizationDecision`
- 🆕 新增：MVC `RequirePermissionAttribute` 与 Minimal API `RequirePermission` 扩展
- ✅ 兼容：旧 `IPermissionVerifyService` 和 `AddPathBasedAuthorization` 自动适配
- ✅ 补充：TestServer 401 / 403 / 200 管道测试和 Endpoint Metadata 回归测试

### 1.2.0
- 🔒 **安全修复**：匿名路径匹配从 `string.Contains` 子串匹配改为 `PathString.StartsWithSegments` 路径段前缀匹配，修复子串命中导致越权放行的缺陷（例如配置 `/api/login` 时 `/admin/api/login/delete` 不再被放行）
- 🐛 修复：二次认证检查改用 `AuthenticateResult.Succeeded` 判断，原 `result.Principal == null` 语义不严谨
- 🔒 收紧：`PermissionRequirement.AllowAnonymousPaths` 保持 `string[]` 公开 API 兼容，内部做防御性拷贝和路径规范化，避免运行期被外部修改
- ✅ 补充：安全相关回归测试（子串误匹配、路径段边界、大小写、认证分支等）

### 1.1.0
- 🐛 修复：`PermissionAuthorizationHandler` 错误实现 `IAuthorizationRequirement`
- 🐛 修复：字符串处理不一致，统一使用 `ToLowerInvariant()`
- ✅ 优化：添加结构化日志支持
- ✅ 优化：改进 XML 文档注释，添加详细说明
- ✅ 重构：重命名方法 `AddMyAuthorization` → `AddPathBasedAuthorization`
- ✅ 重构：重命名属性 `LoginVisitAction` → `AllowAnonymousPaths`
- 🆕 新增：向后兼容的旧方法（标记为 Obsolete）

### 1.0.0
- 多框架支持 .NET 6.0 / 7.0 / 8.0 / 9.0 / 10.0

### 1.0.0-beta1
- 更新依赖包

## 许可证

版权归 Azrng 所有
