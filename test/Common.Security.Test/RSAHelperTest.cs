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

    [Fact]
    public void LongText_EncryptAndDecrypt_ReturnOk()
    {
        var source = new string('A', 1024);
        var (publicKey, privateKey) = RsaHelper.ExportPemRsaKey(RsaKeyFormat.PKCS1);

        var encryptResult = RsaHelper.Encrypt(source, publicKey);
        var decryptResult = RsaHelper.Decrypt(encryptResult, privateKey, privateKeyFormat: RsaKeyFormat.PKCS1);

        Assert.Equal(source, decryptResult);
    }

    [Fact]
    public void Pkcs8SignAndVerify_ReturnOk()
    {
        var source = "pkcs8-sign-verify";
        var (publicKey, privateKey) = RsaHelper.ExportBase64RsaKey();

        var signData = RsaHelper.SignData(source, privateKey, HashAlgorithmName.SHA256, privateKeyType: OutType.Base64);
        var verifyResult = RsaHelper.VerifyData(source, signData, publicKey, HashAlgorithmName.SHA256, publicKeyType: OutType.Base64);

        Assert.True(verifyResult);
    }

    [Fact]
    public void OaepSha256_LongText_EncryptAndDecrypt_ReturnOk()
    {
        var source = new string('B', 1024);
        var (publicKey, privateKey) = RsaHelper.ExportPemRsaKey(RsaKeyFormat.PKCS1);

        var encryptResult = RsaHelper.EncryptOaepSha256(source, publicKey);
        var decryptResult = RsaHelper.DecryptOaepSha256(encryptResult, privateKey, privateKeyFormat: RsaKeyFormat.PKCS1);

        Assert.Equal(source, decryptResult);
    }

    [Fact]
    public void SignAndVerify_Pss_ReturnOk()
    {
        var source = "pss-sign-verify";
        var (publicKey, privateKey) = RsaHelper.ExportBase64RsaKey();

        var signData = RsaHelper.SignDataPss(source, privateKey, HashAlgorithmName.SHA256, privateKeyType: OutType.Base64);
        var verifyResult = RsaHelper.VerifyDataPss(source, signData, publicKey, HashAlgorithmName.SHA256, publicKeyType: OutType.Base64);

        Assert.True(verifyResult);
    }

    [Fact]
    public void QuickSignAndQuickVerify_Pss_ReturnOk()
    {
        var source = "quick-pss";
        var (publicKey, privateKey) = RsaHelper.ExportBase64RsaKey();

        var signData = RsaHelper.QuickSignPss(source, privateKey);
        var verifyResult = RsaHelper.QuickVerifyPss(source, signData, publicKey);

        Assert.True(verifyResult);
    }

    [Fact]
    public void QuickVerify_Pss_TamperedData_ReturnFalse()
    {
        var source = "quick-pss-source";
        var (publicKey, privateKey) = RsaHelper.ExportBase64RsaKey();
        var signData = RsaHelper.QuickSignPss(source, privateKey);

        var verifyResult = RsaHelper.QuickVerifyPss(source + "-x", signData, publicKey);

        Assert.False(verifyResult);
    }
}
