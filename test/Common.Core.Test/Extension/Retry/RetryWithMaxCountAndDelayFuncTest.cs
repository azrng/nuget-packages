using Azrng.Core.RetryTask;

namespace Common.Core.Test.Extension.Retry
{
    public class RetryWithMaxCountAndDelayFuncTest
    {
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

        private static ITask<T> CreateSuccessfulTask<T>(T result)
        {
            return new MockTask<T>(result);
        }

        #endregion
    }
}