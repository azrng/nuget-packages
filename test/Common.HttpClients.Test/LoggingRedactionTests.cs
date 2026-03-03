using Common.HttpClients.Test.Helpers;
using Microsoft.AspNetCore.Http;
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

        [Fact]
        public async Task Logging_WithSkipHeader_ShouldNotWriteAuditLog()
        {
            var logger = new ListLogger<LoggingHandler>();
            var handler = CreateLoggingClientHandler(logger, enableLogRedaction: true);

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/skip");
            request.Headers.TryAddWithoutValidation("X-Skip-Logger", "1");

            await client.SendAsync(request);

            Assert.Empty(logger.Messages);
        }

        [Fact]
        public async Task Logging_WhenAuditLogDisabled_ShouldNotWriteLog()
        {
            var logger = new ListLogger<LoggingHandler>();
            var handler = CreateLoggingClientHandler(logger, enableLogRedaction: true, auditLog: false);

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/disabled");

            await client.SendAsync(request);

            Assert.Empty(logger.Messages);
        }

        [Fact]
        public async Task Logging_ShouldTruncateResponseContent_WhenExceedMaxLength()
        {
            var logger = new ListLogger<LoggingHandler>();
            var handler = CreateLoggingClientHandler(logger, enableLogRedaction: true, maxOutputLength: 5,
                responseBody: "1234567890");

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/long-response");

            await client.SendAsync(request);
            var logs = string.Join(Environment.NewLine, logger.Messages.ToArray());

            Assert.Contains("12345", logs);
            Assert.DoesNotContain("1234567890", logs);
            Assert.Contains("...", logs);
        }

        [Fact]
        public async Task Logging_ShouldAddTraceId_FromHttpContext_WhenMissingInRequest()
        {
            var logger = new ListLogger<LoggingHandler>();
            var accessor = new HttpContextAccessor
                           {
                               HttpContext = new DefaultHttpContext
                                             {
                                                 TraceIdentifier = "ctx-trace-id"
                                             }
                           };

            string? requestTraceId = null;
            var handler = CreateLoggingClientHandler(logger, enableLogRedaction: true, accessor: accessor, onRequest: request =>
            {
                requestTraceId = request.Headers.GetValues("X-Trace-Id").Single();
            });

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/trace");

            await client.SendAsync(request);

            Assert.Equal("ctx-trace-id", requestTraceId);
        }

        [Fact]
        public async Task Logging_ShouldNotReadBinaryResponseContent()
        {
            var logger = new ListLogger<LoggingHandler>();
            var handler = CreateLoggingClientHandler(logger, enableLogRedaction: true, responseBody: "binary-secret",
                responseContentType: "image/jpeg");

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/binary");

            await client.SendAsync(request);
            var logs = string.Join(Environment.NewLine, logger.Messages.ToArray());

            Assert.DoesNotContain("binary-secret", logs);
        }

        [Fact]
        public async Task Logging_ShouldRedact_CustomSensitiveField()
        {
            var logger = new ListLogger<LoggingHandler>();
            var handler = CreateLoggingClientHandler(logger, enableLogRedaction: true,
                additionalSensitiveFields: new[] { "mobile" });

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://unit.test/custom-redaction")
                                {
                                    Content = new StringContent("{\"mobile\":\"13800000000\",\"name\":\"az\"}",
                                        Encoding.UTF8, "application/json")
                                };

            await client.SendAsync(request);
            var logs = string.Join(Environment.NewLine, logger.Messages.ToArray());

            Assert.DoesNotContain("13800000000", logs);
            Assert.Contains("\"mobile\":\"***\"", logs);
        }

        [Fact]
        public async Task Logging_ShouldRedact_CustomSensitiveHeader()
        {
            var logger = new ListLogger<LoggingHandler>();
            var handler = CreateLoggingClientHandler(logger, enableLogRedaction: true,
                additionalSensitiveHeaders: new[] { "X-Secret" });

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/header-redaction");
            request.Headers.TryAddWithoutValidation("X-Secret", "my-secret-header-value");

            await client.SendAsync(request);
            var logs = string.Join(Environment.NewLine, logger.Messages.ToArray());

            Assert.DoesNotContain("my-secret-header-value", logs);
            Assert.Contains("***", logs);
        }

        [Fact]
        public async Task Logging_ShouldSkipResponseBodyAudit_ForStreamingRequest()
        {
            var logger = new ListLogger<LoggingHandler>();
            var handler = CreateLoggingClientHandler(logger, enableLogRedaction: true, responseBody: "stream-body-secret");

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/stream");
            request.Options.Set(new HttpRequestOptionsKey<bool>("Common.HttpClients.SkipResponseBodyAudit"), true);

            await client.SendAsync(request);
            var logs = string.Join(Environment.NewLine, logger.Messages.ToArray());

            Assert.DoesNotContain("stream-body-secret", logs);
            Assert.Contains("response body skipped for streaming request", logs);
        }

        private static LoggingHandler CreateLoggingClientHandler(ListLogger<LoggingHandler> logger, bool enableLogRedaction,
            bool auditLog = true, int maxOutputLength = 0, string responseBody = "{\"access_token\":\"resp-secret-token\",\"message\":\"ok\"}",
            string responseContentType = "application/json", IHttpContextAccessor? accessor = null, Action<HttpRequestMessage>? onRequest = null,
            string[]? additionalSensitiveFields = null, string[]? additionalSensitiveHeaders = null)
        {
            var options = Options.Create(new HttpClientOptions
                                         {
                                             AuditLog = auditLog,
                                             EnableLogRedaction = enableLogRedaction,
                                             MaxOutputResponseLength = maxOutputLength,
                                             AdditionalSensitiveFields = additionalSensitiveFields ?? Array.Empty<string>(),
                                             AdditionalSensitiveHeaders = additionalSensitiveHeaders ?? Array.Empty<string>()
                                         });

            var loggingHandler = new LoggingHandler(logger, options, accessor);
            loggingHandler.InnerHandler = new DelegateHttpMessageHandler((request, _) =>
            {
                onRequest?.Invoke(request);

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                                       {
                                           Content = new StringContent(responseBody, Encoding.UTF8, responseContentType)
                                       });
            });
            return loggingHandler;
        }
    }
}
