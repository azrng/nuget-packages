using Azrng.Core.Model;
using Azrng.EFCore;
using Azrng.EFCore.PostgresSql;
using Azrng.EFCore.PostgresSql.Repository;
using Coldairarrow.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// PostgreSQL数据库
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注入服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="action"></param>
        /// <param name="builder"></param>
        /// <param name="dbContextOptionBuild"></param>
        /// <returns></returns>
        public static IServiceCollection AddEntityFramework(this IServiceCollection services,
                                                            Action<EfCoreConnectOption> action,
                                                            Action<NpgsqlDbContextOptionsBuilder> builder = null,
                                                            Action<IServiceProvider, DbContextOptionsBuilder> dbContextOptionBuild =
                                                                null)
        {
            services.AddEntityFramework<BaseDbContext>(action, builder, dbContextOptionBuild);
            return services;
        }

        /// <summary>
        /// 注入服务
        /// </summary>
        /// <param name="services">自定义DbContext</param>
        /// <param name="action"></param>
        /// <param name="builder"></param>
        /// <param name="dbContextOptionBuild"></param>
        /// <remarks>该方法注入的上下文是DbContext</remarks>
        /// <returns></returns>
        public static IServiceCollection AddEntityFramework<T>(this IServiceCollection services,
                                                               Action<EfCoreConnectOption> action,
                                                               Action<NpgsqlDbContextOptionsBuilder> builder = null,
                                                               Action<IServiceProvider, DbContextOptionsBuilder> dbContextOptionBuild =
                                                                   null)
            where T : DbContext
        {
            var config = new EfCoreConnectOption();
            action.Invoke(config);

            config.ParamVerify();

            EfCoreGlobalConfig.SetConfig(DatabaseType.PostgresSql, config.UseOldUpdateColumn, config.Schema);

            services.AddDbContext<T>((provider, options) =>
            {
                options.UseNpgsql(config.ConnectionString, builder)
                       .UseLoggerFactory(LoggerFactory.Create(configure =>
                       {
                           configure.AddFilter((category, level) =>
                                        category == DbLoggerCategory.Database.Command.Name &&
                                        level == LogLevel.Information)
                                    .AddConsole();
                       }));
                if (config.IsSnakeCaseNaming)
                    options.UseSnakeCaseNamingConvention();

                dbContextOptionBuild?.Invoke(provider, options);
            });

            new IdHelperBootstrapper().SetWorkderId(config.WorkId).Boot();

            services.AddScoped<DbContext, T>();
            services.AddScoped(typeof(IBaseRepository<>), typeof(PostgreRepository<>));
            services.AddScoped(typeof(IBaseRepository<,>), typeof(PostgreRepository<,>));
            services.AddScoped<IUnitOfWork, UnitOfWork<DbContext>>();

            return services;
        }

        /// <summary>
        /// 注入服务
        /// </summary>
        /// <param name="services">自定义DbContext</param>
        /// <param name="action"></param>
        /// <param name="builder"></param>
        /// <param name="dbContextOptionBuild"></param>
        /// <remarks>该方法注入的上下文是DbContext</remarks>
        /// <returns></returns>
        public static IServiceCollection AddEntityFrameworkFactory<T>(this IServiceCollection services,
                                                                      Action<EfCoreConnectOption> action,
                                                                      Action<NpgsqlDbContextOptionsBuilder> builder = null,
                                                                      Action<IServiceProvider, DbContextOptionsBuilder>
                                                                          dbContextOptionBuild =
                                                                          null)
            where T : DbContext
        {
            var config = new EfCoreConnectOption();
            action.Invoke(config);

            config.ParamVerify();

            EfCoreGlobalConfig.SetConfig(DatabaseType.PostgresSql, config.UseOldUpdateColumn, config.Schema);

            services.AddDbContextFactory<T>((provider, options) =>
            {
                options.UseNpgsql(config.ConnectionString, builder)
                       .UseLoggerFactory(LoggerFactory.Create(configure =>
                       {
                           configure.AddFilter((category, level) =>
                                        category == DbLoggerCategory.Database.Command.Name &&
                                        level == LogLevel.Information)
                                    .AddConsole();
                       }));
                if (config.IsSnakeCaseNaming)
                    options.UseSnakeCaseNamingConvention();

                dbContextOptionBuild?.Invoke(provider, options);
            });

            new IdHelperBootstrapper().SetWorkderId(config.WorkId).Boot();

            services.AddScoped<DbContext, T>();
            services.AddScoped(typeof(IBaseRepository<>), typeof(PostgreRepository<>));
            services.AddScoped(typeof(IBaseRepository<,>), typeof(PostgreRepository<,>));
            services.AddScoped<IUnitOfWork, UnitOfWork<DbContext>>();

            return services;
        }
    }
}