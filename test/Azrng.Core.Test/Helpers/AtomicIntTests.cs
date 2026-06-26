using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class AtomicIntTests
{
    [Fact]
    public void Constructor_Default_ShouldInitializeToZero()
    {
        var atomic = new AtomicInt();

        atomic.Get().Should().Be(0);
    }

    [Fact]
    public void Constructor_WithValue_ShouldInitializeToGivenValue()
    {
        var atomic = new AtomicInt(42);

        atomic.Get().Should().Be(42);
    }

    [Fact]
    public void Constructor_WithNegativeValue_ShouldInitializeCorrectly()
    {
        var atomic = new AtomicInt(-10);

        atomic.Get().Should().Be(-10);
    }

    [Fact]
    public void Get_ShouldReturnCurrentValue()
    {
        var atomic = new AtomicInt(7);

        atomic.Get().Should().Be(7);
    }

    [Fact]
    public void Increment_ShouldIncreaseValueByOne()
    {
        var atomic = new AtomicInt(0);

        var result = atomic.Increment();

        result.Should().Be(1);
        atomic.Get().Should().Be(1);
    }

    [Fact]
    public void Increment_MultipleTimes_ShouldAccumulate()
    {
        var atomic = new AtomicInt(0);

        atomic.Increment();
        atomic.Increment();
        var result = atomic.Increment();

        result.Should().Be(3);
        atomic.Get().Should().Be(3);
    }

    [Fact]
    public void Increment_FromNegativeValue_ShouldWork()
    {
        var atomic = new AtomicInt(-3);

        var result = atomic.Increment();

        result.Should().Be(-2);
        atomic.Get().Should().Be(-2);
    }

    [Fact]
    public void Decrement_ShouldDecreaseValueByOne()
    {
        var atomic = new AtomicInt(5);

        var result = atomic.Decrement();

        result.Should().Be(4);
        atomic.Get().Should().Be(4);
    }

    [Fact]
    public void Decrement_MultipleTimes_ShouldAccumulate()
    {
        var atomic = new AtomicInt(10);

        atomic.Decrement();
        atomic.Decrement();
        var result = atomic.Decrement();

        result.Should().Be(7);
        atomic.Get().Should().Be(7);
    }

    [Fact]
    public void Decrement_BelowZero_ShouldWork()
    {
        var atomic = new AtomicInt(0);

        var result = atomic.Decrement();

        result.Should().Be(-1);
        atomic.Get().Should().Be(-1);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        var atomic = new AtomicInt(42);

        atomic.Equals(42).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        var atomic = new AtomicInt(42);

        atomic.Equals(99).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithZero_ShouldMatchDefault()
    {
        var atomic = new AtomicInt();

        atomic.Equals(0).Should().BeTrue();
    }

    [Fact]
    public void IncrementAndDecrement_ShouldBeSymmetric()
    {
        var atomic = new AtomicInt(100);

        atomic.Increment();
        atomic.Increment();
        atomic.Decrement();
        atomic.Decrement();

        atomic.Get().Should().Be(100);
    }

    [Fact]
    public async Task ConcurrentIncrement_ShouldProduceCorrectSum()
    {
        var atomic = new AtomicInt(0);
        const int threadCount = 10;
        const int incrementsPerThread = 1000;

        var tasks = Enumerable.Range(0, threadCount).Select(_ => Task.Run(() =>
        {
            for (int i = 0; i < incrementsPerThread; i++)
            {
                atomic.Increment();
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        atomic.Get().Should().Be(threadCount * incrementsPerThread);
    }

    [Fact]
    public async Task ConcurrentDecrement_ShouldProduceCorrectSum()
    {
        const int threadCount = 10;
        const int incrementsPerThread = 1000;
        var atomic = new AtomicInt(threadCount * incrementsPerThread);

        var tasks = Enumerable.Range(0, threadCount).Select(_ => Task.Run(() =>
        {
            for (int i = 0; i < incrementsPerThread; i++)
            {
                atomic.Decrement();
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        atomic.Get().Should().Be(0);
    }
}
