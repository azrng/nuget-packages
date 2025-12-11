using CommonCollect.Desen;
using CommonCollect.Desen.Handles;
using Xunit.Abstractions;

namespace CommonCollect.Test.Desen
{
    public class DesenTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public DesenTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void 保留前面2个和后面2个()
        {
            var sourceStr = "123456789";

            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.RetainFrontNBackM);
            var result = service.GetDesenResult(sourceStr, 2, 2);
            _testOutputHelper.WriteLine(result);

            Assert.Equal("12*****89", result);
        }

        [Fact]
        public void 保留前面2个到4个()
        {
            var sourceStr = "123456789";

            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.RetainFrontXToY);
            var result = service.GetDesenResult(sourceStr, 2, 4);
            _testOutputHelper.WriteLine(result);

            Assert.Equal("*234*****", result);
        }

        [Fact]
        public void 遮盖前面2后面3个()
        {
            var sourceStr = "123456789";

            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.CoverFrontNBackM);
            var result = service.GetDesenResult(sourceStr, 2, 3);

            Assert.Equal("**3456***", result);
        }

        [Fact]
        public void 遮盖从2开始到第4个()
        {
            var sourceStr = "123456789";

            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.CoverFrontXToY);
            var result = service.GetDesenResult(sourceStr, 2, 4);

            Assert.Equal("1***56789", result);
        }

        /// <summary>
        /// 遮盖从-2到-4
        /// </summary>
        [Fact]
        public void 遮盖从_2开始到_4()
        {
            var sourceStr = "123456789";

            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.CoverFrontXToY);
            var result = service.GetDesenResult(sourceStr, -2, -4);

            Assert.Equal("12345***9", result);
        }

        [Fact]
        public void 遮盖自2到8()
        {
            var origin = "裘文涛";
            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.CoverFrontXToY);
            var result = service.GetDesenResult(origin, 2, 8);
            Assert.Equal("裘**", result);
        }

        /// <summary>
        /// 遮盖从-2到-8
        /// </summary>
        [Fact]
        public void 遮盖自_2到_8()
        {
            var origin = "裘文涛";
            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.CoverFrontXToY);
            var result = service.GetDesenResult(origin, -2, -8);
            Assert.Equal("**涛", result);
        }

        [Fact]
        public void 保留自2到8()
        {
            var origin = "裘文涛";
            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.RetainFrontXToY);
            var result = service.GetDesenResult(origin, 2, 8);
            Assert.Equal("*文涛", result);
        }

        /// <summary>
        /// 保留自-2到-8
        /// </summary>
        [Fact]
        public void 保留自_2到_8()
        {
            var origin = "裘文涛";
            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.RetainFrontXToY);
            var result = service.GetDesenResult(origin, -2, -8);
            Assert.Equal("裘文*", result);
        }

        /// <summary>
        /// 保留自-2到-8
        /// </summary>
        [Fact]
        public void 保留自_2到_4()
        {
            var origin = "123456789";
            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.RetainFrontXToY);
            var result = service.GetDesenResult(origin, -2, -4);
            Assert.Equal("*****678*", result);
        }

        [Fact]
        public void 关键字前面4个到2个()
        {
            var sourceStr = "123456789";

            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.KeywordFrontCover);
            var result = service.GetDesenResult(sourceStr, 4, 2, "78");
            _testOutputHelper.WriteLine(result);

            Assert.Equal("12***6789", result);
        }

        /// <summary>
        /// 关键字前面-10到-12个
        /// </summary>
        [Fact]
        public void 关键字前面_5个到_5()
        {
            var sourceStr = "12345:6789";

            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.KeywordFrontCover);
            var result = service.GetDesenResult(sourceStr, -5, -5, ":");
            _testOutputHelper.WriteLine(result);

            Assert.Equal("1234*:6789", result);
        }

        /// <summary>
        /// 关键字前面-10到-12个
        /// </summary>
        [Fact]
        public void 关键字前面_5个到_10()
        {
            var sourceStr = "12345:6789";

            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.KeywordFrontCover);
            var result = service.GetDesenResult(sourceStr, -5, -10, ":");
            _testOutputHelper.WriteLine(result);

            Assert.Equal("1234*:6789", result);
        }

        /// <summary>
        /// 关键字前面-1到-2个
        /// </summary>
        [Fact]
        public void 关键字前面_1个到_2()
        {
            var sourceStr = "12345:身份证";

            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.KeywordFrontCover);
            var result = service.GetDesenResult(sourceStr, -1, -2, ":身份证");
            _testOutputHelper.WriteLine(result);

            Assert.Equal("**345:身份证", result);
        }

        /// <summary>
        /// 关键字前面-4到-2个
        /// </summary>
        [Fact]
        public void 关键字前面_2个到_4()
        {
            var sourceStr = "12345:6789";

            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.KeywordFrontCover);
            var result = service.GetDesenResult(sourceStr, -2, -4, ":");
            _testOutputHelper.WriteLine(result);

            Assert.Equal("1***5:6789", result);
        }

        /// <summary>
        /// 关键字前面-10到-12个
        /// </summary>
        [Fact]
        public void 关键字前面_10个到_12()
        {
            var sourceStr = "12345:6789";

            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.KeywordFrontCover);
            var result = service.GetDesenResult(sourceStr, -10, -12, ":");
            _testOutputHelper.WriteLine(result);

            Assert.Equal("12345:6789", result);
        }

        [Fact]
        public void 关键字后面2个到4个()
        {
            var sourceStr = "123456789";

            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.KeywordBackCover);
            var result = service.GetDesenResult(sourceStr, 2, 4, "23");
            _testOutputHelper.WriteLine(result);

            Assert.Equal("1234***89", result);
        }

        /// <summary>
        /// 关键字后面-1到-2
        /// </summary>
        [Fact]
        public void 关键字后面_1到_2()
        {
            var sourceStr = "身份证：6789";

            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.KeywordBackCover);
            var result = service.GetDesenResult(sourceStr, -1, -2, "身份证：");
            _testOutputHelper.WriteLine(result);

            Assert.Equal("身份证：67**", result);
        }

        /// <summary>
        /// 关键字后面-2到-4
        /// </summary>
        [Fact]
        public void 关键字后面_2到_4()
        {
            var sourceStr = "12345:6789";

            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.KeywordBackCover);
            var result = service.GetDesenResult(sourceStr, -2, -4, ":");
            _testOutputHelper.WriteLine(result);

            Assert.Equal("12345:***9", result);
        }

        /// <summary>
        /// 关键字后面-2到-4
        /// </summary>
        [Fact]
        public void 关键字后面_1到_6()
        {
            var sourceStr = "12345:6789";

            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.KeywordBackCover);
            var result = service.GetDesenResult(sourceStr, -1, -6, ":");
            _testOutputHelper.WriteLine(result);

            Assert.Equal("12345:****", result);
        }

        /// <summary>
        /// 关键字后面-2到-10
        /// </summary>
        [Fact]
        public void 关键字后面_2到_10()
        {
            var sourceStr = "12345:6789";

            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.KeywordBackCover);
            var result = service.GetDesenResult(sourceStr, -2, -10, ":");
            _testOutputHelper.WriteLine(result);

            Assert.Equal("12345:***9", result);
        }

        [Fact]
        public void 关键字后2到3脱敏()
        {
            var origin = "身份证：裘文涛";
            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.KeywordBackCover);
            var result = service.GetDesenResult(origin, 2, 3, "身份证：");
            Assert.Equal("身份证：裘**", result);
        }

        [Fact]
        public void 关键字后2到10脱敏()
        {
            var origin = "身份证：裘文涛";
            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.KeywordBackCover);
            var result = service.GetDesenResult(origin, 2, 10, "身份证：");
            Assert.Equal("身份证：裘**", result);
        }

        [Fact]
        public void 关键字后10到20脱敏()
        {
            var origin = "身份证：裘文涛";
            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.KeywordBackCover);
            var result = service.GetDesenResult(origin, 10, 20, "身份证：");
            Assert.Equal(result, "身份证：裘文涛");
        }

        [Fact]
        public void 关键字后0到1脱敏()
        {
            var origin = "123456身份证：裘文涛";
            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.KeywordBackCover);
            var result = service.GetDesenResult(origin, 0, 1, "身份证：");
            Assert.Equal(result, "123456身份证：*文涛");
        }

        [Fact]
        public void 关键字前3到2脱敏()
        {
            var origin = "123456身份证：裘文涛";
            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.KeywordFrontCover);
            var result = service.GetDesenResult(origin, 3, 2, "身份证：");
            Assert.Equal(result, "123**6身份证：裘文涛");
        }

        [Fact]
        public void 关键字前10到2脱敏()
        {
            var origin = "123456身份证：裘文涛";
            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.KeywordFrontCover);
            var result = service.GetDesenResult(origin, 10, 2, "身份证：");
            Assert.Equal(result, "*****6身份证：裘文涛");
        }

        [Fact]
        public void 关键字前20到10脱敏()
        {
            var origin = "123456身份证：裘文涛";
            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.KeywordFrontCover);
            var result = service.GetDesenResult(origin, 20, 10, "身份证：");
            Assert.Equal(result, "123456身份证：裘文涛");
        }

        [Fact]
        public void 关键字前1到0脱敏()
        {
            var origin = "123456身份证：裘文涛";
            var service = DesenArithmeticFactory.GetDesenService(DesenArithmeticType.KeywordFrontCover);
            var result = service.GetDesenResult(origin, 1, 1, "身份证：");
            Assert.Equal(result, "12345*身份证：裘文涛");
        }
    }
}