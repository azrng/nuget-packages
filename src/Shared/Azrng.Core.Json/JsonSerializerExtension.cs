using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Azrng.Core.Json
{
    /// <summary>
    /// json序列化扩展
    /// </summary>
    public static class JsonSerializerExtension
    {
        /// <summary>
        /// 配置System.Text.Json序列化
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureOptions"></param>
        public static void ConfigureDefaultJson(this IServiceCollection services,
                                                Action<DefaultJsonSerializerOptions>? configureOptions = null)
        {
            services.AddScoped<IJsonSerializer, SysTextJsonSerializer>();
            if (configureOptions is not null)
                services.AddOptions<DefaultJsonSerializerOptions>().Configure(configureOptions);
            else
            {
                services.AddOptions<DefaultJsonSerializerOptions>();
            }
        }
    }
}