using System.Collections;
using Azrng.NMaxCompute.Models;
using Xunit;

namespace Azrng.NMaxCompute.Test;

/// <summary>
/// 验证 MaxComputeDataReader 对各类型（含新增 interval / vector）的
/// GetFieldType / GetFieldValue&lt;T&gt; / GetValue 行为。用合成 QueryResult，不依赖集群。
/// </summary>
public class MaxComputeDataReaderTypeTest
{
    private static MaxComputeDataReader SingleRowReader(string name, string type, object value)
    {
        var result = new QueryResult
        {
            Columns = new[] { name },
            ColumnTypes = new[] { type },
            Rows = new[] { new object[] { value } },
            RowCount = 1
        };
        var reader = new MaxComputeDataReader(result);
        Assert.True(reader.Read());
        return reader;
    }

    // ---------- 标量类型 ----------

    [Fact]
    public void BigInt_GetInt64_AndFieldType()
    {
        using var r = SingleRowReader("c", "bigint", 5L);
        Assert.Equal(typeof(long), r.GetFieldType(0));
        Assert.Equal(5L, r.GetInt64(0));
        Assert.Equal(5L, r.GetFieldValue<long>(0));
    }

    [Fact]
    public void Double_GetDouble_AndFieldType()
    {
        using var r = SingleRowReader("c", "double", 1.5);
        Assert.Equal(typeof(double), r.GetFieldType(0));
        Assert.Equal(1.5, r.GetDouble(0));
    }

    [Fact]
    public void String_GetString_AndFieldType()
    {
        using var r = SingleRowReader("c", "string", "abc");
        Assert.Equal(typeof(string), r.GetFieldType(0));
        Assert.Equal("abc", r.GetString(0));
    }

    [Fact]
    public void Boolean_GetBoolean_AndFieldType()
    {
        using var r = SingleRowReader("c", "boolean", true);
        Assert.Equal(typeof(bool), r.GetFieldType(0));
        Assert.True(r.GetBoolean(0));
    }

    [Fact]
    public void Decimal_GetDecimal_AndFieldType()
    {
        using var r = SingleRowReader("c", "decimal(10,2)", 9.99m);
        Assert.Equal(typeof(decimal), r.GetFieldType(0));
        Assert.Equal(9.99m, r.GetDecimal(0));
    }

    // ---------- 新增：interval ----------

    [Fact]
    public void IntervalDayTime_GetFieldValue_TimeSpan()
    {
        var ts = TimeSpan.FromSeconds(2.5);
        using var r = SingleRowReader("c", "interval_day_time", ts);
        Assert.Equal(typeof(TimeSpan), r.GetFieldType(0));
        Assert.Equal(ts, r.GetFieldValue<TimeSpan>(0));
        Assert.Equal(ts, r.GetValue(0));
    }

    [Fact]
    public void IntervalYearMonth_GetFieldValue_Long()
    {
        using var r = SingleRowReader("c", "interval_year_month", 18L);
        Assert.Equal(typeof(long), r.GetFieldType(0));
        Assert.Equal(18L, r.GetFieldValue<long>(0));
        Assert.Equal(18L, r.GetInt64(0));
    }

    // ---------- 新增：vector ----------

    [Fact]
    public void Vector_GetFieldValue_DoubleArray()
    {
        var vec = new double[] { 1.5, 2.5, 3.5 };
        using var r = SingleRowReader("c", "vector<float,32>", vec);
        Assert.Equal(typeof(double[]), r.GetFieldType(0));
        Assert.Equal(vec, r.GetFieldValue<double[]>(0));
        Assert.Equal(vec, r.GetValue(0));
    }

    // ---------- 复合类型（GetValue 暴露 decoder 原生产物） ----------

    [Fact]
    public void Array_GetValue_IsList()
    {
        var list = new List<object?> { 1L, 2L, 3L };
        using var r = SingleRowReader("c", "array<bigint>", list);
        // GetFieldType 约定为 object[]；实际 GetValue 返回 IList（decoder 产物）
        Assert.Equal(typeof(object[]), r.GetFieldType(0));
        var got = Assert.IsAssignableFrom<IList>(r.GetValue(0));
        Assert.Equal(3, got.Count);
        Assert.Equal(1L, got[0]);
        Assert.Equal(new[] { 1L, 2L, 3L }, r.GetFieldValue<IList>(0).Cast<object>().Select(x => (long)x));
    }

    [Fact]
    public void Map_GetValue_IsDictionary()
    {
        var dict = new Dictionary<object, object?> { ["a"] = 1L, ["b"] = 2L };
        using var r = SingleRowReader("c", "map<string,bigint>", dict);
        Assert.Equal(typeof(IDictionary), r.GetFieldType(0));
        var got = Assert.IsAssignableFrom<IDictionary>(r.GetValue(0));
        Assert.Equal(2, got.Count);
    }

    [Fact]
    public void Struct_GetFieldValue_ObjectArray()
    {
        var fields = new object?[] { 1L, "x" };
        using var r = SingleRowReader("c", "struct<a:bigint,b:string>", fields);
        Assert.Equal(typeof(object[]), r.GetFieldType(0));
        Assert.Equal(fields, r.GetFieldValue<object[]>(0));
    }

    // ---------- NULL ----------

    [Fact]
    public void Null_IsDBNull_True()
    {
        using var r = SingleRowReader("c", "bigint", DBNull.Value);
        Assert.True(r.IsDBNull(0));
    }
}
