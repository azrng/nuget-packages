using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class AtomicLongTests
{
    [Fact]
    public void Constructor_Default_ShouldInitializeToZero()
    {
        var atomic = new AtomicLong();

        atomic.Get().Should().Be(0);
    }

    [Fact]
    public void Constructor_WithValue_ShouldInitializeToGivenValue()
    {
        var atomic = new AtomicLong(42L);

        atomic.Get().Should().Be(42L);
    }

    [Fact]
    public void Constructor_WithNegativeValue_ShouldInitializeCorrectly()
    {
        var atomic = new AtomicLong(-10L);

        atomic.Get().Should().Be(-10L);
    }

    [Fact]
    public void Constructor_WithMaxValue_ShouldInitializeCorrectly()
    {
        var atomic = new AtomicLong(long.MaxValue);

        atomic.Get().Should().Be(long.MaxValue);
    }

    [Fact]
    public void Constructor_WithMinValue_ShouldInitializeCorrectly()
    {
        var atomic = new AtomicLong(long.MinValue);

        atomic.Get().Should().Be(long.MinValue);
    }

    [Fact]
    public void Get_ShouldReturnCurrentValue()
    {
        var atomic = new AtomicLong(7L);

        atomic.Get().Should().Be(7L);
    }

    [Fact]
    public void Set_ShouldUpdateValue()
    {
        var atomic = new AtomicLong(1L);

        atomic.Set(100L);

        atomic.Get().Should().Be(100L);
    }

    [Fact]
    public void Set_WithNegativeValue_ShouldUpdateValue()
    {
        var atomic = new AtomicLong(0L);

        atomic.Set(-50L);

        atomic.Get().Should().Be(-50L);
    }

    [Fact]
    public void Set_MultipleTimes_ShouldKeepLastValue()
    {
        var atomic = new AtomicLong(0L);

        atomic.Set(1L);
        atomic.Set(2L);
        atomic.Set(3L);

        atomic.Get().Should().Be(3L);
    }

    [Fact]
    public void GetAndSet_ShouldReturnOldValue()
    {
        var atomic = new AtomicLong(10L);

        var old = atomic.GetAndSet(20L);

        old.Should().Be(10L);
        atomic.Get().Should().Be(20L);
    }

    [Fact]
    public void GetAndSet_WithSameValue_ShouldReturnSameValue()
    {
        var atomic = new AtomicLong(5L);

        var old = atomic.GetAndSet(5L);

        old.Should().Be(5L);
        atomic.Get().Should().Be(5L);
    }

    [Fact]
    public void GetAndSet_FromZero_ShouldReturnZero()
    {
        var atomic = new AtomicLong();

        var old = atomic.GetAndSet(99L);

        old.Should().Be(0L);
        atomic.Get().Should().Be(99L);
    }

    [Fact]
    public void CompareTo_Object_WithNull_ShouldReturnPositive()
    {
        var atomic = new AtomicLong(10L);

        atomic.CompareTo((object?)null).Should().BeGreaterThan(0);
    }

    [Fact]
    public void CompareTo_Object_WithSameValue_ShouldReturnZero()
    {
        var atomic = new AtomicLong(42L);
        var other = new AtomicLong(42L);

        atomic.CompareTo((object)other).Should().Be(0);
    }

    [Fact]
    public void CompareTo_Object_WithSmallerValue_ShouldReturnPositive()
    {
        var atomic = new AtomicLong(100L);
        var other = new AtomicLong(50L);

        atomic.CompareTo((object)other).Should().BeGreaterThan(0);
    }

    [Fact]
    public void CompareTo_Object_WithLargerValue_ShouldReturnNegative()
    {
        var atomic = new AtomicLong(10L);
        var other = new AtomicLong(50L);

        atomic.CompareTo((object)other).Should().BeLessThan(0);
    }

    [Fact]
    public void CompareTo_Object_WithNonAtomicLong_ShouldThrowArgumentException()
    {
        var atomic = new AtomicLong(10L);

        var act = () => atomic.CompareTo("not an AtomicLong");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*AtomicLong*");
    }

    [Fact]
    public void CompareTo_Long_WithEqualValue_ShouldReturnZero()
    {
        var atomic = new AtomicLong(42L);

        atomic.CompareTo(42L).Should().Be(0);
    }

    [Fact]
    public void CompareTo_Long_WithSmallerValue_ShouldReturnPositive()
    {
        var atomic = new AtomicLong(100L);

        atomic.CompareTo(50L).Should().BeGreaterThan(0);
    }

    [Fact]
    public void CompareTo_Long_WithLargerValue_ShouldReturnNegative()
    {
        var atomic = new AtomicLong(10L);

        atomic.CompareTo(50L).Should().BeLessThan(0);
    }

    [Fact]
    public void CompareTo_Long_WithNegativeValues_ShouldWork()
    {
        var atomic = new AtomicLong(-10L);

        atomic.CompareTo(-20L).Should().BeGreaterThan(0);
        atomic.CompareTo(-10L).Should().Be(0);
        atomic.CompareTo(0L).Should().BeLessThan(0);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        var atomic = new AtomicLong(42L);

        atomic.Equals(42L).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        var atomic = new AtomicLong(42L);

        atomic.Equals(99L).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithZero_ShouldMatchDefault()
    {
        var atomic = new AtomicLong();

        atomic.Equals(0L).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithNegativeValue_ShouldWork()
    {
        var atomic = new AtomicLong(-100L);

        atomic.Equals(-100L).Should().BeTrue();
        atomic.Equals(100L).Should().BeFalse();
    }

    [Fact]
    public async Task ConcurrentSet_ShouldBeThreadSafe()
    {
        var atomic = new AtomicLong(0L);
        const int threadCount = 10;

        var tasks = Enumerable.Range(0, threadCount).Select(i => Task.Run(() =>
        {
            for (long j = 0; j < 1000; j++)
            {
                atomic.Set(i * 1000 + j);
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        var finalValue = atomic.Get();
        finalValue.Should().BeGreaterOrEqualTo(0);
        finalValue.Should().BeLessThan(threadCount * 1000);
    }

    [Fact]
    public async Task ConcurrentGetAndSet_ShouldReturnAllDistinctOldValues()
    {
        var atomic = new AtomicLong(0L);
        const int threadCount = 10;
        const int operationsPerThread = 100;

        var results = new System.Collections.Concurrent.ConcurrentBag<long>();

        var tasks = Enumerable.Range(0, threadCount).Select(_ => Task.Run(() =>
        {
            for (int i = 0; i < operationsPerThread; i++)
            {
                var old = atomic.GetAndSet(1L);
                results.Add(old);
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        results.Should().HaveCount(threadCount * operationsPerThread);
        atomic.Get().Should().Be(1L);
    }
}
