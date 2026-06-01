using Xunit;

namespace Azrng.NmcWeather.Test
{
    public class NmcLocationClientIntegrationTest
    {
        private readonly INmcLocationClient _locationClient;

        public NmcLocationClientIntegrationTest(INmcLocationClient locationClient)
        {
            _locationClient = locationClient;
        }

        [Fact]
        public async Task GetProvincesAsync_ReturnNonEmptyList()
        {
            var provinces = await _locationClient.GetProvincesAsync();
            Assert.NotEmpty(provinces);
        }

        [Fact]
        public async Task GetProvinceByNameAsync_北京_ReturnProvince()
        {
            var province = await _locationClient.GetProvinceByNameAsync("北京");
            Assert.NotNull(province);
            Assert.Equal("ABJ", province!.Code);
        }

        [Fact]
        public async Task GetProvinceByCodeAsync_ABJ_ReturnProvince()
        {
            var province = await _locationClient.GetProvinceByCodeAsync("ABJ");
            Assert.NotNull(province);
            Assert.Contains("北京", province!.Name);
        }

        [Fact]
        public async Task GetCitiesByProvinceCodeAsync_ABJ_ReturnNonEmptyList()
        {
            var cities = await _locationClient.GetCitiesByProvinceCodeAsync("ABJ");
            Assert.NotEmpty(cities);
        }

        [Fact]
        public async Task GetCityByNameAsync_朝阳_WithProvinceName_ReturnCity()
        {
            var city = await _locationClient.GetCityByNameAsync("朝阳", provinceName: "北京");
            Assert.NotNull(city);
            Assert.NotEmpty(city!.Code);
        }

        [Fact]
        public async Task GetCityCodeByNameAsync_上海_ReturnCode()
        {
            var code = await _locationClient.GetCityCodeByNameAsync("上海", provinceName: "上海");
            Assert.NotNull(code);
            Assert.NotEmpty(code);
        }

        [Fact]
        public async Task GetCityCodeMapByProvinceNameAsync_北京_ReturnNonEmptyMap()
        {
            var map = await _locationClient.GetCityCodeMapByProvinceNameAsync("北京");
            Assert.NotEmpty(map);
        }

        [Fact]
        public async Task SearchCitiesByNameAsync_朝阳_ReturnNonEmptyList()
        {
            var cities = await _locationClient.SearchCitiesByNameAsync("朝阳");
            Assert.NotEmpty(cities);
        }
    }
}
