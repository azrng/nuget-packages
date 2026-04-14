using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class DateTimeExtensionTests
{
    [Fact]
    public void FormattingHelpers_ShouldReturnExpectedStrings()
    {
        var date = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Local);

        date.ToStandardString().Should().Be("2024-01-02 03:04:05");
        date.ToDateString().Should().Be("2024-01-02");
        date.ToFormatString("yyyy/MM/dd").Should().Be("2024/01/02");
        ((DateTime?)date).ToStandardString().Should().Be("2024-01-02 03:04:05");
        ((DateTime?)null).ToDateString().Should().BeEmpty();
    }

    [Fact]
    public void TimestampRoundTrip_ShouldRestoreLocalDateTime()
    {
        var date = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero).LocalDateTime;

        var milliseconds = date.ToTimestamp();
        var seconds = date.ToTimestamp(isSecond: true);

        milliseconds.ToDateTime().Should().BeCloseTo(date, TimeSpan.FromSeconds(1));
        seconds.ToDateTime(isSecond: true).Should().BeCloseTo(date, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void TimeSpanConversion_ShouldRoundTrip()
    {
        var date = new DateTime(2024, 5, 6, 7, 8, 9);

        var timeSpan = date.ToTimeSpan();

        timeSpan.ToDateTime().Should().Be(date);
    }

    [Fact]
    public void CalendarHelpers_ShouldReturnExpectedValues()
    {
        new DateTime(2024, 1, 6).IsWeekend().Should().BeTrue();
        new DateTime(2024, 1, 8).IsWeekday().Should().BeTrue();
        new DateTime(2024, 5, 20).GetQuarter().Should().Be(2);
        new DateTime(2024, 2, 1).GetCurrentMonthDayNumber().Should().Be(29);
    }

    [Theory]
    [InlineData(2024, 1, 1, 1, 1)]
    [InlineData(2024, 1, 8, 2, 2)]
    [InlineData(2024, 1, 15, 3, 3)]
    public void GetWeekOfMonth_ShouldSupportBothModes(int year, int month, int day, int mode1, int mode2)
    {
        var date = new DateTime(year, month, day);

        date.GetWeekOfMonth(1).Should().Be(mode1);
        date.GetWeekOfMonth(2).Should().Be(mode2);
    }

    [Fact]
    public void DateDiff_ShouldReturnCeilingOfAbsoluteDays()
    {
        var start = new DateTime(2024, 1, 1, 0, 0, 0);
        var end = start.AddHours(25);

        start.DateDiff(end).Should().Be(2);
        ((DateTime?)start).DateDiff((DateTime?)end).Should().Be(2);
        ((DateTime?)null).DateDiff(end).Should().Be(0);
    }
}
