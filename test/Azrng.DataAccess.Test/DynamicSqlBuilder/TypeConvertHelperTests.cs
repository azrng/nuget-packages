using Azrng.Database.DynamicSqlBuilder.Model;
using Azrng.Database.DynamicSqlBuilder.Utils;

namespace Azrng.DataAccess.Test.DynamicSqlBuilder;

public class TypeConvertHelperTests
{
    [Theory]
    [InlineData(null, MatchOperator.Equal)]
    [InlineData("", MatchOperator.Equal)]
    [InlineData("   ", MatchOperator.Equal)]
    [InlineData("IN", MatchOperator.In)]
    [InlineData("not in", MatchOperator.NotIn)]
    [InlineData("NOTIN", MatchOperator.NotIn)]
    [InlineData("LIKE", MatchOperator.Like)]
    [InlineData("NOT LIKE", MatchOperator.NotLike)]
    [InlineData("!=", MatchOperator.NotEqual)]
    [InlineData("<>", MatchOperator.NotEqual)]
    [InlineData("==", MatchOperator.Equal)]
    [InlineData("AND", MatchOperator.And)]
    [InlineData(">", MatchOperator.GreaterThan)]
    [InlineData("<", MatchOperator.LessThan)]
    [InlineData(">=", MatchOperator.GreaterThanEqual)]
    [InlineData("<=", MatchOperator.LessThanEqual)]
    [InlineData("BETWEEN", MatchOperator.Between)]
    [InlineData("unknown", MatchOperator.Equal)]
    public void ConvertToEnum_ShouldMapOperatorStrings(string? operatorText, MatchOperator expected)
    {
        var actual = TypeConvertHelper.ConvertToEnum(operatorText!);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("123", typeof(int), 123)]
    [InlineData("1234567890123", typeof(long), 1234567890123L)]
    [InlineData("1.25", typeof(decimal), "1.25")]
    [InlineData("true", typeof(bool), true)]
    [InlineData("5", typeof(short), (short)5)]
    [InlineData("6", typeof(byte), (byte)6)]
    [InlineData("7", typeof(uint), 7u)]
    [InlineData("8", typeof(ulong), 8ul)]
    [InlineData("9", typeof(ushort), (ushort)9)]
    public void ConvertToTargetType_ShouldConvertPrimitiveValues(object value, Type targetType, object expected)
    {
        var actual = TypeConvertHelper.ConvertToTargetType(value, targetType);

        if (targetType == typeof(decimal))
        {
            Assert.Equal(decimal.Parse((string)expected), actual);
        }
        else
        {
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void ConvertToTargetType_ShouldConvertEnumFromStringAndNumber()
    {
        Assert.Equal(MatchOperator.Like, TypeConvertHelper.ConvertToTargetType("Like", typeof(MatchOperator)));
        Assert.Equal(MatchOperator.NotEqual, TypeConvertHelper.ConvertToTargetType(1, typeof(MatchOperator)));
    }

    [Fact]
    public void ConvertToTargetType_ShouldConvertGuidFromStringAndBytes()
    {
        var guid = Guid.NewGuid();

        Assert.Equal(guid, TypeConvertHelper.ConvertToTargetType(guid.ToString(), typeof(Guid)));
        Assert.Equal(guid, TypeConvertHelper.ConvertToTargetType(guid.ToByteArray(), typeof(Guid)));
    }

    [Fact]
    public void ConvertToTargetType_ShouldReturnDefaultsForNullAndDbNull()
    {
        Assert.Equal(0, TypeConvertHelper.ConvertToTargetType((object)null!, typeof(int)));
        Assert.Equal(string.Empty, TypeConvertHelper.ConvertToTargetType(DBNull.Value, typeof(string)));
        Assert.Null(TypeConvertHelper.ConvertToTargetType((object)null!, typeof(Uri)));
    }

    [Fact]
    public void ConvertToTargetType_ShouldReturnDefaultOrThrowWhenConversionFails()
    {
        Assert.Equal(0, TypeConvertHelper.ConvertToTargetType("not-a-number", typeof(int), throwOnError: false));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            TypeConvertHelper.ConvertToTargetType("not-a-number", typeof(int)));

        Assert.Contains("无法将值", exception.Message);
    }

    [Fact]
    public void ConvertToTargetTypeCollection_ShouldReturnStrongTypedListAndUseDefaultsForInvalidValuesByDefault()
    {
        var result = TypeConvertHelper.ConvertToTargetType(
            new object[] { "1", "bad", 3 },
            typeof(int));

        var values = Assert.IsType<List<int>>(result);
        Assert.Equal(new[] { 1, 0, 3 }, values);
    }

    [Fact]
    public void ConvertToTargetTypeCollection_ShouldThrowWhenThrowOnErrorIsTrue()
    {
        Assert.Throws<InvalidOperationException>(() =>
            TypeConvertHelper.ConvertToTargetType(new object[] { "1", "bad" }, typeof(int), throwOnError: true));
    }

    [Fact]
    public void GetDefaultVaule_ShouldHandleNullableAndReferenceTypes()
    {
        Assert.Equal(0, TypeConvertHelper.GetDefaultVaule(typeof(int?)));
        Assert.Equal(string.Empty, TypeConvertHelper.GetDefaultVaule(typeof(string)));
        Assert.Null(TypeConvertHelper.GetDefaultVaule(typeof(Uri)));
    }
}
