# Azrng.AspNetCore.Authorization.Default

一个基于路径的 ASP.NET Core 授权库，实现了灵活的权限验证机制。

## NuGet 包

```
dotnet add package Azrng.AspNetCore.Authorization.Default
```

## 功能特性

- ✅ 基于请求路径的权限验证
- ✅ 支持自定义权限验证逻辑
- ✅ 内置结构化日志记录
- ✅ 支持允许匿名访问的路径配置
- ✅ 可空引用类型支持
- ✅ 支持 .NET 6.0+

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
        return permissions.Any(p => path.Contains(p.ToLowerInvariant()));
    }
}
```

### 3. 注册授权服务

```csharp
// 注册权限验证服务
services.AddScoped<IPermissionVerifyService, MyPermissionService>();

// 注册基于路径的授权服务
services.AddPathBasedAuthorization<MyPermissionService>(
    "/api/login",        // 允许匿名访问的路径
    "/api/register",
    "/api/health"
);
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

    // 允许匿名访问（因为配置在 allowAnonymousPaths 中）
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
        return permissions.Any(p => path.Contains(p.Path.ToLowerInvariant()));
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

### 使用缓存优化性能

```csharp
public class CachedPermissionService : IPermissionVerifyService
{
    private readonly IPermissionVerifyService _innerService;
    private readonly IMemoryCache _cache;

    public CachedPermissionService(
        IPermissionVerifyService innerService,
        IMemoryCache cache)
    {
        _innerService = innerService;
        _cache = cache;
    }

    public async Task<bool> HasPermission(string path)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userId = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return false;

        var cacheKey = $"permissions:{userId}:{path}";

        return await _cache.GetOrCreateAsync(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return _innerService.HasPermission(path);
        });
    }
}
```

## API 参考

### ServiceCollectionExtensions

| 方法 | 说明 |
|------|------|
| `AddPathBasedAuthorization<TPermissionService>(services, allowAnonymousPaths)` | 添加基于路径的授权服务 |

### IPermissionVerifyService

| 方法 | 说明 |
|------|------|
| `HasPermission(string path)` | 验证当前用户是否有访问指定路径的权限 |

### PermissionRequirement

| 属性 | 类型 | 说明 |
|------|------|------|
| `AllowAnonymousPaths` | `string[]` | 允许匿名访问的路径数组 |

## 工作原理

1. **请求到达** → 当一个请求到达需要授权的 Controller 或 Action
2. **匿名检查** → 首先检查请求路径是否在 `AllowAnonymousPaths` 列表中
3. **认证检查** → 检查用户是否已通过认证（如 JWT Token 验证）
4. **权限检查** → 调用 `IPermissionVerifyService.HasPermission()` 验证用户权限
5. **授权结果** → 返回授权成功或失败

## 版本历史

### 1.1.0 (最新)
- 🐛 修复：`PermissionAuthorizationHandler` 错误实现 `IAuthorizationRequirement`
- 🐛 修复：字符串处理不一致，统一使用 `ToLowerInvariant()`
- ✅ 优化：添加结构化日志支持
- ✅ 优化：改进 XML 文档注释，添加详细说明
- ✅ 重构：重命名方法 `AddMyAuthorization` → `AddPathBasedAuthorization`
- ✅ 重构：重命名属性 `LoginVisitAction` → `AllowAnonymousPaths`
- 🆕 新增：向后兼容的旧方法（标记为 Obsolete）

### 1.0.0
- 支持 .NET 10

### 1.0.0-beta1
- 更新依赖包

## 许可证

版权归 Azrng 所有