using Azrng.NmcWeather.Models;
using Common.HttpClients;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Azrng.NmcWeather.Test;

public class NmcWeatherClientTests
{
    private const string BaseUrl = "http://www.nmc.cn";

    [Fact]
    public async Task GetWeatherByCityCodeAsync_ShouldBuildRequestUrl()
    {
        var expected = CreateWeatherEnvelope();
        var client = CreateClient(mock =>
        {
            mock.Setup(helper => helper.GetAsync<NmcWeatherEnvelope>(
                    $"{BaseUrl}/rest/weather?stationid=54433",
                    string.Empty,
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);
        });

        var weather = await client.GetWeatherByCityCodeAsync("54433");

        Assert.Same(expected, weather);
    }

    [Fact]
    public async Task GetWeatherByCityCodeAsync_ShouldPreserveMixedCaseStationCode()
    {
        var expected = CreateWeatherEnvelope();
        var client = CreateClient(mock =>
        {
            mock.Setup(helper => helper.GetAsync<NmcWeatherEnvelope>(
                    $"{BaseUrl}/rest/weather?stationid=Wqsps",
                    string.Empty,
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);
        });

        var weather = await client.GetWeatherByCityCodeAsync("Wqsps");

        Assert.Same(expected, weather);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetWeatherByCityCodeAsync_ShouldThrowWhenInputIsEmpty(string input)
    {
        var client = CreateClient(_ => { });

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => client.GetWeatherByCityCodeAsync(input));

        Assert.Equal("cityCode", exception.ParamName);
    }

    [Fact]
    public async Task GetWeatherByCityCodeAsync_ShouldReturnNullWhenApiReturnsNull()
    {
        var client = CreateClient(mock =>
        {
            mock.Setup(helper => helper.GetAsync<NmcWeatherEnvelope>(
                    $"{BaseUrl}/rest/weather?stationid=54433",
                    string.Empty,
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((NmcWeatherEnvelope?)null);
        });

        var weather = await client.GetWeatherByCityCodeAsync("54433");

        Assert.Null(weather);
    }

    [Fact]
    public void Constructor_ShouldThrowWhenHttpHelperIsNull()
    {
        var options = Options.Create(new NmcWeatherOptions { BaseUrl = BaseUrl });

        var exception = Assert.Throws<ArgumentNullException>(() => new NmcWeatherClient(null!, options));

        Assert.Equal("httpHelper", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldThrowWhenOptionsIsNull()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new NmcWeatherClient(new Mock<IHttpHelper>().Object, null!));

        Assert.Equal("options", exception.ParamName);
    }

    private static NmcWeatherClient CreateClient(Action<Mock<IHttpHelper>> setup)
    {
        var mock = new Mock<IHttpHelper>(MockBehavior.Strict);
        setup(mock);

        var options = Options.Create(new NmcWeatherOptions { BaseUrl = BaseUrl });

        return new NmcWeatherClient(mock.Object, options);
    }

    private static NmcWeatherEnvelope CreateWeatherEnvelope()
    {
        return new NmcWeatherEnvelope
        {
            Msg = "success",
            Code = 0,
            Data = new NmcWeatherData
            {
                Real = new NmcRealWeather
                {
                    Station = new NmcStation
                    {
                        Code = "54433",
                        Province = "北京市",
                        City = "朝阳",
                        Url = "/publish/forecast/ABJ/chaoyang.html"
                    }
                }
            }
        };
    }
}
