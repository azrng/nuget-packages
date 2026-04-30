using Azrng.NmcWeather.Models;
using Common.HttpClients;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Azrng.NmcWeather.Test;

public class NmcLocationClientTests
{
    private const string BaseUrl = "http://www.nmc.cn";

    [Fact]
    public async Task GetProvinceByCodeAsync_ShouldMatchIgnoringCase()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetProvinces([CreateProvince()]);
        });

        var province = await client.GetProvinceByCodeAsync("abj");

        Assert.NotNull(province);
        Assert.Equal("ABJ", province!.Code);
    }

    [Fact]
    public async Task GetProvinceAsync_ShouldMatchByProvinceNameWithoutSuffix()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetProvinces([CreateProvince()]);
        });

        var province = await client.GetProvinceAsync("北京");

        Assert.NotNull(province);
        Assert.Equal("ABJ", province!.Code);
    }

    [Fact]
    public async Task GetProvinceAsync_ShouldMatchByCode()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetProvinces([CreateProvince()]);
        });

        var province = await client.GetProvinceAsync("ABJ");

        Assert.NotNull(province);
        Assert.Equal("北京市", province!.Name);
    }

    [Fact]
    public async Task GetProvinceAsync_ShouldReturnNullWhenNotFound()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetProvinces([CreateProvince()]);
        });

        var province = await client.GetProvinceAsync("天津");

        Assert.Null(province);
    }

    [Fact]
    public async Task GetProvinceCodeAsync_ShouldReturnProvinceCode()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetProvinces([CreateProvince()]);
        });

        var provinceCode = await client.GetProvinceCodeAsync("北京市");

        Assert.Equal("ABJ", provinceCode);
    }

    [Fact]
    public async Task GetProvinceNameAsync_ShouldReturnProvinceName()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetProvinces([CreateProvince()]);
        });

        var provinceName = await client.GetProvinceNameAsync("ABJ");

        Assert.Equal("北京市", provinceName);
    }

    [Fact]
    public async Task GetProvinceCodeMapAsync_ShouldReturnNameCodeMap()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetProvinces([CreateProvince(), CreateProvince("AHE", "河北省")]);
        });

        var provinceCodeMap = await client.GetProvinceCodeMapAsync();

        Assert.Equal(2, provinceCodeMap.Count);
        Assert.Equal("ABJ", provinceCodeMap["北京市"]);
        Assert.Equal("AHE", provinceCodeMap["河北省"]);
    }

    [Fact]
    public async Task GetCitiesByProvinceAsync_ShouldResolveProvinceCodeFromName()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetProvinces([CreateProvince()]);
            mock.SetupGetCities("ABJ", [CreateCity()]);
        });

        var cities = await client.GetCitiesByProvinceAsync("北京");

        Assert.Single(cities);
        Assert.Equal("54433", cities[0].Code);
    }

    [Fact]
    public async Task GetCitiesByProvinceNameAsync_ShouldReturnEmptyWhenProvinceNotFound()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetProvinces([CreateProvince()]);
        });

        var cities = await client.GetCitiesByProvinceNameAsync("河北");

        Assert.Empty(cities);
    }

    [Fact]
    public async Task GetCityCodesByProvinceAsync_ShouldReturnDistinctCityCodes()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetProvinces([CreateProvince()]);
            mock.SetupGetCities("ABJ", [CreateCity(), CreateCity()]);
        });

        var cityCodes = await client.GetCityCodesByProvinceAsync("北京");

        Assert.Single(cityCodes);
        Assert.Equal("54433", cityCodes[0]);
    }

    [Fact]
    public async Task GetCityNamesByProvinceCodeAsync_ShouldReturnDistinctCityNames()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetCities("ABJ", [CreateCity(), CreateCity(city: "海淀")]);
        });

        var cityNames = await client.GetCityNamesByProvinceCodeAsync("ABJ");

        Assert.Equal(2, cityNames.Count);
        Assert.Contains("朝阳", cityNames);
        Assert.Contains("海淀", cityNames);
    }

    [Fact]
    public async Task GetCityCodeMapByProvinceNameAsync_ShouldReturnCityNameCodeMap()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetProvinces([CreateProvince()]);
            mock.SetupGetCities("ABJ", [CreateCity(), CreateCity("54399", city: "海淀")]);
        });

        var cityCodeMap = await client.GetCityCodeMapByProvinceNameAsync("北京");

        Assert.Equal(2, cityCodeMap.Count);
        Assert.Equal("54433", cityCodeMap["朝阳"]);
        Assert.Equal("54399", cityCodeMap["海淀"]);
    }

    [Fact]
    public async Task GetCityByCodeAsync_ShouldSearchAcrossProvinces()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetProvinces([CreateProvince(), CreateProvince("AHE", "河北省")]);
            mock.SetupGetCities("ABJ", []);
            mock.SetupGetCities("AHE", [CreateCity("53698", "河北省", "朝阳市")]);
        });

        var city = await client.GetCityByCodeAsync("53698");

        Assert.NotNull(city);
        Assert.Equal("河北省", city!.Province);
    }

    [Fact]
    public async Task GetCityByNameAsync_ShouldUseProvinceCodeWhenProvided()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetCities("ABJ", [CreateCity()]);
        });

        var city = await client.GetCityByNameAsync("朝阳", provinceCode: "ABJ");

        Assert.NotNull(city);
        Assert.Equal("54433", city!.Code);
    }

    [Fact]
    public async Task GetCityByNameAsync_ShouldUseProvinceNameWhenProvided()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetProvinces([CreateProvince()]);
            mock.SetupGetCities("ABJ", [CreateCity()]);
        });

        var city = await client.GetCityByNameAsync("朝阳市", provinceName: "北京");

        Assert.NotNull(city);
        Assert.Equal("54433", city!.Code);
    }

    [Fact]
    public async Task GetCityAsync_ShouldPreferCodeLookupForNumericInput()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetProvinces([CreateProvince()]);
            mock.SetupGetCities("ABJ", [CreateCity()]);
        });

        var city = await client.GetCityAsync("54433");

        Assert.NotNull(city);
        Assert.Equal("朝阳", city!.City);
    }

    [Fact]
    public async Task GetCityAsync_ShouldTreatMixedCaseStationCodeAsCode()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetProvinces([CreateProvince()]);
            mock.SetupGetCities("ABJ", [CreateCity("Wqsps", city: "北京")]);
        });

        var city = await client.GetCityAsync("Wqsps");

        Assert.NotNull(city);
        Assert.Equal("北京", city!.City);
    }

    [Fact]
    public async Task GetCityCodeByNameAsync_ShouldReturnCityCode()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetCities("ABJ", [CreateCity()]);
        });

        var cityCode = await client.GetCityCodeByNameAsync("朝阳", provinceCode: "ABJ");

        Assert.Equal("54433", cityCode);
    }

    [Fact]
    public async Task GetCityNameByCodeAsync_ShouldReturnCityName()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetProvinces([CreateProvince()]);
            mock.SetupGetCities("ABJ", [CreateCity()]);
        });

        var cityName = await client.GetCityNameByCodeAsync("54433");

        Assert.Equal("朝阳", cityName);
    }

    [Fact]
    public async Task GetCityCodeAsync_ShouldReturnCityCodeForNameInput()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetCities("ABJ", [CreateCity()]);
        });

        var cityCode = await client.GetCityCodeAsync("朝阳市", provinceCode: "ABJ");

        Assert.Equal("54433", cityCode);
    }

    [Fact]
    public async Task SearchCitiesByNameAsync_ShouldReturnMatchesAcrossProvinces()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetProvinces([CreateProvince(), CreateProvince("AHE", "河北省")]);
            mock.SetupGetCities("ABJ", [CreateCity()]);
            mock.SetupGetCities("AHE", [CreateCity("53698", "河北省", "朝阳市")]);
        });

        var cities = await client.SearchCitiesByNameAsync("朝阳");

        Assert.Equal(2, cities.Count);
    }

    [Fact]
    public async Task SearchCitiesByNameAsync_ShouldReturnEmptyWhenNoMatch()
    {
        var client = CreateClient(mock =>
        {
            mock.SetupGetProvinces([CreateProvince()]);
            mock.SetupGetCities("ABJ", [CreateCity()]);
        });

        var cities = await client.SearchCitiesByNameAsync("海淀");

        Assert.Empty(cities);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetProvinceByNameAsync_ShouldThrowWhenInputIsEmpty(string input)
    {
        var client = CreateClient(_ => { });

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => client.GetProvinceByNameAsync(input));

        Assert.Equal("provinceName", exception.ParamName);
    }

    private static NmcLocationClient CreateClient(Action<Mock<IHttpHelper>> setup)
    {
        var mock = new Mock<IHttpHelper>(MockBehavior.Strict);
        setup(mock);

        var options = Options.Create(new NmcWeatherOptions { BaseUrl = BaseUrl });

        return new NmcLocationClient(mock.Object, options);
    }

    private static NmcProvince CreateProvince(string code = "ABJ", string name = "北京市")
    {
        return new NmcProvince { Code = code, Name = name, Url = $"/publish/forecast/{code}.html" };
    }

    private static NmcCity CreateCity(string code = "54433", string province = "北京市", string city = "朝阳")
    {
        return new NmcCity { Code = code, Province = province, City = city, Url = $"/publish/forecast/{province}/{city}.html" };
    }
}
