// using Azrng.Core.RetryTask;
// using System.Runtime.CompilerServices;
//
// namespace Common.Core.Test.Extension.Retry
// {
//     public class HandleTest
//     {
//         [Fact]
//         public async Task Handle_WithValidTask_ShouldWork()
//         {
//             // Arrange
//             var task = CreateSuccessfulTask(true);
//
//             // Act
//             var handleTask = task.Handle();
//
//             // Assert
//             Assert.NotNull(handleTask);
//             var result = await handleTask;
//             Assert.True(result);
//         }
//
//         [Fact]
//         public void Handle_WithNullTask_ThrowsArgumentNullException()
//         {
//             // Arrange
//             ITask<int> task = null;
//
//             // Act & Assert
//             var ex = Assert.Throws<ArgumentNullException>(() => task.Handle<int>());
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
//     #region Mock Implementation
//
//     internal class MockTask<T> : ITask<T>
//     {
//         private readonly T _result;
//
//         public MockTask(T result)
//         {
//             _result = result;
//         }
//
//         public TaskAwaiter<T> GetAwaiter()
//         {
//             return Task.FromResult(_result).GetAwaiter();
//         }
//
//         public ConfiguredTaskAwaitable<T> ConfigureAwait(bool continueOnCapturedContext)
//         {
//             return Task.FromResult(_result).ConfigureAwait(continueOnCapturedContext);
//         }
//     }
//
//     #endregion
// }