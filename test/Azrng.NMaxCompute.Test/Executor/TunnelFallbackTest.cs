using Azrng.NMaxCompute.Provider;
using Xunit;

namespace Azrng.NMaxCompute.Test.Executor;

public class TunnelFallbackTest
{
    [Theory]
    [InlineData("InvalidProjectTable", null)]
    [InlineData("ODPS-0130131:InvalidArgument - bad rowrange", null)]
    [InlineData("NoSuchProject", null)]
    [InlineData("InstanceTypeNotSupported", null)]
    [InlineData("NoDownload", null)]
    [InlineData(null, " ODPS: InvalidProjectTable ")]
    public void ShouldFallback_True_ForKnownMarkers(string? code, string? message)
    {
        Assert.True(DirectOdpsQueryExecutor.ShouldFallbackToResultApi(code, message));
    }

    [Theory]
    [InlineData("AccessDenied", "permission denied")]
    [InlineData("ODPS-0130131", "syntax error near xxx")]
    [InlineData(null, null)]
    [InlineData("", "")]
    public void ShouldFallback_False_ForOtherErrors(string? code, string? message)
    {
        Assert.False(DirectOdpsQueryExecutor.ShouldFallbackToResultApi(code, message));
    }

    [Theory]
    [InlineData("INVALIDPROJECTTABLE")]     // 大小写不敏感
    [InlineData("instancetypenotsupported")]
    public void ShouldFallback_IsCaseInsensitive(string code)
    {
        Assert.True(DirectOdpsQueryExecutor.ShouldFallbackToResultApi(code, null));
    }
}
