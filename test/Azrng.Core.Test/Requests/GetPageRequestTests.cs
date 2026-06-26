using Azrng.Core.Requests;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Requests;

public class GetPageRequestTests
{
    [Fact]
    public void ParameterlessConstructor_DefaultValues()
    {
        var request = new GetPageRequest();

        request.PageIndex.Should().Be(1);
        request.PageSize.Should().Be(10);
        request.Keyword.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithParameters_SetsValues()
    {
        var request = new GetPageRequest(5, 20, "test");

        request.PageIndex.Should().Be(5);
        request.PageSize.Should().Be(20);
        request.Keyword.Should().Be("test");
    }

    [Fact]
    public void Constructor_WithNullKeyword_SetsEmptyString()
    {
        var request = new GetPageRequest(1, 10, null);

        request.Keyword.Should().Be(string.Empty);
    }

    [Fact]
    public void Constructor_WithoutKeyword_SetsEmptyString()
    {
        var request = new GetPageRequest(1, 10);

        request.Keyword.Should().Be(string.Empty);
    }

    [Fact]
    public void PageIndex_CanBeSet()
    {
        var request = new GetPageRequest();
        request.PageIndex = 3;

        request.PageIndex.Should().Be(3);
    }

    [Fact]
    public void PageSize_CanBeSet()
    {
        var request = new GetPageRequest();
        request.PageSize = 50;

        request.PageSize.Should().Be(50);
    }

    [Fact]
    public void Keyword_CanBeSet()
    {
        var request = new GetPageRequest();
        request.Keyword = "search";

        request.Keyword.Should().Be("search");
    }

    [Fact]
    public void Keyword_CanBeNull()
    {
        var request = new GetPageRequest();
        request.Keyword = "value";
        request.Keyword = null;

        request.Keyword.Should().BeNull();
    }
}
