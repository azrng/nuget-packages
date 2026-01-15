// using Azrng.Core.RetryTask;
//
// namespace Common.Core.Test.Extension.Retry
// {
//     public class HandleAsDefaultWhenExceptionTest
//     {
//         [Fact]
//         public async Task HandleAsDefaultWhenException_WithException_ReturnsDefaultValue()
//         {
//             // Arrange
//             var task = CreateFailedTask<int>(new InvalidOperationException("Test exception"));
//
//             // Act
//             var handleTask = task.HandleAsDefaultWhenException();
//
//             // Assert
//             Assert.NotNull(handleTask);
//             var result = await handleTask;
//             Assert.Equal(default(int), result); // Should return default value
//         }
//
//         [Fact]
//         public async Task HandleAsDefaultWhenException_WithoutException_ReturnsActualValue()
//         {
//             // Arrange
//             var task = CreateSuccessfulTask<string>("success");
//
//             // Act
//             var handleTask = task.HandleAsDefaultWhenException();
//
//             // Assert
//             Assert.NotNull(handleTask);
//             var result = await handleTask;
//             Assert.Equal("success", result);
//         }
//
//         [Fact]
//         public void HandleAsDefaultWhenException_WithNullTask_ThrowsArgumentNullException()
//         {
//             // Arrange
//             ITask<int> task = null;
//
//             // Act & Assert
//             var ex = Assert.Throws<ArgumentNullException>(() => task.HandleAsDefaultWhenException());
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
//         private static ITask<T> CreateFailedTask<T>(Exception exception)
//         {
//             return new MockTask<T>(exception);
//         }
//
//         #endregion
//     }
// }