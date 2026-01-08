namespace Common.Core.Test.Helper.StringHelperTest
{
    public class CompressTextTest
    {
        [Fact]
        public void CompressText_RemovesLineBreaksAndTrims()
        {
            var input = "  Hello \r\n World \n ";
            var result = StringHelper.CompressText(input);
            Assert.Equal("Hello World", result);
        }

        [Fact]
        public void CompressText_CollapsesMultipleWhiteSpaceSegments()
        {
            var input = "A    B\t\tC   D";
            var result = StringHelper.CompressText(input);
            Assert.Equal("A B C D", result);
        }

        [Fact]
        public void CompressText_ReturnsOriginalWhenAlreadyCompressed()
        {
            var input = "AlreadyClean";
            var result = StringHelper.CompressText(input);
            Assert.Equal("AlreadyClean", result);
        }
    }
}
