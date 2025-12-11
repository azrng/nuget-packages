using Azrng.ConsoleApp.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConsoleAppDI
{
    public class TempService : IServiceStart
    {
        private readonly ILogger<TempService> _logger;

        public TempService(ILogger<TempService> logger)
        {
            _logger = logger;
        }

        public string Title => "临时测试";

        public Task RunAsync()
        {
            _logger.LogDebug("debug");
            _logger.LogInformation("info");
            _logger.LogError("error");
            _logger.LogWarning("warn");

            return Task.CompletedTask;
        }
    }
}