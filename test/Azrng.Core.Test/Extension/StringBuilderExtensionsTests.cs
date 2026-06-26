using System.Text;
using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class StringBuilderExtensionsTests
{
    #region AppendIF

    [Fact]
    public void AppendIF_ConditionTrue_ShouldAppendString()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendIF(true, " world");
        result.ToString().Should().Be("hello world");
    }

    [Fact]
    public void AppendIF_ConditionFalse_ShouldNotAppendString()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendIF(false, " world");
        result.ToString().Should().Be("hello");
    }

    [Fact]
    public void AppendIF_ConditionTrue_NullString_ShouldAppendNull()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendIF(true, null);
        result.ToString().Should().Be("hello");
    }

    [Fact]
    public void AppendIF_ConditionFalse_NullString_ShouldNotChange()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendIF(false, null);
        result.ToString().Should().Be("hello");
    }

    [Fact]
    public void AppendIF_ShouldReturnSameInstance()
    {
        var sb = new StringBuilder();
        var result = sb.AppendIF(true, "test");
        result.Should().BeSameAs(sb);
    }

    #endregion

    #region AppendLineIF

    [Fact]
    public void AppendLineIF_ConditionTrue_ShouldAppendLine()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendLineIF(true, "world");
        result.ToString().Should().Be("hello" + "world" + Environment.NewLine);
    }

    [Fact]
    public void AppendLineIF_ConditionFalse_ShouldNotAppendLine()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendLineIF(false, "world");
        result.ToString().Should().Be("hello");
    }

    [Fact]
    public void AppendLineIF_ConditionTrue_NullString_ShouldAppendEmptyLine()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendLineIF(true, null);
        result.ToString().Should().Be("hello" + Environment.NewLine);
    }

    [Fact]
    public void AppendLineIF_ConditionFalse_NullString_ShouldNotChange()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendLineIF(false, null);
        result.ToString().Should().Be("hello");
    }

    [Fact]
    public void AppendLineIF_ShouldReturnSameInstance()
    {
        var sb = new StringBuilder();
        var result = sb.AppendLineIF(true, "test");
        result.Should().BeSameAs(sb);
    }

    #endregion

    #region AppendFormatIF

    [Fact]
    public void AppendFormatIF_ConditionTrue_ShouldAppendFormattedString()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendFormatIF(true, " {0} {1}", "world", "!");
        result.ToString().Should().Be("hello world !");
    }

    [Fact]
    public void AppendFormatIF_ConditionFalse_ShouldNotAppend()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendFormatIF(false, " {0} {1}", "world", "!");
        result.ToString().Should().Be("hello");
    }

    [Fact]
    public void AppendFormatIF_ConditionTrue_SingleArg_ShouldAppendFormattedString()
    {
        var sb = new StringBuilder();
        var result = sb.AppendFormatIF(true, "value={0}", 42);
        result.ToString().Should().Be("value=42");
    }

    [Fact]
    public void AppendFormatIF_ShouldReturnSameInstance()
    {
        var sb = new StringBuilder();
        var result = sb.AppendFormatIF(true, "test");
        result.Should().BeSameAs(sb);
    }

    #endregion

    #region AppendLineIfNotEmpty

    [Fact]
    public void AppendLineIfNotEmpty_NonEmptyString_ShouldAppendLine()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendLineIfNotEmpty("world");
        result.ToString().Should().Be("hello" + "world" + Environment.NewLine);
    }

    [Fact]
    public void AppendLineIfNotEmpty_EmptyString_ShouldNotAppend()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendLineIfNotEmpty("");
        result.ToString().Should().Be("hello");
    }

    [Fact]
    public void AppendLineIfNotEmpty_Null_ShouldNotAppend()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendLineIfNotEmpty(null);
        result.ToString().Should().Be("hello");
    }

    [Fact]
    public void AppendLineIfNotEmpty_WhitespaceOnly_ShouldAppendLine()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendLineIfNotEmpty("   ");
        result.ToString().Should().Be("hello" + "   " + Environment.NewLine);
    }

    [Fact]
    public void AppendLineIfNotEmpty_ShouldReturnSameInstance()
    {
        var sb = new StringBuilder();
        var result = sb.AppendLineIfNotEmpty("test");
        result.Should().BeSameAs(sb);
    }

    #endregion

    #region AppendLineIfNotNullOrWhiteSpace

    [Fact]
    public void AppendLineIfNotNullOrWhiteSpace_NonEmptyString_ShouldAppendLine()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendLineIfNotNullOrWhiteSpace("world");
        result.ToString().Should().Be("hello" + "world" + Environment.NewLine);
    }

    [Fact]
    public void AppendLineIfNotNullOrWhiteSpace_EmptyString_ShouldNotAppend()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendLineIfNotNullOrWhiteSpace("");
        result.ToString().Should().Be("hello");
    }

    [Fact]
    public void AppendLineIfNotNullOrWhiteSpace_Null_ShouldNotAppend()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendLineIfNotNullOrWhiteSpace(null);
        result.ToString().Should().Be("hello");
    }

    [Fact]
    public void AppendLineIfNotNullOrWhiteSpace_WhitespaceOnly_ShouldNotAppend()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendLineIfNotNullOrWhiteSpace("   ");
        result.ToString().Should().Be("hello");
    }

    [Fact]
    public void AppendLineIfNotNullOrWhiteSpace_ShouldReturnSameInstance()
    {
        var sb = new StringBuilder();
        var result = sb.AppendLineIfNotNullOrWhiteSpace("test");
        result.Should().BeSameAs(sb);
    }

    #endregion

    #region AppendIfNotEmpty

    [Fact]
    public void AppendIfNotEmpty_NonEmptyString_ShouldAppend()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendIfNotEmpty("world");
        result.ToString().Should().Be("helloworld");
    }

    [Fact]
    public void AppendIfNotEmpty_EmptyString_ShouldNotAppend()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendIfNotEmpty("");
        result.ToString().Should().Be("hello");
    }

    [Fact]
    public void AppendIfNotEmpty_Null_ShouldNotAppend()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendIfNotEmpty(null);
        result.ToString().Should().Be("hello");
    }

    [Fact]
    public void AppendIfNotEmpty_WhitespaceOnly_ShouldAppend()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendIfNotEmpty("   ");
        result.ToString().Should().Be("hello   ");
    }

    [Fact]
    public void AppendIfNotEmpty_ShouldReturnSameInstance()
    {
        var sb = new StringBuilder();
        var result = sb.AppendIfNotEmpty("test");
        result.Should().BeSameAs(sb);
    }

    #endregion

    #region AppendIfNotNullOrWhiteSpace

    [Fact]
    public void AppendIfNotNullOrWhiteSpace_NonEmptyString_ShouldAppend()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendIfNotNullOrWhiteSpace("world");
        result.ToString().Should().Be("helloworld");
    }

    [Fact]
    public void AppendIfNotNullOrWhiteSpace_EmptyString_ShouldNotAppend()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendIfNotNullOrWhiteSpace("");
        result.ToString().Should().Be("hello");
    }

    [Fact]
    public void AppendIfNotNullOrWhiteSpace_Null_ShouldNotAppend()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendIfNotNullOrWhiteSpace(null);
        result.ToString().Should().Be("hello");
    }

    [Fact]
    public void AppendIfNotNullOrWhiteSpace_WhitespaceOnly_ShouldNotAppend()
    {
        var sb = new StringBuilder("hello");
        var result = sb.AppendIfNotNullOrWhiteSpace("   ");
        result.ToString().Should().Be("hello");
    }

    [Fact]
    public void AppendIfNotNullOrWhiteSpace_ShouldReturnSameInstance()
    {
        var sb = new StringBuilder();
        var result = sb.AppendIfNotNullOrWhiteSpace("test");
        result.Should().BeSameAs(sb);
    }

    #endregion

    #region RemoveEnd

    [Fact]
    public void RemoveEnd_ValidLength_ShouldRemoveCharacters()
    {
        var sb = new StringBuilder("hello world");
        var result = sb.RemoveEnd(6);
        result.ToString().Should().Be("hello");
    }

    [Fact]
    public void RemoveEnd_RemoveAll_ShouldEmptyBuilder()
    {
        var sb = new StringBuilder("hello");
        var result = sb.RemoveEnd(5);
        result.ToString().Should().BeEmpty();
    }

    [Fact]
    public void RemoveEnd_LengthZero_ShouldNotChange()
    {
        var sb = new StringBuilder("hello");
        var result = sb.RemoveEnd(0);
        result.ToString().Should().Be("hello");
    }

    [Fact]
    public void RemoveEnd_LengthGreaterThanBuilder_ShouldNotChange()
    {
        var sb = new StringBuilder("hi");
        var result = sb.RemoveEnd(10);
        result.ToString().Should().Be("hi");
    }

    [Fact]
    public void RemoveEnd_NegativeLength_ShouldNotChange()
    {
        var sb = new StringBuilder("hello");
        var result = sb.RemoveEnd(-1);
        result.ToString().Should().Be("hello");
    }

    [Fact]
    public void RemoveEnd_EmptyBuilder_ShouldNotChange()
    {
        var sb = new StringBuilder();
        var result = sb.RemoveEnd(3);
        result.ToString().Should().BeEmpty();
    }

    [Fact]
    public void RemoveEnd_ShouldReturnSameInstance()
    {
        var sb = new StringBuilder("hello");
        var result = sb.RemoveEnd(2);
        result.Should().BeSameAs(sb);
    }

    #endregion

    #region RemoveEndComma

    [Fact]
    public void RemoveEndComma_EndsWithComma_ShouldRemoveComma()
    {
        var sb = new StringBuilder("a,b,c,");
        var result = sb.RemoveEndComma();
        result.ToString().Should().Be("a,b,c");
    }

    [Fact]
    public void RemoveEndComma_DoesNotEndWithComma_ShouldNotChange()
    {
        var sb = new StringBuilder("a,b,c");
        var result = sb.RemoveEndComma();
        result.ToString().Should().Be("a,b,c");
    }

    [Fact]
    public void RemoveEndComma_EmptyBuilder_ShouldNotChange()
    {
        var sb = new StringBuilder();
        var result = sb.RemoveEndComma();
        result.ToString().Should().BeEmpty();
    }

    [Fact]
    public void RemoveEndComma_OnlyComma_ShouldEmptyBuilder()
    {
        var sb = new StringBuilder(",");
        var result = sb.RemoveEndComma();
        result.ToString().Should().BeEmpty();
    }

    [Fact]
    public void RemoveEndComma_ShouldReturnSameInstance()
    {
        var sb = new StringBuilder(",");
        var result = sb.RemoveEndComma();
        result.Should().BeSameAs(sb);
    }

    #endregion

    #region RemoveEndSemicolon

    [Fact]
    public void RemoveEndSemicolon_EndsWithSemicolon_ShouldRemoveSemicolon()
    {
        var sb = new StringBuilder("a;b;c;");
        var result = sb.RemoveEndSemicolon();
        result.ToString().Should().Be("a;b;c");
    }

    [Fact]
    public void RemoveEndSemicolon_DoesNotEndWithSemicolon_ShouldNotChange()
    {
        var sb = new StringBuilder("a;b;c");
        var result = sb.RemoveEndSemicolon();
        result.ToString().Should().Be("a;b;c");
    }

    [Fact]
    public void RemoveEndSemicolon_EmptyBuilder_ShouldNotChange()
    {
        var sb = new StringBuilder();
        var result = sb.RemoveEndSemicolon();
        result.ToString().Should().BeEmpty();
    }

    [Fact]
    public void RemoveEndSemicolon_OnlySemicolon_ShouldEmptyBuilder()
    {
        var sb = new StringBuilder(";");
        var result = sb.RemoveEndSemicolon();
        result.ToString().Should().BeEmpty();
    }

    [Fact]
    public void RemoveEndSemicolon_ShouldReturnSameInstance()
    {
        var sb = new StringBuilder(";");
        var result = sb.RemoveEndSemicolon();
        result.Should().BeSameAs(sb);
    }

    #endregion

    #region RemoveEndWithChar

    [Fact]
    public void RemoveEndWithChar_EndsWithChar_ShouldRemoveChar()
    {
        var sb = new StringBuilder("hello!");
        var result = sb.RemoveEndWithChar('!');
        result.ToString().Should().Be("hello");
    }

    [Fact]
    public void RemoveEndWithChar_DoesNotEndWithChar_ShouldNotChange()
    {
        var sb = new StringBuilder("hello");
        var result = sb.RemoveEndWithChar('!');
        result.ToString().Should().Be("hello");
    }

    [Fact]
    public void RemoveEndWithChar_EmptyBuilder_ShouldNotChange()
    {
        var sb = new StringBuilder();
        var result = sb.RemoveEndWithChar('!');
        result.ToString().Should().BeEmpty();
    }

    [Fact]
    public void RemoveEndWithChar_OnlyTargetChar_ShouldEmptyBuilder()
    {
        var sb = new StringBuilder("!");
        var result = sb.RemoveEndWithChar('!');
        result.ToString().Should().BeEmpty();
    }

    [Fact]
    public void RemoveEndWithChar_MultipleSameCharsAtEnd_ShouldRemoveOnlyLast()
    {
        var sb = new StringBuilder("hello!!!");
        var result = sb.RemoveEndWithChar('!');
        result.ToString().Should().Be("hello!!");
    }

    [Fact]
    public void RemoveEndWithChar_ShouldReturnSameInstance()
    {
        var sb = new StringBuilder("!");
        var result = sb.RemoveEndWithChar('!');
        result.Should().BeSameAs(sb);
    }

    #endregion
}
