namespace Common.Core.Test.Extension.QueryableExtensionTest;

/// <summary>
/// Queryable扩展方法Where相关功能的单元测试
/// </summary>
public class WhereTest
{
    #region QueryableWhereIf Tests

    /// <summary>
    /// 测试QueryableWhereIf方法：当条件为true时，应正确应用Where过滤条件
    /// </summary>
    [Fact]
    public void QueryableWhereIf_ConditionTrue_AppliesWhere()
    {
        // Arrange
        var data = new List<TestEntity> { new() { Id = 1, Name = "Alice" }, new() { Id = 2, Name = "Bob" } }.AsQueryable();

        // Act
        var result = data.QueryableWhereIf(true, x => x.Id == 1);

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
    }

    /// <summary>
    /// 测试QueryableWhereIf方法：当条件为false时，应返回原始查询集，不应用Where过滤
    /// </summary>
    [Fact]
    public void QueryableWhereIf_ConditionFalse_ReturnsOriginal()
    {
        // Arrange
        var data = new List<TestEntity> { new() { Id = 1, Name = "Alice" }, new() { Id = 2, Name = "Bob" } }.AsQueryable();

        // Act
        var result = data.QueryableWhereIf(false, x => x.Id == 1);

        // Assert
        Assert.Equal(2, result.Count());
    }

    /// <summary>
    /// 测试QueryableWhereIf方法：当源查询集为null时，应抛出ArgumentNullException异常
    /// </summary>
    [Fact]
    public void QueryableWhereIf_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        IQueryable<TestEntity>? source = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => source.QueryableWhereIf(true, x => x.Id == 1));
    }

    #endregion

    #region WhereIfTrue Tests

    /// <summary>
    /// 测试WhereIfTrue方法：当条件为true时，应正确应用Where过滤条件
    /// </summary>
    [Fact]
    public void WhereIfTrue_ConditionTrue_AppliesWhere()
    {
        // Arrange
        var data = new List<TestEntity> { new() { Id = 1, Name = "Alice" }, new() { Id = 2, Name = "Bob" } }.AsQueryable();

        // Act
        var result = data.WhereIfTrue(true, x => x.Name == "Alice");

        // Assert
        Assert.Single(result);
        Assert.Equal("Alice", result.First().Name);
    }

    /// <summary>
    /// 测试WhereIfTrue方法：当条件为false时，应返回原始查询集，不应用Where过滤
    /// </summary>
    [Fact]
    public void WhereIfTrue_ConditionFalse_ReturnsOriginal()
    {
        // Arrange
        var data = new List<TestEntity> { new() { Id = 1, Name = "Alice" }, new() { Id = 2, Name = "Bob" } }.AsQueryable();

        // Act
        var result = data.WhereIfTrue(false, x => x.Name == "Alice");

        // Assert
        Assert.Equal(2, result.Count());
    }

    /// <summary>
    /// 测试WhereIfTrue方法：当源查询集为null时，应抛出ArgumentNullException异常
    /// </summary>
    [Fact]
    public void WhereIfTrue_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        IQueryable<TestEntity>? source = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => source.WhereIfTrue(true, x => x.Id == 1));
    }

    #endregion

    #region WhereIfNotNullOrWhiteSpace Tests

    /// <summary>
    /// 测试WhereIfNotNullOrWhiteSpace方法：当字符串值有效（非空白）时，应正确应用Where过滤条件
    /// </summary>
    [Fact]
    public void WhereIfNotNullOrWhiteSpace_ValidString_AppliesWhere()
    {
        // Arrange
        var data = new List<TestEntity> { new() { Id = 1, Name = "Alice" }, new() { Id = 2, Name = "Bob" } }.AsQueryable();

        // Act
        var result = data.WhereIfNotNullOrWhiteSpace("Alice", x => x.Name == "Alice");

        // Assert
        Assert.Single(result);
        Assert.Equal("Alice", result.First().Name);
    }

    /// <summary>
    /// 测试WhereIfNotNullOrWhiteSpace方法：当字符串值为null时，应返回原始查询集，不应用Where过滤
    /// </summary>
    [Fact]
    public void WhereIfNotNullOrWhiteSpace_NullString_ReturnsOriginal()
    {
        // Arrange
        var data = new List<TestEntity> { new() { Id = 1, Name = "Alice" }, new() { Id = 2, Name = "Bob" } }.AsQueryable();

        // Act
        var result = data.WhereIfNotNullOrWhiteSpace(null, x => x.Name == "Alice");

        // Assert
        Assert.Equal(2, result.Count());
    }

    /// <summary>
    /// 测试WhereIfNotNullOrWhiteSpace方法：当字符串值为空字符串时，应返回原始查询集，不应用Where过滤
    /// </summary>
    [Fact]
    public void WhereIfNotNullOrWhiteSpace_EmptyString_ReturnsOriginal()
    {
        // Arrange
        var data = new List<TestEntity> { new() { Id = 1, Name = "Alice" }, new() { Id = 2, Name = "Bob" } }.AsQueryable();

        // Act
        var result = data.WhereIfNotNullOrWhiteSpace(string.Empty, x => x.Name == "Alice");

        // Assert
        Assert.Equal(2, result.Count());
    }

    /// <summary>
    /// 测试WhereIfNotNullOrWhiteSpace方法：当字符串值仅包含空白字符时，应返回原始查询集，不应用Where过滤
    /// </summary>
    [Fact]
    public void WhereIfNotNullOrWhiteSpace_WhiteSpaceString_ReturnsOriginal()
    {
        // Arrange
        var data = new List<TestEntity> { new() { Id = 1, Name = "Alice" }, new() { Id = 2, Name = "Bob" } }.AsQueryable();

        // Act
        var result = data.WhereIfNotNullOrWhiteSpace("   ", x => x.Name == "Alice");

        // Assert
        Assert.Equal(2, result.Count());
    }

    /// <summary>
    /// 测试WhereIfNotNullOrWhiteSpace方法：当源查询集为null时，应抛出ArgumentNullException异常
    /// </summary>
    [Fact]
    public void WhereIfNotNullOrWhiteSpace_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        IQueryable<TestEntity>? source = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => source.WhereIfNotNullOrWhiteSpace("test", x => x.Id == 1));
    }

    #endregion

    #region WhereIfNotNull Tests

    /// <summary>
    /// 测试WhereIfNotNull方法：当int值有效（非null）时，应正确应用Where过滤条件
    /// </summary>
    [Fact]
    public void WhereIfNotNull_ValidInt_AppliesWhere()
    {
        // Arrange
        var data = new List<TestEntity> { new() { Id = 1, Name = "Alice" }, new() { Id = 2, Name = "Bob" } }.AsQueryable();

        // Act
        var result = data.WhereIfNotNull(1, x => x.Id == 1);

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
    }

    /// <summary>
    /// 测试WhereIfNotNull方法：当int值为null时，应返回原始查询集，不应用Where过滤
    /// </summary>
    [Fact]
    public void WhereIfNotNull_NullInt_ReturnsOriginal()
    {
        // Arrange
        var data = new List<TestEntity> { new() { Id = 1, Name = "Alice" }, new() { Id = 2, Name = "Bob" } }.AsQueryable();

        // Act
        int? value = null;
        var result = data.WhereIfNotNull(value, x => x.Id == 1);

        // Assert
        Assert.Equal(2, result.Count());
    }

    /// <summary>
    /// 测试WhereIfNotNull方法：当string值有效（非null）时，应正确应用Where过滤条件
    /// </summary>
    [Fact]
    public void WhereIfNotNull_ValidString_AppliesWhere()
    {
        // Arrange
        var data = new List<TestEntity> { new() { Id = 1, Name = "Alice" }, new() { Id = 2, Name = "Bob" } }.AsQueryable();

        // Act
        var result = data.WhereIfNotNull("Alice", x => x.Name == "Alice");

        // Assert
        Assert.Single(result);
        Assert.Equal("Alice", result.First().Name);
    }

    /// <summary>
    /// 测试WhereIfNotNull方法：当string值为null时，应返回原始查询集，不应用Where过滤
    /// </summary>
    [Fact]
    public void WhereIfNotNull_NullString_ReturnsOriginal()
    {
        // Arrange
        var data = new List<TestEntity> { new() { Id = 1, Name = "Alice" }, new() { Id = 2, Name = "Bob" } }.AsQueryable();

        // Act
        string? value = null;
        var result = data.WhereIfNotNull(value, x => x.Name == "Alice");

        // Assert
        Assert.Equal(2, result.Count());
    }

    /// <summary>
    /// 测试WhereIfNotNull方法：当源查询集为null时，应抛出ArgumentNullException异常
    /// </summary>
    [Fact]
    public void WhereIfNotNull_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        IQueryable<TestEntity>? source = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => source.WhereIfNotNull(1, x => x.Id == 1));
    }

    #endregion
}

#region Test Helper Classes

/// <summary>
/// 测试用实体类，用于Queryable扩展方法单元测试
/// </summary>
internal class TestEntity
{
    /// <summary>
    /// 实体ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 实体名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

#endregion
