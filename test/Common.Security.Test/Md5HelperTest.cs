using Xunit.Abstractions;

namespace Common.Security.Test
{
    public class Md5HelperTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public Md5HelperTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData("123456", false, "E10ADC3949BA59ABBE56E057F20F883E")]
        [InlineData("123456", true, "49BA59ABBE56E057")]
        [InlineData("", false, "D41D8CD98F00B204E9800998ECF8427E")]
        [InlineData("", true, "8F00B204E9800998")]
        [InlineData("abcABC123", false, "480AEB42D7B1E3937FE8DB12A1FFE6D8")]
        [InlineData("abcABC123", true, "D7B1E3937FE8DB12")]
        [InlineData("!@#$%^&*()", false, "05B28D17A7B6E7024B6E5D8CC43A8BF7")]
        [InlineData("!@#$%^&*()", true, "A7B6E7024B6E5D8C")]
        public void StringHash_ReturnOk(string str, bool is16, string value)
        {
            var result = Md5Helper.GetMd5Hash(str, is16: is16);
            _testOutputHelper.WriteLine(result);

            Assert.Equal(value, result);
        }

        [Theory]
        [InlineData("123456", "123456", "30CE71A73BDD908C3955A90E8F7429EF")]
        [InlineData("", "", "74E6F7298A9C2D168935F58C001BAD88")]
        [InlineData("abc", "def", "FDDBD02C4CAD1EC0B38A5E8C8720B3BC")]
        [InlineData("test", "key", "1D4A2743C056E467FF3F09C9AF31DE7E")]
        [InlineData("!@#", "!@#", "2EB0713A9CAF8039F7AF9ABA17AEB1AA")]
        public void HMACHash_ReturnOk(string str, string secret, string value)
        {
            var result = Md5Helper.GetHmacMd5Hash(str, secret);
            _testOutputHelper.WriteLine(result);

            Assert.Equal(value, result);
        }
    }
}