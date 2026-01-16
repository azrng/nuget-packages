using Azrng.Core.RetryTask;
using System.Runtime.CompilerServices;

namespace Common.Core.Test.Extension.Retry
{
    public class HandleTest
    {
        [Fact]
        public async Task Handle_WithValidTask_ShouldWork()
        {
            // Arrange
            var task = CreateSuccessfulTask(true);

            // Act
            var handleTask = task.Handle();

            // Assert
            Assert.NotNull(handleTask);
            var result = await handleTask;
            Assert.True(result);
        }

        [Fact]
        public async Task Handle_WithNullTask_ThrowsArgumentNullException()
        {
            // Arrange
            Task<int> task = null;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () => await task.Handle<int>());
            Assert.Equal("task", ex.ParamName);
        }

        #region Helper Methods

        private async Task<T> CreateSuccessfulTask<T>(T result)
        {
            await Task.Delay(100);
            return result;
        }

        #endregion
    }

    #region Mock Implementation

    internal class MockTask<T> : ITask<T>
    {
        private readonly T _result;

        public MockTask(T result)
        {
            _result = result;
        }

        public TaskAwaiter<T> GetAwaiter()
        {
            return Task.FromResult(_result).GetAwaiter();
        }

        public ConfiguredTaskAwaitable<T> ConfigureAwait(bool continueOnCapturedContext)
        {
            return Task.FromResult(_result).ConfigureAwait(continueOnCapturedContext);
        }
    }

    #endregion
}