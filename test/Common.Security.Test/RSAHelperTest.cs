using Common.Security.Enums;
using System.Security.Cryptography;

namespace Common.Security.Test;

/// <summary>
/// rsa 单元测试
/// </summary>
public class RsaHelperTest
{
    [Fact]
    public void ExportXmlKey_Test()
    {
        var (publicKey, privateKey) = RsaHelper.ExportXmlRsaKey();
        Assert.NotEmpty(publicKey);
        Assert.NotEmpty(privateKey);
    }

    /// <summary>
    /// Pkcs1格式PrivateKey加解密操作
    /// </summary>
    [Fact]
    public void Pkcs1PrivateKey_Encrypt_ReturnOk()
    {
        var (publicKey, privateKey) = RsaHelper.ExportPemRsaKey(RsaKeyFormat.PKCS1);
        var source = "13456789";

        var encryptResult = RsaHelper.Encrypt(source, publicKey);
        Assert.NotEmpty(encryptResult);

        var decryptResult = RsaHelper.Decrypt(encryptResult, privateKey, privateKeyFormat: RsaKeyFormat.PKCS1);
        Assert.NotEmpty(decryptResult);

        Assert.Equal(source, decryptResult);
    }

    /// <summary>
    /// Pkcs8格式PrivateKey加解密操作
    /// </summary>
    [Fact]
    public void Pkcs8PrivateKey_Encrypt_ReturnOk()
    {
        var (publicKey, privateKey) = RsaHelper.ExportPemRsaKey();
        var source = "13456789";

        var encryptResult = RsaHelper.Encrypt(source, publicKey);
        Assert.NotEmpty(encryptResult);

        var decryptResult = RsaHelper.Decrypt(encryptResult, privateKey, privateKeyFormat: RsaKeyFormat.PKCS8);
        Assert.NotEmpty(decryptResult);

        Assert.Equal(source, decryptResult);
    }

    /// <summary>
    /// 使用网络上固定的key进行加密解密
    /// </summary>
    [Fact]
    public void FixedKey_Encrypt_ReturnOk()
    {
        var sourceStr = "itzhangyunpeng@163.com";

        var result1 = RsaHelper.Encrypt(sourceStr, SecurityConst.RSAPublicKey);

        var result3 = RsaHelper.Decrypt(result1, SecurityConst.RSAPrivateKey, privateKeyFormat: RsaKeyFormat.PKCS1);

        Assert.Equal(sourceStr, result3);
    }

    [InlineData("123456789")]
    [InlineData("张三")]
    [InlineData("abcdadjfhsdfds")]
    [Theory]
    public void TestEncrypt_ResultOk(string data)
    {
        var encryptResult = RsaHelper.Encrypt(data, SecurityConst.RSAPublicKey);
        Assert.NotEmpty(encryptResult);
    }

    [Fact]
    public void SignData_ReturnOk()
    {
        var source = "123456789987654321";
        var (publicKey, privateKey) = RsaHelper.ExportPemRsaKey(RsaKeyFormat.PKCS1);
        var signData = RsaHelper.SignData(source, privateKey, HashAlgorithmName.SHA1);
        Assert.NotEmpty(signData);

        var verifyResult = RsaHelper.VerifyData(source, signData, publicKey, HashAlgorithmName.SHA1);
        Assert.True(verifyResult);
    }
}