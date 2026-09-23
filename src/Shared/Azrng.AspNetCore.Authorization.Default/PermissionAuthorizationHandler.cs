using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 基于路径和 Endpoint 元数据的权限授权处理器。
/// </summary>
internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IHttpContextAccessor _accessor;
    private readonly IPermissionEvaluator _evaluator;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    /// <summary>
    /// 初始化 <see cref="PermissionAuthorizationHandler"/> 的新实例。
    /// </summary>
    public PermissionAuthorizationHandler(
        IHttpContextAccessor httpContextAccessor,
        IPermissionEvaluator evaluator,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _accessor = httpContextAccessor;
        _evaluator = evaluator;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var httpContext = _accessor.HttpContext;
        if (httpContext == null)
        {
            _logger.LogWarning("HTTP 上下文为空");
            context.Fail();
            return;
        }

        var requestPath = httpContext.Request.Path;
        var queryUrl = requestPath.Value?.ToLowerInvariant();
        if (string.IsNullOrEmpty(queryUrl))
        {
            _logger.LogWarning(AuthorizationEventIds.EmptyPath, "请求路径为空");
            context.Fail();
            return;
        }

        var endpoint = httpContext.GetEndpoint();
        var permissionMetadata = endpoint?.Metadata.GetOrderedMetadata<IPermissionMetadata>()
            ?? Array.Empty<IPermissionMetadata>();

        // 路径列表是旧 API 的兼容行为。显式 Endpoint 权限声明存在时，不能被路径配置绕过。
        // 使用 StartsWithSegments 按路径段前缀匹配，避免 Contains 子串匹配导致越权放行。
        if (permissionMetadata.Count == 0 &&
            IsAllowAnonymousPath(requestPath, requirement.NormalizedAllowAnonymousPaths))
        {
            _logger.LogDebug(AuthorizationEventIds.AnonymousPath, "路径 {Path} 允许匿名访问", requestPath.Value);
            context.Succeed(requirement);
            return;
        }

        var permissionContext = new PermissionContext(
            httpContext,
            endpoint,
            queryUrl,
            httpContext.Request.Method,
            httpContext.Request.RouteValues,
            context.User,
            permissionMetadata);

        AuthorizationDecision decision;
        try
        {
            decision = await _evaluator.AuthorizeAsync(
                permissionContext,
                httpContext.RequestAborted);
        }
        catch (OperationCanceledException) when (httpContext.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                AuthorizationEventIds.EvaluatorError,
                exception,
                "权限评估器执行异常，路径 {Path}，Endpoint {Endpoint}",
                queryUrl,
                endpoint?.DisplayName ?? "Unknown");
            context.Fail();
            return;
        }

        if (!decision.IsAllowed)
        {
            _logger.LogWarning(
                AuthorizationEventIds.Denied,
                "用户 {UserName} 对路径 {Path} 没有访问权限，结果 {DecisionKind}",
                context.User.Identity?.Name ?? "Unknown",
                queryUrl,
                decision.Kind);
            context.Fail();
            return;
        }

        _logger.LogDebug(
            AuthorizationEventIds.Allowed,
            "用户 {UserName} 对路径 {Path} 授权成功",
            context.User.Identity?.Name ?? "Unknown",
            queryUrl);
        context.Succeed(requirement);
    }

    /// <summary>
    /// 判断请求路径是否落在允许匿名访问的路径段下。
    /// </summary>
    private static bool IsAllowAnonymousPath(PathString requestPath, IEnumerable<string> allowAnonymousPaths)
    {
        foreach (var configured in allowAnonymousPaths)
        {
            if (string.IsNullOrEmpty(configured))
            {
                continue;
            }

            if (requestPath.StartsWithSegments(new PathString(configured), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
