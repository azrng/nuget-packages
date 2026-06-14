namespace Common.HttpClients.Next.Test
{
    /// <summary>
    /// HttpResult&lt;T&gt; 构造逻辑测试
    /// </summary>
    public class HttpResultTests
    {
        [Fact]
        public void Success_ShouldSetIsSuccessTrue_AndNullErrorMessage()
        {
            var result = HttpResult<string>.Success("data", HttpStatusCode.OK, "raw");

            Assert.True(result.IsSuccess);
            Assert.Equal("data", result.Data);
            Assert.Null(result.ErrorMessage);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("raw", result.RawBody);
            Assert.False(result.IsFallbackResponse);
        }

        [Fact]
        public void Fail_ShouldSetIsSuccessFalse_AndErrorMessage()
        {
            var result = HttpResult<string>.Fail("error", HttpStatusCode.InternalServerError, "raw", true);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.Equal("error", result.ErrorMessage);
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.Equal("raw", result.RawBody);
            Assert.True(result.IsFallbackResponse);
        }

        [Fact]
        public void Success_WithNullableReferenceType_ShouldAcceptNullData()
        {
            var result = HttpResult<string?>.Success(null, HttpStatusCode.NoContent, "");

            Assert.True(result.IsSuccess);
            Assert.Null(result.Data);
        }

        [Fact]
        public void Success_WithValueType_ShouldAcceptDefaultStruct()
        {
            var result = HttpResult<int>.Success(default, HttpStatusCode.OK, "0");

            Assert.True(result.IsSuccess);
            Assert.Equal(0, result.Data);
        }

        [Fact]
        public void Fail_ShouldCarryStatusCodeAndBody()
        {
            var result = HttpResult<int>.Fail("not found", HttpStatusCode.NotFound, "raw", false);

            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal("raw", result.RawBody);
            Assert.False(result.IsFallbackResponse);
        }
    }
}
