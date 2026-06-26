using Azrng.Core.Exceptions;
using Azrng.Core.Helpers;
using FluentAssertions;
using System.Reflection;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class AssemblyHelperTests
{
    #region GetAssemblies

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetAssemblies_NullOrWhitespace_ShouldReturnEmptyArray(string? searchPattern)
    {
        var result = AssemblyHelper.GetAssemblies(searchPattern!);

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("test.dll")]
    [InlineData("test.*.dll")]
    [InlineData("*.dll")]
    public void GetAssemblies_ValidPattern_ShouldNotThrow(string searchPattern)
    {
        var action = () => AssemblyHelper.GetAssemblies(searchPattern);

        action.Should().NotThrow();
    }

    [Theory]
    [InlineData("test<.dll")]
    [InlineData("test>.dll")]
    [InlineData("test|.dll")]
    public void GetAssemblies_InvalidCharacters_ShouldThrowParameterException(string searchPattern)
    {
        var action = () => AssemblyHelper.GetAssemblies(searchPattern);

        action.Should().Throw<ParameterException>();
    }

    [Theory]
    [InlineData("sub/test.dll")]
    [InlineData(@"sub\test.dll")]
    public void GetAssemblies_PathSeparator_ShouldThrowParameterException(string searchPattern)
    {
        var action = () => AssemblyHelper.GetAssemblies(searchPattern);

        action.Should().Throw<ParameterException>();
    }

    [Fact]
    public void GetAssemblies_RootedPath_ShouldThrowParameterException()
    {
        var action = () => AssemblyHelper.GetAssemblies(@"C:\test.dll");

        action.Should().Throw<ParameterException>();
    }

    [Theory]
    [InlineData("test.txt")]
    [InlineData("test.exe")]
    public void GetAssemblies_NonDllExtension_ShouldThrowParameterException(string searchPattern)
    {
        var action = () => AssemblyHelper.GetAssemblies(searchPattern);

        action.Should().Throw<ParameterException>();
    }

    [Fact]
    public void GetAssemblies_SamePattern_ShouldReturnCachedResult()
    {
        var first = AssemblyHelper.GetAssemblies("Azrng.Core.Test.dll");
        var second = AssemblyHelper.GetAssemblies("Azrng.Core.Test.dll");

        first.Should().BeSameAs(second);
    }

    [Fact]
    public void GetAssemblies_WildcardPattern_ShouldReturnArray()
    {
        var result = AssemblyHelper.GetAssemblies("*.dll");

        result.Should().NotBeNull();
    }

    #endregion

    #region GetEntryAssembly

    [Fact]
    public void GetEntryAssembly_ShouldReturnAssembly()
    {
        var result = AssemblyHelper.GetEntryAssembly();

        result.Should().NotBeNull();
    }

    [Fact]
    public void GetEntryAssembly_ShouldReturnAssemblyWithName()
    {
        var result = AssemblyHelper.GetEntryAssembly();

        result!.GetName().Name.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region GetAllReferencedAssemblies

    [Fact]
    public void GetAllReferencedAssemblies_Default_ShouldReturnNonEmptyCollection()
    {
        var result = AssemblyHelper.GetAllReferencedAssemblies();

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetAllReferencedAssemblies_SkipSystemAssemblies_ShouldNotThrow()
    {
        var action = () => AssemblyHelper.GetAllReferencedAssemblies(skipSystemAssemblies: true);

        action.Should().NotThrow();
    }

    [Fact]
    public void GetAllReferencedAssemblies_IncludeSystemAssemblies_ShouldNotThrow()
    {
        var action = () => AssemblyHelper.GetAllReferencedAssemblies(skipSystemAssemblies: false);

        action.Should().NotThrow();
    }

    [Fact]
    public void GetAllReferencedAssemblies_SkipFalse_ShouldReturnMoreOrEqualThanSkipTrue()
    {
        var withSkip = AssemblyHelper.GetAllReferencedAssemblies(skipSystemAssemblies: true).ToList();
        var withoutSkip = AssemblyHelper.GetAllReferencedAssemblies(skipSystemAssemblies: false).ToList();

        withoutSkip.Count.Should().BeGreaterThanOrEqualTo(withSkip.Count);
    }

    #endregion

    #region GetAssembly [NET6_0_OR_GREATER]

#if NET6_0_OR_GREATER
    [Fact]
    public void GetAssembly_KnownAssembly_ShouldReturnAssembly()
    {
        var result = AssemblyHelper.GetAssembly("System.Runtime");

        result.Should().NotBeNull();
        result.GetName().Name.Should().Be("System.Runtime");
    }

    [Fact]
    public void GetAssembly_UnknownAssembly_ShouldThrow()
    {
        var action = () => AssemblyHelper.GetAssembly("Non.Existent.Assembly.Xyz");

        action.Should().Throw<Exception>();
    }
#endif

    #endregion

    #region GetType(string, string) [NET6_0_OR_GREATER]

#if NET6_0_OR_GREATER
    [Fact]
    public void GetType_ByAssemblyAndTypeName_ShouldReturnType()
    {
        var result = AssemblyHelper.GetType("System.Runtime", "System.String");

        result.Should().NotBeNull();
        result.FullName.Should().Be("System.String");
    }

    [Fact]
    public void GetType_InvalidTypeName_ShouldReturnNull()
    {
        var result = AssemblyHelper.GetType("System.Runtime", "Non.Existent.Type");

        result.Should().BeNull();
    }
#endif

    #endregion

    #region GetStringType [NET6_0_OR_GREATER]

#if NET6_0_OR_GREATER
    [Fact]
    public void GetStringType_ValidFormat_ShouldReturnType()
    {
        var result = AssemblyHelper.GetStringType("System.Runtime;System.String");

        result.Should().NotBeNull();
        result.FullName.Should().Be("System.String");
    }

    [Fact]
    public void GetStringType_InvalidFormat_ShouldThrow()
    {
        var action = () => AssemblyHelper.GetStringType("InvalidFormat");

        action.Should().Throw<IndexOutOfRangeException>();
    }
#endif

    #endregion

    #region LoadAssembly(string)

    [Fact]
    public void LoadAssembly_NonExistentPath_ShouldReturnNull()
    {
        var result = AssemblyHelper.LoadAssembly(@"C:\nonexistent\path\assembly.dll");

        result.Should().BeNull();
    }

    [Fact]
    public void LoadAssembly_ValidPath_ShouldReturnAssembly()
    {
        var location = typeof(AssemblyHelper).Assembly.Location;

        var result = AssemblyHelper.LoadAssembly(location);

        result.Should().NotBeNull();
    }

    #endregion

    #region LoadAssembly(MemoryStream)

    [Fact]
    public void LoadAssembly_FromMemoryStream_ShouldReturnAssembly()
    {
        var location = typeof(AssemblyHelper).Assembly.Location;
        var bytes = File.ReadAllBytes(location);
        using var stream = new MemoryStream(bytes);

        var result = AssemblyHelper.LoadAssembly(stream);

        result.Should().NotBeNull();
    }

    [Fact]
    public void LoadAssembly_FromMemoryStream_ShouldLoadCorrectAssembly()
    {
        var location = typeof(AssemblyHelper).Assembly.Location;
        var bytes = File.ReadAllBytes(location);
        using var stream = new MemoryStream(bytes);

        var result = AssemblyHelper.LoadAssembly(stream);

        result.GetName().Name.Should().Be("Azrng.Core");
    }

    #endregion

    #region GetType(MemoryStream, string)

    [Fact]
    public void GetType_FromStream_ShouldReturnType()
    {
        var location = typeof(AssemblyHelper).Assembly.Location;
        var bytes = File.ReadAllBytes(location);
        using var stream = new MemoryStream(bytes);

        var result = AssemblyHelper.GetType(stream, "Azrng.Core.Helpers.AssemblyHelper");

        result.Should().NotBeNull();
        result.FullName.Should().Be("Azrng.Core.Helpers.AssemblyHelper");
    }

    [Fact]
    public void GetType_FromStream_InvalidType_ShouldReturnNull()
    {
        var location = typeof(AssemblyHelper).Assembly.Location;
        var bytes = File.ReadAllBytes(location);
        using var stream = new MemoryStream(bytes);

        var result = AssemblyHelper.GetType(stream, "Non.Existent.Type");

        result.Should().BeNull();
    }

    #endregion

    #region TryLoadAssembly

    [Fact]
    public void TryLoadAssembly_ValidPath_ShouldReturnAssembly()
    {
        var location = typeof(AssemblyHelper).Assembly.Location;

        var result = AssemblyHelper.TryLoadAssembly(location);

        result.Should().NotBeNull();
    }

    [Fact]
    public void TryLoadAssembly_InvalidPath_ShouldThrow()
    {
        var action = () => AssemblyHelper.TryLoadAssembly(@"C:\nonexistent\assembly.dll");

        action.Should().Throw<Exception>();
    }

    #endregion

    #region IsManagedAssembly

    [Fact]
    public void IsManagedAssembly_ManagedDll_ShouldReturnTrue()
    {
        var location = typeof(AssemblyHelper).Assembly.Location;

        var result = AssemblyHelper.IsManagedAssembly(location);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsManagedAssembly_NonExistentFile_ShouldThrow()
    {
        var action = () => AssemblyHelper.IsManagedAssembly(@"C:\nonexistent\file.dll");

        action.Should().Throw<Exception>();
    }

    #endregion

    #region IsSystemAssembly

    [Fact]
    public void IsSystemAssembly_SystemDll_ShouldReturnTrue()
    {
        var systemAssemblyPath = Directory.GetFiles(
            System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(),
            "System.Runtime.dll").FirstOrDefault();

        if (systemAssemblyPath != null)
        {
            var result = AssemblyHelper.IsSystemAssembly(systemAssemblyPath);

            result.Should().BeTrue();
        }
    }

    [Fact]
    public void IsSystemAssembly_NonSystemDll_ShouldReturnFalse()
    {
        var location = typeof(AssemblyHelper).Assembly.Location;

        var result = AssemblyHelper.IsSystemAssembly(location);

        result.Should().BeFalse();
    }

    #endregion
}
