using System;
using System.Security.Cryptography;
using Azrng.Security.Extensions;
using Xunit;

namespace Azrng.Security.Test
{
    public class RsaKeyJsonTest
    {
        [Fact]
        public void ToJson_AndFromJson_RestoresPrivateKeyParameters()
        {
            using var rsa = RSA.Create(2048);
            var expected = rsa.ExportParameters(true);
            var json = rsa.ToJsonString(true);

            using var restored = RSA.Create(2048);
            restored.FromJsonString(json);
            var actual = restored.ExportParameters(true);

            Assert.Equal(expected.D, actual.D);
            Assert.Equal(expected.Modulus, actual.Modulus);
        }

        [Fact]
        public void FromJsonString_NullOrWhitespace_Throws()
        {
            using var rsa = RSA.Create(2048);
            Assert.Throws<ArgumentNullException>(() => rsa.FromJsonString(""));
        }
    }
}
