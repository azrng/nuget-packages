using Xunit.Abstractions;

namespace Common.Core.Test.Helper.UrlHelperTest
{
    public class ExtractUrlTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ExtractUrlTest(ITestOutputHelper testOutputHelper)
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
    }
}