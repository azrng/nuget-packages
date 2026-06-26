using System.Runtime.CompilerServices;
using Azrng.Core.RetryTask;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.RetryTask;

public class IHandleTaskTests
{
    #region Interface hierarchy

    [Fact]
    public void IHandleTask_ShouldExtendITask()
    {
        typeof(IHandleTask<int>).Should().BeAssignableTo<ITask<int>>();
    }

    [Fact]
    public void IHandleTask_ShouldExtendITask_ForString()
    {
        typeof(IHandleTask<string>).Should().BeAssignableTo<ITask<string>>();
    }

    #endregion

    #region WhenCatch(Func<TResult>) return type

    [Fact]
    public async Task WhenCatch_FuncOverload_ShouldReturnIHandleTask()
    {
        IHandleTask<int> handleTask = new StubHandleTask<int>(Task.FromResult(42));

        var result = handleTask.WhenCatch<Exception>(() => -1);

        result.Should().BeAssignableTo<IHandleTask<int>>();
    }

    [Fact]
    public async Task WhenCatch_FuncOverload_ShouldPreserveResult_WhenNoException()
    {
        IHandleTask<int> handleTask = new StubHandleTask<int>(Task.FromResult(42));

        var result = await handleTask.WhenCatch<Exception>(() => -1);

        result.Should().Be(42);
    }

    [Fact]
    public async Task WhenCatch_FuncOverload_ShouldCatchSpecifiedException()
    {
        IHandleTask<int> handleTask = new StubHandleTask<int>(
            Task.FromException<int>(new InvalidOperationException("err")));

        var result = await handleTask.WhenCatch<InvalidOperationException>(() => -1);

        result.Should().Be(-1);
    }

    [Fact]
    public async Task WhenCatch_FuncOverload_ShouldNotCatchUnmatchedException()
    {
        IHandleTask<int> handleTask = new StubHandleTask<int>(
            Task.FromException<int>(new ArgumentException("bad")));

        Func<Task> act = async () =>
            await handleTask.WhenCatch<InvalidOperationException>(() => -1);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region WhenCatch(Func<TException, TResult>) return type

    [Fact]
    public async Task WhenCatch_ExceptionParamOverload_ShouldReturnIHandleTask()
    {
        IHandleTask<string> handleTask = new StubHandleTask<string>(Task.FromResult("ok"));

        var result = handleTask.WhenCatch<Exception>(ex => ex.Message);

        result.Should().BeAssignableTo<IHandleTask<string>>();
    }

    [Fact]
    public async Task WhenCatch_ExceptionParamOverload_ShouldPassExceptionToFunc()
    {
        IHandleTask<string> handleTask = new StubHandleTask<string>(
            Task.FromException<string>(new ArgumentException("bad arg")));

        var result = await handleTask.WhenCatch<ArgumentException>(ex => ex.Message);

        result.Should().Be("bad arg");
    }

    [Fact]
    public async Task WhenCatch_ExceptionParamOverload_ShouldPreserveResult_WhenNoException()
    {
        IHandleTask<string> handleTask = new StubHandleTask<string>(Task.FromResult("ok"));

        var result = await handleTask.WhenCatch<ArgumentException>(ex => ex.Message);

        result.Should().Be("ok");
    }

    #endregion

    #region WhenCatchAsync return type

    [Fact]
    public async Task WhenCatchAsync_ShouldReturnIHandleTask()
    {
        IHandleTask<int> handleTask = new StubHandleTask<int>(Task.FromResult(1));

        var result = handleTask.WhenCatchAsync<Exception>(ex => Task.FromResult(-1));

        result.Should().BeAssignableTo<IHandleTask<int>>();
    }

    [Fact]
    public async Task WhenCatchAsync_ShouldCatchSpecifiedException()
    {
        IHandleTask<int> handleTask = new StubHandleTask<int>(
            Task.FromException<int>(new InvalidOperationException("async err")));

        var result = await handleTask.WhenCatchAsync<InvalidOperationException>(
            ex => Task.FromResult(99));

        result.Should().Be(99);
    }

    [Fact]
    public async Task WhenCatchAsync_ShouldPreserveResult_WhenNoException()
    {
        IHandleTask<int> handleTask = new StubHandleTask<int>(Task.FromResult(7));

        var result = await handleTask.WhenCatchAsync<Exception>(
            ex => Task.FromResult(-1));

        result.Should().Be(7);
    }

    [Fact]
    public async Task WhenCatchAsync_ShouldNotCatchUnmatchedException()
    {
        IHandleTask<int> handleTask = new StubHandleTask<int>(
            Task.FromException<int>(new ArgumentException("wrong")));

        Func<Task> act = async () =>
            await handleTask.WhenCatchAsync<InvalidOperationException>(
                ex => Task.FromResult(-1));

        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region Chaining

    [Fact]
    public async Task Chaining_MultipleWhenCatch_ShouldWork()
    {
        IHandleTask<string> handleTask = new StubHandleTask<string>(
            Task.FromException<string>(new ArgumentException("arg err")));

        var result = await handleTask
            .WhenCatch<InvalidOperationException>(ex => "invalid")
            .WhenCatch<ArgumentException>(ex => ex.Message);

        result.Should().Be("arg err");
    }

    [Fact]
    public async Task Chaining_WhenCatch_WhenCatchAsync_ShouldWork()
    {
        IHandleTask<int> handleTask = new StubHandleTask<int>(
            Task.FromException<int>(new InvalidOperationException("err")));

        var result = await handleTask
            .WhenCatchAsync<ArgumentException>(ex => Task.FromResult(10))
            .WhenCatch<InvalidOperationException>(() => 20);

        result.Should().Be(20);
    }

    #endregion

    #region ITask members

    [Fact]
    public async Task GetAwaiter_ShouldWork()
    {
        IHandleTask<int> handleTask = new StubHandleTask<int>(Task.FromResult(42));

        var result = await handleTask;

        result.Should().Be(42);
    }

    [Fact]
    public async Task ConfigureAwait_ShouldWork()
    {
        IHandleTask<int> handleTask = new StubHandleTask<int>(Task.FromResult(99));

        var result = await handleTask.ConfigureAwait(false);

        result.Should().Be(99);
    }

    #endregion

    #region Stub

    private sealed class StubHandleTask<TResult> : IHandleTask<TResult>
    {
        private readonly Task<TResult> _task;

        public StubHandleTask(Task<TResult> task) => _task = task;

        public TaskAwaiter<TResult> GetAwaiter() => _task.GetAwaiter();

        public ConfiguredTaskAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext)
            => _task.ConfigureAwait(continueOnCapturedContext);

        public IHandleTask<TResult> WhenCatch<TException>(Func<TResult> func) where TException : Exception
        {
            ArgumentNullException.ThrowIfNull(func);
            return new StubHandleTask<TResult>(CatchAndReturn<TException>(() => Task.FromResult(func())));
        }

        public IHandleTask<TResult> WhenCatch<TException>(Func<TException, TResult> func) where TException : Exception
        {
            ArgumentNullException.ThrowIfNull(func);
            return new StubHandleTask<TResult>(CatchAndReturnWithEx<TException>(ex => Task.FromResult(func(ex))));
        }

        public IHandleTask<TResult> WhenCatchAsync<TException>(Func<TException, Task<TResult>> func) where TException : Exception
        {
            ArgumentNullException.ThrowIfNull(func);
            return new StubHandleTask<TResult>(CatchAndReturnWithEx<TException>(ex => func(ex)));
        }

        private async Task<TResult> CatchAndReturn<TException>(Func<Task<TResult>> fallback) where TException : Exception
        {
            try
            {
                return await _task.ConfigureAwait(false);
            }
            catch (TException)
            {
                return await fallback().ConfigureAwait(false);
            }
        }

        private async Task<TResult> CatchAndReturnWithEx<TException>(Func<TException, Task<TResult>> fallback) where TException : Exception
        {
            try
            {
                return await _task.ConfigureAwait(false);
            }
            catch (TException ex)
            {
                return await fallback(ex).ConfigureAwait(false);
            }
        }
    }

    #endregion
}
