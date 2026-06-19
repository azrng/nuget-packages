using Azrng.NMaxCompute.Accounts;
using Azrng.NMaxCompute.Rest;
using Xunit;

namespace Azrng.NMaxCompute.Test;

public class CanonicalStringBuilderTest
{
    [Fact]
    public void Build_MinimalGet_ProducesFiveLines()
    {
        var request = new OdpsRequest
        {
            Method = "GET",
            Path = "/projects/my_proj/instances"
        };

        var canonical = CanonicalStringBuilder.Build(request);

        var lines = canonical.Split('\n');
        Assert.Equal("GET", lines[0]);
        Assert.Equal(string.Empty, lines[1]);   // content-type
        Assert.Equal(string.Empty, lines[2]);   // content-md5
        Assert.NotNull(lines[3]);               // date (GMT)
        Assert.Equal("/projects/my_proj/instances", lines[4]);
    }

    [Fact]
    public void Build_QuerySortedByKey()
    {
        var request = new OdpsRequest
        {
            Method = "GET",
            Path = "/projects/p/tables"
        };
        request.WithQuery("zone", "alpha")
               .WithQuery("aaa", "bbb");

        var canonical = CanonicalStringBuilder.Build(request);
        var lastLine = canonical.Split('\n')[^1];

        Assert.Equal("/projects/p/tables?aaa=bbb&zone=alpha", lastLine);
    }

    [Fact]
    public void Build_QueryEmptyValue_OmitsEqualsSign()
    {
        var request = new OdpsRequest
        {
            Method = "GET",
            Path = "/projects/p/tables"
        };
        request.WithQuery("marker");

        var canonical = CanonicalStringBuilder.Build(request);
        var lastLine = canonical.Split('\n')[^1];

        Assert.Equal("/projects/p/tables?marker", lastLine);
    }

    [Fact]
    public void Build_XOdpsHeadersEmittedAsKeyColonValue()
    {
        var request = new OdpsRequest
        {
            Method = "GET",
            Path = "/projects/p/instances"
        };
        request.WithHeader("x-odps-foo", "bar");

        var canonical = CanonicalStringBuilder.Build(request);
        var lines = canonical.Split('\n');

        Assert.Contains("x-odps-foo:bar", lines);
    }

    [Fact]
    public void Build_MissingDate_GeneratesGmtAndWritesBack()
    {
        var request = new OdpsRequest
        {
            Method = "GET",
            Path = "/projects/p"
        };

        Assert.False(request.Headers.ContainsKey("Date"));

        CanonicalStringBuilder.Build(request);

        Assert.True(request.Headers.ContainsKey("Date"));
        Assert.Matches(@"^[A-Z][a-z]{2}, \d{2} ", request.Headers["Date"]);
    }
}
