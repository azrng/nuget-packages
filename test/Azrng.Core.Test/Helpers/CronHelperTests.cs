using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class CronHelperTests
{
    [Fact]
    public void Minute_DefaultInterval_ReturnsEveryMinuteCron()
    {
        var result = CronHelper.Minute();

        result.Should().Be("1 0/1 * * * ? ");
    }

    [Theory]
    [InlineData(1, "1 0/1 * * * ? ")]
    [InlineData(5, "1 0/5 * * * ? ")]
    [InlineData(10, "1 0/10 * * * ? ")]
    [InlineData(30, "1 0/30 * * * ? ")]
    public void Minute_CustomInterval_ReturnsCorrectCron(int interval, string expected)
    {
        var result = CronHelper.Minute(interval);

        result.Should().Be(expected);
    }

    [Fact]
    public void Hour_DefaultParams_ReturnsEveryHourCron()
    {
        var result = CronHelper.Hour();

        result.Should().Be("1 1 0/1 * * ? ");
    }

    [Theory]
    [InlineData(1, 1, "1 1 0/1 * * ? ")]
    [InlineData(30, 2, "1 30 0/2 * * ? ")]
    [InlineData(0, 5, "1 0 0/5 * * ? ")]
    [InlineData(59, 10, "1 59 0/10 * * ? ")]
    public void Hour_CustomParams_ReturnsCorrectCron(int minute, int interval, string expected)
    {
        var result = CronHelper.Hour(minute, interval);

        result.Should().Be(expected);
    }

    [Fact]
    public void Day_DefaultParams_ReturnsEveryDayCron()
    {
        var result = CronHelper.Day();

        result.Should().Be("1 1 1 1/1 * ? ");
    }

    [Theory]
    [InlineData(1, 1, 1, "1 1 1 1/1 * ? ")]
    [InlineData(8, 30, 2, "1 30 8 1/2 * ? ")]
    [InlineData(0, 0, 7, "1 0 0 1/7 * ? ")]
    [InlineData(23, 59, 30, "1 59 23 1/30 * ? ")]
    public void Day_CustomParams_ReturnsCorrectCron(int hour, int minute, int interval, string expected)
    {
        var result = CronHelper.Day(hour, minute, interval);

        result.Should().Be(expected);
    }

    [Fact]
    public void Week_DefaultParams_ReturnsMondayCron()
    {
        var result = CronHelper.Week();

        result.Should().Be("1 1 * * 1");
    }

    [Theory]
    [InlineData(DayOfWeek.Monday, 1, 1, "1 1 * * 1")]
    [InlineData(DayOfWeek.Sunday, 8, 30, "30 8 * * 0")]
    [InlineData(DayOfWeek.Friday, 23, 59, "59 23 * * 5")]
    [InlineData(DayOfWeek.Wednesday, 0, 0, "0 0 * * 3")]
    [InlineData(DayOfWeek.Saturday, 12, 15, "15 12 * * 6")]
    public void Week_CustomParams_ReturnsCorrectCron(DayOfWeek dayOfWeek, int hour, int minute, string expected)
    {
        var result = CronHelper.Week(dayOfWeek, hour, minute);

        result.Should().Be(expected);
    }

    [Fact]
    public void Month_DefaultParams_ReturnsFirstDayOfMonthCron()
    {
        var result = CronHelper.Month();

        result.Should().Be("1 1 1 * *");
    }

    [Theory]
    [InlineData(1, 1, 1, "1 1 1 * *")]
    [InlineData(15, 8, 30, "30 8 15 * *")]
    [InlineData(28, 23, 59, "59 23 28 * *")]
    [InlineData(31, 0, 0, "0 0 31 * *")]
    public void Month_CustomParams_ReturnsCorrectCron(int day, int hour, int minute, string expected)
    {
        var result = CronHelper.Month(day, hour, minute);

        result.Should().Be(expected);
    }

    [Fact]
    public void Year_DefaultParams_ReturnsJanuaryFirstCron()
    {
        var result = CronHelper.Year();

        result.Should().Be("1 1 1 1 *");
    }

    [Theory]
    [InlineData(1, 1, 1, 1, "1 1 1 1 *")]
    [InlineData(6, 15, 8, 30, "30 8 15 6 *")]
    [InlineData(12, 31, 23, 59, "59 23 31 12 *")]
    [InlineData(3, 1, 0, 0, "0 0 1 3 *")]
    public void Year_CustomParams_ReturnsCorrectCron(int month, int day, int hour, int minute, string expected)
    {
        var result = CronHelper.Year(month, day, hour, minute);

        result.Should().Be(expected);
    }
}
