using System.Globalization;

namespace Common.Core.Test.Extension;

/// <summary>
/// DoublelExtension Double扩展方法的单元测试
/// </summary>
public class DoublelExtensionTest
{
    #region ToStandardString Tests

    /// <summary>
    /// 测试ToStandardString方法：可空double转标准字符串
    /// </summary>
    [Fact]
    public void ToStandardString_NullableDouble_ReturnsZero()
    {
        // Arrange
        double? value = null;

        // Act
        var result = value.ToStandardString();

        // Assert
        Assert.Equal("0", result);
    }

    /// <summary>
    /// 测试ToStandardString方法：double转标准字符串
    /// </summary>
    [Theory]
    [InlineData(123.456)]
    [InlineData(0.123)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(123.0)]
    [InlineData(123.456789)]
    public void ToStandardString_ValidDouble_ReturnsFormattedString(double value)
    {
        // Arrange
        var expected = Math.Round(value, 2).ToString(CultureInfo.InvariantCulture);

        // Act
        var result = value.ToStandardString();

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region ToNoZeroString Tests

    /// <summary>
    /// 测试ToNoZeroString方法：可空double转不保留0的字符串
    /// </summary>
    [Fact]
    public void ToNoZeroString_NullableDouble_ReturnsZeroString()
    {
        // Arrange
        double? value = null;

        // Act
        var result = value.ToNoZeroString();

        // Assert
        Assert.Equal("0", result);
    }

    /// <summary>
    /// 测试ToNoZeroString方法：double转不保留0的字符串
    /// </summary>
    [Theory]
    [InlineData(123.456)]
    [InlineData(0.123)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0)]
    [InlineData(123.0)]
    public void ToNoZeroString_ValidDouble_ReturnsFormattedString(double value)
    {
        // Arrange
        var expected = value.ToString("0.##");

        // Act
        var result = value.ToNoZeroString();

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region ToFixedString Tests

    /// <summary>
    /// 测试ToFixedString方法：可空double转保留指定位数的字符串
    /// </summary>
    [Fact]
    public void ToFixedString_NullableDouble_ReturnsZeroString()
    {
        // Arrange
        double? value = null;

        // Act
        var result = value.ToFixedString();

        // Assert
        Assert.Equal("0", result);
    }

    /// <summary>
    /// 测试ToFixedString方法：double转保留指定位数的字符串
    /// </summary>
    [Theory]
    [InlineData(123.456)]
    [InlineData(0.123)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0)]
    [InlineData(123.456789)]
    public void ToFixedString_ValidDouble_ReturnsFormattedString(double value)
    {
        // Arrange
        var expected = value.ToString("F2");

        // Act
        var result = value.ToFixedString();

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region ToPercentString Tests

    /// <summary>
    /// 测试ToPercentString方法：可空double转百分比字符串
    /// </summary>
    [Fact]
    public void ToPercentString_NullableDouble_ReturnsZeroPercent()
    {
        // Arrange
        double? value = null;

        // Act
        var result = value.ToPercentString();

        // Assert
        Assert.Equal("0%", result);
    }

    /// <summary>
    /// 测试ToPercentString方法：double转百分比字符串
    /// </summary>
    [Theory]
    [InlineData(0.5)]
    [InlineData(0.25)]
    [InlineData(1)]
    [InlineData(25.5)]
    [InlineData(0.123)]
    public void ToPercentString_ValidDouble_ReturnsPercentString(double value)
    {
        // Arrange
        var expected = Math.Round(value * 100, 2).ToString("F2") + "%";

        // Act
        var result = value.ToPercentString();

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region ToTruncateString Tests

    /// <summary>
    /// 测试ToTruncateString方法：可空double转截断字符串
    /// </summary>
    [Fact]
    public void ToTruncateString_NullableDouble_ReturnsZeroString()
    {
        // Arrange
        double? value = null;

        // Act
        var result = value.ToTruncateString();

        // Assert
        Assert.Equal("0", result);
    }

    /// <summary>
    /// 测试ToTruncateString方法：double转截断字符串
    /// </summary>
    [Theory]
    [InlineData(123.456, "123.45")]
    [InlineData(0.123, "0.12")]
    [InlineData(1.0, "1.00")]
    [InlineData(-1.0, "-1.00")]
    [InlineData(0, "0.00")]
    [InlineData(123.456789, "123.45")]
    public void ToTruncateString_ValidDouble_ReturnsTruncatedString(double value, string verifyResult)
    {
        // Act
        var result = value.ToTruncateString();

        // Assert
        Assert.Equal(verifyResult, result);
    }

    #endregion

    #region ToAbs Tests

    /// <summary>
    /// 测试ToAbs方法：double转绝对值
    /// </summary>
    [Theory]
    [InlineData(123.456)]
    [InlineData(-123.456)]
    [InlineData(0)]
    [InlineData(-1.5)]
    [InlineData(1.5)]
    public void ToAbs_ValidDouble_ReturnsAbsValue(double value)
    {
        // Act
        var result = value.ToAbs();

        // Assert
        Assert.Equal(Math.Abs(value), result);
    }

    #endregion
}