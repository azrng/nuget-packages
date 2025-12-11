using Azrng.AspNetCore.Core.PreConfigure;
using Microsoft.Extensions.DependencyInjection;

namespace Azrng.AspNetCore.Core.Extension;

/// <summary>
/// 预配置扩展
/// </summary>
public static class PreConfigureExtensions
{
    /// <summary>
    /// 将单实例新增到容器
    /// </summary>
    /// <param name="services"></param>
    /// <param name="obj"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <remarks>只能通过GetObjectOrNull方式去读取</remarks>
    public static ObjectAccessor<T> AddObjectAccessor<T>(this IServiceCollection services, T obj) where T : class
    {
        return services.AddObjectAccessor(new ObjectAccessor<T>(obj));
    }

    /// <summary>
    /// 将单实例新增到容器
    /// </summary>
    /// <param name="services"></param>
    /// <param name="accessor"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static ObjectAccessor<T> AddObjectAccessor<T>(this IServiceCollection services, ObjectAccessor<T> accessor) where T : class
    {
        if (services.Any(s => s.ServiceType == typeof(ObjectAccessor<T>)))
            throw new Exception("An object accessor is registered before for type: " + typeof(T).AssemblyQualifiedName);
        services.Insert(0, ServiceDescriptor.Singleton(typeof(ObjectAccessor<T>), accessor));
        services.Insert(0, ServiceDescriptor.Singleton(typeof(IObjectAccessor<T>), accessor));

        //services.AddSingleton<IConfigureOptions<T>>((IConfigureOptions<T>) new ConfigureNamedOptions<T>(string.Empty,option=> accessor.Value));
        return accessor;
    }

    /// <summary>
    /// 通过  ServiceType 去找ImplementationInstance获取对象
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? GetObjectOrNull<T>(this IServiceCollection services)
        where T : class
    {
        return services.GetSingletonInstanceOrNull<IObjectAccessor<T>>()?.Value;
    }

    /// <summary>
    /// 根据服务类型获取单个实例
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? GetSingletonInstanceOrNull<T>(this IServiceCollection services)
    {
        var result = (T?)services
                         .FirstOrDefault(d => d.ServiceType == typeof(T))
                         ?.ImplementationInstance;
        return result;
    }

    /// <summary>
    /// 配置options的时候，通过PreConfigure包一层
    /// </summary>
    /// <param name="services"></param>
    /// <param name="optionsAction"></param>
    /// <typeparam name="TOptions"></typeparam>
    /// <returns></returns>
    public static IServiceCollection PreConfigure<TOptions>(this IServiceCollection services,
                                                            Action<TOptions> optionsAction)
    {
        services.GetPreConfigureActions<TOptions>().Add(optionsAction);
        return services;
    }

    /// <summary>
    /// 第一次加入，和获取的时候，直接塞入一个IObjectAccessor<PreConfigureActionList<TOptions>单实例到对象访问器中
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="TOptions"></typeparam>
    /// <returns></returns>
    public static PreConfigureActionList<TOptions> GetPreConfigureActions<TOptions>(this IServiceCollection services)
    {
        var actionList = services.GetSingletonInstanceOrNull<IObjectAccessor<PreConfigureActionList<TOptions>>>()
                                 ?.Value;
        if (actionList == null)
        {
            actionList = new PreConfigureActionList<TOptions>();
            services.AddObjectAccessor(actionList);
        }

        return actionList;
    }
}