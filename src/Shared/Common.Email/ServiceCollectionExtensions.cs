using Microsoft.Extensions.DependencyInjection;
using System;

namespace Common.Email
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 发送邮件服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static IServiceCollection AddEmail(this IServiceCollection services, Action<EmailConfig> func)
        {
            services.AddTransient<IEmailHelper, EmailHelper>();
            services.Configure<EmailConfig>(func);

            return services;
        }
    }
}