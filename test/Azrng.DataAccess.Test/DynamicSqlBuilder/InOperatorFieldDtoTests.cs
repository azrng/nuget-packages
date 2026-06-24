using Azrng.Database.DynamicSqlBuilder.Model;

namespace Azrng.DataAccess.Test.DynamicSqlBuilder;

public class InOperatorFieldDtoTests
{
    [Fact]
    public void Create_ShouldInferValueTypeForInOperator()
    {
        var field = InOperatorFieldDto.Create("status", new[] { 1, 2, 3 });

        Assert.Equal("status", field.Field);
        Assert.Equal(typeof(int), field.ValueType);
        Assert.Equal(new object[] { 1, 2, 3 }, field.Ids);
    }

    [Fact]
    public void Create_ShouldInferValueTypeForNotInOperator()
    {
        var field = NotInOperatorFieldDto.Create("code", new[] { "a", "b" });

        Assert.Equal("code", field.Field);
        Assert.Equal(typeof(string), field.ValueType);
        Assert.Equal(new object[] { "a", "b" }, field.Ids);
    }

    [Fact]
    public void Create_ShouldUseEmptySequenceWhenValuesAreNull()
    {
        var inField = InOperatorFieldDto.Create<int>("status", null!);
        var notInField = NotInOperatorFieldDto.Create<int>("status", null!);

        Assert.Empty(inField.Ids);
        Assert.Empty(notInField.Ids);
        Assert.Equal(typeof(int), inField.ValueType);
        Assert.Equal(typeof(int), notInField.ValueType);
    }
}
