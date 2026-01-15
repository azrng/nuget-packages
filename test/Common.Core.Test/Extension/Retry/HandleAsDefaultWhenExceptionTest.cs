using Azrng.Core.Exceptions;
using Xunit.Abstractions;

namespace Common.Core.Test.Extension.Retry
{
    public class HandleAsDefaultWhenExceptionTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public HandleAsDefaultWhenExceptionTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task HandleAsDefaultWhenException_WithException_ReturnsDefaultValue()
        {
            // Arrange
            var task = CreateSuccessfulTaskAsync("error");

            // Act
            var handleTask = task.HandleAsDefaultWhenException();

            // Assert
            Assert.NotNull(handleTask);
            var result = await handleTask;
            Assert.Null(result); // Should return default value
        }

        [Fact]
        public async Task HandleAsDefaultWhenException_WithoutException_ReturnsActualValue()
        {
            // Arrange
            var task = CreateSuccessfulTaskAsync("success");

            // Act
            var handleTask = task.HandleAsDefaultWhenException();

            // Assert
            Assert.NotNull(handleTask);
            var result = await handleTask;
            Assert.Equal("success", result.Name);
        }

        // [Fact]
        // public void HandleAsDefaultWhenException_WithNullTask_ThrowsArgumentNullException()
        // {
        //     // Arrange
        //     ITask<int> task = null;
        //
        //     // Act & Assert
        //     var ex = Assert.Throws<ArgumentNullException>(() => task.HandleAsDefaultWhenException());
        //     Assert.Equal("task", ex.ParamName);
        // }

        #region Helper Methods

        private async Task<ReplayUserInfo> CreateSuccessfulTaskAsync(string str)
        {
            var num = RandomGenerator.GenerateNumber(1, 100);
            _testOutputHelper.WriteLine($"输出当前的随机数  {num} {str}");
            if (str == "error")
            {
                throw new ParameterException("参数异常");
            }

            await Task.Delay(100);

            return new ReplayUserInfo { UserId = "1", Name = str };
        }

        #endregion
    }
}