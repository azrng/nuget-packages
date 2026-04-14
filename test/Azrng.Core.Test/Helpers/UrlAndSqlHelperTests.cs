using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class UrlAndSqlHelperTests
{
    [Fact]
    public void SortUrlParameters_ShouldReturnOrderedQueryString()
    {
        var result = UrlHelper.SortUrlParameters("https://example.com/path?b=2&a=1");

        result.url.Should().Be("https://example.com/path?a=1&b=2");
        result.@params.Should().Be("a=1&b=2");
    }

    [Fact]
    public void ExtractUrl_ShouldReturnAuthorityPart()
    {
        var input = "visit https://example.com/path?q=1 for details";

        UrlHelper.ExtractUrl(input).Should().Be("https://example.com");
    }

    [Fact]
    public void ToDictFromQueryString_ShouldReturnAllQueryPairs()
    {
        var result = UrlHelper.ToDictFromQueryString(new Uri("https://example.com/path?a=1&b=2"));

        result.Should().Contain(new KeyValuePair<string, string>("a", "1"));
        result.Should().Contain(new KeyValuePair<string, string>("b", "2"));
    }

    [Fact]
    public void AddQueryString_ShouldAppendValuesAndPreserveFragment()
    {
        var result = UrlHelper.AddQueryString(
            "https://example.com/path?x=1#frag",
            new[]
            {
                new KeyValuePair<string, string>("a", "1 2"),
                new KeyValuePair<string, string>("b", "3")
            });

        result.Should().Be("https://example.com/path?x=1&a=1+2&b=3#frag");
    }

    [Fact]
    public void HasSqlInjectionRisk_ShouldDetectClassicInjection()
    {
        SqlHelper.HasSqlInjectionRisk("' OR '1'='1").Should().BeTrue();
        SqlHelper.HasSqlInjectionRisk("normal user input").Should().BeFalse();
    }

    [Fact]
    public void IsSafeInput_ShouldRejectDangerousKeywords()
    {
        SqlHelper.IsSafeInput("hello world").Should().BeTrue();
        SqlHelper.IsSafeInput("DROP TABLE Users").Should().BeFalse();
    }
}
