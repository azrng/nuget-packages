namespace Common.Core.Test.Extension.QueryableExtensionTest;

/// <summary>
/// CountBy查询总条数方法的单元测试
/// </summary>
public class CountByTest
{
    /// <summary>
    /// 测试CountBy方法：正确计算查询集的总条数
    /// </summary>
    [Fact]
    public void CountBy_MultipleItems_ReturnsCorrectCount()
    {
        // Arrange
        var data = new List<CountTestEntity>
        {
            new() { Id = 1, Name = "Item1" },
            new() { Id = 2, Name = "Item2" },
            new() { Id = 3, Name = "Item3" },
            new() { Id = 4, Name = "Item4" },
            new() { Id = 5, Name = "Item5" }
        }.AsQueryable();

        // Act
        var result = data.CountBy(out int totalCount);

        // Assert
        Assert.Equal(5, totalCount);
        Assert.Equal(5, result.Count());
    }

    /// <summary>
    /// 测试CountBy方法：空查询集的总条数应为0
    /// </summary>
    [Fact]
    public void CountBy_EmptySource_ReturnsZero()
    {
        // Arrange
        var data = new List<CountTestEntity>().AsQueryable();

        // Act
        var result = data.CountBy(out int totalCount);

        // Assert
        Assert.Equal(0, totalCount);
        Assert.Empty(result);
    }

    /// <summary>
    /// 测试CountBy方法：单个项目的总条数应为1
    /// </summary>
    [Fact]
    public void CountBy_SingleItem_ReturnsOne()
    {
        // Arrange
        var data = new List<CountTestEntity>
        {
            new() { Id = 1, Name = "Item1" }
        }.AsQueryable();

        // Act
        var result = data.CountBy(out int totalCount);

        // Assert
        Assert.Equal(1, totalCount);
        Assert.Single(result);
    }

    /// <summary>
    /// 测试CountBy方法：大量数据时正确计算总条数
    /// </summary>
    [Fact]
    public void CountBy_LargeDataSet_ReturnsCorrectCount()
    {
        // Arrange
        var data = new List<CountTestEntity>();
        for (int i = 1; i <= 1000; i++)
        {
            data.Add(new CountTestEntity { Id = i, Name = $"Item{i}" });
        }

        // Act
        var result = data.AsQueryable().CountBy(out int totalCount);

        // Assert
        Assert.Equal(1000, totalCount);
        Assert.Equal(1000, result.Count());
    }

    /// <summary>
    /// 测试CountBy方法：与Where组合使用时，计算过滤后的条数
    /// </summary>
    [Fact]
    public void CountBy_WithWhere_ReturnsFilteredCount()
    {
        // Arrange
        var data = new List<CountTestEntity>
        {
            new() { Id = 1, Name = "Item1" },
            new() { Id = 2, Name = "Item2" },
            new() { Id = 3, Name = "Item3" },
            new() { Id = 4, Name = "Item4" },
            new() { Id = 5, Name = "Item5" }
        }.AsQueryable();

        // Act
        var result = data.Where(x => x.Id > 2).CountBy(out int totalCount);

        // Assert
        Assert.Equal(3, totalCount);
        Assert.Equal(3, result.Count());
    }

    /// <summary>
    /// 测试CountBy方法：返回的查询集与原始查询集一致
    /// </summary>
    [Fact]
    public void CountBy_ReturnsSameQueryable()
    {
        // Arrange
        var data = new List<CountTestEntity>
        {
            new() { Id = 1, Name = "Item1" },
            new() { Id = 2, Name = "Item2" }
        }.AsQueryable();

        // Act
        var result = data.CountBy(out int totalCount);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, totalCount);
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal(1, resultList[0].Id);
        Assert.Equal(2, resultList[1].Id);
    }
}

#region Test Helper Classes

/// <summary>
/// 计数测试实体类
/// </summary>
internal class CountTestEntity
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

#endregion
