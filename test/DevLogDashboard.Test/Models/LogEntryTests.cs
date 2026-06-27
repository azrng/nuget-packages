using Azrng.DevLogDashboard.Models;
using Microsoft.Extensions.Logging;

namespace Azrng.DevLogDashboard.Test.Models;

public class LogEntryTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var entry = new LogEntry();

        entry.Id.Should().NotBeNullOrWhiteSpace();
        entry.Id.Should().HaveLength(32);
        entry.Timestamp.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        entry.Level.Should().Be(LogLevel.Trace);
        entry.Message.Should().BeEmpty();
        entry.RequestId.Should().BeNull();
        entry.ConnectionId.Should().BeNull();
        entry.Exception.Should().BeNull();
        entry.StackTrace.Should().BeNull();
        entry.Source.Should().BeNull();
        entry.EventId.Should().Be(0);
        entry.RequestPath.Should().BeNull();
        entry.RequestMethod.Should().BeNull();
        entry.ResponseStatusCode.Should().BeNull();
        entry.ElapsedMilliseconds.Should().BeNull();
        entry.ThreadId.Should().BeNull();
        entry.ThreadName.Should().BeNull();
        entry.ProcessId.Should().BeNull();
        entry.MachineName.Should().BeNull();
        entry.Application.Should().BeNull();
        entry.AppVersion.Should().BeNull();
        entry.Environment.Should().BeNull();
        entry.SdkVersion.Should().BeNull();
        entry.Logger.Should().BeNull();
        entry.ActionId.Should().BeNull();
        entry.ActionName.Should().BeNull();
        entry.Properties.Should().BeEmpty();
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var entry = new LogEntry
        {
            Id = "test-id",
            RequestId = "req-1",
            ConnectionId = "conn-1",
            Level = LogLevel.Warning,
            Message = "test message",
            Exception = "test exception",
            StackTrace = "stack trace",
            Source = "TestSource",
            EventId = 42,
            RequestPath = "/api/test",
            RequestMethod = "GET",
            ResponseStatusCode = 200,
            ElapsedMilliseconds = 123.45,
            ThreadId = 7,
            ThreadName = "MainThread",
            ProcessId = 100,
            MachineName = "SERVER-1",
            Application = "TestApp",
            AppVersion = "1.0.0",
            Environment = "Production",
            SdkVersion = "1.0.0",
            Logger = "TestLogger",
            ActionId = "action-1",
            ActionName = "TestAction"
        };

        entry.Id.Should().Be("test-id");
        entry.RequestId.Should().Be("req-1");
        entry.ConnectionId.Should().Be("conn-1");
        entry.Level.Should().Be(LogLevel.Warning);
        entry.Message.Should().Be("test message");
        entry.Exception.Should().Be("test exception");
        entry.StackTrace.Should().Be("stack trace");
        entry.Source.Should().Be("TestSource");
        entry.EventId.Should().Be(42);
        entry.RequestPath.Should().Be("/api/test");
        entry.RequestMethod.Should().Be("GET");
        entry.ResponseStatusCode.Should().Be(200);
        entry.ElapsedMilliseconds.Should().Be(123.45);
        entry.ThreadId.Should().Be(7);
        entry.ThreadName.Should().Be("MainThread");
        entry.ProcessId.Should().Be(100);
        entry.MachineName.Should().Be("SERVER-1");
        entry.Application.Should().Be("TestApp");
        entry.AppVersion.Should().Be("1.0.0");
        entry.Environment.Should().Be("Production");
        entry.SdkVersion.Should().Be("1.0.0");
        entry.Logger.Should().Be("TestLogger");
        entry.ActionId.Should().Be("action-1");
        entry.ActionName.Should().Be("TestAction");
    }

    [Fact]
    public void GetAllProperties_ShouldContainAllFields()
    {
        var now = new DateTime(2025, 6, 26, 10, 30, 45, 123);
        var entry = new LogEntry
        {
            Timestamp = now,
            Level = LogLevel.Error,
            Message = "error occurred",
            Source = "MySource",
            RequestId = "req-1",
            RequestPath = "/api/test",
            RequestMethod = "POST",
            ResponseStatusCode = 500,
            ElapsedMilliseconds = 42.5,
            ThreadId = 5,
            ProcessId = 10,
            MachineName = "MACHINE",
            Application = "MyApp",
            Exception = "SystemException"
        };

        var props = entry.GetAllProperties();

        props["Timestamp"].Should().Be("2025-06-26 10:30:45.123");
        props["Level"].Should().Be("Error");
        props["Message"].Should().Be("error occurred");
        props["Source"].Should().Be("MySource");
        props["RequestId"].Should().Be("req-1");
        props["RequestPath"].Should().Be("/api/test");
        props["RequestMethod"].Should().Be("POST");
        props["ResponseStatusCode"].Should().Be("500");
        props["ElapsedMilliseconds"].Should().Be("42.50ms");
        props["ThreadId"].Should().Be("5");
        props["ProcessId"].Should().Be("10");
        props["MachineName"].Should().Be("MACHINE");
        props["Application"].Should().Be("MyApp");
        props["Exception"].Should().Be("SystemException");
    }

    [Fact]
    public void GetAllProperties_NullableFields_ShouldBeNull()
    {
        var entry = new LogEntry();

        var props = entry.GetAllProperties();

        props["RequestId"].Should().BeNull();
        props["ElapsedMilliseconds"].Should().BeNull();
        props["ResponseStatusCode"].Should().BeNull();
    }

    [Fact]
    public void GetAllProperties_WithCustomProperties_ShouldIncludeThem()
    {
        var entry = new LogEntry();
        entry.Properties["CustomKey"] = "CustomValue";
        entry.Properties["AnotherKey"] = 123;

        var props = entry.GetAllProperties();

        props["CustomKey"].Should().Be("CustomValue");
        props["AnotherKey"].Should().Be(123);
    }

    [Fact]
    public void GetAllProperties_CustomPropertyWithSameNameAsField_ShouldNotOverride()
    {
        var entry = new LogEntry { Source = "Original" };
        entry.Properties["Source"] = "Custom";

        var props = entry.GetAllProperties();

        props["Source"].Should().Be("Original");
    }

    [Fact]
    public void Properties_CanAddMultipleEntries()
    {
        var entry = new LogEntry();
        entry.Properties["Key1"] = "Value1";
        entry.Properties["Key2"] = "Value2";
        entry.Properties["Key3"] = null;

        entry.Properties.Should().HaveCount(3);
        entry.Properties["Key1"].Should().Be("Value1");
        entry.Properties["Key2"].Should().Be("Value2");
        entry.Properties["Key3"].Should().BeNull();
    }

    [Fact]
    public void Id_Format_ShouldBeNFormatGuid()
    {
        var entry = new LogEntry();

        entry.Id.Should().MatchRegex("^[0-9a-f]{32}$");
    }
}
