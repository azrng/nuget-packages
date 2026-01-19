using System.Text;

namespace Common.Core.Test.Extension
{
    public class StringBuilderExtensionsTest
    {
        /// <summary>
        /// 测试 AppendIF 方法 - 根据条件拼接字符串
        /// </summary>
        [Theory]
        [InlineData("123", false, "456", "123")]
        [InlineData("123", true, "456", "123456")]
        [InlineData("", true, "abc", "abc")]
        public void AppendIF_ReturnOk(string str, bool condition, string appendStr, string expected)
        {
            var builder = new StringBuilder(str);
            builder.AppendIF(condition, appendStr);
            Assert.Equal(expected, builder.ToString());
        }

        /// <summary>
        /// 测试 AppendLineIF 方法 - 根据条件拼接字符串（带换行）
        /// </summary>
        [Theory]
        [InlineData("123", false, "456", "123")]
        [InlineData("123", true, "456", "123456\r\n")]
        [InlineData("", true, "abc", "abc\r\n")]
        public void AppendLineIF_ReturnOk(string str, bool condition, string appendStr, string expected)
        {
            var builder = new StringBuilder(str);
            builder.AppendLineIF(condition, appendStr);
            Assert.Equal(expected, builder.ToString());
        }

        /// <summary>
        /// 测试 AppendFormatIF 方法 - 根据条件追加格式化字符串
        /// </summary>
        [Theory]
        [InlineData("Value: ", false, "{0}", 123, "Value: ")]
        [InlineData("Value: ", true, "{0}", 123, "Value: 123")]
        [InlineData("", true, "{0} - ", "A", "A - ")]
        public void AppendFormatIF_ReturnOk(string str, bool condition, string format, object arg, string expected)
        {
            var builder = new StringBuilder(str);
            builder.AppendFormatIF(condition, format, arg);
            Assert.Equal(expected, builder.ToString());
        }

        /// <summary>
        /// 测试 AppendFormatIF 方法 - 多参数格式化
        /// </summary>
        [Theory]
        [InlineData("Result: ", true, "{0}+{1}={2}", 2, 3,
            5, "Result: 2+3=5")]
        public void AppendFormatIF_MultipleArgs_ReturnOk(string str, bool condition, string format, object arg1, object arg2,
                                                         object arg3, string expected)
        {
            var builder = new StringBuilder(str);
            builder.AppendFormatIF(condition, format, arg1, arg2, arg3);
            Assert.Equal(expected, builder.ToString());
        }

        /// <summary>
        /// 测试 AppendLineIfNotEmpty 方法 - 追加换行（如果字符串不为空）
        /// </summary>
        [Theory]
        [InlineData("123", "456", "123456\r\n")]
        [InlineData("123", "", "123")]
        [InlineData("123", null, "123")]
        public void AppendLineIfNotEmpty_ReturnOk(string str, string appendStr, string expected)
        {
            var builder = new StringBuilder(str);
            builder.AppendLineIfNotEmpty(appendStr);
            Assert.Equal(expected, builder.ToString());
        }

        /// <summary>
        /// 测试 AppendLineIfNotNullOrWhiteSpace 方法 - 追加换行（如果字符串不为空白）
        /// </summary>
        [Theory]
        [InlineData("123", "456", "123456\r\n")]
        [InlineData("123", "", "123")]
        [InlineData("123", null, "123")]
        [InlineData("123", "   ", "123")]
        public void AppendLineIfNotNullOrWhiteSpace_ReturnOk(string str, string appendStr, string expected)
        {
            var builder = new StringBuilder(str);
            builder.AppendLineIfNotNullOrWhiteSpace(appendStr);
            Assert.Equal(expected, builder.ToString());
        }

        /// <summary>
        /// 测试 AppendIfNotEmpty 方法 - 追加字符串（如果字符串不为空）
        /// </summary>
        [Theory]
        [InlineData("123", "456", "123456")]
        [InlineData("123", "", "123")]
        [InlineData("123", null, "123")]
        public void AppendIfNotEmpty_ReturnOk(string str, string appendStr, string expected)
        {
            var builder = new StringBuilder(str);
            builder.AppendIfNotEmpty(appendStr);
            Assert.Equal(expected, builder.ToString());
        }

        /// <summary>
        /// 测试 AppendIfNotNullOrWhiteSpace 方法 - 追加字符串（如果字符串不为空白）
        /// </summary>
        [Theory]
        [InlineData("123", "456", "123456")]
        [InlineData("123", "", "123")]
        [InlineData("123", null, "123")]
        [InlineData("123", "   ", "123")]
        public void AppendIfNotNullOrWhiteSpace_ReturnOk(string str, string appendStr, string expected)
        {
            var builder = new StringBuilder(str);
            builder.AppendIfNotNullOrWhiteSpace(appendStr);
            Assert.Equal(expected, builder.ToString());
        }

        /// <summary>
        /// 测试 RemoveEnd 方法 - 移除末尾指定长度的字符
        /// </summary>
        [Theory]
        [InlineData("12345", 2, "123")]
        [InlineData("12345", 5, "")]
        [InlineData("12345", 0, "12345")]
        [InlineData("abc", 10, "abc")] // 长度超过字符串长度时不处理
        [InlineData("", 1, "")] // 空字符串不处理
        public void RemoveEnd_ReturnOk(string str, int length, string expected)
        {
            var builder = new StringBuilder(str);
            builder.RemoveEnd(length);
            Assert.Equal(expected, builder.ToString());
        }

        /// <summary>
        /// 测试 RemoveEndComma 方法 - 移除末尾的逗号（如果存在）
        /// </summary>
        [Theory]
        [InlineData("123,", "123")]
        [InlineData("123", "123")]
        [InlineData("1,2,3,", "1,2,3")]
        [InlineData(",", "")]
        [InlineData("", "")]
        public void RemoveEndComma_ReturnOk(string str, string expected)
        {
            var builder = new StringBuilder(str);
            builder.RemoveEndComma();
            Assert.Equal(expected, builder.ToString());
        }

        /// <summary>
        /// 测试 RemoveEndSemicolon 方法 - 移除末尾的分号（如果存在）
        /// </summary>
        [Theory]
        [InlineData("123;", "123")]
        [InlineData("123", "123")]
        [InlineData("1;2;3;", "1;2;3")]
        [InlineData(";", "")]
        [InlineData("", "")]
        public void RemoveEndSemicolon_ReturnOk(string str, string expected)
        {
            var builder = new StringBuilder(str);
            builder.RemoveEndSemicolon();
            Assert.Equal(expected, builder.ToString());
        }

        /// <summary>
        /// 测试 RemoveEndWithChar 方法 - 移除末尾的特定字符
        /// </summary>
        [Theory]
        [InlineData("123a", 'a', "123")]
        [InlineData("123", 'a', "123")]
        [InlineData("a1a2a", 'a', "a1a2")]
        [InlineData("a", 'a', "")]
        [InlineData("", 'a', "")]
        public void RemoveEndWithChar_ReturnOk(string str, char ch, string expected)
        {
            var builder = new StringBuilder(str);
            builder.RemoveEndWithChar(ch);
            Assert.Equal(expected, builder.ToString());
        }
    }
}