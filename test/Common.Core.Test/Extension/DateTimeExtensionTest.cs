using Azrng.Core.Enums;
using FluentAssertions;
using Xunit.Abstractions;

namespace Common.Core.Test.Extension;

/// <summary>
/// 时间扩展
/// </summary>
public class DateTimeExtensionTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public DateTimeExtensionTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// 获取网络时间
    /// </summary>
    [Fact]
    public async Task GetNetworkTime_ReturnOk()
    {
        var dateTime = DateTime.Now;
        var currTime = await DateTimeHelper.GetNetworkTime();
        currTime!.Value!.Hour.Should().BeLessOrEqualTo(dateTime.Hour);
    }

    /// <summary>
    /// 时间戳转时间
    /// </summary>
    [Fact]
    public void TimeSpanToDateTime_ReturnOk()
    {
        var ts = TimeSpan.FromHours(1);
        var dt = ts.ToDateTime();
        Assert.Equal(1, dt.Hour);

        var ts2 = dt.ToTimeSpan();
        Assert.True(ts == ts2);
    }

    #region 获取指定时间

    /// <summary>
    /// 获取指定时间周一的时间
    /// </summary>
    /// <returns></returns>
    [Fact]
    public void GetWeekOneDate_ReturnOk()
    {
        // 准备
        var date = new DateTime(2024, 08, 13, 12, 12,
            12);

        // 昨天开始
        var yesterdayStart = DateTimeHelper.GetTargetTimeStart(date, TimeType.Yesterday);
        Assert.Equal(new DateTime(2024, 08, 12), yesterdayStart);

        // 昨天结束
        var yesterdayEnd = DateTimeHelper.GetTargetTimeEnd(date, TimeType.Yesterday);
        Assert.Equal(new DateTime(2024, 08, 13).AddSeconds(-1), yesterdayEnd);

        // 今天开始
        var todayStart = DateTimeHelper.GetTargetTimeStart(date, TimeType.Today);
        Assert.Equal(new DateTime(2024, 08, 13), todayStart);

        // 今天结束
        var todayEnd = DateTimeHelper.GetTargetTimeEnd(date, TimeType.Today);
        Assert.Equal(new DateTime(2024, 08, 14).AddSeconds(-1), todayEnd);

        // 这周开始
        var weekOneDateStart = DateTimeHelper.GetTargetTimeStart(date, TimeType.Week);
        Assert.Equal(new DateTime(2024, 08, 12), weekOneDateStart);

        // 这周结束
        var weekOneDateEnd = DateTimeHelper.GetTargetTimeEnd(date, TimeType.Week);
        Assert.Equal(new DateTime(2024, 08, 19).AddSeconds(-1), weekOneDateEnd);

        // 这月开始
        var currentMonthStart = DateTimeHelper.GetTargetTimeStart(date, TimeType.CurrentMonth);
        Assert.Equal(new DateTime(2024, 08, 1), currentMonthStart);

        // 这个月结束
        var currentMonthEnd = DateTimeHelper.GetTargetTimeEnd(date, TimeType.CurrentMonth);
        Assert.Equal(new DateTime(2024, 09, 1).AddSeconds(-1), currentMonthEnd);

        // 下个月开始
        var nextMonthStart = DateTimeHelper.GetTargetTimeStart(date, TimeType.NextMonth);
        Assert.Equal(new DateTime(2024, 09, 1), nextMonthStart);

        // 下个月结束
        var nextMonthEnd = DateTimeHelper.GetTargetTimeEnd(date, TimeType.NextMonth);
        Assert.Equal(new DateTime(2024, 10, 1).AddSeconds(-1), nextMonthEnd);

        // 这个季节开始
        var seasonStart = DateTimeHelper.GetTargetTimeStart(date, TimeType.Season);
        Assert.Equal(new DateTime(2024, 7, 1), seasonStart);

        // 这个季节结束
        var seasonEnd = DateTimeHelper.GetTargetTimeEnd(date, TimeType.Season);
        Assert.Equal(new DateTime(2024, 10, 1).AddSeconds(-1), seasonEnd);

        // 这年开始
        var yearStart = DateTimeHelper.GetTargetTimeStart(date, TimeType.Year);
        Assert.Equal(new DateTime(2024, 1, 1), yearStart);

        // 这年结束
        var yearEnd = DateTimeHelper.GetTargetTimeEnd(date, TimeType.Year);
        Assert.Equal(new DateTime(2025, 1, 1).AddSeconds(-1), yearEnd);
    }

    #endregion


}