using System.Runtime.CompilerServices;
using Azrng.Core.Exceptions;
using Azrng.Core.Extension;
using Azrng.Core.RetryTask;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.RetryTask;

public class ActionRetryTaskTests
{
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

    #region Constructor / Retry basic behavior

    [Fact]
    public async Task Retry_ShouldReturnResult_WhenNoException()
    {
        var task = CreateTask(() => Task.FromResult(42));

        var result = await task.Retry(3);

        result.Should().Be(42);
    }

    [Fact]
    public async Task Retry_ShouldRetryOnRetryMarkException_AndSucceed()
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
    public void Retry_ShouldThrowInternalServerException_WhenExceedsMaxRetry()
    {
        var task = CreateTask<int>(() =>
            Task.FromException<int>(new RetryMarkException("always fail")));

        Func<Task> act = async () => await task.Retry(2);

        act.Should().ThrowAsync<InternalServerException>();
    }

    [Fact]
    public void Retry_ShouldThrowArgumentOutOfRangeException_WhenMaxCountInvalid()
    {
        var task = CreateTask(() => Task.FromResult(1));

        Action act = () => task.Retry(0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region Retry with delay

    [Fact]
    public async Task Retry_WithDelay_ShouldRetryAndSucceed()
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
    public async Task Retry_WithDelay_ShouldApplyDelayBetweenRetries()
    {
        var attempt = 0;
        var startTime = DateTime.UtcNow;

        var task = CreateTask(() => Task.Run(() =>
        {
            if (++attempt < 3)
                throw new RetryMarkException("retry");
            return 42;
        }));

        var result = await task.Retry(3, TimeSpan.FromMilliseconds(50));

        var elapsed = DateTime.UtcNow - startTime;
        result.Should().Be(42);
        attempt.Should().Be(3);
        elapsed.Should().BeGreaterThan(TimeSpan.FromMilliseconds(80));
    }

    #endregion

    #region Retry with delay func

    [Fact]
    public async Task Retry_WithDelayFunc_ShouldRetryAndSucceed()
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
        var task = CreateTask(() => Task.FromResult(42));

        var result = await task.Retry(3, (Func<int, TimeSpan>?)null);

        result.Should().Be(42);
    }

    #endregion

    #region WhenCatch (no params)

    [Fact]
    public async Task WhenCatch_ShouldRetryOnSpecifiedException()
    {
        var attempt = 0;
        var task = CreateTask(() => Task.Run(() =>
        {
            if (++attempt < 3)
                throw new InvalidOperationException("error");
            return 42;
        }));

        var result = await task.Retry(5)
                               .WhenCatch<InvalidOperationException>();

        result.Should().Be(42);
        attempt.Should().Be(3);
    }

    [Fact]
    public async Task WhenCatch_ShouldNotRetry_WhenNoException()
    {
        var task = CreateTask(() => Task.FromResult(100));

        var result = await task.Retry(3)
                               .WhenCatch<Exception>();

        result.Should().Be(100);
    }

    [Fact]
    public void WhenCatch_ShouldThrow_WhenUnmatchedExceptionOccurs()
    {
        var task = CreateTask<int>(() =>
            Task.FromException<int>(new ArgumentException("bad")));

        Func<Task> act = async () => await task.Retry(3)
                                               .WhenCatch<InvalidOperationException>();

        act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region WhenCatch (Action<TException> handler)

    [Fact]
    public async Task WhenCatch_WithHandler_ShouldInvokeHandlerAndRetry()
    {
        var attempt = 0;
        Exception? captured = null;
        var task = CreateTask(() => Task.Run(() =>
        {
            if (++attempt < 2)
                throw new InvalidOperationException("err");
            return 42;
        }));

        var result = await task.Retry(3)
                               .WhenCatch<InvalidOperationException>(ex => captured = ex);

        result.Should().Be(42);
        captured.Should().NotBeNull();
        captured!.Message.Should().Be("err");
    }

    [Fact]
    public async Task WhenCatch_WithNullHandler_ShouldRetry()
    {
        var attempt = 0;
        var task = CreateTask(() => Task.Run(() =>
        {
            if (++attempt < 2)
                throw new InvalidOperationException("err");
            return 42;
        }));

        var result = await task.Retry(3)
                               .WhenCatch<InvalidOperationException>((Action<InvalidOperationException>?)null!);

        result.Should().Be(42);
    }

    #endregion

    #region WhenCatch (Func<TException, bool> predicate)

    [Fact]
    public async Task WhenCatch_WithPredicate_ShouldRetry_WhenPredicateReturnsTrue()
    {
        var attempt = 0;
        var task = CreateTask(() => Task.Run(() =>
        {
            if (++attempt < 3)
                throw new InvalidOperationException("retry");
            return 42;
        }));

        var result = await task.Retry(5)
                               .WhenCatch<InvalidOperationException>(_ => true);

        result.Should().Be(42);
        attempt.Should().Be(3);
    }

    [Fact]
    public void WhenCatch_WithPredicate_ShouldNotRetry_WhenPredicateReturnsFalse()
    {
        var task = CreateTask<int>(() =>
            Task.FromException<int>(new InvalidOperationException("no retry")));

        Func<Task> act = async () => await task.Retry(3)
                                               .WhenCatch<InvalidOperationException>(_ => false);

        act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task WhenCatch_WithNullPredicate_ShouldRetry()
    {
        var attempt = 0;
        var task = CreateTask(() => Task.Run(() =>
        {
            if (++attempt < 2)
                throw new InvalidOperationException("err");
            return 42;
        }));

        var result = await task.Retry(3)
                               .WhenCatch<InvalidOperationException>((Func<InvalidOperationException, bool>?)null!);

        result.Should().Be(42);
    }

    #endregion

    #region WhenCatchAsync (Func<TException, Task> handler)

    [Fact]
    public async Task WhenCatchAsync_WithHandler_ShouldInvokeHandlerAndRetry()
    {
        var attempt = 0;
        var handlerCalled = false;
        var task = CreateTask(() => Task.Run(() =>
        {
            if (++attempt < 2)
                throw new InvalidOperationException("err");
            return 42;
        }));

        var result = await task.Retry(3)
                               .WhenCatchAsync<InvalidOperationException>(ex =>
                               {
                                   handlerCalled = true;
                                   return Task.CompletedTask;
                               });

        result.Should().Be(42);
        handlerCalled.Should().BeTrue();
    }

    [Fact]
    public async Task WhenCatchAsync_WithNullHandler_ShouldRetry()
    {
        var attempt = 0;
        var task = CreateTask(() => Task.Run(() =>
        {
            if (++attempt < 2)
                throw new InvalidOperationException("err");
            return 42;
        }));

        var result = await task.Retry(3)
                               .WhenCatchAsync<InvalidOperationException>((Func<InvalidOperationException, Task>?)null!);

        result.Should().Be(42);
    }

    #endregion

    #region WhenCatchAsync (Func<TException, Task<bool>> predicate)

    [Fact]
    public async Task WhenCatchAsync_WithPredicate_ShouldRetry_WhenPredicateReturnsTrue()
    {
        var attempt = 0;
        var task = CreateTask(() => Task.Run(() =>
        {
            if (++attempt < 3)
                throw new InvalidOperationException("retry");
            return 42;
        }));

        var result = await task.Retry(5)
                               .WhenCatchAsync<InvalidOperationException>(_ => Task.FromResult(true));

        result.Should().Be(42);
        attempt.Should().Be(3);
    }

    [Fact]
    public void WhenCatchAsync_WithPredicate_ShouldNotRetry_WhenPredicateReturnsFalse()
    {
        var task = CreateTask<int>(() =>
            Task.FromException<int>(new InvalidOperationException("no retry")));

        Func<Task> act = async () => await task.Retry(3)
                                               .WhenCatchAsync<InvalidOperationException>(_ => Task.FromResult(false));

        act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task WhenCatchAsync_WithNullPredicate_ShouldRetry()
    {
        var attempt = 0;
        var task = CreateTask(() => Task.Run(() =>
        {
            if (++attempt < 2)
                throw new InvalidOperationException("err");
            return 42;
        }));

        var result = await task.Retry(3)
                               .WhenCatchAsync<InvalidOperationException>((Func<InvalidOperationException, Task<bool>>?)null!);

        result.Should().Be(42);
    }

    #endregion

    #region WhenResult

    [Fact]
    public async Task WhenResult_ShouldNotRetry_WhenResultDoesNotMatchPredicate()
    {
        var task = CreateTask(() => Task.FromResult(42));

        var result = await task.Retry(3)
                               .WhenResult(r => r < 0);

        result.Should().Be(42);
    }

    [Fact]
    public async Task WhenResult_ShouldRetry_WhenResultMatchesPredicate()
    {
        var attempt = 0;
        var task = CreateTask(() => Task.Run(() =>
        {
            return ++attempt < 3 ? -1 : 42;
        }));

        var result = await task.Retry(5)
                               .WhenResult(r => r < 0);

        result.Should().Be(42);
        attempt.Should().Be(3);
    }

    [Fact]
    public void WhenResult_ShouldThrowInternalServerException_WhenAlwaysMatchesAndExceedsRetry()
    {
        var task = CreateTask(() => Task.FromResult(-1));

        Func<Task> act = async () => await task.Retry(2)
                                               .WhenResult(r => r < 0);

        act.Should().ThrowAsync<InternalServerException>();
    }

    [Fact]
    public void WhenResult_ShouldThrowArgumentNullException_WhenPredicateIsNull()
    {
        var task = CreateTask(() => Task.FromResult(42));

        Action act = () => task.Retry(3)
                               .WhenResult(null!);

        act.Should().Throw<ArgumentNullException>()
           .Which.ParamName.Should().Be("predicate");
    }

    #endregion

    #region WhenResultAsync

    [Fact]
    public async Task WhenResultAsync_ShouldNotRetry_WhenResultDoesNotMatchPredicate()
    {
        var task = CreateTask(() => Task.FromResult(42));

        var result = await task.Retry(3)
                               .WhenResultAsync(r => Task.FromResult(r < 0));

        result.Should().Be(42);
    }

    [Fact]
    public async Task WhenResultAsync_ShouldRetry_WhenResultMatchesPredicate()
    {
        var attempt = 0;
        var task = CreateTask(() => Task.Run(() =>
        {
            return ++attempt < 3 ? -1 : 42;
        }));

        var result = await task.Retry(5)
                               .WhenResultAsync(r => Task.FromResult(r < 0));

        result.Should().Be(42);
        attempt.Should().Be(3);
    }

    [Fact]
    public void WhenResultAsync_ShouldThrowInternalServerException_WhenAlwaysMatchesAndExceedsRetry()
    {
        var task = CreateTask(() => Task.FromResult(-1));

        Func<Task> act = async () => await task.Retry(2)
                                               .WhenResultAsync(r => Task.FromResult(r < 0));

        act.Should().ThrowAsync<InternalServerException>();
    }

    [Fact]
    public void WhenResultAsync_ShouldThrowArgumentNullException_WhenPredicateIsNull()
    {
        var task = CreateTask(() => Task.FromResult(42));

        Action act = () => task.Retry(3)
                               .WhenResultAsync(null!);

        act.Should().Throw<ArgumentNullException>()
           .Which.ParamName.Should().Be("predicate");
    }

    #endregion

    #region Non-RetryMarkException should not retry

    [Fact]
    public void Retry_ShouldNotRetry_OnNonRetryMarkException()
    {
        var attempt = 0;
        var task = CreateTask<int>(() =>
        {
            attempt++;
            return Task.FromException<int>(new InvalidOperationException("not a retry exception"));
        });

        Func<Task> act = async () => await task.Retry(3);

        act.Should().ThrowAsync<InvalidOperationException>();
        attempt.Should().Be(1);
    }

    #endregion

    #region Chaining

    [Fact]
    public async Task Chaining_WhenCatch_AfterRetry_ShouldWork()
    {
        var attempt = 0;
        var task = CreateTask(() => Task.Run(() =>
        {
            if (++attempt < 3)
                throw new InvalidOperationException("retry");
            return 42;
        }));

        var result = await task.Retry(5)
                               .WhenCatch<InvalidOperationException>()
                               .WhenCatch<ArgumentException>();

        result.Should().Be(42);
        attempt.Should().Be(3);
    }

    [Fact]
    public async Task Chaining_WhenResult_AfterRetry_ShouldWork()
    {
        var attempt = 0;
        var task = CreateTask(() => Task.Run(() =>
        {
            return ++attempt < 3 ? 0 : 42;
        }));

        var result = await task.Retry(5)
                               .WhenResult(r => r == 0);

        result.Should().Be(42);
        attempt.Should().Be(3);
    }

    #endregion
}
