using Azrng.Core.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Azrng.EFCore
{
    /// <summary>
    /// 本组件参考自：yrjw.ORM.Chimp  非常感谢
    /// 本dll仅做个人学习使用
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注入服务添加ID生成类
        /// </summary>
        /// <param name="services"></param>
        /// <param name="workId"></param>
        /// <returns></returns>
        public static IServiceCollection AddIdHelper(this IServiceCollection services, int workId = 1)
        {
            Snowflake.ConfigureWorkerId(workId);
            return services;
        }

        /// <summary>
        /// 添加工作单元
        /// </summary>
        /// <param name="services"></param>
        /// <typeparam name="TContext"></typeparam>
        /// <remarks>该方法注入的上下文是TContext</remarks>
        /// <returns></returns>
        public static IServiceCollection AddUnitOfWork<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            services.AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();

            // 转发注册，确保 IUnitOfWork 与 IUnitOfWork<TContext> 在同一 Scope 内解析到同一实例
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IUnitOfWork<TContext>>());
            return services;
        }
    }
}
