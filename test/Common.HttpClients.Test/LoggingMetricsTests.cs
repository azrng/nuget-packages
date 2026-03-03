using Common.HttpClients.Test.Helpers;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace Common.HttpClients.Test
{
    /// <summary>
    /// 日志性能指标和审计测试
    /// </summary>
    public class LoggingMetricsTests
    {
        /// <summary>
        /// 测试审计日志中包含请求耗时信息
        /// </summary>
        [Fact]
        public async Task AuditLog_ShouldIncludeElapsedTime()
        {
            var logger = new ListLogger<LoggingHandler>();
            var handler = CreateLoggingClientHandler(logger, enableLogRedaction: true, simulateDelay: 50);

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/metrics");

            await client.SendAsync(request);

            var logs = string.Join(Environment.NewLine, logger.Messages.ToArray());

            // 验证日志包含 "耗时" 或 "Elapsed" 字段
            Assert.Matches(@"耗时:\d+\.?\d*ms", logs);
        }

        /// <summary>
        /// 测试审计日志中包含请求开始和完整审计两条日志
        /// </summary>
        [Fact]
        public async Task AuditLog_ShouldIncludeStartAndCompleteLogs()
        {
            var logger = new ListLogger<LoggingHandler>();
            var handler = CreateLoggingClientHandler(logger, enableLogRedaction: true);

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/audit");

            await client.SendAsync(request);

            Assert.Equal(2, logger.Messages.Count());
            var messagesList = logger.Messages.ToList();
            Assert.Contains("Http请求开始", messagesList[0]);
            Assert.Contains("Http请求审计日志", messagesList[1]);
        }

        /// <summary>
        /// 测试日志包含完整的请求和响应信息
        /// </summary>
        [Fact]
        public async Task AuditLog_ShouldIncludeRequestAndResponseDetails()
        {
            var logger = new ListLogger<LoggingHandler>();
            var handler = CreateLoggingClientHandler(logger, enableLogRedaction: true,
                requestBody: "{\"username\":\"test\"}",
                responseBody: "{\"result\":\"ok\"}");

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://unit.test/details")
            {
                Content = new StringContent("{\"username\":\"test\"}", Encoding.UTF8, "application/json")
            };

            await client.SendAsync(request);

            var logs = string.Join(Environment.NewLine, logger.Messages.ToArray());

            Assert.Contains("RequestHeader", logs);
            Assert.Contains("RequestContent", logs);
            Assert.Contains("ResponseHeader", logs);
            Assert.Contains("ResponseContent", logs);
            Assert.Contains("username", logs);
            Assert.Contains("result", logs);
        }

        /// <summary>
        /// 测试日志中包含 TraceId
        /// </summary>
        [Fact]
        public async Task AuditLog_ShouldIncludeTraceId()
        {
            var logger = new ListLogger<LoggingHandler>();
            var handler = CreateLoggingClientHandler(logger, enableLogRedaction: true);

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/trace");

            await client.SendAsync(request);

            var logs = string.Join(Environment.NewLine, logger.Messages.ToArray());

            // 验证两条日志都包含相同的 TraceId
            var traceIdMatches = Regex.Matches(logs, @"TraceId:([^\s]+)");
            Assert.Equal(2, traceIdMatches.Count);

            var firstTraceId = traceIdMatches[0].Groups[1].Value;
            var secondTraceId = traceIdMatches[1].Groups[1].Value;

            Assert.Equal(firstTraceId, secondTraceId);
        }

        /// <summary>
        /// 测试日志中包含 HTTP 方法
        /// </summary>
        [Fact]
        public async Task AuditLog_ShouldIncludeHttpMethod()
        {
            var logger = new ListLogger<LoggingHandler>();
            var handler = CreateLoggingClientHandler(logger, enableLogRedaction: true);

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://unit.test/method");

            await client.SendAsync(request);

            var logs = string.Join(Environment.NewLine, logger.Messages.ToArray());
            Assert.Contains("Method:POST", logs);
        }

        /// <summary>
        /// 测试日志中包含状态码
        /// </summary>
        [Fact]
        public async Task AuditLog_ShouldIncludeStatusCode()
        {
            var logger = new ListLogger<LoggingHandler>();
            var handler = CreateLoggingClientHandler(logger, enableLogRedaction: true,
                statusCode: HttpStatusCode.Created);

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://unit.test/status");

            await client.SendAsync(request);

            var logs = string.Join(Environment.NewLine, logger.Messages.ToArray());
            Assert.Contains("StatusCode:Created", logs);
            // 注意：日志中记录的是状态码枚举名称而非数字
            Assert.Contains("Created", logs);
        }

        /// <summary>
        /// 测试日志中包含请求URL
        /// </summary>
        [Fact]
        public async Task AuditLog_ShouldIncludeRequestUrl()
        {
            var logger = new ListLogger<LoggingHandler>();
            var handler = CreateLoggingClientHandler(logger, enableLogRedaction: true);

            using var client = new HttpClient(handler);
            var testUrl = "https://unit.test/url-test?param1=value1";
            using var request = new HttpRequestMessage(HttpMethod.Get, testUrl);

            await client.SendAsync(request);

            var logs = string.Join(Environment.NewLine, logger.Messages.ToArray());
            Assert.Contains(testUrl, logs);
        }

        /// <summary>
        /// 测试耗时精度（毫秒级）
        /// </summary>
        [Fact]
        public async Task AuditLog_ElapsedTime_ShouldBeInMilliseconds()
        {
            var logger = new ListLogger<LoggingHandler>();
            var handler = CreateLoggingClientHandler(logger, enableLogRedaction: true, simulateDelay: 100);

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/timing");

            await client.SendAsync(request);

            var logs = string.Join(Environment.NewLine, logger.Messages.ToArray());

            // 验证耗时格式为数字+.+数字ms
            Assert.Matches(@"耗时:\d+\.\d+ms", logs);

            // 验证耗时大于等于模拟延迟（100ms）
            var match = Regex.Match(logs, @"耗时:(\d+\.\d+)ms");
            if (match.Success)
            {
                var elapsed = double.Parse(match.Groups[1].Value);
                Assert.True(elapsed >= 100, $"Elapsed time {elapsed}ms should be >= 100ms");
            }
        }

        /// <summary>
        /// 测试禁用审计日志时不记录日志
        /// </summary>
        [Fact]
        public async Task AuditLog_WhenDisabled_ShouldNotLog()
        {
            var logger = new ListLogger<LoggingHandler>();
            var handler = CreateLoggingClientHandler(logger, enableLogRedaction: true, auditLog: false);

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/no-audit");

            await client.SendAsync(request);

            Assert.Empty(logger.Messages);
        }

        private static LoggingHandler CreateLoggingClientHandler(ListLogger<LoggingHandler> logger,
            bool enableLogRedaction, bool auditLog = true, int simulateDelay = 0,
            string requestBody = null, string responseBody = "{\"result\":\"ok\"}",
            HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var options = Options.Create(new HttpClientOptions
            {
                AuditLog = auditLog,
                EnableLogRedaction = enableLogRedaction,
                MaxOutputResponseLength = 0
            });

            var loggingHandler = new LoggingHandler(logger, options, null);
            loggingHandler.InnerHandler = new DelegateHttpMessageHandler(async (request, _) =>
            {
                if (simulateDelay > 0)
                {
                    await Task.Delay(simulateDelay);
                }

                return await Task.FromResult(new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(responseBody ?? string.Empty, Encoding.UTF8, "application/json")
                });
            });
            return loggingHandler;
        }
    }
}
