namespace Common.Core.Test.Helper.StringHelperTest
{
    public class TextToUnicodeTest
    {
        [Fact]
        public void TextToUnicode_ConvertsLettersToUnicodeSequence()
        {
            var result = StringHelper.TextToUnicode("Ab");
            Assert.Equal("\\u0041\\u0062", result);
        }

        [Fact]
        public void TextToUnicode_ConvertsNonAsciiCharacters()
        {
            var result = StringHelper.TextToUnicode("你好");
            Assert.Equal("\\u4f60\\u597d", result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void TextToUnicode_NullOrEmpty_ReturnsEmpty(string input)
        {
            var result = StringHelper.TextToUnicode(input);
            Assert.Equal(string.Empty, result);
        }
    }
}
