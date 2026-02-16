# Azrng.AspNetCore.Authentication.Basic

一个简单易用的 ASP.NET Core HTTP Basic 认证库。

## NuGet 包

```
dotnet add package Azrng.AspNetCore.Authentication.Basic
```

## 功能特性

- ✅ 开箱即用的 HTTP Basic 认证
- ✅ 支持自定义用户名密码验证
- ✅ 支持自定义用户 Claims 生成
- ✅ 自动处理 401/403 响应
- ✅ 可空引用类型支持
- ✅ 支持 .NET 6.0+

## 依赖说明

此包依赖以下包之一来实现 JSON 序列化：
- `Azrng.Core.Json`（推荐）
- `Azrng.Core.NewtonsoftJson`

## 快速开始

### 1. 基础配置

在 `Program.cs` 或 `Startup.cs` 中配置服务：

```csharp
services.AddAuthentication(BasicAuthentication.AuthenticationSchema)
    .AddBasicAuthentication(options =>
    {
        options.UserName = "admin";
        options.Password = "123456";
    });

// 启用认证授权
app.UseAuthentication();
app.UseAuthorization();
```

### 2. 使用认证

在 Controller 中使用 `[Authorize]` 特性：

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    [HttpGet("profile")]
    [Authorize] // 需要 Basic 认证
    public IActionResult GetProfile()
    {
        var userName = User.Identity?.Name;
        return Ok(new { UserName = userName });
    }
}
```

### 3. 客户端请求

使用 HTTP Basic 认证发送请求：

```bash
# 使用 curl
curl -u admin:123456 https://your-api.com/api/user/profile

# 或使用 Authorization 头
curl -H "Authorization: Basic YWRtaW46MTIzNDU2" https://your-api.com/api/user/profile
```

C# 示例：

```csharp
using var httpClient = new HttpClient();
var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes("admin:123456"));
httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Basic", authValue);

var response = await httpClient.GetAsync("https://your-api.com/api/user/profile");
```

## 高级用法

### 自定义用户验证逻辑

如果需要从数据库或其他来源验证用户凭据：

```csharp
services.AddAuthentication()
    .AddBasicAuthentication(options =>
    {
        // 替换默认的验证器
        options.UserCredentialValidator = async (context, userName, password) =>
        {
            // 从数据库验证用户
            var userService = context.RequestServices.GetRequiredService<IUserService>();
            return await userService.ValidateUserAsync(userName, password);
        };
    });
```

### 自定义用户 Claims

如果需要添加角色、权限等额外信息到 Claims 中：

```csharp
// 1. 实现自定义验证器
public class CustomBasicAuthorizeVerify : IBasicAuthorizeVerify
{
    public async Task<Claim[]> GetCurrentUserClaims(string userName)
    {
        // 从数据库获取用户信息
        var user = await _userRepository.GetUserByNameAsync(userName);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("Department", user.Department)
        };

        return claims.ToArray();
    }
}

// 2. 注册自定义验证器
services.AddAuthentication()
    .AddBasicAuthentication<CustomBasicAuthorizeVerify>(options =>
    {
        options.UserName = "admin"; // 可以不设置，因为使用自定义验证
        options.Password = "123456";
    });
```

### 在 Controller 中访问 Claims

```csharp
[HttpGet("info")]
[Authorize]
public IActionResult GetUserInfo()
{
    var userName = User.Identity?.Name;
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var role = User.FindFirst(ClaimTypes.Role)?.Value;
    var department = User.FindFirst("Department")?.Value;

    return Ok(new
    {
        UserName = userName,
        UserId = userId,
        Role = role,
        Department = department
    });
}
```

## API 参考

### BasicOptions

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `UserName` | `string` | `""` | 默认用户名 |
| `Password` | `string` | `""` | 默认密码 |
| `UserCredentialValidator` | `Func<HttpContext, string, string, Task<bool>>` | 默认验证逻辑 | 用户凭据验证器 |

### IBasicAuthorizeVerify

| 方法 | 说明 |
|------|------|
| `GetCurrentUserClaims(string userName)` | 获取当前用户的 Claims |

## 默认行为

库内置了以下默认行为：

1. **认证失败响应**：返回 JSON 格式的 401 错误
   ```json
   {
     "isSuccess": false,
     "message": "您无权访问该接口，请确保已经登录",
     "code": "401"
   }
   ```

2. **权限不足响应**：返回 JSON 格式的 403 错误
   ```json
   {
     "isSuccess": false,
     "message": "您的访问权限不够，请联系管理员",
     "code": "403"
   }
   ```

## 安全建议

⚠️ **重要提示**：

1. **使用 HTTPS**：Basic 认证会将用户名密码以 Base64 编码传输，必须配合 HTTPS 使用
2. **不要使用默认凭据**：生产环境中应使用自定义验证器连接数据库
3. **定期更换密码**：建议定期更换 Basic 认证的凭据
4. **限制使用场景**：Basic 认证适用于服务间调用、API 网关等场景，不适用于用户登录

## 版本历史

### 1.1.0 (最新)
- 🐛 修复：`BasicOptions` 中 `null!` 导致的运行时异常
- 🐛 修复：默认验证器使用 `IOptionsMonitor` 避免循环依赖
- 🐛 修复：`DefaultBasicAuthorizeVerify` 中未使用的依赖注入
- ✅ 优化：改进错误消息处理，提取为常量
- ✅ 优化：完善 XML 文档注释，添加使用示例
- ✅ 优化：修复 Logger 创建方式

### 1.0.0
- 支持 .NET 10

### 1.0.0-beta1
- 更新依赖包

### 0.1.0
- 适配 Common.Core 1.2.1 的修改
- 支持 .NET 9

### 0.0.2
- 增加认证失败响应内容处理
- 支持 .NET 6、.NET 7、.NET 8

### 0.0.1-beta2
- 增加认证失败响应内容处理

### 0.0.1
- 基础的 Basic 认证包

## 许可证

版权归 Azrng 所有