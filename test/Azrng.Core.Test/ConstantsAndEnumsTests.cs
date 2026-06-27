using Azrng.Core.Enums;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test;

public class ConstantsAndEnumsTests
{
    #region LogLevel Enum

    [Fact]
    public void LogLevel_Trace_ShouldHaveValue0()
    {
        ((int)LogLevel.Trace).Should().Be(0);
    }

    [Fact]
    public void LogLevel_Debug_ShouldHaveValue1()
    {
        ((int)LogLevel.Debug).Should().Be(1);
    }

    [Fact]
    public void LogLevel_Information_ShouldHaveValue2()
    {
        ((int)LogLevel.Information).Should().Be(2);
    }

    [Fact]
    public void LogLevel_Warning_ShouldHaveValue3()
    {
        ((int)LogLevel.Warning).Should().Be(3);
    }

    [Fact]
    public void LogLevel_Error_ShouldHaveValue4()
    {
        ((int)LogLevel.Error).Should().Be(4);
    }

    [Fact]
    public void LogLevel_Critical_ShouldHaveValue5()
    {
        ((int)LogLevel.Critical).Should().Be(5);
    }

    [Fact]
    public void LogLevel_None_ShouldHaveValue6()
    {
        ((int)LogLevel.None).Should().Be(6);
    }

    [Fact]
    public void LogLevel_ShouldHaveExpectedCount()
    {
        var values = Enum.GetValues(typeof(LogLevel));
        values.Length.Should().Be(7);
    }

    [Fact]
    public void LogLevel_DefaultValue_ShouldBeTrace()
    {
        var defaultValue = default(LogLevel);
        defaultValue.Should().Be(LogLevel.Trace);
    }

    [Theory]
    [InlineData("Trace", LogLevel.Trace)]
    [InlineData("Debug", LogLevel.Debug)]
    [InlineData("Information", LogLevel.Information)]
    [InlineData("Warning", LogLevel.Warning)]
    [InlineData("Error", LogLevel.Error)]
    [InlineData("Critical", LogLevel.Critical)]
    [InlineData("None", LogLevel.None)]
    public void LogLevel_Parse_ShouldReturnExpectedValue(string name, LogLevel expected)
    {
        Enum.TryParse<LogLevel>(name, out var result).Should().BeTrue();
        result.Should().Be(expected);
    }

    #endregion

    #region TimeType Enum

    [Fact]
    public void TimeType_Yesterday_ShouldHaveValue0()
    {
        ((int)TimeType.Yesterday).Should().Be(0);
    }

    [Fact]
    public void TimeType_Today_ShouldHaveValue1()
    {
        ((int)TimeType.Today).Should().Be(1);
    }

    [Fact]
    public void TimeType_Tomorrow_ShouldHaveValue2()
    {
        ((int)TimeType.Tomorrow).Should().Be(2);
    }

    [Fact]
    public void TimeType_Week_ShouldHaveValue3()
    {
        ((int)TimeType.Week).Should().Be(3);
    }

    [Fact]
    public void TimeType_CurrentMonth_ShouldHaveValue4()
    {
        ((int)TimeType.CurrentMonth).Should().Be(4);
    }

    [Fact]
    public void TimeType_NextMonth_ShouldHaveValue5()
    {
        ((int)TimeType.NextMonth).Should().Be(5);
    }

    [Fact]
    public void TimeType_Season_ShouldHaveValue6()
    {
        ((int)TimeType.Season).Should().Be(6);
    }

    [Fact]
    public void TimeType_Year_ShouldHaveValue7()
    {
        ((int)TimeType.Year).Should().Be(7);
    }

    [Fact]
    public void TimeType_ShouldHaveExpectedCount()
    {
        var values = Enum.GetValues(typeof(TimeType));
        values.Length.Should().Be(8);
    }

    [Fact]
    public void TimeType_DefaultValue_ShouldBeYesterday()
    {
        var defaultValue = default(TimeType);
        defaultValue.Should().Be(TimeType.Yesterday);
    }

    [Theory]
    [InlineData("Yesterday", TimeType.Yesterday)]
    [InlineData("Today", TimeType.Today)]
    [InlineData("Tomorrow", TimeType.Tomorrow)]
    [InlineData("Week", TimeType.Week)]
    [InlineData("CurrentMonth", TimeType.CurrentMonth)]
    [InlineData("NextMonth", TimeType.NextMonth)]
    [InlineData("Season", TimeType.Season)]
    [InlineData("Year", TimeType.Year)]
    public void TimeType_Parse_ShouldReturnExpectedValue(string name, TimeType expected)
    {
        Enum.TryParse<TimeType>(name, out var result).Should().BeTrue();
        result.Should().Be(expected);
    }

    #endregion

    #region CoreGlobalConfig

    [Fact]
    public void CoreGlobalConfig_MinimumLevel_DefaultShouldBeInformation()
    {
        CoreGlobalConfig.MinimumLevel.Should().Be(LogLevel.Information);
    }

    [Fact]
    public void CoreGlobalConfig_IsClearLocalLog_DefaultShouldBeTrue()
    {
        CoreGlobalConfig.IsClearLocalLog.Should().BeTrue();
    }

    [Fact]
    public void CoreGlobalConfig_CleanupInterval_DefaultShouldBe7()
    {
        CoreGlobalConfig.CleanupInterval.Should().Be(7);
    }

    [Fact]
    public void CoreGlobalConfig_LogRetentionDays_DefaultShouldBe7()
    {
        CoreGlobalConfig.LogRetentionDays.Should().Be(7);
    }

    #endregion

    #region CommonCoreConst - _digitToChinese

    [Theory]
    [InlineData('0', "零")]
    [InlineData('1', "一")]
    [InlineData('2', "二")]
    [InlineData('3', "三")]
    [InlineData('4', "四")]
    [InlineData('5', "五")]
    [InlineData('6', "六")]
    [InlineData('7', "七")]
    [InlineData('8', "八")]
    [InlineData('9', "九")]
    public void DigitToChinese_ShouldMapDigitToChinese(char digit, string expected)
    {
        CommonCoreConst._digitToChinese.Should().ContainKey(digit);
        CommonCoreConst._digitToChinese[digit].Should().Be(expected);
    }

    [Fact]
    public void DigitToChinese_ShouldHave10Entries()
    {
        CommonCoreConst._digitToChinese.Count.Should().Be(10);
    }

    #endregion

    #region CommonCoreConst - FileFormats

    [Theory]
    [InlineData(".gif", "7173")]
    [InlineData(".jpg", "255216")]
    [InlineData(".jpeg", "255216")]
    [InlineData(".png", "13780")]
    [InlineData(".bmp", "6677")]
    [InlineData(".swf", "6787")]
    [InlineData(".flv", "7076")]
    [InlineData(".wma", "4838")]
    [InlineData(".wav", "8273")]
    [InlineData(".amr", "3533")]
    [InlineData(".mp4", "00")]
    [InlineData(".mp3", "255251")]
    [InlineData(".pdf", "3780")]
    [InlineData(".txt", "12334")]
    [InlineData(".zip", "8297")]
    public void FileFormats_ShouldContainExpectedMapping(string extension, string format)
    {
        CommonCoreConst.FileFormats.Should().ContainKey(extension);
        CommonCoreConst.FileFormats[extension].Should().Be(format);
    }

    [Fact]
    public void FileFormats_ShouldHave15Entries()
    {
        CommonCoreConst.FileFormats.Count.Should().Be(15);
    }

    [Fact]
    public void FileFormats_ShouldBeCaseInsensitive()
    {
        CommonCoreConst.FileFormats.ContainsKey(".GIF").Should().BeTrue();
        CommonCoreConst.FileFormats.ContainsKey(".JPG").Should().BeTrue();
        CommonCoreConst.FileFormats.ContainsKey(".PNG").Should().BeTrue();
    }

    #endregion

    #region CommonCoreConst - ContentTypeExtensionsMapping

    [Theory]
    [InlineData(".gif", "image/gif")]
    [InlineData(".jpg", "image/jpg")]
    [InlineData(".jpeg", "image/jpeg")]
    [InlineData(".png", "image/png")]
    [InlineData(".bmp", "application/x-bmp")]
    [InlineData(".mp3", "audio/mp3")]
    [InlineData(".wma", "audio/x-ms-wma")]
    [InlineData(".wav", "audio/wav")]
    [InlineData(".amr", "audio/amr")]
    [InlineData(".mp4", "video/mpeg4")]
    [InlineData(".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [InlineData(".pdf", "application/pdf")]
    [InlineData(".txt", "text/plain")]
    [InlineData(".doc", "application/msword")]
    [InlineData(".xls", "application/vnd.ms-excel")]
    [InlineData(".zip", "application/zip")]
    [InlineData(".csv", "text/csv")]
    [InlineData(".ppt", "application/vnd.ms-powerpoint")]
    [InlineData(".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation")]
    [InlineData(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    public void ContentTypeExtensionsMapping_ShouldContainExpectedMapping(string extension, string contentType)
    {
        CommonCoreConst.ContentTypeExtensionsMapping.Should().ContainKey(extension);
        CommonCoreConst.ContentTypeExtensionsMapping[extension].Should().Be(contentType);
    }

    [Fact]
    public void ContentTypeExtensionsMapping_ShouldHave20Entries()
    {
        CommonCoreConst.ContentTypeExtensionsMapping.Count.Should().Be(20);
    }

    #endregion
}
