using Azrng.Core.Extension;
using System.Collections.Generic;

namespace Common.Core.Test.Extension;

/// <summary>
/// CollectionExtensions集合扩展方法的单元测试
/// </summary>
public class CollectionExtensionsTest
{
    #region AddIfNotContains Tests

    /// <summary>
    /// 测试AddIfNotContains方法：当集合中不存在该项时，应成功添加并返回true
    /// </summary>
    [Fact]
    public void AddIfNotContains_ItemNotExists_AddsAndReturnsTrue()
    {
        // Arrange
        var collection = new List<string> { "item1", "item2" };

        // Act
        var result = collection.AddIfNotContains("item3");

        // Assert
        Assert.True(result);
        Assert.Equal(3, collection.Count);
        Assert.Contains("item3", collection);
    }

    /// <summary>
    /// 测试AddIfNotContains方法：当集合中已存在该项时，不应添加并返回false
    /// </summary>
    [Fact]
    public void AddIfNotContains_ItemExists_DoesNotAddAndReturnsFalse()
    {
        // Arrange
        var collection = new List<string> { "item1", "item2", "item3" };

        // Act
        var result = collection.AddIfNotContains("item2");

        // Assert
        Assert.False(result);
        Assert.Equal(3, collection.Count);
    }

    /// <summary>
    /// 测试AddIfNotContains方法：向空集合添加项
    /// </summary>
    [Fact]
    public void AddIfNotContains_EmptyCollection_AddsAndReturnsTrue()
    {
        // Arrange
        var collection = new List<int>();

        // Act
        var result = collection.AddIfNotContains(42);

        // Assert
        Assert.True(result);
        Assert.Single(collection);
        Assert.Equal(42, collection[0]);
    }

    /// <summary>
    /// 测试AddIfNotContains方法：使用自定义对象类型
    /// </summary>
    [Fact]
    public void AddIfNotContains_CustomObjects_WorksCorrectly()
    {
        // Arrange
        var item1 = new TestItem { Id = 1, Name = "Item1" };
        var item2 = new TestItem { Id = 2, Name = "Item2" };
        var item3 = new TestItem { Id = 1, Name = "Item1" }; // 同样的Id和Name

        var collection = new List<TestItem> { item1, item2 };

        // Act
        var result = collection.AddIfNotContains(item3);

        // Assert
        Assert.False(result); // item1 和 item3 的属性相同，被认为已存在
        Assert.Equal(2, collection.Count);
    }

    /// <summary>
    /// 测试AddIfNotContains方法：集合为null时应抛出ArgumentNullException
    /// </summary>
    [Fact]
    public void AddIfNotContains_NullCollection_ThrowsArgumentNullException()
    {
        // Arrange
        List<string> collection = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => collection.AddIfNotContains("test"));
    }

    /// <summary>
    /// 测试AddIfNotContains方法：连续添加多个不存在的项
    /// </summary>
    [Fact]
    public void AddIfNotContains_MultipleNewItems_AddsAll()
    {
        // Arrange
        var collection = new List<int> { 1, 2, 3 };

        // Act
        var result1 = collection.AddIfNotContains(4);
        var result2 = collection.AddIfNotContains(5);
        var result3 = collection.AddIfNotContains(4); // 重复

        // Assert
        Assert.True(result1);
        Assert.True(result2);
        Assert.False(result3);
        Assert.Equal(5, collection.Count);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, collection);
    }

    /// <summary>
    /// 测试AddIfNotContains方法：添加null值到引用类型集合
    /// </summary>
    [Fact]
    public void AddIfNotContains_NullValue_ToNullableCollection()
    {
        // Arrange
        var collection = new List<string> { "item1", "item2" };

        // Act
        var result = collection.AddIfNotContains((string)null);

        // Assert
        Assert.True(result);
        Assert.Equal(3, collection.Count);
        Assert.Null(collection[2]);
    }

    #endregion
}

#region Test Helper Classes

/// <summary>
/// 测试用项类
/// </summary>
internal class TestItem
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public override bool Equals(object obj)
    {
        if (obj is TestItem other)
        {
            return Id == other.Id && Name == other.Name;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name);
    }
}

#endregion
