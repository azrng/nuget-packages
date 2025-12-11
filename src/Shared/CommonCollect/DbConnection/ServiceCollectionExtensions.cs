using CommonCollect.DbConnection.MySql;
using CommonCollect.DbConnection.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CommonCollect.DbConnection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 可以将该操作类完善，支持多个数据库
        /// </summary>
        /// <param name="services"></param>
        /// <param name="optionsBuilder"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddMySqlServices(this IServiceCollection services, Action<MySqlOptions> optionsBuilder)
        {
            if (optionsBuilder == null)
            {
                throw new ArgumentNullException("MySqlOptions");
            }
            services.Configure(optionsBuilder);
            services.AddSingleton<ISqlConnectionHelper, MySqlConnectionHelper>();
        }
    }
}