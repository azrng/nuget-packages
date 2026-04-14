using Common.QRCode;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;
using System.Runtime.Versioning;
using Xunit;

namespace Common.QRCode.Test;

[SupportedOSPlatform("windows")]
public class QrCodeHelpTests
{
    [Fact]
    public void AddQrCode_RegistersTransientService()
    {
        var services = new ServiceCollection();

        services.AddQrCode();

        services.Should().ContainSingle(x =>
            x.ServiceType == typeof(IQrCodeHelp) &&
            x.ImplementationType == typeof(QrCodeHelp) &&
            x.Lifetime == ServiceLifetime.Transient);
    }

    [Theory]
    [InlineData(120, 120)]
    [InlineData(180, 90)]
    public void CreateQrCode_ReturnsBmpImageWithRequestedDimensions(int width, int height)
    {
        var helper = new QrCodeHelp();

        var bytes = helper.CreateQrCode("https://example.com", width, height);

        bytes.Should().NotBeEmpty();
        bytes[0].Should().Be((byte)'B');
        bytes[1].Should().Be((byte)'M');

        using var image = LoadImage(bytes);
        image.Width.Should().Be(width);
        image.Height.Should().Be(height);
    }

    [Theory]
    [InlineData("1234567890", 160, 80)]
    [InlineData("998877665544", 220, 100)]
    public void CreateBarCode_ReturnsBmpImageWithRequestedDimensions(string content, int width, int height)
    {
        var helper = new QrCodeHelp();

        var bytes = helper.CreateBarCode(content, width, height);

        bytes.Should().NotBeEmpty();
        bytes[0].Should().Be((byte)'B');
        bytes[1].Should().Be((byte)'M');

        using var image = LoadImage(bytes);
        image.Width.Should().Be(width);
        image.Height.Should().Be(height);
    }

    private static Image LoadImage(byte[] buffer)
    {
        var length = BitConverter.ToInt32(buffer, 2);
        using var stream = new MemoryStream(buffer, 0, length, writable: false, publiclyVisible: true);
        return Image.FromStream(stream);
    }
}
