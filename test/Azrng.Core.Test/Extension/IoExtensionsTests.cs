using System.Text;
using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class IoExtensionsTests : IDisposable
{
    private readonly string _tempDir;

    public IoExtensionsTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"IoExtensionsTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string CreateTempFile(string content, string? fileName = null)
    {
        var path = Path.Combine(_tempDir, fileName ?? $"{Guid.NewGuid():N}.txt");
        File.WriteAllText(path, content, Encoding.UTF8);
        return path;
    }

    private string CreateTempFile(byte[] data, string? fileName = null)
    {
        var path = Path.Combine(_tempDir, fileName ?? $"{Guid.NewGuid():N}.bin");
        File.WriteAllBytes(path, data);
        return path;
    }

    #region ToStreamFromFile

    [Fact]
    public void ToStreamFromFile_ShouldReturnStream_WhenFileExists()
    {
        var content = "Hello, World!";
        var path = CreateTempFile(content);

        using var stream = path.ToStreamFromFile();

        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        reader.ReadToEnd().Should().Be(content);
    }

    [Fact]
    public void ToStreamFromFile_ShouldReturnNull_WhenFileNotExists()
    {
        var path = Path.Combine(_tempDir, "nonexistent.txt");

        var result = path.ToStreamFromFile();

        result.Should().BeNull();
    }

    #endregion

    #region ToStreamFromFileAsync

    [Fact]
    public async Task ToStreamFromFileAsync_ShouldReturnStream_WhenFileExists()
    {
        var content = "Async Stream Test";
        var path = CreateTempFile(content);

        using var stream = await path.ToStreamFromFileAsync();

        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        reader.ReadToEnd().Should().Be(content);
    }

    [Fact]
    public async Task ToStreamFromFileAsync_ShouldReturnNull_WhenFileNotExists()
    {
        var path = Path.Combine(_tempDir, "nonexistent.txt");

        var result = await path.ToStreamFromFileAsync();

        result.Should().BeNull();
    }

    #endregion

    #region ToBytesFromFile

    [Fact]
    public void ToBytesFromFile_ShouldReturnBytes_WhenFileExists()
    {
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var path = CreateTempFile(data);

        var result = path.ToBytesFromFile();

        result.Should().NotBeNull();
        result.Should().Equal(data);
    }

    [Fact]
    public void ToBytesFromFile_ShouldReturnNull_WhenFileNotExists()
    {
        var path = Path.Combine(_tempDir, "nonexistent.txt");

        var result = path.ToBytesFromFile();

        result.Should().BeNull();
    }

    #endregion

    #region ToBytesFromFileAsync

    [Fact]
    public async Task ToBytesFromFileAsync_ShouldReturnBytes_WhenFileExists()
    {
        var data = new byte[] { 10, 20, 30 };
        var path = CreateTempFile(data);

        var result = await path.ToBytesFromFileAsync();

        result.Should().NotBeNull();
        result!.Should().Equal(data);
    }

    [Fact]
    public async Task ToBytesFromFileAsync_ShouldReturnNull_WhenFileNotExists()
    {
        var path = Path.Combine(_tempDir, "nonexistent.txt");

        var result = await path.ToBytesFromFileAsync();

        result.Should().BeNull();
    }

    #endregion

    #region GetFileSize

    [Fact]
    public void GetFileSize_ShouldReturnSize_WhenFileExists()
    {
        var data = new byte[123];
        var path = CreateTempFile(data);

        var result = path.GetFileSize();

        result.Should().Be(123);
    }

    [Fact]
    public void GetFileSize_ShouldReturnZero_WhenFileNotExists()
    {
        var path = Path.Combine(_tempDir, "nonexistent.txt");

        var result = path.GetFileSize();

        result.Should().Be(0);
    }

    #endregion

    #region GetFileFormatSize

    [Fact]
    public void GetFileFormatSize_ShouldReturnByte_WhenSizeLessThan1K()
    {
        var data = new byte[512];
        var path = CreateTempFile(data);

        var result = path.GetFileFormatSize();

        result.Should().NotBeNull();
        result.Should().EndWith("Byte");
    }

    [Fact]
    public void GetFileFormatSize_ShouldReturnK_WhenSizeBetween1KAnd1M()
    {
        var data = new byte[2048];
        var path = CreateTempFile(data);

        var result = path.GetFileFormatSize();

        result.Should().NotBeNull();
        result.Should().EndWith("K");
    }

    [Fact]
    public void GetFileFormatSize_ShouldReturnM_WhenSizeBetween1MAnd1G()
    {
        var data = new byte[2 * 1024 * 1024];
        var path = CreateTempFile(data);

        var result = path.GetFileFormatSize();

        result.Should().NotBeNull();
        result.Should().EndWith("M");
    }

    [Fact]
    public void GetFileFormatSize_ShouldReturnByte_WhenFileNotExists()
    {
        var path = Path.Combine(_tempDir, "nonexistent.txt");

        var result = path.GetFileFormatSize();

        result.Should().Be("0.00 Byte");
    }

    #endregion

    #region ToBytes(Stream)

    [Fact]
    public void ToBytes_ShouldReturnBytes_WhenStreamHasData()
    {
        var data = new byte[] { 1, 2, 3 };
        using var stream = new MemoryStream(data);

        var result = stream.ToBytes();

        result.Should().NotBeNull();
        result.Should().Equal(data);
    }

    [Fact]
    public void ToBytes_ShouldReturnEmptyArray_WhenStreamIsNull()
    {
        Stream? stream = null;

        var result = stream.ToBytes();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void ToBytes_ShouldReturnEmptyArray_WhenStreamIsEmpty()
    {
        using var stream = new MemoryStream();

        var result = stream.ToBytes();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void ToBytes_ShouldResetStreamPosition_AfterRead()
    {
        var data = new byte[] { 1, 2, 3 };
        using var stream = new MemoryStream(data);

        stream.ToBytes();

        stream.Position.Should().Be(0);
    }

    #endregion

    #region ToBytesAsync(Stream)

    [Fact]
    public async Task ToBytesAsync_ShouldReturnBytes_WhenStreamHasData()
    {
        var data = new byte[] { 4, 5, 6 };
        using var stream = new MemoryStream(data);

        var result = await stream.ToBytesAsync()!;

        result.Should().NotBeNull();
        result.Should().Equal(data);
    }

    [Fact]
    public async Task ToBytesAsync_ShouldReturnEmptyArray_WhenStreamIsNull()
    {
        Stream? stream = null;

        var result = await stream.ToBytesAsync()!;

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ToBytesAsync_ShouldReturnEmptyArray_WhenStreamIsEmpty()
    {
        using var stream = new MemoryStream();

        var result = await stream.ToBytesAsync()!;

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ToBytesAsync_ShouldResetStreamPosition_AfterRead()
    {
        var data = new byte[] { 4, 5, 6 };
        using var stream = new MemoryStream(data);

        await stream.ToBytesAsync()!;

        stream.Position.Should().Be(0);
    }

    #endregion

    #region ToFile(Stream, string)

    [Fact]
    public void ToFile_ShouldWriteStreamToFile_WhenStreamHasData()
    {
        var data = new byte[] { 7, 8, 9 };
        using var stream = new MemoryStream(data);
        var filePath = Path.Combine(_tempDir, "output.bin");

        var result = stream.ToFile(filePath);

        result.Should().BeTrue();
        File.Exists(filePath).Should().BeTrue();
        File.ReadAllBytes(filePath).Should().Equal(data);
    }

    [Fact]
    public void ToFile_ShouldReturnFalse_WhenStreamIsNull()
    {
        Stream? stream = null;
        var filePath = Path.Combine(_tempDir, "output.bin");

        var result = stream.ToFile(filePath);

        result.Should().BeFalse();
    }

    [Fact]
    public void ToFile_ShouldReturnFalse_WhenStreamIsEmpty()
    {
        using var stream = new MemoryStream();
        var filePath = Path.Combine(_tempDir, "output.bin");

        var result = stream.ToFile(filePath);

        result.Should().BeFalse();
    }

    #endregion

    #region ToFileAsync(Stream, string)

    [Fact]
    public async Task ToFileAsync_ShouldWriteStreamToFile_WhenStreamHasData()
    {
        var data = new byte[] { 10, 11, 12 };
        using var stream = new MemoryStream(data);
        var filePath = Path.Combine(_tempDir, "async_output.bin");

        var result = await stream.ToFileAsync(filePath);

        result.Should().BeTrue();
        File.Exists(filePath).Should().BeTrue();
        File.ReadAllBytes(filePath).Should().Equal(data);
    }

    [Fact]
    public async Task ToFileAsync_ShouldReturnFalse_WhenStreamIsNull()
    {
        Stream? stream = null;
        var filePath = Path.Combine(_tempDir, "async_output.bin");

        var result = await stream.ToFileAsync(filePath);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ToFileAsync_ShouldReturnFalse_WhenStreamIsEmpty()
    {
        using var stream = new MemoryStream();
        var filePath = Path.Combine(_tempDir, "async_output.bin");

        var result = await stream.ToFileAsync(filePath);

        result.Should().BeFalse();
    }

    #endregion
}
