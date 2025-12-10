namespace Common.Core.Test.Helper.FileHelperTest
{
    /// <summary>
    /// 格式化文件大小单元测试
    /// </summary>
    public class FormatFileSizeTest
    {
        [Fact]
        public void FormatFileSize_WhenBytesLessThanUnit_ReturnsBytesWithB()
        {
            // Arrange
            var bytes = 1023;
            var expected = "1023 B";

            // Act
            var result = FileHelper.FormatFileSize(bytes);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FormatFileSize_WhenBytesEqualToUnit_ReturnsBytesWithB()
        {
            // Arrange
            var bytes = 1024;
            var expected = "1.00 KB";

            // Act
            var result = FileHelper.FormatFileSize(bytes);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FormatFileSize_WhenBytesGreaterThanUnit_ReturnsFormattedSizeWithCorrectUnit()
        {
            // Arrange
            var bytes = 1024 * 1024;
            var expected = "1.00 MB";

            // Act
            var result = FileHelper.FormatFileSize(bytes);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FormatFileSize_WhenBytesGreaterThanMultipleUnits_ReturnsFormattedSizeWithCorrectUnit()
        {
            // Arrange
            var bytes = 1024 * 1024 * 1024;
            var expected = "1.00 GB";

            // Act
            var result = FileHelper.FormatFileSize(bytes);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}