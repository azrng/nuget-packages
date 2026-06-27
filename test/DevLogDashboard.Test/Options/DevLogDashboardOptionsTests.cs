using Azrng.DevLogDashboard.Options;
using Microsoft.Extensions.Logging;

namespace Azrng.DevLogDashboard.Test.Options;

public class DevLogDashboardOptionsTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var options = new DevLogDashboardOptions();

        options.EndpointPath.Should().Be("/dev-logs");
        options.MaxLogCount.Should().Be(10000);
        options.OnlyLogErrors.Should().BeFalse();
        options.MinLogLevel.Should().Be(LogLevel.Trace);
        options.BasicAuthentication.Should().BeNull();
        options.ApplicationName.Should().BeNull();
        options.ApplicationVersion.Should().BeNull();
        options.MaxPropertySerializationLength.Should().Be(2048);
        options.SkipStructuredProperties.Should().BeFalse();
    }

    [Fact]
    public void IgnoredPaths_ShouldContainDefaults()
    {
        var options = new DevLogDashboardOptions();

        options.IgnoredPaths.Should().Contain("/health");
        options.IgnoredPaths.Should().Contain("/healthz");
        options.IgnoredPaths.Should().Contain("/ready");
        options.IgnoredPaths.Should().Contain("/metrics");
        options.IgnoredPaths.Should().Contain("/dev-logs");
        options.IgnoredPaths.Should().Contain("/favicon.ico");
        options.IgnoredPaths.Should().HaveCount(6);
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var authOptions = new DevLogDashboardBasicAuthenticationOptions();
        var options = new DevLogDashboardOptions
        {
            EndpointPath = "/custom-path",
            MaxLogCount = 5000,
            OnlyLogErrors = true,
            MinLogLevel = LogLevel.Error,
            BasicAuthentication = authOptions,
            ApplicationName = "MyApp",
            ApplicationVersion = "2.0.0",
            MaxPropertySerializationLength = 4096,
            SkipStructuredProperties = true
        };

        options.EndpointPath.Should().Be("/custom-path");
        options.MaxLogCount.Should().Be(5000);
        options.OnlyLogErrors.Should().BeTrue();
        options.MinLogLevel.Should().Be(LogLevel.Error);
        options.BasicAuthentication.Should().BeSameAs(authOptions);
        options.ApplicationName.Should().Be("MyApp");
        options.ApplicationVersion.Should().Be("2.0.0");
        options.MaxPropertySerializationLength.Should().Be(4096);
        options.SkipStructuredProperties.Should().BeTrue();
    }

    [Fact]
    public void IgnoredPaths_CanBeCustomized()
    {
        var options = new DevLogDashboardOptions();
        options.IgnoredPaths = new List<string> { "/custom-health" };

        options.IgnoredPaths.Should().HaveCount(1);
        options.IgnoredPaths.Should().Contain("/custom-health");
    }
}
