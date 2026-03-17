## Azrng.SettingConfig.BasicAuthorization

该包是 [Azrng.SettingConfig](https://www.nuget.org/packages/SettingConfig) 的 Basic 认证扩展，提供基于 HTTP Basic Authentication 的授权功能。

### 使用场景

当你需要为 SettingConfig 配置中心添加 Basic 认证保护时，可以安装此扩展包。它提供了简单而安全的用户名/密码认证机制。

### 安装

```bash
dotnet add package Azrng.SettingConfig.BasicAuthorization
```

### 配置方法

在 `Program.cs` 或 `Startup.cs` 中配置：

```csharp
var conn = builder.Configuration.GetConnectionString("pgsql");
builder.Services.AddSettingConfig(options =>
{
    options.DbConnectionString = conn;
    options.DbSchema = "sample";
    options.RoutePrefix = "configDashboard";
    options.ApiRoutePrefix = "/api/configDashboard";

    // 配置 Basic 认证
    options.Authorization = new[]
    {
        new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
        {
            RequireSsl = false,           // 是否要求 SSL 连接
            SslRedirect = false,          // 是否自动重定向到 HTTPS
            LoginCaseSensitive = true,    // 登录名是否区分大小写
            Users = new[]
            {
                new BasicAuthAuthorizationUser
                {
                    Login = "admin",
                    PasswordClear = "your-password-here"  // 设置明文密码，会自动转换为 SHA1 哈希
                }
            }
        })
    };
});
```

### 配置选项说明

#### BasicAuthAuthorizationFilterOptions

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `RequireSsl` | bool | true | 是否要求 SSL 连接才能访问配置中心 |
| `SslRedirect` | bool | true | 是否将非 SSL 请求重定向到 SSL URL |
| `LoginCaseSensitive` | bool | true | 登录名验证是否区分大小写 |
| `Users` | IEnumerable\<BasicAuthAuthorizationUser\> | Array.Empty\<BasicAuthAuthorizationUser\>() | 允许访问的用户列表 |

#### BasicAuthAuthorizationUser

| 参数 | 类型 | 说明 |
|------|------|------|
| `Login` | string | 用户登录名 |
| `PasswordClear` | string? | 明文密码（设置时会自动转换为 SHA1 哈希存储） |

### 安全建议

1. **生产环境必须启用 SSL**：Basic 认证会将凭据以 Base64 编码在网络传输，不使用 SSL 会导致凭据泄露风险
2. **使用强密码**：避免使用简单密码，建议结合密码策略使用
3. **定期更换密码**：定期更新配置中心的访问密码
4. **限制访问范围**：结合防火墙或网络策略，限制配置中心的访问来源

### 多用户配置

支持配置多个用户，每个用户有独立的用户名和密码：

```csharp
options.Authorization = new[]
{
    new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
    {
        RequireSsl = true,
        SslRedirect = true,
        LoginCaseSensitive = false,
        Users = new[]
        {
            new BasicAuthAuthorizationUser { Login = "admin", PasswordClear = "admin123" },
            new BasicAuthAuthorizationUser { Login = "operator", PasswordClear = "operator123" },
            new BasicAuthAuthorizationUser { Login = "viewer", PasswordClear = "viewer123" }
        }
    })
};
```

### 密码哈希说明

该包使用 **SHA1** 哈希算法存储密码。出于向后兼容性考虑，当前版本继续使用 SHA1。

> **注意**：SHA1 已被认为是不够安全的哈希算法。未来版本可能会迁移到更安全的算法（如 SHA256 或 PBKDF2）。如果你的安全要求较高，建议：
> - 使用 SSL/TLS 保护传输层
> - 定期更换密码
> - 结合其他安全措施（如 IP 白名单）

## 版本更新记录

### 1.2.1 (最新)
  * ✅ 优化：更新依赖 Azrng.SettingConfig 到 1.3.1
  * ✅ 优化：完善可空引用类型注解
  * ✅ 改进：代码质量和类型安全提升

### 1.2.0
  * 🆕 新增：支持 .NET 9.0
  * ✅ 优化：完全启用可空引用类型支持
  * ✅ 重构：改进 `BasicAuthAuthorizationUser` 的空值处理
  * ✅ 重构：使用 `Array.Empty<T>()` 替代空数组初始化
  * ✅ 重构：移除 `Hangfire` 相关引用，统一为 `SettingConfig`
  * ✅ 改进：增强 XML 文档注释，使用中文文档
  * ✅ 改进：添加构造函数的 XML 文档注释
  * ✅ 改进：使用 `nameof` 操作符替代字符串字面量
  * ✅ 改进：添加常量定义，提高代码可读性
  * ✅ 新增：添加包级别的 README.md 文档

### 1.1.0
  * 适配 .NET 10

### 1.0.0
  * 基本的 Basic 认证功能

## 依赖项

- [Azrng.SettingConfig](https://www.nuget.org/packages/SettingConfig) >= 1.3.1

## 许可证

版权归 Azrng 所有
