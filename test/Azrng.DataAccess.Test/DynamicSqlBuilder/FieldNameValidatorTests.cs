using Azrng.Database.DynamicSqlBuilder.Validation;

namespace Azrng.DataAccess.Test.DynamicSqlBuilder;

public class FieldNameValidatorTests
{
    [Theory]
    [InlineData("name")]
    [InlineData("_name")]
    [InlineData("users.name")]
    [InlineData("\"Column\"")]
    [InlineData("[Column]")]
    [InlineData("`Column`")]
    [InlineData("users.\"DisplayName\"")]
    public void ValidateFieldName_ShouldAcceptSafeIdentifiers(string fieldName)
    {
        FieldNameValidator.ValidateFieldName(fieldName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("users.profile.name")]
    [InlineData("select")]
    [InlineData("name;drop")]
    [InlineData("\"name;drop\"")]
    [InlineData("sp_help")]
    [InlineData("xp_cmdshell")]
    public void ValidateFieldName_ShouldRejectInvalidOrDangerousIdentifiers(string fieldName)
    {
        Assert.Throws<ArgumentException>(() => FieldNameValidator.ValidateFieldName(fieldName));
    }

    [Fact]
    public void AreValidFieldNames_ShouldReturnInvalidNames()
    {
        var isValid = FieldNameValidator.AreValidFieldNames(
            new[] { "id", "users.name", "drop", "bad name" },
            out var invalidFieldNames);

        Assert.False(isValid);
        Assert.Equal(new[] { "drop", "bad name" }, invalidFieldNames);
    }
}
