using Azrng.Core.Enums;
using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class DateTimeHelperTests
{
    #region GetNetworkTime

    [Fact]
    public async Task GetNetworkTime_ShouldNotThrow()
    {
        var action = async () => await DateTimeHelper.GetNetworkTime();

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetNetworkTime_ShouldReturnNullOrValidDateTime()
    {
        var result = await DateTimeHelper.GetNetworkTime();

        if (result.HasValue)
        {
            result.Value.Year.Should().BeGreaterOrEqualTo(2020);
            result.Value.Year.Should().BeLessOrEqualTo(2099);
        }
    }

    #endregion

    #region GetTargetTimeStart

    [Fact]
    public void GetTargetTimeStart_Today_ShouldReturnDateOnly()
    {
        var now = new DateTime(2026, 6, 26, 14, 30, 45);

        var result = DateTimeHelper.GetTargetTimeStart(now, TimeType.Today);

        result.Should().Be(new DateTime(2026, 6, 26));
    }

    [Fact]
    public void GetTargetTimeStart_Yesterday_ShouldReturnPreviousDay()
    {
        var now = new DateTime(2026, 6, 26, 14, 30, 0);

        var result = DateTimeHelper.GetTargetTimeStart(now, TimeType.Yesterday);

        result.Should().Be(new DateTime(2026, 6, 25));
    }

    [Fact]
    public void GetTargetTimeStart_Tomorrow_ShouldReturnNextDay()
    {
        var now = new DateTime(2026, 6, 26, 14, 30, 0);

        var result = DateTimeHelper.GetTargetTimeStart(now, TimeType.Tomorrow);

        result.Should().Be(new DateTime(2026, 6, 27));
    }

    [Fact]
    public void GetTargetTimeStart_Week_ShouldReturnMonday()
    {
        // 2026-06-26 is Friday (DayOfWeek=5)
        var now = new DateTime(2026, 6, 26, 14, 30, 0);

        var result = DateTimeHelper.GetTargetTimeStart(now, TimeType.Week);

        // Monday of that week is 2026-06-22
        result.Should().Be(new DateTime(2026, 6, 22));
        result.DayOfWeek.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void GetTargetTimeStart_Week_WhenMonday_ShouldReturnSameDay()
    {
        // 2026-06-22 is Monday (DayOfWeek=0)
        var now = new DateTime(2026, 6, 22, 10, 0, 0);

        var result = DateTimeHelper.GetTargetTimeStart(now, TimeType.Week);

        result.Should().Be(new DateTime(2026, 6, 22));
    }

    [Fact]
    public void GetTargetTimeStart_Week_WhenSunday_ShouldReturnPreviousMonday()
    {
        // 2026-06-28 is Sunday (DayOfWeek=6)
        var now = new DateTime(2026, 6, 28, 10, 0, 0);

        var result = DateTimeHelper.GetTargetTimeStart(now, TimeType.Week);

        // DayOfWeek: Sunday=6, 0 - 6 + 1 = -5, 28 + (-5) = 23
        result.Should().Be(new DateTime(2026, 6, 22));
        result.DayOfWeek.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void GetTargetTimeStart_CurrentMonth_ShouldReturnFirstDayOfMonth()
    {
        var now = new DateTime(2026, 6, 15, 10, 0, 0);

        var result = DateTimeHelper.GetTargetTimeStart(now, TimeType.CurrentMonth);

        result.Should().Be(new DateTime(2026, 6, 1));
    }

    [Fact]
    public void GetTargetTimeStart_CurrentMonth_WhenFirstDay_ShouldReturnSameDay()
    {
        var now = new DateTime(2026, 6, 1, 10, 0, 0);

        var result = DateTimeHelper.GetTargetTimeStart(now, TimeType.CurrentMonth);

        result.Should().Be(new DateTime(2026, 6, 1));
    }

    [Fact]
    public void GetTargetTimeStart_NextMonth_ShouldReturnFirstDayOfNextMonth()
    {
        var now = new DateTime(2026, 6, 15, 10, 0, 0);

        var result = DateTimeHelper.GetTargetTimeStart(now, TimeType.NextMonth);

        result.Should().Be(new DateTime(2026, 7, 1));
    }

    [Fact]
    public void GetTargetTimeStart_NextMonth_December_ShouldReturnJanuaryNextYear()
    {
        var now = new DateTime(2026, 12, 10, 10, 0, 0);

        var result = DateTimeHelper.GetTargetTimeStart(now, TimeType.NextMonth);

        result.Should().Be(new DateTime(2027, 1, 1));
    }

    [Theory]
    [InlineData(1, 1)]   // Jan -> Q1 starts Jan 1
    [InlineData(2, 1)]   // Feb -> Q1 starts Jan 1
    [InlineData(3, 1)]   // Mar -> Q1 starts Jan 1
    [InlineData(4, 4)]   // Apr -> Q2 starts Apr 1
    [InlineData(5, 4)]   // May -> Q2 starts Apr 1
    [InlineData(6, 4)]   // Jun -> Q2 starts Apr 1
    [InlineData(7, 7)]   // Jul -> Q3 starts Jul 1
    [InlineData(8, 7)]   // Aug -> Q3 starts Jul 1
    [InlineData(9, 7)]   // Sep -> Q3 starts Jul 1
    [InlineData(10, 10)] // Oct -> Q4 starts Oct 1
    [InlineData(11, 10)] // Nov -> Q4 starts Oct 1
    [InlineData(12, 10)] // Dec -> Q4 starts Oct 1
    public void GetTargetTimeStart_Season_ShouldReturnFirstDayOfQuarter(int month, int expectedStartMonth)
    {
        var now = new DateTime(2026, month, 15, 10, 0, 0);

        var result = DateTimeHelper.GetTargetTimeStart(now, TimeType.Season);

        result.Should().Be(new DateTime(2026, expectedStartMonth, 1));
    }

    [Fact]
    public void GetTargetTimeStart_Year_ShouldReturnFirstDayOfYear()
    {
        var now = new DateTime(2026, 6, 26, 10, 0, 0);

        var result = DateTimeHelper.GetTargetTimeStart(now, TimeType.Year);

        result.Should().Be(new DateTime(2026, 1, 1));
    }

    [Fact]
    public void GetTargetTimeStart_Year_WhenJan1_ShouldReturnSameDay()
    {
        var now = new DateTime(2026, 1, 1, 10, 0, 0);

        var result = DateTimeHelper.GetTargetTimeStart(now, TimeType.Year);

        result.Should().Be(new DateTime(2026, 1, 1));
    }

    [Fact]
    public void GetTargetTimeStart_DefaultParameter_ShouldReturnToday()
    {
        var now = new DateTime(2026, 6, 26, 14, 30, 0);

        var result = DateTimeHelper.GetTargetTimeStart(now);

        result.Should().Be(new DateTime(2026, 6, 26));
    }

    #endregion

    #region GetTargetTimeEnd

    [Fact]
    public void GetTargetTimeEnd_Today_ShouldReturnEndOfDay()
    {
        var now = new DateTime(2026, 6, 26, 14, 30, 45);

        var result = DateTimeHelper.GetTargetTimeEnd(now, TimeType.Today);

        result.Should().Be(new DateTime(2026, 6, 26, 23, 59, 59));
    }

    [Fact]
    public void GetTargetTimeEnd_Yesterday_ShouldReturnYesterdayEnd()
    {
        var now = new DateTime(2026, 6, 26, 14, 30, 0);

        var result = DateTimeHelper.GetTargetTimeEnd(now, TimeType.Yesterday);

        result.Should().Be(new DateTime(2026, 6, 25, 23, 59, 59));
    }

    [Fact]
    public void GetTargetTimeEnd_Tomorrow_ShouldReturnTomorrowEnd()
    {
        var now = new DateTime(2026, 6, 26, 14, 30, 0);

        var result = DateTimeHelper.GetTargetTimeEnd(now, TimeType.Tomorrow);

        result.Should().Be(new DateTime(2026, 6, 27, 23, 59, 59));
    }

    [Fact]
    public void GetTargetTimeEnd_Week_ShouldReturnEndOfSunday()
    {
        // 2026-06-26 is Friday (DayOfWeek=5)
        var now = new DateTime(2026, 6, 26, 14, 30, 0);

        var result = DateTimeHelper.GetTargetTimeEnd(now, TimeType.Week);

        // End of week = Sunday 2026-06-28 23:59:59
        result.Should().Be(new DateTime(2026, 6, 28, 23, 59, 59));
    }

    [Fact]
    public void GetTargetTimeEnd_Week_WhenSunday_ShouldReturnEndOfSameSunday()
    {
        var now = new DateTime(2026, 6, 28, 10, 0, 0);

        var result = DateTimeHelper.GetTargetTimeEnd(now, TimeType.Week);

        result.Should().Be(new DateTime(2026, 6, 28, 23, 59, 59));
    }

    [Fact]
    public void GetTargetTimeEnd_CurrentMonth_ShouldReturnEndOfMonth()
    {
        var now = new DateTime(2026, 6, 15, 10, 0, 0);

        var result = DateTimeHelper.GetTargetTimeEnd(now, TimeType.CurrentMonth);

        result.Should().Be(new DateTime(2026, 6, 30, 23, 59, 59));
    }

    [Fact]
    public void GetTargetTimeEnd_CurrentMonth_FebruaryNonLeap_ShouldReturnEndOfFeb()
    {
        var now = new DateTime(2026, 2, 10, 10, 0, 0);

        var result = DateTimeHelper.GetTargetTimeEnd(now, TimeType.CurrentMonth);

        result.Should().Be(new DateTime(2026, 2, 28, 23, 59, 59));
    }

    [Fact]
    public void GetTargetTimeEnd_CurrentMonth_FebruaryLeap_ShouldReturnEndOfFebLeap()
    {
        var now = new DateTime(2028, 2, 10, 10, 0, 0);

        var result = DateTimeHelper.GetTargetTimeEnd(now, TimeType.CurrentMonth);

        result.Should().Be(new DateTime(2028, 2, 29, 23, 59, 59));
    }

    [Fact]
    public void GetTargetTimeEnd_NextMonth_ShouldReturnEndOfNextMonth()
    {
        var now = new DateTime(2026, 6, 15, 10, 0, 0);

        var result = DateTimeHelper.GetTargetTimeEnd(now, TimeType.NextMonth);

        result.Should().Be(new DateTime(2026, 7, 31, 23, 59, 59));
    }

    [Fact]
    public void GetTargetTimeEnd_NextMonth_December_ShouldReturnEndOfJanuary()
    {
        var now = new DateTime(2026, 12, 10, 10, 0, 0);

        var result = DateTimeHelper.GetTargetTimeEnd(now, TimeType.NextMonth);

        result.Should().Be(new DateTime(2027, 1, 31, 23, 59, 59));
    }

    [Theory]
    [InlineData(1, 3)]    // Q1 ends Mar 31
    [InlineData(2, 3)]    // Q1 ends Mar 31
    [InlineData(3, 3)]    // Q1 ends Mar 31
    [InlineData(4, 6)]    // Q2 ends Jun 30
    [InlineData(5, 6)]    // Q2 ends Jun 30
    [InlineData(6, 6)]    // Q2 ends Jun 30
    [InlineData(7, 9)]    // Q3 ends Sep 30
    [InlineData(8, 9)]    // Q3 ends Sep 30
    [InlineData(9, 9)]    // Q3 ends Sep 30
    [InlineData(10, 12)]  // Q4 ends Dec 31
    [InlineData(11, 12)]  // Q4 ends Dec 31
    [InlineData(12, 12)]  // Q4 ends Dec 31
    public void GetTargetTimeEnd_Season_ShouldReturnEndOfQuarter(int month, int expectedEndMonth)
    {
        var now = new DateTime(2026, month, 15, 10, 0, 0);

        var result = DateTimeHelper.GetTargetTimeEnd(now, TimeType.Season);

        var expected = new DateTime(2026, expectedEndMonth, 1).AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
        result.Should().Be(expected);
    }

    [Fact]
    public void GetTargetTimeEnd_Year_ShouldReturnEndOfYear()
    {
        var now = new DateTime(2026, 6, 26, 10, 0, 0);

        var result = DateTimeHelper.GetTargetTimeEnd(now, TimeType.Year);

        result.Should().Be(new DateTime(2026, 12, 31, 23, 59, 59));
    }

    [Fact]
    public void GetTargetTimeEnd_DefaultParameter_ShouldReturnTodayEnd()
    {
        var now = new DateTime(2026, 6, 26, 14, 30, 0);

        var result = DateTimeHelper.GetTargetTimeEnd(now);

        result.Should().Be(new DateTime(2026, 6, 26, 23, 59, 59));
    }

    [Fact]
    public void GetTargetTimeEnd_Year_Dec31_ShouldReturnSameDayEnd()
    {
        var now = new DateTime(2026, 12, 31, 10, 0, 0);

        var result = DateTimeHelper.GetTargetTimeEnd(now, TimeType.Year);

        result.Should().Be(new DateTime(2026, 12, 31, 23, 59, 59));
    }

    #endregion

    #region GetTargetTimeStart/End Consistency

    [Fact]
    public void GetTargetTimeStartEnd_Today_StartShouldBeBeforeEnd()
    {
        var now = new DateTime(2026, 6, 26, 14, 30, 0);

        var start = DateTimeHelper.GetTargetTimeStart(now, TimeType.Today);
        var end = DateTimeHelper.GetTargetTimeEnd(now, TimeType.Today);

        start.Should().BeBefore(end);
    }

    [Theory]
    [InlineData(TimeType.Yesterday)]
    [InlineData(TimeType.Today)]
    [InlineData(TimeType.Tomorrow)]
    [InlineData(TimeType.Week)]
    [InlineData(TimeType.CurrentMonth)]
    [InlineData(TimeType.NextMonth)]
    [InlineData(TimeType.Season)]
    [InlineData(TimeType.Year)]
    public void GetTargetTimeStartEnd_AllTypes_StartShouldBeBeforeEnd(TimeType timeType)
    {
        var now = new DateTime(2026, 6, 26, 14, 30, 0);

        var start = DateTimeHelper.GetTargetTimeStart(now, timeType);
        var end = DateTimeHelper.GetTargetTimeEnd(now, timeType);

        start.Should().BeBefore(end);
    }

    [Theory]
    [InlineData(TimeType.Yesterday)]
    [InlineData(TimeType.Today)]
    [InlineData(TimeType.Tomorrow)]
    [InlineData(TimeType.Week)]
    [InlineData(TimeType.CurrentMonth)]
    [InlineData(TimeType.NextMonth)]
    [InlineData(TimeType.Season)]
    [InlineData(TimeType.Year)]
    public void GetTargetTimeEnd_ShouldBeEndOfPeriod(TimeType timeType)
    {
        var now = new DateTime(2026, 6, 26, 14, 30, 0);

        var end = DateTimeHelper.GetTargetTimeEnd(now, timeType);

        end.Second.Should().Be(59);
        end.Millisecond.Should().Be(0);
    }

    #endregion

    #region GetDateIntervalStr

    [Fact]
    public void GetDateIntervalStr_MoreThan1Day_ShouldReturnMonthAndDay()
    {
        var start = new DateTime(2026, 6, 20, 10, 0, 0);
        var end = new DateTime(2026, 6, 22, 10, 0, 0);

        var result = DateTimeHelper.GetDateIntervalStr(start, end);

        result.Should().Be("6月20日");
    }

    [Fact]
    public void GetDateIntervalStr_Exact1Day_ShouldReturnMonthAndDay()
    {
        var start = new DateTime(2026, 6, 20, 10, 0, 0);
        var end = new DateTime(2026, 6, 21, 10, 0, 0);

        var result = DateTimeHelper.GetDateIntervalStr(start, end);

        result.Should().Be("6月20日");
    }

    [Fact]
    public void GetDateIntervalStr_MoreThan1Hour_ShouldReturnHoursAgo()
    {
        var start = new DateTime(2026, 6, 26, 10, 0, 0);
        var end = new DateTime(2026, 6, 26, 12, 30, 0);

        var result = DateTimeHelper.GetDateIntervalStr(start, end);

        result.Should().Be("2小时前");
    }

    [Fact]
    public void GetDateIntervalStr_1HourOrLess_ShouldReturnMinutesAgo()
    {
        var start = new DateTime(2026, 6, 26, 12, 0, 0);
        var end = new DateTime(2026, 6, 26, 12, 30, 0);

        var result = DateTimeHelper.GetDateIntervalStr(start, end);

        result.Should().Be("30分钟前");
    }

    [Fact]
    public void GetDateIntervalStr_LessThan1Hour_ShouldReturnMinutesAgo()
    {
        var start = new DateTime(2026, 6, 26, 12, 0, 0);
        var end = new DateTime(2026, 6, 26, 12, 5, 0);

        var result = DateTimeHelper.GetDateIntervalStr(start, end);

        result.Should().Be("5分钟前");
    }

    [Fact]
    public void GetDateIntervalStr_SameTime_ShouldReturn0MinutesAgo()
    {
        var time = new DateTime(2026, 6, 26, 12, 0, 0);

        var result = DateTimeHelper.GetDateIntervalStr(time, time);

        result.Should().Be("0分钟前");
    }

    #endregion

    #region GetDateInterval

    [Fact]
    public void GetDateInterval_SameTime_ShouldReturnZeroValues()
    {
        var time = new DateTime(2026, 6, 26, 12, 0, 0);

        var result = DateTimeHelper.GetDateInterval(time, time);

        result.days.Should().Be(0);
        result.hours.Should().Be(0);
        result.minutes.Should().Be(0);
        result.seconds.Should().Be(0);
    }

    [Fact]
    public void GetDateInterval_1Day2Hours3Minutes4Seconds_ShouldReturnCorrectValues()
    {
        var start = new DateTime(2026, 6, 26, 12, 0, 0);
        var end = new DateTime(2026, 6, 27, 14, 3, 4);

        var result = DateTimeHelper.GetDateInterval(start, end);

        result.days.Should().Be(1);
        result.hours.Should().Be(2);
        result.minutes.Should().Be(3);
        result.seconds.Should().Be(4);
    }

    [Fact]
    public void GetDateInterval_ReverseOrder_ShouldReturnSameAbsoluteValues()
    {
        var time1 = new DateTime(2026, 6, 26, 12, 0, 0);
        var time2 = new DateTime(2026, 6, 27, 14, 3, 4);

        var result1 = DateTimeHelper.GetDateInterval(time1, time2);
        var result2 = DateTimeHelper.GetDateInterval(time2, time1);

        result1.Should().Be(result2);
    }

    [Fact]
    public void GetDateInterval_OnlyHoursDifference()
    {
        var start = new DateTime(2026, 6, 26, 10, 0, 0);
        var end = new DateTime(2026, 6, 26, 15, 30, 0);

        var result = DateTimeHelper.GetDateInterval(start, end);

        result.days.Should().Be(0);
        result.hours.Should().Be(5);
        result.minutes.Should().Be(30);
        result.seconds.Should().Be(0);
    }

    [Fact]
    public void GetDateInterval_1SecondDifference()
    {
        var start = new DateTime(2026, 6, 26, 12, 0, 0);
        var end = new DateTime(2026, 6, 26, 12, 0, 1);

        var result = DateTimeHelper.GetDateInterval(start, end);

        result.days.Should().Be(0);
        result.hours.Should().Be(0);
        result.minutes.Should().Be(0);
        result.seconds.Should().Be(1);
    }

    [Fact]
    public void GetDateInterval_MultipleDaysDifference()
    {
        var start = new DateTime(2026, 1, 1, 0, 0, 0);
        var end = new DateTime(2026, 12, 31, 23, 59, 59);

        var result = DateTimeHelper.GetDateInterval(start, end);

        result.days.Should().Be(364);
        result.hours.Should().Be(23);
        result.minutes.Should().Be(59);
        result.seconds.Should().Be(59);
    }

    #endregion

    #region GetTimeDifferenceText

    [Fact]
    public void GetTimeDifferenceText_JustNow_ShouldReturnJustNow()
    {
        var helper = new DateTimeHelper();
        var now = DateTime.Now;

        var result = helper.GetTimeDifferenceText(now);

        result.Should().Be("刚刚");
    }

    [Fact]
    public void GetTimeDifferenceText_30MinutesAgo_ShouldReturnMinutesAgo()
    {
        var helper = new DateTimeHelper();
        var time = DateTime.Now.AddMinutes(-30);

        var result = helper.GetTimeDifferenceText(time);

        result.Should().Be("30分钟前");
    }

    [Fact]
    public void GetTimeDifferenceText_5HoursAgo_ShouldReturnHoursAgo()
    {
        var helper = new DateTimeHelper();
        var time = DateTime.Now.AddHours(-5);

        var result = helper.GetTimeDifferenceText(time);

        result.Should().Be("5小时前");
    }

    [Fact]
    public void GetTimeDifferenceText_10DaysAgo_ShouldReturnDaysAgo()
    {
        var helper = new DateTimeHelper();
        var time = DateTime.Now.AddDays(-10);

        var result = helper.GetTimeDifferenceText(time);

        result.Should().Be("10天前");
    }

    [Fact]
    public void GetTimeDifferenceText_60DaysAgo_ShouldReturnMonthsAgo()
    {
        var helper = new DateTimeHelper();
        var time = DateTime.Now.AddDays(-60);

        var result = helper.GetTimeDifferenceText(time);

        result.Should().Be("2月前");
    }

    [Fact]
    public void GetTimeDifferenceText_MoreThan1YearAgo_ShouldReturnYearAndMonth()
    {
        var helper = new DateTimeHelper();
        var time = DateTime.Now.AddYears(-2);

        var result = helper.GetTimeDifferenceText(time);

        result.Should().Contain("年");
        result.Should().Contain("月");
    }

    [Fact]
    public void GetTimeDifferenceText_MoreThan1YearAgo_ShouldFormatMonthWithPadding()
    {
        var helper = new DateTimeHelper();
        // Use a fixed date that's clearly > 1 year ago
        var time = new DateTime(2024, 3, 15, 10, 0, 0);

        var result = helper.GetTimeDifferenceText(time);

        result.Should().Be("2024年03月");
    }

    #endregion
}
