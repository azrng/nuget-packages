namespace Common.Core.Test.Helper.UrlHelperTest
{
    public class ToDictFromQueryStringTest
    {
        [Fact]
        public void ToDictFromQueryString_Ok()
        {
            var url = "http://www.baidu.com/get/userinfo?userId=123&timestamp=13";
            var dict = UrlHelper.ToDictFromQueryString(new Uri(url));
            Assert.Equal(2, dict.Count);
        }
    }
}