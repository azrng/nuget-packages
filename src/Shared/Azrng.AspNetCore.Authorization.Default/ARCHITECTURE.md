# Azrng.AspNetCore.Authorization.Default 架构说明

## 定位

本包是 ASP.NET Core 授权管道的通用权限适配层。它不包含菜单、角色、租户、ACL SDK 或缓存规则，只负责把框架授权请求转换为 `PermissionContext`，再交给应用提供的 `IPermissionEvaluator`。

## 组件

```text
Controller / Minimal API
        │ [Authorize] / RequirePermission
        ▼
ASP.NET Core AuthorizationService
        │ DefaultPolicy / DefaultPermissionPolicy
        ▼
PermissionAuthorizationHandler
        │ PermissionContext
        ▼
IPermissionEvaluator
        │ AuthorizationDecision
        ▼
AuthorizationResult → 401 / 403 / Endpoint
```

主要类型：

| 类型 | 职责 |
| --- | --- |
| `ServiceCollectionExtensions` | 注册 Scoped 评估器、默认策略、HTTP 上下文访问器和处理器 |
| `PermissionAuthorizationHandler` | 从当前 HTTP 请求创建上下文并执行评估器 |
| `PermissionContext` | 保存请求、用户、Endpoint、路由参数和权限元数据快照 |
| `IPermissionEvaluator` | 承担业务权限判断 |
| `AuthorizationDecision` | 返回结构化授权结果 |
| `RequirePermissionAttribute` | MVC 权限声明 |
| `RequirePermission` | Minimal API 权限声明 |

## 注册和策略

```csharp
services.AddPermissionAuthorization<MyPermissionEvaluator>();
```

注册方法通过标准 `AuthorizationOptions` 设置：

```text
DefaultPolicy = RequireAuthenticatedUser + PermissionAuthorizationRequirement
DefaultPermissionPolicy = 同一策略
```

处理器需求类型是包内部实现细节，不作为应用扩展契约公开。注册过程不添加自定义 `IAuthorizationPolicyProvider`，因此宿主已有的命名策略、动态策略和回退策略不会被本包替换。

## 请求处理流程

1. ASP.NET Core 根据 Endpoint 元数据合并授权策略。
2. 默认策略先要求用户完成认证；未认证请求由认证处理器产生挑战。
3. 授权服务调用 `PermissionAuthorizationHandler`。
4. 处理器读取 `HttpContext`、Endpoint、路由参数、HTTP 方法和用户，并复制 Endpoint 上的 `IPermissionMetadata`。
5. 处理器调用 `IPermissionEvaluator.AuthorizeAsync`，传入请求取消令牌。
6. `Allowed` 调用 `context.Succeed`；其他结果调用 `context.Fail`。
7. 评估器异常按拒绝处理并写入固定 EventId 的结构化日志；请求取消异常按原语义继续抛出。

## 公开契约

### PermissionContext

`PermissionContext` 是一次授权评估的不可替换上下文快照。`RouteValues` 和 `RequiredPermissions` 在构造时复制，避免评估器持有的集合被后续 Endpoint 或请求代码修改。

```csharp
public sealed class PermissionContext
{
    public HttpContext HttpContext { get; }
    public Endpoint? Endpoint { get; }
    public string Path { get; }
    public string Method { get; }
    public RouteValueDictionary RouteValues { get; }
    public ClaimsPrincipal User { get; }
    public IReadOnlyList<IPermissionMetadata> RequiredPermissions { get; }
}
```

### IPermissionEvaluator

评估器由依赖注入创建，适合注入当前用户、数据库仓储、远程权限客户端或缓存。业务实现应自行处理未配置权限、权限数据异常和缓存失效，并在不确定时返回拒绝结果。

```csharp
public interface IPermissionEvaluator
{
    Task<AuthorizationDecision> AuthorizeAsync(
        PermissionContext context,
        CancellationToken cancellationToken = default);
}
```

### AuthorizationDecision

`Allowed` 以外的结果均不会通过授权。`DiagnosticCode` 只用于日志和指标，不应把内部诊断信息直接写入 HTTP 响应。

```csharp
AuthorizationDecision.Allow();
AuthorizationDecision.Deny("permission-denied");
AuthorizationDecision.NotConfigured("permission-not-configured");
AuthorizationDecision.DependencyError("acl-unavailable");
```

## Endpoint 权限声明

MVC：

```csharp
[RequirePermission("orders.read")]
public IActionResult GetOrders() => Ok();
```

Minimal API：

```csharp
app.MapGet("/orders", () => Results.Ok())
    .RequirePermission("orders.read");
```

`PermissionMetadata` 保存权限码和 `PermissionMatchMode`。权限匹配的具体用户、租户和资源规则属于评估器，不由本包解释。

## 生命周期和失败边界

- 评估器按 Scoped 注册，避免跨请求共享用户相关状态。
- 处理器通过构造函数注入评估器，不使用 `RequestServices` 服务定位。
- 外部权限服务应使用请求取消令牌，避免客户端断开后继续占用资源。
- 评估器抛出普通异常时 fail-closed；取消异常在请求已取消时继续传播。
- 日志只记录路径、Endpoint 名称、用户名称和决策类型，不记录 Token 或完整请求头。
- 公开接口使用 `[AllowAnonymous]` / `.AllowAnonymous()`，不依赖路径字符串白名单。

## 验证范围

测试项目覆盖：

- TestServer 未认证 401、无权限 403、授权通过 200。
- Endpoint 权限元数据、匹配模式、HTTP 方法、路由参数和用户上下文传递。
- MVC 特性和 Minimal API 扩展使用默认权限策略。
- 评估器拒绝时授权失败。

## 2.0 迁移说明

2.0.0-beta1 移除了 1.x 路径字符串授权模型、旧兼容适配器、匿名路径配置和自定义策略提供器实现。应用必须迁移到 `IPermissionEvaluator` 与 Endpoint 权限元数据；公开接口改用 ASP.NET Core 原生匿名元数据。
