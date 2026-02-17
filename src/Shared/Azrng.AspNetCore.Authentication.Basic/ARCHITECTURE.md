# Azrng.AspNetCore.Authentication.Basic 架构设计文档

## 一、项目概述

`Azrng.AspNetCore.Authentication.Basic` 是一个基于 ASP.NET Core 认证中间件实现的 HTTP Basic 认证库。该项目遵循 ASP.NET Core 认证架构的设计模式，提供了开箱即用的 Basic 认证功能，同时支持高度的自定义扩展。

### 1.1 目标

- 提供简单易用的 HTTP Basic 认证方案
- 支持自定义用户凭据验证逻辑
- 支持自定义用户 Claims 生成
- 返回统一的 JSON 格式错误响应
- 兼容 .NET 6.0 及以上版本

### 1.2 技术栈

- **目标框架**: net6.0; net7.0; net8.0; net9.0; net10.0
- **核心依赖**:
  - `Microsoft.AspNetCore.App` (ASP.NET Core 框架引用)
  - `Azrng.Core` (核心工具库，提供 JSON 序列化能力)

---

## 二、项目结构

```
Azrng.AspNetCore.Authentication.Basic/
├── BasicAuthentication.cs                    # 认证方案常量定义
├── BasicOptions.cs                           # 认证配置选项
├── BasicAuthenticationHandler.cs             # 认证处理器（核心）
├── IBasicAuthorizeVerify.cs                  # Claims 生成接口
├── DefaultBasicAuthorizeVerify.cs            # 默认 Claims 实现
├── AuthenticationBuilderExtension.cs         # DI 扩展方法
├── Azrng.AspNetCore.Authentication.Basic.csproj
└── README.md
```

---

## 三、核心组件设计

### 3.1 类图

```
┌─────────────────────────────────────────────────────────────────────┐
│                    ASP.NET Core 认证中间件                           │
│                  (AuthenticationMiddleware)                         │
└────────────────────────────────────┬────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────┐
│                  BasicAuthenticationHandler                          │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │  HandleAuthenticateAsync()                                  │   │
│  │  - 解析 Authorization 头                                     │   │
│  │  - Base64 解码用户凭据                                        │   │
│  │  - 调用 UserCredentialValidator 验证                         │   │
│  │  - 调用 IBasicAuthorizeVerify 获取 Claims                    │   │
│  │  - 生成 AuthenticationTicket                                 │   │
│  └─────────────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │  HandleChallengeAsync()      → 返回 401 JSON 响应            │   │
│  │  HandleForbiddenAsync()      → 返回 403 JSON 响应            │   │
│  └─────────────────────────────────────────────────────────────┘   │
└────────────────────────────────────┬────────────────────────────────┘
                                     │
                ┌────────────────────┼────────────────────┐
                ▼                    ▼                    ▼
    ┌──────────────────┐   ┌──────────────────┐   ┌──────────────────┐
    │  BasicOptions    │   │  UserCredential  │   │IBasicAuthorize   │
    │  - UserName      │   │   Validator      │   │     Verify       │
    │  - Password      │   │  (Func 委托)     │   │  - GetCurrentUser │
    └──────────────────┘   └──────────────────┘   │     Claims()     │
                                                  └──────────────────┘
```

### 3.2 组件说明

#### 3.2.1 BasicAuthentication.cs

定义认证方案的常量标识符。

```csharp
public const string AuthenticationSchema = "Basic";
```

#### 3.2.2 BasicOptions.cs

继承自 `AuthenticationSchemeOptions`，配置认证方案的各种选项。

| 属性 | 类型 | 说明 |
|------|------|------|
| `UserName` | `string` | 默认用户名 |
| `Password` | `string` | 默认密码 |
| `UserCredentialValidator` | `Func<HttpContext, string, string, Task<bool>>` | 用户凭据验证器 |

**设计要点**：`UserCredentialValidator` 使用委托而非接口，便于在配置时内联定义验证逻辑，无需创建额外类。

#### 3.2.3 BasicAuthenticationHandler.cs

核心认证处理器，继承自 `AuthenticationHandler<BasicOptions>`。

**职责**：
- 解析 HTTP `Authorization` 请求头
- 验证 Basic 认证格式（`Basic <base64-encoded-credentials>`）
- Base64 解码用户凭据
- 调用验证器验证用户名密码
- 获取用户 Claims 并生成认证票据
- 处理 401/403 错误响应

#### 3.2.4 IBasicAuthorizeVerify.cs

Claims 生成接口，用于自定义用户身份声明。

```csharp
public interface IBasicAuthorizeVerify
{
    Task<Claim[]> GetCurrentUserClaims(string userName);
}
```

#### 3.2.5 DefaultBasicAuthorizeVerify.cs

默认实现，仅返回用户名 Claim，适用于简单场景。

#### 3.2.6 AuthenticationBuilderExtension.cs

依赖注入扩展方法，提供 Fluent API 风格的配置接口。

---

## 四、认证流程

### 4.1 序列图

```
┌─────┐     ┌────────────┐     ┌───────────────────────┐     ┌──────────────┐
│Client│     │   ASP.NET  │     │BasicAuthentication    │     │    User      │
│      │     │   Core     │     │       Handler         │     │ Validator    │
└──┬──┘     └─────┬──────┘     └───────────┬───────────┘     └──────┬───────┘
   │               │                        │                        │
   │  HTTP Request │                        │                        │
   │──────────────>│                        │                        │
   │               │                        │                        │
   │               │  HandleAuthenticateAsync()                       │
   │               │───────────────────────>│                        │
   │               │                        │                        │
   │               │                        │ 检查 Authorization 头   │
   │               │                        │                        │
   │               │                        │ 解析 Base64 凭据        │
   │               │                        │                        │
   │               │                        │ UserCredentialValidator │
   │               │                        │(userName, password)     │
   │               │                        │───────────────────────>│
   │               │                        │                        │
   │               │                        │         bool            │
   │               │                        │<───────────────────────│
   │               │                        │                        │
   │               │                        │ IBasicAuthorizeVerify   │
   │               │                        │ .GetCurrentUserClaims() │
   │               │                        │───────────────────────>│
   │               │                        │                        │
   │               │                        │      Claim[]            │
   │               │                        │<───────────────────────│
   │               │                        │                        │
   │               │                        │ 创建 AuthenticationTicket│
   │               │                        │                        │
   │               │  AuthenticateResult    │                        │
   │               │  Success(ticket)       │                        │
   │               │<───────────────────────│                        │
   │               │                        │                        │
   │               │  设置 HttpContext.User │                        │
   │               │                        │                        │
   │    HTTP 200 OK │                        │                        │
   │<──────────────│                        │                        │
   │               │                        │                        │
```

### 4.2 认证步骤详解

#### 步骤 1: 请求拦截

ASP.NET Core 认证中间件拦截所有请求，根据已注册的认证方案调用对应的 Handler。

#### 步骤 2: 检查 Authorization 头

```csharp
if (!Request.Headers.ContainsKey("Authorization"))
    return AuthenticateResult.Fail("未标注 Authorization 请求头");
```

#### 步骤 3: 验证 Basic 格式

```csharp
if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
    return AuthenticateResult.Fail("Authorization 请求头格式不正确");
```

#### 步骤 4: Base64 解码

```
Authorization: Basic YWRtaW46MTIzNDU2
                  ↓ (Base64 解码)
              admin:123456
```

#### 步骤 5: 凭据验证

调用配置的 `UserCredentialValidator` 委托验证用户名密码：

```csharp
var valid = await Options.UserCredentialValidator.Invoke(
    Request.HttpContext, userName, password);
```

默认实现验证是否与配置的 `UserName` 和 `Password` 匹配。

#### 步骤 6: 生成 Claims

调用 `IBasicAuthorizeVerify.GetCurrentUserClaims()` 获取用户声明：

```csharp
var claims = await _basicAuthorizeVerify.GetCurrentUserClaims(userName);
```

#### 步骤 7: 创建认证票据

```csharp
var claimsPrincipal = new ClaimsPrincipal(
    new ClaimsIdentity(claims, Scheme.Name));
var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);
return AuthenticateResult.Success(ticket);
```

---

## 五、错误处理流程

### 5.1 401 Unauthorized (认证失败)

触发场景：
- 缺少 `Authorization` 头
- `Authorization` 头格式不正确
- 用户名或密码验证失败

响应内容：
```json
{
  "isSuccess": false,
  "message": "您无权访问该接口，请确保已经登录",
  "code": "401"
}
```

### 5.2 403 Forbidden (权限不足)

触发场景：
- 用户已认证，但不符合 `[Authorize(Roles = "Admin")]` 等角色要求

响应内容：
```json
{
  "isSuccess": false,
  "message": "您的访问权限不够，请联系管理员",
  "code": "403"
}
```

---

## 六、扩展点设计

### 6.1 自定义凭据验证

通过 `UserCredentialValidator` 委托实现数据库验证：

```csharp
options.UserCredentialValidator = async (context, userName, password) =>
{
    var userService = context.RequestServices
        .GetRequiredService<IUserService>();
    return await userService.ValidateUserAsync(userName, password);
};
```

### 6.2 自定义 Claims 生成

实现 `IBasicAuthorizeVerify` 接口：

```csharp
public class CustomBasicAuthorizeVerify : IBasicAuthorizeVerify
{
    public async Task<Claim[]> GetCurrentUserClaims(string userName)
    {
        var user = await _userRepository.GetUserByNameAsync(userName);
        return new[]
        {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("Department", user.Department)
        };
    }
}
```

---

## 七、依赖注入配置

### 7.1 默认配置流程

```csharp
services.AddAuthentication(BasicAuthentication.AuthenticationSchema)
    .AddBasicAuthentication(options => { /* ... */ });
```

**内部执行步骤**：
1. 注册 `DefaultBasicAuthorizeVerify` 作为 `IBasicAuthorizeVerify` 的实现
2. 配置 `BasicOptions` 到 ASP.NET Core Options 系统
3. 注册认证方案：`Basic` → `BasicAuthenticationHandler`

### 7.2 自定义 Claims 配置流程

```csharp
services.AddAuthentication()
    .AddBasicAuthentication<MyCustomVerify>(options => { /* ... */ });
```

**内部执行步骤**：
1. 注册 `MyCustomVerify` 作为 `IBasicAuthorizeVerify` 的实现
2. 后续步骤同默认配置

---

## 八、安全性考虑

### 8.1 HTTPS 要求

Basic 认证使用 Base64 编码传输凭据，**并非加密**。Base64 可轻松解码，因此必须配合 HTTPS 使用。

### 8.2 凭据存储

默认实现将用户名密码存储在配置文件中，生产环境应：
- 使用自定义验证器连接数据库
- 避免硬编码凭据
- 使用密钥管理服务（如 Azure Key Vault）

### 8.3 日志记录

认证失败会记录错误日志，便于安全审计，但需注意避免记录敏感信息。

---

## 九、与 ASP.NET Core 认证系统集成

### 9.1 认证方案 (Authentication Scheme)

本库注册的方案名为 `"Basic"`，可与其他认证方案（如 JWT、Cookie）共存：

```csharp
services.AddAuthentication()
    .AddJwtBearer()           // 方案 A
    .AddBasicAuthentication(); // 方案 B
```

### 9.2 组合授权

Controller 可指定使用特定认证方案：

```csharp
[Authorize(AuthenticationSchemes = "Basic")]
public IActionResult OnlyBasic() { }

[Authorize(AuthenticationSchemes = "Bearer")]
public IActionResult OnlyJwt() { }
```

---

## 十、版本演进

| 版本 | 主要变更 |
|------|----------|
| 1.1.0 | 修复 `BasicOptions` `null!` 问题、使用 `IOptionsMonitor` 避免循环依赖 |
| 1.0.0 | 支持 .NET 10 |
| 0.1.0 | 支持 .NET 9 |
| 0.0.2 | 增加 JSON 响应处理，支持 .NET 6/7/8 |
| 0.0.1 | 基础 Basic 认证实现 |

---

## 十一、参考资料

- [ASP.NET Core 认证概述](https://learn.microsoft.com/zh-cn/aspnet/core/security/authentication/)
- [HTTP Basic 认证规范 (RFC 7617)](https://datatracker.ietf.org/doc/html/rfc7617)
- [AuthenticationHandler<TOptions> 源码](https://github.com/dotnet/aspnetcore/tree/main/src/Security/Authentication/Core)
