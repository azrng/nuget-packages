using System.Collections;
using System.Data;
using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class IListExtensionsTests
{
    private class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? Age { get; set; }
    }

    #region ToDataTable(IList, bool)

    [Fact]
    public void ToDataTable_IList_ShouldReturnEmptyTable_WhenListIsEmpty()
    {
        var list = new ArrayList();

        var result = list.ToDataTable();

        result.Rows.Count.Should().Be(0);
        result.Columns.Count.Should().Be(0);
    }

    [Fact]
    public void ToDataTable_IList_ShouldCreateColumnsAndRows_WhenHasColumnsIsTrue()
    {
        var list = new ArrayList
        {
            new TestItem { Id = 1, Name = "Alice", Age = 30 },
            new TestItem { Id = 2, Name = "Bob", Age = null }
        };

        var result = list.ToDataTable(hasColumns: true);

        result.Columns.Count.Should().Be(3);
        result.Columns["Id"]!.DataType.Should().Be(typeof(int));
        result.Columns["Name"]!.DataType.Should().Be(typeof(string));
        result.Columns["Age"]!.DataType.Should().Be(typeof(int));
        result.Rows.Count.Should().Be(2);
        result.Rows[0]["Id"].Should().Be(1);
        result.Rows[0]["Name"].Should().Be("Alice");
        result.Rows[0]["Age"].Should().Be(30);
        result.Rows[1]["Id"].Should().Be(2);
        result.Rows[1]["Name"].Should().Be("Bob");
        result.Rows[1]["Age"].Should().Be(DBNull.Value);
    }

    [Fact]
    public void ToDataTable_IList_ShouldThrow_WhenHasColumnsIsFalseAndListHasItems()
    {
        var list = new ArrayList
        {
            new TestItem { Id = 1, Name = "Alice", Age = 30 }
        };

        var action = () => list.ToDataTable(hasColumns: false);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ToDataTable_IList_ShouldHandleNullableTypes()
    {
        var list = new ArrayList
        {
            new TestItem { Id = 1, Name = "Alice", Age = 25 }
        };

        var result = list.ToDataTable();

        result.Columns["Age"]!.DataType.Should().Be(typeof(int));
    }

    #endregion

    #region ToDataTable<T>(List<T>, bool)

    [Fact]
    public void ToDataTable_Generic_ShouldReturnEmptyTableWithColumns_WhenListIsEmpty()
    {
        var list = new List<TestItem>();

        var result = list.ToDataTable();

        result.Columns.Count.Should().Be(3);
        result.Rows.Count.Should().Be(0);
    }

    [Fact]
    public void ToDataTable_Generic_ShouldCreateColumnsAndRows_WhenHasColumnsIsTrue()
    {
        var list = new List<TestItem>
        {
            new TestItem { Id = 1, Name = "Alice", Age = 30 },
            new TestItem { Id = 2, Name = "Bob", Age = null }
        };

        var result = list.ToDataTable(hasColumns: true);

        result.Columns.Count.Should().Be(3);
        result.Columns["Id"]!.DataType.Should().Be(typeof(int));
        result.Columns["Name"]!.DataType.Should().Be(typeof(string));
        result.Columns["Age"]!.DataType.Should().Be(typeof(int));
        result.Rows.Count.Should().Be(2);
        result.Rows[0]["Id"].Should().Be(1);
        result.Rows[0]["Name"].Should().Be("Alice");
        result.Rows[0]["Age"].Should().Be(30);
        result.Rows[1]["Id"].Should().Be(2);
        result.Rows[1]["Name"].Should().Be("Bob");
    }

    [Fact]
    public void ToDataTable_Generic_ShouldThrow_WhenHasColumnsIsFalseAndListHasItems()
    {
        var list = new List<TestItem>
        {
            new TestItem { Id = 1, Name = "Alice", Age = 30 }
        };

        var action = () => list.ToDataTable(hasColumns: false);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ToDataTable_Generic_ShouldReturnEmptyTable_WhenHasColumnsIsFalseAndListIsEmpty()
    {
        var list = new List<TestItem>();

        var result = list.ToDataTable(hasColumns: false);

        result.Columns.Count.Should().Be(0);
        result.Rows.Count.Should().Be(0);
    }

    [Fact]
    public void ToDataTable_Generic_ShouldHandleNullableTypes()
    {
        var list = new List<TestItem>
        {
            new TestItem { Id = 1, Name = "Alice", Age = 25 }
        };

        var result = list.ToDataTable();

        result.Columns["Age"]!.DataType.Should().Be(typeof(int));
    }

    [Fact]
    public void ToDataTable_Generic_ShouldHandleSingleItem()
    {
        var list = new List<TestItem>
        {
            new TestItem { Id = 42, Name = "Solo", Age = 99 }
        };

        var result = list.ToDataTable();

        result.Rows.Count.Should().Be(1);
        result.Rows[0]["Id"].Should().Be(42);
        result.Rows[0]["Name"].Should().Be("Solo");
        result.Rows[0]["Age"].Should().Be(99);
    }

    #endregion

    #region ToDataTable(IList, DataTable?, bool)

    [Fact]
    public void ToDataTable_IList_WithTable_ShouldAppendToEmptyTable()
    {
        var existingTable = new DataTable();

        var list = new ArrayList
        {
            new TestItem { Id = 1, Name = "Alice", Age = 30 }
        };

        var result = list.ToDataTable(existingTable);

        result.Should().BeSameAs(existingTable);
        result.Columns.Count.Should().Be(3);
        result.Rows.Count.Should().Be(1);
        result.Rows[0]["Id"].Should().Be(1);
        result.Rows[0]["Name"].Should().Be("Alice");
    }

    [Fact]
    public void ToDataTable_IList_WithTable_ShouldAppendToPreConfiguredTable_WhenHasColumnsIsFalse()
    {
        var existingTable = new DataTable();
        existingTable.Columns.Add("Id", typeof(int));
        existingTable.Columns.Add("Name", typeof(string));
        existingTable.Columns.Add("Age", typeof(int));
        existingTable.Rows.Add(0, "PreExisting", 10);

        var list = new ArrayList
        {
            new TestItem { Id = 1, Name = "Alice", Age = 30 }
        };

        var result = list.ToDataTable(existingTable, hasColumns: false);

        result.Should().BeSameAs(existingTable);
        result.Rows.Count.Should().Be(2);
        result.Rows[0]["Name"].Should().Be("PreExisting");
        result.Rows[1]["Id"].Should().Be(1);
        result.Rows[1]["Name"].Should().Be("Alice");
    }

    [Fact]
    public void ToDataTable_IList_WithTable_ShouldCreateNewTable_WhenTableIsNull()
    {
        var list = new ArrayList
        {
            new TestItem { Id = 1, Name = "Alice", Age = 30 }
        };

        var result = list.ToDataTable((DataTable?)null);

        result.Should().NotBeNull();
        result.Columns.Count.Should().Be(3);
        result.Rows.Count.Should().Be(1);
    }

    [Fact]
    public void ToDataTable_IList_WithTable_ShouldReturnEmptyTable_WhenListIsEmpty()
    {
        var existingTable = new DataTable();
        var list = new ArrayList();

        var result = list.ToDataTable(existingTable);

        result.Rows.Count.Should().Be(0);
    }

    [Fact]
    public void ToDataTable_IList_WithTable_ShouldThrow_WhenHasColumnsIsFalseAndNoPreExistingColumns()
    {
        var existingTable = new DataTable();
        var list = new ArrayList
        {
            new TestItem { Id = 1, Name = "Alice", Age = 30 }
        };

        var action = () => list.ToDataTable(existingTable, hasColumns: false);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ToDataTable_IList_WithTable_ShouldHandleNullableTypes()
    {
        var list = new ArrayList
        {
            new TestItem { Id = 1, Name = "Alice", Age = 25 }
        };

        var result = list.ToDataTable((DataTable?)null);

        result.Columns["Age"]!.DataType.Should().Be(typeof(int));
    }

    #endregion

    #region ToDataTable<T>(List<T>, DataTable, bool)

    [Fact]
    public void ToDataTable_Generic_WithTable_ShouldAppendToEmptyTable()
    {
        var existingTable = new DataTable();

        var list = new List<TestItem>
        {
            new TestItem { Id = 1, Name = "Alice", Age = 30 }
        };

        var result = list.ToDataTable(existingTable);

        result.Should().BeSameAs(existingTable);
        result.Columns.Count.Should().Be(3);
        result.Rows.Count.Should().Be(1);
        result.Rows[0]["Id"].Should().Be(1);
        result.Rows[0]["Name"].Should().Be("Alice");
    }

    [Fact]
    public void ToDataTable_Generic_WithTable_ShouldAppendToPreConfiguredTable_WhenHasColumnsIsFalse()
    {
        var existingTable = new DataTable();
        existingTable.Columns.Add("Id", typeof(int));
        existingTable.Columns.Add("Name", typeof(string));
        existingTable.Columns.Add("Age", typeof(int));
        existingTable.Rows.Add(0, "PreExisting", 10);

        var list = new List<TestItem>
        {
            new TestItem { Id = 1, Name = "Alice", Age = 30 }
        };

        var result = list.ToDataTable(existingTable, hasColumns: false);

        result.Should().BeSameAs(existingTable);
        result.Rows.Count.Should().Be(2);
        result.Rows[0]["Name"].Should().Be("PreExisting");
        result.Rows[1]["Id"].Should().Be(1);
        result.Rows[1]["Name"].Should().Be("Alice");
    }

    [Fact]
    public void ToDataTable_Generic_WithTable_ShouldCreateNewTable_WhenTableIsNull()
    {
        var list = new List<TestItem>
        {
            new TestItem { Id = 1, Name = "Alice", Age = 30 }
        };

        var result = list.ToDataTable((DataTable?)null!);

        result.Should().NotBeNull();
        result.Columns.Count.Should().Be(3);
        result.Rows.Count.Should().Be(1);
    }

    [Fact]
    public void ToDataTable_Generic_WithTable_ShouldReturnTable_WhenListIsEmpty()
    {
        var existingTable = new DataTable();
        var list = new List<TestItem>();

        var result = list.ToDataTable(existingTable);

        result.Rows.Count.Should().Be(0);
    }

    [Fact]
    public void ToDataTable_Generic_WithTable_ShouldThrow_WhenHasColumnsIsFalseAndNoPreExistingColumns()
    {
        var existingTable = new DataTable();
        var list = new List<TestItem>
        {
            new TestItem { Id = 1, Name = "Alice", Age = 30 }
        };

        var action = () => list.ToDataTable(existingTable, hasColumns: false);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ToDataTable_Generic_WithTable_ShouldHandleNullableTypes()
    {
        var list = new List<TestItem>
        {
            new TestItem { Id = 1, Name = "Alice", Age = 25 }
        };

        var result = list.ToDataTable(new DataTable());

        result.Columns["Age"]!.DataType.Should().Be(typeof(int));
    }

    [Fact]
    public void ToDataTable_Generic_WithTable_ShouldAppendMultipleItems()
    {
        var existingTable = new DataTable();

        var list = new List<TestItem>
        {
            new TestItem { Id = 1, Name = "Alice", Age = 30 },
            new TestItem { Id = 2, Name = "Bob", Age = 25 },
            new TestItem { Id = 3, Name = "Charlie", Age = 35 }
        };

        var result = list.ToDataTable(existingTable);

        result.Rows.Count.Should().Be(3);
        result.Rows[2]["Name"].Should().Be("Charlie");
    }

    #endregion
}
