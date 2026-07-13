using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 基于路径的权限授权处理器
/// </summary>
/// <remarks>
/// 此处理器实现了以下功能：
/// 1. 检查用户是否已认证
/// 2. 检查请求路径是否在允许匿名访问的列表中
/// 3. 通过 IPermissionVerifyService 验证用户是否有访问权限
/// </remarks>
internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IAuthenticationSchemeProvider _schemes;
    private readonly IHttpContextAccessor _accessor;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    /// <summary>
    /// 初始化 <see cref="PermissionAuthorizationHandler"/> 的新实例
    /// </summary>
    /// <param name="schemes">认证方案提供器</param>
    /// <param name="httpContextAccessor">HTTP 上下文访问器</param>
    /// <param name="logger">日志记录器</param>
    public PermissionAuthorizationHandler(
        IAuthenticationSchemeProvider schemes,
        IHttpContextAccessor httpContextAccessor,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _schemes = schemes;
        _accessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var httpContext = _accessor.HttpContext;
        if (httpContext == null)
        {
            _logger.LogWarning("HTTP 上下文为空");
            context.Fail();
            return;
        }

        var requestPath = httpContext.Request.Path;

        // 如果访问的是允许匿名访问的路径，直接通过
        // 使用 StartsWithSegments 按路径段前缀匹配，避免 Contains 子串匹配导致越权放行
        // 例如配置 "/api/login" 时，"/api/login" 与 "/api/login/callback" 命中，
        // 但 "/admin/api/login/delete"、"/api/login-export" 这类仅含子串的路径不会被放行
        if (IsAllowAnonymousPath(requestPath, requirement.AllowAnonymousPaths))
        {
            _logger.LogDebug("路径 {Path} 允许匿名访问", requestPath.Value);
            context.Succeed(requirement);
            return;
        }

        // 验证用户是否已登录
        if (context.User.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning("用户未认证");
            context.Fail();
            return;
        }

        // 验证认证方案
        var defaultAuthenticate = await _schemes.GetDefaultAuthenticateSchemeAsync();
        if (defaultAuthenticate == null)
        {
            _logger.LogWarning("未找到默认认证方案");
            context.Fail();
            return;
        }

        var result = await httpContext.AuthenticateAsync(defaultAuthenticate.Name);
        if (result?.Succeeded != true)
        {
            _logger.LogWarning("认证失败");
            context.Fail();
            return;
        }

        // 验证用户权限
        // 传给权限验证服务的路径保持小写约定（接口契约要求传入小写路径）
        var queryUrl = requestPath.Value?.ToLowerInvariant();
        if (string.IsNullOrEmpty(queryUrl))
        {
            _logger.LogWarning("请求路径为空");
            context.Fail();
            return;
        }

        var permissionVerifyService = httpContext.RequestServices.GetRequiredService<IPermissionVerifyService>();
        var hasPermission = await permissionVerifyService.HasPermission(queryUrl);
        if (!hasPermission)
        {
            _logger.LogWarning("用户 {UserName} 对路径 {Path} 没有访问权限",
                context.User.Identity?.Name ?? "Unknown", queryUrl);
            context.Fail();
            return;
        }

        _logger.LogDebug("用户 {UserName} 对路径 {Path} 授权成功",
            context.User.Identity?.Name ?? "Unknown", queryUrl);
        context.Succeed(requirement);
    }

    /// <summary>
    /// 判断请求路径是否落在允许匿名访问的路径段下
    /// </summary>
    /// <param name="requestPath">当前请求路径</param>
    /// <param name="allowAnonymousPaths">允许匿名访问的路径集合</param>
    /// <returns>命中返回 true，否则返回 false</returns>
    /// <remarks>
    /// 使用 <see cref="PathString.StartsWithSegments(PathString, StringComparison)"/> 进行路径段边界匹配，
    /// 而非 <c>string.Contains</c>，避免子串命中导致越权放行
    /// </remarks>
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