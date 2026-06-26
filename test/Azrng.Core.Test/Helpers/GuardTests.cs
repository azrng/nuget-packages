using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class GuardTests
{
    [Fact]
    public void Against_ShouldReturnNonNullInstance()
    {
        var result = Guard.Against;

        result.Should().NotBeNull();
    }

    [Fact]
    public void Against_ShouldReturnIGuardClauseInstance()
    {
        var result = Guard.Against;

        result.Should().BeAssignableTo<IGuardClause>();
    }

    [Fact]
    public void Against_MultipleCalls_ShouldReturnSameInstance()
    {
        var first = Guard.Against;
        var second = Guard.Against;

        first.Should().BeSameAs(second);
    }

    [Fact]
    public void Against_ShouldReturnGuardInstance()
    {
        var result = Guard.Against;

        result.Should().BeOfType<Guard>();
    }

    [Fact]
    public void Guard_ShouldImplementIGuardClause()
    {
        typeof(Guard).Should().Implement<IGuardClause>();
    }

    [Fact]
    public void IGuardClause_ShouldBeInterface()
    {
        typeof(IGuardClause).IsInterface.Should().BeTrue();
    }
}
