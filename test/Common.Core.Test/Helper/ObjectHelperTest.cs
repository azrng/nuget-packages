namespace Common.Core.Test.Helper
{
    /// <summary>
    /// todo 单元测试还有问题
    /// </summary>
    public class ObjectHelperTest
    {
        [Fact]
        public void ConvertToTargetType_NullValue_ThrowsArgumentNullException()
        {
            // Arrange
            object value = null;
            Type targetType = typeof(int);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ObjectHelper.ConvertToTargetType(value, targetType));
        }

        [Fact]
        public void ConvertToTargetType_NullTargetType_ThrowsArgumentNullException()
        {
            // Arrange
            object value = "123";
            Type targetType = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ObjectHelper.ConvertToTargetType(value, targetType));
        }

        [Theory]
        [InlineData("123", 123)]
        [InlineData(123, 123)]
        [InlineData("0", 0)]
        [InlineData("-456", -456)]
        public void ConvertToTargetType_ToInt_ReturnsExpectedResult(object value, int expected)
        {
            // Act
            var result = ObjectHelper.ConvertToTargetType(value, typeof(int));

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("123.45", 123.45d)]
        [InlineData(123.45f, 123.45d)]
        [InlineData("0.0", 0.0d)]
        [InlineData("-456.78", -456.78d)]
        public void ConvertToTargetType_ToDouble_ReturnsExpectedResult(object value, double expected)
        {
            // Act
            var result = ObjectHelper.ConvertToTargetType(value, typeof(double));

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("123.45", 123.45f)]
        [InlineData(123.45d, 123.45f)]
        [InlineData("0.0", 0.0f)]
        [InlineData("-456.78", -456.78f)]
        public void ConvertToTargetType_ToFloat_ReturnsExpectedResult(object value, float expected)
        {
            // Act
            var result = ObjectHelper.ConvertToTargetType(value, typeof(float));

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(123, "123")]
        [InlineData(123.45, "123.45")]
        [InlineData(true, "True")]
        public void ConvertToTargetType_ToString_ReturnsExpectedResult(object value, string expected)
        {
            // Act
            var result = ObjectHelper.ConvertToTargetType(value, typeof(string));

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("2023-01-01", "2023-01-01")]
        public void ConvertToTargetType_ToDateTime_ReturnsExpectedResult(string value, string expected)
        {
            // Act
            var result = ObjectHelper.ConvertToTargetType(value, typeof(DateTime));

            // Assert
            Assert.Equal(DateTime.Parse(expected), result);
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData(1, true)]
        [InlineData(0, false)]
        public void ConvertToTargetType_ToBool_ReturnsExpectedResult(object value, bool expected)
        {
            // Act
            var result = ObjectHelper.ConvertToTargetType(value, typeof(bool));

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConvertToTargetType_ToEnumFromString_ReturnsExpectedResult()
        {
            // Arrange
            var value = "Monday";
            var expected = DayOfWeek.Monday;

            // Act
            var result = ObjectHelper.ConvertToTargetType(value, typeof(DayOfWeek));

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConvertToTargetType_ToEnumFromInt_ReturnsExpectedResult()
        {
            // Arrange
            var value = 1;
            var expected = DayOfWeek.Monday;

            // Act
            var result = ObjectHelper.ConvertToTargetType(value, typeof(DayOfWeek));

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConvertToTargetType_ToNullableType_ReturnsExpectedResult()
        {
            // Act
            var result = ObjectHelper.ConvertToTargetType("123", typeof(int?));

            // Assert
            Assert.Equal(123, result);
        }

        [Fact]
        public void ConvertToTargetType_SameType_ReturnsSameObject()
        {
            // Arrange
            var value = 123;

            // Act
            var result = ObjectHelper.ConvertToTargetType(value, typeof(int));

            // Assert
            Assert.Equal(value, result);
        }

        [Fact]
        public void ConvertToTargetType_UnsupportedConversion_ReturnsOriginalValue()
        {
            // Arrange
            var value = new object();

            // Act
            var result = ObjectHelper.ConvertToTargetType(value, typeof(object));

            // Assert
            Assert.Same(value, result);
        }
    }
}