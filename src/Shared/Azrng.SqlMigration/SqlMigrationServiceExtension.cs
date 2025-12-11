using Azrng.SqlMigration.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Azrng.SqlMigration
{
    public static class SqlMigrationServiceExtension
    {
        /// <summary>
        /// 迁移名字
        /// </summary>
        public static readonly List<string> DbNames = new List<string>();

        /// <summary>
        /// 当前迁移名字
        /// </summary>
        internal static string CurrDbName = string.Empty;

        /// <summary>
        /// 添加sql迁移服务
        /// </summary>
        /// <typeparam name="T">迁移步骤回调实现类</typeparam>
        /// <param name="services"></param>
        /// <param name="action"></param>
        /// <param name="migrationDbName">迁移名字</param>
        /// <returns></returns>
        public static IServiceCollection AddSqlMigrationService<T>(this IServiceCollection services,
                                                                   string migrationDbName, Action<SqlMigrationOption> action)
            where T : class, IMigrationHandler
        {
            services.AddKeyedScoped<IMigrationHandler, T>(migrationDbName);
            return services.AddSqlMigrationService(migrationDbName, action);
        }

        /// <summary>
        /// 添加sql迁移服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="action"></param>
        /// <param name="migrationDbName">迁移名字</param>
        /// <returns></returns>
        public static IServiceCollection AddSqlMigrationService(this IServiceCollection services,
                                                                string migrationDbName,
                                                                Action<SqlMigrationOption> action)
        {
            if (string.IsNullOrWhiteSpace(migrationDbName))
            {
                throw new ArgumentException("迁移名字不能为空");
            }

            if (DbNames.Any(t => t == migrationDbName))
            {
                throw new ArgumentException("迁移名字不能重复");
            }

            services.AddOptions().Configure(migrationDbName, action);

            DbNames.Add(migrationDbName);

            var option = new SqlMigrationOption();
            action(option);
            if (option.InitVersionSetterType != null)
                services.AddKeyedScoped(typeof(IInitVersionSetter), migrationDbName, option.InitVersionSetterType);

            services.AddKeyedScoped<IDbVersionService, PgSqlDbVersionService>(migrationDbName);
            services.AddKeyedScoped<ISqlMigrationService, SqlMigrationService>(migrationDbName);

            services.AddKeyedSingleton<IDbConnection>(migrationDbName, (sp, _) => option.ConnectionBuilder(sp));

            return services;
        }

        /// <summary>
        /// sql自动迁移服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAutoMigration(this IServiceCollection services)
        {
            if (DbNames.Count == 0)
                throw new ArgumentException("请先添加迁移配置");

            services.AddTransient<IStartupFilter, SqlMigrationStartupFilter>();
            return services;
        }
    }
}