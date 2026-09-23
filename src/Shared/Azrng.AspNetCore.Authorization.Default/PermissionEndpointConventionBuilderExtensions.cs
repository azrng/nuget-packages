using Azrng.AspNetCore.Authorization.Default;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Endpoint 权限声明扩展。
/// </summary>
public static class PermissionEndpointConventionBuilderExtensions
{
    /// <summary>
    /// 为 Endpoint 添加默认权限策略和权限元数据。
    /// </summary>
    /// <typeparam name="TBuilder">Endpoint 构建器类型。</typeparam>
    /// <param name="builder">Endpoint 构建器。</param>
    /// <param name="permissions">权限码。</param>
    /// <returns>原 Endpoint 构建器。</returns>
    public static TBuilder RequirePermission<TBuilder>(
        this TBuilder builder,
        params string[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequirePermission(PermissionMatchMode.All, permissions);
    }

    /// <summary>
    /// 为 Endpoint 添加默认权限策略和权限元数据。
    /// </summary>
    /// <typeparam name="TBuilder">Endpoint 构建器类型。</typeparam>
    /// <param name="builder">Endpoint 构建器。</param>
    /// <param name="matchMode">多个权限码的匹配方式。</param>
    /// <param name="permissions">权限码。</param>
    /// <returns>原 Endpoint 构建器。</returns>
    public static TBuilder RequirePermission<TBuilder>(
        this TBuilder builder,
        PermissionMatchMode matchMode,
        params string[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        var metadata = new PermissionMetadata(matchMode, permissions);
        builder.Add(endpointBuilder =>
        {
            endpointBuilder.Metadata.Add(metadata);
            endpointBuilder.Metadata.Add(new AuthorizeAttribute(
                Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.DefaultPolicyName));
        });

        return builder;
    }
}
