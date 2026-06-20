using Azrng.NMaxCompute.Tunnel.Types;
using Xunit;

namespace Azrng.NMaxCompute.Test;

public class TypeStringParserTest
{
    [Fact]
    public void Parse_ArrayOfBigInt()
    {
        var d = TypeStringParser.Parse("array<bigint>");
        var array = Assert.IsType<ArrayDecoder>(d);
        Assert.Same(IntegerDecoder.Instance, GetElementDecoder(array));
    }

    [Fact]
    public void Parse_Map()
    {
        var d = TypeStringParser.Parse("map<string,bigint>");
        Assert.IsType<MapDecoder>(d);
    }

    [Fact]
    public void Parse_Struct()
    {
        var d = TypeStringParser.Parse("struct<name:string,age:bigint>");
        var s = Assert.IsType<StructDecoder>(d);
        Assert.Equal(new[] { "name", "age" }, s.FieldNames);
    }

    [Fact]
    public void Parse_NestedArrayInMap()
    {
        var d = TypeStringParser.Parse("map<string,array<bigint>>");
        Assert.IsType<MapDecoder>(d);
    }

    [Fact]
    public void Parse_DeeplyNested()
    {
        var d = TypeStringParser.Parse("array<array<array<bigint>>>");
        var outer = Assert.IsType<ArrayDecoder>(d);
        var inner = Assert.IsType<ArrayDecoder>(GetElementDecoder(outer));
        var innermost = Assert.IsType<ArrayDecoder>(GetElementDecoder(inner));
        Assert.Same(IntegerDecoder.Instance, GetElementDecoder(innermost));
    }

    [Fact]
    public void Parse_WhitespaceTolerant()
    {
        var d = TypeStringParser.Parse("map< string , bigint >");
        Assert.IsType<MapDecoder>(d);
    }

    [Fact]
    public void Parse_BasicTypeFallsBackToFactory()
    {
        Assert.Same(IntegerDecoder.Instance, TypeStringParser.Parse("bigint"));
    }

    [Fact]
    public void Parse_StructWithBackquotedName()
    {
        var d = TypeStringParser.Parse("struct<`my field`:string>");
        var s = Assert.IsType<StructDecoder>(d);
        Assert.Equal("my field", s.FieldNames[0]);
    }

    /// <summary>
    /// 回归：struct 字段为复合类型（曾导致 parser 把 'b:array' 当复合类型名而抛 NotSupportedException）。
    /// </summary>
    [Fact]
    public void Parse_StructWithCompositeFieldType()
    {
        var d = TypeStringParser.Parse("struct<a:bigint,b:array<string>>");
        var s = Assert.IsType<StructDecoder>(d);
        Assert.Equal(new[] { "a", "b" }, s.FieldNames);
    }

    /// <summary>
    /// 回归：复合类型内嵌带长度的基本类型 varchar(n)。
    /// </summary>
    [Fact]
    public void Parse_CompositeWithSizedPrimitive()
    {
        Assert.IsType<ArrayDecoder>(TypeStringParser.Parse("array<varchar(10)>"));
        Assert.IsType<MapDecoder>(TypeStringParser.Parse("map<decimal(10,2),string>"));
    }

    private static ITypeDecoder GetElementDecoder(ArrayDecoder d)
    {
        // 反射拿私有 _elementDecoder（测试用）
        var field = typeof(ArrayDecoder).GetField("_elementDecoder",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (ITypeDecoder)field!.GetValue(d)!;
    }
}
