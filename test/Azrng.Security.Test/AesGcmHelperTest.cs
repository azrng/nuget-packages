using Azrng.Security.Enums;
using System.Security.Cryptography;

namespace Azrng.Security.Test;

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

    [Fact]
    public void LegacyAesApi_ForwardToAesHelper_ReturnOk()
    {
        var source = "legacy-aes-forward";
        var (secret, iv) = AesHelper.ExportSecretAndIv();

#pragma warning disable CS0618
        var cipher = AesGcmHelper.Encrypt(source, secret, iv, CipherMode.CBC, PaddingMode.PKCS7);
        var plain = AesGcmHelper.Decrypt(cipher, secret, iv, CipherMode.CBC, PaddingMode.PKCS7);
#pragma warning restore CS0618

        Assert.Equal(source, plain);
    }

    [Fact]
    public void Decrypt_Bytes_RoundTrip_ReturnPlain()
    {
        var (secret, _) = AesHelper.ExportSecretAndIv();
        var keyBytes = Convert.FromBase64String(secret);
        var (cipher, nonce, tag) = AesGcmHelper.EncryptToParts("hello", secret, outType: OutType.Base64);

        var plain = AesGcmHelper.Decrypt(
            Convert.FromBase64String(cipher),
            Convert.FromBase64String(nonce),
            Convert.FromBase64String(tag),
            keyBytes);

        Assert.Equal("hello", plain);
    }

    [Fact]
    public void Decrypt_Bytes_InvalidTagSize_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AesGcmHelper.Decrypt(new byte[10], new byte[AesGcmHelper.NonceSize], new byte[5], new byte[16], tagSize: 5));
    }

    [Fact]
    public void Decrypt_Bytes_InvalidNonceSize_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            AesGcmHelper.Decrypt(new byte[10], new byte[3], new byte[AesGcmHelper.DefaultTagSize], new byte[16]));
    }

    [Fact]
    public void Decrypt_Bytes_InvalidKeySize_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            AesGcmHelper.Decrypt(new byte[10], new byte[AesGcmHelper.NonceSize], new byte[AesGcmHelper.DefaultTagSize], new byte[7]));
    }
}
