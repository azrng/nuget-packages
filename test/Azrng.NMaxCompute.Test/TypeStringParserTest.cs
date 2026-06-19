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

    private static ITypeDecoder GetElementDecoder(ArrayDecoder d)
    {
        // 反射拿私有 _elementDecoder（测试用）
        var field = typeof(ArrayDecoder).GetField("_elementDecoder",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (ITypeDecoder)field!.GetValue(d)!;
    }
}
