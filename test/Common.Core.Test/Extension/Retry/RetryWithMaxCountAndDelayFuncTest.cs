using Azrng.Core.Exceptions;
using Azrng.Core.RetryTask;
using Xunit.Abstractions;

namespace Common.Core.Test.Extension.Retry
{
    public class RetryWithMaxCountAndDelayFuncTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public RetryWithMaxCountAndDelayFuncTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// 重试 指定次数并指定延迟执行
        /// </summary>
        [Fact]
        public async Task Retry_WithMaxCountAndDelayFunction_ShouldWork()
        {
            // Arrange
            var task = CreateSuccessfulTask<string>("test");
            const int maxCount = 3;

            // Act
            var retryTask = task.Retry(maxCount, i => TimeSpan.FromMilliseconds(i * 10));

            // Assert
            Assert.NotNull(retryTask);
            var result = await retryTask;
            Assert.Equal("test", result);
        }

        /// <summary>
        /// 异常重试
        /// </summary>
        [Fact]
        public async Task Retry_Throw_ShouldWork()
        {
            // Arrange
            var task = CreateFailedTask<string>("test");
            const int maxCount = 3;

            // Act
            var retryTask = task.Retry(maxCount, i => TimeSpan.FromMilliseconds(i * 10));

            // Assert
            Assert.NotNull(retryTask);
            var ex = await Assert.ThrowsAsync<ParameterException>(async () => await retryTask);
            Assert.NotNull(ex);
        }

        /// <summary>
        /// 重试 指定次数并指定延迟执行
        /// </summary>
        [Fact]
        public void Retry_WithMaxCountAndDelayFunction_NullTask_ThrowsArgumentNullException()
        {
            // Arrange
            ITask<int> task = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => task.Retry(3, i => TimeSpan.FromMilliseconds(i * 10)));
            Assert.Equal("task", ex.ParamName);
        }

        #region Helper Methods

        private async Task<T> CreateSuccessfulTask<T>(T result)
        {
            _testOutputHelper.WriteLine($"时间：{DateTime.Now.ToDetailedTimeString()}");
            await Task.Delay(100);
            return result;
        }

        private async Task<T> CreateFailedTask<T>(T result)
        {
            _testOutputHelper.WriteLine($"时间：{DateTime.Now.ToDetailedTimeString()}");

            await Task.Delay(100);
            throw new ParameterException("参数无效");
            return result;
        }

        #endregion
    }
}