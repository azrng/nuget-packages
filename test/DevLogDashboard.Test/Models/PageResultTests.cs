using Azrng.DevLogDashboard.Models;

namespace Azrng.DevLogDashboard.Test.Models;

public class PageResultTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var result = new PageResult<string>();

        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
        result.PageIndex.Should().Be(0);
        result.PageSize.Should().Be(0);
    }

    [Fact]
    public void TotalPages_ShouldCalculateCorrectly()
    {
        var result = new PageResult<string> { Total = 100, PageSize = 50 };
        result.TotalPages.Should().Be(2);

        result.Total = 101;
        result.TotalPages.Should().Be(3);

        result.Total = 50;
        result.TotalPages.Should().Be(1);

        result.Total = 1;
        result.PageSize = 50;
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public void TotalPages_ZeroTotal_ShouldBeZero()
    {
        var result = new PageResult<string> { Total = 0, PageSize = 50 };
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public void HasPrevious_ShouldBeTrueWhenPageIndexGreaterThan1()
    {
        var result = new PageResult<string> { PageIndex = 1 };
        result.HasPrevious.Should().BeFalse();

        result.PageIndex = 2;
        result.HasPrevious.Should().BeTrue();

        result.PageIndex = 0;
        result.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public void HasNext_ShouldBeTrueWhenPageIndexLessThanTotalPages()
    {
        var result = new PageResult<string> { Total = 100, PageSize = 50, PageIndex = 1 };
        result.HasNext.Should().BeTrue();

        result.PageIndex = 2;
        result.HasNext.Should().BeFalse();

        result.PageIndex = 3;
        result.HasNext.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldReturnCorrectResult()
    {
        var items = new List<string> { "a", "b", "c" };
        var result = PageResult<string>.Create(items, 10, 1, 3);

        result.Items.Should().BeEquivalentTo(items);
        result.Total.Should().Be(10);
        result.PageIndex.Should().Be(1);
        result.PageSize.Should().Be(3);
        result.TotalPages.Should().Be(4);
        result.HasPrevious.Should().BeFalse();
        result.HasNext.Should().BeTrue();
    }

    [Fact]
    public void Create_WithIntType_ShouldWork()
    {
        var items = new List<int> { 1, 2, 3, 4, 5 };
        var result = PageResult<int>.Create(items, 20, 2, 5);

        result.Items.Should().HaveCount(5);
        result.Total.Should().Be(20);
        result.TotalPages.Should().Be(4);
        result.HasPrevious.Should().BeTrue();
        result.HasNext.Should().BeTrue();
    }

    [Fact]
    public void Create_LastPage_HasNextShouldBeFalse()
    {
        var items = new List<string> { "x" };
        var result = PageResult<string>.Create(items, 3, 3, 1);

        result.HasPrevious.Should().BeTrue();
        result.HasNext.Should().BeFalse();
    }
}
