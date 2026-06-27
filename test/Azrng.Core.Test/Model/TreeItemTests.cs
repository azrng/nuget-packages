using Azrng.Core.Model;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Model;

public class TreeItemTests
{
    [Fact]
    public void Item_SetAndGet_ShouldWork()
    {
        var item = new TreeItem<int>();
        item.Item = 42;
        item.Item.Should().Be(42);
    }

    [Fact]
    public void Children_SetAndGet_ShouldWork()
    {
        var item = new TreeItem<string>();
        var children = new List<TreeItem<string>>
        {
            new() { Item = "a" },
            new() { Item = "b" }
        };
        item.Children = children;
        item.Children.Should().HaveCount(2);
    }

    [Fact]
    public void Children_DefaultIsNull()
    {
        var item = new TreeItem<int>();
        item.Children.Should().BeNull();
    }

    [Fact]
    public void Item_WithGuid_ShouldWork()
    {
        var id = Guid.NewGuid();
        var item = new TreeItem<Guid>();
        item.Item = id;
        item.Item.Should().Be(id);
    }

    [Fact]
    public void Children_SetToNull_ShouldWork()
    {
        var item = new TreeItem<int>();
        item.Children = new List<TreeItem<int>> { new() { Item = 1 } };
        item.Children = null;
        item.Children.Should().BeNull();
    }

    [Fact]
    public void Item_WithNestedTreeItem_ShouldWork()
    {
        var item = new TreeItem<TreeItem<int>>();
        var child = new TreeItem<int> { Item = 10 };
        item.Item = child;
        item.Item.Item.Should().Be(10);
    }
}
