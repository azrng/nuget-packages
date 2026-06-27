using Azrng.Security.Enums;
using FluentAssertions;

namespace Azrng.Security.Test
{
    public class Md5HelperTests
    {
        [Theory]
        [InlineData("123456", false, "E10ADC3949BA59ABBE56E057F20F883E")]
        [InlineData("123456", true, "49BA59ABBE56E057")]
        [InlineData("", false, "D41D8CD98F00B204E9800998ECF8427E")]
        [InlineData("", true, "8F00B204E9800998")]
        [InlineData("abcABC123", false, "480AEB42D7B1E3937FE8DB12A1FFE6D8")]
        [InlineData("abcABC123", true, "D7B1E3937FE8DB12")]
        [InlineData("!@#$%^&*()", false, "05B28D17A7B6E7024B6E5D8CC43A8BF7")]
        [InlineData("!@#$%^&*()", true, "A7B6E7024B6E5D8C")]
        public void GetMd5Hash_VariousInputs_ReturnsExpected(string input, bool is16, string expected)
        {
            var result = Md5Helper.GetMd5Hash(input, is16: is16);

            result.Should().Be(expected);
        }

        [Fact]
        public void GetMd5Hash_Null_ThrowsArgumentNullException()
        {
            Action act = () => Md5Helper.GetMd5Hash(null!);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("plaintext");
        }

        [Fact]
        public void GetMd5Hash_DefaultOutputType_ReturnsHexUppercase()
        {
            var result = Md5Helper.GetMd5Hash("test");

            result.Should().MatchRegex("^[0-9A-F]{32}$");
        }

        [Fact]
        public void GetMd5Hash_Base64OutputType_ReturnsBase64()
        {
            var result = Md5Helper.GetMd5Hash("test", outputType: OutType.Base64);

            result.Should().NotBeNullOrWhiteSpace();
            Action decode = () => Convert.FromBase64String(result);
            decode.Should().NotThrow();
        }

        [Fact]
        public void GetMd5Hash_SameInput_ReturnsConsistentResult()
        {
            var input = "consistency-check";

            var result1 = Md5Helper.GetMd5Hash(input);
            var result2 = Md5Helper.GetMd5Hash(input);

            result1.Should().Be(result2);
        }

        [Theory]
        [InlineData("123456", "123456", "30CE71A73BDD908C3955A90E8F7429EF")]
        [InlineData("", "", "74E6F7298A9C2D168935F58C001BAD88")]
        [InlineData("abc", "def", "FDDBD02C4CAD1EC0B38A5E8C8720B3BC")]
        [InlineData("test", "key", "1D4A2743C056E467FF3F09C9AF31DE7E")]
        [InlineData("!@#", "!@#", "2EB0713A9CAF8039F7AF9ABA17AEB1AA")]
        public void GetHmacMd5Hash_VariousInputs_ReturnsExpected(string plaintext, string secret, string expected)
        {
            var result = Md5Helper.GetHmacMd5Hash(plaintext, secret);

            result.Should().Be(expected);
        }

        [Fact]
        public void GetHmacMd5Hash_NullPlaintext_ThrowsArgumentNullException()
        {
            Action act = () => Md5Helper.GetHmacMd5Hash(null!, "secret");

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("plaintext");
        }

        [Fact]
        public void GetHmacMd5Hash_NullSecret_ThrowsArgumentNullException()
        {
            Action act = () => Md5Helper.GetHmacMd5Hash("test", null!);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("secret");
        }

        [Fact]
        public void GetHmacMd5Hash_Base64OutputType_ReturnsValidBase64()
        {
            var result = Md5Helper.GetHmacMd5Hash("test", "key", OutType.Base64);

            result.Should().NotBeNullOrWhiteSpace();
            Convert.FromBase64String(result).Should().HaveCount(16);
        }

        [Fact]
        public void GetFileMd5Hash_ValidFile_ReturnsExpected()
        {
            var filePath = Path.GetTempFileName();
            try
            {
                File.WriteAllText(filePath, "123456");
                var result = Md5Helper.GetFileMd5Hash(filePath);

                result.Should().Be("E10ADC3949BA59ABBE56E057F20F883E");
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public void GetFileMd5Hash_NullPath_ThrowsArgumentNullException()
        {
            Action act = () => Md5Helper.GetFileMd5Hash(null!);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("path");
        }

        [Fact]
        public void GetFileMd5Hash_NonExistentFile_ThrowsFileNotFoundException()
        {
            Action act = () => Md5Helper.GetFileMd5Hash("non_existent_file_12345.txt");

            act.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public void GetFileMd5Hash_EmptyFile_ReturnsEmptyFileHash()
        {
            var filePath = Path.GetTempFileName();
            try
            {
                var result = Md5Helper.GetFileMd5Hash(filePath);

                result.Should().Be("D41D8CD98F00B204E9800998ECF8427E");
            }
            finally
            {
                File.Delete(filePath);
            }
        }
    }
}
