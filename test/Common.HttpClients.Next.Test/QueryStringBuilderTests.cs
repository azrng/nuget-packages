using Common.HttpClients.Next.Test.Helpers;
using Common.HttpClients.Utils;
using System.Globalization;

namespace Common.HttpClients.Next.Test
{
    /// <summary>
    /// QueryStringBuilder 行为测试
    /// </summary>
    public class QueryStringBuilderTests
    {
        [Fact]
        public void AppendQuery_NullParameters_ShouldReturnOriginalUrl()
        {
            Assert.Equal("https://unit.test/api", QueryStringBuilder.AppendQuery("https://unit.test/api", null));
        }

        [Fact]
        public void AppendQuery_AnonymousObject_ShouldBuildQueryString()
        {
            var url = QueryStringBuilder.AppendQuery("https://unit.test/api", new { page = 1, pageSize = 20 });

            Assert.Contains("page=1", url);
            Assert.Contains("pageSize=20", url);
            Assert.Contains("?", url);
        }

        [Fact]
        public void AppendQuery_AnonymousObject_ShouldUrlEncodeSpecialCharacters()
        {
            var url = QueryStringBuilder.AppendQuery("https://unit.test/api", new { keyword = "az rng" });

            Assert.Contains("keyword=az+rng", url);
        }

        [Fact]
        public void AppendQuery_StringArray_ShouldExpandMultipleParams()
        {
            var url = QueryStringBuilder.AppendQuery("https://unit.test/api", new { ids = new[] { 1, 2, 3 } });

            Assert.Contains("ids=1", url);
            Assert.Contains("ids=2", url);
            Assert.Contains("ids=3", url);
        }

        [Fact]
        public void AppendQuery_DictionaryNonNullableString_ShouldBuildQueryString()
        {
            // 回归：修复 QueryStringBuilder 漏掉 IDictionary<string,string>（非 nullable）的 bug
            Dictionary<string, string> dict = new()
            {
                ["page"] = "1",
                ["pageSize"] = "20"
            };

            var url = QueryStringBuilder.AppendQuery("https://unit.test/api", dict);

            Assert.Contains("page=1", url);
            Assert.Contains("pageSize=20", url);
        }

        [Fact]
        public void AppendQuery_DictionaryNullableString_ShouldBuildQueryString()
        {
            Dictionary<string, string?> dict = new()
            {
                ["page"] = "1",
                ["keyword"] = null
            };

            var url = QueryStringBuilder.AppendQuery("https://unit.test/api", dict);

            Assert.Contains("page=1", url);
            Assert.DoesNotContain("keyword=", url);
        }

        [Fact]
        public void AppendQuery_NameValueCollection_ShouldBuildQueryString()
        {
            var nvc = new NameValueCollection
            {
                ["page"] = "1",
                ["pageSize"] = "20"
            };

            var url = QueryStringBuilder.AppendQuery("https://unit.test/api", nvc);

            Assert.Contains("page=1", url);
            Assert.Contains("pageSize=20", url);
        }

        [Fact]
        public void AppendQuery_UrlAlreadyHasQuery_ShouldUseAmpersandSeparator()
        {
            var url = QueryStringBuilder.AppendQuery("https://unit.test/api?fixed=true", new { page = 1 });

            Assert.Contains("?fixed=true&page=1", url);
        }

        [Fact]
        public void AppendQuery_DateTime_ShouldUseConfiguredFormat()
        {
            QueryStringBuilder.DateTimeFormat = "yyyy-MM-dd";
            try
            {
                var dt = new DateTime(2026, 6, 14, 10, 30, 0);
                var url = QueryStringBuilder.AppendQuery("https://unit.test/api", new { at = dt });

                Assert.Contains("at=2026-06-14", url);
                Assert.DoesNotContain("10", url.Substring(url.IndexOf("at=", StringComparison.Ordinal)));
            }
            finally
            {
                QueryStringBuilder.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            }
        }

        [Fact]
        public void AppendQuery_DateTime_ShouldIgnoreCurrentCulture()
        {
            // it-IT 的时间分隔符是 "."，区域不应影响生成的 URL（自定义格式中的 ":" 是文化相关占位符）
            var originalCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("it-IT");
            try
            {
                var dt = new DateTime(2026, 6, 14, 10, 30, 5);
                var url = QueryStringBuilder.AppendQuery("https://unit.test/api", new { at = dt });

                Assert.Contains("at=2026-06-14+10%3A30%3A05", url);
                Assert.DoesNotContain("10.30.05", url);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public void AppendQuery_Bool_ShouldUseLowercaseString()
        {
            var url = QueryStringBuilder.AppendQuery("https://unit.test/api", new { enabled = true, disabled = false });

            Assert.Contains("enabled=true", url);
            Assert.Contains("disabled=false", url);
        }

        [Fact]
        public void AppendQuery_Enum_ShouldUseEnumName()
        {
            var url = QueryStringBuilder.AppendQuery("https://unit.test/api", new { method = DayOfWeek.Monday });

            Assert.Contains("method=Monday", url);
        }

        [Fact]
        public void AppendQuery_EmptyObject_ShouldReturnOriginalUrl()
        {
            var url = QueryStringBuilder.AppendQuery("https://unit.test/api", new { });

            Assert.Equal("https://unit.test/api", url);
        }

        [Fact]
        public void AppendQuery_NullValues_ShouldBeSkipped()
        {
            var url = QueryStringBuilder.AppendQuery("https://unit.test/api", new { a = (string?)null, b = "ok" });

            Assert.DoesNotContain("a=", url);
            Assert.Contains("b=ok", url);
        }
    }
}
