using Azrng.Core.Extension.GuardClause;
using Azrng.Core.Helpers;
using Azrng.Core.Model;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Azrng.EFCore.AutoAudit.Config;

/// <summary>
/// 审计配置选项类，用于保存审计模块的全局配置参数。
/// </summary>
internal sealed class AuditConfigOptions
{
    /// <summary>
    /// 是否启用审计功能（默认为 true）。
    /// </summary>
    public bool AuditEnabled { get; set; } = true;

    /// <summary>
    /// 当前数据库类型
    /// </summary>
    public DatabaseType DatabaseType { get; set; } = DatabaseType.PostgresSql;

    /// <summary>
    /// 是否保存未修改的属性值。
    /// 例如：在新增操作中，记录初始值（如默认值）。
    /// </summary>
    public bool SaveUnModifiedProperties { get; set; }

    /// <summary>
    /// 用户ID提供者工厂，用于从服务容器中获取当前用户的ID。
    /// </summary>
    public Func<IServiceProvider, IUserIdProvider>? UserIdProviderFactory { get; set; }

    /// <summary>
    /// 实体过滤器集合，用于控制哪些实体需要被审计。
    /// </summary>
    private readonly IReadOnlyCollection<Func<EntityEntry, bool>> _entityFilters =
        Array.Empty<Func<EntityEntry, bool>>();

    /// <summary>
    /// 获取或设置实体过滤器集合。
    /// 用于在审核时判断哪些实体需要记录审计日志。
    /// </summary>
    public IReadOnlyCollection<Func<EntityEntry, bool>> EntityFilters
    {
        get => _entityFilters;
        init => _entityFilters = Guard.Against.Null(value);
    }

    /// <summary>
    /// 属性过滤器集合，用于控制哪些属性需要被审计。
    /// </summary>
    private readonly IReadOnlyCollection<Func<EntityEntry, PropertyEntry, bool>> _propertyFilters =
        Array.Empty<Func<EntityEntry, PropertyEntry, bool>>();

    /// <summary>
    /// 获取或设置属性过滤器集合。
    /// 用于在审核时判断哪些属性需要记录变更日志。
    /// </summary>
    public IReadOnlyCollection<Func<EntityEntry, PropertyEntry, bool>> PropertyFilters
    {
        get => _propertyFilters;
        init => _propertyFilters = Guard.Against.Null(value);
    }
}