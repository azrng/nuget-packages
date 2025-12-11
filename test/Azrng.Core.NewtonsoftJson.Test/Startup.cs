using Azrng.Core.NewtonsoftJson.JsonConverters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Azrng.Core.NewtonsoftJson.Test
{
    public class Startup
    {
        public void ConfigureHost(IHostBuilder hostBuilder) { }

        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureNewtonsoftJson((options) =>
            {
                options.JsonSerializeOptions.Converters.Add(new LongToStringConverter());
            });
        }

        public void Configure() { }
    }
}