// using Azrng.Core.RetryTask;
//
// namespace Common.Core.Test.Extension.Retry
// {
//     public class RetryWithMaxCountAndDelayTest
//     {
//         [Fact]
//         public async Task Retry_WithMaxCountAndDelay_ShouldWork()
//         {
//             // Arrange
//             var task = CreateSuccessfulTask<int>(100);
//             const int maxCount = 2;
//             var delay = TimeSpan.FromMilliseconds(100);
//
//             // Act
//             var retryTask = task.Retry(maxCount, delay);
//
//             // Assert
//             Assert.NotNull(retryTask);
//             var result = await retryTask;
//             Assert.Equal(100, result);
//         }
//
//         [Fact]
//         public void Retry_WithMaxCountAndDelay_NullTask_ThrowsArgumentNullException()
//         {
//             // Arrange
//             ITask<int> task = null;
//             var delay = TimeSpan.FromMilliseconds(100);
//
//             // Act & Assert
//             var ex = Assert.Throws<ArgumentNullException>(() => task.Retry(2, delay));
//             Assert.Equal("task", ex.ParamName);
//         }
//
//         #region Helper Methods
//
//         private static ITask<T> CreateSuccessfulTask<T>(T result)
//         {
//             return new MockTask<T>(result);
//         }
//
//         #endregion
//     }
//
//
// }