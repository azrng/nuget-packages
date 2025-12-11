using Azrng.EFCore.AutoAudit.Service;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;

namespace Azrng.EFCore.AutoAudit;

/// <summary>
/// 审计配置构建器接口，用于配置自动审计功能。
/// </summary>
public interface IAuditConfigBuilder
{
    /// <summary>
    /// 获取当前服务集合，用于注册审计相关服务。
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// 配置用户ID提供者，使用一个静态的 IUserIdProvider 实例。
    /// </summary>
    /// <param name="auditUserProvider">用户ID提供者实例</param>
    /// <returns>当前配置构建器，用于链式调用。</returns>
    IAuditConfigBuilder WithUserIdProvider(IUserIdProvider auditUserProvider) => WithUserIdProvider(_ => auditUserProvider);

    /// <summary>
    /// 配置用户ID提供者，使用一个工厂方法从服务提供器中获取实例。
    /// </summary>
    /// <param name="auditUserProviderFactory">用于创建 IUserIdProvider 的工厂方法</param>
    /// <returns>当前配置构建器，用于链式调用。</returns>
    IAuditConfigBuilder WithUserIdProvider(Func<IServiceProvider, IUserIdProvider> auditUserProviderFactory);

    /// <summary>
    /// 配置是否保存未修改的属性。默认为 false。
    /// </summary>
    /// <param name="saveUnModifiedProperty">是否保存未修改的属性</param>
    /// <returns>当前配置构建器，用于链式调用。</returns>
    IAuditConfigBuilder WithUnmodifiedProperty(bool saveUnModifiedProperty = true);

    /// <summary>
    /// 配置审计存储服务（用于保存审计日志）。
    /// </summary>
    /// <param name="auditStore">审计存储服务实例</param>
    /// <returns>当前配置构建器，用于链式调用。</returns>
    IAuditConfigBuilder WithStore(IAuditStore auditStore);

    /// <summary>
    /// 配置审计存储服务，通过泛型类型自动解析。
    /// </summary>
    /// <typeparam name="TStore">实现 IAuditStore 接口的类型</typeparam>
    /// <returns>当前配置构建器，用于链式调用。</returns>
    IAuditConfigBuilder WithStore<TStore>() where TStore : class, IAuditStore;

    /// <summary>
    /// 配置实体过滤器，用于决定哪些实体需要被审计。
    /// </summary>
    /// <param name="entityFilter">用于判断实体是否需要审计的函数</param>
    /// <returns>当前配置构建器，用于链式调用。</returns>
    IAuditConfigBuilder WithEntityFilter(Func<EntityEntry, bool> entityFilter);

    /// <summary>
    /// 配置属性过滤器，用于决定哪些属性需要被审计。
    /// </summary>
    /// <param name="propertyFilter">用于判断属性是否需要审计的函数</param>
    /// <returns>当前配置构建器，用于链式调用。</returns>
    IAuditConfigBuilder WithPropertyFilter(Func<EntityEntry, PropertyEntry, bool> propertyFilter);
}