using System.Data;

namespace Common.Core.Test.Extension;

/// <summary>
/// DataTableExtensions DataTable扩展方法的单元测试
/// </summary>
public class DataTableExtensionsTest
{
    #region ToEntity Tests

    /// <summary>
    /// 测试ToEntity方法：将DataTable的单行转换为实体
    /// </summary>
    [Fact]
    public void ToEntity_ValidTable_ReturnsEntity()
    {
        // Arrange
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Age", typeof(int));
        table.Rows.Add(1, "Alice", 25);

        // Act
        var result = table.ToEntity<PersonEntity>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Alice", result.Name);
        Assert.Equal(25, result.Age);
    }

    /// <summary>
    /// 测试ToEntity方法：处理可空类型的列
    /// </summary>
    [Fact]
    public void ToEntity_WithNullableColumns_ReturnsEntity()
    {
        // Arrange
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Age", typeof(int));
        table.Columns.Add("Salary", typeof(decimal));
        table.Rows.Add(1, "Bob", 30, 5000.50m);

        // Act
        var result = table.ToEntity<EmployeeEntity>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Bob", result.Name);
        Assert.Equal(30, result.Age);
        Assert.Equal(5000.50m, result.Salary);
    }

    /// <summary>
    /// 测试ToEntity方法：列值为DBNull时应跳过该属性
    /// </summary>
    [Fact]
    public void ToEntity_WithDBNull_SkipsProperty()
    {
        // Arrange
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Age", typeof(int));
        var row = table.NewRow();
        row["Id"] = 1;
        row["Name"] = DBNull.Value;
        row["Age"] = 25;
        table.Rows.Add(row);

        // Act
        var result = table.ToEntity<PersonEntity>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(25, result.Age);
        Assert.Equal(string.Empty, result.Name); // 默认值
    }

    /// <summary>
    /// 测试ToEntity方法：DataTable中不包含某列时应跳过
    /// </summary>
    [Fact]
    public void ToEntity_ColumnNotExists_SkipsProperty()
    {
        // Arrange
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Age", typeof(int));
        table.Rows.Add(1, 25);

        // Act
        var result = table.ToEntity<PersonEntity>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(25, result.Age);
        Assert.Equal(string.Empty, result.Name); // 默认值
    }

    /// <summary>
    /// 测试ToEntity方法：多行DataTable覆盖
    /// </summary>
    [Fact]
    public void ToEntity_MultipleRows_ConvertsLasterRow()
    {
        // Arrange
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Rows.Add(1, "Alice");
        table.Rows.Add(2, "Bob");

        // Act
        var result = table.ToEntity<PersonEntity>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
        Assert.Equal("Bob", result.Name);
    }

    /// <summary>
    /// 测试ToEntity方法：空DataTable应返回默认实体
    /// </summary>
    [Fact]
    public void ToEntity_EmptyTable_ReturnsDefaultEntity()
    {
        // Arrange
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));

        // Act
        var result = table.ToEntity<PersonEntity>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
        Assert.Equal(string.Empty, result.Name);
    }

    #endregion

    #region ToEntities Tests

    /// <summary>
    /// 测试ToEntities方法：将DataTable的多行转换为实体列表
    /// </summary>
    [Fact]
    public void ToEntities_ValidTable_ReturnsListOfEntities()
    {
        // Arrange
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Rows.Add(1, "Alice");
        table.Rows.Add(2, "Bob");
        table.Rows.Add(3, "Charlie");

        // Act
        var result = table.ToEntities<PersonEntity>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal(2, result[1].Id);
        Assert.Equal("Bob", result[1].Name);
        Assert.Equal(3, result[2].Id);
        Assert.Equal("Charlie", result[2].Name);
    }

    /// <summary>
    /// 测试ToEntities方法：null DataTable应返回null
    /// </summary>
    [Fact]
    public void ToEntities_NullTable_ReturnsNull()
    {
        // Arrange
        DataTable table = null;

        // Act
        var result = table.ToEntities<PersonEntity>();

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// 测试ToEntities方法：空DataTable应返回空列表
    /// </summary>
    [Fact]
    public void ToEntities_EmptyTable_ReturnsEmptyList()
    {
        // Arrange
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));

        // Act
        var result = table.ToEntities<PersonEntity>();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// 测试ToEntities方法：处理包含DBNull值的行
    /// </summary>
    [Fact]
    public void ToEntities_WithDBNull_HandlesCorrectly()
    {
        // Arrange
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Age", typeof(int));
        var row1 = table.NewRow();
        row1["Id"] = 1;
        row1["Name"] = "Alice";
        row1["Age"] = DBNull.Value;
        table.Rows.Add(row1);
        var row2 = table.NewRow();
        row2["Id"] = 2;
        row2["Name"] = DBNull.Value;
        row2["Age"] = 30;
        table.Rows.Add(row2);

        // Act
        var result = table.ToEntities<PersonEntity>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal(0, result[0].Age); // 默认值
        Assert.Equal(2, result[1].Id);
        Assert.Equal(string.Empty, result[1].Name); // 默认值
        Assert.Equal(30, result[1].Age);
    }

    /// <summary>
    /// 测试ToEntities方法：列不存在时跳过该属性
    /// </summary>
    [Fact]
    public void ToEntities_ColumnNotExists_SkipsProperty()
    {
        // Arrange
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        // 不包含Name列
        table.Rows.Add(1);
        table.Rows.Add(2);

        // Act
        var result = table.ToEntities<PersonEntity>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal(2, result[1].Id);
        Assert.Equal(string.Empty, result[0].Name);
        Assert.Equal(string.Empty, result[1].Name);
    }

    #endregion
}

#region Test Helper Classes

/// <summary>
/// 人员实体类，用于测试
/// </summary>
internal class PersonEntity
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
/// 员工实体类，用于测试可空类型
/// </summary>
internal class EmployeeEntity
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

    /// <summary>
    /// 薪资（可空）
    /// </summary>
    public decimal? Salary { get; set; }
}

#endregion
