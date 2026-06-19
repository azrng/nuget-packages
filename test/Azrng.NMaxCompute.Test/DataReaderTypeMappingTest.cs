using Azrng.NMaxCompute.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Azrng.NMaxCompute.Test;

public class DataReaderTypeMappingTest
{
    [Fact]
    public void GetFieldType_ReturnsString_WhenColumnTypesMissing()
    {
        var result = new QueryResult
        {
            Columns = new[] { "a", "b" },
            Rows = new[] { new object[] { "1", "2" } }
        };

        using var reader = new MaxComputeDataReader(result, NullLogger.Instance);
        Assert.Equal(typeof(string), reader.GetFieldType(0));
        Assert.Equal(typeof(string), reader.GetFieldType(1));
        Assert.Equal("String", reader.GetDataTypeName(0));
    }

    [Fact]
    public void GetFieldType_ReturnsClrType_WhenColumnTypesPresent()
    {
        var result = new QueryResult
        {
            Columns = new[] { "id", "score", "flag", "name", "ts" },
            ColumnTypes = new[] { "bigint", "double", "boolean", "string", "datetime" },
            Rows = new[] { new object[] { 1L, 1.5, true, "x", DateTime.Now } }
        };

        using var reader = new MaxComputeDataReader(result, NullLogger.Instance);
        Assert.Equal(typeof(long), reader.GetFieldType(0));
        Assert.Equal(typeof(double), reader.GetFieldType(1));
        Assert.Equal(typeof(bool), reader.GetFieldType(2));
        Assert.Equal(typeof(string), reader.GetFieldType(3));
        Assert.Equal(typeof(DateTime), reader.GetFieldType(4));

        Assert.Equal("bigint", reader.GetDataTypeName(0));
        Assert.Equal("double", reader.GetDataTypeName(1));
        Assert.Equal("boolean", reader.GetDataTypeName(2));
        Assert.Equal("string", reader.GetDataTypeName(3));
        Assert.Equal("datetime", reader.GetDataTypeName(4));
    }

    [Theory]
    [InlineData("tinyint", typeof(long))]
    [InlineData("smallint", typeof(long))]
    [InlineData("int", typeof(long))]
    [InlineData("bigint", typeof(long))]
    [InlineData("float", typeof(float))]
    [InlineData("double", typeof(double))]
    [InlineData("boolean", typeof(bool))]
    [InlineData("string", typeof(string))]
    [InlineData("decimal(10,2)", typeof(decimal))]
    [InlineData("decimal", typeof(decimal))]
    [InlineData("datetime", typeof(DateTime))]
    [InlineData("date", typeof(DateOnly))]
    [InlineData("timestamp", typeof(DateTimeOffset))]
    [InlineData("timestamp_ntz", typeof(DateTimeOffset))]
    [InlineData("varchar", typeof(string))]
    [InlineData("json", typeof(string))]
    [InlineData("binary", typeof(string))]
    [InlineData("array<bigint>", typeof(object[]))]
    [InlineData("map<string,bigint>", typeof(System.Collections.IDictionary))]
    [InlineData("struct<a:string,b:bigint>", typeof(object[]))]
    public void GetFieldType_MapsOdpsTypes(string odpsType, Type expected)
    {
        var result = new QueryResult
        {
            Columns = new[] { "c" },
            ColumnTypes = new[] { odpsType },
            Rows = new[] { new object[] { null! } }
        };

        using var reader = new MaxComputeDataReader(result);
        Assert.Equal(expected, reader.GetFieldType(0));
    }
}
