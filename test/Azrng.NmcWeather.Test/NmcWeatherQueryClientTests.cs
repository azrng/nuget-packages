using Azrng.NmcWeather.Models;
using Moq;
using Xunit;

namespace Azrng.NmcWeather.Test;

public class NmcWeatherQueryClientTests
{
    [Fact]
    public async Task GetWeatherByCityNameAsync_ShouldResolveCodeAndReturnWeather()
    {
        var locationClient = new Mock<INmcLocationClient>(MockBehavior.Strict);
        var weatherClient = new Mock<INmcWeatherClient>(MockBehavior.Strict);
        var expected = CreateWeatherEnvelope();

        locationClient.Setup(client => client.GetCityCodeByNameAsync("朝阳", "ABJ", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync("54433");
        weatherClient.Setup(client => client.GetWeatherByCityCodeAsync("54433", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var queryClient = new NmcWeatherQueryClient(locationClient.Object, weatherClient.Object);

        var weather = await queryClient.GetWeatherByCityNameAsync("朝阳", provinceCode: "ABJ");

        Assert.Same(expected, weather);
    }

    [Fact]
    public async Task GetWeatherByCityAsync_ShouldReturnNullWhenCodeCannotBeResolved()
    {
        var locationClient = new Mock<INmcLocationClient>(MockBehavior.Strict);
        var weatherClient = new Mock<INmcWeatherClient>(MockBehavior.Strict);

        locationClient.Setup(client => client.GetCityCodeAsync("不存在", null, "北京", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var queryClient = new NmcWeatherQueryClient(locationClient.Object, weatherClient.Object);

        var weather = await queryClient.GetWeatherByCityAsync("不存在", provinceName: "北京");

        Assert.Null(weather);
    }

    [Fact]
    public async Task GetWeatherByCityAsync_ShouldResolveCodeAndReturnWeather()
    {
        var locationClient = new Mock<INmcLocationClient>(MockBehavior.Strict);
        var weatherClient = new Mock<INmcWeatherClient>(MockBehavior.Strict);
        var expected = CreateWeatherEnvelope();

        locationClient.Setup(client => client.GetCityCodeAsync("朝阳", null, "北京", It.IsAny<CancellationToken>()))
            .ReturnsAsync("54433");
        weatherClient.Setup(client => client.GetWeatherByCityCodeAsync("54433", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var queryClient = new NmcWeatherQueryClient(locationClient.Object, weatherClient.Object);

        var weather = await queryClient.GetWeatherByCityAsync("朝阳", provinceName: "北京");

        Assert.Same(expected, weather);
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
                        City = "朝阳"
                    }
                }
            }
        };
    }
}
