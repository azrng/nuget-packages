using Azrng.Dapper.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Azrng.Dapper
{
    /// <summary>
    /// 服务注册扩展
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注入 dapper 服务
        /// </summary>
        public static IServiceCollection AddDapper(this IServiceCollection services,
                                                   Action<DapperRepositoryOptions>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(services);

            var options = new DapperRepositoryOptions();
            configure?.Invoke(options);

            services.AddSingleton(options);
            services.AddScoped<IDapperRepository, DapperRepository>();

            return services;
        }
    }
}
