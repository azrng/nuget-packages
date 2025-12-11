using Azrng.Core.Json;
using Azrng.Core.Json.JsonConverters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json.Serialization;

namespace Azrng.Core.DefaultJson.Test
{
    /// <summary>
    /// 序列化配置参考：https://mp.weixin.qq.com/s/3M6bhK_BvXhcBBd2yOZK8A
    /// </summary>
    public class Startup
    {
        public void ConfigureHost(IHostBuilder hostBuilder) { }

        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureDefaultJson(options =>
            {
                // 将null值给忽略掉
                options.JsonSerializeOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

                // 允许从字符串读取数字
                options.JsonDeserializeOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;


                options.JsonSerializeOptions.Converters.Add(new LongToStringConverter());
            });
        }

        public void Configure() { }
    }
}