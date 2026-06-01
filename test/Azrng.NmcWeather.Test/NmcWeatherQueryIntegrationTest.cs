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
        public async Task QueryCity_ReturnOk()
        {
            var result = await _queryClient.GetWeatherByCityNameAsync("上海");
            Assert.True(result is not null);
        }
    }
}