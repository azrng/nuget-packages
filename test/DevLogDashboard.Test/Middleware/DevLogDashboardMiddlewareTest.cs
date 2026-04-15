using Azrng.DevLogDashboard.Background;
using Azrng.DevLogDashboard.Middleware;
using Azrng.DevLogDashboard.Models;
using Azrng.DevLogDashboard.Options;
using Azrng.DevLogDashboard.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace DevLogDashboard.Test.Middleware;

/// <summary>
/// DevLogDashboardMiddleware tests.
/// </summary>
public class DevLogDashboardMiddlewareTest
{
    [Fact]
    public async Task InvokeAsync_WhenRemoteRequestIsNotAllowed_ShouldReturnForbidden()
    {
        var context = CreateContext(
            CreateServices(new DevLogDashboardOptions(), Mock.Of<ILogStore>(), new BackgroundLogQueue(10)),
            "/api/serverTime",
            remoteIp: IPAddress.Parse("203.0.113.10"),
            localIp: IPAddress.Loopback);

        var middleware = new DevLogDashboardMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        (await ReadBodyAsync(context)).Should().Be("Forbidden");
    }

    [Fact]
    public async Task InvokeAsync_WhenNonLocalAccessIsAllowed_ShouldReturnQueueStats()
    {
        var queue = new BackgroundLogQueue(2);
        await queue.QueueLogEntryAsync(new LogEntry { Message = "1" });
        await queue.QueueLogEntryAsync(new LogEntry { Message = "2" });
        await queue.QueueLogEntryAsync(new LogEntry { Message = "3" });

        var context = CreateContext(
            CreateServices(
                new DevLogDashboardOptions
                {
                    AllowNonLocalAccess = true
                },
                Mock.Of<ILogStore>(),
                queue),
            "/api/serverTime",
            remoteIp: IPAddress.Parse("203.0.113.10"),
            localIp: IPAddress.Loopback);

        var middleware = new DevLogDashboardMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);

        using var document = JsonDocument.Parse(await ReadBodyAsync(context));
        document.RootElement.GetProperty("queuedCount").GetInt32().Should().Be(2);
        document.RootElement.GetProperty("droppedCount").GetInt64().Should().Be(1);
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
            remoteIp: IPAddress.Loopback,
            localIp: IPAddress.Loopback,
            queryString: "?startTime=2026-04-15T00:00:00Z&endTime=2026-04-15T01:30:00Z&pageIndex=1&pageSize=50");

        var middleware = new DevLogDashboardMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        capturedQuery.Should().NotBeNull();
        capturedQuery!.StartTime.Should().Be(expectedStart);
        capturedQuery.EndTime.Should().Be(expectedEnd);
    }

    private static ServiceProvider CreateServices(DevLogDashboardOptions options, ILogStore logStore, IBackgroundLogQueue queue)
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
        IPAddress remoteIp,
        IPAddress localIp,
        string queryString = "")
    {
        var context = new DefaultHttpContext
        {
            RequestServices = services
        };
        context.Request.Path = path;
        context.Request.QueryString = new QueryString(queryString);
        context.Connection.RemoteIpAddress = remoteIp;
        context.Connection.LocalIpAddress = localIp;
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }
}
