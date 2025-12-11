using Xunit;
using Xunit.Abstractions;

namespace Common.HttpClients.Test
{
    public class RetryTest
    {
        private readonly IHttpHelper _httpHelper;
        private readonly ITestOutputHelper _testOutputHelper;

        public RetryTest(IHttpHelper httpHelper, ITestOutputHelper testOutputHelper)
        {
            _httpHelper = httpHelper;
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Get_ReturnOk()
        {
            var result = await _httpHelper.GetAsync<string>("http://localhost:5138/Home");
            _testOutputHelper.WriteLine("请求结束");
            _testOutputHelper.WriteLine("响应结果：" + result);
        }
    }
}