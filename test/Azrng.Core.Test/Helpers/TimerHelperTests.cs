using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class TimerHelperTests
{
    [Fact]
    public void Measure_ShouldReturnPositiveTimeSpan()
    {
        var result = TimerHelper.Measure(() => { });

        result.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public void Measure_ShouldExecuteAction()
    {
        var executed = false;

        TimerHelper.Measure(() => executed = true);

        executed.Should().BeTrue();
    }

    [Fact]
    public void Measure_ShouldMeasureElapsedTime()
    {
        var result = TimerHelper.Measure(() => Thread.Sleep(50));

        result.TotalMilliseconds.Should().BeGreaterOrEqualTo(40);
    }

    [Fact]
    public void MeasureAndPrint_ShouldExecuteAction()
    {
        var executed = false;
        var writer = new StringWriter();
        Console.SetOut(writer);

        TimerHelper.MeasureAndPrint(() => executed = true, "test");

        executed.Should().BeTrue();
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
    }

    [Fact]
    public void MeasureAndPrint_ShouldWriteToConsole()
    {
        var writer = new StringWriter();
        Console.SetOut(writer);

        TimerHelper.MeasureAndPrint(() => { }, "MyTask");

        var output = writer.ToString();
        output.Should().Contain("MyTask");
        output.Should().Contain("ms");
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
    }

    [Fact]
    public void MeasureAndSave_ShouldExecuteAction()
    {
        var executed = false;
        var tempFile = Path.GetTempFileName();

        try
        {
            TimerHelper.MeasureAndSave(() => executed = true, tempFile);

            executed.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void MeasureAndSave_ShouldWriteToFile()
    {
        var tempFile = Path.GetTempFileName();

        try
        {
            TimerHelper.MeasureAndSave(() => { }, tempFile);

            var content = File.ReadAllText(tempFile);
            double.TryParse(content, out var ms).Should().BeTrue();
            ms.Should().BeGreaterOrEqualTo(0);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void MeasureAndLog_ShouldExecuteAction()
    {
        var executed = false;
        var tempFile = Path.GetTempFileName();

        try
        {
            TimerHelper.MeasureAndLog(() => executed = true, tempFile);

            executed.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void MeasureAndLog_ShouldAppendToFile()
    {
        var tempFile = Path.GetTempFileName();

        try
        {
            TimerHelper.MeasureAndLog(() => { }, tempFile);
            TimerHelper.MeasureAndLog(() => { }, tempFile);

            var lines = File.ReadAllLines(tempFile);
            lines.Should().HaveCount(2);
            lines[0].Should().Contain("ms");
            lines[1].Should().Contain("ms");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void MeasureAndLog_ShouldContainTimestamp()
    {
        var tempFile = Path.GetTempFileName();

        try
        {
            TimerHelper.MeasureAndLog(() => { }, tempFile);

            var content = File.ReadAllText(tempFile);
            content.Should().Contain(DateTime.Now.ToString("yyyy"));
            content.Should().Contain("ms");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void GetNetworkTime_ShouldReturnReasonableDateTime()
    {
        var networkTime = TimerHelper.GetNetworkTime();

        var now = DateTimeOffset.UtcNow;
        networkTime.Should().BeAfter(now.AddMinutes(-5));
        networkTime.Should().BeBefore(now.AddMinutes(5));
    }
}
