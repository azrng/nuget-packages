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

    #region EqualWhere Tests

    /// <summary>
    /// 测试EqualWhere方法：根据字段名和值进行相等筛选
    /// </summary>
    [Fact]
    public void EqualWhere_ValidFieldAndValue_FiltersCorrectly()
    {
        // Arrange
        var data = new List<TestEntity>
        {
            new() { Id = 1, Name = "Alice" },
            new() { Id = 2, Name = "Bob" },
            new() { Id = 3, Name = "Alice" }
        }.AsQueryable();

        // Act
        var result = data.EqualWhere("Name", "Alice").ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, x => Assert.Equal("Alice", x.Name));
    }

    /// <summary>
    /// 测试EqualWhere方法：筛选不存在的值应返回空结果
    /// </summary>
    [Fact]
    public void EqualWhere_NoMatch_ReturnsEmpty()
    {
        // Arrange
        var data = new List<TestEntity>
        {
            new() { Id = 1, Name = "Alice" },
            new() { Id = 2, Name = "Bob" }
        }.AsQueryable();

        // Act
        var result = data.EqualWhere("Name", "Charlie");

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region LessWhere Tests

    /// <summary>
    /// 测试LessWhere方法：根据字段名进行小于筛选
    /// </summary>
    [Fact]
    public void LessWhere_ValidFieldAndValue_FiltersCorrectly()
    {
        // Arrange
        var data = new List<TestEntity>
        {
            new() { Id = 1, Name = "Alice" },
            new() { Id = 2, Name = "Bob" },
            new() { Id = 3, Name = "Charlie" },
            new() { Id = 4, Name = "David" }
        }.AsQueryable();

        // Act
        var result = data.LessWhere("Id", 3).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal(2, result[1].Id);
    }

    /// <summary>
    /// 测试LessWhere方法：所有值都大于比较值时返回空结果
    /// </summary>
    [Fact]
    public void LessWhere_AllValuesGreater_ReturnsEmpty()
    {
        // Arrange
        var data = new List<TestEntity>
        {
            new() { Id = 5, Name = "Alice" },
            new() { Id = 10, Name = "Bob" }
        }.AsQueryable();

        // Act
        var result = data.LessWhere("Id", 5);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GreaterWhere Tests

    /// <summary>
    /// 测试GreaterWhere方法：根据字段名进行大于筛选
    /// </summary>
    [Fact]
    public void GreaterWhere_ValidFieldAndValue_FiltersCorrectly()
    {
        // Arrange
        var data = new List<TestEntity>
        {
            new() { Id = 1, Name = "Alice" },
            new() { Id = 2, Name = "Bob" },
            new() { Id = 3, Name = "Charlie" },
            new() { Id = 4, Name = "David" }
        }.AsQueryable();

        // Act
        var result = data.GreaterWhere("Id", 2).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(3, result[0].Id);
        Assert.Equal(4, result[1].Id);
    }

    /// <summary>
    /// 测试GreaterWhere方法：所有值都小于比较值时返回空结果
    /// </summary>
    [Fact]
    public void GreaterWhere_AllValuesLess_ReturnsEmpty()
    {
        // Arrange
        var data = new List<TestEntity>
        {
            new() { Id = 1, Name = "Alice" },
            new() { Id = 3, Name = "Bob" }
        }.AsQueryable();

        // Act
        var result = data.GreaterWhere("Id", 5);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region WhereAny Tests

    /// <summary>
    /// 测试WhereAny方法：使用多个谓词进行OR条件查询
    /// </summary>
    [Fact]
    public void WhereAny_MultiplePredicates_AppliesOrCondition()
    {
        // Arrange
        var data = new List<TestEntity>
        {
            new() { Id = 1, Name = "Alice" },
            new() { Id = 2, Name = "Bob" },
            new() { Id = 3, Name = "Charlie" },
            new() { Id = 4, Name = "David" }
        }.AsQueryable();

        // Act
        var result = data.WhereAny(
            x => x.Name == "Alice",
            x => x.Name == "Charlie"
        ).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Name == "Alice");
        Assert.Contains(result, x => x.Name == "Charlie");
    }

    /// <summary>
    /// 测试WhereAny方法：单个谓词时应正常工作
    /// </summary>
    [Fact]
    public void WhereAny_SinglePredicate_WorksCorrectly()
    {
        // Arrange
        var data = new List<TestEntity>
        {
            new() { Id = 1, Name = "Alice" },
            new() { Id = 2, Name = "Bob" }
        }.AsQueryable();

        // Act
        var result = data.WhereAny(x => x.Name == "Alice").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
    }

    /// <summary>
    /// 测试WhereAny方法：空谓词数组应返回所有记录都不匹配的结果
    /// </summary>
    [Fact]
    public void WhereAny_EmptyPredicates_ReturnsNoMatches()
    {
        // Arrange
        var data = new List<TestEntity>
        {
            new() { Id = 1, Name = "Alice" },
            new() { Id = 2, Name = "Bob" }
        }.AsQueryable();

        // Act
        var result = data.WhereAny();

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// 测试WhereAny方法：所有谓词都不匹配时应返回空结果
    /// </summary>
    [Fact]
    public void WhereAny_NoMatches_ReturnsEmpty()
    {
        // Arrange
        var data = new List<TestEntity>
        {
            new() { Id = 1, Name = "Alice" },
            new() { Id = 2, Name = "Bob" }
        }.AsQueryable();

        // Act
        var result = data.WhereAny(
            x => x.Name == "Charlie",
            x => x.Name == "David"
        );

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// 测试WhereAny方法：查询集为null时应抛出ArgumentNullException
    /// </summary>
    [Fact]
    public void WhereAny_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        IQueryable<TestEntity>? source = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => source.WhereAny(x => x.Id == 1));
    }

    /// <summary>
    /// 测试WhereAny方法：谓词数组为null时应抛出ArgumentNullException
    /// </summary>
    [Fact]
    public void WhereAny_NullPredicates_ThrowsArgumentNullException()
    {
        // Arrange
        var data = new List<TestEntity>
        {
            new() { Id = 1, Name = "Alice" }
        }.AsQueryable();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => data.WhereAny(null!));
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
