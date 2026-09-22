using Azrng.NmcWeather.Models;
using Common.HttpClients;
using Moq;

namespace Azrng.NmcWeather.Test;

public static class IHttpHelperMockExtensions
{
    private const string BaseUrl = "http://www.nmc.cn";

    public static void SetupGetProvinces(this Mock<IHttpHelper> mock, List<NmcProvince> provinces)
    {
        mock.Setup(helper => helper.GetAsync<List<NmcProvince>>(
                $"{BaseUrl}/rest/province",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessResult(provinces));
    }

    public static void SetupGetCities(this Mock<IHttpHelper> mock, string provinceCode, List<NmcCity> cities)
    {
        mock.Setup(helper => helper.GetAsync<List<NmcCity>>(
                $"{BaseUrl}/rest/province/{provinceCode}",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessResult(cities));
    }

    public static IHttpResult<T> CreateSuccessResult<T>(T? data)
    {
        var result = new Mock<IHttpResult<T>>();
        result.SetupGet(item => item.Data).Returns(data);
        result.SetupGet(item => item.IsSuccess).Returns(true);
        return result.Object;
    }
}
