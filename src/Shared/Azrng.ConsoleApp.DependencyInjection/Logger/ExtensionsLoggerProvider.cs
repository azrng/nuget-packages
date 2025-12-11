using Microsoft.Extensions.Logging;

namespace Azrng.ConsoleApp.DependencyInjection.Logger
{
    public class ExtensionsLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new ExtensionsLogger(categoryName);
        }

        public void Dispose() { }
    }
}