# Azrng.AspNetCore.Authorization.Default

为 ASP.NET Core 提供基于 Endpoint 权限元数据的权限授权集成。权限业务由应用实现 `IPermissionEvaluator`，本包负责把认证、Endpoint 元数据和请求上下文交给评估器，并将结果接入标准授权管道。

## 安装

```bash
dotnet add package Azrng.AspNetCore.Authorization.Default --prerelease
```

当前版本：`2.0.0-beta3`。

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
        // 示例权限码；实际应用应从权限存储读取当前用户的权限。
        IReadOnlyList<string> grantedPermissions = new[] { "orders.read" };
        var hasRequiredPermissions = PermissionMetadataMatcher.IsSatisfiedBy(
            context.RequiredPermissions,
            grantedPermissions);

        return Task.FromResult(isAdmin || hasRequiredPermissions
            ? AuthorizationDecision.Allow()
            : AuthorizationDecision.Deny());
    }
}
```

`PermissionContext` 提供以下信息：

- `HttpContext`：请求路径、HTTP 方法、路由参数、查询参数和请求头等请求信息
- 当前用户 `ClaimsPrincipal`
- Endpoint 上全部 `IPermissionMetadata`

权限评估器默认按 Scoped 生命周期注册。数据库、远程 ACL 和缓存等业务依赖应由评估器自行注入，并使用传入的 `CancellationToken`。

## 声明权限

MVC Controller 或 Action 使用特性：

```csharp
[RequirePermission("orders.read")]
public IActionResult GetOrders() => Ok();

[RequirePermission("orders.read", "orders.export", MatchMode = PermissionMatchMode.Any)]
public IActionResult ExportOrders() => Ok();
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

`PermissionMetadataMatcher` 可直接按该契约匹配：不同声明之间要求全部满足，单个声明内部按 `All` 或 `Any` 判断；默认以不区分大小写的序号比较权限码。没有权限声明时返回拒绝。用户、租户等权限来源仍由应用评估器负责。

只要求用户登录的接口使用 ASP.NET Core 默认授权策略：

```csharp
[Authorize]
public IActionResult GetProfile() => Ok();

app.MapGet("/profile", () => Results.Ok())
    .RequireAuthorization();
```

`[RequirePermission]` 和 `.RequirePermission(...)` 使用包注册的 `PermissionPolicy`，并调用权限评估器。该策略对应的 Endpoint 必须声明至少一项 `IPermissionMetadata`；缺少元数据时会在调用评估器前拒绝请求。

普通 `[Authorize]` / `.RequireAuthorization()` 不会调用权限评估器，仍按应用配置的 ASP.NET Core `DefaultPolicy` 处理。公开接口使用 ASP.NET Core 原生 `[AllowAnonymous]` 或 `.AllowAnonymous()`。

评估器需要请求路径、HTTP 方法等信息时，可从 `PermissionContext.HttpContext.Request` 读取。本包不再隐式按所有受 `[Authorize]` 保护的路径执行权限判断。

## 授权行为

`AddPermissionAuthorization` 只注册命名权限策略 `PermissionPolicy`：

- `PermissionPolicy`：供 `RequirePermissionAttribute` 和 `RequirePermission` 使用

普通 `[Authorize]` 使用 ASP.NET Core 的默认策略；本包不修改 `AuthorizationOptions.DefaultPolicy`，也不替换宿主的 `IAuthorizationPolicyProvider`。`PermissionPolicy` 要求用户已认证并执行一次 `IPermissionEvaluator`。权限元数据缺失、已认证但评估拒绝或评估器异常都会 fail-closed；未认证请求不会调用评估器。

## API 参考

| API | 用途 |
| --- | --- |
| `AddPermissionAuthorization<TPermissionEvaluator>()` | 注册命名权限策略、处理器和评估器 |
| `ServiceCollectionExtensions.PermissionPolicyName` | 命名权限策略名称 |
| `IPermissionEvaluator` | 实现业务权限判断 |
| `PermissionContext` | 提供 `HttpContext`、用户和 Endpoint 权限上下文 |
| `AuthorizationDecision` | 表示允许或拒绝 |
| `RequirePermissionAttribute` | 为 MVC Controller / Action 声明权限 |
| `RequirePermission(...)` | 为 Minimal API Endpoint 声明权限 |
| `PermissionMetadata` | 表示 Endpoint 上的权限元数据 |
| `PermissionMetadataMatcher` | 按声明的 Any/All 匹配模式检查已授予权限 |

## 升级到 2.0

2.0.0 系列是破坏性版本。1.x 的路径权限兼容层已移除，应用需要：

1. 将权限服务改为实现 `IPermissionEvaluator`。
2. 使用 `PermissionContext.HttpContext` 读取请求路径、方法和路由，使用 `User` 与 `RequiredPermissions` 读取用户和权限元数据。
3. 返回 `AuthorizationDecision`，并将注册方式改为 `AddPermissionAuthorization<TPermissionEvaluator>()`。
4. 使用 `[AllowAnonymous]` 或 `.AllowAnonymous()` 声明公开接口。
5. 仅需要认证的接口改用普通 `[Authorize]` / `.RequireAuthorization()`；需要业务权限评估的接口使用 `[RequirePermission]` / `.RequirePermission(...)` 声明权限。每个权限策略 Endpoint 都必须带权限元数据，多项声明之间要求全部满足，单个声明内部按 `Any` 或 `All` 匹配。

完整变更记录见 [CHANGELOG.md](https://github.com/azrng/nuget-packages/blob/main/src/Shared/Azrng.AspNetCore.Authorization.Default/CHANGELOG.md)。

## 支持框架

包继续提供 `net6.0`、`net7.0`、`net8.0`、`net9.0` 和 `net10.0` 目标框架，并依赖 ASP.NET Core shared framework。

## 许可证

版权归 Azrng 所有。
