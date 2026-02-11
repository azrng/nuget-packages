namespace Common.Core.Test.Extension;

/// <summary>
/// DictionaryExtension Dictionary扩展方法的单元测试
/// </summary>
public class DictionaryExtensionTest
{
    #region GetOrDefault Tests

    /// <summary>
    /// 测试GetOrDefault方法：字典存在键时返回对应值
    /// </summary>
    [Fact]
    public void GetOrDefault_KeyExists_ReturnsValue()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>
        {
            { "a", 1 },
            { "b", 2 }
        };

        // Act
        var result = dictionary.GetOrDefault("a", 0);

        // Assert
        Assert.Equal(1, result);
    }

    /// <summary>
    /// 测试GetOrDefault方法：字典不存在键时返回默认值
    /// </summary>
    [Fact]
    public void GetOrDefault_KeyNotExists_ReturnsDefaultValue()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>
        {
            { "a", 1 }
        };

        // Act
        var result = dictionary.GetOrDefault("b", 0);

        // Assert
        Assert.Equal(0, result);
    }

    /// <summary>
    /// 测试GetOrDefault方法：字典为null时返回默认值
    /// </summary>
    [Fact]
    public void GetOrDefault_NullDictionary_ReturnsDefaultValue()
    {
        // Arrange
        Dictionary<string, int>? dictionary = null;

        // Act
        var result = dictionary.GetOrDefault("a", 99);

        // Assert
        Assert.Equal(99, result);
    }

    #endregion

    #region GetColumnValueByName Tests (string)

    /// <summary>
    /// 测试GetColumnValueByName方法：获取存在的字符串列值
    /// </summary>
    [Fact]
    public void GetColumnValueByName_KeyExists_ReturnsValue()
    {
        // Arrange
        var keyValues = new Dictionary<string, string>
        {
            { "Name", "Alice" },
            { "Age", "25" }
        };

        // Act
        var result = keyValues.GetColumnValueByName("Name");

        // Assert
        Assert.Equal("Alice", result);
    }

    /// <summary>
    /// 测试GetColumnValueByName方法：列不存在时返回空字符串
    /// </summary>
    [Fact]
    public void GetColumnValueByName_KeyNotExists_ReturnsNull()
    {
        // Arrange
        var keyValues = new Dictionary<string, string>
        {
            { "Name", "Alice" }
        };

        // Act
        var result = keyValues.GetColumnValueByName("Address");

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// 测试GetColumnValueByName方法：keyValues为null时返回null
    /// </summary>
    [Fact]
    public void GetColumnValueByName_NullKeyValues_ReturnsNull()
    {
        // Arrange
        Dictionary<string, string>? keyValues = null;

        // Act
        var result = keyValues.GetColumnValueByName("Name");

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// 测试GetColumnValueByName方法：key为null时返回Null
    /// </summary>
    [Fact]
    public void GetColumnValueByName_NullKey_ReturnsNull()
    {
        // Arrange
        var keyValues = new Dictionary<string, string>
        {
            { "Name", "Alice" }
        };

        // Act
        var result = keyValues.GetColumnValueByName(null);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetColumnValueByName Tests (object)

    /// <summary>
    /// 测试GetColumnValueByName方法：获取存在的DateTime列值
    /// </summary>
    [Fact]
    public void GetColumnValueByName_Object_KeyExists_ReturnsDateTimeString()
    {
        // Arrange
        var keyValues = new Dictionary<string, object?>
        {
            { "CreatedTime", new DateTime(2025, 12, 31) }
        };

        // Act
        var result = keyValues.GetColumnValueByName("CreatedTime");

        // Assert
        Assert.Equal("2025-12-31 00:00:00", result);
    }

    /// <summary>
    /// 测试GetColumnValueByName方法：获取存在的decimal列值
    /// </summary>
    [Fact]
    public void GetColumnValueByName_Object_KeyExists_ReturnsDecimalString()
    {
        // Arrange
        var keyValues = new Dictionary<string, object?>
        {
            { "Price", 19.99m }
        };

        // Act
        var result = keyValues.GetColumnValueByName("Price");

        // Assert
        Assert.Equal("19.99", result);
    }

    /// <summary>
    /// 测试GetColumnValueByName方法：列不存在时返回空字符串
    /// </summary>
    [Fact]
    public void GetColumnValueByName_Object_KeyNotExists_ReturnsEmptyString()
    {
        // Arrange
        var keyValues = new Dictionary<string, object?>
        {
            { "Name", "Test" }
        };

        // Act
        var result = keyValues.GetColumnValueByName("Address");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region GetColumnValueByName<T> Tests

    /// <summary>
    /// 测试泛型GetColumnValueByName方法：获取int类型值
    /// </summary>
    [Fact]
    public void GetColumnValueByName_Generic_KeyExists_ReturnsIntValue()
    {
        // Arrange
        var keyValues = new Dictionary<string, object?>
        {
            { "Count", 100 }
        };

        // Act
        var result = keyValues.GetColumnValueByName<int>("Count");

        // Assert
        Assert.Equal(100, result);
    }

    /// <summary>
    /// 测试泛型GetColumnValueByName方法：获取decimal类型值
    /// </summary>
    [Fact]
    public void GetColumnValueByName_Generic_KeyExists_ReturnsDecimalValue()
    {
        // Arrange
        var keyValues = new Dictionary<string, object?>
        {
            { "Price", 99.99m }
        };

        // Act
        var result = keyValues.GetColumnValueByName<decimal>("Price");

        // Assert
        Assert.Equal(99.99m, result);
    }

    /// <summary>
    /// 测试泛型GetColumnValueByName方法：列不存在时返回默认值
    /// </summary>
    [Fact]
    public void GetColumnValueByName_Generic_KeyNotExists_ReturnsDefault()
    {
        // Arrange
        var keyValues = new Dictionary<string, object?>
        {
            { "Name", "Test" }
        };

        // Act
        var result = keyValues.GetColumnValueByName<int>("Address");

        // Assert
        Assert.Equal(0, result);
    }

    /// <summary>
    /// 测试泛型GetColumnValueByName方法：keyValues为null时返回默认值
    /// </summary>
    [Fact]
    public void GetColumnValueByName_Generic_NullKeyValues_ReturnsDefault()
    {
        // Arrange
        Dictionary<string, object?>? keyValues = null;

        // Act
        var result = keyValues.GetColumnValueByName<int>("Name");

        // Assert
        Assert.Equal(0, result);
    }

    #endregion

    #region AddOrUpdate Tests

    /// <summary>
    /// 测试AddOrUpdate方法：键存在时更新值
    /// </summary>
    [Fact]
    public void AddOrUpdate_KeyExists_UpdatesValue()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>
        {
            { "a", 1 }
        };

        // Act
        dictionary.AddOrUpdate("a", 2);

        // Assert
        Assert.Equal(2, dictionary["a"]);
    }

    /// <summary>
    /// 测试AddOrUpdate方法：键不存在时添加值
    /// </summary>
    [Fact]
    public void AddOrUpdate_KeyNotExists_AddsValue()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>
        {
            { "a", 1 }
        };

        // Act
        dictionary.AddOrUpdate("b", 2);

        // Assert
        Assert.True(dictionary.ContainsKey("b"));
        Assert.Equal(2, dictionary["b"]);
    }

    /// <summary>
    /// 测试AddOrUpdate方法：字典为null时抛出异常
    /// </summary>
    [Fact]
    public void AddOrUpdate_NullDictionary_ThrowsException()
    {
        // Arrange
        Dictionary<string, int>? dictionary = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => dictionary.AddOrUpdate("a", 1));
    }

    #endregion

    #region CreateOrInsert Tests

    /// <summary>
    /// 测试CreateOrInsert方法：键存在时向列表添加值
    /// </summary>
    [Fact]
    public void CreateOrInsert_KeyExists_AppendsToList()
    {
        // Arrange
        var dict = new Dictionary<string, List<int>>
        {
            { "a", new List<int> { 1, 2 } }
        };

        // Act
        dict.CreateOrInsert("a", 3);

        // Assert
        Assert.Equal(3, dict["a"].Count);
        Assert.Contains(3, dict["a"]);
    }

    /// <summary>
    /// 测试CreateOrInsert方法：键不存在时创建新列表
    /// </summary>
    [Fact]
    public void CreateOrInsert_KeyNotExists_CreatesNewList()
    {
        // Arrange
        var dict = new Dictionary<string, List<int>>();

        // Act
        var dictionary = dict;
        dictionary.CreateOrInsert("b", 1);

        // Assert
        Assert.Single(dictionary["b"]);
        Assert.Equal(1, dictionary["b"][0]);
    }

    /// <summary>
    /// 测试CreateOrInsert方法：字典为null时抛出异常
    /// </summary>
    [Fact]
    public void CreateOrInsert_NullDict_ThrowsException()
    {
        // Arrange
        Dictionary<string, List<int>>? dict = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => dict.CreateOrInsert("a", 1));
    }

    #endregion

    #region AddOrAppend Tests (int)

    /// <summary>
    /// 测试AddOrAppend方法：键存在时更新int值
    /// </summary>
    [Fact]
    public void AddOrAppend_Int_KeyExists_UpdatesValue()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>
        {
            { "total", 10 }
        };

        // Act
        dictionary.AddOrAppend("total", 5);

        // Assert
        Assert.Equal(15, dictionary["total"]);
    }

    /// <summary>
    /// 测试AddOrAppend方法：键不存在时设置int值
    /// </summary>
    [Fact]
    public void AddOrAppend_Int_KeyNotExists_SetsValue()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>();

        // Act
        dictionary.AddOrAppend("count", 100);

        // Assert
        Assert.Equal(100, dictionary["count"]);
    }

    /// <summary>
    /// 测试AddOrAppend方法：字典为null时抛出异常
    /// </summary>
    [Fact]
    public void AddOrAppend_Int_NullDictionary_ThrowsException()
    {
        // Arrange
        Dictionary<string, int>? dictionary = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => dictionary.AddOrAppend("a", 1));
    }

    #endregion

    #region AddOrAppend Tests (long)

    /// <summary>
    /// 测试AddOrAppend方法：键存在时更新long值
    /// </summary>
    [Fact]
    public void AddOrAppend_Long_KeyExists_UpdatesValue()
    {
        // Arrange
        var dictionary = new Dictionary<string, long>
        {
            { "timestamp", 1000L }
        };

        // Act
        dictionary.AddOrAppend("timestamp", 500L);

        // Assert
        Assert.Equal(1500L, dictionary["timestamp"]);
    }

    /// <summary>
    /// 测试AddOrAppend方法：键不存在时设置long值
    /// </summary>
    [Fact]
    public void AddOrAppend_Long_KeyNotExists_SetsValue()
    {
        // Arrange
        var Dictionary = new Dictionary<string, long>();

        // Act
        Dictionary.AddOrAppend("id", 999L);

        // Assert
        Assert.Equal(999L, Dictionary["id"]);
    }

    /// <summary>
    /// 测试AddOrAppend方法：字典为null时抛出异常
    /// </summary>
    [Fact]
    public void AddOrAppend_Long_NullDictionary_ThrowsException()
    {
        // Arrange
        Dictionary<string, long>? dictionary = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => dictionary.AddOrAppend("a", 1L));
    }

    #endregion
}
