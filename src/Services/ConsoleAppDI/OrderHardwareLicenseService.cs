using Azrng.ConsoleApp.DependencyInjection;
using Common.Security;
using Common.Security.Enums;
using Microsoft.Extensions.Logging;
using StudyUse;
using System.Security.Cryptography;

namespace ConsoleAppDI;

/// <summary>
/// 订单硬件许可证服务
/// </summary>
public class OrderHardwareLicenseService : IServiceStart
{
    public string Title => "订单硬件许可证服务";

    private const string PublicKeyPem =
        "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA1Xmg9t/nvL9yJvlGZfcRtAm68TYWPkufWXG8sTXysIX7YX+EM9KoqYQcWguM2sZ261+XjbxD5/FI9nMH/Jf0X17tqyZPT/dM0zfd/S1KpjMcIxfBFC79sHq7KsKPA4JMgrczSkhNPAJwTyj9WbfHfDzVUOkDHnQjU3PTvZbDq/RVjbVTrZ7nScGB0epsVNS5IFEwnDyU18+9VAIBF9Wcjv3g0Rw9Yr+rEyLzOkoFpqUItHxM9FxCVH2YyxP8SH37M/A5UPfchi9gGzlhKtIPbwT8GzJueF5K8ZZd7VaOk/+ULAUyzggdSH2RtjMWu/3drueidvXYKsp4rdce5y0V4QIDAQAB";

    private string PrivateKey =
        "MIIEpQIBAAKCAQEA1Xmg9t/nvL9yJvlGZfcRtAm68TYWPkufWXG8sTXysIX7YX+EM9KoqYQcWguM2sZ261+XjbxD5/FI9nMH/Jf0X17tqyZPT/dM0zfd/S1KpjMcIxfBFC79sHq7KsKPA4JMgrczSkhNPAJwTyj9WbfHfDzVUOkDHnQjU3PTvZbDq/RVjbVTrZ7nScGB0epsVNS5IFEwnDyU18+9VAIBF9Wcjv3g0Rw9Yr+rEyLzOkoFpqUItHxM9FxCVH2YyxP8SH37M/A5UPfchi9gGzlhKtIPbwT8GzJueF5K8ZZd7VaOk/+ULAUyzggdSH2RtjMWu/3drueidvXYKsp4rdce5y0V4QIDAQABAoIBAD4uqbZ/zi7qzUORBPKan2uEEhJFIQGQYaBaQw6TmlfVDz59OHMn/70xIFkSWDs56FXziF/e4SDk5c3z//WSBbrqGezqwduXO6ei9GTVFpOG+8E1ICbS8U7A0VhJSANLpyHEX4pZuTWruC82z2Wc5NzJk5F37lKmncunR5xIGEMz5qix13Y9Zl+33471mY1vyTHuyYJnR8Z7OBvZeEvpsxLaAYhENUMcsWRVoNSQFHwfq0zCkHMM/g46ZmT6GJdxq8nd8IBEC/v6jLUMrtw801+bkq106Zs6ZIog9ZXT726r0N5Dc/UXIJoEG9STA0GrLvFqg7h9YcXgOKC2JTXjD2kCgYEA5WqsIY2E6dPvsuVgTKlWqOSzTwarlP6dEtQdtT6+GFK7XNMNyp1N7S/WS2PCuqfAVGgGARjjy5rugmlJD1DdjKgn2msXBm1YeNwS+hy9k2fRDzJeK9s4HKTdBVaUsJOeuC28njcCHDLAoIxi2Qowt2gYiYz0QwUYO75C5qWSXlsCgYEA7jYSVuUcsNHPWbwdAwMK+AtymcUwT0seCIRuGN7wUZ44UZfmu9bFVX1Jvk+VTlGUEDTIFQPQ2UXuLxeFR3A78kRPZ/Xh9ZxON71mxcWkfO8ovM6M+/566M0en5k7L3k2yNDRmj3ijUlG9iSLnCxkI8Y2eXQrchODG6OqxtUqiXMCgYEAgi+kem3ajO5tyXEM0rQNr04IysGYQLaz3+lq6l0udpMMK8LAwse9XumUi7eS22UyaTOWpKzBJ9tFmc+xW/Who54Q74txx33phLwuMKx6j9mL8zm74ttF3ktX+R2GxyUeHpolvQquMd5DHVhNB6kWuB1kPzozqoyLkeuH/2bZxp0CgYEA4mx3Ji2FBfWIWE1cbj54MKoA9nTepKBMHeBzHiTa3Vm9QqFWanmM/OOoMsNGsjkMuhLRlFgaLkwwSIbc4ril4nRX3gN0EpfOKWFYzOg+n5pcaIsUq3qKrbo7P4zRGyDmmB8U/L2SGKXsU48NPRdc4DxKD0wC993gI2eArpp0Yk0CgYEAldYuo9QbV/i5vEejzp2mjVo9fLeukFWQlYKUjhXfCoQA9ywIoSMh2I6Ea8SeHav/5jF8TOWuTGEJwQc+AAczSKRMe1vn1VBGoZhTiK1JJLeEmNxGvUvETKvXrESLwLfGuAhstHRT6B6ViRSN8f/ymxOZRS4me6SdWDYFc+kpbEs=";

    private readonly ILogger<OrderHardwareLicenseService> _logger;

    public OrderHardwareLicenseService(ILogger<OrderHardwareLicenseService> logger)
    {
        _logger = logger;
    }

    public Task RunAsync()
    {
        // 设备指纹
        var fingerprint = "185226bc7bd2613634e9193d7878e2d7242822bef42a7fc8ba7ffe8cb42f2b43";

        fingerprint = "79e9afeae5f17b40b685f7c84e01af89f76a060209d6541ffb41bef35deb1976";

        // 生成授权码
        var licenseKey = RsaHelper.SignData(fingerprint, PrivateKey, HashAlgorithmName.SHA256, outputType: OutType.Hex);
        _logger.LogInformation($"授权码：{licenseKey}");

        var result = OrderHardwareLicense.Validate(fingerprint, licenseKey, PublicKeyPem,false);
        Console.WriteLine(result);

        return Task.CompletedTask;
    }
}