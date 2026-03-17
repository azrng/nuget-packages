# Azrng.SettingConfig.BasicAuthorization

> [Azrng.SettingConfig](https://www.nuget.org/packages/SettingConfig) 的 Basic 认证扩展，为配置管理中心提供简单而安全的 HTTP Basic Authentication 授权功能。

[![NuGet](https://img.shields.io/badge/NuGet-1.3.0-green.svg)](https://www.nuget.org/packages/Azrng.SettingConfig.BasicAuthorization)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-6.0%20%7C%207.0%20%7C%208.0%20%7C%209.0%20%7C%2010.0-purple.svg)](https://dotnet.microsoft.com/download)

## ✨ 特性

- 🔐 **简单易用** - 基于标准 HTTP Basic Authentication 协议
- 🚀 **零配置启动** - 最小化配置，快速启用认证保护
- 👥 **多用户支持** - 支持配置多个独立用户账号
- 🔒 **SSL/TLS 支持** - 可配置强制使用 HTTPS 加密传输
- 🛡️ **密码哈希** - 自动将明文密码转换为 SHA1 哈希存储
- 🎯 **灵活配置** - 支持大小写敏感、SSL 重定向等选项
- 💪 **类型安全** - 完全启用可空引用类型，提升代码质量

## 📦 安装

### 通过 NuGet 安装

```bash
dotnet add package Azrng.SettingConfig.BasicAuthorization
```

### 依赖要求

- [Azrng.SettingConfig](https://www.nuget.org/packages/SettingConfig) >= 1.3.1

## 🚀 快速开始

### 基础配置

在 `Program.cs` 中配置 Basic 认证：

```csharp
using Azrng.SettingConfig;
using Azrng.SettingConfig.BasicAuthorization;

var builder = WebApplication.CreateBuilder(args);

// 获取数据库连接字符串
var conn = builder.Configuration.GetConnectionString("pgsql");

// 添加 SettingConfig 服务并配置 Basic 认证
builder.Services.AddSettingConfig(options =>
{
    // 数据库配置
    options.DbConnectionString = conn;
    options.DbSchema = "sample";

    // 路由配置
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
                    PasswordClear = "your-password-here"  // 明文密码，自动转换为 SHA1 哈希
                }
            }
        })
    };
});

var app = builder.Build();
app.UseSettingDashboard();
app.Run();
```

### 访问配置中心

启动项目后访问配置中心时，浏览器会弹出认证对话框：

```
用户名: admin
密码: your-password-here
```

## ⚙️ 配置选项

### BasicAuthAuthorizationFilterOptions

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `RequireSsl` | bool | true | 是否要求 SSL 连接才能访问配置中心 |
| `SslRedirect` | bool | true | 是否将非 SSL 请求自动重定向到 HTTPS |
| `LoginCaseSensitive` | bool | true | 用户名验证是否区分大小写 |
| `Users` | IEnumerable\<BasicAuthAuthorizationUser\> | Array.Empty\<BasicAuthAuthorizationUser\>() | 允许访问的用户列表 |

### BasicAuthAuthorizationUser

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `Login` | string | ✅ | 用户登录名 |
| `PasswordClear` | string? | ✅ | 明文密码（设置时会自动转换为 SHA1 哈希存储） |

## 🎯 使用场景

### 场景 1：开发环境快速保护

在开发环境中为配置中心添加简单保护：

```csharp
options.Authorization = new[]
{
    new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
    {
        RequireSsl = false,          // 开发环境不强制 SSL
        SslRedirect = false,
        LoginCaseSensitive = false,  // 用户名不区分大小写
        Users = new[]
        {
            new BasicAuthAuthorizationUser
            {
                Login = "dev",
                PasswordClear = "dev123"
            }
        }
    })
};
```

### 场景 2：生产环境安全配置

生产环境推荐配置（启用 SSL）：

```csharp
options.Authorization = new[]
{
    new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
    {
        RequireSsl = true,           // ✅ 强制使用 SSL
        SslRedirect = true,          // ✅ 自动重定向到 HTTPS
        LoginCaseSensitive = true,   // 用户名区分大小写
        Users = new[]
        {
            new BasicAuthAuthorizationUser
            {
                Login = "admin",
                PasswordClear = "Strong-P@ssw0rd!2024"  // ✅ 使用强密码
            }
        }
    })
};
```

### 场景 3：多用户角色管理

为不同角色配置不同用户：

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
            // 管理员账号
            new BasicAuthAuthorizationUser
            {
                Login = "admin",
                PasswordClear = "admin-secure-pass-2024"
            },
            // 运维账号
            new BasicAuthAuthorizationUser
            {
                Login = "ops",
                PasswordClear = "ops-secure-pass-2024"
            },
            // 只读账号
            new BasicAuthAuthorizationUser
            {
                Login = "viewer",
                PasswordClear = "viewer-secure-pass-2024"
            }
        }
    })
};
```

### 场景 4：结合环境变量使用

从配置文件读取用户凭据：

```csharp
// appsettings.json
{
  "SettingConfig": {
    "AdminUser": "admin",
    "AdminPassword": "your-secure-password"
  }
}

// Program.cs
var adminUser = builder.Configuration["SettingConfig:AdminUser"];
var adminPassword = builder.Configuration["SettingConfig:AdminPassword"];

options.Authorization = new[]
{
    new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
    {
        RequireSsl = true,
        Users = new[]
        {
            new BasicAuthAuthorizationUser
            {
                Login = adminUser,
                PasswordClear = adminPassword
            }
        }
    })
};
```

## 🔧 高级配置

### 自定义认证行为

如果 Basic 认证不满足需求，可以实现自定义授权过滤器：

```csharp
using Azrng.SettingConfig.Service;

public class CustomAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardFilterContext context)
    {
        var httpContext = context.HttpContext;
        var request = httpContext.Request;

        // 自定义授权逻辑
        // 例如：检查特定 Header、Token、IP 白名单等

        return true;  // 返回 true 允许访问，false 拒绝访问
    }
}

// 使用自定义过滤器
options.Authorization = new[]
{
    new CustomAuthorizationFilter()
};
```

### 组合多个授权策略

可以组合多个授权过滤器：

```csharp
options.Authorization = new[]
{
    // 先检查 Basic 认证
    new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
    {
        RequireSsl = true,
        Users = new[]
        {
            new BasicAuthAuthorizationUser { Login = "admin", PasswordClear = "password" }
        }
    }),
    // 再检查 IP 白名单
    new IpWhitelistAuthorizationFilter(new[] { "192.168.1.0/24" })
};
```

## 📝 版本更新记录

* **1.3.0** (最新)
  * 🚀 **重大更新**：适配 Azrng.SettingConfig 1.4.0 重大版本
  * ✅ **依赖升级**：更新依赖 Azrng.SettingConfig 到 1.4.0
  * 🎨 **前端重构支持**：配合主包零依赖前端的重大改进
  * ⚡ **性能提升**：享受主包 97% 资源减少和 75% 加载时间优化
  * 🔒 **安全增强**：支持主包增强的安全头配置
  * 📱 **响应式支持**：完美配合主包的移动端优化
  * ✅ 优化：完善可空引用类型注解
  * ✅ 改进：代码质量和类型安全提升

* 1.2.1
  * ✅ 优化：更新依赖 Azrng.SettingConfig 到 1.3.1
  * ✅ 优化：完善可空引用类型注解
  * ✅ 改进：代码质量和类型安全提升

* 1.2.0
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

* 1.1.0
  * 适配 .NET 10

* 1.0.0
  * 基本的 Basic 认证功能