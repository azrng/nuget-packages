using Xunit.Abstractions;

namespace Common.Core.Test.Helper
{
    public class ChinaDateHelperTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ChinaDateHelperTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// 国庆节
        /// </summary>
        [Fact]
        public void 国庆ChineseDate_ReturnOk()
        {
            // 准备
            var originDate = new DateTime(1949, 10, 1);

            // 行为
            var resultDate = ChinaDateHelper.GetChinaDate(originDate);
            _testOutputHelper.WriteLine(resultDate.CnFtvs);

            // 断言
            Assert.Contains("国庆节", resultDate.CnFtvs);
        }

        /// <summary>
        /// 根据时间获取节气（有些获取到是空）
        /// </summary>
        [Fact]
        public void CnSolarTermDate_ReturnOk()
        {
            // 准备
            var originDate = new DateTime(2023, 4, 20);

            // 行为
            var resultDate = ChinaDateHelper.GetChinaDate(originDate);
            _testOutputHelper.WriteLine(resultDate.CnSolarTerm);

            // 断言
            Assert.Contains("谷雨", resultDate.CnSolarTerm);
        }
    }
}