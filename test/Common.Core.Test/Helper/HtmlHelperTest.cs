using Xunit.Abstractions;

namespace Common.Core.Test.Helper
{
    public class HtmlHelperTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public HtmlHelperTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void SampleHtmlToStr_ReturnOk()
        {
            var htmlStr =
                "<head> <style> .bold-text { font-weight: bold; } </style> </head> <body> <p class=\"bold-text\">这段文本通过CSS类设置为粗体</p> </body>";
            var str = HtmlHelper.HtmlToText(htmlStr);
            _testOutputHelper.WriteLine(str);
        }
    }
}