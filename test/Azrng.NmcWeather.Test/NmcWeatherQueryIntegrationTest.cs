using Xunit;

namespace Azrng.NmcWeather.Test
{
    public class NmcWeatherQueryIntegrationTest
    {
        private readonly INmcWeatherQueryClient _queryClient;

        public NmcWeatherQueryIntegrationTest(INmcWeatherQueryClient queryClient)
        {
            _queryClient = queryClient;
        }

        [Fact]
        public async Task GetWeatherByCityNameAsync_上海_ReturnOk()
        {
            var result = await _queryClient.GetWeatherByCityNameAsync("上海");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetWeatherByCityAsync_按名称_ReturnOk()
        {
            var result = await _queryClient.GetWeatherByCityAsync("上海", provinceName: "上海");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetWeatherByCityAsync_按名称_北京_ReturnOk()
        {
            var result = await _queryClient.GetWeatherByCityAsync("海淀", provinceName: "北京");
            Assert.NotNull(result);
        }
    }
}
