using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class RandomArraySelectorTests
{
    [Fact]
    public void GetNext_ShouldReturnEachItemOncePerCycle()
    {
        var source = new[] { 1, 2, 3, 4 };
        var selector = new RandomArraySelector<int>(source);

        var firstCycle = Enumerable.Range(0, source.Length)
            .Select(_ => selector.GetNext())
            .ToArray();

        firstCycle.Should().BeEquivalentTo(source);
    }

    [Fact]
    public void GetNext_ShouldRestartAfterCycleEnds()
    {
        var source = new[] { "A", "B", "C" };
        var selector = new RandomArraySelector<string>(source);

        _ = Enumerable.Range(0, source.Length).Select(_ => selector.GetNext()).ToArray();
        var secondCycle = Enumerable.Range(0, source.Length).Select(_ => selector.GetNext()).ToArray();

        secondCycle.Should().BeEquivalentTo(source);
    }
}
