using Azrng.NMaxCompute.Tunnel.Types;
using Xunit;

namespace Azrng.NMaxCompute.Test;

public class TypeDecoderFactoryTest
{
    [Theory]
    [InlineData("bigint", typeof(IntegerDecoder))]
    [InlineData("BIGINT", typeof(IntegerDecoder))]
    [InlineData("int", typeof(IntegerDecoder))]
    [InlineData("tinyint", typeof(IntegerDecoder))]
    [InlineData("smallint", typeof(IntegerDecoder))]
    [InlineData("double", typeof(DoubleDecoder))]
    [InlineData("float", typeof(FloatDecoder))]
    [InlineData("boolean", typeof(BooleanDecoder))]
    [InlineData("bool", typeof(BooleanDecoder))]
    [InlineData("string", typeof(StringDecoder))]
    [InlineData("binary", typeof(StringDecoder))]
    [InlineData("varchar", typeof(StringDecoder))]
    [InlineData("char", typeof(StringDecoder))]
    [InlineData("json", typeof(JsonDecoder))]
    [InlineData("datetime", typeof(DateTimeDecoder))]
    [InlineData("date", typeof(DateDecoder))]
    [InlineData("timestamp", typeof(TimestampDecoder))]
    [InlineData("timestamp_ntz", typeof(TimestampDecoder))]
    [InlineData("decimal", typeof(DecimalDecoder))]
    [InlineData("decimal(10,2)", typeof(DecimalDecoder))]
    public void GetDecoder_MapsKnownTypes(string typeString, Type expected)
    {
        var decoder = TypeDecoderFactory.GetDecoder(typeString);
        Assert.IsType(expected, decoder);
    }

    [Theory]
    [InlineData("array<string>", typeof(ArrayDecoder))]
    [InlineData("map<string,bigint>", typeof(MapDecoder))]
    [InlineData("struct<a:string,b:bigint>", typeof(StructDecoder))]
    [InlineData("array<array<bigint>>", typeof(ArrayDecoder))]
    public void GetDecoder_ParsesCompositeTypes(string typeString, Type expected)
    {
        var decoder = TypeDecoderFactory.GetDecoder(typeString);
        Assert.IsType(expected, decoder);
    }

    [Theory]
    [InlineData("unknown_type")]
    public void GetDecoder_Unsupported_Throws(string typeString)
    {
        Assert.Throws<NotSupportedException>(() => TypeDecoderFactory.GetDecoder(typeString));
    }

    [Fact]
    public void GetDecoder_Empty_Throws()
    {
        Assert.Throws<ArgumentException>(() => TypeDecoderFactory.GetDecoder(""));
        Assert.Throws<ArgumentException>(() => TypeDecoderFactory.GetDecoder("   "));
    }
}
