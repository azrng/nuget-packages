# Azrng.AspNetCore.Authorization.Default

为 ASP.NET Core 提供基于 Endpoint 权限元数据的默认授权策略。权限业务由应用实现 `IPermissionEvaluator`，本包负责把认证、Endpoint 元数据和请求上下文交给评估器，并将结果接入标准授权管道。

## 安装

```bash
dotnet add package Azrng.AspNetCore.Authorization.Default --prerelease
```

当前版本：`2.0.0-beta1`。

## 快速开始

先配置应用自己的认证方案，并按 ASP.NET Core 顺序启用认证和授权中间件：

```csharp
builder.Services.AddAuthentication(/* 应用的认证方案 */);
builder.Services.AddAuthorization();
builder.Services.AddPermissionAuthorization<MyPermissionEvaluator>();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
```

实现权限评估器：

```csharp
using Azrng.AspNetCore.Authorization.Default;

public sealed class MyPermissionEvaluator : IPermissionEvaluator
{
    public Task<AuthorizationDecision> AuthorizeAsync(
        PermissionContext context,
        CancellationToken cancellationToken = default)
    {
        var isAdmin = context.User.IsInRole("Admin");
        var hasOrderPermission = context.RequiredPermissions
            .SelectMany(metadata => metadata.Permissions)
            .Contains("orders.read", StringComparer.OrdinalIgnoreCase);

        return Task.FromResult(isAdmin || hasOrderPermission
            ? AuthorizationDecision.Allow()
            : AuthorizationDecision.Deny("permission-denied"));
    }
}
```

`PermissionContext` 提供以下信息：

- `HttpContext` 和当前 `Endpoint`
- 小写请求路径、HTTP 方法和路由参数快照
- 当前用户 `ClaimsPrincipal`
- Endpoint 上全部 `IPermissionMetadata`

权限评估器默认按 Scoped 生命周期注册。数据库、远程 ACL 和缓存等业务依赖应由评估器自行注入，并使用传入的 `CancellationToken`。

## 声明权限

MVC Controller 或 Action 使用特性：

```csharp
[RequirePermission("orders.read")]
public IActionResult GetOrders() => Ok();
```

Minimal API 使用 Endpoint 扩展：

```csharp
app.MapGet("/orders", () => Results.Ok())
    .RequirePermission("orders.read");

app.MapGet("/orders/export", () => Results.Ok())
    .RequirePermission(
        PermissionMatchMode.Any,
        "orders.read",
        "orders.export");
```

同一个声明中的权限码默认要求全部满足，`PermissionMatchMode.Any` 表示满足任意一个即可。多个权限声明由 ASP.NET Core 按授权策略合并。

不需要认证的接口使用 ASP.NET Core 原生 `[AllowAnonymous]` 或 `.AllowAnonymous()`，本包不再提供路径白名单配置。

## 授权行为

`AddPermissionAuthorization` 配置两个相同的策略：

- 默认策略：供不指定策略名的 `[Authorize]` 使用
- `DefaultPermissionPolicy`：供 `RequirePermissionAttribute` 和 `RequirePermission` 使用

两者都要求已认证用户，并执行一次 `IPermissionEvaluator`。本包通过 `AuthorizationOptions` 配置策略，不替换宿主的 `IAuthorizationPolicyProvider`。未认证请求由 ASP.NET Core 返回 401；已认证但权限评估失败返回 403；评估器异常按 fail-closed 处理并记录结构化日志。

## API 参考

| API | 用途 |
| --- | --- |
| `AddPermissionAuthorization<TPermissionEvaluator>()` | 注册默认权限策略、处理器和评估器 |
| `IPermissionEvaluator` | 实现业务权限判断 |
| `PermissionContext` | 提供请求、用户和 Endpoint 权限上下文 |
| `AuthorizationDecision` | 表示允许、拒绝、未配置或依赖异常 |
| `RequirePermissionAttribute` | 为 MVC Controller / Action 声明权限 |
| `RequirePermission(...)` | 为 Minimal API Endpoint 声明权限 |
| `PermissionMetadata` | 表示 Endpoint 上的权限元数据 |

## 升级到 2.0

2.0.0-beta1 是破坏性版本。1.x 的路径权限兼容层已移除，应用需要：

1. 将权限服务改为实现 `IPermissionEvaluator`。
2. 使用 `PermissionContext` 读取请求路径、方法、路由、用户和权限元数据。
3. 返回 `AuthorizationDecision`，并将注册方式改为 `AddPermissionAuthorization<TPermissionEvaluator>()`。
4. 使用 `[AllowAnonymous]` 或 `.AllowAnonymous()` 声明公开接口。

完整变更记录见 [CHANGELOG.md](https://github.com/azrng/nuget-packages/blob/main/src/Shared/Azrng.AspNetCore.Authorization.Default/CHANGELOG.md)。

## 支持框架

包继续提供 `net6.0`、`net7.0`、`net8.0`、`net9.0` 和 `net10.0` 目标框架，并依赖 ASP.NET Core shared framework。

## 许可证

版权归 Azrng 所有。
