using Azrng.Security.Enums;

namespace Azrng.Security.Test;

public class Des3HelperTest
{
    private const string SecretKey = "123456789012345678901234";

    [Fact]
    public void EncryptDecrypt_Base64_ReturnOk()
    {
        var source = "hello 3des";
        var cipher = Des3Helper.Encrypt(source, SecretKey, OutType.Base64);
        var plain = Des3Helper.Decrypt(cipher, SecretKey, OutType.Base64);

        Assert.Equal(source, plain);
    }

    [Fact]
    public void EncryptDecrypt_Hex_ReturnOk()
    {
        var source = "hello 3des hex";
        var cipher = Des3Helper.Encrypt(source, SecretKey, OutType.Hex);
        var plain = Des3Helper.Decrypt(cipher, SecretKey, OutType.Hex);

        Assert.Equal(source, plain);
    }

    [Fact]
    public void Encrypt_NullPlaintext_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Des3Helper.Encrypt(null!, SecretKey, OutType.Base64));
    }
}
