using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Azrng.Core.NewtonsoftJson
{
    /// <summary>
    /// json序列化扩展
    /// </summary>
    public static class JsonSerializerExtension
    {
        /// <summary>
        /// 配置NewtonsoftJson序列化
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureOptions"></param>
        public static void ConfigureNewtonsoftJson(this IServiceCollection services,
                                                   Action<JsonNetSerializerOptions>? configureOptions = null)
        {
            services.AddScoped<IJsonSerializer, NewtonsoftJsonSerializer>();
            if (configureOptions is not null)
                services.AddOptions<JsonNetSerializerOptions>().Configure(configureOptions);
            else
                services.AddOptions<JsonNetSerializerOptions>();
        }

        /// <summary>
        /// 校验字符串是否是json字符串（通过try catch实现）
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static bool IsJsonString(this string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
                return false;
            try
            {
                JsonConvert.DeserializeObject(jsonString);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}