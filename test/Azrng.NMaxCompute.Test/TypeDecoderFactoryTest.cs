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
    [InlineData("json", typeof(StringDecoder))]
    [InlineData("datetime", typeof(DateTimeDecoder))]
    [InlineData("date", typeof(DateDecoder))]
    [InlineData("decimal", typeof(DecimalDecoder))]
    [InlineData("decimal(10,2)", typeof(DecimalDecoder))]
    public void GetDecoder_MapsKnownTypes(string typeString, Type expected)
    {
        var decoder = TypeDecoderFactory.GetDecoder(typeString);
        Assert.IsType(expected, decoder);
    }

    [Theory]
    [InlineData("array<string>")]
    [InlineData("map<string,bigint>")]
    [InlineData("struct<a:string,b:bigint>")]
    [InlineData("timestamp")]
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
