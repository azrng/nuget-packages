using Azrng.DevLogDashboard.Background;
using Azrng.DevLogDashboard.Middleware;
using Azrng.DevLogDashboard.Models;
using Azrng.DevLogDashboard.Options;
using Azrng.DevLogDashboard.Storage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace DevLogDashboard.Test.Middleware;

/// <summary>
/// DevLogDashboardMiddleware tests.
/// </summary>
public class DevLogDashboardMiddlewareTest
{
    [Fact]
    public async Task InvokeAsync_WhenAuthorizationFilterIsNull_ShouldAllowAnonymousRemoteAccess()
    {
        var queue = new BackgroundLogQueue(2);
        await queue.QueueLogEntryAsync(new LogEntry { Message = "1" });
        await queue.QueueLogEntryAsync(new LogEntry { Message = "2" });
        await queue.QueueLogEntryAsync(new LogEntry { Message = "3" });

        var context = CreateContext(
            CreateServices(new DevLogDashboardOptions(), Mock.Of<ILogStore>(), queue),
            "/api/serverTime");

        var middleware = new DevLogDashboardMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);

        using var document = JsonDocument.Parse(await ReadBodyAsync(context));
        document.RootElement.GetProperty("queuedCount").GetInt32().Should().Be(2);
        document.RootElement.GetProperty("droppedCount").GetInt64().Should().Be(1);
    }

    [Fact]
    public async Task InvokeAsync_WhenAuthorizationFilterReturnsFalse_ShouldReturnForbidden()
    {
        var context = CreateContext(
            CreateServices(
                new DevLogDashboardOptions
                {
                    AuthorizationFilter = _ => Task.FromResult(false)
                },
                Mock.Of<ILogStore>(),
                new BackgroundLogQueue(10)),
            "/api/serverTime");

        var middleware = new DevLogDashboardMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        (await ReadBodyAsync(context)).Should().Be("Forbidden");
    }

    [Fact]
    public async Task InvokeAsync_WhenQueryContainsUtcIsoTime_ShouldConvertToLocalTime()
    {
        var expectedStart = new DateTimeOffset(2026, 4, 15, 0, 0, 0, TimeSpan.Zero).LocalDateTime;
        var expectedEnd = new DateTimeOffset(2026, 4, 15, 1, 30, 0, TimeSpan.Zero).LocalDateTime;
        LogQuery? capturedQuery = null;

        var storeMock = new Mock<ILogStore>();
        storeMock.Setup(x => x.QueryAsync(It.IsAny<LogQuery>(), It.IsAny<CancellationToken>()))
            .Callback<LogQuery, CancellationToken>((query, _) => capturedQuery = query)
            .ReturnsAsync(PageResult<LogEntry>.Create([], 0, 1, 50));

        var context = CreateContext(
            CreateServices(
                new DevLogDashboardOptions(),
                storeMock.Object,
                new BackgroundLogQueue(10)),
            "/api/logs",
            queryString: "?startTime=2026-04-15T00:00:00Z&endTime=2026-04-15T01:30:00Z&pageIndex=1&pageSize=50");

        var middleware = new DevLogDashboardMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        capturedQuery.Should().NotBeNull();
        capturedQuery!.StartTime.Should().Be(expectedStart);
        capturedQuery.EndTime.Should().Be(expectedEnd);
    }

    [Fact]
    public async Task InvokeAsync_WhenBasicAuthenticationSchemeIsConfiguredWithoutHeader_ShouldChallenge()
    {
        var context = CreateContext(
            CreateServices(
                new DevLogDashboardOptions
                {
                    BasicAuthentication = new DevLogDashboardBasicAuthenticationOptions
                    {
                        Scheme = TestBasicAuthenticationHandler.SchemeName,
                        Realm = "Dashboard"
                    }
                },
                Mock.Of<ILogStore>(),
                new BackgroundLogQueue(10),
                services =>
                {
                    services.AddAuthentication()
                        .AddScheme<AuthenticationSchemeOptions, TestBasicAuthenticationHandler>(
                            TestBasicAuthenticationHandler.SchemeName,
                            _ => { });
                }),
            "/api/serverTime");

        var middleware = new DevLogDashboardMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        context.Response.Headers.WWWAuthenticate.ToString().Should().Be("Basic realm=\"Dashboard\"");
    }

    [Fact]
    public async Task InvokeAsync_WhenBasicAuthenticationSchemeIsConfiguredWithValidHeader_ShouldAuthenticateAndReturnQueueStats()
    {
        var queue = new BackgroundLogQueue(2);
        await queue.QueueLogEntryAsync(new LogEntry { Message = "1" });
        await queue.QueueLogEntryAsync(new LogEntry { Message = "2" });
        await queue.QueueLogEntryAsync(new LogEntry { Message = "3" });

        var context = CreateContext(
            CreateServices(
                new DevLogDashboardOptions
                {
                    BasicAuthentication = new DevLogDashboardBasicAuthenticationOptions
                    {
                        Scheme = TestBasicAuthenticationHandler.SchemeName,
                        Realm = "Dashboard"
                    }
                },
                Mock.Of<ILogStore>(),
                queue,
                services =>
                {
                    services.AddAuthentication()
                        .AddScheme<AuthenticationSchemeOptions, TestBasicAuthenticationHandler>(
                            TestBasicAuthenticationHandler.SchemeName,
                            _ => { });
                }),
            "/api/serverTime",
            authorizationHeader: TestBasicAuthenticationHandler.ValidAuthorizationHeader);

        var middleware = new DevLogDashboardMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        context.User.Identity?.IsAuthenticated.Should().BeTrue();
        context.User.Identity?.AuthenticationType.Should().Be(TestBasicAuthenticationHandler.SchemeName);

        using var document = JsonDocument.Parse(await ReadBodyAsync(context));
        document.RootElement.GetProperty("queuedCount").GetInt32().Should().Be(2);
        document.RootElement.GetProperty("droppedCount").GetInt64().Should().Be(1);
    }

    private static ServiceProvider CreateServices(
        DevLogDashboardOptions options,
        ILogStore logStore,
        IBackgroundLogQueue queue,
        Action<IServiceCollection>? configureAuthentication = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        configureAuthentication?.Invoke(services);
        services.AddSingleton(options);
        services.AddSingleton(logStore);
        services.AddSingleton(queue);
        return services.BuildServiceProvider();
    }

    private static DefaultHttpContext CreateContext(
        IServiceProvider services,
        string path,
        string queryString = "",
        string? authorizationHeader = null)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = services
        };
        context.Request.Path = path;
        context.Request.Method = HttpMethods.Get;
        context.Request.QueryString = new QueryString(queryString);
        context.Response.Body = new MemoryStream();

        if (!string.IsNullOrWhiteSpace(authorizationHeader))
        {
            context.Request.Headers.Authorization = authorizationHeader;
        }

        return context;
    }

    private static async Task<string> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    private sealed class TestBasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "DashboardBasic";
        public const string ValidAuthorizationHeader = "Basic dXNlcjpwYXNz";

        public TestBasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authorization))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing authorization header"));
            }

            if (!string.Equals(authorization.ToString(), ValidAuthorizationHeader, StringComparison.Ordinal))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid authorization header"));
            }

            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.Name, "user")
                },
                SchemeName);

            return Task.FromResult(AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
    }
}
