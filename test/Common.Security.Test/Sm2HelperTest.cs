using Common.Security.Enums;
using Xunit.Abstractions;

namespace Common.Security.Test;

public class Sm2HelperTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Sm2HelperTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void GenKey_ReturnOk()
    {
        var (publicKey, privateKey) = Sm2Helper.ExportKey();
        _testOutputHelper.WriteLine("publicKey: " + publicKey);
        _testOutputHelper.WriteLine("privateKey: " + privateKey);
        Assert.NotEmpty(publicKey);
        Assert.NotEmpty(privateKey);
    }

    /// <summary>
    /// 十六进制的key 加解密
    /// </summary>/
    [Fact]
    public void Encrypt_Hex_ReturnOk()
    {
        var (publicKey, privateKey) = Sm2Helper.ExportKey();

        var source = "123456";
        var encryptStr = Sm2Helper.Encrypt(source, publicKey, publicKeyType: OutType.Hex, outType: OutType.Hex);
        _testOutputHelper.WriteLine(encryptStr);

        var decryptStr = Sm2Helper.Decrypt(encryptStr, privateKey, privateKeyType: OutType.Hex, inputType: OutType.Hex);
        _testOutputHelper.WriteLine(decryptStr);
        Assert.Equal(source, decryptStr);
    }

    /// <summary>
    /// base64 key的加解密
    /// </summary>
    [Fact]
    public void Encrypt_Base64_ReturnOk()
    {
        var (publicKey, privateKey) = Sm2Helper.ExportKey(OutType.Base64);

        var source = "123456";
        var encryptStr = Sm2Helper.Encrypt(source, publicKey, publicKeyType: OutType.Base64,
            outType: OutType.Hex);
        _testOutputHelper.WriteLine(encryptStr);

        var decryptStr = Sm2Helper.Decrypt(encryptStr, privateKey, privateKeyType: OutType.Base64,
            inputType: OutType.Hex);
        _testOutputHelper.WriteLine(decryptStr);
        Assert.Equal(source, decryptStr);
    }

    /// <summary>
    /// 外部密钥(base64格式)加解密
    /// </summary>
    [Fact]
    public void ExternalKey_Encrypt_ReturnOk()
    {
        var publicKey = "BGe1BZDFN+NhCQtc2qlVk8nUlXrIwcyjT3mMt7Xx3BkDNBGBQjFPV0+h3/cGUYXo2TFI1SShS7hWl9zi6SxUHvg=";
        var privateKey = "Ja4UIUJz7XRNDhIiuWXwL78qd1Pc7SC0/Z9LzyF4SL8=";

        var source = "123456";
        var encryptStr = Sm2Helper.Encrypt(source, publicKey, publicKeyType: OutType.Base64,
            outType: OutType.Hex);
        _testOutputHelper.WriteLine(encryptStr);

        var decryptStr = Sm2Helper.Decrypt(encryptStr, privateKey, privateKeyType: OutType.Base64,
            inputType: OutType.Hex);
        _testOutputHelper.WriteLine(decryptStr);
        Assert.Equal(source, decryptStr);
    }

    /// <summary>
    /// 固定密文解密 报错
    /// </summary>
    [Fact]
    public void EncryptText_Base64_Decrypt_ReturnOk()
    {
        var publicKey = "BGe1BZDFN+NhCQtc2qlVk8nUlXrIwcyjT3mMt7Xx3BkDNBGBQjFPV0+h3/cGUYXo2TFI1SShS7hWl9zi6SxUHvg=";
        var privateKey = "Ja4UIUJz7XRNDhIiuWXwL78qd1Pc7SC0/Z9LzyF4SL8=";

        var source = "质控规则类型权重";
        var encryptStr = "BKRyLeNC5GdRBmoX+VKEsw0JLps7iDGanLkjUmEMMAwDPqpFDC2nGG4GHEowGr7DjAv2sElazKfhmeq7TDcpXxN4+/bopvD74G3HgEYo6AAk3Dt2X+7MbvADbQpc2tKzfqJSLguUp+hZElSxN0X/d0sX+CrvkVQj+g==";

        var decryptStr = Sm2Helper.Decrypt(encryptStr, privateKey, privateKeyType: OutType.Base64,
            inputType: OutType.Base64);
        _testOutputHelper.WriteLine(decryptStr);
        Assert.Equal(source, decryptStr);
    }

    /// <summary>
    /// 固定密文解密
    /// </summary>
    [Fact]
    public void EncryptText_Hex_Decrypt_ReturnOk()
    {
        var publicKey = "040BD5027180C3185F46B1D93B35ACB0161CA8C933CC8DB36AA0664AE00C313C47B60FEA642C02CB13903A90976CD8BD93D2B3BB7AC8D5920B3EA7DD659E7E21AD";
        var privateKey = "2A8CAA0EB4F718CC7C1F98546DDAAA52799D35126323B634FB4A7C899953CEA1";

        var source = "质控规则类型权重";
        var encryptStr = "04B89A62D77933D66A73D0A33C761B9BE924028684A2EB87ACB145B9B3B0E02E09B954A1693938F99055FCE1AB76B6EB5F98343637375F5B7B76D173D0BD38EBF34618DDBD9ADF6967B4A7464000F3AEA8656A634E36C083301134B2E1531F0E49A918F462C33C621681EF787DD64CF804943A3ADF532879C4";

        var decryptStr = Sm2Helper.Decrypt(encryptStr, privateKey, privateKeyType: OutType.Hex,
            inputType: OutType.Hex);
        _testOutputHelper.WriteLine(decryptStr);
        Assert.Equal(source, decryptStr);
    }
}