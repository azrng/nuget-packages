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

        // 请求的 URL（使用 InvariantCulture 进行大小写不敏感比较）
        var queryUrl = httpContext.Request.Path.Value?.ToLowerInvariant();
        if (string.IsNullOrEmpty(queryUrl))
        {
            _logger.LogWarning("请求路径为空");
            context.Fail();
            return;
        }

        // 如果访问的是允许匿名访问的路径，直接通过
        if (requirement.AllowAnonymousPaths.Any(t => queryUrl.Contains(t.ToLowerInvariant())))
        {
            _logger.LogDebug("路径 {Path} 允许匿名访问", queryUrl);
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
        if (result.Principal == null)
        {
            _logger.LogWarning("认证失败");
            context.Fail();
            return;
        }

        // 验证用户权限
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
}