using Azrng.Core.Extension.GuardClause;

namespace Common.Core.Test.Helper.GuardTest;

public class GuardAgainstNullOrEmptyTest
{
    [Fact]
    public void GivenUnEmptyValueNoNothing()
    {
        Guard.Against.NullOrEmpty("123", parameterName: "string");
    }

    [Fact]
    public void DoesNothingGivenNonEmptyGuidValue()
    {
        Guard.Against.NullOrEmpty(Guid.NewGuid(), parameterName: "guid");
    }

    [Fact]
    public void DoesNothingGivenNonEmptyEnumerable()
    {
        Guard.Against.NullOrEmpty(new[]
                                  {
                                      "foo",
                                      "bar"
                                  }, parameterName: "stringArray");
        Guard.Against.NullOrEmpty(new[]
                                  {
                                      1,
                                      2
                                  }, parameterName: "intArray");
    }

    [Fact]
    public void GivenNullStringValueThrows()
    {
        string nullString = null;
        Assert.Throws<ArgumentNullException>(() => Guard.Against.NullOrEmpty(nullString, parameterName: "null String"));
    }

    [Fact]
    public void GivenEmptyStringValueThrows()
    {
        Assert.Throws<ArgumentException>(() => Guard.Against.NullOrEmpty("", parameterName: "null String"));
    }

    [Fact]
    public void ThrowsCustomExceptionWhenSuppliedGivenEmptyString()
    {
        var customException = new Exception();
        Assert.Throws<Exception>(() =>
            Guard.Against.NullOrEmpty("", parameterName: "emptyString", exceptionCreator: () => customException));
    }

    [Fact]
    public void ThrowsGivenNullGuid()
    {
        Guid? nullGuid = null;
        Assert.Throws<ArgumentNullException>(() => Guard.Against.NullOrEmpty(nullGuid, parameterName: "nullGuid"));
    }

    [Fact]
    public void ThrowsGivenEmptyGuid()
    {
        Assert.Throws<ArgumentException>(() => Guard.Against.NullOrEmpty(Guid.Empty, parameterName: "emptyGuid"));
    }

    [Fact]
    public void ThrowsCustomExceptionWhenSuppliedGivenEmptyGuid()
    {
        var customException = new Exception();
        Assert.Throws<Exception>(() =>
            Guard.Against.NullOrEmpty(Guid.Empty, parameterName: "emptyGuid", exceptionCreator: () => customException));
    }

    [Fact]
    public void ThrowsGivenEmptyEnumerable()
    {
        Assert.Throws<ArgumentException>(() =>
            Guard.Against.NullOrEmpty(Enumerable.Empty<string>(), parameterName: "emptyStringEnumerable"));
    }

    [Fact]
    public void ThrowsCustomExceptionWhenSuppliedGivenEmptyEnumerable()
    {
        var customException = new Exception();
        Assert.Throws<Exception>(() => Guard.Against.NullOrEmpty(Enumerable.Empty<string>(), parameterName: "emptyStringEnumerable",
            exceptionCreator: () => customException));
    }

    [Fact]
    public void ReturnsExpectedValueWhenGivenValidValue()
    {
        Assert.Equal("a", Guard.Against.NullOrEmpty("a", parameterName: "string"));
        Assert.Equal("1", Guard.Against.NullOrEmpty("1", parameterName: "aNumericString"));

        var collection1 = new[]
                          {
                              "foo",
                              "bar"
                          };
        Assert.Equal(collection1, Guard.Against.NullOrEmpty(collection1, parameterName: "stringArray"));

        var collection2 = new[]
                          {
                              1,
                              2
                          };
        Assert.Equal(collection2, Guard.Against.NullOrEmpty(collection2, parameterName: "intArray"));
    }

    [Theory]
    [InlineData(null, "Required input xyz was empty. (Parameter 'xyz')")]
    [InlineData("Value is empty", "Value is empty (Parameter 'xyz')")]
    public void ErrorMessageMatchesExpectedWhenNameNotExplicitlyProvidedGivenStringValue(string customMessage, string expectedMessage)
    {
        var xyz = string.Empty;

        var exception = Assert.Throws<ArgumentException>(() => Guard.Against.NullOrEmpty(xyz, message: customMessage));

        Assert.NotNull(exception);
        Assert.NotNull(exception.Message);
        Assert.Contains(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData(null, "Required input xyz was empty. (Parameter 'xyz')")]
    [InlineData("Value is empty", "Value is empty (Parameter 'xyz')")]
    public void ErrorMessageMatchesExpectedWhenNameNotExplicitlyProvidedGivenGuidValue(string customMessage, string expectedMessage)
    {
        var xyz = Guid.Empty;

        var exception = Assert.Throws<ArgumentException>(() => Guard.Against.NullOrEmpty(xyz, message: customMessage));

        Assert.NotNull(exception);
        Assert.NotNull(exception.Message);
        Assert.Contains(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData(null, "Required input xyz was empty. (Parameter 'xyz')")]
    [InlineData("Value is empty", "Value is empty (Parameter 'xyz')")]
    public void ErrorMessageMatchesExpectedWhenNameNotExplicitlyProvidedGivenIEnumerableValue(string? customMessage, string expectedMessage)
    {
        var xyz = Enumerable.Empty<string>();

        var exception = Assert.Throws<ArgumentException>(() => Guard.Against.NullOrEmpty(xyz, message: customMessage));

        Assert.NotNull(exception);
        Assert.NotNull(exception.Message);
        Assert.Contains(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData(null, "Required input parameterName was empty. (Parameter 'parameterName')")]
    [InlineData("Value is empty", "Value is empty (Parameter 'parameterName')")]
    public void ErrorMessageMatchesExpectedWhenInputIsEmpty(string customMessage, string expectedMessage)
    {
        var emptyString = string.Empty;
        var emptyGuid = Guid.Empty;
        var emptyEnumerable = Enumerable.Empty<string>();

        var clausesToEvaluate = new List<Action>
                                {
                                    () => Guard.Against.NullOrEmpty(emptyString, message: customMessage, parameterName: "parameterName"),
                                    () => Guard.Against.NullOrEmpty(emptyGuid, message: customMessage, parameterName: "parameterName"),
                                    () => Guard.Against.NullOrEmpty(emptyEnumerable, message: customMessage, parameterName: "parameterName")
                                };

        foreach (var clauseToEvaluate in clausesToEvaluate)
        {
            var exception = Assert.Throws<ArgumentException>(clauseToEvaluate);
            Assert.NotNull(exception);
            Assert.NotNull(exception.Message);
            Assert.Equal(expectedMessage, exception.Message);
        }
    }

    [Theory]
    [InlineData(null, "Value cannot be null. (Parameter 'parameterName')")]
    [InlineData("Value must be correct", "Value must be correct (Parameter 'parameterName')")]
    public void ErrorMessageMatchesExpectedWhenInputIsNull(string customMessage, string expectedMessage)
    {
        string nullString = null;
        Guid? nullGuid = null;
        IEnumerable<string> nullEnumerable = null;

        var clausesToEvaluate = new List<Action>
                                {
                                    () => Guard.Against.NullOrEmpty(nullString, message: customMessage, parameterName: "parameterName"),
                                    () => Guard.Against.NullOrEmpty(nullGuid, message: customMessage, parameterName: "parameterName"),
                                    () => Guard.Against.NullOrEmpty(nullEnumerable, message: customMessage, parameterName: "parameterName")
                                };

        foreach (var clauseToEvaluate in clausesToEvaluate)
        {
            var exception = Assert.Throws<ArgumentNullException>(clauseToEvaluate);
            Assert.NotNull(exception);
            Assert.NotNull(exception.Message);
            Assert.Equal(expectedMessage, exception.Message);
        }
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(null, "Please provide correct value")]
    [InlineData("SomeParameter", null)]
    [InlineData("SomeOtherParameter", "Value must be correct")]
    public void ExceptionParamNameMatchesExpectedWhenInputIsEmpty(string expectedParamName, string customMessage)
    {
        var emptyString = string.Empty;
        var emptyGuid = Guid.Empty;
        var emptyEnumerable = Enumerable.Empty<string>();

        var clausesToEvaluate = new List<Action>
                                {
                                    () => Guard.Against.NullOrEmpty(emptyString, message: customMessage, parameterName: expectedParamName),
                                    () => Guard.Against.NullOrEmpty(emptyGuid, message: customMessage, parameterName: expectedParamName),
                                    () => Guard.Against.NullOrEmpty(emptyEnumerable, message: customMessage,
                                        parameterName: expectedParamName)
                                };

        foreach (var clauseToEvaluate in clausesToEvaluate)
        {
            var exception = Assert.Throws<ArgumentException>(clauseToEvaluate);
            Assert.NotNull(exception);
            Assert.Equal(expectedParamName, exception.ParamName);
        }
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(null, "Please provide correct value")]
    [InlineData("SomeParameter", null)]
    [InlineData("SomeOtherParameter", "Value must be correct")]
    public void ExceptionParamNameMatchesExpectedWhenInputIsNull(string expectedParamName, string customMessage)
    {
        string nullString = null;
        Guid? nullGuid = null;
        IEnumerable<string> nullEnumerable = null;

        var clausesToEvaluate = new List<Action>
                                {
                                    () => Guard.Against.NullOrEmpty(nullString, message: customMessage, parameterName: expectedParamName),
                                    () => Guard.Against.NullOrEmpty(nullGuid, message: customMessage, parameterName: expectedParamName),
                                    () => Guard.Against.NullOrEmpty(nullEnumerable, message: customMessage,
                                        parameterName: expectedParamName)
                                };

        foreach (var clauseToEvaluate in clausesToEvaluate)
        {
            var exception = Assert.Throws<ArgumentNullException>(clauseToEvaluate);
            Assert.NotNull(exception);
            Assert.Equal(expectedParamName, exception.ParamName);
        }
    }
}