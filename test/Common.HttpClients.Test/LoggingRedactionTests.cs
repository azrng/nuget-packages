using Common.HttpClients.Test.Helpers;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using Xunit;

namespace Common.HttpClients.Test
{
    public class LoggingRedactionTests
    {
        [Fact]
        public async Task Logging_Default_ShouldRedactSensitiveValues()
        {
            var logger = new ListLogger<LoggingHandler>();
            var handler = CreateLoggingClientHandler(logger, enableLogRedaction: true);

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://unit.test/login")
                                {
                                    Content = new StringContent("{\"password\":\"abc123\",\"name\":\"az\"}",
                                        Encoding.UTF8, "application/json")
                                };
            request.Headers.TryAddWithoutValidation("Authorization", "Bearer request-secret-token");

            await client.SendAsync(request);
            var logs = string.Join(Environment.NewLine, logger.Messages.ToArray());

            Assert.DoesNotContain("request-secret-token", logs);
            Assert.DoesNotContain("abc123", logs);
            Assert.DoesNotContain("resp-secret-token", logs);
            Assert.Contains("***", logs);
        }

        [Fact]
        public async Task Logging_WhenRedactionDisabled_ShouldKeepOriginalSensitiveValues()
        {
            var logger = new ListLogger<LoggingHandler>();
            var handler = CreateLoggingClientHandler(logger, enableLogRedaction: false);

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://unit.test/login")
                                {
                                    Content = new StringContent("{\"password\":\"abc123\",\"name\":\"az\"}",
                                        Encoding.UTF8, "application/json")
                                };
            request.Headers.TryAddWithoutValidation("Authorization", "Bearer request-secret-token");

            await client.SendAsync(request);
            var logs = string.Join(Environment.NewLine, logger.Messages.ToArray());

            Assert.Contains("request-secret-token", logs);
            Assert.Contains("abc123", logs);
            Assert.Contains("resp-secret-token", logs);
        }

        private static LoggingHandler CreateLoggingClientHandler(ListLogger<LoggingHandler> logger, bool enableLogRedaction)
        {
            var options = Options.Create(new HttpClientOptions
                                         {
                                             AuditLog = true,
                                             EnableLogRedaction = enableLogRedaction,
                                             MaxOutputResponseLength = 0
                                         });

            var loggingHandler = new LoggingHandler(logger, options);
            loggingHandler.InnerHandler = new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                                {
                                    Content = new StringContent(
                                        "{\"access_token\":\"resp-secret-token\",\"message\":\"ok\"}", Encoding.UTF8,
                                        "application/json")
                                }));
            return loggingHandler;
        }
    }
}
