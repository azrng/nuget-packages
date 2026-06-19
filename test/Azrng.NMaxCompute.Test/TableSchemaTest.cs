using Azrng.NMaxCompute.Tunnel;
using Xunit;

namespace Azrng.NMaxCompute.Test;

public class TableSchemaTest
{
    [Fact]
    public void Parse_EmptyJson_ReturnsEmpty()
    {
        var schema = TableSchema.Parse("");
        Assert.Empty(schema.Columns);
    }

    [Fact]
    public void Parse_NoColumns_ReturnsEmpty()
    {
        var schema = TableSchema.Parse(@"{""comment"":""test""}");
        Assert.Empty(schema.Columns);
    }

    [Fact]
    public void Parse_SimpleColumns()
    {
        var json = @"{
            ""columns"": [
                {""name"": ""id"", ""type"": ""bigint"", ""comment"": ""pk"", ""isNullable"": false},
                {""name"": ""name"", ""type"": ""string""}
            ]
        }";

        var schema = TableSchema.Parse(json);

        Assert.Equal(2, schema.Columns.Count);
        Assert.Equal("id", schema.Columns[0].Name);
        Assert.Equal("bigint", schema.Columns[0].Type);
        Assert.Equal("pk", schema.Columns[0].Comment);
        Assert.False(schema.Columns[0].IsNullable);

        Assert.Equal("name", schema.Columns[1].Name);
        Assert.Equal("string", schema.Columns[1].Type);
        Assert.True(schema.Columns[1].IsNullable);
    }

    [Fact]
    public void Parse_ComplexType_PreservedAsRaw()
    {
        var json = @"{""columns"":[{""name"":""tags"",""type"":""array<string>""}]}";
        var schema = TableSchema.Parse(json);

        Assert.Equal("array<string>", schema.Columns[0].Type);
    }
}
