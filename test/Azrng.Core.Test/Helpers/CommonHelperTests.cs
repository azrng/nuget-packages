using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class CommonHelperTests
{
    [Fact]
    public void GenerateRandomNumber_DefaultLength_ReturnsNonEmptyBase64String()
    {
        var result = CommonHelper.GenerateRandomNumber();

        result.Should().NotBeNullOrEmpty();
        Convert.FromBase64String(result).Length.Should().Be(6);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    public void GenerateRandomNumber_CustomLength_ReturnsBase64StringWithCorrectLength(int length)
    {
        var result = CommonHelper.GenerateRandomNumber(length);

        result.Should().NotBeNullOrEmpty();
        var bytes = Convert.FromBase64String(result);
        bytes.Length.Should().Be(length);
    }

    [Fact]
    public void GenerateRandomNumber_MultipleCalls_ReturnsDifferentResults()
    {
        var results = Enumerable.Range(0, 10)
            .Select(_ => CommonHelper.GenerateRandomNumber())
            .Distinct()
            .Count();

        results.Should().Be(10);
    }

    [Fact]
    public void GenerateRandomNumber_ZeroLength_ReturnsEmptyBase64()
    {
        var result = CommonHelper.GenerateRandomNumber(0);

        result.Should().Be(Convert.ToBase64String(Array.Empty<byte>()));
    }

    [Fact]
    public void SeqGuid_ReturnsNonEmptyGuid()
    {
        var result = CommonHelper.SeqGuid();

        result.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void SeqGuid_MultipleCalls_ReturnsDifferentGuids()
    {
        var results = Enumerable.Range(0, 10)
            .Select(_ => CommonHelper.SeqGuid())
            .Distinct()
            .Count();

        results.Should().Be(10);
    }
}
