using Xunit.Abstractions;

namespace Common.Core.Test.Helper
{
    public class UrlHelperTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public UrlHelperTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void ToDictFromQueryString_Ok()
        {
            var url = "http://www.baidu.com/get/userinfo?userId=123&timestamp=13";
            var dict = UrlHelper.ToDictFromQueryString(new Uri(url));
            Assert.Equal(2, dict.Count);
        }

        [Fact]
        public void AddQueryString_Ok()
        {
            var url = "http://www.baidu.com/get/userinfo?userId=123&timestamp=13";
            var dict = new Dictionary<string, string>() { { "configId", "123" }, { "bbb", "654321" } };
            var result = UrlHelper.AddQueryString(url, dict);
            Assert.NotEmpty(result);
            _testOutputHelper.WriteLine(result);
        }
    }
}