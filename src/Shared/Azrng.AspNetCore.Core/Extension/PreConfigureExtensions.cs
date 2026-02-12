using Azrng.AspNetCore.Core.PreConfigure;
using Microsoft.Extensions.DependencyInjection;

namespace Azrng.AspNetCore.Core.Extension;

/// <summary>
/// 预配置扩展方法
/// </summary>
public static class PreConfigureExtensions
{
    /// <summary>
    /// 将对象实例作为单例添加到服务容器中
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="obj">要添加的对象实例</param>
    /// <typeparam name="T">对象类型，必须为引用类型</typeparam>
    /// <returns>对象访问器，可用于修改对象</returns>
    /// <remarks>
    /// 注意：只能通过 GetObjectOrNull 方法去读取此对象
    /// </remarks>
    public static ObjectAccessor<T> AddObjectAccessor<T>(this IServiceCollection services, T obj) where T : class
    {
        return services.AddObjectAccessor(new ObjectAccessor<T>(obj));
    }

    /// <summary>
    /// 将对象访问器作为单例添加到服务容器中
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="accessor">对象访问器</param>
    /// <typeparam name="T">对象类型，必须为引用类型</typeparam>
    /// <returns>已添加的对象访问器</returns>
    /// <exception cref="Exception">如果已经注册了相同类型的对象访问器</exception>
    public static ObjectAccessor<T> AddObjectAccessor<T>(this IServiceCollection services, ObjectAccessor<T> accessor) where T : class
    {
        if (services.Any(s => s.ServiceType == typeof(ObjectAccessor<T>)))
            throw new Exception("An object accessor is registered before for type: " + typeof(T).AssemblyQualifiedName);
        services.Insert(0, ServiceDescriptor.Singleton(typeof(ObjectAccessor<T>), accessor));
        services.Insert(0, ServiceDescriptor.Singleton(typeof(IObjectAccessor<T>), accessor));

        return accessor;
    }

    /// <summary>
    /// 从服务容器中获取已注册的对象访问器的值
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <typeparam name="T">对象类型，必须为引用类型</typeparam>
    /// <returns>对象值，如果未注册则返回 null</returns>
    public static T? GetObjectOrNull<T>(this IServiceCollection services)
        where T : class
    {
        return services.GetSingletonInstanceOrNull<ObjectAccessor<T>>()?.Value;
    }

    /// <summary>
    /// 根据服务类型获取单例实例
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <typeparam name="T">服务类型</typeparam>
    /// <returns>单例实例，如果未找到则返回 null</returns>
    public static T? GetSingletonInstanceOrNull<T>(this IServiceCollection services)
    {
        var result = (T?)services
                         .FirstOrDefault(d => d.ServiceType == typeof(T))
                         ?.ImplementationInstance;
        return result;
    }

    /// <summary>
    /// 添加预配置动作，在正式配置 Options 之前执行
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="optionsAction">预配置动作</param>
    /// <typeparam name="TOptions">选项类型</typeparam>
    /// <returns>服务集合</returns>
    /// <remarks>
    /// 预配置动作会在 Options 正式配置之前按添加顺序执行
    /// 适用于需要在 Configure 之前设置默认值的场景
    /// </remarks>
    public static IServiceCollection PreConfigure<TOptions>(this IServiceCollection services,
                                                            Action<TOptions> optionsAction)
    {
        services.GetPreConfigureActions<TOptions>().Add(optionsAction);
        return services;
    }

    /// <summary>
    /// 获取或创建预配置动作列表
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <typeparam name="TOptions">选项类型</typeparam>
    /// <returns>预配置动作列表</returns>
    /// <remarks>
    /// 第一次调用时会创建新的 PreConfigureActionList 并通过 AddObjectAccessor 添加到容器中
    /// </remarks>
    public static PreConfigureActionList<TOptions> GetPreConfigureActions<TOptions>(this IServiceCollection services)
    {
        var accessor = services.GetSingletonInstanceOrNull<ObjectAccessor<PreConfigureActionList<TOptions>>>();
        var actionList = accessor?.Value;

        if (actionList == null)
        {
            actionList = new PreConfigureActionList<TOptions>();
            services.AddObjectAccessor(actionList);
        }

        return actionList;
    }
}
