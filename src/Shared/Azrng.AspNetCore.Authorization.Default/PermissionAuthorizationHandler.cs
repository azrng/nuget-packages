using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 基于 Endpoint 元数据的权限授权处理器，由授权中间件调用，早于业务代码执行。
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

    /// <summary>
    /// 授权服务失败后不短路后续 handler（InvokeHandlersAfterFailure 默认 true），
    /// 匿名请求仍会进入本方法，故在此短路，避免匿名流量触发评估器。
    /// </summary>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionAuthorizationRequirement requirement)
    {
        // 任意一个 identity 已认证即视为已认证，与框架 DenyAnonymousAuthorizationRequirement 判定一致
        if (!context.User.Identities.Any(static identity => identity.IsAuthenticated))
        {
            _logger.LogWarning(
                AuthorizationEventIds.Unauthenticated,
                "未认证用户访问路径 {Path}，跳过权限评估直接拒绝",
                _accessor.HttpContext?.Request.Path.Value ?? "Unknown");
            context.Fail();
            return;
        }

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
