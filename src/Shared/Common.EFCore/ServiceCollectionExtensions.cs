using Coldairarrow.Util;
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
            new IdHelperBootstrapper().SetWorkderId(workId).Boot();
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

            // 确保IUnitOfWork和IUnitOfWork<TContext>注入的实例是同一个
            // services.AddScoped<IUnitOfWork<TContext>>(provider => new UnitOfWork<TContext>(provider.GetRequiredService<TContext>(),
            //     provider.GetRequiredService<ILogger<UnitOfWork<TContext>>>()));
            return services;
        }
    }
}