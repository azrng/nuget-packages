using Azrng.Core.Extension.GuardClause;
using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class GuardClauseNullExtensionsTests
{
    #region Null (reference type)

    [Fact]
    public void Null_ReferenceType_NonNullValue_ReturnsInput()
    {
        var input = "hello";
        Guard.Against.Null(input).Should().Be("hello");
    }

    [Fact]
    public void Null_ReferenceType_NullValue_ThrowsArgumentNullException()
    {
        string? input = null;
        var act = () => Guard.Against.Null(input);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Null_ReferenceType_NullValue_WithCustomMessage_ThrowsWithMessage()
    {
        string? input = null;
        var act = () => Guard.Against.Null(input, "custom error");
        act.Should().Throw<ArgumentNullException>().WithMessage("*custom error*");
    }

    [Fact]
    public void Null_ReferenceType_NullValue_WithExceptionCreator_ThrowsCustomException()
    {
        string? input = null;
        var act = () => Guard.Against.Null(input, exceptionCreator: () => new InvalidOperationException("custom"));
        act.Should().Throw<InvalidOperationException>().WithMessage("custom");
    }

    #endregion

    #region Null (nullable value type)

    [Fact]
    public void Null_NullableValueType_HasValue_ReturnsValue()
    {
        int? input = 42;
        Guard.Against.Null(input).Should().Be(42);
    }

    [Fact]
    public void Null_NullableValueType_NullValue_ThrowsArgumentNullException()
    {
        int? input = null;
        var act = () => Guard.Against.Null(input);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Null_NullableValueType_NullValue_WithCustomMessage_ThrowsWithMessage()
    {
        int? input = null;
        var act = () => Guard.Against.Null(input, "value required");
        act.Should().Throw<ArgumentNullException>().WithMessage("*value required*");
    }

    [Fact]
    public void Null_NullableValueType_NullValue_WithExceptionCreator_ThrowsCustomException()
    {
        int? input = null;
        var act = () => Guard.Against.Null(input, exceptionCreator: () => new InvalidOperationException("no value"));
        act.Should().Throw<InvalidOperationException>().WithMessage("no value");
    }

    #endregion

    #region NullOrEmpty (string)

    [Fact]
    public void NullOrEmpty_String_NonEmptyValue_ReturnsInput()
    {
        Guard.Against.NullOrEmpty("hello").Should().Be("hello");
    }

    [Fact]
    public void NullOrEmpty_String_NullValue_ThrowsArgumentNullException()
    {
        string? input = null;
        var act = () => Guard.Against.NullOrEmpty(input!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NullOrEmpty_String_EmptyValue_ThrowsArgumentException()
    {
        var act = () => Guard.Against.NullOrEmpty(string.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NullOrEmpty_String_EmptyValue_WithCustomMessage_ThrowsWithMessage()
    {
        var act = () => Guard.Against.NullOrEmpty(string.Empty, "cannot be empty");
        act.Should().Throw<ArgumentException>().WithMessage("*cannot be empty*");
    }

    [Fact]
    public void NullOrEmpty_String_EmptyValue_WithExceptionCreator_ThrowsCustomException()
    {
        var act = () => Guard.Against.NullOrEmpty(string.Empty, exceptionCreator: () => new InvalidOperationException("empty"));
        act.Should().Throw<InvalidOperationException>().WithMessage("empty");
    }

    #endregion

    #region NullOrEmpty (Guid?)

    [Fact]
    public void NullOrEmpty_Guid_NonEmptyValue_ReturnsValue()
    {
        var guid = Guid.NewGuid();
        Guard.Against.NullOrEmpty((Guid?)guid).Should().Be(guid);
    }

    [Fact]
    public void NullOrEmpty_Guid_NullValue_ThrowsArgumentNullException()
    {
        Guid? input = null;
        var act = () => Guard.Against.NullOrEmpty(input);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NullOrEmpty_Guid_EmptyValue_ThrowsArgumentException()
    {
        Guid? input = Guid.Empty;
        var act = () => Guard.Against.NullOrEmpty(input);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NullOrEmpty_Guid_EmptyValue_WithCustomMessage_ThrowsWithMessage()
    {
        Guid? input = Guid.Empty;
        var act = () => Guard.Against.NullOrEmpty(input, "guid required");
        act.Should().Throw<ArgumentException>().WithMessage("*guid required*");
    }

    [Fact]
    public void NullOrEmpty_Guid_EmptyValue_WithExceptionCreator_ThrowsCustomException()
    {
        Guid? input = Guid.Empty;
        var act = () => Guard.Against.NullOrEmpty(input, exceptionCreator: () => new InvalidOperationException("bad guid"));
        act.Should().Throw<InvalidOperationException>().WithMessage("bad guid");
    }

    #endregion

    #region NullOrEmpty (IEnumerable<T>)

    [Fact]
    public void NullOrEmpty_Enumerable_NonEmpty_ReturnsInput()
    {
        var input = new[] { 1, 2, 3 };
        Guard.Against.NullOrEmpty(input).Should().BeEquivalentTo(input);
    }

    [Fact]
    public void NullOrEmpty_Enumerable_NullValue_ThrowsArgumentNullException()
    {
        IEnumerable<int>? input = null;
        var act = () => Guard.Against.NullOrEmpty(input!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NullOrEmpty_Enumerable_EmptyArray_ThrowsArgumentException()
    {
        var act = () => Guard.Against.NullOrEmpty(Array.Empty<int>());
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NullOrEmpty_Enumerable_EmptyList_ThrowsArgumentException()
    {
        var act = () => Guard.Against.NullOrEmpty(new List<string>());
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NullOrEmpty_Enumerable_Empty_WithCustomMessage_ThrowsWithMessage()
    {
        var act = () => Guard.Against.NullOrEmpty(Array.Empty<int>(), "collection required");
        act.Should().Throw<ArgumentException>().WithMessage("*collection required*");
    }

    [Fact]
    public void NullOrEmpty_Enumerable_Empty_WithExceptionCreator_ThrowsCustomException()
    {
        var act = () => Guard.Against.NullOrEmpty(Array.Empty<int>(), exceptionCreator: () => new InvalidOperationException("empty collection"));
        act.Should().Throw<InvalidOperationException>().WithMessage("empty collection");
    }

    #endregion

    #region NullOrWhiteSpace

    [Fact]
    public void NullOrWhiteSpace_NonWhiteSpaceValue_ReturnsInput()
    {
        Guard.Against.NullOrWhiteSpace("hello").Should().Be("hello");
    }

    [Fact]
    public void NullOrWhiteSpace_NullValue_ThrowsArgumentNullException()
    {
        string? input = null;
        var act = () => Guard.Against.NullOrWhiteSpace(input!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NullOrWhiteSpace_EmptyValue_ThrowsArgumentException()
    {
        var act = () => Guard.Against.NullOrWhiteSpace(string.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NullOrWhiteSpace_WhiteSpaceValue_ThrowsArgumentException()
    {
        var act = () => Guard.Against.NullOrWhiteSpace("   ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NullOrWhiteSpace_WhiteSpace_WithCustomMessage_ThrowsWithMessage()
    {
        var act = () => Guard.Against.NullOrWhiteSpace("   ", "cannot be whitespace");
        act.Should().Throw<ArgumentException>().WithMessage("*cannot be whitespace*");
    }

    [Fact]
    public void NullOrWhiteSpace_WhiteSpace_WithExceptionCreator_ThrowsCustomException()
    {
        var act = () => Guard.Against.NullOrWhiteSpace("   ", exceptionCreator: () => new InvalidOperationException("whitespace"));
        act.Should().Throw<InvalidOperationException>().WithMessage("whitespace");
    }

    #endregion

    #region Default

    [Fact]
    public void Default_ReferenceType_NonNullNonDefault_ReturnsInput()
    {
        Guard.Against.Default("hello").Should().Be("hello");
    }

    [Fact]
    public void Default_ReferenceType_NullValue_ThrowsArgumentException()
    {
        string? input = null;
        var act = () => Guard.Against.Default(input);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Default_ValueType_NonDefaultValue_ReturnsInput()
    {
        Guard.Against.Default(42).Should().Be(42);
    }

    [Fact]
    public void Default_ValueType_DefaultValue_ThrowsArgumentException()
    {
        var act = () => Guard.Against.Default(0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Default_ValueType_DefaultValue_WithCustomMessage_ThrowsWithMessage()
    {
        var act = () => Guard.Against.Default(0, "invalid value");
        act.Should().Throw<ArgumentException>().WithMessage("*invalid value*");
    }

    [Fact]
    public void Default_ValueType_DefaultValue_WithExceptionCreator_ThrowsCustomException()
    {
        var act = () => Guard.Against.Default(0, exceptionCreator: () => new InvalidOperationException("is default"));
        act.Should().Throw<InvalidOperationException>().WithMessage("is default");
    }

    #endregion
}
