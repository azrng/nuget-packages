using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 基于 Endpoint 元数据的权限授权处理器。
/// </summary>
internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionAuthorizationRequirement>
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
        PermissionAuthorizationRequirement requirement)
    {
        var httpContext = _accessor.HttpContext;
        if (httpContext == null)
        {
            _logger.LogWarning("HTTP 上下文为空");
            context.Fail();
            return;
        }

        var queryUrl = httpContext.Request.Path.Value?.ToLowerInvariant();
        if (string.IsNullOrEmpty(queryUrl))
        {
            _logger.LogWarning(AuthorizationEventIds.EmptyPath, "请求路径为空");
            context.Fail();
            return;
        }

        var endpoint = httpContext.GetEndpoint();
        var permissionMetadata = endpoint?.Metadata.GetOrderedMetadata<IPermissionMetadata>()
            ?? Array.Empty<IPermissionMetadata>();
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
}
