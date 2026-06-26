using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class MimeExtensionsTests
{
    #region GetMimeType - null / empty / whitespace

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("\t")]
    public void GetMimeType_NullOrWhitespace_ShouldReturnNull(string? input)
    {
        input.GetMimeType().Should().BeNull();
    }

    #endregion

    #region GetMimeType - known extensions

    [Theory]
    [InlineData("test.pdf", "application/pdf")]
    [InlineData("image.jpg", "image/jpeg")]
    [InlineData("image.jpeg", "image/jpeg")]
    [InlineData("image.png", "image/png")]
    [InlineData("image.gif", "image/gif")]
    [InlineData("image.bmp", "image/bmp")]
    [InlineData("image.webp", "image/webp")]
    [InlineData("image.svg", "image/svg+xml")]
    [InlineData("doc.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("sheet.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [InlineData("slide.pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation")]
    [InlineData("page.html", "text/html")]
    [InlineData("page.htm", "text/html")]
    [InlineData("style.css", "text/css")]
    [InlineData("app.js", "text/javascript")]
    [InlineData("data.json", "application/json")]
    [InlineData("data.xml", "text/xml")]
    [InlineData("data.csv", "text/csv")]
    [InlineData("readme.txt", "text/plain")]
    [InlineData("readme.md", "text/markdown")]
    [InlineData("archive.zip", "application/x-zip-compressed")]
    [InlineData("archive.7z", "application/x-7z-compressed")]
    [InlineData("archive.tar", "application/x-tar")]
    [InlineData("archive.gz", "application/x-gzip")]
    [InlineData("video.mp4", "video/mp4")]
    [InlineData("video.avi", "video/x-msvideo")]
    [InlineData("video.mov", "video/quicktime")]
    [InlineData("video.webm", "video/webm")]
    [InlineData("audio.mp3", "audio/mpeg")]
    [InlineData("audio.wav", "audio/wav")]
    [InlineData("audio.flac", "audio/flac")]
    [InlineData("audio.aac", "audio/aac")]
    [InlineData("audio.ogg", "video/ogg")]
    [InlineData("font.ttf", "application/x-font-ttf")]
    [InlineData("font.woff", "application/font-woff")]
    [InlineData("font.woff2", "font/woff2")]
    [InlineData("font.otf", "font/otf")]
    [InlineData("app.exe", "application/vnd.microsoft.portable-executable")]
    [InlineData("app.msi", "application/octet-stream")]
    [InlineData("app.apk", "application/vnd.android.package-archive")]
    [InlineData("app.wasm", "application/wasm")]
    [InlineData("code.cs", null)]
    [InlineData("code.c", "text/plain")]
    [InlineData("code.cpp", "text/plain")]
    [InlineData("code.h", "text/plain")]
    [InlineData("image.ico", "image/x-icon")]
    [InlineData("image.tiff", "image/tiff")]
    [InlineData("image.tif", "image/tiff")]
    [InlineData("config.yaml", null)]
    public void GetMimeType_KnownExtension_ShouldReturnExpected(string fileName, string? expected)
    {
        fileName.GetMimeType().Should().Be(expected);
    }

    #endregion

    #region GetMimeType - case insensitivity

    [Theory]
    [InlineData("FILE.PDF", "application/pdf")]
    [InlineData("FILE.Pdf", "application/pdf")]
    [InlineData("FILE.JPG", "image/jpeg")]
    [InlineData("FILE.HTML", "text/html")]
    [InlineData("FILE.JSON", "application/json")]
    public void GetMimeType_UpperCaseExtension_ShouldReturnExpected(string fileName, string expected)
    {
        fileName.GetMimeType().Should().Be(expected);
    }

    #endregion

    #region GetMimeType - file with path

    [Fact]
    public void GetMimeType_FileWithPath_ShouldReturnMimeType()
    {
        "/usr/local/bin/app.pdf".GetMimeType().Should().Be("application/pdf");
    }

    [Fact]
    public void GetMimeType_WindowsPath_ShouldReturnMimeType()
    {
        @"C:\Users\test\document.docx".GetMimeType()
            .Should().Be("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
    }

    #endregion

    #region GetMimeType - no extension

    [Fact]
    public void GetMimeType_NoExtension_ShouldThrow()
    {
        var act = () => "Makefile".GetMimeType();
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region GetMimeType - unknown extension

    [Fact]
    public void GetMimeType_UnknownExtension_ShouldReturnNull()
    {
        "file.unknownext123".GetMimeType().Should().BeNull();
    }

    #endregion

    #region GetMimeType - multiple dots

    [Fact]
    public void GetMimeType_MultipleDots_ShouldUseLastExtension()
    {
        "archive.tar.gz".GetMimeType().Should().Be("application/x-gzip");
    }

    [Fact]
    public void GetMimeType_DottedFileName_ShouldUseLastExtension()
    {
        ".gitignore".GetMimeType().Should().BeNull();
    }

    #endregion
}
