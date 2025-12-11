using Azrng.DistributeLock.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Azrng.DistributeLock.PostgreSql
{
    /// <summary>
    /// 分布式锁扩展
    /// </summary>
    public static class LockProviderExtension
    {
        /// <summary>
        ///添加数据库分布式锁服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="schema">分布式锁表空间</param>
        /// <param name="table">分布式锁表名称</param>
        /// <returns></returns>
        public static IServiceCollection AddDbLockProvider(this IServiceCollection services, string connectionString,
            string schema = "public",
            string table = "distribute_lock")
        {
            services.AddSingleton<ILockProvider, DbLockProvider>();
            services.AddOptions().Configure<DbLockOptions>(x =>
            {
                x.ConnectionString = connectionString;
                x.Schema = schema;
                x.Table = table;
            });
            return services;
        }
    }
}