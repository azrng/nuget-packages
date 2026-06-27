using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class ConsoleHelperTests
{
    private readonly StringWriter _stringWriter = new();

    public ConsoleHelperTests()
    {
        Console.SetOut(_stringWriter);
    }

    [Fact]
    public void WriteErrorLine_DefaultColor_DoesNotThrow()
    {
        var act = () => ConsoleHelper.WriteErrorLine("error message");

        act.Should().NotThrow();
    }

    [Fact]
    public void WriteErrorLine_CustomColor_DoesNotThrow()
    {
        var act = () => ConsoleHelper.WriteErrorLine("error message", ConsoleColor.DarkRed);

        act.Should().NotThrow();
    }

    [Fact]
    public void WriteErrorLine_DefaultColor_OutputsCorrectText()
    {
        ConsoleHelper.WriteErrorLine("test error");

        _stringWriter.ToString().Should().Contain("test error");
    }

    [Fact]
    public void WriteWarningLine_DefaultColor_DoesNotThrow()
    {
        var act = () => ConsoleHelper.WriteWarningLine("warning message");

        act.Should().NotThrow();
    }

    [Fact]
    public void WriteWarningLine_CustomColor_DoesNotThrow()
    {
        var act = () => ConsoleHelper.WriteWarningLine("warning message", ConsoleColor.DarkYellow);

        act.Should().NotThrow();
    }

    [Fact]
    public void WriteWarningLine_DefaultColor_OutputsCorrectText()
    {
        ConsoleHelper.WriteWarningLine("test warning");

        _stringWriter.ToString().Should().Contain("test warning");
    }

    [Fact]
    public void WriteInfoLine_DefaultColor_DoesNotThrow()
    {
        var act = () => ConsoleHelper.WriteInfoLine("info message");

        act.Should().NotThrow();
    }

    [Fact]
    public void WriteInfoLine_CustomColor_DoesNotThrow()
    {
        var act = () => ConsoleHelper.WriteInfoLine("info message", ConsoleColor.Cyan);

        act.Should().NotThrow();
    }

    [Fact]
    public void WriteInfoLine_DefaultColor_OutputsCorrectText()
    {
        ConsoleHelper.WriteInfoLine("test info");

        _stringWriter.ToString().Should().Contain("test info");
    }

    [Fact]
    public void WriteSuccessLine_DefaultColor_DoesNotThrow()
    {
        var act = () => ConsoleHelper.WriteSuccessLine("success message");

        act.Should().NotThrow();
    }

    [Fact]
    public void WriteSuccessLine_CustomColor_DoesNotThrow()
    {
        var act = () => ConsoleHelper.WriteSuccessLine("success message", ConsoleColor.DarkGreen);

        act.Should().NotThrow();
    }

    [Fact]
    public void WriteSuccessLine_DefaultColor_OutputsCorrectText()
    {
        ConsoleHelper.WriteSuccessLine("test success");

        _stringWriter.ToString().Should().Contain("test success");
    }

    [Fact]
    public void ReadLineWithPrompt_WithPrompt_OutputsPromptAndReturnsInput()
    {
        Console.SetIn(new StringReader("hello\n"));

        var result = ConsoleHelper.ReadLineWithPrompt("Enter something");

        result.Should().Be("hello");
        _stringWriter.ToString().Should().Contain("Enter something");
    }

    [Fact]
    public void ReadLineWithPrompt_NullPrompt_SkipsPromptAndReturnsInput()
    {
        Console.SetIn(new StringReader("world\n"));

        var result = ConsoleHelper.ReadLineWithPrompt(null);

        result.Should().Be("world");
        _stringWriter.ToString().Should().BeEmpty();
    }

    [Fact]
    public void ReadLineWithPrompt_DefaultPrompt_OutputsDefaultAndReturnsInput()
    {
        Console.SetIn(new StringReader("test\n"));

        var result = ConsoleHelper.ReadLineWithPrompt();

        result.Should().Be("test");
        _stringWriter.ToString().Should().Contain("Press Enter to continue");
    }

    [Fact]
    public void ReadKeyWithPrompt_WithPrompt_ThrowsWhenInputRedirected()
    {
        Console.SetIn(new StringReader("a"));

        var act = () => ConsoleHelper.ReadKeyWithPrompt("Press a key");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ReadKeyWithPrompt_NullPrompt_ThrowsWhenInputRedirected()
    {
        Console.SetIn(new StringReader("b"));

        var act = () => ConsoleHelper.ReadKeyWithPrompt(null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ReadKeyWithPrompt_DefaultPrompt_ThrowsWhenInputRedirected()
    {
        Console.SetIn(new StringReader("x"));

        var act = () => ConsoleHelper.ReadKeyWithPrompt();

        act.Should().Throw<InvalidOperationException>();
    }
}
