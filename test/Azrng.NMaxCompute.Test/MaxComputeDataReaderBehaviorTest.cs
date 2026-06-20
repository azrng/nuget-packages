using Azrng.NMaxCompute.Models;
using Xunit;

namespace Azrng.NMaxCompute.Test;

/// <summary>
/// MaxComputeDataReader 行为层测试：多行 Read / 列名查找 / GetValues / Close / 越界等
/// （类型映射见 DataReaderTypeMappingTest，GetFieldValue 见 MaxComputeDataReaderTypeTest）。
/// </summary>
public class MaxComputeDataReaderBehaviorTest
{
    private static MaxComputeDataReader ThreeRows()
    {
        var result = new QueryResult
        {
            Columns = new[] { "id", "name" },
            ColumnTypes = new[] { "bigint", "string" },
            Rows = new[]
            {
                new object[] { 1L, "a" },
                new object[] { 2L, "bb" },
                new object[] { 3L, "ccc" }
            },
            RowCount = 3
        };
        return new MaxComputeDataReader(result);
    }

    [Fact]
    public void Read_IteratesAllRows_ThenFalse()
    {
        using var r = ThreeRows();
        Assert.True(r.Read()); Assert.Equal(1L, r.GetInt64(0));
        Assert.True(r.Read()); Assert.Equal(2L, r.GetInt64(0));
        Assert.True(r.Read()); Assert.Equal(3L, r.GetInt64(0));
        Assert.False(r.Read());
    }

    [Fact]
    public void FieldCount_AndGetName()
    {
        using var r = ThreeRows();
        Assert.Equal(2, r.FieldCount);
        Assert.Equal("id", r.GetName(0));
        Assert.Equal("name", r.GetName(1));
    }

    [Fact]
    public void GetOrdinal_CaseInsensitive()
    {
        using var r = ThreeRows();
        Assert.Equal(0, r.GetOrdinal("id"));
        Assert.Equal(1, r.GetOrdinal("NAME"));
        Assert.Throws<ArgumentException>(() => r.GetOrdinal("missing"));
    }

    [Fact]
    public void Indexer_ByName()
    {
        using var r = ThreeRows();
        r.Read();
        Assert.Equal(1L, r["id"]);
        Assert.Equal("a", r["name"]);
    }

    [Fact]
    public void GetValues_FillsBuffer()
    {
        using var r = ThreeRows();
        r.Read();
        var buf = new object[3];
        var n = r.GetValues(buf);
        Assert.Equal(2, n);
        Assert.Equal(1L, buf[0]);
        Assert.Equal("a", buf[1]);
    }

    [Fact]
    public void GetValue_BeforeRead_Throws()
    {
        using var r = ThreeRows();
        Assert.Throws<InvalidOperationException>(() => r.GetInt64(0));
    }

    [Fact]
    public void GetValue_OutOfRange_Throws()
    {
        using var r = ThreeRows();
        r.Read();
        Assert.Throws<ArgumentOutOfRangeException>(() => r.GetValue(5));
        Assert.Throws<ArgumentOutOfRangeException>(() => r.GetValue(-1));
    }

    [Fact]
    public void Close_MarksClosed_AndBlocksAccess()
    {
        using var r = ThreeRows();
        Assert.False(r.IsClosed);
        r.Read();
        r.Close();
        Assert.True(r.IsClosed);
        Assert.False(r.Read());
        Assert.Throws<InvalidOperationException>(() => r.GetValue(0));
    }

    [Fact]
    public void NextResult_AlwaysFalse()
    {
        using var r = ThreeRows();
        Assert.False(r.NextResult());
    }

    [Fact]
    public void IsDBNull_DetectsNull()
    {
        var result = new QueryResult
        {
            Columns = new[] { "a", "b" },
            ColumnTypes = new[] { "bigint", "string" },
            Rows = new[] { new object[] { DBNull.Value, "x" } },
            RowCount = 1
        };
        using var r = new MaxComputeDataReader(result);
        r.Read();
        Assert.True(r.IsDBNull(0));
        Assert.False(r.IsDBNull(1));
    }

    [Fact]
    public void EmptyResultSet_ReadFalseImmediately()
    {
        var result = new QueryResult { Columns = new[] { "a" }, ColumnTypes = new[] { "bigint" } };
        using var r = new MaxComputeDataReader(result);
        Assert.False(r.Read());
        Assert.False(r.HasRows);
    }
}
