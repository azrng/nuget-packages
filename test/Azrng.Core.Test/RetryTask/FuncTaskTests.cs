using Azrng.Core.RetryTask;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.RetryTask;

public class FuncTaskTests
{
    #region GetAwaiter

    [Fact]
    public async Task GetAwaiter_ShouldReturnResultFromFunc()
    {
        var funcTask = new FuncTask<int>(() => Task.FromResult(42));

        var result = await funcTask;

        result.Should().Be(42);
    }

    [Fact]
    public async Task GetAwaiter_ShouldReturnStringResult()
    {
        var funcTask = new FuncTask<string>(() => Task.FromResult("hello"));

        var result = await funcTask;

        result.Should().Be("hello");
    }

    [Fact]
    public void GetAwaiter_ShouldPropagateException()
    {
        var funcTask = new FuncTask<int>(() => Task.FromException<int>(new InvalidOperationException("fail")));

        Func<Task> act = async () => await funcTask;

        act.Should().ThrowAsync<InvalidOperationException>()
           .WithMessage("fail");
    }

    [Fact]
    public async Task GetAwaiter_ShouldInvokeFuncOnEachAwait()
    {
        var invokeCount = 0;
        var funcTask = new FuncTask<int>(() =>
        {
            invokeCount++;
            return Task.FromResult(invokeCount);
        });

        var r1 = await funcTask;
        var r2 = await funcTask;

        r1.Should().Be(1);
        r2.Should().Be(2);
        invokeCount.Should().Be(2);
    }

    #endregion

    #region ConfigureAwait

    [Fact]
    public async Task ConfigureAwait_ShouldReturnResultFromFunc()
    {
        var funcTask = new FuncTask<int>(() => Task.FromResult(99));

        var result = await funcTask.ConfigureAwait(false);

        result.Should().Be(99);
    }

    [Fact]
    public async Task ConfigureAwait_WithTrue_ShouldReturnResult()
    {
        var funcTask = new FuncTask<string>(() => Task.FromResult("ok"));

        var result = await funcTask.ConfigureAwait(true);

        result.Should().Be("ok");
    }

    [Fact]
    public void ConfigureAwait_ShouldPropagateException()
    {
        var funcTask = new FuncTask<int>(() =>
            Task.FromException<int>(new ArgumentException("bad")));

        Func<Task> act = async () => await funcTask.ConfigureAwait(false);

        act.Should().ThrowAsync<ArgumentException>()
           .WithMessage("bad");
    }

    [Fact]
    public async Task ConfigureAwait_ShouldInvokeFuncOnEachCall()
    {
        var invokeCount = 0;
        var funcTask = new FuncTask<int>(() =>
        {
            invokeCount++;
            return Task.FromResult(invokeCount);
        });

        var r1 = await funcTask.ConfigureAwait(false);
        var r2 = await funcTask.ConfigureAwait(true);

        r1.Should().Be(1);
        r2.Should().Be(2);
        invokeCount.Should().Be(2);
    }

    #endregion

    #region ITask interface conformance

    [Fact]
    public void FuncTask_ShouldImplementITask()
    {
        var funcTask = new FuncTask<int>(() => Task.FromResult(1));

        funcTask.Should().BeAssignableTo<ITask<int>>();
    }

    #endregion
}
