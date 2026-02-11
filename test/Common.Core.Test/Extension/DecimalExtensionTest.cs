namespace Common.Core.Test.Extension;

/// <summary>
/// DecimalExtension Decimal扩展方法的单元测试
/// </summary>
public class DecimalExtensionTest
{
    #region ToStandardString Tests

    /// <summary>
    /// 测试ToStandardString方法：decimal值转字符串（四舍五入）
    /// </summary>
    [Theory]
    [InlineData(1.24, 2, "1.24")]
    [InlineData(1.246, 2, "1.25")]
    [InlineData(1.245, 2, "1.24")]
    [InlineData(1.2451, 2, "1.25")]
    [InlineData(1.2350, 2, "1.24")]
    [InlineData(1.2, 2, "1.2")]
    public void ToStandardString_ShouldFormatCorrectly(decimal input, int number, string expected)
    {
        // Act
        var result = input.ToStandardString(number);

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// 测试ToStandardString方法：可空decimal为null时应返回"0"
    /// </summary>
    [Fact]
    public void ToStandardString_NullDecimal_ReturnsZero()
    {
        // Arrange
        decimal? value = null;

        // Act
        var result = value.ToStandardString();

        // Assert
        Assert.Equal("0", result);
    }

    /// <summary>
    /// 测试ToStandardString方法：负数的情况
    /// </summary>
    [Fact]
    public void ToStandardString_NegativeDecimal_ReturnsFormattedString()
    {
        // Arrange
        var value = -123.456m;

        // Act
        var result = value.ToStandardString();

        // Assert
        Assert.Equal("-123.46", result);
    }

    #endregion

    #region ToNoZeroString Tests

    /// <summary>
    /// 测试ToNoZeroString方法：去除小数点后的无效零
    /// </summary>
    [Theory]
    [InlineData(1.20, 2, "1.2")]
    [InlineData(1.255, 2, "1.26")]
    [InlineData(1.2, 1, "1.2")]
    [InlineData(1.255, 3, "1.255")]
    public void ToNoZeroString_ShouldFormatCorrectly(decimal input, int number, string expected)
    {
        // Act
        var result = input.ToNoZeroString(number);

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// 测试ToNoZeroString方法：可空decimal为null时应返回"0"
    /// </summary>
    [Fact]
    public void ToNoZeroString_NullDecimal_ReturnsZero()
    {
        // Arrange
        decimal? value = null;

        // Act
        var result = value.ToNoZeroString();

        // Assert
        Assert.Equal("0", result);
    }

    /// <summary>
    /// 测试ToNoZeroString方法：所有小数位都是零时应只保留整数部分
    /// </summary>
    [Fact]
    public void ToNoZeroString_AllZeroDecimals_ReturnsIntegerOnly()
    {
        // Arrange
        var value = 123.000m;

        // Act
        var result = value.ToNoZeroString();

        // Assert
        Assert.Equal("123", result);
    }

    #endregion

    #region ToFixedString Tests

    /// <summary>
    /// 测试ToFixedString方法：保留指定小数位数并保留结尾零
    /// </summary>
    [Theory]
    [InlineData(1.2345, 2, "1.23")]
    [InlineData(1.2345, 3, "1.235")]
    [InlineData(1.2345, 0, "1")]
    [InlineData(1.2345, -1, "1.23")]
    public void ToFixedString_ShouldFormatCorrectly(decimal input, int number, string expected)
    {
        // Act
        var result = input.ToFixedString(number);

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// 测试ToFixedString方法：可空decimal为null时应返回"0"
    /// </summary>
    [Theory]
    [InlineData(null, 2, "0")]
    public void ToFixedString_Null_ShouldFormatCorrectly(decimal? input, int number, string expected)
    {
        // Act
        var result = input.ToFixedString(number);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region ToPercentString Tests

    /// <summary>
    /// 测试ToPercentString方法：decimal转百分比字符串
    /// </summary>
    [Theory]
    [InlineData(0.1234d, 2, "12.34%")]
    [InlineData(0.1234d, 3, "12.340%")]
    [InlineData(0.1234d, 0, "12%")]
    [InlineData(0.1234d, -1, "12.34%")]
    public void ToPercentString_ShouldFormatCorrectly(decimal input, int number, string expected)
    {
        // Act
        var result = input.ToPercentString(number);

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// 测试ToPercentString方法：可空decimal为null时应返回"0%"
    /// </summary>
    [Theory]
    [InlineData(null, 2, "0%")]
    public void ToPercentString_Null_ShouldFormatCorrectly(decimal? input, int number, string expected)
    {
        // Act
        var result = input.ToPercentString(number);

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// 测试ToPercentString方法：大于1的值
    /// </summary>
    [Fact]
    public void ToPercentString_GreaterThanOne_ReturnsCorrectPercent()
    {
        // Arrange
        var value = 1.5m;

        // Act
        var result = value.ToPercentString();

        // Assert
        Assert.Equal("150.00%", result);
    }

    #endregion

    #region ToTruncateString Tests

    /// <summary>
    /// 测试ToTruncateString方法：强制截取到指定小数位数（不四舍五入）
    /// </summary>
    [Theory]
    [InlineData(1.2345, 2, "1.23")]
    [InlineData(1.2345, 3, "1.234")]
    [InlineData(1.2345, 0, "1")]
    [InlineData(1.2345, -1, "1.23")]
    public void ToTruncateString_ShouldFormatCorrectly(decimal input, int number, string expected)
    {
        // Act
        var result = input.ToTruncateString(number);

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// 测试ToTruncateString方法：可空decimal为null时应返回"0"
    /// </summary>
    [Theory]
    [InlineData(null, 2, "0")]
    public void ToTruncateString_Null_ShouldFormatCorrectly(decimal? input, int number, string expected)
    {
        // Act
        var result = input.ToTruncateString(number);

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// 测试ToTruncateString方法：截断而不是四舍五入
    /// </summary>
    [Fact]
    public void ToTruncateString_ShouldNotRound_Truncates()
    {
        // Arrange
        var value = 123.999m;

        // Act
        var result = value.ToTruncateString();

        // Assert
        Assert.Equal("123.99", result); // 不是 124.00
    }

    #endregion

    #region ToAbs Tests

    /// <summary>
    /// 测试ToAbs方法：取绝对值
    /// </summary>
    [Fact]
    public void decima负数绝对值_ReturnOK()
    {
        // Arrange
        var str = -1.255m;

        // Act
        var result = str.ToAbs();

        // Assert
        Assert.Equal(1.255m, result);
    }

    /// <summary>
    /// 测试ToAbs方法：正数的绝对值
    /// </summary>
    [Fact]
    public void ToAbs_PositiveDecimal_ReturnsSameValue()
    {
        // Arrange
        var value = 123.45m;

        // Act
        var result = value.ToAbs();

        // Assert
        Assert.Equal(123.45m, result);
    }

    /// <summary>
    /// 测试ToAbs方法：零的绝对值
    /// </summary>
    [Fact]
    public void ToAbs_Zero_ReturnsZero()
    {
        // Arrange
        var value = 0m;

        // Act
        var result = value.ToAbs();

        // Assert
        Assert.Equal(0m, result);
    }

    #endregion

    #region Edge Cases Tests

    /// <summary>
    /// 测试边界情况：大数值
    /// </summary>
    [Fact]
    public void ToStandardString_LargeNumber_HandlesCorrectly()
    {
        // Arrange
        var value = 999999.999m;

        // Act
        var result = value.ToStandardString();

        // Assert
        Assert.Equal("1000000.00", result);
    }

    /// <summary>
    /// 测试边界情况：非常小的小数
    /// </summary>
    [Fact]
    public void ToStandardString_VerySmallDecimal_RoundsCorrectly()
    {
        // Arrange
        var value = 0.0001m;

        // Act
        var result = value.ToStandardString();

        // Assert
        Assert.Equal("0.00", result);
    }

    #endregion
}
