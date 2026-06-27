using Azrng.Core.Extension;
using Azrng.Core.Model;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class TreeExtensionsTests
{
    private record Node(int Id, int? ParentId, string Name);

    #region GenerateTree

    [Fact]
    public void GenerateTree_ShouldBuildTree_WithNullRootId()
    {
        var items = new List<Node>
        {
            new(1, null, "Root"),
            new(2, 1, "Child1"),
            new(3, 1, "Child2"),
            new(4, 2, "Grandchild"),
        };

        var result = items.GenerateTree(x => x.Id, x => x.ParentId).ToList();

        result.Should().HaveCount(1);
        result[0].Item.Name.Should().Be("Root");
        result[0].Children.Should().HaveCount(2);

        var children = result[0].Children!.ToList();
        children[0].Item.Name.Should().Be("Child1");
        children[0].Children.Should().HaveCount(1);
        children[0].Children!.Single().Item.Name.Should().Be("Grandchild");

        children[1].Item.Name.Should().Be("Child2");
        children[1].Children.Should().BeEmpty();
    }

    [Fact]
    public void GenerateTree_ShouldBuildTree_WithSpecificRootId()
    {
        var items = new List<Node>
        {
            new(1, 0, "Root"),
            new(2, 1, "Child1"),
            new(3, 1, "Child2"),
        };

        var result = items.GenerateTree(x => x.Id, x => x.ParentId, 1).ToList();

        result.Should().HaveCount(2);
        result[0].Item.Name.Should().Be("Child1");
        result[1].Item.Name.Should().Be("Child2");
    }

    [Fact]
    public void GenerateTree_ShouldReturnEmpty_WhenNoMatchingRoot()
    {
        var items = new List<Node>
        {
            new(1, 10, "Orphan"),
        };

        var result = items.GenerateTree(x => x.Id, x => x.ParentId, 999).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void GenerateTree_ShouldReturnMultipleRoots_WhenMultipleItemsHaveNullParent()
    {
        var items = new List<Node>
        {
            new(1, null, "Root1"),
            new(2, null, "Root2"),
            new(3, 1, "Child1"),
        };

        var result = items.GenerateTree(x => x.Id, x => x.ParentId).ToList();

        result.Should().HaveCount(2);
        result[0].Item.Name.Should().Be("Root1");
        result[0].Children.Should().HaveCount(1);
        result[1].Item.Name.Should().Be("Root2");
        result[1].Children.Should().BeEmpty();
    }

    [Fact]
    public void GenerateTree_ShouldReturnEmpty_WhenCollectionIsEmpty()
    {
        var items = new List<Node>();

        var result = items.GenerateTree(x => x.Id, x => x.ParentId).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void GenerateTree_ShouldThrow_WhenCollectionIsNull()
    {
        IEnumerable<Node>? items = null;

        var action = () => items!.GenerateTree(x => x.Id, x => x.ParentId).ToList();

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("collection");
    }

    [Fact]
    public void GenerateTree_ShouldThrow_WhenIdSelectorIsNull()
    {
        var items = new List<Node> { new(1, null, "Root") };

        var action = () => items.GenerateTree<Node, int?>(null!, x => x.ParentId).ToList();

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("idSelector");
    }

    [Fact]
    public void GenerateTree_ShouldThrow_WhenParentIdSelectorIsNull()
    {
        var items = new List<Node> { new(1, null, "Root") };

        var action = () => items.GenerateTree<Node, int?>(x => x.Id, null!).ToList();

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("parentIdSelector");
    }

    [Fact]
    public void GenerateTree_ShouldHandleFlatList_WithNoChildren()
    {
        var items = new List<Node>
        {
            new(1, null, "A"),
            new(2, null, "B"),
        };

        var result = items.GenerateTree(x => x.Id, x => x.ParentId).ToList();

        result.Should().HaveCount(2);
        result[0].Children.Should().BeEmpty();
        result[1].Children.Should().BeEmpty();
    }

    [Fact]
    public void GenerateTree_ShouldHandleDeepNestedTree()
    {
        var items = new List<Node>
        {
            new(1, null, "L0"),
            new(2, 1, "L1"),
            new(3, 2, "L2"),
            new(4, 3, "L3"),
        };

        var result = items.GenerateTree(x => x.Id, x => x.ParentId).ToList();

        result.Should().HaveCount(1);
        var l1 = result[0].Children!.Single();
        l1.Item.Name.Should().Be("L1");
        var l2 = l1.Children!.Single();
        l2.Item.Name.Should().Be("L2");
        var l3 = l2.Children!.Single();
        l3.Item.Name.Should().Be("L3");
        l3.Children.Should().BeEmpty();
    }

    [Fact]
    public void GenerateTree_ShouldWorkWithStringIds()
    {
        var items = new List<(string Id, string? ParentId, string Name)>
        {
            ("a", null, "Root"),
            ("b", "a", "Child"),
        };

        var result = items.GenerateTree(x => x.Id, x => x.ParentId).ToList();

        result.Should().HaveCount(1);
        result[0].Item.Name.Should().Be("Root");
        result[0].Children!.Single().Item.Name.Should().Be("Child");
    }

    #endregion

    #region Traverse

    [Fact]
    public void Traverse_ShouldReturnAllNodes_InDepthFirstOrder()
    {
        var tree = new List<TreeItem<string>>
        {
            new()
            {
                Item = "Root",
                Children = new List<TreeItem<string>>
                {
                    new()
                    {
                        Item = "Child1",
                        Children = new List<TreeItem<string>>
                        {
                            new() { Item = "Grandchild1" }
                        }
                    },
                    new() { Item = "Child2" }
                }
            }
        };

        var result = tree.Traverse(x => x.Children ?? Enumerable.Empty<TreeItem<string>>()).ToList();

        result.Should().HaveCount(4);
        result.Select(x => x.Item).Should().Contain("Root");
        result.Select(x => x.Item).Should().Contain("Child1");
        result.Select(x => x.Item).Should().Contain("Child2");
        result.Select(x => x.Item).Should().Contain("Grandchild1");
    }

    [Fact]
    public void Traverse_ShouldReturnEmpty_WhenCollectionIsEmpty()
    {
        var tree = new List<TreeItem<string>>();

        var result = tree.Traverse(x => x.Children ?? Enumerable.Empty<TreeItem<string>>()).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Traverse_ShouldThrow_WhenItemsIsNull()
    {
        IEnumerable<TreeItem<string>>? items = null;

        var action = () => items!.Traverse(x => x.Children ?? Enumerable.Empty<TreeItem<string>>()).ToList();

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("items");
    }

    [Fact]
    public void Traverse_ShouldThrow_WhenChildSelectorIsNull()
    {
        var items = new List<TreeItem<string>> { new() { Item = "A" } };

        var action = () => items.Traverse<TreeItem<string>>(null!).ToList();

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("childSelector");
    }

    [Fact]
    public void Traverse_ShouldHandleCycleWithoutInfiniteLoop()
    {
        var a = new CycleNode("A");
        var b = new CycleNode("B");
        a.Children = new[] { b };
        b.Children = new[] { a };

        var items = new List<CycleNode> { a };

        var result = items.Traverse(x => (IEnumerable<CycleNode>)(x.Children ?? Enumerable.Empty<CycleNode>())).ToList();

        result.Should().HaveCount(2);
        result.Select(x => x.Name).Should().Contain("A");
        result.Select(x => x.Name).Should().Contain("B");
    }

    [Fact]
    public void Traverse_ShouldHandleSingleItem_WithNoChildren()
    {
        var tree = new List<TreeItem<int>>
        {
            new() { Item = 42 }
        };

        var result = tree.Traverse(x => x.Children ?? Enumerable.Empty<TreeItem<int>>()).ToList();

        result.Should().HaveCount(1);
        result[0].Item.Should().Be(42);
    }

    [Fact]
    public void Traverse_ShouldHandleMultipleRoots()
    {
        var tree = new List<TreeItem<string>>
        {
            new()
            {
                Item = "A",
                Children = new List<TreeItem<string>>
                {
                    new() { Item = "A1" }
                }
            },
            new()
            {
                Item = "B",
                Children = new List<TreeItem<string>>
                {
                    new() { Item = "B1" }
                }
            }
        };

        var result = tree.Traverse(x => x.Children ?? Enumerable.Empty<TreeItem<string>>()).ToList();

        result.Should().HaveCount(4);
        result.Select(x => x.Item).Should().Contain(new[] { "A", "A1", "B", "B1" });
    }

    [Fact]
    public void Traverse_ShouldSkipNullChildren()
    {
        var tree = new List<TreeItem<string>>
        {
            new()
            {
                Item = "Root",
                Children = new List<TreeItem<string>>
                {
                    null!,
                    new() { Item = "Valid" }
                }
            }
        };

        var result = tree.Traverse(x => x.Children ?? Enumerable.Empty<TreeItem<string>>()).ToList();

        result.Should().HaveCount(2);
        result.Select(x => x.Item).Should().Contain(new[] { "Root", "Valid" });
    }

    #endregion

    private class CycleNode
    {
        public string Name { get; }
        public IEnumerable<CycleNode>? Children { get; set; }

        public CycleNode(string name) => Name = name;
    }
}
