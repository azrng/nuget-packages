using Azrng.Notification.QYWeiXinRobot;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common.Notification.QYWeiXinRobot.Tests
{
    public class Startup
    {
        public void ConfigureHost(IHostBuilder hostBuilder)
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddQyWeiXinRobot(x => x.Key = "8a20eea4-f8a0-4673-8b42-38da812ee55d");
        }
    }
}