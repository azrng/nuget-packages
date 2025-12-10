namespace Common.Core.Test.Helper
{
    public class ChineseTextHelperTest
    {
        [Theory]
        [InlineData("Hello你好World", "你好")]
        [InlineData("123中文456", "中文")]
        [InlineData("abc中文def中文ghi", "中文def中文")]
        [InlineData("开始start中间end结束", "开始start中间end结束")]
        public void RemoveNonChineseFromStartAndEnd_ShouldRemoveNonChineseCharactersFromStartAndEnd(string input, string expected)
        {
            // Act
            var result = ChineseTextHelper.RemoveNonChineseFromStartAndEnd(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("123")]
        [InlineData("ABC")]
        [InlineData("!@#$%")]
        public void RemoveNonChineseFromStartAndEnd_ShouldReturnEmptyStringWhenNoChineseOrEmptyInput(string input)
        {
            // Act
            var result = ChineseTextHelper.RemoveNonChineseFromStartAndEnd(input);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Theory]
        [InlineData('中', true)]
        [InlineData('文', true)]
        [InlineData('A', false)]
        [InlineData('1', false)]
        [InlineData('#', false)]
        [InlineData(' ', false)]
        public void IsChineseCharacter_ShouldCorrectlyIdentifyChineseCharacters(char input, bool expected)
        {
            // Act
            var result = ChineseTextHelper.IsChineseCharacter(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("中文", true)]
        [InlineData("Hello中文", true)]
        [InlineData("中文123", true)]
        [InlineData("Hello", false)]
        [InlineData("123", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void ContainsChinese_ShouldCorrectlyDetectChineseCharacters(string input, bool expected)
        {
            // Act
            var result = ChineseTextHelper.ContainsChinese(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}