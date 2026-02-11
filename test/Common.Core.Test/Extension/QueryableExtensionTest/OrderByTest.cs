using Azrng.Core.Requests;
using Xunit.Abstractions;

namespace Common.Core.Test.Extension.QueryableExtensionTest;

/// <summary>
/// OrderBy排序相关方法的单元测试
/// </summary>
public class OrderByTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public OrderByTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    #region OrderBy<TSource, TKey>(bool isAsc) Tests

    /// <summary>
    /// 测试OrderBy方法：使用bool参数指定升序排序
    /// </summary>
    [Fact]
    public void OrderBy_BoolParam_Ascending_SortsCorrectly()
    {
        // Arrange
        var data = new List<OrderTestEntity>
                   {
                       new() { Id = 3, Name = "Charlie", Age = 35 },
                       new() { Id = 1, Name = "Alice", Age = 25 },
                       new() { Id = 2, Name = "Bob", Age = 30 }
                   }.AsQueryable();

        // Act
        var result = data.OrderBy(x => x.Id, true).ToList();

        // Assert
        Assert.Equal(1, result[0].Id);
        Assert.Equal(2, result[1].Id);
        Assert.Equal(3, result[2].Id);
    }

    /// <summary>
    /// 测试OrderBy方法：使用bool参数指定降序排序
    /// </summary>
    [Fact]
    public void OrderBy_BoolParam_Descending_SortsCorrectly()
    {
        // Arrange
        var data = new List<OrderTestEntity>
                   {
                       new() { Id = 1, Name = "Alice", Age = 25 },
                       new() { Id = 3, Name = "Charlie", Age = 35 },
                       new() { Id = 2, Name = "Bob", Age = 30 }
                   }.AsQueryable();

        // Act
        var result = data.OrderBy(x => x.Id, false).ToList();

        // Assert
        Assert.Equal(3, result[0].Id);
        Assert.Equal(2, result[1].Id);
        Assert.Equal(1, result[2].Id);
    }

    #endregion

    #region OrderBy<TSource, TKey>(SortEnum sortEnum) Tests

    /// <summary>
    /// 测试OrderBy方法：使用SortEnum枚举指定升序排序
    /// </summary>
    [Fact]
    public void OrderBy_SortEnumAsc_SortsCorrectly()
    {
        // Arrange
        var data = new List<OrderTestEntity>
                   {
                       new() { Id = 3, Name = "Charlie", Age = 35 },
                       new() { Id = 1, Name = "Alice", Age = 25 },
                       new() { Id = 2, Name = "Bob", Age = 30 }
                   }.AsQueryable();

        // Act
        var result = data.OrderBy(x => x.Age, SortEnum.Asc).ToList();

        // Assert
        Assert.Equal(25, result[0].Age);
        Assert.Equal(30, result[1].Age);
        Assert.Equal(35, result[2].Age);
    }

    /// <summary>
    /// 测试OrderBy方法：使用SortEnum枚举指定降序排序
    /// </summary>
    [Fact]
    public void OrderBy_SortEnumDesc_SortsCorrectly()
    {
        // Arrange
        var data = new List<OrderTestEntity>
                   {
                       new() { Id = 1, Name = "Alice", Age = 25 },
                       new() { Id = 3, Name = "Charlie", Age = 35 },
                       new() { Id = 2, Name = "Bob", Age = 30 }
                   }.AsQueryable();

        // Act
        var result = data.OrderBy(x => x.Age, SortEnum.Desc).ToList();

        // Assert
        Assert.Equal(35, result[0].Age);
        Assert.Equal(30, result[1].Age);
        Assert.Equal(25, result[2].Age);
    }

    #endregion

    #region OrderBy(params SortContent[] orderContent) Tests

    /// <summary>
    /// 测试OrderBy方法：使用SortContent数组排序（多个排序条件）
    /// </summary>
    [Fact]
    public void OrderBy_SortContentArray_MultipleSortFields_SortsCorrectly()
    {
        // Arrange
        var data = new List<OrderTestEntity>
                   {
                       new() { Id = 1, Name = "Alice", Age = 25 },
                       new() { Id = 4, Name = "Alice", Age = 20 },
                       new() { Id = 2, Name = "Bob", Age = 30 },
                       new() { Id = 3, Name = "Charlie", Age = 35 }
                   }.AsQueryable();

        var sortContents = new[]
                           {
                               new SortContent("Name", SortEnum.Asc),
                               new SortContent("Age", SortEnum.Asc)
                           };

        // Act
        var result = data.OrderBy(sortContents).ToList();

        // Assert
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal(20, result[0].Age); // Alice按年龄升序
        Assert.Equal("Alice", result[1].Name);
        Assert.Equal(25, result[1].Age);
        Assert.Equal("Bob", result[2].Name);
        Assert.Equal("Charlie", result[3].Name);
    }

    /// <summary>
    /// 测试OrderBy方法：空SortContent数组应返回原始查询集
    /// </summary>
    [Fact]
    public void OrderBy_EmptySortContentArray_ReturnsOriginal()
    {
        // Arrange
        var data = new List<OrderTestEntity> { new() { Id = 3, Name = "Charlie", Age = 35 }, new() { Id = 1, Name = "Alice", Age = 25 } }
            .AsQueryable();

        // Act
        var result = data.OrderBy(Array.Empty<SortContent>()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(3, result[0].Id); // 保持原始顺序
    }

    #endregion

    #region OrderBy(SortContent orderContent) Tests

    /// <summary>
    /// 测试OrderBy方法：使用单个SortContent对象进行升序排序
    /// </summary>
    [Fact]
    public void OrderBy_SingleSortContent_Asc_SortsCorrectly()
    {
        // Arrange
        var data = new List<OrderTestEntity>
                   {
                       new() { Id = 3, Name = "Charlie", Age = 35 },
                       new() { Id = 1, Name = "Alice", Age = 25 },
                       new() { Id = 2, Name = "Bob", Age = 30 }
                   }.AsQueryable();

        var sortContent = new SortContent("Id", SortEnum.Asc);

        // Act
        var result = data.OrderBy(sortContent).ToList();

        // Assert
        Assert.Equal(1, result[0].Id);
        Assert.Equal(2, result[1].Id);
        Assert.Equal(3, result[2].Id);
    }

    /// <summary>
    /// 测试OrderBy方法：使用单个SortContent对象进行降序排序
    /// </summary>
    [Fact]
    public void OrderBy_SingleSortContent_Desc_SortsCorrectly()
    {
        // Arrange
        var data = new List<OrderTestEntity>
                   {
                       new() { Id = 1, Name = "Alice", Age = 25 },
                       new() { Id = 3, Name = "Charlie", Age = 35 },
                       new() { Id = 2, Name = "Bob", Age = 30 }
                   }.AsQueryable();

        var sortContent = new SortContent("Age", SortEnum.Desc);

        // Act
        var result = data.OrderBy(sortContent).ToList();

        // Assert
        Assert.Equal(35, result[0].Age);
        Assert.Equal(30, result[1].Age);
        Assert.Equal(25, result[2].Age);
    }

    /// <summary>
    /// 测试OrderBy方法：SortContent为null时应抛出ArgumentNullException
    /// </summary>
    [Fact]
    public void OrderBy_NullSortContent_ThrowsArgumentNullException()
    {
        // Arrange
        var data = new List<OrderTestEntity> { new() { Id = 1, Name = "Alice", Age = 25 } }.AsQueryable();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => data.OrderBy((SortContent)null!));
    }

    /// <summary>
    /// 测试OrderBy方法：SortContent的SortName为空白时应抛出ArgumentNullException
    /// </summary>
    [Fact]
    public void OrderBy_EmptySortName_ThrowsArgumentNullException()
    {
        // Arrange
        var data = new List<OrderTestEntity> { new() { Id = 1, Name = "Alice", Age = 25 } }.AsQueryable();

        var sortContent = new SortContent("", SortEnum.Asc);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => data.OrderBy(sortContent));
    }

    #endregion

    #region OrderBy(string sortField, bool isAsc) Tests

    /// <summary>
    /// 测试OrderBy方法：使用字符串字段名进行升序排序
    /// </summary>
    [Fact]
    public void OrderBy_StringField_Asc_SortsCorrectly()
    {
        // Arrange
        var data = new List<OrderTestEntity>
                   {
                       new() { Id = 3, Name = "Charlie", Age = 35 },
                       new() { Id = 1, Name = "Alice", Age = 25 },
                       new() { Id = 2, Name = "Bob", Age = 30 }
                   }.AsQueryable();

        // Act
        var result = data.OrderBy("Id", true).ToList();

        // Assert
        Assert.Equal(1, result[0].Id);
        Assert.Equal(2, result[1].Id);
        Assert.Equal(3, result[2].Id);
    }

    /// <summary>
    /// 测试OrderBy方法：使用字符串字段名进行降序排序
    /// </summary>
    [Fact]
    public void OrderBy_StringField_Desc_SortsCorrectly()
    {
        // Arrange
        var data = new List<OrderTestEntity>
                   {
                       new() { Id = 1, Name = "Alice", Age = 25 },
                       new() { Id = 3, Name = "Charlie", Age = 35 },
                       new() { Id = 2, Name = "Bob", Age = 30 }
                   }.AsQueryable();

        // Act
        var result = data.OrderBy("Age", false).ToList();

        // Assert
        Assert.Equal(35, result[0].Age);
        Assert.Equal(30, result[1].Age);
        Assert.Equal(25, result[2].Age);
    }

    /// <summary>
    /// 测试OrderBy方法：使用无效的属性名时应抛出异常
    /// </summary>
    [Fact]
    public void OrderBy_InvalidProperty_ThrowsException()
    {
        // Arrange
        var data = new List<OrderTestEntity> { new() { Id = 1, Name = "Alice", Age = 25 } }.AsQueryable();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => data.OrderBy("InvalidField", true));
    }

    #endregion

    #region OrderBy(params FiledOrderParam[] orderParams) Tests

    /// <summary>
    /// 测试OrderBy方法：使用FiledOrderParam数组进行多字段排序
    /// </summary>
    [Fact]
    public void OrderBy_FiledOrderParamArray_MultipleFields_SortsCorrectly()
    {
        // Arrange
        var data = new List<OrderTestEntity>
                   {
                       new() { Id = 1, Name = "Alice", Age = 25 },
                       new() { Id = 4, Name = "Alice", Age = 20 },
                       new() { Id = 2, Name = "Bob", Age = 30 },
                       new() { Id = 3, Name = "Charlie", Age = 35 }
                   }.AsQueryable();

        var orderParams = new[]
                          {
                              new FiledOrderParam { PropertyName = "Name", IsAsc = true },
                              new FiledOrderParam { PropertyName = "Age", IsAsc = true }
                          };

        // Act
        var result = data.OrderBy(orderParams).ToList();

        // Assert
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal(20, result[0].Age);
        Assert.Equal("Alice", result[1].Name);
        Assert.Equal(25, result[1].Age);
        Assert.Equal("Bob", result[2].Name);
        Assert.Equal("Charlie", result[3].Name);
    }

    /// <summary>
    /// 测试OrderBy方法：空FiledOrderParam数组应返回原始查询集
    /// </summary>
    [Fact]
    public void OrderBy_EmptyFiledOrderParamArray_ReturnsOriginal()
    {
        // Arrange
        var data = new List<OrderTestEntity> { new() { Id = 3, Name = "Charlie", Age = 35 }, new() { Id = 1, Name = "Alice", Age = 25 } }
            .AsQueryable();

        // Act
        var result = data.OrderBy(Array.Empty<FiledOrderParam>()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    /// <summary>
    /// 测试OrderBy方法：FiledOrderParam包含无效属性名时应跳过该排序条件
    /// </summary>
    [Fact]
    public void OrderBy_FiledOrderParam_InvalidProperty_SkipsInvalidField()
    {
        // Arrange
        var data = new List<OrderTestEntity>
                   {
                       new() { Id = 3, Name = "Charlie", Age = 35 },
                       new() { Id = 1, Name = "Alice", Age = 25 },
                       new() { Id = 2, Name = "Bob", Age = 30 }
                   }.AsQueryable();

        var orderParams = new[]
                          {
                              new FiledOrderParam { PropertyName = "InvalidField", IsAsc = true },
                              new FiledOrderParam { PropertyName = "Id", IsAsc = true }
                          };

        // Act
        var result = data.OrderBy(orderParams).ToList();
        _testOutputHelper.WriteLine($"排序后总数：{result.Count}");

        // Assert
        Assert.Equal(1, result[0].Id);
        Assert.Equal(2, result[1].Id);
        Assert.Equal(3, result[2].Id);
    }

    #endregion
}

#region Test Helper Classes

/// <summary>
/// 排序测试实体类
/// </summary>
internal class OrderTestEntity
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 年龄
    /// </summary>
    public int Age { get; set; }
}

#endregion