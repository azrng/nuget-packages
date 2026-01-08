namespace Common.Core.Test.Helper.StringHelperTest
{
    public class UnicodeToTextTest
    {
        [Fact]
        public void UnicodeToText_ConvertsEscapedSequence()
        {
            var input = @"\u0041\u0062";
            var result = StringHelper.UnicodeToText(input);
            Assert.Equal("Ab", result);
        }

        [Fact]
        public void UnicodeToText_PreservesCharactersOutsideUnicodeSequence()
        {
            var input = @"prefix\u0041suffix";
            var result = StringHelper.UnicodeToText(input);
            Assert.Equal("prefixAsuffix", result);
        }

        [Fact]
        public void UnicodeToText_TreatsHexCodesCaseInsensitively()
        {
            var input = @"\u0041\u0062\u4F60";
            var result = StringHelper.UnicodeToText(input);
            Assert.Equal("Ab你", result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void UnicodeToText_NullOrEmpty_ReturnsEmpty(string input)
        {
            var result = StringHelper.UnicodeToText(input);
            Assert.Equal(string.Empty, result);
        }
    }
}
