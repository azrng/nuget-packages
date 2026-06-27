using Azrng.Core;
using Azrng.Core.Enums;
using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class LocalLogHelperTests : IDisposable
{
    private readonly string _logPath;
    private readonly LogLevel _originalLevel;

    public LocalLogHelperTests()
    {
        _originalLevel = CoreGlobalConfig.MinimumLevel;
        _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
    }

    public void Dispose()
    {
        CoreGlobalConfig.MinimumLevel = _originalLevel;
    }

    private string? GetLatestLogFilePath()
    {
        if (!Directory.Exists(_logPath)) return null;
        return new DirectoryInfo(_logPath)
            .GetFiles("*.log")
            .Where(f => f.Name != "error.log")
            .OrderByDescending(f => f.LastWriteTime)
            .FirstOrDefault()?.FullName;
    }

    private string? GetLatestLogContent()
    {
        var path = GetLatestLogFilePath();
        if (path == null || !File.Exists(path)) return null;
        return File.ReadAllText(path);
    }

    #region WriteMyLogs

    [Fact]
    public async Task WriteMyLogs_ValidArgs_EnqueuesAndFlushesToFile()
    {
        var msg = $"WriteMyLogs_Test_{Guid.NewGuid():N}";
        LocalLogHelper.WriteMyLogs("Info", msg);
        await LocalLogHelper.FlushAsync();

        var content = GetLatestLogContent();
        content.Should().Contain(msg);
        content.Should().Contain("Info");
    }

    #endregion

    #region WriteMyLogsAsync

    [Fact]
    public async Task WriteMyLogsAsync_ValidArgs_EnqueuesAndFlushesToFile()
    {
        var msg = $"WriteMyLogsAsync_Test_{Guid.NewGuid():N}";
        await LocalLogHelper.WriteMyLogsAsync("Debug", msg);
        await LocalLogHelper.FlushAsync();

        var content = GetLatestLogContent();
        content.Should().Contain(msg);
        content.Should().Contain("Debug");
    }

    #endregion

    #region FlushAsync

    [Fact]
    public async Task FlushAsync_EmptyQueue_DoesNotThrow()
    {
        var act = () => LocalLogHelper.FlushAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task FlushAsync_WithPendingLogs_WritesToFile()
    {
        var msg = $"FlushAsync_Test_{Guid.NewGuid():N}";
        await LocalLogHelper.WriteMyLogsAsync("Info", msg);
        await LocalLogHelper.FlushAsync();

        var content = GetLatestLogContent();
        content.Should().Contain(msg);
    }

    #endregion

    #region LogXxx - level filter (sync)

    [Fact]
    public async Task LogTrace_WhenMinimumLevelIsTrace_WritesLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.Trace;
        var msg = $"LogTrace_Test_{Guid.NewGuid():N}";

        LocalLogHelper.LogTrace(msg);

        await LocalLogHelper.FlushAsync();
        var content = GetLatestLogContent();
        content.Should().Contain(msg);
        content.Should().Contain("Trace");
    }

    [Fact]
    public async Task LogTrace_WhenMinimumLevelIsInformation_DoesNotWriteLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.Information;
        var msg = $"LogTrace_Filtered_{Guid.NewGuid():N}";

        LocalLogHelper.LogTrace(msg);

        await LocalLogHelper.FlushAsync();
        var content = GetLatestLogContent();
        content.Should().NotContain(msg);
    }

    [Fact]
    public async Task LogDebug_WhenMinimumLevelIsDebug_WritesLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.Debug;
        var msg = $"LogDebug_Test_{Guid.NewGuid():N}";

        LocalLogHelper.LogDebug(msg);

        await LocalLogHelper.FlushAsync();
        var content = GetLatestLogContent();
        content.Should().Contain(msg);
        content.Should().Contain("Debug");
    }

    [Fact]
    public async Task LogDebug_WhenMinimumLevelIsWarning_DoesNotWriteLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.Warning;
        var msg = $"LogDebug_Filtered_{Guid.NewGuid():N}";

        LocalLogHelper.LogDebug(msg);

        await LocalLogHelper.FlushAsync();
        var content = GetLatestLogContent();
        content.Should().NotContain(msg);
    }

    [Fact]
    public async Task LogInformation_WhenMinimumLevelIsInformation_WritesLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.Information;
        var msg = $"LogInformation_Test_{Guid.NewGuid():N}";

        LocalLogHelper.LogInformation(msg);

        await LocalLogHelper.FlushAsync();
        var content = GetLatestLogContent();
        content.Should().Contain(msg);
        content.Should().Contain("Information");
    }

    [Fact]
    public async Task LogInformation_WhenMinimumLevelIsError_DoesNotWriteLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.Error;
        var msg = $"LogInformation_Filtered_{Guid.NewGuid():N}";

        LocalLogHelper.LogInformation(msg);

        await LocalLogHelper.FlushAsync();
        var content = GetLatestLogContent();
        content.Should().NotContain(msg);
    }

    [Fact]
    public async Task LogWarning_WhenMinimumLevelIsWarning_WritesLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.Warning;
        var msg = $"LogWarning_Test_{Guid.NewGuid():N}";

        LocalLogHelper.LogWarning(msg);

        await LocalLogHelper.FlushAsync();
        var content = GetLatestLogContent();
        content.Should().Contain(msg);
        content.Should().Contain("Warning");
    }

    [Fact]
    public async Task LogWarning_WhenMinimumLevelIsCritical_DoesNotWriteLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.Critical;
        var msg = $"LogWarning_Filtered_{Guid.NewGuid():N}";

        LocalLogHelper.LogWarning(msg);

        await LocalLogHelper.FlushAsync();
        var content = GetLatestLogContent();
        content.Should().NotContain(msg);
    }

    [Fact]
    public async Task LogError_WhenMinimumLevelIsError_WritesLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.Error;
        var msg = $"LogError_Test_{Guid.NewGuid():N}";

        LocalLogHelper.LogError(msg);

        await LocalLogHelper.FlushAsync();
        var content = GetLatestLogContent();
        content.Should().Contain(msg);
        content.Should().Contain("Error");
    }

    [Fact]
    public async Task LogError_WhenMinimumLevelIsNone_DoesNotWriteLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.None;
        var msg = $"LogError_Filtered_{Guid.NewGuid():N}";

        LocalLogHelper.LogError(msg);

        await LocalLogHelper.FlushAsync();
        var content = GetLatestLogContent();
        content.Should().NotContain(msg);
    }

    [Fact]
    public async Task LogCritical_WhenMinimumLevelIsCritical_WritesLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.Critical;
        var msg = $"LogCritical_Test_{Guid.NewGuid():N}";

        LocalLogHelper.LogCritical(msg);

        await LocalLogHelper.FlushAsync();
        var content = GetLatestLogContent();
        content.Should().Contain(msg);
        content.Should().Contain("Critical");
    }

    #endregion

    #region LogXxxAsync - level filter

    [Fact]
    public async Task LogTraceAsync_WhenMinimumLevelIsTrace_WritesLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.Trace;
        var msg = $"LogTraceAsync_Test_{Guid.NewGuid():N}";

        await LocalLogHelper.LogTraceAsync(msg);
        await LocalLogHelper.FlushAsync();

        var content = GetLatestLogContent();
        content.Should().Contain(msg);
        content.Should().Contain("Trace");
    }

    [Fact]
    public async Task LogTraceAsync_WhenMinimumLevelIsInformation_DoesNotWriteLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.Information;
        var msg = $"LogTraceAsync_Filtered_{Guid.NewGuid():N}";

        await LocalLogHelper.LogTraceAsync(msg);
        await LocalLogHelper.FlushAsync();

        var content = GetLatestLogContent();
        content.Should().NotContain(msg);
    }

    [Fact]
    public async Task LogDebugAsync_WhenMinimumLevelIsDebug_WritesLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.Debug;
        var msg = $"LogDebugAsync_Test_{Guid.NewGuid():N}";

        await LocalLogHelper.LogDebugAsync(msg);
        await LocalLogHelper.FlushAsync();

        var content = GetLatestLogContent();
        content.Should().Contain(msg);
        content.Should().Contain("Debug");
    }

    [Fact]
    public async Task LogDebugAsync_WhenMinimumLevelIsWarning_DoesNotWriteLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.Warning;
        var msg = $"LogDebugAsync_Filtered_{Guid.NewGuid():N}";

        await LocalLogHelper.LogDebugAsync(msg);
        await LocalLogHelper.FlushAsync();

        var content = GetLatestLogContent();
        content.Should().NotContain(msg);
    }

    [Fact]
    public async Task LogInformationAsync_WhenMinimumLevelIsInformation_WritesLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.Information;
        var msg = $"LogInformationAsync_Test_{Guid.NewGuid():N}";

        await LocalLogHelper.LogInformationAsync(msg);
        await LocalLogHelper.FlushAsync();

        var content = GetLatestLogContent();
        content.Should().Contain(msg);
        content.Should().Contain("Information");
    }

    [Fact]
    public async Task LogInformationAsync_WhenMinimumLevelIsError_DoesNotWriteLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.Error;
        var msg = $"LogInformationAsync_Filtered_{Guid.NewGuid():N}";

        await LocalLogHelper.LogInformationAsync(msg);
        await LocalLogHelper.FlushAsync();

        var content = GetLatestLogContent();
        content.Should().NotContain(msg);
    }

    [Fact]
    public async Task LogWarningAsync_WhenMinimumLevelIsWarning_WritesLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.Warning;
        var msg = $"LogWarningAsync_Test_{Guid.NewGuid():N}";

        await LocalLogHelper.LogWarningAsync(msg);
        await LocalLogHelper.FlushAsync();

        var content = GetLatestLogContent();
        content.Should().Contain(msg);
        content.Should().Contain("Warning");
    }

    [Fact]
    public async Task LogWarningAsync_WhenMinimumLevelIsCritical_DoesNotWriteLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.Critical;
        var msg = $"LogWarningAsync_Filtered_{Guid.NewGuid():N}";

        await LocalLogHelper.LogWarningAsync(msg);
        await LocalLogHelper.FlushAsync();

        var content = GetLatestLogContent();
        content.Should().NotContain(msg);
    }

    [Fact]
    public async Task LogErrorAsync_WhenMinimumLevelIsError_WritesLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.Error;
        var msg = $"LogErrorAsync_Test_{Guid.NewGuid():N}";

        await LocalLogHelper.LogErrorAsync(msg);
        await LocalLogHelper.FlushAsync();

        var content = GetLatestLogContent();
        content.Should().Contain(msg);
        content.Should().Contain("Error");
    }

    [Fact]
    public async Task LogErrorAsync_WhenMinimumLevelIsNone_DoesNotWriteLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.None;
        var msg = $"LogErrorAsync_Filtered_{Guid.NewGuid():N}";

        await LocalLogHelper.LogErrorAsync(msg);
        await LocalLogHelper.FlushAsync();

        var content = GetLatestLogContent();
        content.Should().NotContain(msg);
    }

    [Fact]
    public async Task LogCriticalAsync_WhenMinimumLevelIsCritical_WritesLog()
    {
        CoreGlobalConfig.MinimumLevel = LogLevel.Critical;
        var msg = $"LogCriticalAsync_Test_{Guid.NewGuid():N}";

        await LocalLogHelper.LogCriticalAsync(msg);
        await LocalLogHelper.FlushAsync();

        var content = GetLatestLogContent();
        content.Should().Contain(msg);
        content.Should().Contain("Critical");
    }

    #endregion

    #region Log format

    [Fact]
    public async Task WriteMyLogsAsync_WritesCorrectFormat()
    {
        var msg = $"Format_Test_{Guid.NewGuid():N}";
        await LocalLogHelper.WriteMyLogsAsync("Info", msg);
        await LocalLogHelper.FlushAsync();

        var content = GetLatestLogContent();
        content.Should().Contain("==> Info\t");
    }

    [Fact]
    public async Task FlushAsync_CreatesLogDirectory()
    {
        await LocalLogHelper.WriteMyLogsAsync("Info", "dir_check");
        await LocalLogHelper.FlushAsync();

        Directory.Exists(_logPath).Should().BeTrue();
    }

    [Fact]
    public async Task FlushAsync_CreatesLogFileWithDateName()
    {
        var msg = $"DateName_Test_{Guid.NewGuid():N}";
        await LocalLogHelper.WriteMyLogsAsync("Info", msg);
        await LocalLogHelper.FlushAsync();

        var logFile = GetLatestLogFilePath();
        logFile.Should().NotBeNull();
        Path.GetFileNameWithoutExtension(logFile).Should().Be(DateTime.Now.ToString("yyyyMMdd"));
    }

    #endregion
}
