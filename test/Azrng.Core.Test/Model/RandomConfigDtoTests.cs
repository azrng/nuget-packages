using System.Reflection;
using Azrng.Core.Model;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Model;

public class RandomConfigDtoTests
{
    private static readonly Type ConfigType = typeof(RandomConfigDto);

    private static string[] GetInternalStringArray(string fieldName)
    {
        var field = ConfigType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
        field.Should().NotBeNull($"field '{fieldName}' should exist");
        return (string[])field!.GetValue(null)!;
    }

    private static int[] GetInternalIntArray(string fieldName)
    {
        var field = ConfigType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
        field.Should().NotBeNull($"field '{fieldName}' should exist");
        return (int[])field!.GetValue(null)!;
    }

    [Fact]
    public void FirstNames_ShouldNotBeNullOrEmpty()
    {
        var names = GetInternalStringArray("FirstNames");
        names.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FirstNames_ShouldContainExpectedCount()
    {
        var names = GetInternalStringArray("FirstNames");
        names.Length.Should().Be(164);
    }

    [Theory]
    [InlineData("张")]
    [InlineData("王")]
    [InlineData("李")]
    [InlineData("赵")]
    [InlineData("陈")]
    public void FirstNames_ShouldContainCommonSurnames(string surname)
    {
        var names = GetInternalStringArray("FirstNames");
        names.Should().Contain(surname);
    }

    [Fact]
    public void FirstNames_ShouldNotContainDuplicates()
    {
        var names = GetInternalStringArray("FirstNames");
        names.Distinct().Count().Should().Be(names.Length);
    }

    [Fact]
    public void LastNames_ShouldNotBeNullOrEmpty()
    {
        var names = GetInternalStringArray("LastNames");
        names.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void LastNames_ShouldContainExpectedCount()
    {
        var names = GetInternalStringArray("LastNames");
        names.Length.Should().Be(280);
    }

    [Theory]
    [InlineData("伟")]
    [InlineData("芳")]
    [InlineData("强")]
    public void LastNames_ShouldContainCommonGivenNames(string givenName)
    {
        var names = GetInternalStringArray("LastNames");
        names.Should().Contain(givenName);
    }

    [Fact]
    public void AreaCodes_ShouldNotBeNullOrEmpty()
    {
        var codes = GetInternalStringArray("AreaCodes");
        codes.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AreaCodes_ShouldContainExpectedCount()
    {
        var codes = GetInternalStringArray("AreaCodes");
        codes.Length.Should().Be(56);
    }

    [Fact]
    public void AreaCodes_AllShouldBe6Digits()
    {
        var codes = GetInternalStringArray("AreaCodes");
        codes.Should().AllSatisfy(code => code.Length.Should().Be(6));
    }

    [Theory]
    [InlineData("110101")]
    [InlineData("310101")]
    [InlineData("440101")]
    public void AreaCodes_ShouldContainMajorCityPrefixes(string code)
    {
        var codes = GetInternalStringArray("AreaCodes");
        codes.Should().Contain(code);
    }

    [Fact]
    public void PhonePrefixes_ShouldNotBeNullOrEmpty()
    {
        var prefixes = GetInternalStringArray("_phonePrefixes");
        prefixes.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void PhonePrefixes_AllShouldStartWith1()
    {
        var prefixes = GetInternalStringArray("_phonePrefixes");
        prefixes.Should().AllSatisfy(p => p.Should().StartWith("1"));
    }

    [Fact]
    public void PhonePrefixes_AllShouldBe3Digits()
    {
        var prefixes = GetInternalStringArray("_phonePrefixes");
        prefixes.Should().AllSatisfy(p => p.Length.Should().Be(3));
    }

    [Fact]
    public void Provinces_ShouldNotBeNullOrEmpty()
    {
        var provinces = GetInternalStringArray("_provinces");
        provinces.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Provinces_ShouldContainExpectedCount()
    {
        var provinces = GetInternalStringArray("_provinces");
        provinces.Length.Should().Be(10);
    }

    [Theory]
    [InlineData("北京市")]
    [InlineData("上海市")]
    [InlineData("广东省")]
    public void Provinces_ShouldContainMajorProvinces(string province)
    {
        var provinces = GetInternalStringArray("_provinces");
        provinces.Should().Contain(province);
    }

    [Fact]
    public void Cities_ShouldNotBeNullOrEmpty()
    {
        var cities = GetInternalStringArray("_cities");
        cities.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Cities_ShouldContainExpectedCount()
    {
        var cities = GetInternalStringArray("_cities");
        cities.Length.Should().Be(3);
    }

    [Fact]
    public void Districts_ShouldNotBeNullOrEmpty()
    {
        var districts = GetInternalStringArray("_districts");
        districts.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Districts_ShouldContainExpectedCount()
    {
        var districts = GetInternalStringArray("_districts");
        districts.Length.Should().Be(10);
    }

    [Theory]
    [InlineData("朝阳区")]
    [InlineData("海淀区")]
    public void Districts_ShouldContainCommonDistricts(string district)
    {
        var districts = GetInternalStringArray("_districts");
        districts.Should().Contain(district);
    }

    [Fact]
    public void Streets_ShouldNotBeNullOrEmpty()
    {
        var streets = GetInternalStringArray("_streets");
        streets.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Streets_ShouldContainExpectedCount()
    {
        var streets = GetInternalStringArray("_streets");
        streets.Length.Should().Be(10);
    }

    [Theory]
    [InlineData("中山路")]
    [InlineData("人民路")]
    public void Streets_ShouldContainCommonStreets(string street)
    {
        var streets = GetInternalStringArray("_streets");
        streets.Should().Contain(street);
    }

    [Fact]
    public void BinCodes_ShouldNotBeNullOrEmpty()
    {
        var binCodes = GetInternalStringArray("_binCodes");
        binCodes.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void BinCodes_AllShouldBe6Digits()
    {
        var binCodes = GetInternalStringArray("_binCodes");
        binCodes.Should().AllSatisfy(code => code.Length.Should().Be(6));
    }

    [Fact]
    public void BinCodes_ShouldContainExpectedCount()
    {
        var binCodes = GetInternalStringArray("_binCodes");
        binCodes.Length.Should().Be(28);
    }

    [Fact]
    public void Domains_ShouldNotBeNullOrEmpty()
    {
        var domains = GetInternalStringArray("_domains");
        domains.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Domains_ShouldContainExpectedCount()
    {
        var domains = GetInternalStringArray("_domains");
        domains.Length.Should().Be(10);
    }

    [Theory]
    [InlineData("qq.com")]
    [InlineData("163.com")]
    [InlineData("gmail.com")]
    public void Domains_ShouldContainCommonEmailDomains(string domain)
    {
        var domains = GetInternalStringArray("_domains");
        domains.Should().Contain(domain);
    }

    [Fact]
    public void Weights_ShouldNotBeNullOrEmpty()
    {
        var weights = GetInternalIntArray("_weights");
        weights.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Weights_ShouldContainExpectedCount()
    {
        var weights = GetInternalIntArray("_weights");
        weights.Length.Should().Be(17);
    }

    [Fact]
    public void Weights_AllValuesShouldBeBetween1And10()
    {
        var weights = GetInternalIntArray("_weights");
        weights.Should().AllSatisfy(w =>
        {
            w.Should().BeGreaterThanOrEqualTo(1);
            w.Should().BeLessThanOrEqualTo(10);
        });
    }

    [Fact]
    public void CheckCodes_ShouldNotBeNullOrEmpty()
    {
        var codes = GetInternalStringArray("_checkCodes");
        codes.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CheckCodes_ShouldContainExpectedCount()
    {
        var codes = GetInternalStringArray("_checkCodes");
        codes.Length.Should().Be(11);
    }

    [Fact]
    public void CheckCodes_ShouldContainX()
    {
        var codes = GetInternalStringArray("_checkCodes");
        codes.Should().Contain("X");
    }

    [Fact]
    public void CheckCodes_ShouldContainAllDigits0To9()
    {
        var codes = GetInternalStringArray("_checkCodes");
        for (int i = 0; i <= 9; i++)
        {
            codes.Should().Contain(i.ToString());
        }
    }
}
