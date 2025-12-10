
using Azrng.Core.Extension.GuardClause;

namespace Common.Core.Test.Helper.GuardTest;

public class GuardAgainstNullTest
{
    [Fact]
    public void GivenUnNullValueDoseNothing()
    {
        Guard.Against.Null("", "string");
        Guard.Against.Null(1, "int");
        Guard.Against.Null(Guid.Empty, "guid");
        Guard.Against.Null(DateTime.Now, "datetime");
        Guard.Against.Null(new object(), "object");
    }

    [Fact]
    public void ThrowsGivenNullValue()
    {
        object obj = null!;
        Assert.Throws<ArgumentNullException>(() => Guard.Against.Null(obj, "null"));
    }

    [Fact]
    public void ThrowsGivenWhiteSpaceValue()
    {
        var obj = "";
        var exception = Assert.Throws<ArgumentException>(() => Guard.Against.NullOrWhiteSpace(obj, "null"));
        Assert.Equal("null (Parameter 'obj')", exception.Message);
        Assert.Equal("obj", exception.ParamName);
    }

    [Fact]
    public void ThrowsCustomExceptionWhenSuppliedGivenNullValue()
    {
        object obj = null!;
        Assert.Throws<Exception>(() => Guard.Against.Null(obj, "null", exceptionCreator: () => new Exception()));
    }

    [Fact]
    public void ReturnsExpectedValueWhenGivenNonNullValue()
    {
        Assert.Equal("", Guard.Against.Null("", "string"));
        Assert.Equal(1, Guard.Against.Null(1, "int"));

        var guid = Guid.Empty;
        Assert.Equal(guid, Guard.Against.Null(guid, "guid"));

        var now = DateTime.Now;
        Assert.Equal(now, Guard.Against.Null(now, "datetime"));

        var obj = new Object();
        Assert.Equal(obj, Guard.Against.Null(obj, "object"));
    }

    [Fact]
    public void ReturnsNonNullableValueTypeWhenGivenNullableValueTypeIsNotNull()
    {
        int? @int = 4;
        Assert.False(IsNullableType(Guard.Against.Null(@int, parameterName: "int")));

        double? @double = 11.11;
        Assert.False(IsNullableType(Guard.Against.Null(@double, parameterName: "@double")));

        DateTime? now = DateTime.Now;
        Assert.False(IsNullableType(Guard.Against.Null(now, parameterName: "now")));

        Guid? guid = Guid.Empty;
        Assert.False(IsNullableType(Guard.Against.Null(guid, parameterName: "guid")));

        static bool IsNullableType<T>(T value)
        {
            if (value is null)
            {
                return false;
            }

            var type = typeof(T);
            if (!type.IsValueType)
            {
                return true;
            }

            return Nullable.GetUnderlyingType(type) != null;
        }
    }

    /// <summary>
    /// 异常信息匹配
    /// </summary>
    /// <param name="customMessage"></param>
    /// <param name="expectedMessage"></param>
    [Theory]
    [InlineData(null, "Value cannot be null. (Parameter 'parameterName')")]
    [InlineData("Please provide correct value", "Please provide correct value (Parameter 'parameterName')")]
    public void ErrorMessageMatchesExpected(string customMessage, string expectedMessage)
    {
        string nullString = null;
        var exception = Assert.Throws<ArgumentNullException>(() => Guard.Against.Null(nullString, customMessage, "parameterName"));
        Assert.NotNull(exception);
        Assert.NotNull(exception.Message);
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void ErrorMessageMatchesExpectedWhenNameNotExplicitlyProvided()
    {
        string xyz = null;

        var exception = Assert.Throws<ArgumentNullException>(() => Guard.Against.Null(xyz));

        Assert.NotNull(exception);
        Assert.NotNull(exception.Message);
        Assert.Contains($"Value cannot be null. (Parameter '{nameof(xyz)}')", exception.Message);
    }

    [Theory]

    // [InlineData(null, null)]
    // [InlineData(null, "Please provide correct value")]
    // [InlineData("SomeParameter", null)]
    [InlineData("SomeOtherParameter", "Value must be correct")]
    public void ExceptionParamNameMatchesExpected(string expectedParamName, string customMessage)
    {
        string nullString = null;
        var exception = Assert.Throws<ArgumentNullException>(() => Guard.Against.Null(nullString, customMessage, expectedParamName));
        Assert.NotNull(exception);
        Assert.Equal(expectedParamName, exception.ParamName);
    }
}