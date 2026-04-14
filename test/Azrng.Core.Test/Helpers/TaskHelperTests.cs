using Azrng.Core.Helpers;
using Azrng.Core.Results;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class TaskHelperTests
{
    [Fact]
    public async Task RunTimeLimitAsync_ShouldReturnResult_WhenCompletedInTime()
    {
        var result = await TaskHelper.RunTimeLimitAsync(async () =>
        {
            await Task.Delay(10);
            return 42;
        }, TimeSpan.FromSeconds(1));

        result.Should().Be(42);
    }

    [Fact]
    public async Task RunTimeLimitAsync_ShouldThrowTimeoutException_WhenOperationExceedsLimit()
    {
        Func<Task<int>> action = () => TaskHelper.RunTimeLimitAsync(async () =>
        {
            await Task.Delay(200);
            return 42;
        }, TimeSpan.FromMilliseconds(20));

        await action.Should().ThrowAsync<TimeoutException>();
    }

    [Fact]
    public async Task TryWaitAsync_ShouldReturnSuccessBeforeTimeout()
    {
        var count = 0;

        var result = await TaskHelper.TryWaitAsync(async () =>
        {
            await Task.Yield();
            count++;
            return count >= 2
                ? ResultModel<int>.Success(99)
                : ResultModel<int>.Error("pending");
        }, TimeSpan.FromSeconds(3));

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(99);
    }

    [Fact]
    public async Task ExecuteFuncWithRetryAsync_ShouldRetryUntilSuccess()
    {
        var count = 0;

        var result = await TaskHelper.ExecuteFuncWithRetryAsync(async () =>
        {
            await Task.Yield();
            count++;
            if (count < 3)
            {
                throw new InvalidOperationException("not yet");
            }

            return 7;
        }, maxAttempts: 3, delayInMilliseconds: 1);

        result.Should().Be(7);
        count.Should().Be(3);
    }

    [Fact]
    public void ExecuteFuncWithRetry_ShouldRetryUntilSuccess()
    {
        var count = 0;

        var result = TaskHelper.ExecuteFuncWithRetry(() =>
        {
            count++;
            if (count < 2)
            {
                throw new InvalidOperationException("not yet");
            }

            return 5;
        }, maxAttempts: 2, delayInMilliseconds: 1);

        result.Should().Be(5);
        count.Should().Be(2);
    }

    [Fact]
    public void ExecuteActionWithRetry_ShouldEventuallySucceed()
    {
        var count = 0;

        TaskHelper.ExecuteActionWithRetry(() =>
        {
            count++;
            if (count < 2)
            {
                throw new InvalidOperationException("not yet");
            }
        }, maxAttempts: 2, delayInMilliseconds: 1);

        count.Should().Be(2);
    }
}
