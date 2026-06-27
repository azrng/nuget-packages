using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class RandomExtensionsTests
{
    private readonly Random _random = new();

    [Theory]
    [InlineData(0.0, 1.0)]
    [InlineData(-10.0, 10.0)]
    [InlineData(100.0, 200.0)]
    [InlineData(-100.0, -50.0)]
    [InlineData(0.0, 0.0001)]
    [InlineData(double.MinValue / 2, double.MaxValue / 2)]
    public void NextDouble_ShouldReturnWithinRange(double minValue, double maxValue)
    {
        for (int i = 0; i < 1000; i++)
        {
            var result = _random.NextDouble(minValue, maxValue);

            result.Should().BeGreaterThanOrEqualTo(minValue);
            result.Should().BeLessThan(maxValue);
        }
    }

    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(5.0, 5.0)]
    [InlineData(-3.0, -3.0)]
    public void NextDouble_ShouldThrowWhenMinEqualsMax(double minValue, double maxValue)
    {
        var act = () => _random.NextDouble(minValue, maxValue);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("minValue");
    }

    [Theory]
    [InlineData(1.0, 0.0)]
    [InlineData(10.0, -10.0)]
    [InlineData(0.0, -1.0)]
    public void NextDouble_ShouldThrowWhenMinGreaterThanMax(double minValue, double maxValue)
    {
        var act = () => _random.NextDouble(minValue, maxValue);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("minValue");
    }

    [Fact]
    public void NextDouble_WithSeededRandom_ShouldReturnExpectedValues()
    {
        var seeded = new Random(42);

        var result1 = seeded.NextDouble(0.0, 100.0);
        var result2 = seeded.NextDouble(0.0, 100.0);

        result1.Should().BeGreaterThanOrEqualTo(0.0).And.BeLessThan(100.0);
        result2.Should().BeGreaterThanOrEqualTo(0.0).And.BeLessThan(100.0);
        result1.Should().NotBe(result2);
    }

    [Fact]
    public void NextDouble_WithNegativeRange_ShouldReturnWithinRange()
    {
        for (int i = 0; i < 1000; i++)
        {
            var result = _random.NextDouble(-200.0, -100.0);

            result.Should().BeGreaterThanOrEqualTo(-200.0);
            result.Should().BeLessThan(-100.0);
        }
    }

    [Fact]
    public void NextDouble_ShouldProduceDifferentValuesOverMultipleCalls()
    {
        var values = new HashSet<double>();

        for (int i = 0; i < 100; i++)
        {
            values.Add(_random.NextDouble(0.0, 1000.0));
        }

        values.Count.Should().BeGreaterThan(1);
    }
}
