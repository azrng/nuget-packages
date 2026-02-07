using Azrng.ConsoleApp.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace ConsoleAppDI
{
    public class ConfigurationReadService : IServiceStart
    {
        public string Title => "配置读取";

        private readonly IConfiguration _configuration;

        public ConfigurationReadService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task RunAsync()
        {
            // AddEnvironmentVariables("ASPNETCORE_") 会移除前缀，所以这里用 LLM:ApiUrl
            var baseUrl = _configuration["LLM:ApiUrl"];
            Console.WriteLine("baseUrl: " + baseUrl);
            return Task.CompletedTask;
        }
    }
}