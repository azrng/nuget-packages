using Microsoft.Extensions.DependencyInjection;
using System;

namespace Azrng.Notification.QYWeiXinRobot
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册企业微信机器人服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <remarks>每个机器人发送的消息不能超过20条/分钟</remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static IServiceCollection AddQyWeiXinRobot(this IServiceCollection services,
            Action<QyWeiXinRobotConfig> action)
        {
            services.Configure(action);

            var config = new QyWeiXinRobotConfig();
            action.Invoke(config);
            if (string.IsNullOrWhiteSpace(config.BaseUrl))
                throw new ArgumentNullException(nameof(config.BaseUrl));
            if (string.IsNullOrWhiteSpace(config.Key))
                throw new ArgumentNullException(nameof(config.Key));

            services.AddScoped<IQyWeiXinRobotClient, QyWeiXinRobotClient>();
            services.AddHttpClient();

            return services;
        }
    }
}