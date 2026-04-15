using Azrng.DevLogDashboard.Background;
using Azrng.DevLogDashboard.Middleware;
using Azrng.DevLogDashboard.Models;
using Azrng.DevLogDashboard.Options;
using Azrng.DevLogDashboard.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;

namespace DevLogDashboard.Test.Middleware;

/// <summary>
/// DevLogDashboardMiddleware tests.
/// </summary>
public class DevLogDashboardMiddlewareTest
{
    [Fact]
    public async Task InvokeAsync_WhenBasicAuthenticationIsNotConfigured_ShouldAllowAnonymousRemoteAccess()
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
    public async Task InvokeAsync_WhenBasicAuthenticationIsConfiguredWithoutHeader_ShouldReturnUnauthorized()
    {
        var context = CreateContext(
            CreateServices(
                new DevLogDashboardOptions
                {
                    BasicAuthentication = new DevLogDashboardBasicAuthenticationOptions
                    {
                        UserName = "admin",
                        Password = "123456",
                        Realm = "Dashboard"
                    }
                },
                Mock.Of<ILogStore>(),
                new BackgroundLogQueue(10)),
            "/api/serverTime");

        var middleware = new DevLogDashboardMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        context.Response.Headers.WWWAuthenticate.ToString().Should().Be("Basic realm=\"Dashboard\"");
        (await ReadBodyAsync(context)).Should().Be("Unauthorized");
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
    public async Task InvokeAsync_WhenBasicAuthenticationIsConfiguredWithInvalidHeader_ShouldReturnUnauthorized()
    {
        var context = CreateContext(
            CreateServices(
                new DevLogDashboardOptions
                {
                    BasicAuthentication = new DevLogDashboardBasicAuthenticationOptions
                    {
                        UserName = "admin",
                        Password = "123456",
                        Realm = "Dashboard"
                    }
                },
                Mock.Of<ILogStore>(),
                new BackgroundLogQueue(10)),
            "/api/serverTime",
            authorizationHeader: CreateBasicHeader("admin", "wrong-password"));

        var middleware = new DevLogDashboardMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        context.Response.Headers.WWWAuthenticate.ToString().Should().Be("Basic realm=\"Dashboard\"");
        (await ReadBodyAsync(context)).Should().Be("Unauthorized");
    }

    [Fact]
    public async Task InvokeAsync_WhenBasicAuthenticationIsConfiguredWithValidHeader_ShouldAuthenticateAndReturnQueueStats()
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
                        UserName = "admin",
                        Password = "123456",
                        Realm = "Dashboard"
                    }
                },
                Mock.Of<ILogStore>(),
                queue),
            "/api/serverTime",
            authorizationHeader: CreateBasicHeader("admin", "123456"));

        var middleware = new DevLogDashboardMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        context.User.Identity?.IsAuthenticated.Should().BeTrue();
        context.User.Identity?.AuthenticationType.Should().Be("Basic");

        using var document = JsonDocument.Parse(await ReadBodyAsync(context));
        document.RootElement.GetProperty("queuedCount").GetInt32().Should().Be(2);
        document.RootElement.GetProperty("droppedCount").GetInt64().Should().Be(1);
    }

    private static ServiceProvider CreateServices(
        DevLogDashboardOptions options,
        ILogStore logStore,
        IBackgroundLogQueue queue)
    {
        var services = new ServiceCollection();
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

    private static string CreateBasicHeader(string userName, string password)
    {
        var bytes = Encoding.UTF8.GetBytes($"{userName}:{password}");
        return $"Basic {Convert.ToBase64String(bytes)}";
    }
}
