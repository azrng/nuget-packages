using System.Reflection;
using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class AssemblyExtensionsTests
{
    [Fact]
    public void GetAssemblyName_ShouldWorkForAssemblyTypeAndTypeInfo()
    {
        var assembly = typeof(AssemblyExtensionsTests).Assembly;

        assembly.GetAssemblyName().Should().Be(assembly.GetName().Name);
        typeof(AssemblyExtensionsTests).GetAssemblyName().Should().Be(assembly.GetName().Name);
        typeof(AssemblyExtensionsTests).GetTypeInfo().GetAssemblyName().Should().Be(assembly.GetName().Name);
    }

    [Fact]
    public void GetType_ShouldReturnRuntimeType()
    {
        var assembly = typeof(AssemblyExtensionsTests).Assembly;

        assembly.GetType(typeof(AssemblyExtensionsTests).FullName!)
            .Should().Be(typeof(AssemblyExtensionsTests));
    }

    [Fact]
    public void IsSystemAssembly_ShouldIdentifyMicrosoftAssembly()
    {
        typeof(string).Assembly.IsSystemAssembly().Should().BeTrue();
        typeof(AssemblyExtensionsTests).Assembly.IsSystemAssembly().Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldBeTrueForCurrentAssembly()
    {
        typeof(AssemblyExtensionsTests).Assembly.IsValid().Should().BeTrue();
    }
}
