namespace Common.Core.Test.Extension;

/// <summary>
/// DateTimeExtension DateTime扩展方法的单元测试
/// </summary>
public class DateTimeExtensionTest
{
    #region 格式化时间 Tests

    /// <summary>
    /// 测试ToStandardString方法：DateTime转标准时间字符串
    /// </summary>
    [Fact]
    public void ToStandardString_ValidDateTime_ReturnsFormattedString()
    {
        // Arrange
        var dateTime = new DateTime(2019, 1, 21, 20, 57, 51);

        // Act
        var result = dateTime.ToStandardString();

        // Assert
        Assert.Equal("2019-01-21 20:57:51", result);
    }

    /// <summary>
    /// 测试ToStandardString方法：可空DateTime为null时应返回空字符串
    /// </summary>
    [Fact]
    public void ToStandardString_NullDateTime_ReturnsEmptyString()
    {
        // Arrange
        DateTime? dateTime = null;

        // Act
        var result = dateTime.ToStandardString();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    /// <summary>
    /// 测试ToDetailedTimeString方法：DateTime转详细时间字符串
    /// </summary>
    [Fact]
    public void ToDetailedTimeString_ValidDateTime_ReturnsDetailedString()
    {
        // Arrange
        var dateTime = new DateTime(2019, 1, 21, 20, 57, 51, 123);

        // Act
        var result = dateTime.ToDetailedTimeString();

        // Assert
        Assert.Equal("2019-01-21 20:57:51.1230000", result);
    }

    /// <summary>
    /// 测试ToDetailedTimeString方法：可空DateTime为null时应返回空字符串
    /// </summary>
    [Fact]
    public void ToDetailedTimeString_NullDateTime_ReturnsEmptyString()
    {
        // Arrange
        DateTime? dateTime = null;

        // Act
        var result = dateTime.ToDetailedTimeString();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    /// <summary>
    /// 测试ToDateString方法：DateTime转日期字符串
    /// </summary>
    [Fact]
    public void ToDateString_ValidDateTime_ReturnsDateString()
    {
        // Arrange
        var dateTime = new DateTime(2019, 1, 21);

        // Act
        var result = dateTime.ToDateString();

        // Assert
        Assert.Equal("2019-01-21", result);
    }

    /// <summary>
    /// 测试ToDateString方法：可空DateTime为null时应返回空字符串
    /// </summary>
    [Fact]
    public void ToDateString_NullDateTime_ReturnsEmptyString()
    {
        // Arrange
        DateTime? dateTime = null;

        // Act
        var result = dateTime.ToDateString();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    /// <summary>
    /// 测试ToIsoDateTimeString方法：DateTime转ISO 8601标准时间字符串
    /// </summary>
    [Fact]
    public void ToIsoDateTimeString_ValidDateTime_ReturnsIsoString()
    {
        // Arrange
        var dateTime = new DateTime(2019, 1, 21, 20, 57, 51, DateTimeKind.Local);

        // Act
        var result = dateTime.ToIsoDateTimeString();

        // Assert
        Assert.StartsWith("2019-01-21T20:57:51", result);
    }

    /// <summary>
    /// 测试ToIsoDateTimeString方法：可空DateTime为null时应返回空字符串
    /// </summary>
    [Fact]
    public void ToIsoDateTimeString_NullDateTime_ReturnsEmptyString()
    {
        // Arrange
        DateTime? dateTime = null;

        // Act
        var result = dateTime.ToIsoDateTimeString();

        // Assert
        Assert.Equal("", result);
    }

    /// <summary>
    /// 测试ToFormatString方法：自定义时间格式
    /// </summary>
    [Fact]
    public void ToFormatString_CustomFormat_ReturnsFormattedString()
    {
        // Arrange
        var dateTime = new DateTime(2019, 1, 21, 20, 57, 51);

        // Act
        var result = dateTime.ToFormatString("yyyy年MM月dd日 HH时mm分ss秒");

        // Assert
        Assert.Equal("2019年01月21日 20时57分51秒", result);
    }

    /// <summary>
    /// 测试ToFormatString方法：格式为null时应使用默认格式
    /// </summary>
    [Fact]
    public void ToFormatString_NullFormat_UsesDefaultFormat()
    {
        // Arrange
        var dateTime = new DateTime(2019, 1, 21, 20, 57, 51);

        // Act
        var result = dateTime.ToFormatString(null);

        // Assert
        Assert.Equal("2019-01-21 20:57:51", result);
    }

    #endregion

    #region 时区处理 Tests

    /// <summary>
    /// 测试ToNowDateTime方法：获取无时区的当前时间
    /// </summary>
    [Fact]
    public void ToNowDateTime_ReturnsUnspecifiedTime()
    {
        // Arrange
        var now = DateTime.Now;

        // Act
        var result = now.ToNowDateTime();

        // Assert
        Assert.Equal(DateTimeKind.Unspecified, result.Kind);
    }

    /// <summary>
    /// 测试ToUnspecifiedDateTime方法：转换为无时区时间
    /// </summary>
    [Fact]
    public void ToUnspecifiedDateTime_ConvertsKindToUnspecified()
    {
        // Arrange
        var dateTime = new DateTime(2019, 1, 21, 20, 57, 51, DateTimeKind.Utc);

        // Act
        var result = dateTime.ToUnspecifiedDateTime();

        // Assert
        Assert.Equal(DateTimeKind.Unspecified, result.Kind);
        Assert.Equal(2019, result.Year);
        Assert.Equal(1, result.Month);
        Assert.Equal(21, result.Day);
    }

    #endregion

    #region 时间戳 Tests

    /// <summary>
    /// 测试ToTimestamp方法：获取毫秒级时间戳
    /// </summary>
    [Fact]
    public void ToTimestamp_Milliseconds_ReturnsCorrectTimestamp()
    {
        // Arrange
        var dateTime = new DateTime(2019, 1, 21, 20, 57, 51, DateTimeKind.Utc);

        // Act
        var result = dateTime.ToTimestamp(false);

        // Assert
        Assert.True(result > 0);
    }

    /// <summary>
    /// 测试ToTimestamp方法：获取秒级时间戳
    /// </summary>
    [Fact]
    public void ToTimestamp_Seconds_ReturnsCorrectTimestamp()
    {
        // Arrange
        var dateTime = new DateTime(2019, 1, 21, 20, 57, 51, DateTimeKind.Utc);

        // Act
        var result = dateTime.ToTimestamp(true);

        // Assert
        Assert.True(result > 0);
        // 秒级时间戳应该比毫秒级小约1000倍
        var msTimestamp = dateTime.ToTimestamp(false);
        Assert.True(result * 1000 - msTimestamp < 1000); // 允许1秒的误差
    }

    /// <summary>
    /// 测试ToDateTime方法：毫秒级时间戳转DateTime
    /// </summary>
    [Fact]
    public void ToDateTime_MillisecondsTimestamp_ConvertsToDateTime()
    {
        // Arrange
        var timestamp = 1548070671000L; // 2019-01-21 20:57:51 UTC 附近的时间戳

        // Act
        var result = timestamp.ToDateTime(false);

        // Assert
        Assert.Equal(2019, result.Year);
        Assert.Equal(1, result.Month);
    }

    /// <summary>
    /// 测试ToDateTime方法：秒级时间戳转DateTime
    /// </summary>
    [Fact]
    public void ToDateTime_SecondsTimestamp_ConvertsToDateTime()
    {
        // Arrange
        var timestamp = 1548070671L; // 秒级时间戳

        // Act
        var result = timestamp.ToDateTime(true);

        // Assert
        Assert.True(result.Year >= 2019);
    }

    #endregion

    #region 时间段 Tests

    /// <summary>
    /// 测试ToDateTime方法：TimeSpan转DateTime
    /// </summary>
    [Fact]
    public void TimeSpan_ToDateTime_ConvertsToDateTime()
    {
        // Arrange
        var timeSpan = new TimeSpan(1, 2, 3, 4, 5, 6); // 1天2小时3分4秒5毫秒

        // Act
        var result = timeSpan.ToDateTime();

        // Assert
        Assert.Equal(1, result.Day);
        Assert.True(result.Hour >= 2);
    }

    /// <summary>
    /// 测试ToTimeSpan方法：DateTime转TimeSpan
    /// </summary>
    [Fact]
    public void DateTime_ToTimeSpan_ConvertsToTimeSpan()
    {
        // Arrange
        var dateTime = new DateTime(1, 1, 1, 12, 30, 0); // 公元1年

        // Act
        var result = dateTime.ToTimeSpan();

        // Assert
        Assert.True(result.TotalDays > 0);
    }

    #endregion

    #region 日期判断 Tests

    /// <summary>
    /// 测试IsWeekend方法：周六是周末
    /// </summary>
    [Fact]
    public void IsWeekend_Saturday_ReturnsTrue()
    {
        // Arrange
        var saturday = new DateTime(2019, 1, 26); // 2019-01-26 是周六

        // Act
        var result = saturday.IsWeekend();

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// 测试IsWeekend方法：周日是周末
    /// </summary>
    [Fact]
    public void IsWeekend_Sunday_ReturnsTrue()
    {
        // Arrange
        var sunday = new DateTime(2019, 1, 27); // 2019-01-27 是周日

        // Act
        var result = sunday.IsWeekend();

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// 测试IsWeekend方法：周三不是周末
    /// </summary>
    [Fact]
    public void IsWeekend_Wednesday_ReturnsFalse()
    {
        // Arrange
        var wednesday = new DateTime(2019, 1, 23); // 2019-01-23 是周三

        // Act
        var result = wednesday.IsWeekend();

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// 测试IsWeekday方法：工作日应返回true
    /// </summary>
    [Fact]
    public void IsWeekday_Monday_ReturnsTrue()
    {
        // Arrange
        var monday = new DateTime(2019, 1, 21); // 周一

        // Act
        var result = monday.IsWeekday();

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// 测试IsWeekday方法：周末应返回false
    /// </summary>
    [Fact]
    public void IsWeekday_Saturday_ReturnsFalse()
    {
        // Arrange
        var saturday = new DateTime(2019, 1, 26);

        // Act
        var result = saturday.IsWeekday();

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// 测试GetWeekOfMonth方法：周一作为一周的开始
    /// </summary>
    [Fact]
    public void GetWeekOfMonth_WeekStartMonday_ReturnsCorrectWeek()
    {
        // Arrange
        var date = new DateTime(2026, 2, 11); // 是周三

        // Act
        var result = date.GetWeekOfMonth(1);

        // Assert
        Assert.Equal(2, result); // 第2周
    }

    /// <summary>
    /// 测试GetWeekOfMonth方法：周日作为一周的开始
    /// </summary>
    [Fact]
    public void GetWeekOfMonth_WeekStartSunday_ReturnsCorrectWeek()
    {
        // Arrange
        var date = new DateTime(2019, 1, 6); // 2019-01-06 是周日

        // Act
        var result = date.GetWeekOfMonth(2);

        // Assert
        Assert.Equal(1, result); // 第一周
    }

    /// <summary>
    /// 测试GetQuarter方法：获取月份对应的季度
    /// </summary>
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 1)]
    [InlineData(4, 2)]
    [InlineData(5, 2)]
    [InlineData(6, 2)]
    [InlineData(7, 3)]
    [InlineData(8, 3)]
    [InlineData(9, 3)]
    [InlineData(10, 4)]
    [InlineData(11, 4)]
    [InlineData(12, 4)]
    public void GetQuarter_DifferentMonths_ReturnsCorrectQuarter(int month, int expectedQuarter)
    {
        // Arrange
        var dateTime = new DateTime(2019, month, 15);

        // Act
        var result = dateTime.GetQuarter();

        // Assert
        Assert.Equal(expectedQuarter, result);
    }

    /// <summary>
    /// 测试GetCurrentMonthDayNumber方法：获取当前月份有多少天
    /// </summary>
    [Fact]
    public void GetCurrentMonthDayNumber_January_Returns31()
    {
        // Arrange
        var dateTime = new DateTime(2019, 1, 15);

        // Act
        var result = dateTime.GetCurrentMonthDayNumber();

        // Assert
        Assert.Equal(31, result);
    }

    /// <summary>
    /// 测试GetCurrentMonthDayNumber方法：二月平年有28天
    /// </summary>
    [Fact]
    public void GetCurrentMonthDayNumber_FebruaryNonLeapYear_Returns28()
    {
        // Arrange
        var dateTime = new DateTime(2019, 2, 15);

        // Act
        var result = dateTime.GetCurrentMonthDayNumber();

        // Assert
        Assert.Equal(28, result);
    }

    /// <summary>
    /// 测试GetCurrentMonthDayNumber方法：二月闰年有29天
    /// </summary>
    [Fact]
    public void GetCurrentMonthDayNumber_FebruaryLeapYear_Returns29()
    {
        // Arrange
        var dateTime = new DateTime(2020, 2, 15);

        // Act
        var result = dateTime.GetCurrentMonthDayNumber();

        // Assert
        Assert.Equal(29, result);
    }

    #endregion

    #region 日期差 Tests

    /// <summary>
    /// 测试DateDiff方法：计算两个日期之间的天数
    /// </summary>
    [Fact]
    public void DateDiff_OneDayDifference_ReturnsOne()
    {
        // Arrange
        var startDate = new DateTime(2019, 1, 21);
        var endDate = new DateTime(2019, 1, 22);

        // Act
        var result = startDate.DateDiff(endDate);

        // Assert
        Assert.Equal(1, result);
    }

    /// <summary>
    /// 测试DateDiff方法：不足24小时算一天
    /// </summary>
    [Fact]
    public void DateDiff_LessThan24Hours_ReturnsOne()
    {
        // Arrange
        var startDate = new DateTime(2019, 1, 21, 10, 0, 0);
        var endDate = new DateTime(2019, 1, 21, 20, 0, 0);

        // Act
        var result = startDate.DateDiff(endDate);

        // Assert
        Assert.Equal(1, result); // 不足24小时
    }

    /// <summary>
    /// 测试DateDiff方法：不超过24小时才算一天
    /// </summary>
    [Fact]
    public void DateDiff_MoreThan24Hours_ReturnsOne()
    {
        // Arrange
        var startDate = new DateTime(2019, 1, 21, 10, 0, 0);
        var endDate = new DateTime(2019, 1, 21, 23, 59, 59);

        // Act
        var result = startDate.DateDiff(endDate);

        // Assert
        Assert.Equal(1, result); // 13小时59分，四舍五入为2天
    }

    /// <summary>
    /// 测试DateDiff方法：可空DateTime为null时应返回0
    /// </summary>
    [Fact]
    public void DateDiff_NullStartDate_ReturnsZero()
    {
        // Arrange
        DateTime? startDate = null;
        var endDate = new DateTime(2019, 1, 22);

        // Act
        var result = startDate.DateDiff(endDate);

        // Assert
        Assert.Equal(0, result);
    }

    /// <summary>
    /// 测试DateDiff方法：可空DateTime为null时应返回0
    /// </summary>
    [Fact]
    public void DateDiff_NullEndDate_ReturnsZero()
    {
        // Arrange
        var startDate = new DateTime(2019, 1, 21);
        DateTime? endDate = null;

        // Act
        var result = startDate.DateDiff(endDate);

        // Assert
        Assert.Equal(0, result);
    }

    /// <summary>
    /// 测试DateDiff方法：反向日期计算应返回正数
    /// </summary>
    [Fact]
    public void DateDiff_ReverseOrder_ReturnsPositiveValue()
    {
        // Arrange
        var startDate = new DateTime(2019, 1, 25);
        var endDate = new DateTime(2019, 1, 21);

        // Act
        var result = startDate.DateDiff(endDate);

        // Assert
        Assert.Equal(4, result); // 4天
    }

    #endregion

    #region 边界情况 Tests

    /// <summary>
    /// 测试边界情况：最小DateTime值
    /// </summary>
    [Fact]
    public void ToTimeSpan_MinDateTime_ReturnsZeroTimeSpan()
    {
        // Arrange
        var minDate = DateTime.MinValue;

        // Act
        var result = minDate.ToTimeSpan();

        // Assert
        Assert.Equal(TimeSpan.Zero, result);
    }

    /// <summary>
    /// 测试边界情况：GetWeekOfMonth的无效参数
    /// </summary>
    [Fact]
    public void GetWeekOfMonth_InvalidWeekStart_ThrowsException()
    {
        // Arrange
        var date = new DateTime(2019, 1, 7);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => date.GetWeekOfMonth(3));
    }

    #endregion
}
