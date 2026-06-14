namespace Common.HttpClients.Next.Test
{
    /// <summary>
    /// DefaultHttpLogRedactor 脱敏行为测试
    /// </summary>
    public class LoggingRedactionTests
    {
        [Fact]
        public void RedactHeaders_SensitiveHeaders_ShouldBeReplacedWithStars()
        {
            var redactor = NewRedactor();
            var headers = new Dictionary<string, string>
            {
                ["Authorization"] = "Bearer abc",
                ["X-Api-Key"] = "key",
                ["Accept"] = "application/json"
            };

            var redacted = redactor.RedactHeaders(headers);

            Assert.Equal("***", redacted["Authorization"]);
            Assert.Equal("***", redacted["X-Api-Key"]);
            Assert.Equal("application/json", redacted["Accept"]);
        }

        [Fact]
        public void RedactHeaders_AdditionalSensitiveHeaders_ShouldBeHonored()
        {
            var redactor = NewRedactor(additionalHeaders: new[] { "X-Tenant" });
            var headers = new Dictionary<string, string>
            {
                ["X-Tenant"] = "tenant-001",
                ["X-Other"] = "ok"
            };

            var redacted = redactor.RedactHeaders(headers);

            Assert.Equal("***", redacted["X-Tenant"]);
            Assert.Equal("ok", redacted["X-Other"]);
        }

        [Fact]
        public void RedactHeaders_CaseInsensitiveMatch()
        {
            var redactor = NewRedactor(additionalHeaders: new[] { "X-Secret" });
            var headers = new Dictionary<string, string>
            {
                ["x-secret"] = "lower",
                ["X-SECRET"] = "upper"
            };

            var redacted = redactor.RedactHeaders(headers);

            Assert.Equal("***", redacted["x-secret"]);
            Assert.Equal("***", redacted["X-SECRET"]);
        }

        [Fact]
        public void RedactHeaders_Null_ShouldReturnEmptyDictionary()
        {
            var redacted = NewRedactor().RedactHeaders(null);
            Assert.NotNull(redacted);
            Assert.Empty(redacted);
        }

        [Fact]
        public void RedactHeaders_Empty_ShouldReturnEmptyDictionary()
        {
            var redacted = NewRedactor().RedactHeaders(new Dictionary<string, string>());
            Assert.Empty(redacted);
        }

        [Fact]
        public void RedactContent_JsonSensitiveFields_ShouldBeReplaced()
        {
            var redactor = NewRedactor();
            var json = "{\"password\":\"abc\",\"name\":\"az\"}";

            var redacted = redactor.RedactContent(json);

            using var doc = JsonDocument.Parse(redacted);
            Assert.Equal("***", doc.RootElement.GetProperty("password").GetString());
            Assert.Equal("az", doc.RootElement.GetProperty("name").GetString());
        }

        [Fact]
        public void RedactContent_NestedSensitiveFields_ShouldBeReplaced()
        {
            var redactor = NewRedactor();
            var json = "{\"nested\":{\"access_token\":\"hidden-token\"},\"items\":[{\"api_key\":\"k\"}]}";

            var redacted = redactor.RedactContent(json);

            using var doc = JsonDocument.Parse(redacted);
            Assert.Equal("***", doc.RootElement.GetProperty("nested").GetProperty("access_token").GetString());
            Assert.Equal("***", doc.RootElement.GetProperty("items")[0].GetProperty("api_key").GetString());
        }

        [Fact]
        public void RedactContent_BearerToken_ShouldBeRedactedInStringValue()
        {
            var redactor = NewRedactor();
            var json = "{\"auth\":\"Bearer super-secret\"}";

            var redacted = redactor.RedactContent(json);

            using var doc = JsonDocument.Parse(redacted);
            Assert.Equal("Bearer ***", doc.RootElement.GetProperty("auth").GetString());
        }

        [Fact]
        public void RedactContent_AdditionalSensitiveFields_ShouldBeHonored()
        {
            var redactor = NewRedactor(additionalFields: new[] { "mobile" });
            var json = "{\"mobile\":\"13800000000\",\"name\":\"az\"}";

            var redacted = redactor.RedactContent(json);

            using var doc = JsonDocument.Parse(redacted);
            Assert.Equal("***", doc.RootElement.GetProperty("mobile").GetString());
            Assert.Equal("az", doc.RootElement.GetProperty("name").GetString());
        }

        [Fact]
        public void RedactContent_NonJson_ShouldFallbackToKeyValueRedaction()
        {
            var redactor = NewRedactor();
            var kv = "password=abc&name=az";

            var redacted = redactor.RedactContent(kv);

            Assert.Contains("password=***", redacted);
            Assert.Contains("name=az", redacted);
        }

        [Fact]
        public void RedactContent_PlaintextBearer_ShouldBeRedacted()
        {
            var redactor = NewRedactor();
            var text = "Authorization: Bearer abc.def-ghi";

            var redacted = redactor.RedactContent(text);

            Assert.Contains("Bearer ***", redacted);
            Assert.DoesNotContain("abc.def-ghi", redacted);
        }

        [Fact]
        public void RedactContent_Empty_ShouldReturnAsIs()
        {
            Assert.Equal("", NewRedactor().RedactContent(""));
        }

        [Fact]
        public void RedactContent_PreservesJsonEscapedQuotes()
        {
            var redactor = NewRedactor();
            var json = "{\"password\":\"a\\\"b\"}";

            var redacted = redactor.RedactContent(json);

            using var doc = JsonDocument.Parse(redacted);
            Assert.Equal("***", doc.RootElement.GetProperty("password").GetString());
        }

        [Fact]
        public void RedactContent_ArrayValues_ShouldBePreserved()
        {
            var redactor = NewRedactor();
            var json = "{\"tags\":[\"a\",\"b\"],\"secret\":\"x\"}";

            var redacted = redactor.RedactContent(json);

            using var doc = JsonDocument.Parse(redacted);
            Assert.Equal(2, doc.RootElement.GetProperty("tags").GetArrayLength());
            Assert.Equal("***", doc.RootElement.GetProperty("secret").GetString());
        }

        [Fact]
        public void RedactContent_NoSensitiveFields_ShouldKeepJsonIntact()
        {
            var redactor = NewRedactor();
            var json = "{\"id\":1,\"name\":\"az\"}";

            var redacted = redactor.RedactContent(json);

            using var doc = JsonDocument.Parse(redacted);
            Assert.Equal(1, doc.RootElement.GetProperty("id").GetInt32());
            Assert.Equal("az", doc.RootElement.GetProperty("name").GetString());
        }

        [Fact]
        public void Constructor_NullOptions_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new DefaultHttpLogRedactor(null!));
        }

        private static DefaultHttpLogRedactor NewRedactor(IEnumerable<string>? additionalHeaders = null, IEnumerable<string>? additionalFields = null)
        {
            var options = new HttpClientOptions
            {
                AdditionalSensitiveHeaders = (additionalHeaders ?? Array.Empty<string>()).ToList(),
                AdditionalSensitiveFields = (additionalFields ?? Array.Empty<string>()).ToList()
            };
            return new DefaultHttpLogRedactor(options);
        }
    }
}
