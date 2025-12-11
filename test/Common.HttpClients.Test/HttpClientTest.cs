using AutoFixture;
using Common.HttpClients.Test.Dto;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Common.HttpClients.Test
{
    public class HttpClientTest
    {
        private readonly IHttpHelper _httpHelper;
        private readonly ITestOutputHelper _testOutputHelper;

        public HttpClientTest(IHttpHelper httpHelper, ITestOutputHelper testOutputHelper)
        {
            _httpHelper = httpHelper;
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData("https://se2.360simg.com/t0187b7a71a740c691b.png")]
        public async Task GetStream_ReturnTrue(string url)
        {
            var result = await _httpHelper.GetAsync(url, timeout: 1);
            Assert.True(result.Length > 1);
        }

        [Theory]
        [InlineData("https://jsonplaceholder.typicode.com/posts/1")]
        public async Task Get_Valid_ReturnTrue(string url)
        {
            var result = await _httpHelper.GetAsync<GetBlogDetailsResponse>(url);
            _testOutputHelper.WriteLine(JsonConvert.SerializeObject(result));
            Assert.True(result.id == 1);
        }

        [Theory]
        [InlineData("https://jsonplaceholder.typicode.com/posts/1", "token")]
        public async Task GetAndToken_Valid_ReturnTrue(string url, string token)
        {
            var result = await _httpHelper.GetAsync<GetBlogDetailsResponse>(url, token);
            Assert.True(result.id == 1);
        }

        [Theory]
        [InlineData("https://jsonplaceholder.typicode.com/posts")]
        public async Task Post_Valid_ReturnTrue(string url)
        {
            var fixture = new Fixture();
            var sut = fixture.Create<AddBlogRequest>();
            var result = await _httpHelper.PostAsync<GetBlogDetailsResponse>(url, sut);
            Assert.True(result.title == sut.title);
        }

        [Theory]
        [InlineData("https://jsonplaceholder.typicode.com/posts/1")]
        public async Task Put_Valid_ReturnTrue(string url)
        {
            var fixture = new Fixture();
            var sut = fixture.Create<AddBlogRequest>();
            var result = await _httpHelper.PutAsync<GetBlogDetailsResponse>(url, sut);
            Assert.True(result.title == sut.title);
        }

        [Theory]
        [InlineData("https://jsonplaceholder.typicode.com/posts/2000000")]
        public async Task Put_Invalid_ReturnTrue(string url)
        {
            //var result = await Assert.ThrowsAsync<Exception>(() => _httpHelper.PutAsync<GetBlogDetailsResponse>(url, sut));
            //Assert.True(result is not null);

            try
            {
                var sut = new AddBlogRequest { id = 100000000, body = null };
                var result = await _httpHelper.PutAsync<GetBlogDetailsResponse>(url, sut);
                _testOutputHelper.WriteLine("未抛出异常" + JsonConvert.SerializeObject(result));
            }
            catch (Exception ex)
            {
                _testOutputHelper.WriteLine("抛出异常" + ex.Message);
            }
        }

        [Theory]
        [InlineData("https://jsonplaceholder.typicode.com/posts/1")]
        public async Task Delete_Valid_ReturnTrue(string url)
        {
            var result = await _httpHelper.DeleteAsync(url);
            Assert.False(string.IsNullOrWhiteSpace(result));
        }

        /// <summary>
        /// 校验不安全证书
        /// </summary>
        [Fact]
        public async Task Verify_Untrusted_Certificate_ReturnTrue()
        {
            try
            {
                var url = "https://sso-stable.synyi.sy/api/health/alive?ngsw-bypass=true";
                var result = await _httpHelper.GetAsync(url);
                Assert.False(string.IsNullOrWhiteSpace(result));
            }
            catch (Exception ex)
            {
                _testOutputHelper.WriteLine("抛出异常：" + ex.Message);
                Assert.True(ex is HttpRequestException);
            }
        }
    }
}