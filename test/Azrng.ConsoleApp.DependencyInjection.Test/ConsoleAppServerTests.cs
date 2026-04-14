using Azrng.ConsoleApp.DependencyInjection;
using Azrng.ConsoleApp.DependencyInjection.Logger;
using Azrng.Core;
using CoreLogLevel = Azrng.Core.Enums.LogLevel;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Azrng.ConsoleApp.DependencyInjection.Test;

public class ConsoleAppServerTests
{
    [Fact]
    public void Build_RegistersConfigurationAndLoggingServices()
    {
        var server = new ConsoleAppServer(["App:Mode=Test"]);

        using var provider = server.Build<NoOpStartService>();

        provider.GetRequiredService<IConfiguration>()["App:Mode"].Should().Be("Test");
        provider.GetRequiredService<ILogger<ConsoleAppServer>>().Should().NotBeNull();
    }

    [Fact]
    public void Build_WithRegisterServicesAction_ReplacesPreviousStartRegistration()
    {
        var server = new ConsoleAppServer();
        var marker = new StartupMarker("registered");
        server.Services.AddSingleton<IServiceStart, OriginalStartService>();

        using var provider = server.Build<InjectedStartService>(services =>
        {
            services.AddSingleton(marker);
        });

        server.Services.Where(x => x.ServiceType == typeof(IServiceStart)).Should().ContainSingle();
        server.Services.Single(x => x.ServiceType == typeof(IServiceStart)).ImplementationType
            .Should().Be(typeof(InjectedStartService));

        provider.GetRequiredService<IServiceStart>().Should().BeOfType<InjectedStartService>();
        provider.GetRequiredService<StartupMarker>().Value.Should().Be("registered");
    }

    [Fact]
    public async Task RunAsync_ExecutesStartServiceWithinScope()
    {
        ScopedStartRecorder.Reset();
        var server = new ConsoleAppServer();

        await using var provider = server.Build<ScopedStartService>(services =>
        {
            services.AddScoped<ScopedDependency>();
        });

        await provider.RunAsync();

        ScopedStartRecorder.RunCount.Should().Be(1);
        ScopedStartRecorder.LastTitle.Should().Be("Scoped Start");
        ScopedStartRecorder.DependencyId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task RunAsync_WhenStartServiceThrows_RethrowsOriginalException()
    {
        var server = new ConsoleAppServer();

        await using var provider = server.Build<ThrowingStartService>();

        var act = () => provider.RunAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("boom");
    }

    [Fact]
    public void ExtensionsLoggerProvider_CreatesLoggerThatRespectsMinimumLevel()
    {
        var originalLevel = CoreGlobalConfig.MinimumLevel;
        try
        {
            CoreGlobalConfig.MinimumLevel = CoreLogLevel.Warning;
            var provider = new ExtensionsLoggerProvider();

            var logger = provider.CreateLogger("ConsoleApp");
            var extensionsLogger = logger.Should().BeOfType<ExtensionsLogger>().Subject;

            extensionsLogger.BeginScope("scope").Should().NotBeNull();
            extensionsLogger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information).Should().BeFalse();
            extensionsLogger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Warning).Should().BeTrue();

            var act = () => extensionsLogger.Log(
                Microsoft.Extensions.Logging.LogLevel.Warning,
                new EventId(1, "warning"),
                "message",
                null,
                (state, _) => state);

            act.Should().NotThrow();
        }
        finally
        {
            CoreGlobalConfig.MinimumLevel = originalLevel;
        }
    }

    [Fact]
    public void PrintTitle_WritesDividerAndTitleToConsole()
    {
        var originalWriter = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            ConsoleTool.PrintTitle("Demo");
        }
        finally
        {
            Console.SetOut(originalWriter);
        }

        var output = writer.ToString();
        output.Should().Contain("Title");
        output.Should().Contain("Demo");
        output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Count(line => line.Contains("===="))
            .Should().BeGreaterOrEqualTo(2);
    }
}

public sealed class NoOpStartService : IServiceStart
{
    public string Title => "NoOp";

    public Task RunAsync()
    {
        return Task.CompletedTask;
    }
}

public sealed class OriginalStartService : IServiceStart
{
    public string Title => "Original";

    public Task RunAsync()
    {
        return Task.CompletedTask;
    }
}

public sealed record StartupMarker(string Value);

public sealed class InjectedStartService(StartupMarker marker) : IServiceStart
{
    public StartupMarker Marker { get; } = marker;

    public string Title => $"Injected:{Marker.Value}";

    public Task RunAsync()
    {
        return Task.CompletedTask;
    }
}

public sealed class ScopedDependency
{
    public Guid Id { get; } = Guid.NewGuid();
}

public sealed class ScopedStartService(ScopedDependency dependency) : IServiceStart
{
    public string Title => "Scoped Start";

    public Task RunAsync()
    {
        ScopedStartRecorder.Record(Title, dependency.Id);
        return Task.CompletedTask;
    }
}

public sealed class ThrowingStartService : IServiceStart
{
    public string Title => "Throwing";

    public Task RunAsync()
    {
        throw new InvalidOperationException("boom");
    }
}

internal static class ScopedStartRecorder
{
    public static int RunCount { get; private set; }

    public static string? LastTitle { get; private set; }

    public static Guid DependencyId { get; private set; }

    public static void Reset()
    {
        RunCount = 0;
        LastTitle = null;
        DependencyId = Guid.Empty;
    }

    public static void Record(string title, Guid dependencyId)
    {
        RunCount++;
        LastTitle = title;
        DependencyId = dependencyId;
    }
}
