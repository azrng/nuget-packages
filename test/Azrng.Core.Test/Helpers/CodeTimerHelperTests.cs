using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class CodeTimerHelperTests
{
    [Fact]
    public void Time_ActionDelegate_WithEmptyName_ShouldReturnEarly()
    {
        var callCount = 0;

        CodeTimerHelper.Time("", 1, (CodeTimerHelper.ActionDelegate)(() => callCount++));

        callCount.Should().Be(0);
    }

    [Fact]
    public void Time_ActionDelegate_WithNullName_ShouldReturnEarly()
    {
        var callCount = 0;

        CodeTimerHelper.Time(null!, 1, (CodeTimerHelper.ActionDelegate)(() => callCount++));

        callCount.Should().Be(0);
    }

    [Fact]
    public void Time_ActionDelegate_WithNullAction_ShouldReturnEarly()
    {
        Action act = () => CodeTimerHelper.Time("test", 1, (CodeTimerHelper.ActionDelegate?)null!);

        act.Should().NotThrow();
    }

    [Fact]
    public void Time_ActionDelegate_ShouldExecuteActionIterationTimes()
    {
        var callCount = 0;
        var writer = new StringWriter();
        Console.SetOut(writer);

        CodeTimerHelper.Time("test", 5, (CodeTimerHelper.ActionDelegate)(() => callCount++));

        callCount.Should().Be(5);
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
    }

    [Fact]
    public void Time_ActionDelegate_ShouldWriteOutputWithName()
    {
        var writer = new StringWriter();
        Console.SetOut(writer);

        CodeTimerHelper.Time("MyTest", 1, (CodeTimerHelper.ActionDelegate)(() => { }));

        var output = writer.ToString();
        output.Should().Contain("MyTest");
        output.Should().Contain("运行时间");
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
    }

    [Fact]
    public void Time_IAction_WithEmptyName_ShouldReturnEarly()
    {
        var callCount = 0;
        var action = new TestAction(() => callCount++);

        CodeTimerHelper.Time("", 1, action);

        callCount.Should().Be(0);
    }

    [Fact]
    public void Time_IAction_WithNullName_ShouldReturnEarly()
    {
        var callCount = 0;
        var action = new TestAction(() => callCount++);

        CodeTimerHelper.Time(null!, 1, action);

        callCount.Should().Be(0);
    }

    [Fact]
    public void Time_IAction_WithNullAction_ShouldReturnEarly()
    {
        Action act = () => CodeTimerHelper.Time("test", 1, (IAction?)null!);

        act.Should().NotThrow();
    }

    [Fact]
    public void Time_IAction_ShouldExecuteActionIterationTimes()
    {
        var callCount = 0;
        var writer = new StringWriter();
        Console.SetOut(writer);

        CodeTimerHelper.Time("test", 5, new TestAction(() => callCount++));

        callCount.Should().Be(5);
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
    }

    [Fact]
    public void Time_IAction_ShouldWriteOutputWithName()
    {
        var writer = new StringWriter();
        Console.SetOut(writer);

        CodeTimerHelper.Time("IActionTest", 1, new TestAction(() => { }));

        var output = writer.ToString();
        output.Should().Contain("IActionTest");
        output.Should().Contain("运行时间");
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
    }

    private class TestAction : IAction
    {
        private readonly Action _action;

        public TestAction(Action action)
        {
            _action = action;
        }

        public void Action()
        {
            _action();
        }
    }
}
