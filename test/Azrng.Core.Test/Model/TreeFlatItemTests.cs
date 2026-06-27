using Azrng.Core.Model;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Model;

public class TreeFlatItemTests
{
    [Fact]
    public void DefaultConstructor_ShouldInitializeChildrenToEmptyList()
    {
        var item = new TreeFlatItem<int, string>();

        item.Children.Should().BeEmpty();
    }

    [Fact]
    public void Id_SetAndGet_ShouldWork()
    {
        var item = new TreeFlatItem<int, string>();
        item.Id = 42;
        item.Id.Should().Be(42);
    }

    [Fact]
    public void ParentId_SetAndGet_ShouldWork()
    {
        var item = new TreeFlatItem<int, string>();
        item.ParentId = 10;
        item.ParentId.Should().Be(10);
    }

    [Fact]
    public void Children_SetAndGet_ShouldWork()
    {
        var item = new TreeFlatItem<int, string>();
        var children = new List<string> { "a", "b" };
        item.Children = children;
        item.Children.Should().BeEquivalentTo(new[] { "a", "b" });
    }

    [Fact]
    public void Id_WithGuid_ShouldWork()
    {
        var id = Guid.NewGuid();
        var item = new TreeFlatItem<Guid, int>();
        item.Id = id;
        item.Id.Should().Be(id);
    }

    [Fact]
    public void ParentId_WithGuid_ShouldWork()
    {
        var parentId = Guid.NewGuid();
        var item = new TreeFlatItem<Guid, int>();
        item.ParentId = parentId;
        item.ParentId.Should().Be(parentId);
    }

    [Fact]
    public void Children_WithComplexType_ShouldWork()
    {
        var item = new TreeFlatItem<int, TreeFlatItem<int, string>>();
        var child = new TreeFlatItem<int, string> { Id = 1, ParentId = 0 };
        item.Children.Add(child);
        item.Children.Should().HaveCount(1);
        item.Children[0].Id.Should().Be(1);
    }

    [Fact]
    public void Children_ModifyList_ShouldReflectChanges()
    {
        var item = new TreeFlatItem<int, string>();
        item.Children.Add("new");
        item.Children.Should().ContainSingle().Which.Should().Be("new");
    }
}
