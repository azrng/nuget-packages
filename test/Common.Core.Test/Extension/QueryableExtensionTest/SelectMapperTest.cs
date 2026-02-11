namespace Common.Core.Test.Extension.QueryableExtensionTest;

/// <summary>
/// SelectMapper查询映射方法的单元测试
/// </summary>
public class SelectMapperTest
{
    /// <summary>
    /// 测试SelectMapper方法：成功将源类型映射到目标类型（属性名称匹配）
    /// </summary>
    [Fact]
    public void SelectMapper_PropertyNamesMatch_MapsCorrectly()
    {
        // Arrange
        var data = new List<SourceEntity>
        {
            new() { Id = 1, Name = "Alice", Age = 25 },
            new() { Id = 2, Name = "Bob", Age = 30 }
        }.AsQueryable();

        // Act
        var result = data.SelectMapper<SourceEntity,DestDto>();

        // Assert
        Assert.Equal(2, result.Count());
        var first = result.First();
        Assert.Equal(1, first.Id);
        Assert.Equal("Alice", first.Name);
    }

    /// <summary>
    /// 测试SelectMapper方法：目标类型包含源类型没有的属性时，该属性使用默认值
    /// </summary>
    [Fact]
    public void SelectMapper_DestHasExtraProperty_UsesDefaultValue()
    {
        // Arrange
        var data = new List<SourceEntity>
        {
            new() { Id = 1, Name = "Alice", Age = 25 }
        }.AsQueryable();

        // Act
        var result = data.SelectMapper<SourceEntity,DestDto>();

        // Assert
        var first = result.First();
        Assert.Equal(1, first.Id);
        Assert.Equal("Alice", first.Name);
        Assert.Equal(0, first.ExtraField); // int 默认值
    }

    /// <summary>
    /// 测试SelectMapper方法：空数据源应返回空结果
    /// </summary>
    [Fact]
    public void SelectMapper_EmptySource_ReturnsEmptyResult()
    {
        // Arrange
        var data = new List<SourceEntity>().AsQueryable();

        // Act
        var result = data.SelectMapper<SourceEntity,DestDto>();

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// 测试SelectMapper方法：多个数据项正确映射
    /// </summary>
    [Fact]
    public void SelectMapper_MultipleItems_MapsAllCorrectly()
    {
        // Arrange
        var data = new List<SourceEntity>
        {
            new() { Id = 1, Name = "Alice", Age = 25 },
            new() { Id = 2, Name = "Bob", Age = 30 },
            new() { Id = 3, Name = "Charlie", Age = 35 }
        }.AsQueryable();

        // Act
        var result = data.SelectMapper<SourceEntity,DestDto>().ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal(2, result[1].Id);
        Assert.Equal("Bob", result[1].Name);
        Assert.Equal(3, result[2].Id);
        Assert.Equal("Charlie", result[2].Name);
    }
}

#region Test Helper Classes

/// <summary>
/// 源实体类，用于测试SelectMapper
/// </summary>
internal class SourceEntity
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

/// <summary>
/// 目标DTO类，用于测试SelectMapper
/// </summary>
internal class DestDto
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
    /// 额外字段（源实体中不存在）
    /// </summary>
    public int ExtraField { get; set; }
}

#endregion
