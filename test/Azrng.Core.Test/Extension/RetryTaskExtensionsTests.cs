using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Azrng.Core.Exceptions;
using Azrng.Core.Extension;
using Azrng.Core.RetryTask;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class RetryTaskExtensionsTests
{
    private static ITask<TResult> CreateTask<TResult>(Task<TResult> task)
        => new TaskWrapper<TResult>(() => task);

    private static ITask<TResult> CreateTask<TResult>(Func<Task<TResult>> taskFactory)
        => new TaskWrapper<TResult>(taskFactory);

    private sealed class TaskWrapper<TResult> : ITask<TResult>
    {
        private readonly Func<Task<TResult>> _taskFactory;
        public TaskWrapper(Func<Task<TResult>> taskFactory) => _taskFactory = taskFactory;
        public TaskAwaiter<TResult> GetAwaiter() => _taskFactory().GetAwaiter();
        public ConfiguredTaskAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext)
            => _taskFactory().ConfigureAwait(continueOnCapturedContext);
    }

    #region Retry(ITask, int)

    [Fact]
    public async Task Retry_WithMaxCount_ShouldReturnResult_WhenNoException()
    {
        var task = CreateTask(Task.FromResult(42));

        var result = await task.Retry(3);

        result.Should().Be(42);
    }

    [Fact]
    public async Task Retry_WithMaxCount_ShouldRetryOnRetryMarkException()
    {
        var attempt = 0;
        var task = CreateTask(() => Task.Run(() =>
        {
            if (++attempt < 3)
                throw new RetryMarkException("retry");
            return 99;
        }));

        var result = await task.Retry(5);

        result.Should().Be(99);
        attempt.Should().Be(3);
    }

    [Fact]
    public async Task Retry_WithMaxCount_ShouldThrow_WhenExceedsMaxRetry()
    {
        var task = CreateTask<int>(() =>
            Task.FromException<int>(new RetryMarkException("always fail")));

        Func<Task> act = async () => await task.Retry(2);

        await act.Should().ThrowAsync<InternalServerException>();
    }

    [Fact]
    public void Retry_WithMaxCount_ShouldThrow_WhenTaskIsNull()
    {
        ITask<int> task = null!;

        Action act = () => task.Retry(3);

        act.Should().Throw<ArgumentNullException>()
           .Which.ParamName.Should().Be("task");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Retry_WithMaxCount_ShouldThrow_WhenMaxCountInvalid(int maxCount)
    {
        var task = CreateTask(Task.FromResult(1));

        Action act = () => task.Retry(maxCount);

        act.Should().Throw<ArgumentOutOfRangeException>()
           .Which.ParamName.Should().Be("maxCount");
    }

    #endregion

    #region Retry(ITask, int, TimeSpan)

    [Fact]
    public async Task Retry_WithDelay_ShouldReturnResult_WhenNoException()
    {
        var task = CreateTask(Task.FromResult("hello"));

        var result = await task.Retry(3, TimeSpan.FromMilliseconds(1));

        result.Should().Be("hello");
    }

    [Fact]
    public async Task Retry_WithDelay_ShouldRetryOnRetryMarkException()
    {
        var attempt = 0;
        var task = CreateTask(() => Task.Run(() =>
        {
            if (++attempt < 2)
                throw new RetryMarkException("retry");
            return "done";
        }));

        var result = await task.Retry(3, TimeSpan.FromMilliseconds(1));

        result.Should().Be("done");
        attempt.Should().Be(2);
    }

    [Fact]
    public void Retry_WithDelay_ShouldThrow_WhenTaskIsNull()
    {
        ITask<int> task = null!;

        Action act = () => task.Retry(3, TimeSpan.FromSeconds(1));

        act.Should().Throw<ArgumentNullException>()
           .Which.ParamName.Should().Be("task");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Retry_WithDelay_ShouldThrow_WhenMaxCountInvalid(int maxCount)
    {
        var task = CreateTask(Task.FromResult(1));

        Action act = () => task.Retry(maxCount, TimeSpan.FromSeconds(1));

        act.Should().Throw<ArgumentOutOfRangeException>()
           .Which.ParamName.Should().Be("maxCount");
    }

    #endregion

    #region Retry(ITask, int, Func<int, TimeSpan>?)

    [Fact]
    public async Task Retry_WithDelayFunc_ShouldReturnResult_WhenNoException()
    {
        var task = CreateTask(Task.FromResult(123));

        var result = await task.Retry(3, _ => TimeSpan.FromMilliseconds(1));

        result.Should().Be(123);
    }

    [Fact]
    public async Task Retry_WithDelayFunc_ShouldRetryOnRetryMarkException()
    {
        var attempt = 0;
        var task = CreateTask(() => Task.Run(() =>
        {
            if (++attempt < 3)
                throw new RetryMarkException("retry");
            return 77;
        }));

        var result = await task.Retry(5, _ => TimeSpan.FromMilliseconds(1));

        result.Should().Be(77);
        attempt.Should().Be(3);
    }

    [Fact]
    public async Task Retry_WithNullDelayFunc_ShouldReturnResult()
    {
        var task = CreateTask(Task.FromResult(42));

        var result = await task.Retry(3, (Func<int, TimeSpan>?)null);

        result.Should().Be(42);
    }

    [Fact]
    public void Retry_WithDelayFunc_ShouldThrow_WhenTaskIsNull()
    {
        ITask<int> task = null!;

        Action act = () => task.Retry(3, (Func<int, TimeSpan>?)null);

        act.Should().Throw<ArgumentNullException>()
           .Which.ParamName.Should().Be("task");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Retry_WithDelayFunc_ShouldThrow_WhenMaxCountInvalid(int maxCount)
    {
        var task = CreateTask(Task.FromResult(1));

        Action act = () => task.Retry(maxCount, (Func<int, TimeSpan>?)null);

        act.Should().Throw<ArgumentOutOfRangeException>()
           .Which.ParamName.Should().Be("maxCount");
    }

    #endregion

    #region HandleAsDefaultWhenException

    [Fact]
    public async Task HandleAsDefaultWhenException_ShouldReturnResult_WhenNoException()
    {
        var task = Task.FromResult(42);

        var result = await task.HandleAsDefaultWhenException();

        result.Should().Be(42);
    }

    [Fact]
    public async Task HandleAsDefaultWhenException_ShouldReturnDefault_WhenExceptionThrown()
    {
        var task = CreateFailingTask<int>(new InvalidOperationException("error"));

        var result = await task.HandleAsDefaultWhenException();

        result.Should().Be(default);
    }

    [Fact]
    public async Task HandleAsDefaultWhenException_ShouldReturnDefault_WhenReferenceType()
    {
        var task = CreateFailingTask<string>(new InvalidOperationException("error"));

        var result = await task.HandleAsDefaultWhenException();

        result.Should().BeNull();
    }

    #endregion

    #region Handle

    [Fact]
    public async Task Handle_ShouldReturnResult_WhenNoException()
    {
        var task = Task.FromResult(100);

        var result = await task.Handle();

        result.Should().Be(100);
    }

    [Fact]
    public void Handle_ShouldThrow_WhenTaskIsNull()
    {
        Task<int> task = null!;

        Action act = () => task.Handle();

        act.Should().Throw<ArgumentNullException>()
           .Which.ParamName.Should().Be("task");
    }

    [Fact]
    public async Task Handle_ShouldAllowChaining_WhenCatch()
    {
        var task = CreateFailingTask<int>(new InvalidOperationException("error"));

        var result = await task.Handle()
                               .WhenCatch<Exception>(() => -1);

        result.Should().Be(-1);
    }

    [Fact]
    public async Task Handle_ShouldNotCatch_WhenNoException()
    {
        var task = Task.FromResult(99);

        var result = await task.Handle()
                               .WhenCatch<Exception>(() => -1);

        result.Should().Be(99);
    }

    [Fact]
    public async Task Handle_ShouldCatchSpecificException()
    {
        var task = CreateFailingTask<string>(new ArgumentException("bad arg"));

        var result = await task.Handle()
                               .WhenCatch<ArgumentException>(ex => ex.Message);

        result.Should().Be("bad arg");
    }

    [Fact]
    public async Task Handle_ShouldNotCatchUnmatchedException()
    {
        var task = CreateFailingTask<string>(new InvalidOperationException("not arg"));

        Func<Task> act = async () => await task.Handle()
                                               .WhenCatch<ArgumentException>(ex => ex.Message);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region Helpers

    private static Task<TResult> CreateFailingTask<TResult>(Exception ex)
    {
        return Task.FromException<TResult>(ex);
    }

    #endregion
}
