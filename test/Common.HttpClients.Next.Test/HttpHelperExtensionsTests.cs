namespace Common.HttpClients.Next.Test
{
    /// <summary>
    /// HttpHelperExtensions.CreateBearerHeaders / EnsureSuccess 测试
    /// </summary>
    public class HttpHelperExtensionsTests
    {
        [Fact]
        public void CreateBearerHeaders_WithoutBearerPrefix_ShouldAutoAppendBearer()
        {
            var headers = HttpHelperExtensions.CreateBearerHeaders("token-abc");

            Assert.Equal("Bearer token-abc", headers["Authorization"]);
        }

        [Fact]
        public void CreateBearerHeaders_WithBearerPrefix_ShouldNotDuplicate()
        {
            var headers = HttpHelperExtensions.CreateBearerHeaders("Bearer token-abc");

            Assert.Equal("Bearer token-abc", headers["Authorization"]);
        }

        [Fact]
        public void CreateBearerHeaders_WithBearerLowercase_ShouldNotDuplicate()
        {
            var headers = HttpHelperExtensions.CreateBearerHeaders("bearer token-abc");

            Assert.Equal("bearer token-abc", headers["Authorization"]);
        }

        [Fact]
        public void CreateBearerHeaders_NullToken_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => HttpHelperExtensions.CreateBearerHeaders(null!));
        }

        [Fact]
        public void CreateBearerHeaders_EmptyToken_ShouldReturnBearerWithoutPayload()
        {
            var headers = HttpHelperExtensions.CreateBearerHeaders("");

            Assert.Equal("Bearer ", headers["Authorization"]);
        }

        [Fact]
        public void EnsureSuccess_WhenSuccess_ShouldReturnSelf()
        {
            var result = HttpResult<string>.Success("ok", HttpStatusCode.OK, "ok");

            var returned = result.EnsureSuccess();

            Assert.Same(result, returned);
            Assert.Equal("ok", returned.Data);
        }

        [Fact]
        public void EnsureSuccess_WhenFailed_ShouldThrowHttpRequestException()
        {
            var result = HttpResult<string>.Fail("err", HttpStatusCode.InternalServerError, "err", false);

            var ex = Assert.Throws<HttpRequestException>(() => result.EnsureSuccess());
            Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        }

        [Fact]
        public void EnsureSuccess_NullResult_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ((IHttpResult<string>)null!).EnsureSuccess());
        }
    }
}
