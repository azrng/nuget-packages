using Microsoft.Extensions.DependencyInjection;

namespace Azrng.AspNetCore.Inject.Attributes;

/// <summary>
/// 依赖注入标记
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class InjectOnAttribute : Attribute
{
    /// <summary>
    /// 要注入的服务
    /// </summary>
    public Type[]? ServicesType { get; set; }

    /// <summary>
    /// 生命周期
    /// </summary>
    public ServiceLifetime Lifetime { get; set; }

    /// <summary>
    /// 注入模式
    /// </summary>
    public InjectScheme Scheme { get; set; }

    /// <summary>
    /// 是否注入自己
    /// </summary>
    public bool Own { get; set; } = false;

    public InjectOnAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient,
        InjectScheme scheme = InjectScheme.OnlyInterfaces)
    {
        Lifetime = lifetime;
        Scheme = scheme;
    }
}