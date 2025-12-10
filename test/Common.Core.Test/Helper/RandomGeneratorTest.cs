using System.Globalization;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace Common.Core.Test.Helper
{
    public class RandomGeneratorTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public RandomGeneratorTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// 生成固定长度字符串
        /// </summary>
        [Fact]
        public void GenFixedLengthString_ReturnOk()
        {
            var resultStr = RandomGenerator.GenerateString();
            _testOutputHelper.WriteLine(resultStr);
            Assert.True(resultStr.Length == 6);
        }

        /// <summary>
        /// 生成随机小数
        /// </summary>
        [Fact]
        public void GenDouble_ReturnOk()
        {
            var result = RandomGenerator.GenerateDoubleNumber();
            _testOutputHelper.WriteLine(result.ToString(CultureInfo.InvariantCulture));
            Assert.True(result is > 0 and < 1);
        }

        /// <summary>
        /// 将数组随机排序
        /// </summary>
        [Fact]
        public void ArrayRandomSort_ReturnOk()
        {
            var arr = new int[]
                      {
                          1,
                          2,
                          3,
                          4,
                          5,
                          6
                      };

            RandomGenerator.GenerateArray(arr);

            _testOutputHelper.WriteLine(string.Join(",", arr));
        }

        #region 验证码

        /// <summary>
        /// 验证是否包含数字
        /// </summary>
        [Fact]
        public void Verify_SmsCode_HasNumber()
        {
            var code = RandomGenerator.GenerateVerifyCode(6, true, false);
            _testOutputHelper.WriteLine($"验证是否包含数字 {code}");

            Assert.True(code.Length == 6);
            Assert.Matches("[0-9]", code);
        }

        /// <summary>
        /// 验证是否包含大写字母
        /// </summary>
        [Fact]
        public void Verify_SmsCode_HasUpper()
        {
            var code = RandomGenerator.GenerateVerifyCode(6, true, true);
            _testOutputHelper.WriteLine($"验证是否包含大写字母 {code}");
            Assert.True(code.Length == 6);
            Assert.Matches("[A-Z]", code);
        }

        /// <summary>
        /// 验证是否包含小写字母
        /// </summary>
        [Fact]
        public void Verify_SmsCode_HasLowercase()
        {
            var code = RandomGenerator.GenerateVerifyCode(6, true, false, true);
            _testOutputHelper.WriteLine($"验证是否包含小写字母 {code}");
            Assert.True(code.Length == 6);
            Assert.Matches("[a-z]", code);
        }

        /// <summary>
        /// 验证是否包含特殊字符
        /// </summary>
        [Fact]
        public void Verify_SmsCode_HasNonAlphanumeric()
        {
            var code = RandomGenerator.GenerateVerifyCode(6, true, false, false, true);
            _testOutputHelper.WriteLine($"验证是否包含特殊字符 {code}");

            Assert.Matches("[^a-zA-Z]", code);
        }

        /// <summary>
        /// 验证是包含数字 大写 小写  特殊字符
        /// </summary>
        [Fact]
        public void Verify_SmsCode_HasAll()
        {
            var code = RandomGenerator.GenerateVerifyCode(6, true);
            Assert.True(code.Length == 6);
            var existNumber = Regex.IsMatch(code, "[0-9]");

            var code2 = RandomGenerator.GenerateVerifyCode(6, true, true);
            Assert.True(code.Length == 6);
            var existUpper = Regex.IsMatch(code2, "[A-Z]");

            var code3 = RandomGenerator.GenerateVerifyCode(6, true, false, true);
            Assert.True(code.Length == 6);
            var existLowercase = Regex.IsMatch(code3, "[a-z]");

            var code4 = RandomGenerator.GenerateVerifyCode(6, true, false, false, true);
            Assert.True(code.Length == 6);
            var existNonAlphanumeric = Regex.IsMatch(code4, "[^a-zA-Z]");

            Assert.True(existNumber && existUpper && existLowercase && existNonAlphanumeric);
        }

        #endregion

        /// <summary>
        /// 生成随机时间
        /// </summary>
        [Fact]
        public void GenRandomDateTime_ReturnOk()
        {
            var startTime = new DateTime(2023, 10, 20);
            var endTime = new DateTime(2023, 10, 21);

            var result = RandomGenerator.GenerateDateTime(startTime, endTime);
            _testOutputHelper.WriteLine(result.ToStandardString());
            Assert.True(startTime < result);
            Assert.True(endTime > result);
        }
    }
}