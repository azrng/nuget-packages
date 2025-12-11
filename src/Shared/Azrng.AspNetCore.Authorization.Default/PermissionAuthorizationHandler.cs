using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 自定义权限授权需求处理器
/// </summary>
internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>, IAuthorizationRequirement
{
    /// <summary>
    /// 验证方案提供对象
    /// </summary>
    private readonly IAuthenticationSchemeProvider _schemes;

    private readonly IHttpContextAccessor _accessor;

    public PermissionAuthorizationHandler(IAuthenticationSchemeProvider schemes,
        IHttpContextAccessor httpContextAccessor)
    {
        _schemes = schemes;
        _accessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var httpContext = _accessor.HttpContext;
        //请求Url
        if (httpContext == null)
            return;

        //请求的url
        var queryUrl = httpContext.Request.Path.Value?.ToLower();
        if (string.IsNullOrEmpty(queryUrl))
        {
            context.Fail();
            return;
        }

        //如果访问的是无需授权的直接通过
        if (requirement.LoginVisitAction.Any(t => queryUrl.Contains(t.ToLowerInvariant())))
        {
            context.Succeed(requirement);
            return;
        }

        #region 验证登录

        //检验是否经过登录认证
        if (context.User.Identity?.IsAuthenticated != true)
        {
            context.Fail();
            return;
        }

        //判断请求是否停止
        var handlers = httpContext.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
        foreach (var scheme in await _schemes.GetRequestHandlerSchemesAsync())
        {
            if (await handlers.GetHandlerAsync(httpContext, scheme.Name) is IAuthenticationRequestHandler
                    handler && await handler.HandleRequestAsync())
            {
                context.Fail();
                return;
            }
        }

        //判断请求是否拥有凭据，即有没有登录
        var defaultAuthenticate = await _schemes.GetDefaultAuthenticateSchemeAsync();
        if (defaultAuthenticate == null)
        {
            context.Fail();
            return;
        }

        var result = await httpContext.AuthenticateAsync(defaultAuthenticate.Name);
        //result?.Principal不为空即登录成功
        if (result.Principal == null)
        {
            context.Fail();
            return;
        }

        #endregion

        var permissionVerifyService = httpContext.RequestServices.GetRequiredService<IPermissionVerifyService>();
        //验证权限
        var verify = await permissionVerifyService.HasPermission(queryUrl);
        if (!verify)
        {
            context.Fail();
            return;
        }

        context.Succeed(requirement);
    }
}