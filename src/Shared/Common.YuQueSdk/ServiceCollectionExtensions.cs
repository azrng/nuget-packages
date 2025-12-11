using Common.YuQueSdk.Dto;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System;

namespace Common.YuQueSdk
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册语雀sdk服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <remarks>API来源：https://www.yuque.com/yuque/developer/api</remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static IServiceCollection AddYuQueService(this IServiceCollection services, Action<YuQueConfig> action)
        {
            services.Configure(action);
            services.AddScoped<IYuQueHelper, YuQueHelper>();
            services.AddScoped<IYuQueExtensionHelper, YuQueExtensionHelper>();

            services.AddScoped<RequestHeaderHandler>();
            services.AddRefitClient<IYuQueApi>(new RefitSettings(new NewtonsoftJsonContentSerializer()))
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://www.yuque.com/api/v2"))
                .AddHttpMessageHandler<RequestHeaderHandler>();

            return services;
        }
    }
}