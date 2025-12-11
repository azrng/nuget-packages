using Common.SecurityCrypto.Model;
using Xunit;
using Xunit.Abstractions;

namespace Common.SecurityCrypto.Test
{
    /// <summary>
    /// 对称加密测试
    /// </summary>
    public class SymmetryCryptoTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public SymmetryCryptoTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Sm4_Base64加解密()
        {
            var crypto = new SM4Encryption(OutType.Base64);
            var value = "abcd";
            var key = "JeF8U9wHFOMfs2Y8";
            var encryptValue = crypto.Encrypt(value, key);
            _testOutputHelper.WriteLine(encryptValue);
            Assert.Equal("+0TiZl/YuV7/DC7InSC6MQ==", encryptValue);

            var sourceValue = crypto.Decrypt(encryptValue, key);
            Assert.Equal(sourceValue, value);
        }

        [Fact]
        public void Sm4_Base64加解密并替换值()
        {
            // 说明：因为考虑到base64的+号通过URL传递时会变成空格等，所以部分值需要替换处理
            var crypto = new SM4Encryption(OutType.Base64);
            var value = "abcd";
            var key = "JeF8U9wHFOMfs2Y8";
            var encryptValue = crypto.Encrypt(value, key);
            encryptValue = encryptValue.Trim()
                .Replace("\r", "")
                .Replace("\n", "")
                .Replace("+", "%2B")
                .Replace("/", "%2F");
            _testOutputHelper.WriteLine(encryptValue);
            Assert.Equal("%2B0TiZl%2FYuV7%2FDC7InSC6MQ==", encryptValue);

            encryptValue = encryptValue.Replace("%2B", "+")
                .Replace("%2F", "/");
            var sourceValue = crypto.Decrypt(encryptValue, key);

            Assert.Equal(sourceValue, value);
        }

        [Fact]
        public void Des3_Base64加解密()
        {
            // 说明：因为考虑到base64的+号通过URL传递时会变成空格等，所以部分值需要替换处理
            var crypto = new DES3Encryption(OutType.Base64);
            var value = "abcdsfsasfs";
            var key = "JeF8U9wHFOMfs2Y8";
            var encryptValue = crypto.Encrypt(value, key);

            _testOutputHelper.WriteLine(encryptValue);
            Assert.Equal("GXnw/D8Lvjm8TVJ13nG+9A==", encryptValue);

            var sourceValue = crypto.Decrypt(encryptValue, key);

            Assert.Equal(sourceValue, value);
        }
    }
}