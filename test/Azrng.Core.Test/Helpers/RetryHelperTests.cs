using Azrng.Core.Exceptions;
using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class RetryHelperTests
{
    [Fact]
    public async Task ExecuteAsync_WithSuccessfulOperation_ShouldReturnResult()
    {
        var result = await RetryHelper.ExecuteAsync(() => Task.FromResult(42), 3);

        result.Should().Be(42);
    }

    [Fact]
    public async Task ExecuteAsync_WithSuccessfulStringOperation_ShouldReturnResult()
    {
        var result = await RetryHelper.ExecuteAsync(() => Task.FromResult("hello"), 3);

        result.Should().Be("hello");
    }

    [Fact]
    public void ExecuteAsync_WithNullOperation_ShouldThrowArgumentNullException()
    {
        Action act = () => RetryHelper.ExecuteAsync<int>(null!, 3).GetAwaiter().GetResult();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExecuteAsync_WithZeroMaxRetryCount_ShouldThrowArgumentOutOfRangeException()
    {
        Action act = () => RetryHelper.ExecuteAsync(() => Task.FromResult(1), 0).GetAwaiter().GetResult();

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ExecuteAsync_WithNegativeMaxRetryCount_ShouldThrowArgumentOutOfRangeException()
    {
        Action act = () => RetryHelper.ExecuteAsync(() => Task.FromResult(1), -1).GetAwaiter().GetResult();

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithRetryMarkException_ShouldRetryAndSucceed()
    {
        var attemptCount = 0;

        var result = await RetryHelper.ExecuteAsync(async () =>
        {
            attemptCount++;
            if (attemptCount < 3)
                throw new RetryMarkException();
            return 42;
        }, 3);

        result.Should().Be(42);
        attemptCount.Should().Be(3);
    }

    [Fact]
    public void ExecuteAsync_WithRetryMarkException_ShouldRetryAndFailAfterMaxRetries()
    {
        var attemptCount = 0;

        Action act = () => RetryHelper.ExecuteAsync<int>(async () =>
        {
            attemptCount++;
            throw new RetryMarkException();
        }, 3).GetAwaiter().GetResult();

        act.Should().Throw<InternalServerException>();
        attemptCount.Should().Be(4);
    }

    [Fact]
    public void ExecuteAsync_WithNonRetryMarkException_ShouldNotRetry()
    {
        var attemptCount = 0;

        Action act = () => RetryHelper.ExecuteAsync<int>(async () =>
        {
            attemptCount++;
            throw new InvalidOperationException("test error");
        }, 3).GetAwaiter().GetResult();

        act.Should().Throw<InvalidOperationException>();
        attemptCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithFixedDelay_ShouldReturnResult()
    {
        var result = await RetryHelper.ExecuteAsync(
            () => Task.FromResult(42),
            3,
            TimeSpan.FromMilliseconds(10));

        result.Should().Be(42);
    }

    [Fact]
    public void ExecuteAsync_WithFixedDelay_AndNullOperation_ShouldThrowArgumentNullException()
    {
        Action act = () => RetryHelper.ExecuteAsync<int>(
            null!,
            3,
            TimeSpan.FromMilliseconds(10)).GetAwaiter().GetResult();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExecuteAsync_WithFixedDelay_AndZeroMaxRetryCount_ShouldThrowArgumentOutOfRangeException()
    {
        Action act = () => RetryHelper.ExecuteAsync(
            () => Task.FromResult(1),
            0,
            TimeSpan.FromMilliseconds(10)).GetAwaiter().GetResult();

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithFixedDelay_AndRetryMarkException_ShouldRetryAndSucceed()
    {
        var attemptCount = 0;

        var result = await RetryHelper.ExecuteAsync(async () =>
        {
            attemptCount++;
            if (attemptCount < 3)
                throw new RetryMarkException();
            return 42;
        }, 3, TimeSpan.FromMilliseconds(10));

        result.Should().Be(42);
        attemptCount.Should().Be(3);
    }

    [Fact]
    public void ExecuteAsync_WithFixedDelay_AndRetryMarkException_ShouldRetryAndFailAfterMaxRetries()
    {
        var attemptCount = 0;

        Action act = () => RetryHelper.ExecuteAsync<int>(async () =>
        {
            attemptCount++;
            throw new RetryMarkException();
        }, 3, TimeSpan.FromMilliseconds(10)).GetAwaiter().GetResult();

        act.Should().Throw<InternalServerException>();
        attemptCount.Should().Be(4);
    }

    [Fact]
    public async Task ExecuteAsync_WithDelayStrategy_ShouldReturnResult()
    {
        var result = await RetryHelper.ExecuteAsync(
            () => Task.FromResult(42),
            3,
            i => TimeSpan.FromMilliseconds(10));

        result.Should().Be(42);
    }

    [Fact]
    public void ExecuteAsync_WithDelayStrategy_AndNullOperation_ShouldThrowArgumentNullException()
    {
        Action act = () => RetryHelper.ExecuteAsync<int>(
            null!,
            3,
            i => TimeSpan.FromMilliseconds(10)).GetAwaiter().GetResult();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExecuteAsync_WithDelayStrategy_AndZeroMaxRetryCount_ShouldThrowArgumentOutOfRangeException()
    {
        Action act = () => RetryHelper.ExecuteAsync(
            () => Task.FromResult(1),
            0,
            i => TimeSpan.FromMilliseconds(10)).GetAwaiter().GetResult();

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithDelayStrategy_AndRetryMarkException_ShouldRetryAndSucceed()
    {
        var attemptCount = 0;

        var result = await RetryHelper.ExecuteAsync(async () =>
        {
            attemptCount++;
            if (attemptCount < 3)
                throw new RetryMarkException();
            return 42;
        }, 3, i => TimeSpan.FromMilliseconds(10));

        result.Should().Be(42);
        attemptCount.Should().Be(3);
    }

    [Fact]
    public void ExecuteAsync_WithDelayStrategy_AndRetryMarkException_ShouldRetryAndFailAfterMaxRetries()
    {
        var attemptCount = 0;

        Action act = () => RetryHelper.ExecuteAsync<int>(async () =>
        {
            attemptCount++;
            throw new RetryMarkException();
        }, 3, i => TimeSpan.FromMilliseconds(10)).GetAwaiter().GetResult();

        act.Should().Throw<InternalServerException>();
        attemptCount.Should().Be(4);
    }

    [Fact]
    public async Task ExecuteAsync_WithDelayStrategy_ShouldUseExponentialBackoff()
    {
        var attemptCount = 0;
        var delays = new List<TimeSpan>();

        var result = await RetryHelper.ExecuteAsync(async () =>
        {
            attemptCount++;
            if (attemptCount < 3)
                throw new RetryMarkException();
            return 42;
        }, 3, i =>
        {
            var delay = TimeSpan.FromMilliseconds(Math.Pow(2, i));
            delays.Add(delay);
            return delay;
        });

        result.Should().Be(42);
        attemptCount.Should().Be(3);
        delays.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_WithFixedDelay_ShouldApplyDelayBetweenRetries()
    {
        var attemptCount = 0;
        var startTime = DateTime.UtcNow;

        var result = await RetryHelper.ExecuteAsync(async () =>
        {
            attemptCount++;
            if (attemptCount < 3)
                throw new RetryMarkException();
            return 42;
        }, 3, TimeSpan.FromMilliseconds(50));

        var elapsed = DateTime.UtcNow - startTime;
        result.Should().Be(42);
        attemptCount.Should().Be(3);
        elapsed.Should().BeGreaterThan(TimeSpan.FromMilliseconds(80));
    }
}
