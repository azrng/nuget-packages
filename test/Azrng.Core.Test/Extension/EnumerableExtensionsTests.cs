using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class EnumerableExtensionsTests
{
    [Fact]
    public void IsNullOrEmpty_ShouldReturnTrue_WhenSourceIsNull()
    {
        IEnumerable<int>? source = null;

        source.IsNullOrEmpty().Should().BeTrue();
    }

    [Fact]
    public void IsNullOrEmpty_ShouldReturnTrue_WhenSourceIsEmpty()
    {
        var source = new List<int>();

        source.IsNullOrEmpty().Should().BeTrue();
    }

    [Fact]
    public void IsNullOrEmpty_ShouldReturnFalse_WhenSourceHasItems()
    {
        var source = new List<int> { 1, 2, 3 };

        source.IsNullOrEmpty().Should().BeFalse();
    }

    [Fact]
    public void IsNotNullOrEmpty_ShouldReturnFalse_WhenSourceIsNull()
    {
        IEnumerable<int>? source = null;

        source.IsNotNullOrEmpty().Should().BeFalse();
    }

    [Fact]
    public void IsNotNullOrEmpty_ShouldReturnFalse_WhenSourceIsEmpty()
    {
        var source = new List<int>();

        source.IsNotNullOrEmpty().Should().BeFalse();
    }

    [Fact]
    public void IsNotNullOrEmpty_ShouldReturnTrue_WhenSourceHasItems()
    {
        var source = new List<int> { 1, 2, 3 };

        source.IsNotNullOrEmpty().Should().BeTrue();
    }

    [Fact]
    public void WhereIF_ShouldFilter_WhenConditionIsTrue()
    {
        var source = new List<int> { 1, 2, 3, 4, 5 };

        var result = source.WhereIF(true, x => x > 3);

        result.Should().Equal(4, 5);
    }

    [Fact]
    public void WhereIF_ShouldReturnAll_WhenConditionIsFalse()
    {
        var source = new List<int> { 1, 2, 3, 4, 5 };

        var result = source.WhereIF(false, x => x > 3);

        result.Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public void WhereIF_ShouldThrow_WhenSourceIsNull()
    {
        IEnumerable<int>? source = null;

        var action = () => source!.WhereIF(true, x => x > 0);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToPage_ShouldReturnCorrectPage()
    {
        var source = Enumerable.Range(1, 10);

        var result = source.ToPage(2, 3);

        result.Should().Equal(4, 5, 6);
    }

    [Fact]
    public void ToPage_ShouldReturnFirstPage_WhenPageIndexIsOne()
    {
        var source = Enumerable.Range(1, 10);

        var result = source.ToPage(1, 3);

        result.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void ToPage_ShouldClampPageIndex_WhenLessThanOne()
    {
        var source = Enumerable.Range(1, 10);

        var result = source.ToPage(0, 3);

        result.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void ToPage_ShouldClampPageSize_WhenLessThanOne()
    {
        var source = Enumerable.Range(1, 10);

        var result = source.ToPage(1, 0);

        result.Should().Equal(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
    }

    [Fact]
    public void ToPage_ShouldReturnPartialPage_WhenNotEnoughItems()
    {
        var source = Enumerable.Range(1, 5);

        var result = source.ToPage(2, 3);

        result.Should().Equal(4, 5);
    }

    [Fact]
    public void ToPage_ShouldReturnEmpty_WhenPageExceedsTotal()
    {
        var source = Enumerable.Range(1, 5);

        var result = source.ToPage(3, 3);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ToPage_ShouldThrow_WhenSourceIsNull()
    {
        IEnumerable<int>? source = null;

        var action = () => source!.ToPage(1, 10);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToPageList_ShouldReturnListWithCorrectPage()
    {
        var source = Enumerable.Range(1, 10);

        var result = source.ToPageList(2, 3);

        result.Should().Equal(4, 5, 6);
        result.Should().BeOfType<List<int>>();
    }

    [Fact]
    public void ToPageArray_ShouldReturnArrayWithCorrectPage()
    {
        var source = Enumerable.Range(1, 10);

        var result = source.ToPageArray(2, 3);

        result.Should().Equal(4, 5, 6);
        result.Should().BeOfType<int[]>();
    }

    [Fact]
    public void AsciiDictionary_ShouldSortKeysByAsciiOrder()
    {
        var dic = new Dictionary<string, string>
        {
            { "banana", "2" },
            { "apple", "1" },
            { "cherry", "3" }
        };

        var result = dic.AsciiDictionary();

        result.Keys.Should().Equal("apple", "banana", "cherry");
        result["apple"].Should().Be("1");
        result["banana"].Should().Be("2");
        result["cherry"].Should().Be("3");
    }

    [Fact]
    public void AsciiDictionary_ShouldHandleSingleItem()
    {
        var dic = new Dictionary<string, string>
        {
            { "key", "value" }
        };

        var result = dic.AsciiDictionary();

        result.Should().ContainSingle();
        result["key"].Should().Be("value");
    }

    [Fact]
    public void WithIndex_ShouldReturnItemWithCorrectIndex()
    {
        var source = new List<string> { "a", "b", "c" };

        var result = source.WithIndex().ToList();

        result.Should().HaveCount(3);
        result[0].Should().Be(("a", 0));
        result[1].Should().Be(("b", 1));
        result[2].Should().Be(("c", 2));
    }

    [Fact]
    public void WithIndex_ShouldReturnEmpty_WhenSourceIsEmpty()
    {
        var source = new List<int>();

        var result = source.WithIndex();

        result.Should().BeEmpty();
    }
}
