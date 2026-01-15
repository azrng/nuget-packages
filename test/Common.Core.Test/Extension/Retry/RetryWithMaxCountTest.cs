using Azrng.Core.RetryTask;

namespace Common.Core.Test.Extension.Retry
{
    public class RetryWithMaxCountTest
    {
        [Fact]
        public async Task Retry_WithMaxCount_ShouldWork()
        {
            // Arrange
            var task = CreateSuccessfulTask<int>(42);
            const int maxCount = 3;

            // Act
            var retryTask = task.Retry(maxCount);

            // Assert
            Assert.NotNull(retryTask);
            var result = await retryTask;
            Assert.Equal(42, result);
        }

        [Fact]
        public void Retry_WithInvalidMaxCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var task = CreateSuccessfulTask<int>(42);
            const int invalidMaxCount = 0;

            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => task.Retry(invalidMaxCount));
            Assert.Equal("maxCount", ex.ParamName);
        }

        [Fact]
        public void Retry_WithNullTask_ThrowsArgumentNullException()
        {
            // Arrange
            ITask<int> task = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => task.Retry(3));
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