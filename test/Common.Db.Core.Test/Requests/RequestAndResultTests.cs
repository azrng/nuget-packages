using Azrng.Core.Requests;
using Azrng.Core.Results;
using FluentAssertions;
using Xunit;

namespace Common.Db.Core.Test.Requests;

public class RequestAndResultTests
{
    [Fact]
    public void GetPageRequest_Constructors_ShouldPopulateDefaults()
    {
        var defaultRequest = new GetPageRequest();
        var request = new GetPageRequest(pageIndex: 3, pageSize: 25);

        defaultRequest.PageIndex.Should().Be(1);
        defaultRequest.PageSize.Should().Be(10);
        request.PageIndex.Should().Be(3);
        request.PageSize.Should().Be(25);
        request.Keyword.Should().BeEmpty();
    }

    [Fact]
    public void GetPageSortRequest_ShouldUseProvidedSortsOrEmptyArray()
    {
        var defaultRequest = new GetPageSortRequest();
        var sort = new SortContent("Name", SortEnum.Desc);
        var request = new GetPageSortRequest(2, 10, [sort]);

        defaultRequest.SortContents.Should().BeEmpty();
        request.PageIndex.Should().Be(2);
        request.PageSize.Should().Be(10);
        request.SortContents.Should().ContainSingle().Which.Should().BeSameAs(sort);
    }

    [Fact]
    public void GetQueryPageResult_ShouldComputeTotalsFromRequest()
    {
        var request = new GetPageRequest(2, 10);

        var result = new GetQueryPageResult(request, totalCount: 35);

        result.PageIndex.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.Total.Should().Be(35);
        result.TotalPage.Should().Be(4);
    }

    [Fact]
    public void GetQueryPageResultOfT_ShouldStoreRowsAndPageInfo()
    {
        var rows = new List<SampleRow>
        {
            new() { Name = "Alice" }
        };

        var result = new GetQueryPageResult<SampleRow>(rows, pageIndex: 1, pageSize: 20, totalCount: 21);

        result.Rows.Should().BeSameAs(rows);
        result.PageInfo.PageIndex.Should().Be(1);
        result.PageInfo.PageSize.Should().Be(20);
        result.PageInfo.Total.Should().Be(21);
        result.PageInfo.TotalPage.Should().Be(2);
    }

    private sealed class SampleRow
    {
        public string Name { get; set; } = string.Empty;
    }
}
