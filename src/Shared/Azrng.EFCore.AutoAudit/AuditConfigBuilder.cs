using Azrng.EFCore.AutoAudit.Config;
using Azrng.EFCore.AutoAudit.Service;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;

namespace Azrng.EFCore.AutoAudit;

/// <summary>
/// 审计配置构建器，用于配置审计行为和存储选项。
/// </summary>
internal class AuditConfigBuilder : IAuditConfigBuilder
{
    /// <summary>
    /// 默认的用户ID提供者工厂，用于从服务容器中获取或使用默认实现。
    /// </summary>
    private Func<IServiceProvider, IUserIdProvider>? _auditUserProviderFactory =
        sp =>
        {
            var userIdProvider = sp.GetService<IUserIdProvider>();
            return userIdProvider ?? EnvironmentUserIdProvider.Instance;
        };

    /// <summary>
    /// 实体过滤器列表，用于决定哪些实体需要被审计。
    /// </summary>
    private readonly List<Func<EntityEntry, bool>> _entityFilters = new();

    /// <summary>
    /// 属性过滤器列表，用于决定哪些属性需要被审计。
    /// </summary>
    private readonly List<Func<EntityEntry, PropertyEntry, bool>> _propertyFilters = new();

    /// <summary>
    /// 是否保存未修改的属性。
    /// </summary>
    private bool _saveUnModifiedProperty;

    /// <summary>
    /// 构造函数，初始化服务集合。
    /// </summary>
    /// <param name="services">服务集合</param>
    public AuditConfigBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// 获取当前服务集合。
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// 设置用户ID提供者工厂，用于自定义获取当前用户ID的方式。
    /// </summary>
    /// <param name="auditUserProviderFactory">提供用户ID的函数</param>
    /// <returns>当前配置构建器，支持链式调用。</returns>
    public IAuditConfigBuilder WithUserIdProvider(Func<IServiceProvider, IUserIdProvider> auditUserProviderFactory)
    {
        _auditUserProviderFactory = auditUserProviderFactory;
        return this;
    }

    /// <summary>
    /// 设置是否保存未修改的属性。
    /// </summary>
    /// <param name="saveUnModifiedProperty">是否保存未修改的属性</param>
    /// <returns>当前配置构建器，支持链式调用。</returns>
    public IAuditConfigBuilder WithUnmodifiedProperty(bool saveUnModifiedProperty = true)
    {
        _saveUnModifiedProperty = saveUnModifiedProperty;
        return this;
    }

    /// <summary>
    /// 设置审计存储服务，使用指定的实现。
    /// </summary>
    /// <param name="auditStore">审计存储实例</param>
    /// <returns>当前配置构建器，支持链式调用。</returns>
    public IAuditConfigBuilder WithStore(IAuditStore auditStore)
    {
        ArgumentNullException.ThrowIfNull(auditStore);

        Services.AddSingleton<IAuditStore>(auditStore);
        return this;
    }

    /// <summary>
    /// 设置审计存储服务，使用指定类型的服务注册。
    /// </summary>
    /// <typeparam name="TStore">实现 IAuditStore 的类型</typeparam>
    /// <returns>当前配置构建器，支持链式调用。</returns>
    public IAuditConfigBuilder WithStore<TStore>() where TStore : class, IAuditStore
    {
        Services.AddScoped<IAuditStore, TStore>();
        return this;
    }

    /// <summary>
    /// 添加实体过滤器，用于控制哪些实体需要被审计。
    /// </summary>
    /// <param name="entityFilter">判断实体是否需要被审计的函数</param>
    /// <returns>当前配置构建器，支持链式调用。</returns>
    public IAuditConfigBuilder WithEntityFilter(Func<EntityEntry, bool> entityFilter)
    {
        ArgumentNullException.ThrowIfNull(entityFilter);
        _entityFilters.Add(entityFilter);
        return this;
    }

    /// <summary>
    /// 添加属性过滤器，用于控制哪些属性需要被审计。
    /// </summary>
    /// <param name="propertyFilter">判断属性是否需要被审计的函数</param>
    /// <returns>当前配置构建器，支持链式调用。</returns>
    public IAuditConfigBuilder WithPropertyFilter(Func<EntityEntry, PropertyEntry, bool> propertyFilter)
    {
        ArgumentNullException.ThrowIfNull(propertyFilter);
        _propertyFilters.Add(propertyFilter);
        return this;
    }

    /// <summary>
    /// 构建最终的审计配置对象。
    /// </summary>
    /// <returns>包含所有配置选项的 AuditConfigOptions 实例</returns>
    public AuditConfigOptions Build()
    {
        return new AuditConfigOptions
               {
                   EntityFilters = _entityFilters,
                   PropertyFilters = _propertyFilters,
                   SaveUnModifiedProperties = _saveUnModifiedProperty,
                   UserIdProviderFactory = _auditUserProviderFactory
               };
    }
}