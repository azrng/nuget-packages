using Common.Security.Enums;
using System.Security.Cryptography;

namespace Common.Security.Test;

public class AesGcmHelperTest
{
    [Fact]
    public void EncryptDecrypt_Combined_Base64_ReturnOk()
    {
        var source = "hello aes gcm";
        var (secret, _) = AesHelper.ExportSecretAndIv();

        var cipher = AesGcmHelper.Encrypt(
            plainText: source,
            secretKey: secret,
            secretType: SecretType.Base64,
            outType: OutType.Base64,
            tagSize: AesGcmHelper.DefaultTagSize);
        var plain = AesGcmHelper.Decrypt(
            cipherCombined: cipher,
            secretKey: secret,
            secretType: SecretType.Base64,
            cipherTextType: OutType.Base64,
            tagSize: AesGcmHelper.DefaultTagSize);

        Assert.Equal(source, plain);
    }

    [Fact]
    public void EncryptDecrypt_Parts_Base64_ReturnOk()
    {
        var source = "hello aes gcm parts";
        var (secret, _) = AesHelper.ExportSecretAndIv();

        var (cipher, nonce, tag) = AesGcmHelper.EncryptToParts(source, secret, outType: OutType.Base64);
        var plain = AesGcmHelper.DecryptFromParts(cipher, nonce, tag, secret, cipherTextType: OutType.Base64);

        Assert.Equal(source, plain);
    }

    [Fact]
    public void Decrypt_TamperedTag_ThrowsCryptographicException()
    {
        var source = "hello aes gcm tamper";
        var (secret, _) = AesHelper.ExportSecretAndIv();
        var cipher = AesGcmHelper.Encrypt(
            plainText: source,
            secretKey: secret,
            secretType: SecretType.Base64,
            outType: OutType.Base64,
            tagSize: AesGcmHelper.DefaultTagSize);

        var bytes = Convert.FromBase64String(cipher);
        bytes[^1] ^= 0x01;
        var tampered = Convert.ToBase64String(bytes);

        Assert.Throws<CryptographicException>(() => AesGcmHelper.Decrypt(
            cipherCombined: tampered,
            secretKey: secret,
            secretType: SecretType.Base64,
            cipherTextType: OutType.Base64,
            tagSize: AesGcmHelper.DefaultTagSize));
    }
}
