namespace Common.Core.Test.Helper.LongHelper;

/// <summary>
/// FormatLongNumber单元测试
/// </summary>
public class FormatLongNumberTest
{
    [Fact]
    public void FormatLongNumber_LessThanThousand_ReturnsNumberAsString()
    {
        // Arrange
        long number = 999;

        // Act
        var result = NumberHelper.FormatLongNumber(number);

        // Assert
        Assert.Equal("999", result);
    }

    [Fact]
    public void FormatLongNumber_BetweenThousandAndMillion_ReturnsValueInThousands()
    {
        // Arrange
        long number = 1234567;

        // Act
        var result = NumberHelper.FormatLongNumber(number);

        // Assert
        Assert.Equal("123.5万", result);
    }

    [Fact]
    public void FormatLongNumber_BetweenMillionAndBillion_ReturnsValueInMillions()
    {
        // Arrange
        long number = 1234567890;

        // Act
        var result = NumberHelper.FormatLongNumber(number);

        // Assert
        Assert.Equal("12.3亿", result);
    }

    [Fact]
    public void FormatLongNumber_GreaterThanBillion_ReturnsValueInBillions()
    {
        // Arrange
        long number = 1234567890123;

        // Act
        var result = NumberHelper.FormatLongNumber(number);

        // Assert
        Assert.Equal("1.2万亿", result);
    }
}