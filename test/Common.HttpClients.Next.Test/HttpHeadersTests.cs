namespace Common.HttpClients.Next.Test
{
    /// <summary>
    /// HttpHeaders 多值请求头集合行为测试
    /// </summary>
    public class HttpHeadersTests
    {
        [Fact]
        public void Indexer_SetSingleValue_ShouldOverwrite()
        {
            var headers = new HttpHeaders { ["Authorization"] = "a" };

            headers["Authorization"] = "b";

            Assert.Equal("b", headers["Authorization"]);
            Assert.Single(headers.GetValues("Authorization"));
        }

        [Fact]
        public void Add_SingleValue_ShouldAccumulate()
        {
            var headers = new HttpHeaders();

            headers.Add("Accept", "application/json");
            headers.Add("Accept", "text/plain");

            Assert.Equal(2, headers.GetValues("Accept").Count);
            Assert.Equal("application/json", headers.GetValues("Accept")[0]);
            Assert.Equal("text/plain", headers.GetValues("Accept")[1]);
        }

        [Fact]
        public void Add_MultiValue_ShouldAccumulate()
        {
            var headers = new HttpHeaders();

            headers.Add("Accept", new[] { "application/json", "text/plain" });

            Assert.Equal(2, headers.GetValues("Accept").Count);
        }

        [Fact]
        public void GetValues_NotExist_ShouldReturnEmpty()
        {
            var headers = new HttpHeaders();

            Assert.Empty(headers.GetValues("Missing"));
        }

        [Fact]
        public void Indexer_GetNotExist_ShouldThrowKeyNotFound()
        {
            var headers = new HttpHeaders();

            Assert.Throws<KeyNotFoundException>(() => headers["Missing"]);
        }

        [Fact]
        public void Keys_ShouldBeCaseInsensitive()
        {
            var headers = new HttpHeaders { ["Authorization"] = "a" };

            Assert.True(headers.ContainsKey("authorization"));
            Assert.Equal("a", headers["AUTHORIZATION"]);
        }

        [Fact]
        public void Enumeration_ShouldYieldAllHeadersWithCounts()
        {
            var headers = new HttpHeaders { ["Authorization"] = "a" };
            headers.Add("Accept", "b");

            Assert.Equal(2, headers.Count);
            var dict = headers.ToDictionary(kv => kv.Key, kv => kv.Value.Count);
            Assert.Equal(1, dict["Authorization"]);
            Assert.Equal(1, dict["Accept"]);
        }

        [Fact]
        public void Remove_ShouldDeleteHeader()
        {
            var headers = new HttpHeaders { ["Authorization"] = "a" };

            Assert.True(headers.Remove("Authorization"));
            Assert.False(headers.ContainsKey("Authorization"));
        }
    }
}
