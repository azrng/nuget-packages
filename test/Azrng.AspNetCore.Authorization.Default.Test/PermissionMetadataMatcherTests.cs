using Azrng.AspNetCore.Authorization.Default;
using FluentAssertions;
using Xunit;

namespace Azrng.AspNetCore.Authorization.Default.Test;

public class PermissionMetadataMatcherTests
{
    [Fact]
    public void Any_ShouldPass_WhenOnePermissionIsGranted()
    {
        var required = new IPermissionMetadata[]
        {
            new PermissionMetadata(PermissionMatchMode.Any, "orders.read", "orders.export")
        };

        PermissionMetadataMatcher.IsSatisfiedBy(required, new[] { "orders.export" })
            .Should().BeTrue();
    }

    [Fact]
    public void All_ShouldFail_WhenOnePermissionIsMissing()
    {
        var required = new IPermissionMetadata[]
        {
            new PermissionMetadata(PermissionMatchMode.All, "orders.read", "orders.export")
        };

        PermissionMetadataMatcher.IsSatisfiedBy(required, new[] { "orders.read" })
            .Should().BeFalse();
    }

    [Fact]
    public void MultipleDeclarations_ShouldAllBeSatisfied()
    {
        var required = new IPermissionMetadata[]
        {
            new PermissionMetadata(PermissionMatchMode.Any, "orders.read", "orders.write"),
            new PermissionMetadata(PermissionMatchMode.All, "orders.export", "orders.audit")
        };

        PermissionMetadataMatcher.IsSatisfiedBy(
                required,
                new[] { "ORDERS.WRITE", "orders.export", "orders.audit" })
            .Should().BeTrue();

        PermissionMetadataMatcher.IsSatisfiedBy(
                required,
                new[] { "ORDERS.WRITE", "orders.export" })
            .Should().BeFalse();
    }

    [Fact]
    public void EmptyDeclarations_ShouldFail()
    {
        PermissionMetadataMatcher.IsSatisfiedBy(
                Array.Empty<IPermissionMetadata>(),
                new[] { "orders.read" })
            .Should().BeFalse();
    }
}
