using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Profile;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Azrng.AliyunSms
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAliyunSms(this IServiceCollection services, Action<AliyunSmsConfig> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action), "调用 AliyunSms 配置时出错，未传入配置过程。");
            }

            var config = new AliyunSmsConfig();
            action.Invoke(config);
            //services.TryAddSingleton(new AliyunSmsConfig
            //{
            //    AccessKeyId = config.AccessKeyId,
            //    AccessSecret = config.AccessSecret
            //});

            //var client = new DefaultAcsClient(DefaultProfile.GetProfile("cn-hangzhou", "LTAI4GHu9vidSSiEhMMdh82w", "CcvTVRjtIcpJ5B9y1X293Mkn1UsfQB"));
            var client = new DefaultAcsClient(DefaultProfile.GetProfile("cn-hangzhou", config.AccessKeyId, config.AccessSecret));
            services.AddSingleton<DefaultAcsClient>(client);
            services.TryAddTransient<ISmsService, SmsService>();
            return services;
        }
    }
}