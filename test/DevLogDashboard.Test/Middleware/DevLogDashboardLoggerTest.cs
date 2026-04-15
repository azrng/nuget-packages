using Azrng.DevLogDashboard.Background;
using Azrng.DevLogDashboard.Middleware;
using Azrng.DevLogDashboard.Models;
using Azrng.DevLogDashboard.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DevLogDashboard.Test.Middleware;

/// <summary>
/// DevLogDashboardLogger tests.
/// </summary>
public class DevLogDashboardLoggerTest
{
    [Fact]
    public void Log_WithActiveScope_ShouldPersistScopeValues()
    {
        // Arrange
        LogEntry? capturedEntry = null;
        var queueMock = new Mock<IBackgroundLogQueue>();
        queueMock.Setup(x => x.QueueLogEntryAsync(It.IsAny<LogEntry>()))
            .Callback<LogEntry>(entry => capturedEntry = entry)
            .Returns(ValueTask.CompletedTask);

        var logger = new DevLogDashboardLogger(
            "DevLogDashboard.Test",
            new DevLogDashboardOptions(),
            new HttpContextAccessor(),
            queueMock.Object,
            "Development");

        logger.SetScopeProvider(new LoggerExternalScopeProvider());

        // Act
        using (((ILogger)logger).BeginScope("scope-a"))
        using (((ILogger)logger).BeginScope("scope-b"))
        {
            logger.Log(
                LogLevel.Information,
                new EventId(1, "test"),
                "message",
                null,
                static (state, _) => state);
        }

        // Assert
        capturedEntry.Should().NotBeNull();
        capturedEntry!.Properties.Should().ContainKey("_scopes");
        var scopes = capturedEntry.Properties["_scopes"].Should().BeAssignableTo<List<string>>().Subject;
        scopes.Should().ContainInOrder("scope-a", "scope-b");
    }
}
