using Common.Security.Enums;

namespace Common.Security.Test;

public class Des3HelperTest
{
    private const string SecretKey = "123456789012345678901234";
    private readonly Des3Helper _helper = new();

    [Fact]
    public void EncryptDecrypt_Base64_ReturnOk()
    {
        var source = "hello 3des";
        var cipher = _helper.Encrypt(source, SecretKey, OutType.Base64);
        var plain = _helper.Decrypt(cipher, SecretKey, OutType.Base64);

        Assert.Equal(source, plain);
    }

    [Fact]
    public void EncryptDecrypt_Hex_ReturnOk()
    {
        var source = "hello 3des hex";
        var cipher = _helper.Encrypt(source, SecretKey, OutType.Hex);
        var plain = _helper.Decrypt(cipher, SecretKey, OutType.Hex);

        Assert.Equal(source, plain);
    }
}
