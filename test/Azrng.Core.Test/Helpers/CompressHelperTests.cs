using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class CompressHelperTests
{
    [Fact]
    public void Compress_And_Decompress_RoundTrip_ReturnsOriginalString()
    {
        var original = "Hello, World! 你好世界";

        var compressed = CompressHelper.Compress(original);
        var decompressed = CompressHelper.Decompress(compressed);

        decompressed.Should().Be(original);
    }

    [Fact]
    public void Compress_ReturnsNonEmptyBase64String()
    {
        var result = CompressHelper.Compress("test");

        result.Should().NotBeNullOrEmpty();
        Convert.FromBase64String(result); // should not throw
    }

    [Fact]
    public void Compress_EmptyString_RoundTripsToEmptyString()
    {
        var result = CompressHelper.Compress(string.Empty);

        result.Should().NotBeNull();
        var decompressed = CompressHelper.Decompress(result);
        decompressed.Should().BeEmpty();
    }

    [Fact]
    public void Compress_LongString_RoundTripsCorrectly()
    {
        var original = new string('A', 10000);

        var compressed = CompressHelper.Compress(original);
        var decompressed = CompressHelper.Decompress(compressed);

        decompressed.Should().Be(original);
    }

    [Fact]
    public void Compress_UnicodeString_RoundTripsCorrectly()
    {
        var original = "日本語テスト 🚀 émojis & spëcial chars: <>&\"'";

        var compressed = CompressHelper.Compress(original);
        var decompressed = CompressHelper.Decompress(compressed);

        decompressed.Should().Be(original);
    }

    [Fact]
    public void Compress_SimilarContent_CompressesToShorterString()
    {
        var original = new string('B', 1000);

        var compressed = CompressHelper.Compress(original);

        // GZip should compress repetitive content significantly
        compressed.Length.Should().BeLessThan(original.Length);
    }

    [Fact]
    public void Decompress_InvalidBase64_ThrowsFormatException()
    {
        var act = () => CompressHelper.Decompress("not-valid-base64!!!");

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Decompress_ValidBase64ButNotGZip_ThrowsInvalidDataException()
    {
        var invalidGzip = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 });

        var act = () => CompressHelper.Decompress(invalidGzip);

        act.Should().Throw<InvalidDataException>();
    }
}
