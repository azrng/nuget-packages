using Azrng.AspNetCore.Core;
using Azrng.AspNetCore.Core.Filter;
using Azrng.AspNetCore.Core.Model;
using Azrng.Core.DependencyInjection;
using Azrng.Core.Extension;
using Azrng.Core.Helpers;
using Azrng.Core.Results;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 服务注册
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加默认控制器配置处理
    /// </summary>
    /// <param name="services"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IMvcBuilder AddDefaultControllers(this IServiceCollection services,
                                                    Action<CommonMvcConfig>? action = null)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        var config = new CommonMvcConfig();
        action?.Invoke(config);

        var mvcBuild = services.AddControllers(options =>
        {
            if (config.EnabledCustomerResultPack)
                options.Filters.Add<CustomResultPackFilter>();

            if (config.EnabledModelVerify)
                options.Filters.Add<ModelVerifyFilter>();
        });

        if (action != null)
            services.Configure<CommonMvcConfig>(action);
        if (config.EnabledModelVerify)
            services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

        return mvcBuild;
    }

    /// <summary>
    /// 添加 CORS 策略（统一配置方法）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="policyName">策略名称，默认为 "DefaultCors"</param>
    /// <param name="configure">CORS 配置</param>
    /// <returns>服务集合</returns>
    /// <remarks>
    /// 使用示例：
    /// <code>
    /// // 开发环境：允许任何来源
    /// services.AddCorsPolicy(configure: c => c.AllowAny());
    ///
    /// // 生产环境：指定来源
    /// services.AddCorsPolicy(configure: c => c
    ///     .WithOrigins("https://example.com")
    ///     .WithCredentials());
    ///
    /// // SignalR 环境
    /// services.AddCorsPolicy(configure: c => c
    ///     .WithOrigins("https://example.com")
    ///     .ForSignalR());
    /// </code>
    /// </remarks>
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services,
                                                   string policyName = "DefaultCors",
                                                   Action<CorsPolicyConfig>? configure = null)
    {
        var config = new CorsPolicyConfig();
        configure?.Invoke(config);

        services.AddCors(options =>
        {
            options.AddPolicy(policyName, builder =>
            {
                // 根据配置构建策略
                if (config.AllowAnyOrigin)
                {
                    builder.AllowAnyOrigin();
                }
                else if (config.AllowedOrigins != null && config.AllowedOrigins.Length > 0)
                {
                    builder.WithOrigins(config.AllowedOrigins);
                }
                else
                {
                    throw new InvalidOperationException("必须指定允许的来源或使用 AllowAnyOrigin()");
                }

                // 配置方法
                if (config.AllowedMethods != null && config.AllowedMethods.Length > 0)
                {
                    builder.WithMethods(config.AllowedMethods);
                }
                else
                {
                    builder.AllowAnyMethod();
                }

                // 配置头部
                if (config.AllowedHeaders != null && config.AllowedHeaders.Length > 0)
                {
                    builder.WithHeaders(config.AllowedHeaders);
                }
                else
                {
                    builder.AllowAnyHeader();
                }

                // 配置凭证
                if (config.AllowCredentials)
                {
                    builder.AllowCredentials();
                }

                // 配置暴露的响应头
                if (config.ExposedHeaders != null && config.ExposedHeaders.Length > 0)
                {
                    builder.WithExposedHeaders(config.ExposedHeaders);
                }

                // 配置预检请求缓存
                if (config.PreflightMaxAge.HasValue)
                {
                    builder.SetPreflightMaxAge(config.PreflightMaxAge.Value);
                }
            });
        });

        return services;
    }

    /// <summary>
    /// 添加允许任何来源的 CORS 策略（快捷方法）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="policyName">策略名称，默认为 "AnyCors"</param>
    /// <returns>服务集合</returns>
    /// <remarks>
    /// ⚠️ 警告: 此方法允许任何来源的请求，仅适用于开发环境。
    /// 生产环境应使用配置的 CORS 策略，限制允许的来源、方法和头部。
    /// </remarks>
    public static IServiceCollection AddAnyCors(this IServiceCollection services,
                                                string policyName = "AnyCors")
    {
        return services.AddCorsPolicy(policyName, config => config.AllowAny());
    }

    /// <summary>
    /// 显示所有注入服务的
    /// </summary>
    /// <param name="services"></param>
    /// <param name="path">路由地址</param>
    /// <returns></returns>
    public static IServiceCollection AddShowAllServices(this IServiceCollection services,
                                                        string path = "/allservices")
    {
        services.Configure<ShowServiceConfig>(config =>
        {
            config.Services = new List<ServiceDescriptor>(services);
            config.Path = path;
        });

        return services;
    }

    /// <summary>
    /// 注册 模型验证 过滤器
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddMvcModelVerifyFilter(this IServiceCollection services)
    {
        // 老方法
        // services.Configure<MvcOptions>(options =>
        // {
        //     options.Filters.Add<ModelVerifyFilter>();
        // });
        // //关闭自带的模型校验方法
        // services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

        //注册统一模型验证
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = actionContext =>
            {
                //获取验证失败的模型字段
                var errorResults = new List<ErrorInfo>();
                foreach (var (key, value) in actionContext.ModelState.Where(e => e.Value?.Errors.Count > 0))
                {
                    var errorInfo = new ErrorInfo { Field = key };
                    foreach (var error in value.Errors)
                    {
                        if (!string.IsNullOrEmpty(errorInfo.Message))
                        {
                            errorInfo.Message += '|';
                        }

                        errorInfo.Message += error.ErrorMessage;
                    }

                    errorResults.Add(errorInfo);
                }

                var result = new ResultModel(false, "参数格式不正确", StatusCodes.Status400BadRequest.ToString(), errorResults);

                return new BadRequestObjectResult(result);
            };
        });

        return services;
    }

    /// <summary>
    /// 注册 返回值包装过滤器
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="ignoreRoutePrefix">不需要包装的路由前缀</param>
    /// <returns>服务集合</returns>
    /// <remarks>在不需要包装的Action上面标注特性：NoWrapperAttribute</remarks>
    public static IServiceCollection AddMvcResultPackFilter(this IServiceCollection services,
                                                             params string[]? ignoreRoutePrefix)
    {
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add(new CustomResultPackFilter(ignoreRoutePrefix));
        });

        return services;
    }

    /// <summary>
    /// 注册 返回值包装过滤器 (已过时的方法名，请使用 AddMvcResultPackFilter)
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="ignoreRoutePrefix">不需要包装的路由前缀</param>
    /// <returns>服务集合</returns>
    [Obsolete("请使用 AddMvcResultPackFilter 代替", true)]
    public static IServiceCollection AddMvcResultPackFilterFilter(this IServiceCollection services,
                                                                  params string[]? ignoreRoutePrefix)
    {
        return services.AddMvcResultPackFilter(ignoreRoutePrefix);
    }

    #region 服务注册

    ///  <summary>
    /// 统一服务注册
    ///  </summary>
    ///  <param name="services">服务容器</param>
    ///  <param name="assembleSearchPattern">程序集匹配范围,示例(NetCoreAPI_EFCore.dll、NetCoreAPI_EFCore.*.dll)</param>
    ///  <param name="ignoreNameSpace">要忽略的命名空间,多个使用逗号隔开</param>
    ///  <returns></returns>
    public static IServiceCollection RegisterBusinessServices(this IServiceCollection services,
                                                              string assembleSearchPattern, string ignoreNameSpace = "")
    {
        // 统一注册
        var assemblies = AssemblyHelper.GetAssemblies(assembleSearchPattern);

        //不自动注册该命名空间下的接口
        var ignoreNameSpaceArray = new List<string> { "Microsoft.", "System." };
        if (ignoreNameSpace.IsNotNullOrWhiteSpace())
        {
            ignoreNameSpaceArray.AddRange(ignoreNameSpace.Split(','));
        }

        services.RegisterUniteServices(assemblies, typeof(ITransientDependency), ServiceLifetime.Transient, ignoreNameSpaceArray);
        services.RegisterUniteServices(assemblies, typeof(IScopedDependency), ServiceLifetime.Scoped, ignoreNameSpaceArray);
        services.RegisterUniteServices(assemblies, typeof(ISingletonDependency), ServiceLifetime.Singleton, ignoreNameSpaceArray);

        return services;
    }

    ///  <summary>
    /// 统一服务注册
    ///  </summary>
    ///  <param name="services">服务容器</param>
    ///  <param name="assemblies">需要注册的程序集</param>
    ///  <returns></returns>
    public static IServiceCollection RegisterBusinessServices(this IServiceCollection services,
                                                              params Assembly[] assemblies)
    {
        //不自动注册该命名空间下的接口
        var ignoreNameSpaces = new List<string> { "Microsoft.", "System." };
        services.RegisterUniteServices(assemblies, typeof(ITransientDependency), ServiceLifetime.Transient, ignoreNameSpaces);
        services.RegisterUniteServices(assemblies, typeof(IScopedDependency), ServiceLifetime.Scoped, ignoreNameSpaces);
        services.RegisterUniteServices(assemblies, typeof(ISingletonDependency), ServiceLifetime.Singleton, ignoreNameSpaces);

        return services;
    }

    ///  <summary>
    /// 统一注册服务，注册不同生命周期类型
    ///  </summary>
    ///  <param name="services"></param>
    ///  <param name="assemblies"></param>
    ///  <param name="lifeType"></param>
    ///  <param name="lifetime"></param>
    ///  <param name="ignoreNameSpaces"></param>
    ///  <returns></returns>
    private static IServiceCollection RegisterUniteServices(this IServiceCollection services,
                                                            IEnumerable<Assembly> assemblies, Type lifeType,
                                                            ServiceLifetime lifetime, List<string> ignoreNameSpaces)
    {
        var dependencyTypes = assemblies
                              .SelectMany(a => a.GetTypes().Where(t => t.IsClass && t.GetInterfaces().Contains(lifeType)))
                              .ToList();

        dependencyTypes.ForEach(implementType =>
        {
            var interfaces = implementType.GetInterfaces().ToList();
            interfaces.RemoveAll(x =>
                ignoreNameSpaces.Any(p => x.FullName is not null && x.FullName.IndexOf(p, StringComparison.Ordinal) == 0));
            if (interfaces.Count > 0)
            {
                interfaces.ForEach(serviceType =>
                    services.Add(new ServiceDescriptor(serviceType, implementType, lifetime)));
            }
            else
            {
                services.Add(new ServiceDescriptor(implementType, implementType, lifetime));
            }
        });
        return services;
    }

    #endregion
}