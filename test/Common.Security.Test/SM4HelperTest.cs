using Common.Security.Enums;
using Xunit.Abstractions;

namespace Common.Security.Test;

public class Sm4HelperTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Sm4HelperTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private const string SecretKey = "csNH9xjQVLsJGEhD";

    private const string Iv = "csNH9xjQVLsJGbca";


    /// <summary>
    /// ecb加密 十六进制
    /// </summary>
    [Fact]
    public void EncryptEcb_Hex_ReturnOk()
    {
        var str = "123456";

        var encryptStr = Sm4Helper.Encrypt(str, SecretKey, outType: OutType.Hex);
        _testOutputHelper.WriteLine(encryptStr);

        var decrypt = Sm4Helper.Decrypt(encryptStr, SecretKey, inputType: OutType.Hex);
        _testOutputHelper.WriteLine(decrypt);

        Assert.Equal(str, decrypt);
    }

    /// <summary>
    /// ecb加密  base64
    /// </summary>
    [Fact]
    public void EncryptEcb_Base64_ReturnOk()
    {
        var str = "123456";

        var encryptStr = Sm4Helper.Encrypt(str, SecretKey, outType: OutType.Base64);
        _testOutputHelper.WriteLine(encryptStr);

        var decrypt = Sm4Helper.Decrypt(encryptStr, SecretKey, inputType: OutType.Base64);
        _testOutputHelper.WriteLine(decrypt);

        Assert.Equal(str, decrypt);
    }

    /// <summary>
    /// cbc加密 十六进制
    /// </summary>
    [Fact]
    public void EncryptCbc_Hex_ReturnOk()
    {
        var str = "123456";

        var encryptStr = Sm4Helper.Encrypt(str, SecretKey, Sm4CryptoEnum.CBC, outType: OutType.Hex, iv: Iv);
        _testOutputHelper.WriteLine(encryptStr);

        var decrypt = Sm4Helper.Decrypt(encryptStr, SecretKey, Sm4CryptoEnum.CBC, inputType: OutType.Hex,
            iv: Iv);
        _testOutputHelper.WriteLine(decrypt);

        Assert.Equal(str, decrypt);
    }

    /// <summary>
    /// cbc加密  base64
    /// </summary>
    [Fact]
    public void EncryptCbc_Base64_ReturnOk()
    {
        var str = "123456";

        var encryptStr =
            Sm4Helper.Encrypt(str, SecretKey, Sm4CryptoEnum.CBC, outType: OutType.Base64, iv: Iv);
        _testOutputHelper.WriteLine(encryptStr);

        var decrypt = Sm4Helper.Decrypt(encryptStr, SecretKey, Sm4CryptoEnum.CBC, inputType: OutType.Base64,
            iv: Iv);
        _testOutputHelper.WriteLine(decrypt);

        Assert.Equal(str, decrypt);
    }

    /// <summary>
    /// cbc加密固定结果测试
    /// </summary>
    [Fact]
    public void EncryptCbc_FixedResult_ReturnOk()
    {
        var str = "质控规则类型权重";

        var encryptStr =
            Sm4Helper.Encrypt(str, SecretKey, Sm4CryptoEnum.CBC, outType: OutType.Base64, iv: Iv);
        _testOutputHelper.WriteLine(encryptStr);

        Assert.Equal("7rDhFse/S53cpUB2YTMOpuZ5bb2RMUx0Tag+8jkyYyk=", encryptStr);
    }

    /// <summary>
    /// ecb加密固定结果测试
    /// </summary>
    [Fact]
    public void EncryptEcb_FixedResult_ReturnOk()
    {
        var str = "质控规则类型权重";

        var encryptStr =
            Sm4Helper.Encrypt(str, SecretKey, outType: OutType.Hex);
        _testOutputHelper.WriteLine(encryptStr);

        Assert.Equal("ED0A37CCADFAC11E2E4982BF68BA5CDE3D95A527F59808C56056279C1CF236B6", encryptStr.ToUpper());
    }
    [Fact]
    public void Encrypt_InvalidKeyLength_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => Sm4Helper.Encrypt("123456", "short-key"));
        Assert.Contains("16 bytes", ex.Message);
    }

    [Fact]
    public void Encrypt_CbcInvalidIvLength_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Sm4Helper.Encrypt("123456", SecretKey, Sm4CryptoEnum.CBC, iv: "short-iv"));
        Assert.Contains("16 bytes", ex.Message);
    }
}
