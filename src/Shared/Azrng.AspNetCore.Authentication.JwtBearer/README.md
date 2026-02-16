# Azrng.AspNetCore.Authentication.JwtBearer

一个简单易用的 ASP.NET Core JWT Bearer 认证库，提供了开箱即用的配置和灵活的扩展能力。

## NuGet 包

```
dotnet add package Azrng.AspNetCore.Authentication.JwtBearer
```

## 功能特性

- ✅ 开箱即用的 JWT Token 生成和验证
- ✅ 自动处理 Token 过期和认证失败
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
    options.JwtSecretKey = "your-secret-key-at-least-16-characters-long";
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
            jwtConfig.JwtSecretKey = "your-secret-key";
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

> **注意**：此方法会**保留默认的** `OnAuthenticationFailed` 和 `OnChallenge` 事件处理，不会覆盖它们。

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
        // JWT 签名密钥（最少16位）
        options.JwtSecretKey = "your-very-long-secret-key";

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
| `JwtSecretKey` | `string` | 默认密钥 | JWT 签名密钥（≥16位） |
| `JwtIssuer` | `string` | `"issuer"` | JWT 颁发者标识 |
| `JwtAudience` | `string` | `"audience"` | JWT 受众标识 |
| `ValidTime` | `TimeSpan` | `24小时` | Token 有效期 |

## 默认行为

库内置了以下默认行为：

1. **Token 过期处理**：过期 Token 会自动添加 `Token-Expired: true` 响应头
2. **认证失败响应**：返回 JSON 格式的 401 错误
   ```json
   {
     "isSuccess": false,
     "message": "您无权访问该接口，请确保已经登录",
     "code": "401"
   }
   ```
3. **Token 验证**：完整验证签名、过期时间、颁发者和受众

## 版本历史

### 1.4.0 (最新)
- 🆕 新增：支持自定义 `JwtBearerEvents`，可在默认配置基础上扩展
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