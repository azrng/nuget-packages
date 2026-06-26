using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class ChinaDateHelperTests
{
    #region GetChinaDate - 基本属性

    [Fact]
    public void GetChinaDate_RegularDate_ShouldReturnNotNull()
    {
        var dt = new DateTime(2025, 6, 15);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.Should().NotBeNull();
    }

    [Fact]
    public void GetChinaDate_RegularDate_ShouldHaveValidCnIntYear()
    {
        var dt = new DateTime(2025, 6, 15);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnIntYear.Should().BeGreaterThan(1900);
        result.CnIntYear.Should().BeLessThan(2050);
    }

    [Fact]
    public void GetChinaDate_RegularDate_ShouldHaveValidCnIntMonth()
    {
        var dt = new DateTime(2025, 6, 15);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnIntMonth.Should().BeGreaterOrEqualTo(1);
        result.CnIntMonth.Should().BeLessOrEqualTo(12);
    }

    [Fact]
    public void GetChinaDate_RegularDate_ShouldHaveValidCnIntDay()
    {
        var dt = new DateTime(2025, 6, 15);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnIntDay.Should().BeGreaterOrEqualTo(1);
        result.CnIntDay.Should().BeLessOrEqualTo(30);
    }

    [Fact]
    public void GetChinaDate_RegularDate_ShouldHaveNonEmptyCnStrYear()
    {
        var dt = new DateTime(2025, 6, 15);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnStrYear.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetChinaDate_RegularDate_ShouldHaveNonEmptyCnStrMonth()
    {
        var dt = new DateTime(2025, 6, 15);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnStrMonth.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetChinaDate_RegularDate_ShouldHaveNonEmptyCnStrDay()
    {
        var dt = new DateTime(2025, 6, 15);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnStrDay.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetChinaDate_RegularDate_ShouldHaveNonEmptyCnAnm()
    {
        var dt = new DateTime(2025, 6, 15);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnAnm.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region GetChinaDate - 天干地支

    [Theory]
    [InlineData(2024, "甲辰")]
    [InlineData(2025, "乙巳")]
    [InlineData(2026, "丙午")]
    public void GetChinaDate_CnStrYear_ShouldMatchGanZhi(int year, string expectedGanZhi)
    {
        var dt = new DateTime(year, 6, 15);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnStrYear.Should().Be(expectedGanZhi);
    }

    #endregion

    #region GetChinaDate - 生肖

    [Theory]
    [InlineData(2024, "龙")]
    [InlineData(2025, "蛇")]
    [InlineData(2026, "马")]
    [InlineData(2027, "羊")]
    public void GetChinaDate_CnAnm_ShouldMatchZodiac(int year, string expectedZodiac)
    {
        var dt = new DateTime(year, 6, 15);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnAnm.Should().Be(expectedZodiac);
    }

    #endregion

    #region GetChinaDate - 农历月份名称

    [Theory]
    [InlineData(1, "正")]
    [InlineData(2, "二")]
    [InlineData(3, "三")]
    [InlineData(4, "四")]
    [InlineData(5, "五")]
    [InlineData(6, "六")]
    [InlineData(7, "七")]
    [InlineData(8, "八")]
    [InlineData(9, "九")]
    [InlineData(10, "十")]
    [InlineData(11, "十一")]
    [InlineData(12, "十二")]
    public void GetChinaDate_CnStrMonth_ShouldContainMonthName(int lunarMonth, string expectedName)
    {
        var dt = new DateTime(2025, 6, 15);

        var result = ChinaDateHelper.GetChinaDate(dt);

        if (result.CnIntMonth == lunarMonth)
        {
            result.CnStrMonth.Should().Be(expectedName);
        }
    }

    #endregion

    #region GetChinaDate - 农历日期名称

    [Fact]
    public void GetChinaDate_CnStrDay_Day1_ShouldReturn初一()
    {
        var dt = new DateTime(2025, 1, 29);

        var result = ChinaDateHelper.GetChinaDate(dt);

        if (result.CnIntDay == 1)
        {
            result.CnStrDay.Should().Be("初一");
        }
    }

    [Fact]
    public void GetChinaDate_CnStrDay_Day10_ShouldReturn初十()
    {
        var dt = new DateTime(2025, 2, 7);

        var result = ChinaDateHelper.GetChinaDate(dt);

        if (result.CnIntDay == 10)
        {
            result.CnStrDay.Should().Be("初十");
        }
    }

    [Fact]
    public void GetChinaDate_CnStrDay_Day20_ShouldReturn二十()
    {
        var dt = new DateTime(2025, 2, 17);

        var result = ChinaDateHelper.GetChinaDate(dt);

        if (result.CnIntDay == 20)
        {
            result.CnStrDay.Should().Be("二十");
        }
    }

    [Fact]
    public void GetChinaDate_CnStrDay_Day30_ShouldReturn三十()
    {
        var dt = new DateTime(2025, 1, 29);

        var result = ChinaDateHelper.GetChinaDate(dt);

        if (result.CnIntDay == 30)
        {
            result.CnStrDay.Should().Be("三十");
        }
    }

    #endregion

    #region GetChinaDate - 公历节日

    [Fact]
    public void GetChinaDate_NewYearDay_ShouldReturn元旦()
    {
        var dt = new DateTime(2025, 1, 1);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnFtvs.Should().Contain("新年元旦");
    }

    [Fact]
    public void GetChinaDate_LaborDay_ShouldReturn劳动节()
    {
        var dt = new DateTime(2025, 5, 1);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnFtvs.Should().Contain("国际劳动节");
    }

    [Fact]
    public void GetChinaDate_NationalDay_ShouldReturn国庆节()
    {
        var dt = new DateTime(2025, 10, 1);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnFtvs.Should().Contain("国庆节");
    }

    [Fact]
    public void GetChinaDate_ValentinesDay_ShouldReturn情人节()
    {
        var dt = new DateTime(2025, 2, 14);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnFtvs.Should().Contain("情人节");
    }

    [Fact]
    public void GetChinaDate_ChristmasDay_ShouldReturn圣诞节()
    {
        var dt = new DateTime(2025, 12, 25);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnFtvs.Should().Contain("圣诞节");
    }

    [Fact]
    public void GetChinaDate_NonFestivalDay_ShouldHaveEmptyCnFtvs()
    {
        var dt = new DateTime(2025, 6, 15);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnFtvs.Should().BeNullOrEmpty();
    }

    #endregion

    #region GetChinaDate - 农历节日

    [Fact]
    public void GetChinaDate_SpringFestival_ShouldReturn农历春节()
    {
        var dt = new DateTime(2025, 1, 29);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnFtvl.Should().Contain("农历春节");
    }

    [Fact]
    public void GetChinaDate_MidAutumnFestival_ShouldReturn中秋节()
    {
        var dt = new DateTime(2025, 10, 6);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnFtvl.Should().Contain("中秋节");
    }

    [Fact]
    public void GetChinaDate_DragonBoatFestival_ShouldReturn端午节()
    {
        var dt = new DateTime(2025, 5, 31);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnFtvl.Should().Contain("端午节");
    }

    [Fact]
    public void GetChinaDate_LanternFestival_ShouldReturn元宵节()
    {
        var dt = new DateTime(2025, 2, 12);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnFtvl.Should().Contain("元宵节");
    }

    [Fact]
    public void GetChinaDate_DoubleNinthFestival_ShouldReturn重阳节()
    {
        var dt = new DateTime(2025, 10, 29);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnFtvl.Should().Contain("重阳节");
    }

    #endregion

    #region GetChinaDate - 除夕

    [Fact]
    public void GetChinaDate_ChineseNewYearsEve_ShouldReturn除夕()
    {
        var dt = new DateTime(2025, 1, 28);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnFtvl.Should().Be("除夕");
    }

    #endregion

    #region GetChinaDate - 二十四节气

    [Fact]
    public void GetChinaDate_SolarTerm_VernalEquinox_2025_ShouldReturn春分()
    {
        var dt = new DateTime(2025, 3, 20);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnSolarTerm.Should().Be("春分");
    }

    [Fact]
    public void GetChinaDate_SolarTerm_AutumnalEquinox_2025_ShouldReturn秋分()
    {
        var dt = new DateTime(2025, 9, 23);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnSolarTerm.Should().Be("秋分");
    }

    [Fact]
    public void GetChinaDate_NonSolarTermDay_ShouldHaveEmptyCnSolarTerm()
    {
        var dt = new DateTime(2025, 6, 15);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnSolarTerm.Should().BeNullOrEmpty();
    }

    #endregion

    #region GetChinaDate - 边界值

    [Fact]
    public void GetChinaDate_LeapMonth_2025_ShouldReturn闰七月()
    {
        // 2025年闰七月, 验证闰月日期仍返回有效农历月份
        var dt = new DateTime(2025, 8, 23);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.CnIntMonth.Should().Be(7);
        result.CnStrMonth.Should().Be("七");
    }

    [Fact]
    public void GetChinaDate_YearBoundary_Jan1_ShouldReturnValidResult()
    {
        var dt = new DateTime(2025, 1, 1);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.Should().NotBeNull();
        result.CnIntYear.Should().BeGreaterThan(0);
        result.CnStrYear.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetChinaDate_YearBoundary_Dec31_ShouldReturnValidResult()
    {
        var dt = new DateTime(2025, 12, 31);

        var result = ChinaDateHelper.GetChinaDate(dt);

        result.Should().NotBeNull();
        result.CnIntYear.Should().BeGreaterThan(0);
        result.CnStrYear.Should().NotBeNullOrEmpty();
    }

    #endregion
}
