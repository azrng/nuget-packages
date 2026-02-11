using Azrng.Core.Requests;

namespace Common.Core.Test.Extension.QueryableExtensionTest;

/// <summary>
/// PagedBy分页相关方法的单元测试
/// </summary>
public class PagedByTest
{
    #region PagedBy(GetPageRequest pageContent) Tests

    /// <summary>
    /// 测试PagedBy方法：使用GetPageRequest参数进行分页
    /// </summary>
    [Fact]
    public void PagedBy_GetPageRequest_ReturnsCorrectPage()
    {
        // Arrange
        var data = new List<PaginatedTestEntity>
        {
            new() { Id = 1, Name = "Item1" },
            new() { Id = 2, Name = "Item2" },
            new() { Id = 3, Name = "Item3" },
            new() { Id = 4, Name = "Item4" },
            new() { Id = 5, Name = "Item5" }
        }.AsQueryable();

        var pageRequest = new GetPageRequest(pageIndex: 2, pageSize: 2);

        // Act
        var result = data.PagedBy(pageRequest).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(3, result[0].Id);
        Assert.Equal(4, result[1].Id);
    }

    /// <summary>
    /// 测试PagedBy方法：第一页分页（PageIndex=1）
    /// </summary>
    [Fact]
    public void PagedBy_GetPageRequest_FirstPage_ReturnsFirstItems()
    {
        // Arrange
        var data = new List<PaginatedTestEntity>
        {
            new() { Id = 1, Name = "Item1" },
            new() { Id = 2, Name = "Item2" },
            new() { Id = 3, Name = "Item3" },
            new() { Id = 4, Name = "Item4" },
            new() { Id = 5, Name = "Item5" }
        }.AsQueryable();

        var pageRequest = new GetPageRequest(pageIndex: 1, pageSize: 3);

        // Act
        var result = data.PagedBy(pageRequest).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal(2, result[1].Id);
        Assert.Equal(3, result[2].Id);
    }

    /// <summary>
    /// 测试PagedBy方法：GetPageRequest为null时应抛出ArgumentNullException
    /// </summary>
    [Fact]
    public void PagedBy_NullPageRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var data = new List<PaginatedTestEntity>
        {
            new() { Id = 1, Name = "Item1" }
        }.AsQueryable();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => data.PagedBy((GetPageRequest)null!));
    }

    #endregion

    #region PagedBy(GetPageRequest pageContent, out int totalCount) Tests

    /// <summary>
    /// 测试PagedBy方法：使用GetPageRequest参数并输出总条数
    /// </summary>
    [Fact]
    public void PagedBy_GetPageRequest_OutTotalCount_ReturnsCorrectPageAndTotal()
    {
        // Arrange
        var data = new List<PaginatedTestEntity>
        {
            new() { Id = 1, Name = "Item1" },
            new() { Id = 2, Name = "Item2" },
            new() { Id = 3, Name = "Item3" },
            new() { Id = 4, Name = "Item4" },
            new() { Id = 5, Name = "Item5" }
        }.AsQueryable();

        var pageRequest = new GetPageRequest(pageIndex: 2, pageSize: 2);

        // Act
        var result = data.PagedBy(pageRequest, out int totalCount).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(3, result[0].Id);
        Assert.Equal(4, result[1].Id);
        Assert.Equal(5, totalCount);
    }

    /// <summary>
    /// 测试PagedBy方法：验证总条数计算正确性
    /// </summary>
    [Fact]
    public void PagedBy_GetPageRequest_OutTotalCount_CountsCorrectly()
    {
        // Arrange
        var data = new List<PaginatedTestEntity>();
        for (int i = 1; i <= 100; i++)
        {
            data.Add(new PaginatedTestEntity { Id = i, Name = $"Item{i}" });
        }

        var pageRequest = new GetPageRequest(pageIndex: 5, pageSize: 10);

        // Act
        var result = data.AsQueryable().PagedBy(pageRequest, out int totalCount);

        // Assert
        Assert.Equal(100, totalCount);
    }

    /// <summary>
    /// 测试PagedBy方法：空数据集的总条数应为0
    /// </summary>
    [Fact]
    public void PagedBy_GetPageRequest_OutTotalCount_EmptySource_ReturnsZero()
    {
        // Arrange
        var data = new List<PaginatedTestEntity>().AsQueryable();
        var pageRequest = new GetPageRequest(pageIndex: 1, pageSize: 10);

        // Act
        var result = data.PagedBy(pageRequest, out int totalCount);

        // Assert
        Assert.Empty(result);
        Assert.Equal(0, totalCount);
    }

    #endregion

    #region PagedBy(int pageIndex, int pageSize) Tests

    /// <summary>
    /// 测试PagedBy方法：使用int参数进行分页
    /// </summary>
    [Fact]
    public void PagedBy_IntParams_ReturnsCorrectPage()
    {
        // Arrange
        var data = new List<PaginatedTestEntity>
        {
            new() { Id = 1, Name = "Item1" },
            new() { Id = 2, Name = "Item2" },
            new() { Id = 3, Name = "Item3" },
            new() { Id = 4, Name = "Item4" },
            new() { Id = 5, Name = "Item5" },
            new() { Id = 6, Name = "Item6" },
            new() { Id = 7, Name = "Item7" }
        }.AsQueryable();

        // Act
        var result = data.PagedBy(pageIndex: 2, pageSize: 3).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(4, result[0].Id);
        Assert.Equal(5, result[1].Id);
        Assert.Equal(6, result[2].Id);
    }

    /// <summary>
    /// 测试PagedBy方法：使用默认参数值
    /// </summary>
    [Fact]
    public void PagedBy_IntParams_DefaultValues_ReturnsFirstTenItems()
    {
        // Arrange
        var data = new List<PaginatedTestEntity>();
        for (int i = 1; i <= 20; i++)
        {
            data.Add(new PaginatedTestEntity { Id = i, Name = $"Item{i}" });
        }

        // Act
        var result = data.AsQueryable().PagedBy().ToList();

        // Assert
        Assert.Equal(10, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal(10, result[10 - 1].Id);
    }

    /// <summary>
    /// 测试PagedBy方法：请求超出范围的页应返回空结果
    /// </summary>
    [Fact]
    public void PagedBy_IntParams_PageOutOfRange_ReturnsEmpty()
    {
        // Arrange
        var data = new List<PaginatedTestEntity>
        {
            new() { Id = 1, Name = "Item1" },
            new() { Id = 2, Name = "Item2" }
        }.AsQueryable();

        // Act
        var result = data.PagedBy(pageIndex: 10, pageSize: 5);

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// 测试PagedBy方法：负数页索引应抛出异常
    /// </summary>
    [Fact]
    public void PagedBy_IntParams_NegativePageIndex_ThrowsException()
    {
        // Arrange
        var data = new List<PaginatedTestEntity>
        {
            new() { Id = 1, Name = "Item1" }
        }.AsQueryable();

        // Act & Assert
        // Skip方法对于负数会抛出异常
        Assert.ThrowsAny<Exception>(() => data.PagedBy(pageIndex: -1, pageSize: 10).ToList());
    }

    #endregion

    #region PagedBy(int pageIndex, int pageSize, out int totalCount) Tests

    /// <summary>
    /// 测试PagedBy方法：使用int参数并输出总条数
    /// </summary>
    [Fact]
    public void PagedBy_IntParams_OutTotalCount_ReturnsCorrectPageAndTotal()
    {
        // Arrange
        var data = new List<PaginatedTestEntity>();
        for (int i = 1; i <= 15; i++)
        {
            data.Add(new PaginatedTestEntity { Id = i, Name = $"Item{i}" });
        }

        // Act
        var result = data.AsQueryable().PagedBy(pageIndex: 2, pageSize: 5, out int totalCount).ToList();

        // Assert
        Assert.Equal(5, result.Count);
        Assert.Equal(6, result[0].Id);
        Assert.Equal(10, result[4].Id);
        Assert.Equal(15, totalCount);
    }

    /// <summary>
    /// 测试PagedBy方法：空数据集分页时总条数应为0
    /// </summary>
    [Fact]
    public void PagedBy_IntParams_OutTotalCount_EmptySource_ReturnsZeroTotal()
    {
        // Arrange
        var data = new List<PaginatedTestEntity>().AsQueryable();

        // Act
        var result = data.PagedBy(pageIndex: 1, pageSize: 10, out int totalCount);

        // Assert
        Assert.Empty(result);
        Assert.Equal(0, totalCount);
    }

    /// <summary>
    /// 测试PagedBy方法：单页数据分页验证
    /// </summary>
    [Fact]
    public void PagedBy_IntParams_OutTotalCount_SinglePage_ReturnsAllAndCorrectTotal()
    {
        // Arrange
        var data = new List<PaginatedTestEntity>
        {
            new() { Id = 1, Name = "Item1" },
            new() { Id = 2, Name = "Item2" },
            new() { Id = 3, Name = "Item3" }
        }.AsQueryable();

        // Act
        var result = data.PagedBy(pageIndex: 1, pageSize: 10, out int totalCount).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(3, totalCount);
        Assert.Equal(1, result[0].Id);
        Assert.Equal(2, result[1].Id);
        Assert.Equal(3, result[2].Id);
    }

    #endregion
}

#region Test Helper Classes

/// <summary>
/// 分页测试实体类
/// </summary>
internal class PaginatedTestEntity
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
