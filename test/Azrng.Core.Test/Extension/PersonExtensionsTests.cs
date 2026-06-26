using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class PersonExtensionsTests
{
    #region GetBirthdayByIdCard

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("123", null)]
    [InlineData("12345678901234567890", null)]
    public void GetBirthdayByIdCard_InvalidInput_ReturnsNull(string? input, string? expected)
    {
        input.GetBirthdayByIdCard().Should().Be(expected);
    }

    [Theory]
    [InlineData("11010119900307001X", "1990-03-07")]
    [InlineData("110101199003071234", "1990-03-07")]
    [InlineData("320102200012310011", "2000-12-31")]
    [InlineData("110101198501010011", "1985-01-01")]
    public void GetBirthdayByIdCard_18DigitId_ReturnsBirthday(string input, string expected)
    {
        input.GetBirthdayByIdCard().Should().Be(expected);
    }

    [Theory]
    [InlineData("110101900307001", "1990-03-07")]
    [InlineData("110101850101001", "1985-01-01")]
    [InlineData("320102001231001", "1900-12-31")]
    public void GetBirthdayByIdCard_15DigitId_ReturnsBirthdayWith19Prefix(string input, string expected)
    {
        input.GetBirthdayByIdCard().Should().Be(expected);
    }

    #endregion

    #region GetSexByIdCard

    [Theory]
    [InlineData("11010119900307001X", 1)]
    [InlineData("11010119900307003X", 1)]
    [InlineData("11010119900307002X", 0)]
    [InlineData("11010119900307004X", 0)]
    [InlineData("110101199003071234", 1)]
    [InlineData("110101199003071224", 0)]
    public void GetSexByIdCard_18DigitId_ReturnsSexBasedOnSequenceOddEven(string input, int expected)
    {
        input.GetSexByIdCard().Should().Be(expected);
    }

    [Theory]
    [InlineData("110101900307001")]
    [InlineData("110101850101001")]
    public void GetSexByIdCard_15DigitId_ReturnsZero(string input)
    {
        input.GetSexByIdCard().Should().Be(0);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345678901234567890")]
    public void GetSexByIdCard_InvalidLength_ReturnsZero(string input)
    {
        input.GetSexByIdCard().Should().Be(0);
    }

    [Fact]
    public void GetSexByIdCard_NullInput_ReturnsZero()
    {
        string? input = null;
        input!.GetSexByIdCard().Should().Be(0);
    }

    #endregion
}
