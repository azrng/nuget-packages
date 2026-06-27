using Azrng.Security.Enums;
using FluentAssertions;
using System.Text;

namespace Azrng.Security.Test
{
    public class SHAHelperTests
    {
        [Fact]
        public void GetSha1Hash_ReturnsExpected()
        {
            var result = ShaHelper.GetSha1Hash("123456");

            result.Should().Be("7C4A8D09CA3762AF61E59520943DC26494F8941B");
        }

        [Fact]
        public void GetSha1Hash_Null_ThrowsArgumentNullException()
        {
            Action act = () => ShaHelper.GetSha1Hash(null!);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("str");
        }

        [Fact]
        public void GetSha1Hash_EmptyString_ReturnsHash()
        {
            var result = ShaHelper.GetSha1Hash("");

            result.Should().HaveLength(40);
        }

        [Fact]
        public void GetSha1Hash_Base64Output_ReturnsValidBase64()
        {
            var result = ShaHelper.GetSha1Hash("test", OutType.Base64);

            Convert.FromBase64String(result).Should().HaveCount(20);
        }

        [Fact]
        public void GetSha256Hash_ReturnsExpected()
        {
            var result = ShaHelper.GetSha256Hash("123456");

            result.Should().Be("8D969EEF6ECAD3C29A3A629280E686CF0C3F5D5A86AFF3CA12020C923ADC6C92");
        }

        [Fact]
        public void GetSha256Hash_Null_ThrowsArgumentNullException()
        {
            Action act = () => ShaHelper.GetSha256Hash((string)null!);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("str");
        }

        [Fact]
        public void GetSha256Hash_Stream_ReturnsExpected()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("123456"));

            var result = ShaHelper.GetSha256Hash(stream);

            result.Should().Be("8D969EEF6ECAD3C29A3A629280E686CF0C3F5D5A86AFF3CA12020C923ADC6C92");
        }

        [Fact]
        public void GetSha256Hash_NullStream_ThrowsArgumentNullException()
        {
            Action act = () => ShaHelper.GetSha256Hash((Stream)null!);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("stream");
        }

        [Fact]
        public void GetSha512Hash_ReturnsExpected()
        {
            var result = ShaHelper.GetSha512Hash("123456");

            result.Should().Be("BA3253876AED6BC22D4A6FF53D8406C6AD864195ED144AB5C87621B6C233B548BAEAE6956DF346EC8C17F5EA10F35EE3CBC514797ED7DDD3145464E2A0BAB413");
        }

        [Fact]
        public void GetSha512Hash_Null_ThrowsArgumentNullException()
        {
            Action act = () => ShaHelper.GetSha512Hash((string)null!);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("str");
        }

        [Fact]
        public void GetSha512Hash_Stream_ReturnsExpected()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("123456"));

            var result = ShaHelper.GetSha512Hash(stream);

            result.Should().Be("BA3253876AED6BC22D4A6FF53D8406C6AD864195ED144AB5C87621B6C233B548BAEAE6956DF346EC8C17F5EA10F35EE3CBC514797ED7DDD3145464E2A0BAB413");
        }

        [Fact]
        public void GetSha512Hash_NullStream_ThrowsArgumentNullException()
        {
            Action act = () => ShaHelper.GetSha512Hash((Stream)null!);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("stream");
        }

        [Fact]
        public void GetHmacSha1Hash_ReturnsExpected()
        {
            var result = ShaHelper.GetHmacSha1Hash("987654321", "123456");

            result.Should().Be("145B9726076579A02B61B0085397100F9594F398");
        }

        [Fact]
        public void GetHmacSha1Hash_NullPlaintext_ThrowsArgumentNullException()
        {
            Action act = () => ShaHelper.GetHmacSha1Hash(null!, "secret");

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("str");
        }

        [Fact]
        public void GetHmacSha1Hash_NullSecret_ThrowsArgumentNullException()
        {
            Action act = () => ShaHelper.GetHmacSha1Hash("test", null!);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("secret");
        }

        [Fact]
        public void GetHmacSha256Hash_ReturnsExpected()
        {
            var result = ShaHelper.GetHmacSha256Hash("987654321", "123456");

            result.Should().Be("E366E9660A2DA262EF72049C830A029495CE8EEBA5BE544BA6D3328397958267");
        }

        [Fact]
        public void GetHmacSha256Hash_NullPlaintext_ThrowsArgumentNullException()
        {
            Action act = () => ShaHelper.GetHmacSha256Hash(null!, "secret");

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("str");
        }

        [Fact]
        public void GetHmacSha256Hash_NullSecret_ThrowsArgumentNullException()
        {
            Action act = () => ShaHelper.GetHmacSha256Hash("test", null!);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("secret");
        }

        [Fact]
        public void GetHmacSha512Hash_ReturnsExpected()
        {
            var result = ShaHelper.GetHmacSha512Hash("987654321", "123456");

            result.Should().Be("5447EF22593E971CC3B7B1E33BE8B63A5C47C1F3CEFF94EA2D0A56E46894CBC46AF30DF2FD6CCAE34DCA1AB5CA24E0FBA0247C452B21B237C40FC347D000537B");
        }

        [Fact]
        public void GetHmacSha512Hash_NullPlaintext_ThrowsArgumentNullException()
        {
            Action act = () => ShaHelper.GetHmacSha512Hash(null!, "secret");

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("str");
        }

        [Fact]
        public void GetHmacSha512Hash_NullSecret_ThrowsArgumentNullException()
        {
            Action act = () => ShaHelper.GetHmacSha512Hash("test", null!);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("secret");
        }

        [Fact]
        public void VerifyHmacSha1Hash_ValidHash_ReturnsTrue()
        {
            var hash = ShaHelper.GetHmacSha1Hash("verify-sha1", "s1", OutType.Base64);

            var result = ShaHelper.VerifyHmacSha1Hash("verify-sha1", "s1", hash, OutType.Base64);

            result.Should().BeTrue();
        }

        [Fact]
        public void VerifyHmacSha1Hash_InvalidHash_ReturnsFalse()
        {
            var result = ShaHelper.VerifyHmacSha1Hash("verify-sha1", "s1",
                Convert.ToBase64String(new byte[20]), OutType.Base64);

            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyHmacSha1Hash_NullStr_ThrowsArgumentNullException()
        {
            Action act = () => ShaHelper.VerifyHmacSha1Hash(null!, "secret", "hash");

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("str");
        }

        [Fact]
        public void VerifyHmacSha1Hash_NullSecret_ThrowsArgumentNullException()
        {
            Action act = () => ShaHelper.VerifyHmacSha1Hash("test", null!, "hash");

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("secret");
        }

        [Fact]
        public void VerifyHmacSha1Hash_NullHash_ThrowsArgumentNullException()
        {
            Action act = () => ShaHelper.VerifyHmacSha1Hash("test", "secret", null!);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("hash");
        }

        [Fact]
        public void VerifyHmacSha256Hash_ValidHash_ReturnsTrue()
        {
            var hash = ShaHelper.GetHmacSha256Hash("verify-sha256", "s2", OutType.Base64);

            var result = ShaHelper.VerifyHmacSha256Hash("verify-sha256", "s2", hash, OutType.Base64);

            result.Should().BeTrue();
        }

        [Fact]
        public void VerifyHmacSha256Hash_InvalidHash_ReturnsFalse()
        {
            var result = ShaHelper.VerifyHmacSha256Hash("verify-sha256", "s2",
                Convert.ToBase64String(new byte[32]), OutType.Base64);

            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyHmacSha256Hash_NullHash_ThrowsArgumentNullException()
        {
            Action act = () => ShaHelper.VerifyHmacSha256Hash("test", "secret", null!);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("hash");
        }

        [Fact]
        public void VerifyHmacSha512Hash_ValidHash_ReturnsTrue()
        {
            var hash = ShaHelper.GetHmacSha512Hash("verify-sha512", "s3", OutType.Base64);

            var result = ShaHelper.VerifyHmacSha512Hash("verify-sha512", "s3", hash, OutType.Base64);

            result.Should().BeTrue();
        }

        [Fact]
        public void VerifyHmacSha512Hash_InvalidHash_ReturnsFalse()
        {
            var result = ShaHelper.VerifyHmacSha512Hash("verify-sha512", "s3",
                Convert.ToBase64String(new byte[64]), OutType.Base64);

            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyHmacSha512Hash_NullHash_ThrowsArgumentNullException()
        {
            Action act = () => ShaHelper.VerifyHmacSha512Hash("test", "secret", null!);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("hash");
        }

        [Fact]
        public void GetFileSha256Hash_ValidFile_ReturnsExpected()
        {
            var filePath = Path.GetTempFileName();
            try
            {
                File.WriteAllText(filePath, "123456");
                var result = ShaHelper.GetFileSha256Hash(filePath);

                result.Should().Be("8D969EEF6ECAD3C29A3A629280E686CF0C3F5D5A86AFF3CA12020C923ADC6C92");
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public void GetFileSha256Hash_NullPath_ThrowsArgumentNullException()
        {
            Action act = () => ShaHelper.GetFileSha256Hash(null!);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("path");
        }

        [Fact]
        public void GetFileSha256Hash_NonExistentFile_ThrowsFileNotFoundException()
        {
            Action act = () => ShaHelper.GetFileSha256Hash("non_existent_file_12345.txt");

            act.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public void GetFileSha512Hash_ValidFile_ReturnsExpected()
        {
            var filePath = Path.GetTempFileName();
            try
            {
                File.WriteAllText(filePath, "123456");
                var result = ShaHelper.GetFileSha512Hash(filePath);

                result.Should().Be("BA3253876AED6BC22D4A6FF53D8406C6AD864195ED144AB5C87621B6C233B548BAEAE6956DF346EC8C17F5EA10F35EE3CBC514797ED7DDD3145464E2A0BAB413");
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public void GetFileSha512Hash_NullPath_ThrowsArgumentNullException()
        {
            Action act = () => ShaHelper.GetFileSha512Hash(null!);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("path");
        }

        [Fact]
        public void GetFileSha512Hash_NonExistentFile_ThrowsFileNotFoundException()
        {
            Action act = () => ShaHelper.GetFileSha512Hash("non_existent_file_12345.txt");

            act.Should().Throw<FileNotFoundException>();
        }

        [Theory]
        [InlineData(OutType.Hex)]
        [InlineData(OutType.Base64)]
        public void GetSha256Hash_DifferentOutputTypes_ReturnsValidFormat(OutType outputType)
        {
            var result = ShaHelper.GetSha256Hash("test", outputType);

            result.Should().NotBeNullOrWhiteSpace();
            if (outputType == OutType.Hex)
                result.Should().HaveLength(64);
            else
                Convert.FromBase64String(result).Should().HaveCount(32);
        }
    }
}
