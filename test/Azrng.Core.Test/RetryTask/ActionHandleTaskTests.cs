using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.RetryTask;

public class ActionHandleTaskTests
{
    #region WhenCatch(Func<TResult>)

    [Fact]
    public async Task WhenCatch_NoException_ReturnsOriginalResult()
    {
        var task = Task.FromResult(42);

        var result = await task.Handle()
                               .WhenCatch<Exception>(() => -1);

        result.Should().Be(42);
    }

    [Fact]
    public async Task WhenCatch_CatchesSpecifiedException()
    {
        var task = CreateFailingTask<int>(new InvalidOperationException("error"));

        var result = await task.Handle()
                               .WhenCatch<InvalidOperationException>(() => -1);

        result.Should().Be(-1);
    }

    [Fact]
    public async Task WhenCatch_DoesNotCatchUnmatchedException()
    {
        var task = CreateFailingTask<int>(new ArgumentException("bad"));

        Func<Task> act = async () => await task.Handle()
                                                .WhenCatch<InvalidOperationException>(() => -1);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void WhenCatch_FuncOverload_ThrowsOnNullFunc()
    {
        var task = Task.FromResult(1);

        Action act = () => task.Handle()
                               .WhenCatch<Exception>((Func<int>)null!);

        act.Should().Throw<ArgumentNullException>()
           .Which.ParamName.Should().Be("func");
    }

    #endregion

    #region WhenCatch(Func<TException, TResult>)

    [Fact]
    public async Task WhenCatch_WithExceptionParam_ReturnsExceptionBasedResult()
    {
        var task = CreateFailingTask<string>(new ArgumentException("bad arg"));

        var result = await task.Handle()
                               .WhenCatch<ArgumentException>(ex => ex.Message);

        result.Should().Be("bad arg");
    }

    [Fact]
    public async Task WhenCatch_WithExceptionParam_NoException_ReturnsOriginalResult()
    {
        var task = Task.FromResult("ok");

        var result = await task.Handle()
                               .WhenCatch<ArgumentException>(ex => ex.Message);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task WhenCatch_WithExceptionParam_DoesNotCatchUnmatchedException()
    {
        var task = CreateFailingTask<string>(new InvalidOperationException("not arg"));

        Func<Task> act = async () => await task.Handle()
                                                .WhenCatch<ArgumentException>(ex => ex.Message);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void WhenCatch_ExceptionParamOverload_ThrowsOnNullFunc()
    {
        var task = Task.FromResult(1);

        Action act = () => task.Handle()
                               .WhenCatch<Exception>((Func<Exception, int>)null!);

        act.Should().Throw<ArgumentNullException>()
           .Which.ParamName.Should().Be("func");
    }

    #endregion

    #region WhenCatchAsync

    [Fact]
    public async Task WhenCatchAsync_CatchesSpecifiedException()
    {
        var task = CreateFailingTask<int>(new InvalidOperationException("async error"));

        var result = await task.Handle()
                               .WhenCatchAsync<InvalidOperationException>(ex => Task.FromResult(99));

        result.Should().Be(99);
    }

    [Fact]
    public async Task WhenCatchAsync_NoException_ReturnsOriginalResult()
    {
        var task = Task.FromResult(7);

        var result = await task.Handle()
                               .WhenCatchAsync<Exception>(ex => Task.FromResult(-1));

        result.Should().Be(7);
    }

    [Fact]
    public async Task WhenCatchAsync_DoesNotCatchUnmatchedException()
    {
        var task = CreateFailingTask<int>(new ArgumentException("wrong type"));

        Func<Task> act = async () => await task.Handle()
                                                .WhenCatchAsync<InvalidOperationException>(
                                                    ex => Task.FromResult(-1));

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void WhenCatchAsync_ThrowsOnNullFunc()
    {
        var task = Task.FromResult(1);

        Action act = () => task.Handle()
                               .WhenCatchAsync<Exception>((Func<Exception, Task<int>>)null!);

        act.Should().Throw<ArgumentNullException>()
           .Which.ParamName.Should().Be("func");
    }

    [Fact]
    public async Task WhenCatchAsync_UsesAsyncHandler()
    {
        var task = CreateFailingTask<string>(new InvalidOperationException("boom"));

        var result = await task.Handle()
                               .WhenCatchAsync<InvalidOperationException>(
                                   ex => Task.FromResult($"recovered: {ex.Message}"));

        result.Should().Be("recovered: boom");
    }

    #endregion

    #region Chaining

    [Fact]
    public async Task Chaining_MultipleWhenCatch_CatchesMatchingException()
    {
        var task = CreateFailingTask<string>(new ArgumentException("arg err"));

        var result = await task.Handle()
                               .WhenCatch<InvalidOperationException>(ex => "invalid")
                               .WhenCatch<ArgumentException>(ex => ex.Message);

        result.Should().Be("arg err");
    }

    [Fact]
    public async Task Chaining_MultipleWhenCatch_FirstMatchWins()
    {
        var task = CreateFailingTask<int>(new InvalidOperationException("err"));

        var result = await task.Handle()
                               .WhenCatch<InvalidOperationException>(() => 10)
                               .WhenCatch<Exception>(() => 20);

        result.Should().Be(10);
    }

    #endregion

    #region Helpers

    private static Task<TResult> CreateFailingTask<TResult>(Exception ex)
    {
        return Task.FromException<TResult>(ex);
    }

    #endregion
}
