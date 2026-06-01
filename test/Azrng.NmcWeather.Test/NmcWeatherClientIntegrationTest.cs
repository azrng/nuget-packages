using Xunit;

namespace Azrng.NmcWeather.Test
{
    public class NmcWeatherClientIntegrationTest
    {
        private readonly INmcWeatherClient _weatherClient;
        private readonly INmcLocationClient _locationClient;

        public NmcWeatherClientIntegrationTest(INmcWeatherClient weatherClient, INmcLocationClient locationClient)
        {
            _weatherClient = weatherClient;
            _locationClient = locationClient;
        }

        [Fact]
        public async Task GetWeatherByCityCodeAsync_有效编码_ReturnWeather()
        {
            var cityCode = await _locationClient.GetCityCodeByNameAsync("上海", provinceName: "上海");
            Assert.NotNull(cityCode);

            var weather = await _weatherClient.GetWeatherByCityCodeAsync(cityCode!);
            Assert.NotNull(weather);
            Assert.NotNull(weather!.Data);
        }

        [Fact]
        public async Task GetWeatherByCityCodeAsync_北京_ReturnWeather()
        {
            var cityCode = await _locationClient.GetCityCodeByNameAsync("海淀", provinceName: "北京");
            Assert.NotNull(cityCode);

            var weather = await _weatherClient.GetWeatherByCityCodeAsync(cityCode!);
            Assert.NotNull(weather);
        }
    }
}
