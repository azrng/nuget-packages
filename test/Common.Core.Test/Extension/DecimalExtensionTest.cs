namespace Common.Core.Test.Extension
{
    /// <summary>
    /// 数值测试类
    /// </summary>
    public class DecimalExtensionTest
    {
        /// <summary>
        /// 格式化小数位数
        /// </summary>
        /// <param name="input"></param>
        /// <param name="number"></param>
        /// <param name="expected"></param>
        [Theory]
        [InlineData(1.24, 2, "1.24")]
        [InlineData(1.246, 2, "1.25")]
        [InlineData(1.245, 2, "1.24")]
        [InlineData(1.2451, 2, "1.25")]
        [InlineData(1.2350, 2, "1.24")]
        [InlineData(1.2, 2, "1.2")]
        public void ToStandardString_ShouldFormatCorrectly(decimal input, int number, string expected)
        {
            Assert.Equal(expected, input.ToStandardString(number));
        }

        [Theory]
        [InlineData(1.20, 2, "1.2")]
        [InlineData(1.255, 2, "1.26")]
        [InlineData(1.2, 1, "1.2")]
        [InlineData(1.255, 3, "1.255")]
        public void ToNoZeroString_ShouldFormatCorrectly(decimal input, int number, string expected)
        {
            Assert.Equal(expected, input.ToNoZeroString(number));
        }

        [Fact]
        public void decima负数绝对值_ReturnOK()
        {
            var str = -1.255m;
            var result = str.ToAbs();
            Assert.Equal(1.255m, result);
        }

        /// <summary>
        /// 格式化小数位数
        /// </summary>
        /// <param name="input"></param>
        /// <param name="number"></param>
        /// <param name="expected"></param>
        [Theory]
        [InlineData(1.2345, 2, "1.23")]
        [InlineData(1.2345, 3, "1.235")]
        [InlineData(1.2345, 0, "1")]
        [InlineData(1.2345, -1, "1.23")]
        public void ToFixedString_ShouldFormatCorrectly(decimal input, int number, string expected)
        {
            Assert.Equal(expected, input.ToFixedString(number));
        }

        /// <summary>
        /// 格式化小数位数
        /// </summary>
        /// <param name="input"></param>
        /// <param name="number"></param>
        /// <param name="expected"></param>
        [Theory]
        [InlineData(null, 2, "0")]
        public void ToFixedString_Null_ShouldFormatCorrectly(decimal? input, int number, string expected)
        {
            Assert.Equal(expected, input.ToFixedString(number));
        }

        [Theory]
        [InlineData(0.1234d, 2, "12.34%")]
        [InlineData(0.1234d, 3, "12.340%")]
        [InlineData(0.1234d, 0, "12%")]
        [InlineData(0.1234d, -1, "12.34%")]
        public void ToPercentString_ShouldFormatCorrectly(decimal input, int number, string expected)
        {
            Assert.Equal(expected, input.ToPercentString(number));
        }

        [Theory]
        [InlineData(null, 2, "0%")]
        public void ToPercentString_Null_ShouldFormatCorrectly(decimal? input, int number, string expected)
        {
            Assert.Equal(expected, input.ToPercentString(number));
        }

        /// <summary>
        /// 直接截断处理
        /// </summary>
        /// <param name="input"></param>
        /// <param name="number"></param>
        /// <param name="expected"></param>
        [Theory]
        [InlineData(1.2345, 2, "1.23")]
        [InlineData(1.2345, 3, "1.234")]
        [InlineData(1.2345, 0, "1")]
        [InlineData(1.2345, -1, "1.23")]
        public void ToTruncateString_ShouldFormatCorrectly(decimal input, int number, string expected)
        {
            Assert.Equal(expected, input.ToTruncateString(number));
        }

        [Theory]
        [InlineData(null, 2, "0")]
        public void ToTruncateString_Null_ShouldFormatCorrectly(decimal? input, int number, string expected)
        {
            Assert.Equal(expected, input.ToTruncateString(number));
        }
    }
}