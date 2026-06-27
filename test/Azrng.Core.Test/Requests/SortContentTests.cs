using Azrng.Core.Requests;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Requests;

public class SortContentTests
{
    [Fact]
    public void Constructor_SetsSortName()
    {
        var content = new SortContent("Name", SortEnum.Asc);

        content.SortName.Should().Be("Name");
    }

    [Fact]
    public void Constructor_SetsSort_Asc()
    {
        var content = new SortContent("Name", SortEnum.Asc);

        content.Sort.Should().Be(SortEnum.Asc);
    }

    [Fact]
    public void Constructor_SetsSort_Desc()
    {
        var content = new SortContent("Name", SortEnum.Desc);

        content.Sort.Should().Be(SortEnum.Desc);
    }

    [Fact]
    public void SortName_CanBeSet()
    {
        var content = new SortContent("Name", SortEnum.Asc);
        content.SortName = "CreatedTime";

        content.SortName.Should().Be("CreatedTime");
    }

    [Fact]
    public void SortName_CanBeSetToNull()
    {
        var content = new SortContent("Name", SortEnum.Asc);
        content.SortName = null!;

        content.SortName.Should().BeNull();
    }

    [Fact]
    public void Sort_CanBeSet()
    {
        var content = new SortContent("Name", SortEnum.Asc);
        content.Sort = SortEnum.Desc;

        content.Sort.Should().Be(SortEnum.Desc);
    }

    [Fact]
    public void SortEnum_Asc_IsZero()
    {
        ((int)SortEnum.Asc).Should().Be(0);
    }

    [Fact]
    public void SortEnum_Desc_IsOne()
    {
        ((int)SortEnum.Desc).Should().Be(1);
    }
}
