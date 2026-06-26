using System.Text;
using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class StringExtensionTests
{
    #region IsBool

    [Theory]
    [InlineData("0", false)]
    [InlineData("1", true)]
    [InlineData("是", true)]
    [InlineData("否", false)]
    [InlineData("yes", true)]
    [InlineData("YES", true)]
    [InlineData("Yes", true)]
    [InlineData("abc", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("  ", false)]
    [InlineData(" true ", false)]
    public void IsBool_ShouldReturnExpected(string? input, bool expected)
    {
        input.IsBool().Should().Be(expected);
    }

    #endregion

    #region IsIntFormat

    [Theory]
    [InlineData("123", true)]
    [InlineData("-123", true)]
    [InlineData("0", true)]
    [InlineData("abc", false)]
    [InlineData("12.34", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsIntFormat_ShouldReturnExpected(string? input, bool expected)
    {
        input.IsIntFormat().Should().Be(expected);
    }

    #endregion

    #region ToInt / ToInt32

    [Theory]
    [InlineData("123", 0, 123)]
    [InlineData("-123", 0, -123)]
    [InlineData("0", 0, 0)]
    [InlineData("abc", 0, 0)]
    [InlineData("abc", 99, 99)]
    [InlineData(null, 0, 0)]
    [InlineData(null, -1, -1)]
    [InlineData("", 0, 0)]
    public void ToInt_ShouldReturnExpected(string? input, int defaultVal, int expected)
    {
        input.ToInt(defaultVal).Should().Be(expected);
    }

    [Theory]
    [InlineData("456", 0, 456)]
    [InlineData("abc", 0, 0)]
    [InlineData(null, 77, 77)]
    public void ToInt32_ShouldReturnExpected(string? input, int defaultVal, int expected)
    {
        input.ToInt32(defaultVal).Should().Be(expected);
    }

    #endregion

    #region IsFloatFormat

    [Theory]
    [InlineData("1.5", true)]
    [InlineData("-1.5", true)]
    [InlineData("0", true)]
    [InlineData("abc", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsFloatFormat_ShouldReturnExpected(string? input, bool expected)
    {
        input.IsFloatFormat().Should().Be(expected);
    }

    #endregion

    #region ToFloat

    [Theory]
    [InlineData("1.5", 0f, 1.5f)]
    [InlineData("-2.5", 0f, -2.5f)]
    [InlineData("abc", 0f, 0f)]
    [InlineData("abc", 9.9f, 9.9f)]
    [InlineData(null, 0f, 0f)]
    public void ToFloat_ShouldReturnExpected(string? input, float defaultVal, float expected)
    {
        input.ToFloat(defaultVal).Should().Be(expected);
    }

    #endregion

    #region IsDoubleFormat

    [Theory]
    [InlineData("1.5", true)]
    [InlineData("-1.5", true)]
    [InlineData("0", true)]
    [InlineData("abc", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsDoubleFormat_ShouldReturnExpected(string? input, bool expected)
    {
        input.IsDoubleFormat().Should().Be(expected);
    }

    #endregion

    #region ToDouble

    [Theory]
    [InlineData("1.5", 0d, 1.5d)]
    [InlineData("-2.5", 0d, -2.5d)]
    [InlineData("abc", 0d, 0d)]
    [InlineData("abc", 9.9d, 9.9d)]
    [InlineData(null, 0d, 0d)]
    public void ToDouble_ShouldReturnExpected(string? input, double defaultVal, double expected)
    {
        input.ToDouble(defaultVal).Should().Be(expected);
    }

    #endregion

    #region IsInt64Format

    [Theory]
    [InlineData("1234567890123", true)]
    [InlineData("-1234567890123", true)]
    [InlineData("0", true)]
    [InlineData("abc", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsInt64Format_ShouldReturnExpected(string? input, bool expected)
    {
        input.IsInt64Format().Should().Be(expected);
    }

    #endregion

    #region ToInt64

    [Theory]
    [InlineData("1234567890123", 0L, 1234567890123L)]
    [InlineData("-1234567890123", 0L, -1234567890123L)]
    [InlineData("abc", 0L, 0L)]
    [InlineData("abc", 99L, 99L)]
    [InlineData(null, 0L, 0L)]
    public void ToInt64_ShouldReturnExpected(string? input, long defaultVal, long expected)
    {
        input.ToInt64(defaultVal).Should().Be(expected);
    }

    #endregion

    #region IsDecimalFormat

    [Theory]
    [InlineData("1.5", true)]
    [InlineData("-1.5", true)]
    [InlineData("0", true)]
    [InlineData("12345678901234567890", true)]
    [InlineData("abc", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsDecimalFormat_ShouldReturnExpected(string? input, bool expected)
    {
        input.IsDecimalFormat().Should().Be(expected);
    }

    #endregion

    #region ToDecimal

    [Theory]
    [InlineData("1.5", 0, 1.5)]
    [InlineData("-2.5", 0, -2.5)]
    [InlineData("abc", 0, 0)]
    [InlineData("abc", 9.9, 9.9)]
    [InlineData(null, 0, 0)]
    public void ToDecimal_ShouldReturnExpected(string? input, double defaultVal, double expected)
    {
        input.ToDecimal((decimal)defaultVal).Should().Be((decimal)expected);
    }

    #endregion

    #region IsDateFormat

    [Theory]
    [InlineData("2024-01-01", true)]
    [InlineData("2024/01/01", true)]
    [InlineData("abc", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsDateFormat_ShouldReturnExpected(string? input, bool expected)
    {
        input.IsDateFormat().Should().Be(expected);
    }

    #endregion

    #region ToDateTime

    [Fact]
    public void ToDateTime_ValidDate_ShouldReturnDate()
    {
        var result = "2024-01-15".ToDateTime();
        result.Should().NotBeNull();
        result!.Value.Year.Should().Be(2024);
        result.Value.Month.Should().Be(1);
        result.Value.Day.Should().Be(15);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData(null)]
    public void ToDateTime_InvalidInput_ShouldReturnNull(string? input)
    {
        input.ToDateTime().Should().BeNull();
    }

    #endregion

    #region ToBytesFromHexString

    [Fact]
    public void ToBytesFromHexString_ValidHex_ShouldReturnBytes()
    {
        "4A6F686E".ToBytesFromHexString().Should().Equal(new byte[] { 0x4A, 0x6F, 0x68, 0x6E });
    }

    [Fact]
    public void ToBytesFromHexString_OddLength_ShouldPadLeadingZero()
    {
        "FFF".ToBytesFromHexString().Should().Equal(new byte[] { 0x0F, 0xFF });
    }

    [Fact]
    public void ToBytesFromHexString_Empty_ShouldReturnSingleByte()
    {
        "".ToBytesFromHexString().Should().HaveCount(1);
    }

    #endregion

    #region ToBytes

    [Fact]
    public void ToBytes_DefaultEncoding_ShouldReturnUtf8Bytes()
    {
        var bytes = "hello".ToBytes();
        bytes.Should().Equal(Encoding.UTF8.GetBytes("hello"));
    }

    [Fact]
    public void ToBytes_CustomEncoding_ShouldUseSpecifiedEncoding()
    {
        var bytes = "hello".ToBytes(Encoding.ASCII);
        bytes.Should().Equal(Encoding.ASCII.GetBytes("hello"));
    }

    #endregion

    #region ToGuid

    [Fact]
    public void ToGuid_ValidGuid_ShouldReturnGuid()
    {
        var guidStr = "D3B07384-D113-4CE8-B95F-E36D4A2C9E1F";
        var result = guidStr.ToGuid();
        result.Should().Be(Guid.Parse(guidStr));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData(null)]
    public void ToGuid_InvalidInput_ShouldReturnDefault(string? input)
    {
        input.ToGuid().Should().Be(Guid.Empty);
    }

    [Fact]
    public void ToGuid_InvalidInput_WithCustomDefault_ShouldReturnCustomDefault()
    {
        var defaultGuid = Guid.NewGuid();
        "invalid".ToGuid(defaultGuid).Should().Be(defaultGuid);
    }

    #endregion

    #region HasChinese

    [Theory]
    [InlineData("你好", true)]
    [InlineData("hello你好", true)]
    [InlineData("hello", false)]
    [InlineData("123", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void HasChinese_ShouldReturnExpected(string? input, bool expected)
    {
        input.HasChinese().Should().Be(expected);
    }

    #endregion

    #region IsNullOrWhiteSpace / IsNotNullOrWhiteSpace

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData("abc", false)]
    public void IsNullOrWhiteSpace_ShouldReturnExpected(string? input, bool expected)
    {
        input.IsNullOrWhiteSpace().Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("abc", true)]
    public void IsNotNullOrWhiteSpace_ShouldReturnExpected(string? input, bool expected)
    {
        input.IsNotNullOrWhiteSpace().Should().Be(expected);
    }

    #endregion

    #region IsNullOrEmpty / IsNotNullOrEmpty

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("   ", false)]
    [InlineData("abc", false)]
    public void IsNullOrEmpty_ShouldReturnExpected(string? input, bool expected)
    {
        input.IsNullOrEmpty().Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", true)]
    [InlineData("abc", true)]
    public void IsNotNullOrEmpty_ShouldReturnExpected(string? input, bool expected)
    {
        input.IsNotNullOrEmpty().Should().Be(expected);
    }

    #endregion

    #region GetOrDefault

    [Theory]
    [InlineData("hello", "default", "hello")]
    [InlineData(null, "default", "default")]
    [InlineData("", "default", "default")]
    [InlineData("   ", "default", "default")]
    public void GetOrDefault_ShouldReturnExpected(string? input, string defaultVal, string expected)
    {
        input.GetOrDefault(defaultVal).Should().Be(expected);
    }

    #endregion

    #region ToStrArray

    [Fact]
    public void ToStrArray_DefaultSeparator_ShouldSplitByComma()
    {
        "a,b,c".ToStrArray().Should().Equal("a", "b", "c");
    }

    [Fact]
    public void ToStrArray_CustomSeparator_ShouldSplitCorrectly()
    {
        "a|b|c".ToStrArray("|").Should().Equal("a", "b", "c");
    }

    [Fact]
    public void ToStrArray_ShouldRemoveEmptyEntries()
    {
        "a,,b,c".ToStrArray().Should().Equal("a", "b", "c");
    }

    #endregion

    #region GetByIndex

    [Theory]
    [InlineData("hello", 0, "h")]
    [InlineData("hello", 4, "o")]
    [InlineData("hello", -1, "")]
    [InlineData("hello", 5, "")]
    public void GetByIndex_ShouldReturnExpected(string input, int index, string expected)
    {
        input.GetByIndex(index).Should().Be(expected);
    }

    #endregion

    #region EqualsNoCase

    [Theory]
    [InlineData("Hello", "hello", true)]
    [InlineData("HELLO", "hello", true)]
    [InlineData("hello", "hello", true)]
    [InlineData("hello", "world", false)]
    [InlineData(null, "hello", false)]
    [InlineData("hello", null, false)]
    [InlineData(null, null, false)]
    public void EqualsNoCase_ShouldReturnExpected(string? a, string? b, bool expected)
    {
        a.EqualsNoCase(b).Should().Be(expected);
    }

    #endregion

    #region RemoveControlChars

    [Fact]
    public void RemoveControlChars_WithControlChars_ShouldRemove()
    {
        var input = "hello\tworld\n";
        input.RemoveControlChars().Should().Be("helloworld");
    }

    [Fact]
    public void RemoveControlChars_Null_ShouldReturnNull()
    {
        ((string?)null).RemoveControlChars().Should().BeNull();
    }

    [Fact]
    public void RemoveControlChars_NoControlChars_ShouldReturnSame()
    {
        "hello".RemoveControlChars().Should().Be("hello");
    }

    #endregion

    #region ToUpperFirst

    [Theory]
    [InlineData("hello", "Hello")]
    [InlineData("HELLO", "HELLO")]
    [InlineData("a", "A")]
    public void ToUpperFirst_ShouldCapitalizeFirstLetter(string input, string expected)
    {
        input.ToUpperFirst().Should().Be(expected);
    }

    [Fact]
    public void ToUpperFirst_Null_ShouldReturnNull()
    {
        ((string?)null).ToUpperFirst().Should().BeNull();
    }

    #endregion

    #region ToBytesByBase64

    [Fact]
    public void ToBytesByBase64_ValidBase64_ShouldReturnBytes()
    {
        var original = "hello";
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(original));
        base64.ToBytesByBase64().Should().Equal(Encoding.UTF8.GetBytes(original));
    }

    #endregion

    #region ToBase64Encode / FromBase64Decode

    [Fact]
    public void ToBase64Encode_ShouldReturnBase64String()
    {
        "hello".ToBase64Encode().Should().Be(Convert.ToBase64String(Encoding.UTF8.GetBytes("hello")));
    }

    [Fact]
    public void FromBase64Decode_ShouldReturnOriginalString()
    {
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("hello"));
        base64.FromBase64Decode().Should().Be("hello");
    }

    [Fact]
    public void Base64_RoundTrip_ShouldPreserveOriginal()
    {
        var original = "你好世界 Hello";
        var encoded = original.ToBase64Encode();
        encoded.FromBase64Decode().Should().Be(original);
    }

    [Fact]
    public void FromBase64Decode_WithSpaces_ShouldReplaceWithPlus()
    {
        var base64WithSpaces = Convert.ToBase64String(Encoding.UTF8.GetBytes("test+data")).Replace("+", " ");
        base64WithSpaces.FromBase64Decode().Should().Be("test+data");
    }

    #endregion

    #region ToVersionNumber

    [Theory]
    [InlineData("1.0.0", 10000)]
    [InlineData("1.1.1", 10101)]
    [InlineData("2.3.4", 20304)]
    [InlineData("v1.2.3", 10203)]
    [InlineData("1.2.3.4", 1020304)]
    [InlineData("1.2.3.txt", 10203)]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("1.2", 0)]
    [InlineData("abc", 0)]
    public void ToVersionNumber_ShouldReturnExpected(string? input, int expected)
    {
        input.ToVersionNumber().Should().Be(expected);
    }

    #endregion
}
