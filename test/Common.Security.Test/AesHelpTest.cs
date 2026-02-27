using Common.Security.Enums;
using System.Security.Cryptography;
using Xunit.Abstractions;

namespace Common.Security.Test
{
    public class AesHelpTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public AesHelpTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void ExportSecretAndIv_ReturnOk()
        {
            var (secretKey, iv) = AesHelper.ExportSecretAndIv(OutType.Hex);
            _testOutputHelper.WriteLine(secretKey);
            _testOutputHelper.WriteLine(iv);
        }

        [Fact]
        public void ExportSecretAndIv_WithKeySize_ReturnExpectedLength()
        {
            var (secretKey, iv) = AesHelper.ExportSecretAndIv(256, OutType.Base64);
            Assert.Equal(32, Convert.FromBase64String(secretKey).Length);
            Assert.Equal(16, Convert.FromBase64String(iv).Length);
        }

        [Fact]
        public void ExportSecretAndIv_WithInvalidKeySize_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => AesHelper.ExportSecretAndIv(111, OutType.Base64));
        }

        [Fact]
        public void CbcEncryptAndDecrypt_Base64_ReturnOk()
        {
            var sourceStr = "hello world";
            var secretKey = "lB2BxrJdI4UUjK3KEZyQ0obuSgavB1SYJuAFq9oVw0Y=";
            var iv = "6lra6ceX26Fazwj1R4PCOg==";

            var encryptStr = AesHelper.Encrypt(sourceStr, secretKey, iv, CipherMode.CBC, PaddingMode.PKCS7);
            _testOutputHelper.WriteLine(encryptStr);
            Assert.Equal("D7vwJ1qlOSnosfBGyYdObg==", encryptStr, StringComparer.CurrentCultureIgnoreCase);

            var decryptStr = AesHelper.Decrypt(encryptStr, secretKey, iv, CipherMode.CBC, PaddingMode.PKCS7);
            _testOutputHelper.WriteLine(decryptStr);
            Assert.Equal(sourceStr, decryptStr);
        }

        [Fact]
        public void CbcEncryptAndDecrypt_Hex_ReturnOk()
        {
            var sourceStr = "hello world";
            var secretKey = "3b0f53fa1ebb24d40f8aefa9522e1c7dc0ed1517e4a12986d10d61edd220ad59";
            var iv = "e63ba3b2af7c02620dd5b5585b63f540";

            var encryptStr = AesHelper.Encrypt(sourceStr, secretKey, iv, CipherMode.CBC, PaddingMode.PKCS7,
                secretType: SecretType.Hex, outType: OutType.Hex);
            _testOutputHelper.WriteLine(encryptStr);
            Assert.Equal("66CB7AB55B0D19065C0A51E40364D5CE", encryptStr, StringComparer.CurrentCultureIgnoreCase);

            var decryptStr = AesHelper.Decrypt(encryptStr, secretKey, iv, CipherMode.CBC, PaddingMode.PKCS7,
                secretType: SecretType.Hex, cipherTextType: OutType.Hex);
            _testOutputHelper.WriteLine(decryptStr);
            Assert.Equal(sourceStr, decryptStr);
        }

        /// <summary>
        /// esb模式 偏移量：pkcs7 输出格式：base64
        /// </summary>
        [Fact]
        public void Ecb_Pkcs7_Base64_ReturnOk()
        {
            var sourceStr = "his";
            var secretKey = "879f803731774546";

            var encryptStr = AesHelper.Encrypt(sourceStr, secretKey, cipherMode: CipherMode.ECB,
                paddingMode: PaddingMode.PKCS7,
                secretType: SecretType.Text, outType: OutType.Base64);
            _testOutputHelper.WriteLine(encryptStr);
            Assert.Equal("3zP1LtIgKyjoD0Ndf2GI2Q==", encryptStr, StringComparer.CurrentCultureIgnoreCase);

            var decryptStr = AesHelper.Decrypt(encryptStr, secretKey, cipherMode: CipherMode.ECB, paddingMode: PaddingMode.PKCS7,
                secretType: SecretType.Text, cipherTextType: OutType.Base64);
            _testOutputHelper.WriteLine(decryptStr);
            Assert.Equal(sourceStr, decryptStr);
        }

        [Fact]
        public void EncryptCbcPkcs7AndDecrypt_ReturnOk()
        {
            var sourceStr = "secure-default-cbc";
            var (secretKey, iv) = AesHelper.ExportSecretAndIv();

            var encryptStr = AesHelper.EncryptCbcPkcs7(sourceStr, secretKey, iv);
            var decryptStr = AesHelper.DecryptCbcPkcs7(encryptStr, secretKey, iv);

            Assert.Equal(sourceStr, decryptStr);
        }
    }
}
