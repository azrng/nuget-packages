using System.Text;

namespace Common.Core.Test.Extension
{
    public class StringBuilderExtensionsTest
    {
        [Theory]
        [InlineData("123", false, "123")]
        public void AppendIf_ReturnOk(string str, bool condition, string expected)
        {
            var builder = new StringBuilder(str);
            builder.AppendIF(condition, "123");
            Assert.Equal(expected, builder.ToString());
        }
    }
}