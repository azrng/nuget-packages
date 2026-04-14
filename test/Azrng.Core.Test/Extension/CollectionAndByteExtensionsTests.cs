using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class CollectionAndByteExtensionsTests
{
    [Fact]
    public void AddIfNotContains_ShouldAddOnlyWhenItemIsMissing()
    {
        var items = new List<int> { 1, 2 };

        items.AddIfNotContains(3).Should().BeTrue();
        items.AddIfNotContains(3).Should().BeFalse();
        items.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void AddIfNotContains_ShouldThrow_WhenCollectionIsNull()
    {
        ICollection<int>? items = null;

        var action = () => items!.AddIfNotContains(1);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ByteHelpers_ShouldConvertToHexBase64StreamAndInt32()
    {
        var bytes = new byte[] { 1, 2, 3, 4 };

        bytes.ToHexString().Should().Be("01020304");
        bytes.ToBase64().Should().Be("AQIDBA==");
        bytes.ToInt32().Should().Be(BitConverter.ToInt32(bytes, 0));

        using var stream = bytes.ToStream();
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        memory.ToArray().Should().Equal(bytes);
    }

    [Fact]
    public void ToInt32_ShouldReturnZero_WhenBytesLengthIsLessThanFour()
    {
        new byte[] { 1, 2, 3 }.ToInt32().Should().Be(0);
    }

    [Fact]
    public void FileTypeHelpers_ShouldDetectPngMetadata()
    {
        var pngBytes = new byte[] { 137, 80, 78, 71 };

        pngBytes.GetFileSuffix().Should().Be(".png");
        pngBytes.GetContentType().Should().Be("image/png");
        pngBytes.GetRandomFileName().Should().EndWith(".png");
    }

    [Fact]
    public void ToFileByBytes_ShouldPersistBytesToDisk()
    {
        var bytes = new byte[] { 10, 20, 30 };
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.bin");

        try
        {
            bytes.ToFileByBytes(filePath);

            File.ReadAllBytes(filePath).Should().Equal(bytes);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
