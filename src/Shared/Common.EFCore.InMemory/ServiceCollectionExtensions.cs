using Azrng.Core.Model;
using Azrng.EFCore;
using Azrng.EFCore.InMemory;
using Azrng.EFCore.InMemory.Repository;
using Coldairarrow.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 内存数据库
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注入服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="builder"></param>
        /// <param name="dataBaseName">数据库名字</param>
        /// <param name="workId">节点Id</param>
        /// <returns></returns>
        public static IServiceCollection AddEntityFramework(this IServiceCollection services, string dataBaseName = "db", int workId = 1,
                                                            Action<InMemoryDbContextOptionsBuilder> builder = null)
        {
            services.AddEntityFramework<BaseDbContext>(dataBaseName, workId, builder);
            return services;
        }

        /// <summary>
        /// 注入服务
        /// </summary>
        /// <param name="services">自定义DbContext</param>
        /// <param name="builder"></param>
        /// <param name="dataBaseName">数据库名字</param>
        /// <param name="workId">节点ID</param>
        /// <returns></returns>
        public static IServiceCollection AddEntityFramework<T>(this IServiceCollection services, string dataBaseName = "db", int workId = 1,
                                                               Action<InMemoryDbContextOptionsBuilder> builder = null) where T : DbContext
        {
            EfCoreGlobalConfig.SetConfig(DatabaseType.InMemory, false);
            services.AddDbContext<T>(option => option.UseInMemoryDatabase(dataBaseName, builder));

            new IdHelperBootstrapper().SetWorkderId(workId).Boot();

            services.AddScoped<DbContext, T>();
            services.AddScoped(typeof(IBaseRepository<>), typeof(InMemoryRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork<DbContext>>();

            return services;
        }

        /// <summary>
        /// 注入服务
        /// </summary>
        /// <param name="services">自定义DbContext</param>
        /// <param name="builder"></param>
        /// <param name="dataBaseName">数据库名字</param>
        /// <param name="workId">节点ID</param>
        /// <returns></returns>
        public static IServiceCollection AddEntityFrameworkFactory<T>(this IServiceCollection services, string dataBaseName = "db",
                                                                      int workId = 1,
                                                                      Action<InMemoryDbContextOptionsBuilder> builder = null)
            where T : DbContext
        {
            EfCoreGlobalConfig.SetConfig(DatabaseType.InMemory, false);
            services.AddDbContextFactory<T>(option => option.UseInMemoryDatabase(dataBaseName, builder));

            new IdHelperBootstrapper().SetWorkderId(workId).Boot();

            services.AddScoped<DbContext, T>();
            services.AddScoped(typeof(IBaseRepository<>), typeof(InMemoryRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork<DbContext>>();

            return services;
        }
    }
}