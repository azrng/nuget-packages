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

        [Theory]
        [InlineData("http://www.baidu.com/get/userinfo?userId=123&timestamp=13", "http://www.baidu.com")]
        public void GetUrl_Ok(string str, string url)
        {
            var result = UrlHelper.ExtractUrl(str);
            _testOutputHelper.WriteLine(result);
            Assert.Equal(url, result);
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
            Assert.Equal("http://www.baidu.com/get/userinfo?userId=123&timestamp=13&configId=123&bbb=654321", result);
        }

        [Fact]
        public void AddQueryString_ShouldIgnoreInvalidPairs()
        {
            var url = "http://www.baidu.com/get/userinfo";
            var queryPairs = new List<KeyValuePair<string, string>>
                             {
                                 new KeyValuePair<string, string>(null, "invalid"),
                                 new KeyValuePair<string, string>(" ", "also invalid"),
                                 new KeyValuePair<string, string>("config id", "hello world"),
                                 new KeyValuePair<string, string>("skip", null)
                             };

            // 只保留有效的键值对
            var result = UrlHelper.AddQueryString(url, queryPairs);
            Assert.Equal("http://www.baidu.com/get/userinfo?config+id=hello+world", result);
        }

        [Fact]
        public void AddQueryString_ShouldPreserveFragment()
        {
            var url = "https://demo.com/list?page=1#section";
            var queryPairs = new Dictionary<string, string>() { { "size", "20" } };

            // 锚点应该保持在末尾
            var result = UrlHelper.AddQueryString(url, queryPairs);
            Assert.Equal("https://demo.com/list?page=1&size=20#section", result);
        }
    }
}