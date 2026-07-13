# Azrng.AspNetCore.Authentication.JwtBearer

一个简单易用的 ASP.NET Core JWT Bearer 认证库，提供了开箱即用的配置和灵活的扩展能力。

## NuGet 包

```
dotnet add package Azrng.AspNetCore.Authentication.JwtBearer
```

## 功能特性

- ✅ 开箱即用的 JWT Token 生成和验证
- ✅ 保留 ASP.NET Core JwtBearer 默认认证行为，支持按需自定义事件
- ✅ 内置性能优化（缓存 SecurityKey 和 SigningCredentials）
- ✅ 完整的 Token 验证（签名、过期时间、颁发者、受众）
- ✅ 支持自定义 JwtBearerEvents（如 SignalR 支持）
- ✅ 可空引用类型支持
- ✅ 支持 .NET 6.0+

## 快速开始

### 1. 基础配置

```csharp
// 在 Program.cs 或 Startup.cs 中配置服务
services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearerAuthentication(options =>
{
    options.JwtAudience = "your-audience";
    options.JwtIssuer = "your-issuer";
    options.JwtSecretKey = "your-secret-key-at-least-32-characters-long";
});

// 启用认证授权
app.UseAuthentication();
app.UseAuthorization();
```

### 2. 生成 Token

注入 `IBearerAuthService` 来创建 Token：

```csharp
public class AuthService
{
    private readonly IBearerAuthService _bearerAuthService;

    public AuthService(IBearerAuthService bearerAuthService)
    {
        _bearerAuthService = bearerAuthService;
    }

    // 生成仅包含用户ID的 Token
    public string GenerateToken(string userId)
    {
        return _bearerAuthService.CreateToken(userId);
    }

    // 生成包含用户ID和用户名的 Token
    public string GenerateToken(string userId, string userName)
    {
        return _bearerAuthService.CreateToken(userId, userName);
    }

    // 生成自定义 Claims 的 Token
    public string GenerateToken(IEnumerable<Claim> claims)
    {
        return _bearerAuthService.CreateToken(claims);
    }
}
```

### 3. 使用 Token 验证

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    [HttpGet("profile")]
    [Authorize] // 需要认证
    public IActionResult GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(new { UserId = userId });
    }
}
```

## 高级用法

### 支持 SignalR（从查询参数读取 Token）

如果你的应用使用 SignalR，需要从查询参数中读取 Token：

```csharp
services.AddAuthentication()
    .AddJwtBearerAuthentication(
        // JWT 配置
        jwtConfig =>
        {
            jwtConfig.JwtAudience = "your-audience";
            jwtConfig.JwtIssuer = "your-issuer";
            jwtConfig.JwtSecretKey = "your-secret-key-at-least-32-characters-long";
        },
        // JwtBearerEvents 自定义配置
        events =>
        {
            // 添加 OnMessageReceived 处理器
            events.OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                // 如果是 SignalR 请求且包含 access_token，则从查询参数读取
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/chathub") || path.StartsWithSegments("/notificationhub")))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            };
        });
```

> **注意**：库不再内置默认 `OnAuthenticationFailed` / `OnChallenge` 响应处理；传入的 `JwtBearerEvents` 会直接应用到 ASP.NET Core JwtBearer 选项。

### 使用预置 Token 过期和 401 响应

```csharp
services.AddAuthentication()
    .AddJwtBearerAuthentication(
        jwtConfig => { /* ... */ },
        events => events.UseAzrngJwtBearerDefaultResponses());
```

以上等价于同时启用：

- `UseTokenExpiredHeader()`：Token 过期时添加 `Token-Expired: true` 响应头
- `UseUnauthorizedJsonResponse()`：认证挑战时返回 Azrng 预置 JSON 401 响应体

也可以只启用其中一项：

```csharp
services.AddAuthentication()
    .AddJwtBearerAuthentication(
        jwtConfig => { /* ... */ },
        events => events.UseTokenExpiredHeader());
```

### 自定义 Token 验证失败响应

```csharp
services.AddAuthentication()
    .AddJwtBearerAuthentication(
        jwtConfig => { /* ... */ },
        events =>
        {
            events.OnChallenge = context =>
            {
                // 自定义响应
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "未授权访问",
                    code = 401
                });

                return Task.CompletedTask;
            };
        });
```

### 完整的配置选项

```csharp
services.AddAuthentication()
    .AddJwtBearerAuthentication(options =>
    {
        // JWT 签名密钥（最少 32 位，至少 8 种不同字符）
        options.JwtSecretKey = "your-secret-key-at-least-32-characters-long";

        // JWT 颁发者
        options.JwtIssuer = "https://your-domain.com";

        // JWT 受众
        options.JwtAudience = "your-api-audience";

        // Token 有效期（默认24小时）
        options.ValidTime = TimeSpan.FromHours(2);
    });
```

## API 参考

### IBearerAuthService

| 方法 | 说明 |
|------|------|
| `CreateToken(string userId)` | 生成包含用户ID的 Token |
| `CreateToken(string userId, string userName)` | 生成包含用户ID和用户名的 Token |
| `CreateToken(IEnumerable<Claim> claims)` | 生成包含自定义 Claims 的 Token |
| `ValidateToken(string token)` | 验证 Token 是否有效（签名、过期、颁发者、受众） |
| `GetJwtNameIdentifier(string jwtStr)` | 从 Token 中获取用户标识 |
| `GetJwtInfo(string jwtStr)` | 解析 Token 返回所有载荷信息 |

### JwtTokenConfig

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `JwtSecretKey` | `string` | （无默认值，必填） | JWT 签名密钥（≥32 位，至少 8 种不同字符） |
| `JwtIssuer` | `string` | `"issuer"` | JWT 颁发者标识 |
| `JwtAudience` | `string` | `"audience"` | JWT 受众标识 |
| `ValidTime` | `TimeSpan` | `24小时` | Token 有效期 |

## 内置行为

库内置了以下行为：

1. **Token 验证**：完整验证签名、过期时间、颁发者和受众
2. **密钥强制校验**：配置在首次解析或应用启动时强制校验（非空、长度 ≥ 32、至少 8 种不同字符），无默认密钥
3. **事件处理**：默认使用 ASP.NET Core JwtBearer 行为；如需 `Token-Expired` 响应头或自定义 401 响应体，可通过 `jwtBearerEventsAction` 显式配置，也可调用预置扩展方法简化配置

## 版本历史

### 1.5.1 (最新)
- 🔒 **安全**：升级 `Microsoft.AspNetCore.Authentication.JwtBearer` 到各目标框架最新 patch 版本，避免已知 IdentityModel JWT 传递依赖漏洞
- 🏗️ 调整：移除配置热更新语义，`IBearerAuthService` 构造期读取 `IOptions<JwtTokenConfig>`，避免服务签发与中间件校验配置分叉
- 🧹 调整：移除库内置默认 `JwtBearerEvents`，不再自动写入 `Token-Expired` 响应头或自定义 401 JSON，默认回到 ASP.NET Core 标准行为
- 🆕 新增：提供 `UseTokenExpiredHeader`、`UseUnauthorizedJsonResponse`、`UseAzrngJwtBearerDefaultResponses` 扩展方法，显式启用旧响应行为时更简洁

### 1.5.0
- 🔒 **安全**：移除硬编码默认密钥，改为必填并强制校验（长度 ≥ 32、至少 8 种不同字符），通过 `IValidateOptions` 收口，任何注册路径都会校验
- 🔒 **安全**：`CreateToken` 移除掩盖真实异常的 try-catch；`ValidateToken`/`GetJwt*` 对空 token 显式抛 `ArgumentException`，仅吞 `SecurityTokenException`
- 🐛 修复：`GetJwtInfo` 在 claim 值为 null 时不再抛 NRE
- 🏗️ 重构：`IBearerAuthService` 改为 Singleton（无状态）
- 🏗️ 重构：抽取共享 `TokenValidationParameters`，中间件与 `ValidateToken` 使用一致规则（统一 `ClockSkew = Zero`）
- ✅ 测试：从 7 项扩展到 27 项（覆盖过期 token、空 token、NRE、Singleton、events 配置等场景），目标框架扩展到 net8/9/10

### 1.4.0
- 🆕 新增：支持自定义 `JwtBearerEvents`
- ⚡ 优化：性能优化，缓存 `SecurityKey` 和 `SigningCredentials`
- ✅ 优化：增强 `ValidateToken` 方法，完整验证签名、过期时间、颁发者、受众
- 🔒 安全：添加可空引用类型支持
- 🐛 修复：`DateTime.Now` 改为 `DateTime.UtcNow`

### 1.3.0
- 支持 .NET 10

### 1.2.0
- 移除不必要的依赖包

### 1.1.0
- 适配 Common.Core 1.2.1 的修改

### 1.0.0
- 从包 Common.JwtToken 中迁移过来

## 许可证

版权归 Azrng 所有
