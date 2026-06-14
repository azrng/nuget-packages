using Common.HttpClients.Next.Test.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Common.HttpClients.Next.Test
{
    /// <summary>
    /// LoggingHandler 行为测试：审计开关、跳过标记、二进制检测、命名 options 解析、TraceId
    /// </summary>
    public class LoggingHandlerTests
    {
        [Fact]
        public async Task SendAsync_WhenAuditLogDisabled_ShouldNotLog()
        {
            var (logger, handler) = CreateHandler("clientA", new HttpClientOptions { AuditLog = false });
            using var client = new HttpClient(handler);
            using var req = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/no-audit");

            await client.SendAsync(req);

            Assert.Empty(logger.Messages);
        }

        [Fact]
        public async Task SendAsync_WithSkipLoggerHeader_ShouldNotLog()
        {
            var (logger, handler) = CreateHandler("clientA", new HttpClientOptions { AuditLog = true });
            using var client = new HttpClient(handler);
            using var req = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/skip");
            req.Headers.TryAddWithoutValidation("X-Skip-Logger", "1");

            await client.SendAsync(req);

            Assert.Empty(logger.Messages);
        }

        [Fact]
        public async Task SendAsync_WithLoggerHeaderSkipValue_ShouldNotLog()
        {
            var (logger, handler) = CreateHandler("clientA", new HttpClientOptions { AuditLog = true });
            using var client = new HttpClient(handler);
            using var req = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/skip-value");
            req.Headers.TryAddWithoutValidation("X-Logger", "skip");

            await client.SendAsync(req);

            Assert.Empty(logger.Messages);
        }

        [Fact]
        public async Task SendAsync_ShouldLogRequestStartAndAudit()
        {
            var (logger, handler) = CreateHandler("clientA", new HttpClientOptions
            {
                AuditLog = true,
                EnableLogRedaction = false,
                MaxOutputResponseLength = 0
            });
            using var client = new HttpClient(handler);
            using var req = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/logged");

            await client.SendAsync(req);
            var joined = string.Join(Environment.NewLine, logger.Messages);

            Assert.Contains("Http请求开始", joined);
            Assert.Contains("Http请求审计日志", joined);
        }

        [Fact]
        public async Task SendAsync_ResponseContent_ShouldBeTruncatedByOptions()
        {
            var (logger, handler) = CreateHandler("clientA", new HttpClientOptions
            {
                AuditLog = true,
                EnableLogRedaction = false,
                MaxOutputResponseLength = 5
            });
            handler.InnerHandler = new DelegateHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("1234567890")
            }));

            using var client = new HttpClient(handler);
            using var req = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/truncate");

            await client.SendAsync(req);
            var joined = string.Join(Environment.NewLine, logger.Messages);

            Assert.Contains("12345", joined);
            Assert.DoesNotContain("1234567890", joined);
            Assert.Contains("...", joined);
        }

        [Fact]
        public async Task SendAsync_BinaryContent_ShouldNotBeRead()
        {
            var (logger, handler) = CreateHandler("clientA", new HttpClientOptions
            {
                AuditLog = true,
                EnableLogRedaction = false
            });
            handler.InnerHandler = new DelegateHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("binary-secret", Encoding.UTF8, "image/jpeg")
            }));

            using var client = new HttpClient(handler);
            using var req = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/binary");

            await client.SendAsync(req);
            var joined = string.Join(Environment.NewLine, logger.Messages);

            Assert.DoesNotContain("binary-secret", joined);
        }

        [Fact]
        public async Task SendAsync_FormDataContent_ShouldBeSkipped()
        {
            var (logger, handler) = CreateHandler("clientA", new HttpClientOptions
            {
                AuditLog = true,
                EnableLogRedaction = false
            });

            using var client = new HttpClient(handler);
            using var multipart = new MultipartFormDataContent();
            multipart.Add(new StringContent("field-value"), "field");
            using var req = new HttpRequestMessage(HttpMethod.Post, "https://unit.test/form") { Content = multipart };

            await client.SendAsync(req);
            var joined = string.Join(Environment.NewLine, logger.Messages);

            Assert.DoesNotContain("field-value", joined);
            Assert.Contains("[multipart form-data content skipped]", joined);
        }

        [Fact]
        public async Task SendAsync_StreamingRequest_ShouldSkipResponseBody()
        {
            var (logger, handler) = CreateHandler("clientA", new HttpClientOptions
            {
                AuditLog = true,
                EnableLogRedaction = false
            });
            handler.InnerHandler = new DelegateHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("stream-body-secret")
            }));

            using var client = new HttpClient(handler);
            using var req = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/stream");
            req.Options.Set(new HttpRequestOptionsKey<bool>("Common.HttpClients.SkipResponseBodyAudit"), true);

            await client.SendAsync(req);
            var joined = string.Join(Environment.NewLine, logger.Messages);

            Assert.DoesNotContain("stream-body-secret", joined);
            Assert.Contains("response body skipped for streaming request", joined);
        }

        [Fact]
        public async Task SendAsync_ShouldReadOptionsByClientName_NotGlobalDefault()
        {
            // 回归：原 LoggingHandler 注入 IOptions<>（非命名），命名客户端配置不生效
            var monitor = new FakeOptionsMonitor<HttpClientOptions>(new HttpClientOptions { AuditLog = true });
            monitor.Set("clientA", new HttpClientOptions { AuditLog = false });
            monitor.Set("clientB", new HttpClientOptions { AuditLog = true });

            var loggerA = new ListLogger<LoggingHandler>();
            var handlerA = new LoggingHandler("clientA", loggerA, monitor);
            handlerA.InnerHandler = new DelegateHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("ok")
            }));

            var loggerB = new ListLogger<LoggingHandler>();
            var handlerB = new LoggingHandler("clientB", loggerB, monitor);
            handlerB.InnerHandler = new DelegateHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("ok")
            }));

            using (var client = new HttpClient(handlerA))
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/a");
                await client.SendAsync(req);
            }

            using (var client = new HttpClient(handlerB))
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/b");
                await client.SendAsync(req);
            }

            Assert.Empty(loggerA.Messages); // clientA 关闭审计
            Assert.NotEmpty(loggerB.Messages); // clientB 开启审计
        }

        [Fact]
        public async Task SendAsync_AdditionalSensitiveHeaders_ShouldBeAppliedPerClient()
        {
            var monitor = new FakeOptionsMonitor<HttpClientOptions>(new HttpClientOptions { AuditLog = true });
            monitor.Set("clientA", new HttpClientOptions
            {
                AuditLog = true,
                EnableLogRedaction = true,
                AdditionalSensitiveHeaders = new List<string> { "X-Secret-A" }
            });
            monitor.Set("clientB", new HttpClientOptions
            {
                AuditLog = true,
                EnableLogRedaction = true,
                AdditionalSensitiveHeaders = new List<string> { "X-Secret-B" }
            });

            var loggerA = new ListLogger<LoggingHandler>();
            var handlerA = new LoggingHandler("clientA", loggerA, monitor);
            handlerA.InnerHandler = new DelegateHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("ok")
            }));

            var loggerB = new ListLogger<LoggingHandler>();
            var handlerB = new LoggingHandler("clientB", loggerB, monitor);
            handlerB.InnerHandler = new DelegateHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("ok")
            }));

            using (var client = new HttpClient(handlerA))
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/a");
                req.Headers.TryAddWithoutValidation("X-Secret-A", "value-a");
                req.Headers.TryAddWithoutValidation("X-Secret-B", "value-b");
                await client.SendAsync(req);
            }

            using (var client = new HttpClient(handlerB))
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/b");
                req.Headers.TryAddWithoutValidation("X-Secret-A", "value-a");
                req.Headers.TryAddWithoutValidation("X-Secret-B", "value-b");
                await client.SendAsync(req);
            }

            var logsA = string.Join(Environment.NewLine, loggerA.Messages);
            var logsB = string.Join(Environment.NewLine, loggerB.Messages);

            Assert.DoesNotContain("value-a", logsA); // clientA 脱敏 X-Secret-A
            Assert.Contains("value-b", logsA);       // clientA 不脱敏 X-Secret-B
            Assert.Contains("value-a", logsB);       // clientB 不脱敏 X-Secret-A
            Assert.DoesNotContain("value-b", logsB); // clientB 脱敏 X-Secret-B
        }

        [Fact]
        public async Task SendAsync_TraceId_ShouldBeInjectedFromHttpContext()
        {
            var (logger, handler) = CreateHandler("clientA", new HttpClientOptions
            {
                AuditLog = true,
                EnableLogRedaction = false
            });
            var accessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext
                {
                    TraceIdentifier = "ctx-trace"
                }
            };

            var monitor = new FakeOptionsMonitor<HttpClientOptions>(new HttpClientOptions { AuditLog = true, EnableLogRedaction = false });
            handler = new LoggingHandler("clientA", logger, monitor, accessor);
            handler.InnerHandler = new DelegateHttpMessageHandler((r, _) =>
            {
                Assert.Equal("ctx-trace", r.Headers.GetValues("X-Trace-Id").Single());
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("ok")
                });
            });

            using var client = new HttpClient(handler);
            using var req = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/trace");

            await client.SendAsync(req);

            var joined = string.Join(Environment.NewLine, logger.Messages);
            Assert.Contains("ctx-trace", joined);
        }

        [Fact]
        public async Task SendAsync_TraceId_ShouldRespectExistingHeader()
        {
            var (logger, _) = CreateHandler("clientA", new HttpClientOptions { AuditLog = true, EnableLogRedaction = false });
            var monitor = new FakeOptionsMonitor<HttpClientOptions>(new HttpClientOptions { AuditLog = true, EnableLogRedaction = false });

            string? captured = null;
            var handler = new LoggingHandler("clientA", logger, monitor);
            handler.InnerHandler = new DelegateHttpMessageHandler((r, _) =>
            {
                captured = r.Headers.GetValues("X-Trace-Id").Single();
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("ok")
                });
            });

            using var client = new HttpClient(handler);
            using var req = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/trace");
            req.Headers.TryAddWithoutValidation("X-Trace-Id", "client-supplied");

            await client.SendAsync(req);

            Assert.Equal("client-supplied", captured);
        }

        [Fact]
        public void Constructor_NullClientName_ShouldThrow()
        {
            var monitor = new FakeOptionsMonitor<HttpClientOptions>(new HttpClientOptions());
            var logger = new ListLogger<LoggingHandler>();

            Assert.Throws<ArgumentNullException>(() => new LoggingHandler(null!, logger, monitor));
        }

        [Fact]
        public async Task SendAsync_WhenCustomRedactorRegistered_ShouldUseItForAllClients()
        {
            var tracking = new TrackingRedactor();
            var monitor = new FakeOptionsMonitor<HttpClientOptions>(new HttpClientOptions { AuditLog = true });
            monitor.Set("clientA", new HttpClientOptions
            {
                AuditLog = true,
                EnableLogRedaction = true,
                AdditionalSensitiveFields = new List<string> { "ignored-by-custom" }
            });

            var logger = new ListLogger<LoggingHandler>();
            var handler = new LoggingHandler("clientA", logger, monitor, null, tracking);
            handler.InnerHandler = new DelegateHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"x\":1}")
            }));

            using var client = new HttpClient(handler);
            using var req = new HttpRequestMessage(HttpMethod.Post, "https://unit.test/x")
            {
                Content = new StringContent("{\"x\":1}", Encoding.UTF8, "application/json")
            };

            await client.SendAsync(req);

            Assert.True(tracking.ContentCalled);
            Assert.True(tracking.HeadersCalled);
        }

        private static (ListLogger<LoggingHandler> logger, LoggingHandler handler) CreateHandler(string clientName, HttpClientOptions options)
        {
            var logger = new ListLogger<LoggingHandler>();
            var monitor = new FakeOptionsMonitor<HttpClientOptions>(new HttpClientOptions());
            monitor.Set(clientName, options);

            var handler = new LoggingHandler(clientName, logger, monitor);
            handler.InnerHandler = new DelegateHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("ok")
            }));
            return (logger, handler);
        }

        private sealed class TrackingRedactor : IHttpLogRedactor
        {
            public bool ContentCalled { get; private set; }
            public bool HeadersCalled { get; private set; }

            public string RedactContent(string content)
            {
                ContentCalled = true;
                return content;
            }

            public IDictionary<string, string> RedactHeaders(IDictionary<string, string>? headers)
            {
                HeadersCalled = true;
                return headers ?? new Dictionary<string, string>();
            }
        }
    }
}
