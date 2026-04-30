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
                string.Empty,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(provinces);
    }

    public static void SetupGetCities(this Mock<IHttpHelper> mock, string provinceCode, List<NmcCity> cities)
    {
        mock.Setup(helper => helper.GetAsync<List<NmcCity>>(
                $"{BaseUrl}/rest/province/{provinceCode}",
                string.Empty,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cities);
    }
}
