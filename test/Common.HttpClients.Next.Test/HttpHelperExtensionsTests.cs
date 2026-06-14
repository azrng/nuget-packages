namespace Common.HttpClients.Next.Test
{
    /// <summary>
    /// HttpHelperExtensions.CreateBearerHeaders 测试
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
    }
}
